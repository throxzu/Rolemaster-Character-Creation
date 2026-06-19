"""Extract Rolemaster Core Law Chapter 11 critical strike tables from the PDF.

Each table (Acid, Cold, ... Unbalance) is keyed by attack roll (rows) and
critical severity A-E (columns). Each row also carries a body location
(Head/Chest/Abdomen/Leg/Arm, printed rotated in the left margin). Cells are
multi-line descriptive prose, reconstructed per column by collecting the words
in the column's x-band between consecutive roll-row tops and joining them in
reading order.

Output: docs/game-data/critical-tables.json
Usage:  python scripts/extract_critical_tables.py
"""
import json
import re

import pdfplumber

PDF = "docs/rules/Rolemaster_Core_Law_(RMU).pdf"
OUT = "docs/game-data/critical-tables.json"

# Critical strike table pages (0-based), inclusive. (Fumble tables excluded.)
FIRST_PAGE, LAST_PAGE = 226, 240

SEVERITIES = ["A", "B", "C", "D", "E"]
ROLLNUM = re.compile(r"^\d+(-\d+)?$")
LOCATIONS = {"Head", "Chest", "Abdomen", "Leg", "Arm"}

# Webdings/Wingdings effect glyphs → readable tokens, per the Section 11 legend
# (page 225). A number before a symbol is duration; after it is severity.
GLYPHS = {
    chr(0xF053): " bleed",       # Bleeding, at X hits/round
    chr(0xF0DE): " fatigue",     # Fatigue penalty of X
    chr(0xF040): " breakage",    # Breakage roll at -X
    chr(0xF02B): " stun",        # Stunned (T rounds at the given penalty)
    chr(0xF0AE): " stagger",     # Staggered (loses 1 AP or X)
    chr(0xF05F): " knockback",   # Knocked back X'
    chr(0xF0CA): " prone",       # Knocked prone / pushed to ground
    chr(0xF022): " grapple",     # Grappled X%
    chr(0xF0A5): " +crit",       # Additional criticals of X severity
    chr(0x1F480): " KILLED",     # Target is dead / dying / defeated
}


def normalize(text):
    """Translate effect glyphs to words and tidy spaced numbers/symbols."""
    for g, repl in GLYPHS.items():
        text = text.replace(g + g, repl).replace(g, repl)
    text = re.sub(r"O\s*O", " hits", text)               # doubled-droplet = hits
    # Re-join digits/signs split by kerning: "( - 1 0 )" → "(-10)", "+ 2" → "+2".
    for _ in range(3):
        text = re.sub(r"(\d)\s+(\d)", r"\1\2", text)
        text = re.sub(r"([-+])\s+(\d)", r"\1\2", text)
        text = re.sub(r"(\d)\s+([%'])", r"\1\2", text)
    text = re.sub(r"\(\s+", "(", text)
    text = re.sub(r"\s+\)", ")", text)
    text = re.sub(r"\[\s+", "[", text)
    text = re.sub(r"\s+\]", "]", text)
    text = re.sub(r"\s+([,.;])", r"\1", text)
    return re.sub(r"\s+", " ", text).strip()


def join_text(words):
    """Words (already in reading order) → de-hyphenated, normalized text."""
    text = " ".join(w["text"] for w in words)
    text = re.sub(r"(\w)-\s+(\w)", r"\1\2", text)       # mend line-break hyphens
    return normalize(text)


