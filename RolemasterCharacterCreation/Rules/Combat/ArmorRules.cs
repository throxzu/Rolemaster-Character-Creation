namespace RolemasterCharacterCreation.Rules;

// Armor data from Table 6-4 (Section 7.2, RMU Core Law)
public static class ArmorRules
{
    // ── Torso armor ───────────────────────────────────────────────────────────
    // Table 6-4's columns run: AT | Name | Cost | Prod days | Enc (%) | Weight (lbs) |
    // Strength | Maneuver | Ranged | Perception. Cost and production time are not modelled.
    // Ranged and Perception belong to the suit's vambraces and helm respectively (7.2), so
    // they are carried on the full suit only; a piecemeal wearer gets them from the pieces.
    public record ArmorDef(
        int    AT,
        string Name,
        // Full suit (torso + head + arms + legs)
        int    FullWeight,       // lb
        int    FullManPen,       // negative
        int    FullRangedPen,    // negative — the suit's vambraces
        int    FullPercPen,      // negative — the suit's helm
        double FullEncPct,       // fraction of body weight
        int    FullMinStrength,
        // Torso piece only
        int    TorsoWeight,
        int    TorsoManPen,
        double TorsoEncPct,
        int    TorsoMinStrength
    );

    //                          ── full suit ──────────────────────────  ── torso ─────────────
    //                          wt   man  rng  per   enc    st           wt   man   enc    st
    public static readonly IReadOnlyList<ArmorDef> All = new ArmorDef[]
    {
        new( 1, "No Armor",       0,    0,   0,   0, 0.00,   0,           0,   0,  0.00,   0),
        new( 2, "Heavy Cloth",   11,  -10,  -5,  -5, 0.06,  30,           4,  -5,  0.02,  30),
        new( 3, "Soft Leather",  13,  -15,  -5,  -5, 0.07,  30,           6, -10,  0.03,  30),
        new( 4, "Hide Scale",    20,  -35, -20, -10, 0.11,  35,           7, -10,  0.04,  35),
        new( 5, "Laminar",       22,  -45, -20, -10, 0.12,  40,           9, -20,  0.05,  40),
        new( 6, "Rigid Leather", 26,  -50, -20, -10, 0.14,  45,          13, -25,  0.07,  45),
        new( 7, "Metal Scale",   35,  -70, -30, -15, 0.19,  50,          17, -25,  0.09,  50),
        new( 8, "Mail",          39,  -80, -30, -15, 0.21,  55,          20, -35,  0.11,  55),
        new( 9, "Brigandine",    43,  -90, -30, -15, 0.23,  70,          24, -45,  0.13,  70),
        new(10, "Plate",         46, -100, -30, -15, 0.25,  75,          28, -55,  0.15,  75),
    };

    public static readonly IReadOnlyDictionary<int, ArmorDef> ByAT =
        All.ToDictionary(a => a.AT);

    // ── Piecemeal pieces ──────────────────────────────────────────────────────
    // Section 7.2: every piece carries a maneuver penalty; vambraces additionally penalise
    // ranged attacks, and helms additionally penalise perception. Greaves have neither, so
    // those fields stay 0 — which is also why each piece gets its own named column here.
    // Cross-check: for every AT, torso + helm + vambraces + greaves maneuver penalties sum
    // to exactly the full suit's (e.g. Plate: -55 -10 -15 -20 = -100).
    public record PieceDef(string Grade, int AT, int Weight, int ManPen,
                           int RangedPen, int PercPen, double EncPct, int MinStrength);

    //                     at  wt  man  rng  per   enc    st
    public static readonly PieceDef[] Helmets =
    [
        new("None",   1,  0,   0,   0,   0, 0.00,  0),
        new("Light",  3,  2,   0,   0,  -5, 0.01, 35),
        new("Medium", 5,  4,  -5,   0, -10, 0.02, 55),
        new("Heavy",  9,  6, -10,   0, -15, 0.03, 75),
    ];

    public static readonly PieceDef[] Vambraces =
    [
        new("None",   1,  0,   0,   0,   0, 0.00,  0),
        new("Light",  3,  2,   0,  -5,   0, 0.01, 35),
        new("Medium", 5,  4, -10, -20,   0, 0.02, 55),
        new("Heavy",  9,  6, -15, -30,   0, 0.03, 75),
    ];

