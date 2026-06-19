"""Extract Rolemaster Spell Law spell lists (chapters 6-9) from the PDF.

Each spell list begins with a "Lvl Spell AoE Dur. Range Type" table header (a
reliable anchor); the all-caps list title sits just above it, and the section
code in the running header gives the category. The table rows (anchored by "N)")
give each spell's level/name/attributes; the numbered prose descriptions
("N. Name - text") that follow — flowing across two columns and multiple pages
until the next list — are matched to spells by their number.

Output: docs/game-data/spell-lists.json
Usage:  python scripts/extract_spell_lists.py [first_page] [last_page]
"""
import json
import re
import sys
from collections import Counter

import pdfplumber

PDF = "docs/rules/Rolemaster_Spell_Law_(RMU).pdf"
OUT = "docs/game-data/spell-lists.json"

FIRST_PAGE, LAST_PAGE = 60, 244          # spell-list content (0-based, inclusive)

HEADER_LABELS = ["Lvl", "Spell", "AoE", "Dur.", "Range", "Type"]
DASH_RE = re.compile(r"\s[–—-]\s")

# Section code → (realm, category, profession). Authoritative (chapter overviews).
CODE_CATEGORIES = {
    "6.1": ("Channeling", "Open Channeling", None),
    "6.2": ("Channeling", "Closed Channeling", None),
    "6.3": ("Channeling", "Cleric Base", "Cleric"),
    "6.4": ("Channeling", "Druid Base", "Druid"),
    "6.5": ("Channeling", "Paladin Base", "Paladin"),
    "6.6": ("Channeling", "Ranger Base", "Ranger"),
    "6.7": ("Channeling", "Evil Channeling", None),
    "7.1": ("Essence", "Open Essence", None),
    "7.2": ("Essence", "Closed Essence", None),
    "7.3": ("Essence", "Bard Base", "Bard"),
    "7.4": ("Essence", "Dabbler Base", "Dabbler"),
    "7.5": ("Essence", "Illusionist Base", "Illusionist"),
    "7.6": ("Essence", "Magician Base", "Magician"),
    "7.7": ("Essence", "Evil Essence", None),
    "8.1": ("Mentalism", "Open Mentalism", None),
    "8.2": ("Mentalism", "Closed Mentalism", None),
    "8.3": ("Mentalism", "Lay Healer Base", "Lay Healer"),
    "8.4": ("Mentalism", "Magent Base", "Magent"),
    "8.5": ("Mentalism", "Mentalist Base", "Mentalist"),
    "8.6": ("Mentalism", "Monk Base", "Monk"),
    "8.7": ("Mentalism", "Evil Mentalism", None),
    "9.1": ("Hybrid", "Healer Base", "Healer"),
    "9.2": ("Hybrid", "Mystic Base", "Mystic"),
    "9.3": ("Hybrid", "Sorcerer Base", "Sorcerer"),
}
CODE_RE = re.compile(r"([6-9])\1?\.\.?([1-9])\2?")     # matches "6.1" or doubled "66..11"


def dedouble(s):
    """Collapse fully-doubled bold text ('CCUURRSSEESS' -> 'CURSES')."""
    if len(s) >= 4 and len(s) % 2 == 0 and all(s[i] == s[i + 1] for i in range(0, len(s), 2)):
        return s[::2]
    return s


def titlecase(s):
    """Capitalise each word's first letter only (keeps 'Concussion's Way')."""
    return " ".join(w[:1].upper() + w[1:].lower() for w in s.split())


def column_edges(words):
    """Per-page (left_edge, right_edge, split) from the 'N.' marker x-positions.

    Even/odd pages have mirrored margins, so the two body columns sit at
    different x on different pages. Numbered markers left-align tightly at each
    column edge, so take the two most frequent x-bins (robust to stray numeric
    tokens that also end in '.')."""
    bins = Counter(round(w["x0"] / 3) * 3 for w in words if re.fullmatch(r"\d+\.", w["text"]))
    if not bins:
        return 70.0, 310.0, 300.0
    primary = bins.most_common(1)[0][0]
    others = [(b, n) for b, n in bins.items() if abs(b - primary) > 80]
    if not others:                                    # single column
        return float(primary), float(primary + 240), float(primary + 230)
    secondary = max(others, key=lambda bn: bn[1])[0]
    left_edge, right_edge = sorted((primary, secondary))
    return float(left_edge), float(right_edge), float(right_edge - 10)


