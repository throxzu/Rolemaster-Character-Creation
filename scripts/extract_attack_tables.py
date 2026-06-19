"""Extract Rolemaster Core Law Chapter 10 attack tables from the PDF.

Each attack-table page (Arming Sword .. Whip) carries three result grids
(Small / Medium / Big). A grid is keyed by attack roll range (rows) x Armor
Type 1-10 (columns); each cell is a raw result string like "26FP" (26 hits,
severity F, critical type P). Blank cells = no result.

pdfplumber gives clean word coordinates, so we reconstruct each grid by:
  1. locating its "Roll" + "1..10" header row to fix the 10 column x-centres,
  2. collecting roll-range words (\\d+-\\d+) below the header to fix the rows,
  3. snapping every result word to its nearest column centre.

The "first quadrant" metadata block (criticals, disarm/subdual modifiers, the
weapon stat group, and any range / area-effect notes) is captured best-effort;
rotated header tokens are reversed back to reading order. The resulting JSON is
the curated, hand-correctable source of truth seeded into SQL.

Output: docs/game-data/attack-tables.json
Usage:  python scripts/extract_attack_tables.py
"""
import json
import re
import sys

import pdfplumber

PDF = "docs/rules/Rolemaster_Core_Law_(RMU).pdf"
OUT = "docs/game-data/attack-tables.json"

# Chapter 10 attack-table pages (0-based pdfplumber indices), inclusive.
FIRST_PAGE, LAST_PAGE = 185, 223

# Attack category for page grouping / favorites matching.
NATURAL = {
    "Beak", "Bite", "Claw", "Crush", "Grapple", "Horn", "Ram", "Stinger",
    "Trample", "Unarmed Strikes", "Unarmed Sweeps",
}


def categorize(name):
    if name.startswith(("Ball,", "Bolt,")):
        return "Spell"
    if name in NATURAL:
        return "Natural"
    return "Weapon"


ROLL = re.compile(r"^\d{2,3}-\d{2,3}$")
CELL = re.compile(r"^\d{1,3}[A-Z]{0,2}$")          # 26FP, 14DP, 8, 6ZK ...
SIZEMOD = re.compile(r"^[+-]\d$")                    # +0 -1 +1


def center(w):
    return (w["x0"] + w["x1"]) / 2


def is_cell(text):
    """A result cell: hits + optional severity/crit letters. Reject stray
    pure-digit tokens like printed page numbers (184-223) — no result delivers
    that many hits, while bare low-hit cells ('8', '6') are well under 60."""
    if not CELL.match(text):
        return False
    if text.isdigit() and int(text) > 60:
        return False
    return True


def find_grid(words, roll_word, col_count=10):
    """Given the 'Roll' header word, return (col_centres, header_top)."""
    top = roll_word["top"]
    headers = [
        w for w in words
        if abs(w["top"] - top) < 3 and w["x0"] > roll_word["x1"] - 2
        and re.fullmatch(r"\d{1,2}", w["text"])
    ]
    headers.sort(key=lambda w: w["x0"])
    centres = [center(w) for w in headers[:col_count]]
    return centres, top


def collect_grid(words, roll_word, col_centres, header_top, x_max, top_max):
    """Build [{rollLow, rollHigh, cells:[c1..c10]}] for one grid."""
    x_min = roll_word["x0"] - 4
    # roll-range words that anchor each row
    row_words = [
        w for w in words
        if ROLL.match(w["text"]) and header_top + 3 < w["top"] < top_max
        and x_min - 2 <= w["x0"] <= roll_word["x1"] + 6
    ]
    row_words.sort(key=lambda w: w["top"])

    # candidate result cells inside this grid's box
    cells = [
        w for w in words
        if is_cell(w["text"]) and header_top + 3 < w["top"] < top_max
        and col_centres[0] - 12 <= center(w) <= col_centres[-1] + 12
        and w["x0"] >= roll_word["x1"] - 2
    ]

    spacing = (col_centres[-1] - col_centres[0]) / (len(col_centres) - 1)
    tol = spacing / 2 + 2

    rows = []
    for i, rw in enumerate(row_words):
        lo_top = rw["top"] - 4
        hi_top = row_words[i + 1]["top"] - 4 if i + 1 < len(row_words) else top_max
        line = [c for c in cells if lo_top <= c["top"] < hi_top]
        slots = [None] * len(col_centres)
        for c in line:
            cx = center(c)
            j = min(range(len(col_centres)), key=lambda k: abs(col_centres[k] - cx))
            if abs(col_centres[j] - cx) <= tol and slots[j] is None:
                slots[j] = c["text"]
        if not any(slots):
            continue
        lo, hi = rw["text"].split("-")
        rows.append({"rollLow": int(lo), "rollHigh": int(hi), "cells": slots})
    return rows


def label_value(words, label_top, after_x, x_max=300):
    """Join words on a given top line between after_x and x_max (left block only).

    Bounded right by x_max so grid cells that happen to share the label's
    vertical position (the grids sit to the right) are not swept in.
    """
    parts = [w for w in words if abs(w["top"] - label_top) < 3
             and after_x - 2 <= w["x0"] < x_max]
    parts.sort(key=lambda w: w["x0"])
    return " ".join(w["text"] for w in parts).strip()


