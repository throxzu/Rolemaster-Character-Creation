"""Ensure size/age/rank/sex variants of a creature all carry the shared description.

A variant suffix that is a SIZE/AGE/RANK/SEX word (big, medium, Young, Greater,
female, ...) means the creature shares its group's writeup, so empty siblings are
filled from a described sibling. Type suffixes (Fire, Clay, Cobra, ...) are left
distinct. Never overwrites an existing description.

Run with --apply to write; otherwise dry-run (report only).
"""
import json
import sys

NAMES = "scripts/creature-names.json"
CANON = "docs/game-data/creature-descriptions.json"

VARIANT = {
    "tiny", "diminutive", "miniscule", "small", "medium", "large", "big", "huge",
    "gigantic", "giant", "enormous", "immense",
    "young", "mature", "old", "juvenile", "adult",
    "lesser", "greater", "minor", "major", "female", "male",
}


def group_key(name):
    parts = [p.strip() for p in name.split(",")]
    if len(parts) >= 2 and parts[-1].lower() in VARIANT:
        return ", ".join(parts[:-1])
    return name


def main():
    apply = "--apply" in sys.argv
    names = json.load(open(NAMES, encoding="utf-8"))
    canon = json.load(open(CANON, encoding="utf-8"))
    desc = {c["Name"]: c["Description"] for c in canon}

    from collections import defaultdict
    groups = defaultdict(list)
    for n in names:
        groups[group_key(n)].append(n)

    additions = []
    for key, members in sorted(groups.items()):
        if len(members) < 2:
            continue
        described = [(m, desc[m]) for m in members if desc.get(m)]
        if not described:
            continue
        source = max((d for _, d in described), key=len)   # longest existing writeup
        for m in members:
            if not desc.get(m):
                additions.append((m, key, source))

    print(f"Groups with shared variants and a source description applied:")
    shown = set()
    for m, key, _ in additions:
        if key not in shown:
            shown.add(key)
            print(f"  [{key}] -> fill: " + ", ".join(a for a, k, _ in additions if k == key))
    print(f"\nTotal variant entries to add: {len(additions)}")

    if apply:
        existing = {c["Name"] for c in canon}
        for m, _, source in additions:
            if m not in existing:
                canon.append({"Name": m, "Description": source})
                existing.add(m)
        canon.sort(key=lambda c: c["Name"])
        json.dump(canon, open(CANON, "w", encoding="utf-8"), ensure_ascii=False, indent=2)
        print(f"APPLIED. JSON now has {len(canon)} entries.")


if __name__ == "__main__":
    main()
