namespace RolemasterCharacterCreation.Rules;

/// <summary>
/// Table 2-5b: Stat Gains (Core Law, Section 2.5).
///
/// "Look up the current (temporary) value in Table 2-5b to determine what die is used for
/// the roll. Roll that die type and add the result to the temporary stat. Stats cannot be
/// raised above their potential value."
///
/// Note this is not the RM2/RMSS rule of rolling d100 against the stat for a flat +1 — the
/// die result itself is the gain, so a stat in the 19–81 band can jump by as much as 10.
/// </summary>
public static class StatGainRules
{
    /// <summary>A die with an optional flat modifier, e.g. d3-1 rolls 0–2.</summary>
    public record Die(int Sides, int Modifier)
    {
        public int Min => 1 + Modifier;
        public int Max => Sides + Modifier;

        /// <summary>"d10", "d3-1" — how the table writes it.</summary>
        public string Label => Modifier == 0 ? $"d{Sides}" : $"d{Sides}{Modifier:+#;-#}";
    }

    /// <summary>
    /// The die for a stat at this temporary value. Gains are largest in the wide middle
    /// band and taper off at both extremes.
    /// </summary>
    public static Die DieFor(int temporary) => temporary switch
    {
        <=  6 => new Die(3, -1),   //  1 –  6
        <=  8 => new Die(3,  0),   //  7 –  8
        <= 18 => new Die(6,  0),   //  9 – 18
        <= 81 => new Die(10, 0),   // 19 – 81
        <= 90 => new Die(6,  0),   // 82 – 90
        <= 92 => new Die(3,  0),   // 91 – 92
        _     => new Die(3, -1),   // 93 – 99
    };

    /// <summary>Rolls the die for a stat at this temporary value. May legitimately be 0.</summary>
    public static int Roll(Random rng, int temporary)
    {
        var die = DieFor(temporary);
        return rng.Next(1, die.Sides + 1) + die.Modifier;
    }
}