def looks_mirrored(t):
    # Rotated column headers come out reversed, e.g. 'trohS', 'htgneL', ')02+('.
    return t[::-1] in {
        "Blank", "Point", "Short", "Medium", "Long", "Extreme", "Absurd",
        "Size", "Length", "Strength", "Weight", "Fumble",
    } or re.fullmatch(r"\(\+?-?\d+\)", t[::-1]) is not None


def extract_meta(words, small_top):
    """Best-effort metadata block (left of grids, above the Small label)."""
    meta = {"critTypes": "", "disarmMod": "", "subdualMod": "",
            "weapons": [], "notes": ""}

    crit = next((w for w in words if w["text"] == "Criticals:"), None)
    if crit:
        meta["critTypes"] = label_value(words, crit["top"], crit["x1"])

    for key, lbl in (("disarmMod", "Disarm"), ("subdualMod", "Subdual")):
        anchor = next((w for w in words if w["text"] == lbl and w["top"] > 240), None)
        if anchor:
            mod = next((w for w in words if w["text"] == "modifier:"
                        and abs(w["top"] - anchor["top"]) < 3), None)
            if mod:
                meta[key] = label_value(words, anchor["top"], mod["x1"])

    # Weapon stat group: rows with a size modifier (+0/-1/+1) left of the grids.
    sub_top = next((w["top"] for w in words if w["text"] == "Subdual"), 260)
    region = [w for w in words if w["x0"] < 300 and sub_top + 6 < w["top"] < small_top - 4]
    by_top = {}
    for w in region:
        by_top.setdefault(round(w["top"] / 4) * 4, []).append(w)

    # Stat columns (x-centres) on weapon pages.
    cols = {"sizeMod": 178, "length": 201, "strength": 222, "weight": 244, "fumble": 266}
    note_bits = []
    for top in sorted(by_top):
        line = sorted(by_top[top], key=lambda w: w["x0"])
        nums = [w for w in line if w["x0"] > 170]
        has_size = any(SIZEMOD.match(w["text"]) for w in nums)
        if has_size:
            name = " ".join(w["text"] for w in line if w["x0"] <= 170).strip()
            entry = {"name": name, "sizeMod": None, "length": None,
                     "strength": None, "weight": None, "fumble": None}
            for w in nums:
                field = min(cols, key=lambda f: abs(cols[f] - center(w)))
                if abs(cols[field] - center(w)) < 12:
                    entry[field] = w["text"]
            if name:
                meta["weapons"].append(entry)
        else:
            for w in line:
                t = w["text"][::-1] if looks_mirrored(w["text"]) else w["text"]
                # Drop the rotated stat-group column headers (already structured
                # into `weapons`); keep range headers (Short/Long/...) for context.
                if t in {"Size", "Length", "Strength", "Weight", "Fumble"}:
                    continue
                note_bits.append(t)
    meta["notes"] = re.sub(r"\s+", " ", " ".join(note_bits)).strip()
    return meta


def extract_page(pg):
    words = pg.extract_words()
    name = " ".join(
        w["text"] for w in sorted(
            (w for w in words if w["top"] < 50 and w["x0"] < 300), key=lambda w: w["x0"]))
    name = name.strip()

    roll_words = [w for w in words if w["text"] == "Roll"]
    grids = {}
    for rw in roll_words:
        centres, htop = find_grid(words, rw)
        if len(centres) != 10:
            continue
        if htop < 100:                                   # Medium: top-right
            size, x_max, top_max = "Medium", 560, 400
        elif rw["x0"] < 200:                             # Small: bottom-left
            size, x_max, top_max = "Small", 300, 780
        else:                                            # Big: bottom-right
            size, x_max, top_max = "Big", 560, 780
        grids[size] = collect_grid(words, rw, centres, htop, x_max, top_max)

    small_top = next((w["top"] for w in words if w["text"] == "Small"), 405)
    meta = extract_meta(words, small_top)

    return {
        "name": name,
        "category": categorize(name),
        "critTypes": meta["critTypes"],
        "disarmMod": meta["disarmMod"],
        "subdualMod": meta["subdualMod"],
        "weapons": meta["weapons"],
        "notes": meta["notes"],
        "sizes": {s: grids.get(s, []) for s in ("Small", "Medium", "Big")},
    }


def main():
    pdf = pdfplumber.open(PDF)
    tables = []
    issues = []
    for i in range(FIRST_PAGE, LAST_PAGE + 1):
        t = extract_page(pdf.pages[i])
        tables.append(t)
        for size, rows in t["sizes"].items():
            if not rows:
                issues.append(f"{t['name']}: {size} grid empty")
            for r in rows:
                # report rows where an interior column is blank but neighbours filled
                pass
    with open(OUT, "w", encoding="utf-8") as f:
        json.dump(tables, f, indent=2, ensure_ascii=False)
    print(f"Wrote {len(tables)} attack tables to {OUT}")
    for ln in issues:
        print("  WARN", ln, file=sys.stderr)


if __name__ == "__main__":
    main()