def extract_page(pg):
    # use_text_flow preserves word integrity (justified kerning otherwise splits
    # words into 1-2 char fragments) and keeps effect numbers attached.
    words = pg.extract_words(use_text_flow=True)

    # Title: top line, before "Critical", excluding any header-row tokens.
    skip = {"Roll", "A", "B", "C", "D", "E"}
    top_line = sorted((w for w in words if w["top"] < 50), key=lambda w: w["x0"])
    name = []
    for w in top_line:
        if w["text"] == "Critical":
            break
        if w["text"] not in skip:
            name.append(w["text"])
    name = " ".join(name).strip()

    # Severity header row (Roll A B C D E) fixes the column centres. It sits just
    # below the title; the same letters recur as prose lower down, so anchor on
    # the header's "Roll" token and only take A-E on that line.
    roll_hdr = next((w for w in words if w["text"] == "Roll" and w["top"] < 75), None)
    if roll_hdr is None:
        return None
    hdr_top = roll_hdr["top"]
    header = {w["text"]: (w["x0"] + w["x1"]) / 2
              for w in words if abs(w["top"] - hdr_top) < 5 and w["text"] in SEVERITIES}
    if len(header) != 5:
        return None
    centres = [header[s] for s in SEVERITIES]
    roll_centre = (roll_hdr["x0"] + roll_hdr["x1"]) / 2
    # The Roll column is narrow; column A's text (left-aligned) starts just right
    # of the roll numbers while its header letter is centred far right. Anchor the
    # divider a fixed offset right of the roll-header centre so the left edge of
    # column A's prose isn't clipped (and footnote superscripts stay excluded).
    # Scales with the table when a page is shifted left.
    roll_a_div = roll_centre + 18
    bounds = [roll_a_div]
    for i in range(4):
        bounds.append((centres[i] + centres[i + 1]) / 2)
    bounds.append(600.0)
    col_x = list(zip(bounds[:-1], bounds[1:]))          # 5 (lo,hi) bands, A..E
    header_bottom = max(w["bottom"] for w in words
                        if abs(w["top"] - hdr_top) < 5 and w["text"] in set(SEVERITIES) | {"Roll"})

    # Roll rows: numeric/dash tokens left of the Roll/A divider (this excludes the
    # alphabetic rotated location labels). Reassemble split tokens ('2','-','3').
    roll_toks = [w for w in words
                 if (w["x0"] + w["x1"]) / 2 < roll_a_div and w["top"] > header_bottom
                 and re.fullmatch(r"[\d-]+", w["text"])]
    rows_by_top = {}
    for w in sorted(roll_toks, key=lambda w: (w["top"], w["x0"])):
        key = next((k for k in rows_by_top if abs(k - w["top"]) <= 4), w["top"])
        rows_by_top.setdefault(key, []).append(w)
    roll_rows = []
    for top in sorted(rows_by_top):
        toks = sorted(rows_by_top[top], key=lambda w: w["x0"])
        # Keep only the contiguous run from the leftmost token; a footnote
        # superscript (e.g. "²") sits to the right with a wide gap and is dropped.
        run = [toks[0]]
        for t in toks[1:]:
            if t["x0"] - run[-1]["x1"] <= 4:
                run.append(t)
            else:
                break
        if run[0]["x0"] >= roll_centre + 16:      # starts in the footnote sliver
            continue
        m = re.fullmatch(r"(\d+)(?:-(\d+))?", "".join(t["text"] for t in run))
        if not m:
            continue
        lo, hi = int(m.group(1)), int(m.group(2) or m.group(1))
        if 1 <= lo <= hi <= 100:
            roll_rows.append({"top": top, "low": lo, "high": hi})
    if not roll_rows:
        return None

    # Rotated body-location labels (reversed text) in the far-left margin. These
    # come out cleanly from a plain extract_words pass (text_flow mangles them).
    loc_labels = []
    for w in pg.extract_words():
        if (w["x0"] + w["x1"]) / 2 < roll_a_div and w["top"] > header_bottom:
            loc = next((c for c in (w["text"], w["text"][::-1]) if c in LOCATIONS), None)
            if loc:
                loc_labels.append({"top": w["top"], "loc": loc})

    # Row vertical bands: midpoints between consecutive roll tops.
    content_bottom = max(w["bottom"] for w in words
                         if w["x0"] > 72 and w["top"] > header_bottom)
    tops = [r["top"] for r in roll_rows]
    out_rows = []
    for i, r in enumerate(roll_rows):
        lo_y = header_bottom - 1 if i == 0 else (tops[i - 1] + tops[i]) / 2
        hi_y = (tops[i] + tops[i + 1]) / 2 if i + 1 < len(roll_rows) else content_bottom + 2

        loc = min(loc_labels, key=lambda l: abs(l["top"] - r["top"]), default=None)
        loc = loc["loc"] if loc and abs(loc["top"] - r["top"]) < 16 else None

        cells = {}
        for sev, (x0, x1) in zip(SEVERITIES, col_x):
            cw = [w for w in words if lo_y <= w["top"] < hi_y and x0 <= (w["x0"] + w["x1"]) / 2 < x1]
            cw.sort(key=lambda w: (round(w["top"] / 3), w["x0"]))
            cells[sev.lower()] = join_text(cw)

        out_rows.append({
            "rollLow": r["low"], "rollHigh": r["high"], "location": loc,
            "a": cells["a"], "b": cells["b"], "c": cells["c"],
            "d": cells["d"], "e": cells["e"],
        })

    return {"name": name, "rows": out_rows}


def main():
    pdf = pdfplumber.open(PDF)
    tables = []
    for i in range(FIRST_PAGE, LAST_PAGE + 1):
        t = extract_page(pdf.pages[i])
        if t:
            tables.append(t)
    with open(OUT, "w", encoding="utf-8") as f:
        json.dump(tables, f, indent=2, ensure_ascii=False)
    print(f"Wrote {len(tables)} critical tables to {OUT}")
    for t in tables:
        print(f"  {t['name']:<14} {len(t['rows'])} rows")


if __name__ == "__main__":
    main()
