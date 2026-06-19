using System.Text.RegularExpressions;

namespace RolemasterCharacterCreation.Rules;

/// <summary>
/// Resolves the Core Law Chapter 10 attack table a spell uses. The spell
/// description usually names it explicitly ("results determined on the Fire Bolt
/// Table", "Claw Attack Table"); that is authoritative. When the description does
/// not name a table, fall back to inferring an elemental attack from the spell's
/// Type code (Ed = directed bolt, Eb = ball) and element keyword in its name.
/// Returns null for spells with no attack table.
/// </summary>
public static class SpellAttackTable
{
    // Table names as they appear in spell descriptions → the AttackTable name.
    private static readonly (string Phrase, string Table)[] DescriptionRefs =
    {
        ("fire bolt", "Bolt, Fire"),
        ("ice bolt", "Bolt, Ice"),
        ("lightning bolt", "Bolt, Lightning"),
        ("lighting bolt", "Bolt, Lightning"),       // book typo
        ("water bolt", "Bolt, Water"),
        ("fire ball", "Ball, Fire"),
        ("cold ball", "Ball, Cold"),
        ("lightning ball", "Ball, Lightning"),
        ("martial arts striking", "Unarmed Strikes"),
        ("martial arts sweeps", "Unarmed Sweeps"),
        ("claw", "Claw"),
        ("bite", "Bite"),
        ("sling", "Sling"),
        ("grapple", "Grapple"),
        ("horn", "Horn"),
        ("stinger", "Stinger"),
    };

    public static string? For(string? name, string? type, string? description) =>
        FromDescription(description) ?? FromNameType(name, type);

    // "<table> [attack] table" stated in the description is authoritative.
    private static string? FromDescription(string? description)
    {
        if (string.IsNullOrEmpty(description)) return null;
        foreach (var (phrase, table) in DescriptionRefs)
            if (Regex.IsMatch(description, $@"\b{Regex.Escape(phrase)}\s+(attack\s+)?table\b",
                              RegexOptions.IgnoreCase))
                return table;
        return null;
    }

    private static string? FromNameType(string? name, string? type)
    {
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(type) || type[0] != 'E')
            return null;

        var n = name.ToLowerInvariant();
        bool ball = type.Contains('b') || n.Contains("ball");
        bool bolt = type.Contains('d') || n.Contains("bolt");

        string? elem =
            n.Contains("fire") || n.Contains("flame") ? "Fire"
            : n.Contains("ice") || n.Contains("frost") ? "Ice"
            : n.Contains("cold") ? "Cold"
            : n.Contains("lightning") || n.Contains("shock") || n.Contains("spark")
              || n.Contains("electric") ? "Lightning"
            : n.Contains("water") ? "Water"
            : null;
        if (elem is null) return null;

        if (ball)
            return elem switch
            {
                "Fire" => "Ball, Fire",
                "Cold" or "Ice" => "Ball, Cold",
                "Lightning" => "Ball, Lightning",
                _ => null,
            };
        if (bolt)
            return elem switch
            {
                "Fire" => "Bolt, Fire",
                "Ice" or "Cold" => "Bolt, Ice",
                "Lightning" => "Bolt, Lightning",
                "Water" => "Bolt, Water",
                _ => null,
            };
        return null;
    }
}
