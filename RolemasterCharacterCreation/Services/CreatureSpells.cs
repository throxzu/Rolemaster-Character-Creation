using System.Text.RegularExpressions;

namespace RolemasterCharacterCreation.Services;

/// <summary>A single spell list a creature can use, e.g. "Ice Law [Own lvl, SCR: 31]".</summary>
public sealed class CreatureSpellRef
{
    public string  Name  { get; init; } = "";
    public string  Meta  { get; init; } = "";   // bracket contents, e.g. "Own lvl, SCR: 31"
    public string? Range { get; init; }          // percentile range for "X% will know…" forms, e.g. "01-30"
}

/// <summary>A run of spell lists sharing a realm/category label, e.g. "(Closed, Essence)".</summary>
public sealed class CreatureSpellGroup
{
    public string? Label { get; init; }          // e.g. "Closed, Essence"; null when the source gives none
    public List<CreatureSpellRef> Lists { get; } = [];
}

public sealed class CreatureSpellInfo
{
    public string? Note { get; init; }            // leading prose, e.g. "10% will know one of the following…"
    public IReadOnlyList<CreatureSpellGroup> Groups { get; init; } = [];
    public bool HasContent => !string.IsNullOrWhiteSpace(Note) || Groups.Count > 0;
}

/// <summary>
/// Turns the flat <see cref="CreatureEntry.Spells"/> text parsed from Creature Law
/// into grouped, linkable spell-list references. The source lists each list as
/// "Name [Own lvl, SCR: NN]" separated by ";", with an occasional trailing
/// "(Category, Realm)" label that applies to the preceding run of lists.
/// </summary>
public static class CreatureSpells
{
    private static readonly Regex Entry = new(
        @"(?:\((?<range>\d{1,2}-\d{2,3})\)\s*)?"  // optional "(01-30)" percentile range
        + @"(?<name>[A-Za-z][A-Za-z'’ \-/]*?)"     // spell-list name
        + @"\s*\[(?<meta>[^\]]*)\]"                 // "[Own lvl, SCR: 31]"
        + @"(?:\s*\((?<label>[^)]*)\))?",           // optional "(Closed, Essence)" label
        RegexOptions.Compiled);

    public static CreatureSpellInfo Parse(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return new CreatureSpellInfo();

        var matches = Entry.Matches(raw);
        if (matches.Count == 0)
            return new CreatureSpellInfo { Note = Tidy(raw) };

        // Leading prose before the first list (e.g. the "X% will know…" preamble).
        var note = matches[0].Index > 0 ? Tidy(raw[..matches[0].Index]) : null;

        var refs   = new List<CreatureSpellRef>(matches.Count);
        var labels = new string?[matches.Count];
        for (var i = 0; i < matches.Count; i++)
        {
            var m = matches[i];
            refs.Add(new CreatureSpellRef
            {
                Name  = m.Groups["name"].Value.Trim(),
                Meta  = m.Groups["meta"].Value.Trim(),
                Range = m.Groups["range"].Success ? m.Groups["range"].Value : null,
            });
            labels[i] = m.Groups["label"].Success ? m.Groups["label"].Value.Trim() : null;
        }

        // A "(Category, Realm)" label applies to its own list and to every
        // preceding unlabelled list back to the last labelled one.
        var resolved = new string?[refs.Count];
        var runStart = 0;
        for (var i = 0; i < refs.Count; i++)
        {
            if (labels[i] is { Length: > 0 } lbl)
            {
                for (var j = runStart; j <= i; j++) resolved[j] = lbl;
                runStart = i + 1;
            }
        }

        // Coalesce consecutive lists with the same resolved label into one group.
        var groups = new List<CreatureSpellGroup>();
        CreatureSpellGroup? cur = null;
        for (var i = 0; i < refs.Count; i++)
        {
            if (cur is null || cur.Label != resolved[i])
            {
                cur = new CreatureSpellGroup { Label = resolved[i] };
                groups.Add(cur);
            }
            cur.Lists.Add(refs[i]);
        }

        return new CreatureSpellInfo { Note = note, Groups = groups };
    }

    // Trims a prose fragment and drops it if nothing readable remains.
    private static string? Tidy(string s)
    {
        s = Regex.Replace(s, @"\s+", " ").Trim();
        // Drop a trailing genre/section header that introduces the lists that follow,
        // e.g. "… SCR: 75 Earth genré Spells:".
        s = Regex.Replace(s, @"\s*\b[\w’']+(?:\s+genré)?\s+Spells:\s*$", "",
                          RegexOptions.IgnoreCase).Trim();
        s = s.TrimEnd(';', ',');
        return s.Any(char.IsLetter) ? s : null;
    }
}
