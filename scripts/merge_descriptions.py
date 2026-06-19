"""Merge PDF-extracted descriptions onto the canonical creature names.

The page looks up descriptions by the parser's creature name, so we keep the
existing names in docs/game-data/creature-descriptions.json and replace each
description with the correctly-attributed one extracted from the PDF (matched by
normalised name). Prints a quality report of entries that still need review.
"""
import json
import re

CANON = "docs/game-data/creature-descriptions.json"
EXTRACT = "scripts/extracted_descriptions.json"
NOISE = re.compile(r"ROLEMASTER|UNIFIED|Creature Law")
DP = re.compile(r"\(-?\d+\s*DP\)")


def norm(s):
    return re.sub(r"[^a-z0-9]", "", s.lower())


def main():
    canon = json.load(open(CANON, encoding="utf-8"))
    extract = json.load(open(EXTRACT, encoding="utf-8"))

    emap = {}
    for r in extract:
        if not r["Name"]:
            continue
        k = norm(r["Name"])
        # prefer the longest clean candidate for a given normalised name
        cur = emap.get(k, "")
        if len(r["Description"]) > len(cur):
            emap[k] = r["Description"]

    matched, unmatched, flagged = 0, [], []
    for c in canon:
        k = norm(c["Name"])
        new = emap.get(k)
        if new:
            c["Description"] = new
            matched += 1
        else:
            unmatched.append(c["Name"])
        d = c["Description"]
        reasons = []
        if not d:
            reasons.append("EMPTY")
        else:
            if d[0].islower():
                reasons.append("starts-lowercase")
            if len(d.split()) < 12:
                reasons.append("short")
            if NOISE.search(d):
                reasons.append("noise")
            if DP.search(d):
                reasons.append("DP-leak")
        if reasons:
            flagged.append((c["Name"], ",".join(reasons), d[:90]))

    json.dump(canon, open(CANON, "w", encoding="utf-8"), ensure_ascii=False, indent=2)

    print(f"Canonical: {len(canon)}; matched from PDF: {matched}; unmatched: {len(unmatched)}")
    print("\n--- UNMATCHED (kept old text) ---")
    for n in unmatched:
        print("  " + n)
    print(f"\n--- FLAGGED for review ({len(flagged)}) ---")
    for n, why, head in flagged:
        print(f"  [{why}] {n}: {head}")


if __name__ == "__main__":
    main()
