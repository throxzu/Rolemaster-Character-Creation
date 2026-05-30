# Maneuvers & Movement

Rolemaster Core Law maneuver and movement data — extracted from Chapter 5.

---

## 5.1 Maneuvers

Maneuvers are resolved by rolling **d100OE** plus modifiers and consulting Table 5-1. Any maneuver requiring movement or precise motions is also penalized by encumbrance penalty (Section 5.4) and armor maneuver penalty (Section 7.2).

### Absolute vs. Percentage Maneuvers

- **Absolute**: All-or-nothing — either succeed or fail entirely (e.g., jumping a chasm, hiding). Five possible results: Absolute Failure, Failure, Unusual Event (unmodified 66), Partial Success, Success, Absolute Success.
- **Percentage**: Graduated success — numeric results are cumulative until 100% is reached (e.g., climbing a cliff, cranking a drawbridge). Failure on a moving maneuver → Unbalancing critical of indicated severity. Results >100% (110–150) mean faster completion; Exceptional = 150 + further advantage.

### Table 5-1: Maneuvers

#### Absolute Maneuvers

| Roll | Result |
|------|--------|
| < 1 | **Absolute Failure** — fail and make situation worse |
| 1 – 75 | **Failure** — task fails; time and resources wasted |
| UM 66 | **Unusual Event** — succeed/fail normally, but unexpected side effect |
| 76 – 100 | **Partial Success** — partially done; treat as Failure if partial success not possible |
| 101 – 175 | **Success** — task accomplished |
| 176+ | **Absolute Success** — task done with additional benefit (faster, fewer resources, inspires bonus) |

#### Percentage Maneuvers

| Roll | Result |
|------|--------|
| −100 or less | E critical |
| −99 to −80 | D critical |
| −79 to −60 | C critical |
| −59 to −40 | B critical |
| −39 to −20 | A critical |
| −19 to 0 | Fail to act |
| 1 – 10 | 5% |
| 11 – 20 | 10% |
| 21 – 30 | 20% |
| 31 – 40 | 30% |
| 41 – 50 | 40% |
| 51 – 60 | 50% |
| 61 – 70 | 60% |
| 71 – 80 | 70% |
| 81 – 90 | 80% |
| 91 – 100 | 90% |
| 101 – 130 | 100% |
| 131 – 160 | 110% |
| 161 – 190 | 120% |
| 191 – 220 | 130% |
| 221 – 250 | 140% |
| 251 – 280 | 150% |
| 281+ | Exceptional (treat as 150% + further advantage) |

### Difficulty Scale

| Difficulty | Modifier |
|------------|----------|
| Casual | +70 |
| Simple | +50 |
| Routine | +30 |
| Easy | +20 |
| Light | +10 |
| Medium | 0 |
| Hard | −10 |
| Very Hard | −20 |
| Extremely Hard | −30 |
| Sheer Folly | −50 |
| Absurd | −70 |
| Nigh Impossible | −100 |

### Lighting Modifiers

| Condition | Required / Helpful |
|-----------|-------------------|
| No shadows | 0 / 0 |
| Light shadows | −10 / −5 |
| Medium shadows | −20 / −10 |
| Heavy shadows | −30 / −15 |
| Dark | −50 / −25 |
| Extremely Dark | −70 / −35 |
| Pitch Black | −100 / −50 |

### Other Modifiers

| Condition | Modifier |
|-----------|----------|
| Pain (every 25% hits lost) | −10 |
| Injuries & Fatigue | varies |
| Encumbrance | varies |
| Maneuvering in Armor | varies |

---

## 5.2 Feats of Strength

