# Context

Background on the Rolemaster Unified (RMU) ruleset for this project.

## What is Rolemaster?
Rolemaster Unified (RMU) is a detailed fantasy tabletop RPG by Iron Crown Enterprises (ICE).
This app implements **Rolemaster Core Law (RMCL)** — combining Character Law, Arms Law, and Gamemaster Law.

Core tenet: **any character may learn any skill**; professions make some skills cheaper. Exception: spell lists are restricted by realm attunement.

---

## Character Creation Steps (official checklist)

1. **Concept** — race, culture, profession
   - a. **Race** (Section 2.2) — stat bonuses/penalties, racial talents/flaws, bonus DP, height/weight
   - b. **Culture** (Section 2.3) — grants starting skill ranks before level 1
   - c. **Profession** (Section 2.4) — select 10 Professional Skills, 2 Knacks; spellcasters get realm + 6 base spell lists
2. **Generate Stats** (Section 2.5)
3. **Select Talents** (Section 2.6) — GM approval required; purchased with DP
4. **Purchase Skills** (Section 2.6) — costs depend on profession
5. *(If above level 1)* Repeat steps 3–4 for each level
6. **Finishing Touches** (Section 2.7) — buy equipment, calculate all derived values

---

## The 10 Stats

| Stat | Abbrev | Description | Also Used For |
|------|--------|-------------|---------------|
| Agility | Ag | Manual dexterity, grace, fine motor control | |
| Constitution | Co | Health, disease resistance, absorbing physical damage | Physical RR |
| Empathy | Em | Perceiving emotional states; forming attachments | Essence realm stat |
| Intuition | In | Subconscious thinking, luck, precognition | Channeling realm stat |
| Memory | Me | Retaining what has been learned | |
| Presence | Pr | Sense of self; projecting personality to affect others | Mentalism realm stat |
| Quickness | Qu | Speed, reflexes, conscious reaction time | DB, Initiative, BMR |
| Reasoning | Re | Drawing logical conclusions from available information | |
| Self-Discipline | SD | Mind over body; focus and willpower | Fear RR |
| Strength | St | Musculature and its effective use (relative to size) | Encumbrance |

### Stat Ranges
| Range | Label |
|-------|-------|
| 1–17 | Deficient/Poor |
| 18–41 | Below Average |
| 42–59 | Average (48–53 = 0 bonus) |
| 60–83 | Above Average |
| 84–95 | Superior |
| 96–100 | Exceptional |

### Stat Bonus Table (Table 2-5a)
| Stat | Bonus | Stat | Bonus |
|------|-------|------|-------|
| 1 | -15 | 60–65 | +2 |
| 2 | -14 | 66–71 | +3 |
| 3 | -13 | 72–77 | +4 |
| 4 | -12 | 78–83 | +5 |
| 5 | -11 | 84–86 | +6 |
| 6 | -10 | 87–89 | +7 |
| 7–8 | -9 | 90–92 | +8 |
| 9–11 | -8 | 93–94 | +9 |
| 12–14 | -7 | 95 | +10 |
| 15–17 | -6 | 96 | +11 |
| 18–23 | -5 | 97 | +12 |
| 24–29 | -4 | 98 | +13 |
| 30–35 | -3 | 99 | +14 |
| 36–41 | -2 | 100 | +15 |
| 42–47 | -1 | | |
| 48–53 | 0 | | |
| 54–59 | +1 | | |

Racial bonuses/penalties apply to the **stat bonus**, not the stat value itself.

---

## Generating Stats

**Rolled method:**
- Roll d100 three times per stat; reroll any result below 11
- Highest = **potential**, middle = **temporary** (starting value), lowest discarded
- Pick **any 2 boosts** (or 1 twice):
  - Replace one stat temp/potential with 56/78
  - Replace highest temp with 90; boost potential by 10 (max 100)
  - Replace second-highest temp with 85; boost potential by 10 (max 100)
  - Make 2 stat gain rolls
- Up to **2 swaps**: exchange the temp/potential pair of one stat with another

**Point-buy method (optional):**
- Roll potentials as normal; discard rolled temporaries
- 15 points to buy temporaries; each point = 1 bonus tier
- Stat 50 = 0 points; above 50 costs; below 50 refunds
- No temporary may exceed potential

### Stat Gains (per level from 2nd)
Pick 2 stats for gain rolls (or 1 stat twice). Roll die type based on current temp:

| Stat | Die |
|------|-----|
| 1–6 | d3–1 |
| 7–8 | d3 |
| 9–18 | d6 |
| 19–81 | d10 |
| 82–90 | d6 |
| 91–92 | d3 |
| 93–99 | d3–1 |

Cannot exceed potential. Additional gain rolls: 4 DP each.

---

## Derived Values (Finishing Touches)

| Value | Formula |
|-------|---------|
| Base Movement Rate (BMR) | 20'/round + ½ Quickness bonus (round up) + race stride |
| Defensive Bonus (DB) | 3 × Quickness stat bonus |
| Hits | Race Base Hits + Body Development skill bonus (×0.75 Small, ×1.5 Big) |
| Endurance | Body Development bonus + racial endurance modifier |
| Initiative | Quickness stat bonus |
| Power Points (PP) | Power Development skill bonus |
| Encumbrance | % of body weight; offset by Strength |
| Armor Type (AT) | 1 (no armor) to 10 (plate), based on torso armor |
| Skill Bonus | Stat bonuses + rank bonus + professional bonus + knacks + special |
| RR (Physical) | Constitution bonus + racial + 2/level |
| RR (Fear) | Self-Discipline bonus + racial + 2/level |
| RR (Channeling) | Intuition bonus + racial + 2/level |
| RR (Essence) | Empathy bonus + racial + 2/level |
| RR (Mentalism) | Presence bonus + racial + 2/level |

