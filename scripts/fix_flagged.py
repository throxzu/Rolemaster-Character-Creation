"""Pull clean, correctly-attributed descriptions for the flagged creatures.

Each flagged creature is given an anchor phrase (the first words of its real
description in the reconstructed PDF text). We collect from that anchor until the
next stat field / ALL-CAPS group header, then scrub interleaved page-footer and
stat-block fragments. Outputs scripts/flagged_fixes.json for review.
"""
import json
import re

SRC = "scripts/reconstructed.txt"
FIELD = re.compile(r"^(Category|Archetype|Size|Armor|Treasure|Realm|Misc|Stat Bonuses|"
                   r"Spells|Talents[\\/]Flaws|Variants|Outlook|Hits|Level)\b")
ALLCAPS = re.compile(r"^[A-Z][A-Z'()/ ,&-]{4,}$")
NOISE = re.compile(r"ROLEMASTER|UNIFIED|Creature Law")
PAGENUM = re.compile(r"\b\d{2,3}\b")
# embedded right-column stat fragments that bleed into a description line.
# Value = up to 3 whole words so we never eat into the following real word.
STATFRAG = re.compile(r"\b(Heal|Realm|Archetype|Size|Armor|Treasure|Category|Misc|Hits|AT|DB|Init|Outlook)"
                      r"\s*:\s*[A-Za-z0-9'\"+\-]+(?:\s+[A-Za-z0-9'\"+\-]+){0,2}")
LEAD_TALENT = re.compile(r"^(?:[^.]*?\(-?\d+\s*DP\)\s*)+")
DPFRAG = re.compile(r"\s*;?\s*[A-Za-z][A-Za-z ,\[\]'+\d/-]*\(-?\d+\s*DP\)")

ANCHORS = {
    "Air Drake, Young": "Generally, these winged serpents are brown, green,",
    "Fire Drake, Young": "Fire Drakes are typically red or reddish gold",
    "Gas Drake, Young": "Grey, green, black, or brown, Gas Drakes blend",
    "Light Drake, Young": "Generally black, bluish black, sky-blue",
    "Sea Drake, Young": "Sea Drakes are usually green, blue, blue-green",
    "Bat, diminutive": "Of the order Chiroptera, bats are successful",
    "Bat, miniscule": "Of the order Chiroptera, bats are successful",
    "Cat, small": "The cat body type is powerful and very flexible",
    "Lion, female": "Lions are characterized by their broad",
    "Octopus/ Squid, medium": "Octopuses and squid are both invertebrates",
    "Octopus/ Squid, small": "Octopuses and squid are both invertebrates",
    "Jellyfish": "Jellyfish are recognized by a floating",
    "Ray, Electric": "Electric Rays, or Torpedo Rays, are capable",
    "Kangaroo/ Wallaby, medium": "Kangaroos and wallabies are both herbivorous",
    "Siren": "Sirens resemble Harpies in form",
    "Mustelid / Civet, tiny": "Mustelids include badgers, ermines",
    "Whale, Baleen": "Baleen whales are those that feed with their baleen",
    "Whale, Beaked": "Beaked whales comprise 18 species",
    "Antelope, small": "In this grouping of creatures are the generally",
    "Birds of Prey, Eagle": "Eagles are formidable predators, soaring for long",
    "Dolphin, Killer Whale": "Dolphins and porpoises are mammals that are familiar",
    "Construct, Minor": "Constructs are made from several parts",
    "Ogre, big": "An unkempt, slovenly race whose coarse, grizzled",
    "Ogre, large": "An unkempt, slovenly race whose coarse, grizzled",
    "Werebear, Lesser": "In its transformed state, the Werebear has the form",
    "Maazhat, Lieutenants": "Medium arthropods encased in the armor of a chi",
    "Maazhat, Workers/Drones": "Medium arthropods encased in the armor of a chi",
    "Ki-Rin": "By day, the sun bounces from the golden fur",
    "Camel, Alpaca/Llama": "Alpacas and llamas are smaller than camels",
    "Crab/ Lobster": "Crabs and lobsters are related crustaceans",
    "Vestice": "These beings in their untransformed state seem",
    "Great Spider, Major": "All Great Spiders feed on flesh and blood",
    "Elemental Guardian, Fire": "Guardians are Elementals and there is a type",
}

# Targeted text fixes for the few page-footer / merged-line artefacts.
POST_REPLACE = {
    "Maazhat, Lieutenants": [("have 290 become", "have become")],
    "Maazhat, Workers/Drones": [("have 290 become", "have become")],
}

# Entries the layout merges too badly to auto-extract: supply clean text directly.
MANUAL = {
    "Whale, Beaked": "Beaked whales comprise 18 species, all of which have slender bodies, "
                     "long snouts, and generally only one or two pairs of teeth. They feed on "
                     "fish, squid, and crustaceans.",
}


def clean(text):
    text = NOISE.sub(" ", text)
    text = STATFRAG.sub(" ", text)
    text = re.sub(r"-\s+", "", text)
    text = re.sub(r"\s+", " ", text).strip()
    text = LEAD_TALENT.sub("", text)
    text = DPFRAG.sub("", text)                        # drop trailing talent costs
    return re.sub(r"\s+", " ", text).strip()


def main():
    lines = open(SRC, encoding="utf-8").read().split("\n")
    out = {}
    for name, anchor in ANCHORS.items():
        start = next((i for i, l in enumerate(lines) if anchor in l), None)
        if start is None:
            out[name] = ""
            continue
        buf = []
        for k in range(start, min(start + 40, len(lines))):
            s = lines[k].strip()
            nxt = lines[k + 1].strip() if k + 1 < len(lines) else ""
            if k > start and (FIELD.match(s) or ALLCAPS.match(s) or nxt.startswith("Category:")):
                break
            buf.append(s)
        d = clean(" ".join(buf))
        for old, new in POST_REPLACE.get(name, []):
            d = d.replace(old, new)
        out[name] = d

    for name, text in MANUAL.items():
        out[name] = text
    json.dump(out, open("scripts/flagged_fixes.json", "w", encoding="utf-8"),
              ensure_ascii=False, indent=2)
    for n, d in out.items():
        print(f"### {n} [{len(d)}]\n{d[:220]}\n")


if __name__ == "__main__":
    main()