def page_code(text):
    """Most common in-map section code on the page (doubled-text tolerant)."""
    codes = [f"{m.group(1)}.{m.group(2)}" for m in CODE_RE.finditer(text)]
    codes = [c for c in codes if c in CODE_CATEGORIES]
    return Counter(codes).most_common(1)[0][0] if codes else None


def find_table_header(words):
    """Locate the 'Lvl Spell AoE Dur. Range Type' row → (labels, lvl_word)."""
    for lvl in (w for w in words if w["text"] == "Lvl"):
        row = [w for w in words if abs(w["top"] - lvl["top"]) < 4]
        labels = {w["text"]: (w["x0"] + w["x1"]) / 2 for w in row if w["text"] in HEADER_LABELS}
        if len(labels) >= 5:
            return labels, lvl
    return None


def reading_order(words):
    """Order words within a single column into proper text lines.

    Cluster by top (a line's words share a baseline within a few px), then read
    lines top-to-bottom and words left-to-right. Avoids the bucket-boundary
    scrambling that a coarse round(top/N) sort causes, which split a number
    marker from the rest of its own line."""
    out, line, line_top = [], [], None
    for w in sorted(words, key=lambda w: (w["top"], w["x0"])):
        if line_top is None or w["top"] - line_top <= 4:
            line.append(w)
            line_top = line[0]["top"] if line_top is None else line_top
        else:
            out.extend(sorted(line, key=lambda w: w["x0"]))
            line, line_top = [w], w["top"]
    out.extend(sorted(line, key=lambda w: w["x0"]))
    return out


def title_above(words, header_top, lvl_centre):
    """All-caps (possibly doubled) list title just above the table header."""
    cand = [w for w in words
            if 6 < header_top - w["top"] < 26 and abs((w["x0"] + w["x1"]) / 2 - lvl_centre) < 230
            and re.fullmatch(r"[A-Z][A-Z'’.&\-]*|[IVX]+|\d+", w["text"])]
    if not cand:
        return ""
    top = min(cand, key=lambda w: w["top"])["top"]
    line = [w for w in cand if abs(w["top"] - top) < 4]
    return titlecase(" ".join(dedouble(w["text"]) for w in sorted(line, key=lambda w: w["x0"])).strip())


def extract_table(words, labels, header_top, right_edge):
    """Spell rows under the header, assigning each word to the nearest column.

    Words are bounded left of `right_edge` (the description column) so that, on
    pages where the table's Type column abuts the right column, description text
    aligned with a table row is not swallowed."""
    cols = [l for l in HEADER_LABELS if l in labels]
    centres = [labels[l] for l in cols]
    anchors = sorted((w for w in words if re.fullmatch(r"\d+\)", w["text"]) and w["top"] > header_top
                      and (w["x0"] + w["x1"]) / 2 < labels["Lvl"] + 20), key=lambda w: w["top"])
    # Spell tables are strictly level-ascending; drop out-of-sequence false anchors
    # (e.g. a "10)" that is really description prose lower on the page).
    seq, last = [], 0
    for a in anchors:
        n = int(a["text"].rstrip(")"))
        if n > last:
            seq.append(a)
            last = n
    anchors = seq
    rows, used = [], []
    for a in anchors:
        line = [w for w in words if abs(w["top"] - a["top"]) < 4 and w is not a
                and labels["Lvl"] - 10 < (w["x0"] + w["x1"]) / 2 < centres[-1] + 20
                and not re.fullmatch(r"\d+\.", w["text"])]
        bucket = {c: [] for c in cols}
        for w in line:
            cx = (w["x0"] + w["x1"]) / 2
            j = min(range(len(centres)), key=lambda k: abs(centres[k] - cx))
            bucket[cols[j]].append(w)
            used.append(w)
        used.append(a)

        def col(name):
            return " ".join(x["text"] for x in sorted(bucket.get(name, []), key=lambda w: w["x0"])).strip()

        rows.append({"level": int(a["text"].rstrip(")")), "name": col("Spell"),
                     "aoe": col("AoE"), "duration": col("Dur."), "range": col("Range"),
                     "type": col("Type")})
    return rows, set(id(w) for w in used)


