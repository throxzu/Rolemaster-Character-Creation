namespace RolemasterCharacterCreation.Rules;

/// <summary>
/// Table 15.1.2: Stat Gain Table, from the older Rolemaster system. Offered as a campaign
/// house rule in place of RMU's Table 2-5b (see <see cref="StatGainRules"/>).
///
/// The two work very differently. Table 2-5b picks a die from the stat's current temporary
/// value and adds whatever it rolls. This one always rolls d100, and reads the gain off a
/// grid whose column is the <em>room left</em> in the stat — the difference between Potential
/// and Temporary. A stat close to its potential creeps up by one at best; a stat 15 or more
/// below it can jump by as much as 15. No column can ever overshoot: the largest gain in each
/// column equals the smallest difference that column covers.
/// </summary>
public static class OldStatGainRules
{
    /// <summary>Every stat is rolled on d100, whatever its value.</summary>
    public const int DieSides = 100;

    /// <summary>
    /// The smallest Potential − Temporary difference each column covers, in column order:
    /// 1, 2, 3, 4-5, 6-7, 8-9, 10-11, 12-14, 15+. A difference at or above the last entry
    /// uses the last column.
    /// </summary>
    static readonly int[] ColumnFloor = [1, 2, 3, 4, 6, 8, 10, 12, 15];

    static readonly string[] ColumnLabels = ["1", "2", "3", "4-5", "6-7", "8-9", "10-11", "12-14", "15+"];

    /// <summary>The inclusive upper bound of each d100 row, in row order.</summary>
    static readonly int[] RowCeiling =
        [4, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80, 85, 90, 95, 99, 100];

    /// <summary>
    /// The grid, one row of nine columns per d100 band. Row 0 is the 01-04 row, printed as
    /// asterisks in the book; this campaign reads it as no change, exactly like a 0.
    /// </summary>
    static readonly int[][] Gain =
    [
        //        1   2   3  4-5 6-7 8-9 10-11 12-14 15+
        /* 01-04 */ [0,  0,  0,  0,  0,  0,  0,  0,  0],
        /* 05-10 */ [0,  0,  0,  0,  0,  0,  1,  1,  1],
        /* 11-15 */ [0,  0,  0,  0,  0,  1,  1,  2,  2],
        /* 16-20 */ [0,  0,  0,  0,  1,  1,  2,  3,  3],
        /* 21-25 */ [0,  0,  0,  1,  1,  2,  2,  4,  4],
        /* 26-30 */ [0,  0,  1,  1,  2,  2,  3,  5,  5],
        /* 31-35 */ [0,  1,  1,  1,  2,  2,  3,  5,  6],
        /* 36-40 */ [0,  1,  1,  1,  2,  3,  4,  6,  7],
        /* 41-45 */ [0,  1,  1,  2,  3,  3,  4,  6,  8],
        /* 46-50 */ [0,  1,  1,  2,  3,  3,  5,  7,  9],
        /* 51-55 */ [1,  1,  2,  2,  3,  4,  5,  7, 10],
        /* 56-60 */ [1,  1,  2,  2,  4,  4,  6,  8, 11],
        /* 61-65 */ [1,  1,  2,  3,  4,  4,  6,  8, 11],
        /* 66-70 */ [1,  2,  2,  3,  4,  5,  7,  9, 12],
        /* 71-75 */ [1,  2,  2,  3,  5,  5,  7,  9, 12],
        /* 76-80 */ [1,  2,  3,  3,  5,  6,  8, 10, 13],
        /* 81-85 */ [1,  2,  3,  4,  5,  6,  8, 10, 13],
        /* 86-90 */ [1,  2,  3,  4,  6,  7,  9, 11, 14],
        /* 91-95 */ [1,  2,  3,  4,  6,  7,  9, 11, 14],
        /* 96-99 */ [1,  2,  3,  4,  6,  8, 10, 12, 15],
        /* 100   */ [1,  2,  3,  4,  6,  8, 10, 12, 15],
    ];

    /// <summary>The 01-04 row, which the book prints as asterisks rather than numbers.</summary>
    public static bool IsAsteriskRoll(int roll) => roll >= 1 && roll <= RowCeiling[0];

    /// <summary>
    /// Index of the column covering this difference, or −1 when the stat is already at (or
    /// past) its potential and the table has nothing to offer.
    /// </summary>
    static int ColumnIndex(int difference)
    {
        if (difference < ColumnFloor[0]) return -1;
        for (int i = ColumnFloor.Length - 1; i >= 0; i--)
            if (difference >= ColumnFloor[i]) return i;
        return -1;
    }

    /// <summary>The column heading for this difference, e.g. "6-7", or "—" at potential.</summary>
    public static string ColumnLabel(int difference)
    {
        int i = ColumnIndex(difference);
        return i < 0 ? "—" : ColumnLabels[i];
    }

    /// <summary>
    /// The gain for a d100 roll at this Potential − Temporary difference. A stat at its
    /// potential gains nothing, and a roll outside 1–100 is clamped into range rather than
    /// throwing — the caller validates player input before it gets here.
    /// </summary>
    public static int GainFor(int roll, int difference)
    {
        int col = ColumnIndex(difference);
        if (col < 0) return 0;

        roll = Math.Clamp(roll, 1, DieSides);
        for (int row = 0; row < RowCeiling.Length; row++)
            if (roll <= RowCeiling[row]) return Gain[row][col];

        return Gain[^1][col];
    }

    /// <summary>Room left in the stat, which is the column the roll is read against.</summary>
    public static int Difference(int temporary, int potential) => potential - temporary;

    /// <summary>Whether this stat has any room left to gain.</summary>
    public static bool CanGain(int temporary, int potential) => Difference(temporary, potential) >= ColumnFloor[0];
}
