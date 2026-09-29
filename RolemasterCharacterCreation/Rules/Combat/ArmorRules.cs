namespace RolemasterCharacterCreation.Rules;

// Armor data from Table 6-4 (Section 7.2, RMU Core Law)
public static class ArmorRules
{
    // ── Torso armor ───────────────────────────────────────────────────────────
    public record ArmorDef(
        int    AT,
        string Name,
        // Full suit (torso + head + arms + legs)
        int    FullWeight,   // lb
        int    FullManPen,   // negative
        double FullEncPct,   // fraction of body weight
        // Torso piece only
        int    TorsoWeight,
        int    TorsoManPen,
        double TorsoEncPct
    );

    public static readonly IReadOnlyList<ArmorDef> All = new ArmorDef[]
    {
        new( 1, "No Armor",       0,   0,  0.00,   0,   0,  0.00),
        new( 2, "Heavy Cloth",   10, -11,  0.06,   2,  -4,  0.02),
        new( 3, "Soft Leather",  10, -13,  0.07,   2,  -6,  0.03),
        new( 4, "Hide Scale",    15, -20,  0.11,   7,  -7,  0.04),
        new( 5, "Laminar",       25, -22,  0.12,  10,  -9,  0.05),
        new( 6, "Rigid Leather", 19, -26,  0.14,   4, -13,  0.07),
        new( 7, "Metal Scale",   29, -35,  0.19,  14, -17,  0.09),
        new( 8, "Mail",          58, -39,  0.21,  35, -20,  0.11),
        new( 9, "Brigandine",    37, -43,  0.23,  14, -24,  0.13),
        new(10, "Plate",         44, -46,  0.25,  21, -28,  0.15),
    };

    public static readonly IReadOnlyDictionary<int, ArmorDef> ByAT =
        All.ToDictionary(a => a.AT);

    // ── Piecemeal pieces ──────────────────────────────────────────────────────
    // ManPen for helmet = perception penalty; for vambraces = ranged penalty;
    // for greaves = movement penalty added on top of torso-only piece.
    public record PieceDef(string Grade, int AT, int Weight, int ManPen, double EncPct);

    public static readonly PieceDef[] Helmets =
    [
        new("None",   1,  0,  0, 0.00),
        new("Light",  3,  4, -2, 0.01),
        new("Medium", 5,  5, -4, 0.02),
        new("Heavy",  9,  7, -6, 0.03),
    ];

    public static readonly PieceDef[] Vambraces =
    [
        new("None",   1,  0,  0, 0.00),
        new("Light",  3,  2, -2, 0.01),
        new("Medium", 5,  5, -4, 0.02),
        new("Heavy",  9,  8, -6, 0.03),
    ];

    public static readonly PieceDef[] Greaves =
    [
        new("None",   1,  0,  0, 0.00),
        new("Light",  3,  2, -4, 0.02),
        new("Medium", 5,  5, -6, 0.03),
        new("Heavy",  9,  8, -7, 0.04),
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