def parse_descriptions(stream, edges):
    """Split a reading-order word stream at 'N.' markers at a column's left edge.

    `edges` maps page index → (left_edge, right_edge); a marker is real only when
    it left-aligns with one of its page's two columns and is followed by a dash."""
    starts = []
    for i, w in enumerate(stream):
        if not re.fullmatch(r"\d+\.", w["text"]):
            continue
        le, re_ = edges.get(w["_pi"], (70.0, 310.0))
        if min(abs(w["x0"] - le), abs(w["x0"] - re_)) < 12:
            ahead = " ".join(x["text"] for x in stream[i + 1:i + 12])
            if DASH_RE.search(" " + ahead):
                starts.append((i, int(w["text"].rstrip("."))))
    descs = {}
    for k, (i, n) in enumerate(starts):
        j = starts[k + 1][0] if k + 1 < len(starts) else len(stream)
        full = " ".join(w["text"] for w in stream[i + 1:j])
        full = re.sub(r"(\w)-\s+(\w)", r"\1\2", full)
        full = re.sub(r"\s+", " ", full).strip()
        m = DASH_RE.search(full)
        name, text = (full[:m.start()].strip(), full[m.end():].strip()) if m else ("", full)
        descs.setdefault(n, {"name": name, "text": text})    # first = this list's marker
    return descs


def main():
    first = int(sys.argv[1]) if len(sys.argv) > 1 else FIRST_PAGE
    last = int(sys.argv[2]) if len(sys.argv) > 2 else LAST_PAGE
    pdf = pdfplumber.open(PDF)

    # Pass 1: words per page (tagged with page index + column), category, header.
    pages = []
    edges = {}
    for i in range(first, last + 1):
        pg = pdf.pages[i]
        words = pg.extract_words(use_text_flow=True)
        le, re_, split = column_edges(words)
        edges[i] = (le, re_)
        for w in words:
            w["_pi"] = i
            w["_col"] = 0 if w["x0"] < split else 1
        pages.append({"i": i, "words": words, "code": page_code(pg.extract_text() or ""),
                      "header": find_table_header(words)})

    # Global reading-order stream: page order, within a page left column then right
    # column, each column ordered into proper text lines.
    stream = []
    for p in pages:
        body = [w for w in p["words"] if w["x0"] > 8 and 30 < w["top"] < 760]
        for col in (0, 1):
            stream.extend(reading_order([w for w in body if w["_col"] == col]))
    pos = {id(w): i for i, w in enumerate(stream)}

    # Header anchors (list starts), ordered by their position in the stream.
    heads = [{"p": p, "labels": p["header"][0], "lvl": p["header"][1]}
             for p in pages if p["header"] and id(p["header"][1]) in pos]
    heads.sort(key=lambda h: pos[id(h["lvl"])])

    results = []
    for hi, h in enumerate(heads):
        p = h["p"]
        code = p["code"]
        if code not in CODE_CATEGORIES:
            continue
        lvl = h["lvl"]
        title = title_above(p["words"], lvl["top"], (lvl["x0"] + lvl["x1"]) / 2)
        rows, used = extract_table(p["words"], h["labels"], lvl["top"], edges[p["i"]][1])
        if not title or not rows:
            continue

        start = pos[id(lvl)]
        end = pos[id(heads[hi + 1]["lvl"])] if hi + 1 < len(heads) else len(stream)
        slice_ = [w for w in stream[start:end] if id(w) not in used]
        descs = parse_descriptions(slice_, edges)
        for r in rows:
            dd = descs.get(r["level"])
            r["description"] = dd["text"] if dd else ""
            if not r["name"] and dd and dd["name"]:      # recover name from marker
                r["name"] = dd["name"]

        realm, catname, prof = CODE_CATEGORIES[code]
        results.append({"category": catname, "realm": realm, "profession": prof,
                        "code": code, "name": title, "spells": rows})

    with open(OUT, "w", encoding="utf-8") as f:
        json.dump(results, f, indent=2, ensure_ascii=False)

    print(f"Wrote {len(results)} spell lists to {OUT}")
    c = Counter((r["realm"], r["category"]) for r in results)
    for (realm, cat), n in sorted(c.items()):
        print(f"  {realm:<10} {cat:<22} {n} lists")
    tot = sum(len(r["spells"]) for r in results)
    miss = sum(1 for r in results for s in r["spells"] if not s["description"])
    print(f"  spells: {tot}, missing description: {miss}")


if __name__ == "__main__":
    main()