All RR types get +10 for magic of the character's own realm.

---

## Development Points

- **60 DP per level** (all characters)
- **+ race bonus DP** (up to 25 per level until exhausted)
- Spent on: skill ranks, talents, extra stat gain rolls (4 DP each)
- Leveling: 10,000 EP per level (30,000 total for level 3, etc.)

### Skill Bonus Formula
`Skill Bonus = Stat Bonus (category) + Stat Bonus (individual skill) + Rank Bonus + Prof Bonus + Knacks + Special`

Each skill category has 2 assigned stats; each individual skill adds a 3rd.

### Rank Bonus Reference (partial — full table in Chapter 3)
| Ranks | Bonus |
|-------|-------|
| 0 | –25 |
| 1 | +5 |
| 5 | +25 |

---

## Cultures (10 types, Table 2-3)

| Culture | Description |
|---------|-------------|
| Cosmopolitan | Large city; trade/commerce focus; any craft |
| Harsh | Wasteland survival (tundra, desert, lava, etc.) |
| Highland | Rough hills/mountains; limited agriculture + herding |
| Mariner | Coastal/river; fishing and trade |
| Nomad | Herding/hunting; portable property |
| Reaver | Raiders; lives by preying on others |
| Rural | Settled open lands; farming |
| Sylvan | Woodland; hunting and gathering |
| Underground | Caves/structures; mining, fungus farming |
| Urban | Towns and small cities; skilled craftsmen |

Culture grants starting skill ranks (see `skills.md` Table 2-3).

---

## Professions

A profession determines: skill costs, professional skills + knacks, realm (spellcasters), and 6 base spell lists.

**Professional Skills:** 15 listed per profession. Player chooses:
- **10** skills → +1/rank bonus (max +30 per skill)
- **2** skills → fixed +5 knack bonus

### Profession Groups

**Arms (non-magical focus):**
| Profession | Description |
|------------|-------------|
| Fighter | Primary combat; weapons, armor, mounted |
| Warrior Monk | Combat + discipline; unarmed, acrobatics |
| Thief | Stealth, subterfuge, mechanical skills |
| Rogue | Generalist arms; combat + outdoor + subterfuge |
| Laborer | Physical trades and crafts |
| Scholar | Academic/cerebral; lore, science, writing |

**Semi Spellcasters:**
| Profession | Realm | Description |
|------------|-------|-------------|
| Ranger | Channeling | Outdoor skills + nature spells |
| Paladin | Channeling | Holy warrior; combat + support spells |
| Monk | Mentalism | Martial artist; personal enhancement spells |
| Magent | Mentalism | Mage-agent; espionage + assassination spells |
| Bard | Essence | Musician/entertainer; music-woven spells |
| Dabbler | Essence | Subterfuge + Essence; commerce/mechanical spells |

**Pure Spellcasters:**
| Profession | Realm | Focus |
|------------|-------|-------|
| Cleric | Channeling | Life, divine channeling, undead/demon countering |
| Druid | Channeling | Nature, animals, weather, plants |
| Magician | Essence | Elemental attacks (earth, fire, ice, light, water, wind) |
| Illusionist | Essence | Misdirection; sense manipulation |
| Mentalist | Mentalism | Mind reading, mental communication, mind control |
| Lay Healer | Mentalism | Healing diseases/injuries, prosthetics |

**Hybrid Spellcasters:**
| Profession | Realms | Focus |
|------------|--------|-------|
| Healer | Channeling + Mentalism | Self-healing; taking on others' injuries |
| Sorcerer | Essence + Channeling | Destruction of living and inanimate matter |
| Mystic | Essence + Mentalism | Personal illusions, matter modification |

### Realm Stats
| Realm | Stat |
|-------|------|
| Channeling | Intuition |
| Essence | Empathy |
| Mentalism | Presence |
| Hybrid | Lower of the two realm stats |

Arms professions must still choose a realm at character creation (+10 to RRs vs own realm).

### Skill Cost Format
`X/Y` = X DP for first rank, Y DP for second rank in a category. (Full table in `skills.md`.)

---

## Core Mechanics

### Die Rolls
- **d100OE** (open-ended): 96–00 = roll again + add; 01–05 (first roll only) = roll again − subtract
- **2d10**: Initiative rolls
- **d100**: Critical/fumble tables (not open-ended)

### Maneuvers
- All resolved by d100OE + skill bonus + difficulty modifier
- **Success:** total ≥ 101
- **Absolute Maneuver (AM):** Pass/fail; 76–100 = partial; ≤75 = fail
- **Percentage Maneuver (PM):** Variable success (0–150%)
- Difficulty range: Casual (+70) to Nigh Impossible (–100)

### Combat
- Attack: d100OE + OB – DB → consult weapon attack table vs opponent's AT
- Low unmodified roll = Fumble; successful hit may produce a Critical

### Resistance Rolls (RR)
- Not modified by skill; based on stats, level, talents/flaws
- No attacker skill: must exceed 50 to succeed
- RR increases +2 per level

---

## Key Terminology
| Term | Meaning |
|------|---------|
| AM | Absolute Maneuver — pass/fail |
| AT | Armor Type — 1 (none) to 10 (plate) |
| BMR | Base Movement Rate — avg human = 20'/round |
| DB | Defensive Bonus = 3× Quickness bonus |
| DP | Development Points — 60/level; spent on skills/talents |
| Init | Initiative = Quickness bonus |
| OB | Offensive Bonus — added to attack rolls |
| PM | Percentage Maneuver — variable success |
| PP | Power Points = Power Development bonus |
| RR | Resistance Roll — resists spells/poison/disease/fear |
| Round | 5 seconds of in-game time |
| UM | Unmodified Roll — no modifiers applied |
