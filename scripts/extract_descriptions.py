"""Extract correctly-attributed creature descriptions from the Creature Law PDF.

Strategy: pdftotext -layout preserves the two-column layout, so each description
sits adjacent to its own creature. We reconstruct columns into reading order,
then walk the stream pairing each creature stat block (anchored on "Category:")
with the prose paragraph that follows its Talents/Flaws list. Group-header
paragraphs (e.g. "Great Eagles", with no stat block) are captured separately and
later matched to their variant creatures by name key.

Output: scripts/extracted_descriptions.json  (list of {Name, Description})
        plus a console report of creatures with no captured description.
"""
import json
import re
import subprocess

PDF = "docs/rules/Rolemaster_Creature_Law_I.pdf"
SPLITX = 50
GAP = re.compile(r"\s{3,}")
NOISE = {"ROLEMASTER", "UNIFIED", "Creature Law"}
FIELD = re.compile(r"^(Category|Archetype|Size|Armor|Treasure|Realm|Misc|Stat Bonuses|"
                   r"Spells|Talents/Flaws|Talents\\Flaws|Variants|Outlook|Movement|"
                   r"Combat|Magical Stats|Encounter|Skills)\b", re.IGNORECASE)
STAT_HDR = re.compile(r"^AG\s+CO\s+EM\s+IN\s+ME\s+PR\s+QU\s+RE\s+SD\s+ST")
NUMS = re.compile(r"^-?\d+(\s+-?\d+){5,}$")
DP = re.compile(r"\(-?\d+\s*DP\)")
CHAPTER = re.compile(r"^\d+\.\d*\.?\s+\S")


def reconstruct(page):
    left, right = [], []
    for raw in page.split("\n"):
        line = raw.rstrip("\r")
        if not line.strip():
            left.append("")
            right.append("")
            continue
        segs, pos = [], 0
        for part in GAP.split(line):
            idx = line.find(part, pos) if part else pos
            if part.strip():
                segs.append((idx, part))
            pos = idx + len(part)
        left.append(" ".join(t for c, t in segs if c < SPLITX))
        right.append(" ".join(t for c, t in segs if c >= SPLITX))
    return left, right


def build_stream():
    out = subprocess.run(["pdftotext", "-layout", PDF, "-"],
                         capture_output=True, text=True, encoding="utf-8",
                         errors="replace").stdout
    lines = []
    for page in out.split("\f"):
        L, R = reconstruct(page)
        lines.extend(L)
        lines.extend(R)
    return lines


def is_noise(s):
    s = s.strip()
    return (not s) or s in NOISE or bool(re.fullmatch(r"\d{1,3}", s)) or bool(CHAPTER.match(s))


LEAD_TALENT = re.compile(r"^(?:[^.]*?\(-?\d+\s*DP\)\s*)+")


NOISE_TOK = re.compile(r"ROLEMASTER|UNIFIED|Creature Law")


def clean(text):
    text = NOISE_TOK.sub("", text)           # strip page-footer tokens that bled in
    text = re.sub(r"-\s+", "", text)         # join hyphenated line breaks
    text = re.sub(r"\s+", " ", text).strip()
    text = LEAD_TALENT.sub("", text).strip()  # drop a leaked leading talent fragment
    return text


def has_noise(text):
    return bool(NOISE_TOK.search(text))


def looks_like_name(s):
    s = s.strip()
    if not s or len(s) > 60 or FIELD.match(s) or STAT_HDR.match(s) or NUMS.match(s):
        return False
    # Names are short title-ish lines, not full sentences.
    return s[0].isupper() and not s.endswith(".") and len(s.split()) <= 7


def main():
    lines = [l.rstrip() for l in build_stream()]

    # Find indices of "Category:" lines = stat-block anchors.
    creatures = []  # (name, cat_idx)
    for i, l in enumerate(lines):
        if re.match(r"^Category:", l.strip()):
            # name = nearest non-noise, non-field line above
            j = i - 1
            while j >= 0 and (is_noise(lines[j]) or not lines[j].strip()):
                j -= 1
            name = lines[j].strip() if j >= 0 and looks_like_name(lines[j]) else ""
            creatures.append((name, i, j))

    # For each creature, the description is the prose AFTER its Talents/Flaws block,
    # up to the next creature's name. Find Talents block end then collect prose.
    results = []
    for idx, (name, cat_idx, name_idx) in enumerate(creatures):
        next_name_idx = creatures[idx + 1][2] if idx + 1 < len(creatures) else len(lines)
        # locate Talents/Flaws within [cat_idx, next stat region]
        tal = None
        for k in range(cat_idx, min(next_name_idx, len(lines))):
            if re.match(r"^Talents[\\/]Flaws:", lines[k].strip()):
                tal = k
                break
        desc_lines = []
        if tal is not None:
            k = tal + 1
            # The talents list is the run of consecutive non-blank lines after the label.
            while k < next_name_idx and lines[k].strip():
                k += 1
            # Skip blank line(s), then collect prose up to the next creature's name,
            # skipping any stray field/noise lines.
            while k < next_name_idx:
                s = lines[k].strip()
                if s and not FIELD.match(s) and not is_noise(s):
                    desc_lines.append(s)
                k += 1
        desc = clean(" ".join(desc_lines))

        # Fallback 1: pre-name prose (group-header description placed before the
        # stat block, e.g. "Great Eagles Massive creatures of the air...").
        if not desc and name_idx is not None:
            pre = []
            j = name_idx - 1
            while j >= 0:
                s = lines[j].strip()
                if not s or is_noise(s):
                    j -= 1
                    continue
                if FIELD.match(s) or STAT_HDR.match(s) or NUMS.match(s) or looks_like_name(s) or DP.search(s):
                    break
                pre.insert(0, s)
                j -= 1
            desc = clean(" ".join(pre))

        results.append({"Name": name, "Description": desc, "_idx": cat_idx})

    # Fallback 2: variant inheritance — an empty variant copies a sibling that
    # shares the same name prefix (before the first comma), e.g. all "Great Eagle, *".
    by_prefix = {}
    for r in results:
        if r["Name"] and r["Description"]:
            by_prefix.setdefault(r["Name"].split(",")[0].strip(), r["Description"])
    inherited = 0
    for r in results:
        if r["Name"] and not r["Description"]:
            key = r["Name"].split(",")[0].strip()
            if "," in r["Name"] and key in by_prefix:
                r["Description"] = by_prefix[key]
                inherited += 1
    print(f"Inherited from sibling: {inherited}")

    json.dump(results, open("scripts/extracted_descriptions.json", "w", encoding="utf-8"),
              ensure_ascii=False, indent=2)
    named = [r for r in results if r["Name"]]
    print(f"Stat blocks found: {len(results)}; with name: {len(named)}")
    print(f"With description: {sum(1 for r in named if r['Description'])}")
    print("\n--- sample ---")
    for r in named[:4]:
        print(f"[{r['Name']}] {r['Description'][:120]}")


if __name__ == "__main__":
    main()