Uses **Weight-Training** skill. May be Absolute (no rest point, e.g., picking up a barrel) or Percentage (rest points, e.g., dragging across a floor — roll per 10' moved). Keeping an object lifted requires a maneuver roll every round.

**Load** = item weight as % of character's body weight, **minus 2 × Strength bonus**.

### Table 5-2: Feats of Strength

| Difficulty | Load | Modifier |
|------------|------|----------|
| Casual | 5% | +70 |
| Simple | 10% | +50 |
| Routine | 20% | +30 |
| Easy | 30% | +20 |
| Light | 40% | +10 |
| Medium | 50% | 0 |
| Hard | 75% | −10 |
| Very Hard | 100% | −20 |
| Extremely Hard | 150% | −30 |
| Sheer Folly | 200% | −50 |
| Absurd | 300% | −70 |
| Nigh Impossible | 400% | −100 |

*Load = weight as a percentage of character's body weight, minus 2×St modifier.*

### Mechanical Advantage

- Dragging/pushing (weight resting on ground): halve effective weight.
- Wheels, pulleys, levers: reduce to ⅓–⅕ weight; theoretical maximum 1/100 weight.

Breaking free from a grapple: use Wrestling or Contortions skill (defender at full grapple penalty); conflicting Percentage Maneuver — grapple % reduced by result.

---

## 5.3 Movement

**Base Movement Rate (BMR)**: speed in feet per 5-second round (combat round).

```
BMR = 20' + Quickness/2 + Stride
```

- Stride is determined by race (or height with Individual Stride optional rule).
- Average humanoid BMR = 20'/round.
- Convert: BMR × 12 = ft/min; BMR × 0.14 = mph.

Normal movement requires no roll. Rolls required for stressful movement (rough terrain, stunned, slick ground, etc.).

### Table 5-3: Movement

| Pace | Per Round | Per Phase | Penalty / AP | Load Limit |
|------|-----------|-----------|--------------|------------|
| Creep | ×½ BMR | ×⅛ BMR | — | — |
| Walk | ×1 BMR | ×¼ BMR | −25 or 1 AP | 90% |
| Jog | ×2 BMR | ×½ BMR | −50 or 2 AP | 60% |
| Run | ×3 BMR | ×¾ BMR | −75 or 3 AP | 45% |
| Sprint | ×4 BMR | ×1 BMR | 4 AP | 30% |
| Dash | ×5 BMR | ×1.25 BMR | 4+ AP | 15% |

- **Dash**: requires 4 AP plus your instantaneous action.
- **Backwards movement**: half speed, maximum Jog pace.
- **Prone movement**: max Jog pace at half speed; no other actions while moving.
- **Pace penalty**: applies to all other actions performed while moving (combat, spells, etc.). Does not affect the movement roll itself; terrain difficulty does not affect the pace penalty.
- **Load Limit**: maximum load as % of body weight permitting that pace.

### Example Terrain Modifiers

| Terrain | Modifier |
|---------|----------|
| Easy: Perfectly flat and uniform | +20 |
| Light: Nearly flat, no obstacles | +10 |
| Medium: Mostly flat and open | 0 |
| Hard: Rough and rocky, furnished room | −10 |
| Very Hard: Sloping and rocky, people in the way | −20 |
| Extremely Hard: Modest slopes/rocks, light crowd | −30 |
| Sheer Folly: Numerous obstacles, steep, crowds | −50 |
| Absurd: Dense obstacles, packed crowd | −70 |
| Nigh Impossible: Sheer cliff, tightly packed crowd | −100 |

---

## 5.4 Encumbrance

Encumbrance penalty applies to all maneuvers affected by equipment (movement, physical maneuvers, Endurance rolls) — **not** Combat Training skills (OB and parry).

### Weight Allowance

```
WA (%) = 15% + 2 × Strength Modifier
```

If WA is negative, treat as 0% (carrying anything incurs a penalty).

### Encumbrance Penalty

```
Enc. Penalty = Load (%) – WA (%)
```

Encumbrance Penalty cannot be negative (cannot add to maneuvers).

### During Play (simplified)

Convert WA to pounds:

```
WA (lbs) = WA (%) × Body Weight / 100
```

For small weights, apply penalty in increments:

```
Enc. Penalty = −5 for every (Body Weight / 20) lbs above WA (lbs)
```

*Example: 220 lb character, St modifier +3 → WA = 21% = ~46 lbs. Every 11 lbs (220/20) over 46 lbs adds −5 penalty.*

### Fitted Armor

Armor weights are listed as % of body weight (Table 6-4, Chapter 6) rather than a fixed number, so heavier characters wear heavier armor of the same type.

---

## 5.5 Fatigue

Characters make **Endurance rolls** periodically to avoid fatigue. Endurance roll = Absolute Maneuver using **Body Development** skill bonus + racial Endurance modifier + applicable modifiers.

Fatigue >−50 accumulated: further fatigue penalties become injury penalties instead.

**Recovery**: −1 fatigue penalty per minute of rest. Recovery without food/water only halves the food/water deprivation modifier (never worse than −50).

### Table 5-5: Endurance

| Roll | Result |
|------|--------|
| < 1 | **Absolute Failure** — fatigue penalty +20, suffer 10 hits |
| 1 – 75 | **Failure** — fatigue penalty +10 |
| UM 66 | **Unusual Event** — brief hallucination; may reroll one failed Lore, Science, or Delving roll within 24 hours |
| 76 – 100 | **Partial Success** — fatigue penalty +5 |
| 101 – 175 | **Success** — no additional fatigue |
| 176+ | **Absolute Success** — reduce current fatigue penalty by 10; +5 to next action |

### Endurance Modifiers

| Modifier | Condition |
|----------|-----------|
| varies | Accumulated fatigue penalties |
| varies | Encumbrance and armor maneuver penalties |
| −20 | Every day of no sleep |
| −10 | Every day of half sleep (4 hours) |
| −5 | Every hour after a day of no water |
| −5 | Every 8 hours after 3 days of half water |
| −10 | Every day of no food |
| −10 | Every 3 days of half rations |
| −5 | Heat: every 5°F above acclimatized range |
| −5 | Cold: every 5°F below acclimatized range |
| −10 | Altitude: every 2,500' |

### Suggested Check Intervals

| Interval | Activity |
|----------|----------|
| 2 hours | Walk |
| 5 min. | Jog |
| 1 min. | Run |
| 2 rounds | Sprint |
| 1 round | Dash |
| 6 rounds | Melee combat |
| 6 rounds | Climbing / Swimming |
| 6 rounds | Concentration |

### Altitude

−10 to Endurance rolls per 2,500' altitude (e.g., −10 at 2,501'–5,000'; −20 at 5,001'–7,500'). Fatigue recovery takes twice as long at altitude. Acclimatization: ~2 weeks per 2,500' change.

### Temperature

−5 per 5°F (2.5°C) above or below acclimatized range. Human baseline: 60–80°F (15–27.5°C). Acclimatization: 1 week per 5°F change; human limits 40°F–100°F.

- Cold weather gear (Heavy Cloth AT 2): tolerate 20°F more cold.
- Extreme cold gear (Soft Leather AT 3): tolerate 40°F more cold.
- Desert robes: tolerate 10°F more heat (maximum).

### Concentration

- Combat: Endurance roll every 6 rounds using **Mental Focus** (not Body Development); cumulative −10 per additional 6-round interval.
- Calm (non-combat): roll every minute.
- Meditative trance (successful Meditation maneuver): roll every 5 minutes.
- Base difficulty while concentrating only: Routine (+30).

### Extended Effort

Single roll to determine duration before fatigue penalty incurs:
- Success: continue for 1 check interval + 1 extra per 10 succeeded by (e.g., 101–110 = 1 interval, 111–120 = 2 intervals).
- At end of intervals: fatigue penalty +10; make new roll if continuing.

---

## 5.6 Resistance Rolls

Resistance Rolls (RRs) have no skill modifier — only stats, race, level, and special modifiers (spells, etc.).

### Formula

```
RR = d100OE + Stat + Race + OwnRealm + 2×Target Lvl − 2×Attack Lvl
```

- **OwnRealm**: +10 when target is resisting their own realm of magic.
- Pre-calculate the fixed portion (Stat + Race + OwnRealm + 2×Target Lvl) on the character sheet; subtract 2×Attack Lvl at time of roll.

### Success Threshold

| Situation | Success requires |
|-----------|-----------------|
| vs. spell attack | RR > caster's Spellcasting roll |
| vs. skill-based effect (Poison Mastery, Traps) | RR > opponent's roll − 100 |
| vs. non-caster effects (disease, fear, magical trap) | RR ≥ 50 |

### Table 5-6: Resistance Rolls

| Type | Stat | Failure By | Severity |
|------|------|-----------|----------|
| Physical | Constitution | 100+ | Extreme Failure |
| Fear | Self-Discipline | 51–99 | Severe Failure |
| Channeling | Intuition | 26–50 | Moderate Failure |
| Essence | Empathy | 1–25 | Mild Failure |
| Mentalism | Presence | — | — |

*Failure by = how much the RR result falls short of the success threshold.*
