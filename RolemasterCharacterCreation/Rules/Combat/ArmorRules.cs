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
    // DB comes from the Shield skill bonus, not the shield type itself.
    // Weight and AT are intrinsic properties of the shield.
    public record ShieldEntry(string Type, int Weight, int AT);

    public static readonly ShieldEntry[] Shields =
    [
        new("None",    0,  1),
        new("Small",   5,  3),
        new("Medium",  8,  5),
        new("Large",  12,  6),
    ];

    // ── Lookup helpers ────────────────────────────────────────────────────────
    // null and "None" are treated identically — no piece worn
    public static PieceDef   Helmet(string? grade)  => Helmets.FirstOrDefault(p => p.Grade == (grade  ?? "None")) ?? Helmets[0];
    public static PieceDef  Vambrace(string? grade) => Vambraces.FirstOrDefault(p => p.Grade == (grade ?? "None")) ?? Vambraces[0];
    public static PieceDef   Greave(string? grade)  => Greaves.FirstOrDefault(p => p.Grade == (grade  ?? "None")) ?? Greaves[0];
    public static ShieldEntry Shield(string? type)  => Shields.FirstOrDefault(s => s.Type  == (type   ?? "None")) ?? Shields[0];
}
