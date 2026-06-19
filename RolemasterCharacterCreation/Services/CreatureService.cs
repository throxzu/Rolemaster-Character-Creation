using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;

namespace RolemasterCharacterCreation.Services;

public sealed class CreatureService
{
    private static readonly HashSet<string> Noise =
        new(StringComparer.OrdinalIgnoreCase)
        { "ROLEMASTER", "UNIFIED", "Creature Law", "Rolemaster" };

    private static readonly Regex ChapterNum    = new(@"^\d+\.", RegexOptions.Compiled);
    private static readonly Regex DpMarker      = new(@"\(\d+\s*DP\)", RegexOptions.Compiled);
    private static readonly Regex TableHeading  = new(@"^Table\s+\d+\.\d", RegexOptions.Compiled);
    private static readonly Regex SectionHead   = new(@"^\d+\.\d*\.?\s", RegexOptions.Compiled);

    private static readonly char[] EdgePunct =
        ['.', ',', ';', ':', '(', ')', '[', ']', '"', '\'', '’', '“', '”', '—', '-', '>', '<', '#'];

    // Substrings that mark a line as belonging to the summary stat/skill tables
    // (movement codes, column headers, abbreviated skill labels). These tables sit
    // between the last creature of a section and the first of the next; the real
    // per-creature prose always follows them, so hitting one of these resets the
    // pending-description buffer and discards the table garbage.
    private static readonly string[] TableMarkers =
    [
        "Magical Stats", "Resist Rolls", "Encounter Stats", "Movement Stats",
        "Combat Statistics", "(Dash)", "(Jog)", "(Sprint)", "Walk.Quad", "Walk:",
        "Wings.", "Climb.Claw", "Climb.Cling", "Climb.Snake", "Climb.:",
        "Swim.I", "Swim.:", "Run.:", "Surv. Biome", "Adr. Foc", "Adr. Spe",
        "Wei. Trn", "Soc. Aware", "Rev. Str", "Restr. Quar", "Music. Spec",
        "Lor. Hist", "Comp. Illusion", "Men. Foc", "Percept.:", "Track.:",
        "Subt.Amb", "Subt. Amb", "Stalking:", "Footwork:", "riding check",
        "control checks", "Misc.:",
    ];

    // Marks the start of a trailing summary table that bled into a description.
    private static readonly Regex TableStart = new(
        @"(Table\s+\d+\.\d|#{3,}\s*Movement Stats|#{3,}\s*Combat Statistics|Magical Stats/Resist Rolls Skills)",
        RegexOptions.Compiled);

    private static readonly Regex DpCount = new(@"\(-?\d+\s*DP\)", RegexOptions.Compiled);

