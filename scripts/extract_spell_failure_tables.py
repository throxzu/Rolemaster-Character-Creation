"""Extract Rolemaster Spell Law spell failure tables (Tables 4-4a/b/c) from the PDF.

Three tables — Channeling, Essence and Mentalism — each keyed by spell failure
roll (rows) with four spell-list-type columns (Elemental, Force, Informational,
Utility) of descriptive prose. Same coordinate approach as the fumble tables.

Output: docs/game-data/spell-failure-tables.json
Usage:  python scripts/extract_spell_failure_tables.py
"""
import json
import re

import pdfplumber

PDF = "docs/rules/Rolemaster_Spell_Law_(RMU).pdf"
OUT = "docs/game-data/spell-failure-tables.json"

# Spell failure table pages (0-based) → realm name.
PAGES = {42: "Channeling", 43: "Essence", 44: "Mentalism"}

MAX_ROLL = 300  # spell failure tables run 1–300

# Webdings/Wingdings effect glyphs → readable tokens (shared legend notation).
GLYPHS = {
    chr(0xF053): " bleed", chr(0xF0DE): " fatigue", chr(0xF040): " breakage",
    chr(0xF02B): " stun", chr(0xF0AE): " stagger", chr(0xF05F): " knockback",
    chr(0xF0CA): " prone", chr(0xF022): " grapple", chr(0xF0A5): " +crit",
    chr(0x1F480): " KILLED",
}


def normalize(text):
    for g, repl in GLYPHS.items():
        text = text.replace(g + g, repl).replace(g, repl)
    text = re.sub(r"O\s*O", " hits", text)
    text = text.replace("–", "-")
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
    text = " ".join(w["text"] for w in words)
    text = re.sub(r"(\w)-\s+(\w)", r"\1\2", text)
    return normalize(text)


def cluster_columns(header_words):
    """Group header words into columns by horizontal gaps; return [(name, centre)]."""
    header_words = sorted(header_words, key=lambda w: w["x0"])
    groups, cur = [], [header_words[0]]
    for w in header_words[1:]:
        if w["x0"] - cur[-1]["x1"] > 35:
            groups.append(cur)
            cur = [w]
        else:
            cur.append(w)
    groups.append(cur)
    return [(" ".join(g["text"] for g in grp),
             (grp[0]["x0"] + grp[-1]["x1"]) / 2) for grp in groups]


def extract_page(pg, name):
    words = pg.extract_words(use_text_flow=True)

    roll_hdr = next((w for w in words if w["text"] == "Roll" and w["top"] < 70), None)
    if roll_hdr is None:
        return None
    hdr_top = roll_hdr["top"]
    header_bottom = roll_hdr["bottom"]
    roll_centre = (roll_hdr["x0"] + roll_hdr["x1"]) / 2

    header_words = [w for w in words if abs(w["top"] - hdr_top) < 5 and w["text"] != "Roll"]
    columns = cluster_columns(header_words)
    centres = [c for _, c in columns]

    roll_a_div = roll_centre + 18
    bounds = [roll_a_div]
    for i in range(len(centres) - 1):
        bounds.append((centres[i] + centres[i + 1]) / 2)
    bounds.append(600.0)
    col_x = list(zip(bounds[:-1], bounds[1:]))

    roll_toks = [w for w in words
                 if (w["x0"] + w["x1"]) / 2 < roll_a_div and w["top"] > header_bottom
                 and re.fullmatch(r"[\d–-]+", w["text"])]
    rows_by_top = {}
    for w in sorted(roll_toks, key=lambda w: (w["top"], w["x0"])):
        key = next((k for k in rows_by_top if abs(k - w["top"]) <= 4), w["top"])
        rows_by_top.setdefault(key, []).append(w)
    roll_rows = []
    for top in sorted(rows_by_top):
        toks = sorted(rows_by_top[top], key=lambda w: w["x0"])
        run = [toks[0]]
        for t in toks[1:]:
            if t["x0"] - run[-1]["x1"] <= 4:
                run.append(t)
            else:
                break
        if run[0]["x0"] >= roll_centre + 16:
            continue
        label = "".join(t["text"] for t in run).replace("–", "-")
        m = re.fullmatch(r"(\d+)(?:-(\d+))?", label)
        if not m:
            continue
        lo, hi = int(m.group(1)), int(m.group(2) or m.group(1))
        if 1 <= lo <= hi <= MAX_ROLL:
            roll_rows.append({"top": top, "low": lo, "high": hi})
    if not roll_rows:
        return None

    content_bottom = max(w["bottom"] for w in words
                         if w["x0"] > roll_a_div and w["top"] > header_bottom)
    tops = [r["top"] for r in roll_rows]
    out_rows = []
    for i, r in enumerate(roll_rows):
        lo_y = header_bottom - 1 if i == 0 else (tops[i - 1] + tops[i]) / 2
        hi_y = (tops[i] + tops[i + 1]) / 2 if i + 1 < len(roll_rows) else content_bottom + 2
        cells = []
        for x0, x1 in col_x:
            cw = [w for w in words if lo_y <= w["top"] < hi_y and x0 <= (w["x0"] + w["x1"]) / 2 < x1]
            cw.sort(key=lambda w: (round(w["top"] / 3), w["x0"]))
            cells.append(join_text(cw))
        out_rows.append({"rollLow": r["low"], "rollHigh": r["high"], "cells": cells})

    return {"name": name, "columns": [c for c, _ in columns], "rows": out_rows}


def main():
    pdf = pdfplumber.open(PDF)
    tables = [extract_page(pdf.pages[i], name) for i, name in PAGES.items()]
    tables = [t for t in tables if t]
    with open(OUT, "w", encoding="utf-8") as f:
        json.dump(tables, f, indent=2, ensure_ascii=False)
    print(f"Wrote {len(tables)} spell failure tables to {OUT}")
    for t in tables:
        print(f"  {t['name']:<12} {len(t['rows'])} rows, cols={t['columns']}")


if __name__ == "__main__":
    main()
