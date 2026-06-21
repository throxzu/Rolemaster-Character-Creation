using System.Security.Cryptography;

namespace RolemasterCharacterCreation.Identity;

// Generates the random first-time password texted to a newly-invited player. It satisfies
// Identity's default policy (length >= 8, with upper, lower, digit and a symbol) and avoids
// visually ambiguous characters (O/0, I/l/1) since the player types it from an SMS.
public static class TempPassword
{
    private const string Upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";   // no I, O
    private const string Lower = "abcdefghijkmnpqrstuvwxyz";    // no l, o
    private const string Digit = "23456789";                   // no 0, 1
    private const string Symbol = "!@#$%*?";

    public static string Generate(int length = 10)
    {
        if (length < 8) length = 8;

        const string all = Upper + Lower + Digit + Symbol;

        // Guarantee one of each required class, then fill the rest from the full set.
        var chars = new List<char>
        {
            Pick(Upper), Pick(Lower), Pick(Digit), Pick(Symbol),
        };
        while (chars.Count < length)
            chars.Add(Pick(all));

        // Shuffle so the guaranteed characters aren't always in the same positions.
        for (int i = chars.Count - 1; i > 0; i--)
        {
            int j = RandomNumberGenerator.GetInt32(i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }

        return new string(chars.ToArray());
    }

    private static char Pick(string set) => set[RandomNumberGenerator.GetInt32(set.Length)];
}
