using System.Security.Cryptography;

namespace RolemasterCharacterCreation.Identity;

// Generates the random first-time password texted to a newly-invited player. It uses only
// digits, upper- and lower-case letters (no symbols) so it's easy to type from an SMS, and
// avoids visually ambiguous characters (O/0, I/l/1). The Identity policy is configured to not
// require a non-alphanumeric character (see Program.cs).
public static class TempPassword
{
    private const string Upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";   // no I, O
    private const string Lower = "abcdefghijkmnpqrstuvwxyz";    // no l, o
    private const string Digit = "23456789";                   // no 0, 1

    public static string Generate(int length = 10)
    {
        if (length < 8) length = 8;

        const string all = Upper + Lower + Digit;

        // Guarantee one of each required class, then fill the rest from the full set.
        var chars = new List<char>
        {
            Pick(Upper), Pick(Lower), Pick(Digit),
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