    // Spell-list / stat-note / talent fragments that leak in from the table region.
    private static readonly Regex JunkBits = new(
        @"\b\d+%\s+will know[^.]*?half-their\b"      // "5% will know 1 of the following spell lists to half-their"
        + @"|Distinct odor \(-?\d+ DP\)"             // stray talent cost
        + @"|Note:\s*shift bonus values[^.]*?realm\.?"
        + @"|\[\d*(?:st|nd|rd|th)?,?\s*SCR:?\s*\d+\]", // "[SCR: 19]", "[5th, SCR: 15]"
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public IReadOnlyList<CreatureEntry> All { get; }
    public IReadOnlyList<string> CategoryGroups { get; }

    public CreatureService(IWebHostEnvironment env)
    {
        var path = Path.GetFullPath(
            Path.Combine(env.ContentRootPath, "../docs/rules/Rolemaster_Creature_Law_I.md"));

        All = File.Exists(path) ? Parse(path) : [];
        CategoryGroups = All.Select(c => c.CategoryGroup).Distinct().Order().ToList();
    }

    // ── Parser ────────────────────────────────────────────────────────────────
    //
    // Document structure (PDF-to-MD artefacts):
    //
    //   [pre-name description]       → pendingDesc → becomes NEW creature's description
    //   NameName                     → doubled name (creature or chapter)
    //   ```Category: X```            → stat fields (inside code fences)
    //   Stat Bonuses:\nAG CO...\n... → plain-text stat bonuses
    //   Talents\Flaws:\n```...```    → talent text (inside code fences)
    //   [post-talent description]    → pre-noise: appended to CURRENT creature's desc
    //   [page-break noise]           → divider (ROLEMASTER / UNIFIED / page-number)
    //   [next-creature pre-name]     → pendingDesc for the NEXT creature
    //
    // Noise is used as the divider between post-talent desc (current creature)
    // and pre-name desc (next creature).

    private static IReadOnlyList<CreatureEntry> Parse(string path)
    {
        var lines = File.ReadAllLines(path, Encoding.UTF8);
        var results = new List<CreatureEntry>();

        // Text that accumulates between creatures; passed to the NEXT creature as its description.
        var pendingDesc = new StringBuilder();

        // Collects everything after "Talents\Flaws:" up to the first noise line.
        // Post-talent prose (after last DP marker) stays with the CURRENT creature.
        var talentSectionBuf = new StringBuilder();

        // ── Per-creature state ────────────────────────────────────────────────
        string curGroup = "Unknown", curName = "", curCategory = "",
               curArchetype = "", curSize = "", curArmor = "",
               curTreasure = "", curRealm = "", curMisc = "";
        var curStats      = new int[10];
        var curDescBuf    = new StringBuilder();
        var curTalentsBuf = new StringBuilder();

        bool inFence             = false;
        bool statStarted         = false;
        bool waitStatHeader      = false;
        bool waitStatValues      = false;
        bool collectingTalents   = false;  // after Talents label, before first noise
        bool talentNoiseHit      = false;  // noise seen → switch to pendingDesc

        // ── Helpers ───────────────────────────────────────────────────────────

        void Emit()
        {
            if (string.IsNullOrWhiteSpace(curName) || string.IsNullOrWhiteSpace(curCategory)) return;
            results.Add(new CreatureEntry
            {
                Name          = curName,
                CategoryGroup = curGroup,
                Category      = curCategory,
                Archetype     = curArchetype,
                Size          = curSize,
                Armor         = curArmor,
                Treasure      = curTreasure,
                Realm         = curRealm,
                Misc          = curMisc,
                StatBonuses   = (int[])curStats.Clone(),
                TalentsFlaws  = CleanTalents(curTalentsBuf.ToString()),
                Description   = Polish(CleanText(curDescBuf.ToString())),
            });
        }

        // Splits the talent-section buffer at the last (XX DP) marker.
        // Talents  = everything up to and including the last marker.
        // PostDesc = everything after (post-talent prose for the current creature).
        void FinaliseTalents()
        {
            if (!collectingTalents) return;

            var raw = talentSectionBuf.ToString();
            SplitAtLastDp(raw, out var talents, out var postDesc);
            curTalentsBuf.Append(talents);
            if (!string.IsNullOrWhiteSpace(postDesc))
            {
                if (curDescBuf.Length > 0) curDescBuf.Append(' ');
                curDescBuf.Append(postDesc.Trim());
            }

            talentSectionBuf.Clear();
            collectingTalents = false;
            talentNoiseHit    = false;
        }

        void StartCreature(string name, string group, string desc)
        {
            FinaliseTalents();
            Emit();

            curName = name; curGroup = group;
            curCategory = curArchetype = curSize = curArmor =
            curTreasure = curRealm    = curMisc  = "";
            Array.Clear(curStats);
            curDescBuf.Clear();
            curDescBuf.Append(desc);
            curTalentsBuf.Clear();

            inFence        = false;
            statStarted    = false;
            waitStatHeader = false;
            waitStatValues = false;
        }

        // ── Main loop ─────────────────────────────────────────────────────────

        foreach (var raw in lines)
        {
            var line = raw.Trim();

            // Code-fence toggle (always processed first)
            if (line == "```") { inFence = !inFence; continue; }

            if (string.IsNullOrEmpty(line)) continue;

            // ── Noise lines (page-break artefacts) ───────────────────────────
            if (IsNoise(line))
            {
                // First noise after talent section starts → switch collection target
                if (collectingTalents && !talentNoiseHit)
                    talentNoiseHit = true;
                continue;
            }

            // ── Doubled-text detection (always highest priority) ──────────────
            if (!inFence && TryExtractDoubled(line, out var half) && half.Length > 2)
            {
                if (ChapterNum.IsMatch(half))
                {
                    FinaliseTalents();
                    curGroup = NormalizeGroup(half);
                }
                else
                {
                    StartCreature(half, curGroup, pendingDesc.ToString());
                    pendingDesc.Clear();
                }
                continue;
            }

            // ── Talent-section collection ─────────────────────────────────────
            if (collectingTalents)
            {
                if (!talentNoiseHit)
                    talentSectionBuf.Append(line).Append(' ');  // pre-noise → current creature
                else if (IsTableJunk(line))
                    pendingDesc.Clear();                        // summary table → drop leading garbage
                else if (IsProse(line))
                    pendingDesc.Append(line).Append(' ');       // post-noise → next creature
                continue;
            }

            // ── Normal stat-block parsing ─────────────────────────────────────
            if (inFence)
            {
                if (!statStarted && line.StartsWith("Category:"))
                {
                    curCategory = line[9..].Trim();
                    statStarted = true;
                }
                else if (statStarted)
                {
                    if      (line.StartsWith("Archetype:")) curArchetype = line[10..].Trim();
                    else if (line.StartsWith("Size:"))       curSize      = line[5..].Trim();
                    else if (line.StartsWith("Armor:"))      curArmor     = line[6..].Trim();
                    else if (line.StartsWith("Treasure:"))   curTreasure  = line[9..].Trim();
                    else if (line.StartsWith("Realm:"))      curRealm     = line[6..].Trim();
                    else if (line.StartsWith("Misc"))
                    {
                        var c = line.IndexOf(':');
                        curMisc = c >= 0 ? line[(c + 1)..].Trim() : "";
                    }
                }
            }
            else
            {
                if (line == "Stat Bonuses:")
                    waitStatHeader = true;
                else if (waitStatHeader && line.StartsWith("AG "))
                {
                    waitStatHeader = false; waitStatValues = true;
                }
                else if (waitStatValues)
                {
                    curStats = ParseStatLine(line);
                    waitStatValues = false;
                }
                else if (IsTalentsLabel(line))
                {
                    collectingTalents = true;
                    talentNoiseHit    = false;
                }
                else if (IsTableJunk(line))
                {
                    // A summary stat/skill table line. The real prose for the next creature
                    // comes *after* the table, so drop everything accumulated so far.
                    pendingDesc.Clear();
                }
                else if (IsProse(line))
                {
                    // Prose outside code fence, outside talent section → pending for next creature.
                    // Stat-summary tables (Table 5.3a, ##### Movement Stats, stat codes) are skipped.
                    pendingDesc.Append(line).Append(' ');
                }
            }
        }

        FinaliseTalents();
        Emit();
        return results;
    }

    // ── Static helpers ────────────────────────────────────────────────────────

    private static void SplitAtLastDp(string raw, out string talents, out string postDesc)
    {
        var matches = DpMarker.Matches(raw);
        if (matches.Count == 0) { talents = raw; postDesc = ""; return; }

        var last  = matches[^1];
        var split = last.Index + last.Length;

        // Consume trailing semicolons / whitespace so they stay with the talent section
        while (split < raw.Length && (raw[split] == ';' || char.IsWhiteSpace(raw[split])))
            split++;

        talents  = raw[..split].Trim();
        postDesc = split < raw.Length ? raw[split..].Trim() : "";
    }

    private static bool TryExtractDoubled(string line, out string half)
    {
        if (line.Length >= 4 && line.Length % 2 == 0)
        {
            var h = line.Length / 2;
            if (line[..h] == line[h..]) { half = line[..h].Trim(); return !string.IsNullOrEmpty(half); }
        }
        half = "";
        return false;
    }

    private static string NormalizeGroup(string half) =>
        ChapterNum.Match(half).Value.TrimEnd('.') switch
        {
            "5"  => "Animals",
            "6"  => "Artificials",
            "7"  => "Elementals",
            "8"  => "Extraplanar Creatures",
            "9"  => "Fell Creatures",
            "10" => "Fey",
            "11" => "Monsters",
            "12" => "Plants",
            "13" => "Races",
            "14" => "Undead",
            _    => Regex.Replace(half, @"^\d+\.\s*", "").Trim()
        };

    // Distinguishes readable prose (creature description) from stat-table garbage.
    // The summary tables (e.g. "Table 5.3a", "##### Movement Stats") are walls of
    // stat codes — "92M(5)ra76M(5)gr", "Percept.:", "M/V,-,-", lone numbers — whose
    // tokens carry internal digits/punctuation. A prose line, by contrast, contains
    // several plain alphabetic words. Require at least two such words.
    // Final cleanup of an assembled description: cut off any trailing summary table
    // that bled in, and discard "descriptions" that are really leftover talent costs.
    private static string Polish(string desc)
    {
        if (string.IsNullOrWhiteSpace(desc)) return "";

        var m = TableStart.Match(desc);
        if (m.Success) desc = desc[..m.Index].Trim();

        desc = JunkBits.Replace(desc, " ");
        desc = Regex.Replace(desc, @"\s+", " ").Trim();

        // Drop anything that is now just a leftover fragment rather than real prose.
        var dpHits = DpCount.Matches(desc).Count;
        var words  = desc.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;
        if (dpHits >= 2 || words < 8) return "";

        return desc;
    }

    private static bool IsTableJunk(string line)
    {
        if (line.EndsWith(':')) return true;                 // skill/column label rows
        foreach (var m in TableMarkers)
            if (line.Contains(m, StringComparison.Ordinal)) return true;
        return false;
    }

    private static bool IsProse(string line)
    {
        if (line.StartsWith('#'))            return false;
        if (TableHeading.IsMatch(line))      return false;
        if (SectionHead.IsMatch(line))       return false;
        if (IsTableJunk(line))               return false;

        var words = 0;
        foreach (var tok in line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
        {
            var t = tok.Trim(EdgePunct);
            if (t.Length >= 2 && t.All(char.IsLetter) && ++words >= 2)
                return true;
        }
        return false;
    }

    private static bool IsNoise(string line)
    {
        if (Noise.Contains(line)) return true;
        return int.TryParse(line, out _);
    }

    private static bool IsTalentsLabel(string line) =>
        line.StartsWith("Talents", StringComparison.OrdinalIgnoreCase)
        && line.Contains("Flaws", StringComparison.OrdinalIgnoreCase);

    private static int[] ParseStatLine(string line)
    {
        var parts  = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        var result = new int[10];
        for (var i = 0; i < Math.Min(parts.Length, 10); i++)
            int.TryParse(parts[i], out result[i]);
        return result;
    }

    // Join hyphenated line-breaks (e.g. "inadver- tent" → "inadvertent"),
    // then collapse all remaining whitespace runs to a single space.
    private static string CleanTalents(string raw) =>
        Regex.Replace(Regex.Replace(raw, @"-\s+", ""), @"\s+", " ").Trim();

    private static string CleanText(string raw) =>
        Regex.Replace(Regex.Replace(raw, @"-\s+", ""), @"\s+", " ").Trim();
}
