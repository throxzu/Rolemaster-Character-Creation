using RolemasterCharacterCreation.Models;

namespace RolemasterCharacterCreation.Rules;

// The default decimal coinage (Core Law Table 6-0a, repeated as Treasure Law Table 2-1):
//   1 gold piece (gp) = 10 silver pieces
//   1 silver piece (sp) = 10 bronze pieces
//   1 bronze piece (bp) = 10 copper pieces
//   1 copper piece (cp) = the lowest value coin
// Prices in both books are quoted in silver, so silver is the unit everything converts to.
public static class CoinRules
{
    public record Coin(string Abbr, string Name, decimal Silver);

    // Highest value first, which is also the order a purse is written in.
    public static readonly IReadOnlyList<Coin> All =
    [
        new("gp", "Gold",   10m),
        new("sp", "Silver",  1m),
        new("bp", "Bronze",  0.1m),
        new("cp", "Copper",  0.01m),
    ];

    /// <summary>The purse expressed in silver pieces, the unit equipment prices use.</summary>
    public static decimal TotalSilver(Character c) =>
        c.Gold * 10m + c.Silver + c.Bronze * 0.1m + c.Copper * 0.01m;

    /// <summary>The silver total as the rules write it — "145.07", a dot decimal on any locale.</summary>
    public static string TotalSilverText(Character c) =>
        TotalSilver(c).ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);

    /// <summary>"12 gp, 4 sp, 3 cp" — empty denominations omitted, "—" when penniless.</summary>
    public static string Describe(Character c)
    {
        var parts = new List<string>(4);
        if (c.Gold   != 0) parts.Add($"{c.Gold} gp");
        if (c.Silver != 0) parts.Add($"{c.Silver} sp");
        if (c.Bronze != 0) parts.Add($"{c.Bronze} bp");
        if (c.Copper != 0) parts.Add($"{c.Copper} cp");
        return parts.Count == 0 ? "—" : string.Join(", ", parts);
    }
}
