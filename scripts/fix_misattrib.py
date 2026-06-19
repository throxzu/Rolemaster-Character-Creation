"""Fix the remaining mis-attributed / garbage descriptions found by the content audit.

Three mechanisms:
  SWAP   - the correct text already sits (clean) on another entry; copy it over.
  MANUAL - supply hand-verified clean text directly.
  ANCHOR - pull the description from the reconstructed PDF text at an anchor phrase.
"""
import json
import re

CANON = "docs/game-data/creature-descriptions.json"
SRC = "scripts/reconstructed.txt"

FIELD = re.compile(r"^(Category|Archetype|Size|Armor|Treasure|Realm|Misc|Stat Bonuses|"
                   r"Spells|Talents[\\/]Flaws|Variants|Outlook|Hits|Level)\b")
ALLCAPS = re.compile(r"^[A-Z][A-Z'()/ ,&-]{4,}$")
NOISE = re.compile(r"ROLEMASTER|UNIFIED|Creature Law")
STATFRAG = re.compile(r"\b(Heal|Realm|Archetype|Size|Armor|Treasure|Category|Misc|Hits|AT|DB|Init|Outlook)"
                      r"\s*:\s*[A-Za-z0-9'\"+\-]+(?:\s+[A-Za-z0-9'\"+\-]+){0,2}")
DPFRAG = re.compile(r"\s*;?\s*[+A-Za-z][A-Za-z0-9 ,\[\]'+/-]*\(-?\d+\s*DP\)")
LEAD = re.compile(r"^[;,\s]+")

# correct text already present (clean) on another entry → copy
SWAP = {
    "Air Drake, Mature": "Air Drake, Young",
    "Cave Drake, Old": "Air Drake, Mature",   # currently holds the clean CAVE DRAKES text
    "Warder, Greater": "Shard, Greater",       # currently holds the clean WARDERS text
    "Dolphin, River": "Dolphin, Killer Whale", # dolphins share the group writeup
}

MANUAL = {
    "Hydra": ("The appearance of a hydra is akin to a fat serpent with a long neck and tail, "
              "two strong legs, and a pair of small arms. Most have nine heads, but some have as "
              "few as three. Hydras have 5'-10' necks, a 7'-15' body, and a 7'-15' tail. Hydras "
              "are flightless relatives of Dragons, dull-witted, multi-headed creatures seemingly "
              "spawned by some heinous nightmare. Hydras dwell near coastal areas, or swim in the "
              "sea. They cannot breathe underwater, but they can remain below the surface for at "
              "least one minute per level. They feed on large sea creatures or those that wander "
              "too close to the water's edge."),
}

ANCHOR = {
    "Bear, medium": "Bears are heavily-built animals with big heads and",
    "Owl, tiny": "Owls can be found in most areas of the world",
    "Carrion Birds, Vulture": "Vultures can be found over an extensive portion",
    "Eel, Moray": "Moray eels normally inhabit rocky shores",
    "Shard, Greater": "Shards are awful simu",
    "Spider, Poisonous": "Spiders are eight-legged invertebrates that are related",
    "Weretiger, Lesser": "There is very little to distinguish a normal human from",
    "Boar": "A giant boar which is commonly known to be the an",
    "Horse, War, Lesser": "Warhorses are trained against the din of combat",
    "Turtle, small": "Turtles are in the order of reptiles and are primarily",
    "Zephyr Hound, Water": "Thick, greenish-blue fur covers the hide of the evil",
}


def clean(text):
    text = NOISE.sub(" ", text)
    text = STATFRAG.sub(" ", text)
    text = re.sub(r"-\s+", "", text)
    text = re.sub(r"\s+", " ", text).strip()
    text = DPFRAG.sub("", text)
    text = LEAD.sub("", text)
    return re.sub(r"\s+", " ", text).strip()


def extract(lines, anchor):
    start = next((i for i, l in enumerate(lines) if anchor in l), None)
    if start is None:
        return ""
    buf = []
    for k in range(start, min(start + 40, len(lines))):
        s = lines[k].strip()
        nxt = lines[k + 1].strip() if k + 1 < len(lines) else ""
        if k > start and (FIELD.match(s) or ALLCAPS.match(s) or nxt.startswith("Category:")):
            break
        buf.append(s)
    return clean(" ".join(buf))


def main():
    canon = json.load(open(CANON, encoding="utf-8"))
    cur = {c["Name"]: c["Description"] for c in canon}
    snapshot = dict(cur)
    lines = open(SRC, encoding="utf-8").read().split("\n")

    new = {}
    for name, src in SWAP.items():
        new[name] = snapshot.get(src, "")
    for name, text in MANUAL.items():
        new[name] = clean(text)
    for name, anchor in ANCHOR.items():
        new[name] = extract(lines, anchor)

    for c in canon:
        if c["Name"] in new and new[c["Name"]]:
            c["Description"] = new[c["Name"]]

    json.dump(canon, open(CANON, "w", encoding="utf-8"), ensure_ascii=False, indent=2)
    for n in sorted(new):
        print(f"### {n} [{len(new[n])}]\n{new[n][:180]}\n")


if __name__ == "__main__":
    main()
