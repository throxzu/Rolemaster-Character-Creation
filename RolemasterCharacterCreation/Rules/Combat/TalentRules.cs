namespace RolemasterCharacterCreation.Rules;

// Talents & Flaws catalog — Chapter 4, RMU Core Law.
// CostTier1 = DP for tier 1 (or flat cost when MaxTier == 1).
// CostPerTier = additional DP per tier beyond tier 1. Same as CostTier1 for "X/Tier" talents.
// Flaw costs are negative (DP returned).
// TotalCost(tier) = CostTier1 + (tier - 1) * CostPerTier
public static class TalentRules
{
    public record TalentDef(
        string Name,
        string Category,
        bool   IsFlaw,
        int    MaxTier,       // 0 = unlimited; 1 = flat (no tiers); N = max N tiers
        int    CostTier1,     // DP cost (or return) for tier 1
        int    CostPerTier,   // DP cost (or return) per additional tier; 0 if flat
        string Effect,
        bool   NeedsSpec = false  // player must supply a specialization (e.g. element, skill name)
    )
    {
        public int TotalCost(int tier) =>
            MaxTier == 1 || tier <= 1
                ? CostTier1
                : CostTier1 + (tier - 1) * CostPerTier;
    }

    public static readonly IReadOnlyList<TalentDef> All = new TalentDef[]
    {
        // ── Combat ────────────────────────────────────────────────────────────
        new("Fast Attack",            "Combat", false, 0, 10, 10, "+5 Initiative per Tier"),
        new("Deadeye",                "Combat", false, 0,  7,  7, "–10 range penalty per Tier"),
        new("Foiler",                 "Combat", false, 0,  7,  7, "+1 to foe's fumble range per Tier"),
        new("Sharpshooter",           "Combat", false, 0,  5,  5, "+5 OB per round of aiming, max +5/Tier"),
        new("Pressing the Advantage", "Combat", false, 0,  5,  5, "+10 OB per Tier on next attack after scoring a critical"),
        new("Riposte",                "Combat", false, 1, 20,  0, "Immediate counterattack after successful parry"),
        new("Opportunistic Strike",   "Combat", false, 1, 15,  0, "Immediate free attack when foe fumbles"),
        new("Sense Weakness",         "Combat", false, 1, 20,  0, "Reroll one critical after observing foe for one round"),
        new("Quickdraw",              "Combat", false, 1,  7,  0, "0 AP to draw or ready a weapon"),
        new("Slow on the Draw",       "Combat", true,  1, -5,  0, "2 AP to draw when surprised (flaw)"),

        // ── Magical ───────────────────────────────────────────────────────────
        new("Eloquence",              "Magical", false, 0, 15, 15, "+5 to all Spellcasting rolls per Tier"),
        new("Graceful Recovery",      "Magical", false, 0,  8,  8, "Spell Failure rolls reduced by –5/Tier (min 1)"),
        new("Extended Reach",         "Magical", false, 0, 15,  5, "Touch spells cast at 5'/Tier range"),
        new("Scope Skills",           "Magical", false, 0, 20, 20, "Radius or # of targets +100% per Tier"),
        new("Spatial Skills",         "Magical", false, 0, 10, 10, "Spell ranges +50% per Tier"),
        new("Temporal Skills",        "Magical", false, 0, 10, 10, "Spell durations +50% per Tier"),
        new("Quick-Caster",           "Magical", false, 2, 10, 10, "T1: cast costs 2–3 AP; T2: cast costs 2 AP"),
        new("Power Recycling",        "Magical", false, 2, 10, 10, "T1: recover ½ PP on failed spell; T2: recover all PP"),
        new("Subconscious Discipline","Magical", false, 2, 10, 10, "Spells continue 1 (T1) or 2 (T2) rounds after concentration ends"),
        new("Mumbler",                "Magical", true,  0,-10,-10, "–5 per Tier to all Spellcasting rolls (flaw)"),
        new("Inglorious Failure",     "Magical", true,  0, -6, -6, "Spell Failure rolls +5 per Tier (flaw)"),

        // ── Physical ─────────────────────────────────────────────────────────
        new("Ambidextrous",           "Physical", false, 1, 10,  0, "No –20 penalty for off-hand actions"),
        new("Beast of Burden",        "Physical", false, 0, 10, 10, "+10% encumbrance capacity per Tier"),
        new("Slow Bleeder",           "Physical", false, 0, 25, 25, "Bleeding reduced by 1 hit/round per Tier"),
        new("Fast Healer",            "Physical", false, 1, 10,  0, "Recovery times halved"),
        new("Frenzy",                 "Physical", false, 1, 20,  0, "+5 Str bonus; no hit-loss penalties; can fight past 0 hits"),
        new("Rapid Bleeder",          "Physical", true,  0,-25,-25, "Bleeding increased 1 hit/round per Tier (flaw)"),
        new("Slow Healer",            "Physical", true,  1,-10,  0, "Recovery times doubled (flaw)"),
        new("Uncoordinated",          "Physical", true,  2,-10,-10, "Movement penalties treated as one pace higher per Tier (flaw)"),

        // ── Discipline ────────────────────────────────────────────────────────
        new("Light Sleeper",          "Discipline", false, 1, 10,  0, "+25 to Perception to awaken"),
        new("Iron Will",              "Discipline", false, 0,  4,  4, "+5 per Tier to RRs vs. mental spells"),
        new("Look of Eagles",         "Discipline", false, 0, 10, 10, "+10/Tier to Leadership; followers +2/Tier to fear RRs"),
        new("Prodigy",                "Discipline", false, 0,  6,  6, "+5 per Tier to all specializations of one skill", true),
        new("Multilingual",           "Discipline", false, 1,  8,  0, "10 ranks in one language OR 5 ranks in each of two"),
        new("Neutral Odor",           "Discipline", false, 1, 10,  0, "Smell-based Perception rolls against you at –50"),
        new("Distinct Odor",          "Discipline", true,  1,-10,  0, "+50 to tracking/Perception by smell against you (flaw)"),
        new("Heavy Sleeper",          "Discipline", true,  1, -5,  0, "–25 to Perception to awaken (flaw)"),
        new("Inept",                  "Discipline", true,  0, -4, -4, "–5 per Tier to all specializations of one skill (flaw)", true),
        new("Mute",                   "Discipline", true,  1,-10,  0, "Cannot speak (flaw)"),
        new("Math Illiterate",        "Discipline", true,  1,-10,  0, "Cannot do math beyond basic counting (flaw)"),
        new("Revulsion",              "Discipline", true,  1,-15,  0, "Must make Fear RR vs. rate of visible bleeding (flaw)"),
        new("Non-violent",            "Discipline", true,  0, -5, -5, "–20/Tier action penalty after inflicting a critical (flaw)"),
        new("Disturbing Voice",       "Discipline", true,  0,-10,-10, "–20/Tier to Influence, Singing, Leadership, etc. (flaw)"),

        // ── Senses ────────────────────────────────────────────────────────────
        new("Empathy",                "Senses", false, 1, 20,  0, "Read emotions within 25'; +50 to Social Awareness"),
        new("Animal Empathy",         "Senses", false, 0, 15,  5, "+25 to maneuvers with one animal type; empathic communication", true),
        new("Direction Sense",        "Senses", false, 1, 10,  0, "+30 to Navigation"),
        new("Destiny Sense",          "Senses", false, 2, 10, 10, "T1: know direction to goal; T2: direction + distance (1×/day)"),
        new("Visions",                "Senses", false, 0, 30, 10, "Spontaneous visions from touching objects (1×/day/Tier)"),
        new("Acute Hearing",          "Senses", false, 0,  4,  4, "+5 per Tier to hearing-based Perception"),
        new("Acute Smell",            "Senses", false, 0,  3,  3, "+5 per Tier to smell-based Perception"),
        new("Acute Taste",            "Senses", false, 0,  2,  2, "+5 per Tier to taste-based Perception"),
        new("Acute Touch",            "Senses", false, 0,  1,  1, "+5 per Tier to touch-based Perception"),
        new("Acute Vision",           "Senses", false, 0,  5,  5, "+5 per Tier to vision-based Perception"),
        new("Hearing, Cat",           "Senses", false, 1, 10,  0, "+10 on sound-based Perception"),
        new("Hearing, Dog",           "Senses", false, 1, 15,  0, "Hear 4× farther; +10 to Perception"),
        new("Hearing, Hare",          "Senses", false, 1, 20,  0, "Hear 10× farther; +20 to Perception"),
        new("Heatsense",              "Senses", false, 1, 10,  0, "+20 to Perception and Tracking for warm/cold objects"),
        new("Sight, Eagle",           "Senses", false, 1, 15,  0, "+20 to vision-based Perception"),
        new("Sight, Gecko",           "Senses", false, 1, 10,  0, "+10 to vision-based Perception; detail close-up"),
        new("Sight, Hawk",            "Senses", false, 1, 20,  0, "+30 to vision-based Perception; see up to 5 miles"),
        new("Tetrachromatic Vision",  "Senses", false, 1, 15,  0, "UV sensitivity; –30 to spot invisible/camouflaged foes"),
        new("Strike Reflex",          "Senses", false, 1,  5,  0, "+20 Initiative when triggered by sudden movement"),
        new("Poor Hearing",           "Senses", true,  0, -3, -3, "–5 per Tier to hearing-based Perception (flaw)"),
        new("Poor Smell",             "Senses", true,  0, -2, -2, "–5 per Tier to smell-based Perception (flaw)"),
        new("Poor Taste",             "Senses", true,  0, -1, -1, "–5 per Tier to taste-based Perception (flaw)"),
        new("Poor Touch",             "Senses", true,  0, -1, -1, "–5 per Tier to touch-based Perception (flaw)"),
        new("Poor Vision",            "Senses", true,  0, -4, -4, "–5 per Tier to vision-based Perception (flaw)"),
        new("One Eye",                "Senses", true,  1,-10,  0, "–25 to ranged attacks (flaw)"),

        // ── Racial (can occasionally be purchased; always assigned free to races) ──
        new("Darkvision",             "Racial", false, 0, 10,  5, "See 10'/Tier in complete darkness"),
        new("Nightvision",            "Racial", false, 1, 10,  0, "See in dim light; darkness penalties reduced by 40"),
        new("Defensive Aura",         "Racial", false, 0,  6,  6, "+5 DB per Tier (not cumulative with Aura/Blur spells)"),
        new("Efficient Sleeper",      "Racial", false, 2,  5,  5, "T1: 3 hrs = 4 hrs; T2: 2 hrs = 4 hrs"),
        new("Immune to Disease",      "Racial", false, 2, 10, 20, "T1: mundane diseases; T2: all diseases including magical"),
        new("Natural Armor",          "Racial", false, 0,  5,  5, "+1 AT per Tier; no encumbrance/maneuver penalty"),
        new("Natural Weaponry",       "Racial", false, 1,  5,  0, "Natural attack equal to character size"),
        new("Elemental Resistance",   "Racial", false, 0,  4,  4, "+5/Tier to DBs, RRs, and Endurance vs. one element", true),
        new("Elemental Susceptibility","Racial",true,  0, -3, -3, "–5/Tier vs. one element (flaw)", true),
        new("Increased Size",         "Racial", false, 0, 30, 30, "+1 size per Tier"),
        new("Decreased Size",         "Racial", true,  0,-30,-30, "–1 size per Tier (flaw)"),
        new("Fast Healer (Racial)",   "Racial", false, 1, 10,  0, "Recovery times halved; +10 to recovery rolls"),
        new("Tough",                  "Racial", false, 0,  3,  3, "+5 per Tier modifier to base hits"),
        new("Fragile",                "Racial", true,  0, -3, -3, "–5 per Tier modifier to base hits (flaw)"),
        new("Vigorous",               "Racial", false, 0,  5,  5, "+10 per Tier to Endurance rolls"),
        new("Feeble",                 "Racial", true,  0, -5, -5, "–10 per Tier to Endurance rolls (flaw)"),
        new("Light-Boned",            "Racial", true,  0,-15,-15, "Hits and attacks treated as one size smaller per Tier (flaw)"),
        new("Light Sensitivity",      "Racial", true,  0,-10,-10, "–25 per Tier in bright sunlight (flaw)"),
        new("Magical Resistance",     "Racial", false, 0,  3,  3, "+5/Tier to RRs vs. one magical realm", true),
        new("Magical Vulnerability",  "Racial", true,  0, -3, -3, "–5/Tier to RRs vs. one magical realm (flaw)", true),
        new("Physical Resistance",    "Racial", false, 0,  3,  3, "+5/Tier to Physical RRs"),
        new("Physical Vulnerability", "Racial", true,  0, -3, -3, "–5/Tier to Physical RRs (flaw)"),
        new("Poison Injection",       "Racial", false, 1, 20,  0, "Natural attack delivers poison on critical hit"),
        new("Recurved Musculature",   "Racial", false, 1, 30,  0, "3× jump distance; +20 to Acrobatics, Climbing, Jumping, Running"),
        new("Restricted Diet",        "Racial", true,  1, -5,  0, "Diet limited to specific foods; –20 penalty or no sustenance (flaw)"),
        new("Wings, Vestigial",       "Racial", false, 1,  5,  0, "+10 to Jumping and Acrobatics for fall reduction"),
        new("Increased Stride",       "Racial", false, 0,  1,  1, "+2'/round per Tier to movement"),
        new("Decreased Stride",       "Racial", true,  0, -1, -1, "–2'/round per Tier to movement (flaw)"),
        new("Extra Joints",           "Racial", false, 1, 20,  0, "+20 to all activities with that limb"),
        new("Third Eyelid",           "Racial", false, 1,  5,  0, "Unaffected by dust, sand, and water in eyes"),
        new("Perfect Pitch",          "Racial", false, 0,  7,  7, "+10/Tier to Singing and Play Instrument"),
        new("Golden Throat",          "Racial", false, 0,  5,  5, "+10/Tier to Charm, Singing, Leadership, etc."),
        new("Additional Limb Pair",   "Racial", false, 0, 20, 20, "One additional pair of arms per Tier"),
        new("Breath Holding",         "Racial", false, 1, 10,  0, "Hold breath 1 minute per level; doubled if inactive"),
        new("Immune to Disease",      "Racial", false, 2, 10, 20, "T1: mundane; T2: all diseases"),
    };

    public static readonly IReadOnlyDictionary<string, TalentDef> ByName =
        All.GroupBy(t => t.Name).ToDictionary(g => g.Key, g => g.First());

    public static readonly IReadOnlyList<string> Categories =
        All.Select(t => t.Category).Distinct().ToList();

    public static int TierCost(string name, int tier)
    {
        if (!ByName.TryGetValue(name, out var def)) return 0;
        return def.TotalCost(tier);
    }
}