    public static readonly PieceDef[] Greaves =
    [
        new("None",   1,  0,   0,   0,   0, 0.00,  0),
        new("Light",  3,  4,  -5,   0,   0, 0.02, 35),
        new("Medium", 5,  6, -10,   0,   0, 0.03, 55),
        new("Heavy",  9,  7, -20,   0,   0, 0.04, 75),
    ];

    // ── Shields ───────────────────────────────────────────────────────────────
    // Table 9-6 gives the base DB and how many attacks a shield may be raised against;
    // Table 6-4 gives its weight and the Strength needed to bear it. A shield has no AT
    // of its own — it is interposed, not worn.
    public record ShieldEntry(string Type, int Db, int Weight, int MinStrength, int MaxAttacks)
    {
        // "None" is the only row without a DB, so this doubles as "is a shield carried".
        public bool IsWorn => Db > 0;
    }

    public static readonly ShieldEntry[] Shields =
    [
        new("None",    0,  0,  0, 0),
        new("Target", 15,  4, 50, 1),
        new("Normal", 20,  8, 55, 2),
        new("Full",   25, 12, 60, 3),
        new("Wall",   30, 20, 65, 4),
    ];

    /// <summary>Section 9.6: "a maximum of +50 DB due to shield (including the base shield DB)".</summary>
    public const int PassiveShieldDbCap = 50;

    /// <summary>
    /// Every shield number the sheet shows, derived in one place. <paramref name="shieldSkillBonus"/>
    /// is the Shield skill's ordinary total bonus — the figure the skills table prints — which
    /// Section 9.6 uses for partial and full blocking, while passive blocking uses raw ranks.
    /// </summary>
    public record ShieldDefense(ShieldEntry Shield, int Ranks, int SkillBonus,
                                int PassiveTotal, int PartialTotal, int FullTotal,
                                bool PassiveCapped)
    {
        public bool IsWorn => Shield.IsWorn;
        public int  BaseDb => Shield.Db;
    }

    public static ShieldDefense ShieldDb(string? type, int shieldRanks, int shieldSkillBonus)
    {
        var sh = Shield(type);
        if (!sh.IsWorn) return new(sh, 0, 0, 0, 0, 0, false);

        int ranks = Math.Max(0, shieldRanks);
        // A clumsy bearer is no worse off than one who never raises the shield, so a
        // negative skill bonus never eats into the shield's own DB.
        int skill = Math.Max(0, shieldSkillBonus);
        int passiveRaw = sh.Db + ranks;

        return new(sh, ranks, skill,
                   Math.Min(PassiveShieldDbCap, passiveRaw),  // the cap applies here and nowhere else
                   sh.Db + skill / 2,                          // Table 9-5: half the shield skill
                   sh.Db + skill,                              // Table 9-5: the shield skill
                   passiveRaw > PassiveShieldDbCap);
    }

    // ── Lookup helpers ────────────────────────────────────────────────────────
    // null and "None" are treated identically — no piece worn
    public static PieceDef   Helmet(string? grade)  => Helmets.FirstOrDefault(p => p.Grade == (grade  ?? "None")) ?? Helmets[0];
    public static PieceDef  Vambrace(string? grade) => Vambraces.FirstOrDefault(p => p.Grade == (grade ?? "None")) ?? Vambraces[0];
    public static PieceDef   Greave(string? grade)  => Greaves.FirstOrDefault(p => p.Grade == (grade  ?? "None")) ?? Greaves[0];
    public static ShieldEntry Shield(string? type)  => Shields.FirstOrDefault(s => s.Type  == Canonical(type)) ?? Shields[0];

    // The shield types were renamed to the book's own (Table 9-6) by the RenameShieldTypesToRmu
    // migration. This keeps the old names readable in case the code meets a database the
    // migration has not reached, where they would otherwise fall through to "None" and a
    // character's shield would silently vanish.
    private static string Canonical(string? type) => type switch
    {
        null or "" or "None" => "None",
        "Small"  => "Target",
        "Medium" => "Normal",
        "Large"  => "Full",
        _        => type,
    };
}
