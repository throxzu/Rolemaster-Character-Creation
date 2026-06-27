using System.Text;
using System.Text.RegularExpressions;

namespace RolemasterCharacterCreation.Services;

/// <summary>
/// Normalises outgoing SMS text so "fancy" formatting can't trip up the provider:
/// strips URLs/links, transliterates typographic characters (smart quotes, dashes, ellipsis)
/// to plain equivalents, drops anything not representable in the GSM&nbsp;03.38 alphabet
/// (emoji, control chars, …) — while keeping Danish æ/ø/å, which are valid GSM-7 characters —
/// and collapses the leftover whitespace. Applied centrally in the SMS sender so it covers
/// every message sent from the solution.
/// </summary>
public static partial class SmsText
{
    // Characters representable in the GSM 03.38 default alphabet (incl. the basic extension set).
    private static readonly HashSet<char> Gsm7 = BuildGsm7();

    public static string Sanitize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return "";

        var s = input;

        // 1) Remove links entirely (these are the usual cause of rejected/blocked sends).
        s = LinkRegex().Replace(s, "");

        // 2) Transliterate common typographic characters to plain ASCII.
        s = s
            .Replace('‘', '\'').Replace('’', '\'').Replace('‚', '\'').Replace('‛', '\'')
            .Replace('“', '"').Replace('”', '"').Replace('„', '"').Replace('‟', '"')
            .Replace('–', '-').Replace('—', '-').Replace('‒', '-').Replace('―', '-').Replace('−', '-')
            .Replace('•', '-')
            .Replace(' ', ' ').Replace(' ', ' ').Replace(' ', ' ').Replace('\t', ' ')
            .Replace("…", "...");

        // 3) Drop anything still not representable in GSM-7 (emoji, control chars, etc.).
        var sb = new StringBuilder(s.Length);
        foreach (var ch in s)
            if (ch == '\n' || ch == '\r' || Gsm7.Contains(ch))
                sb.Append(ch);

        // 4) Collapse the whitespace left behind (e.g. where a URL was removed).
        var cleaned = HorizontalWhitespaceRegex().Replace(sb.ToString(), " ");
        cleaned = NewlinePaddingRegex().Replace(cleaned, "\n");
        return cleaned.Trim();
    }

    private static HashSet<char> BuildGsm7()
    {
        var set = new HashSet<char>();
        for (char c = 'A'; c <= 'Z'; c++) set.Add(c);
        for (char c = 'a'; c <= 'z'; c++) set.Add(c);
        for (char c = '0'; c <= '9'; c++) set.Add(c);
        // Punctuation, symbols and national characters of the GSM 03.38 alphabet (+ extension).
        foreach (var c in "@£$¥èéùìòÇØøÅåΔ_ΦΓΛΩΠΨΣΘΞÆæßÉ !\"#¤%&'()*+,-./:;<=>?¡ÄÖÑÜ§¿äöñüà^{}\\[~]|€")
            set.Add(c);
        return set;
    }

    [GeneratedRegex(@"\b(?:https?://|www\.)\S+", RegexOptions.IgnoreCase)]
    private static partial Regex LinkRegex();

    [GeneratedRegex(@"[^\S\n]+")]
    private static partial Regex HorizontalWhitespaceRegex();

    [GeneratedRegex(@" ?\n ?")]
    private static partial Regex NewlinePaddingRegex();
}
