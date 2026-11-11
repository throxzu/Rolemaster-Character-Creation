using RolemasterCharacterCreation.Models;

namespace RolemasterCharacterCreation.Rules;

/// <summary>
/// What the talents and racial traits in <see cref="TalentRules"/> and <see cref="RaceRules"/>
/// actually do to a character's numbers.
///
/// <para>Deliberately separate from the catalogue: <see cref="TalentRules.All"/> is a faithful
/// transcription of Chapter 4 and lists every talent, while the table here is only the subset the
/// app models. A talent missing from <see cref="Applied"/> is not a bug — it is one whose effect is
/// situational (Sharpshooter's aimed OB), narrative (Riposte), or already expressed elsewhere in
/// the data (see <see cref="NotModelled"/>). The sheet says which is which per row rather than
/// leaving the player to guess.</para>
///
/// <para>Racial free traits and purchased talents speak the same vocabulary — name, tier,
/// restriction — so both go through <see cref="For"/> and the caller keeps them apart only to show
/// them in different columns.</para>
/// </summary>
public static class TalentEffects
{
    // ── What one talent does ─────────────────────────────────────────────────

    private enum Kind
    {
        Hits, Db, NaturalAt, Initiative, Endurance, Bmr, Encumbrance, Spellcasting,
        RrPhysical,
        RrRealm,              // which realm comes from the restriction
        Skills,               // a fixed set of skills
        SkillFromRestriction, // the skill named in the restriction
    }

    /// <param name="PerTier">Signed, so a flaw's entry carries its own minus.</param>
    /// <param name="Skills">For <see cref="Kind.Skills"/>: the skills it modifies.</param>
    /// <param name="Label">How the sheet names the thing it modified.</param>
    private sealed record Eff(Kind Kind, int PerTier, string[]? Skills = null, string Label = "");

    private static readonly IReadOnlyDictionary<string, Eff> Applied =
        new Dictionary<string, Eff>(StringComparer.OrdinalIgnoreCase)
        {
            // Hits. The book modifies *base* hits ("normally equal to 25"), so this lands
            // before the race's size multiplier rather than after it.
            ["Tough"]                   = new(Kind.Hits, +5, Label: "base hits"),
            ["Fragile"]                 = new(Kind.Hits, -5, Label: "base hits"),

            ["Defensive Aura"]          = new(Kind.Db, +5, Label: "DB"),
            ["Natural Armor"]           = new(Kind.NaturalAt, +1, Label: "AT"),
            ["Fast Attack"]             = new(Kind.Initiative, +5, Label: "Initiative"),
            ["Vigorous"]                = new(Kind.Endurance, +10, Label: "Endurance"),
            ["Feeble"]                  = new(Kind.Endurance, -10, Label: "Endurance"),
            ["Increased Stride"]        = new(Kind.Bmr, +2, Label: "BMR"),
            ["Decreased Stride"]        = new(Kind.Bmr, -2, Label: "BMR"),
            ["Beast of Burden"]         = new(Kind.Encumbrance, +10, Label: "carrying capacity"),
            ["Eloquence"]               = new(Kind.Spellcasting, +5, Label: "spell casting"),
            ["Mumbler"]                 = new(Kind.Spellcasting, -5, Label: "spell casting"),

            ["Physical Resistance"]     = new(Kind.RrPhysical, +5, Label: "Physical RR"),
            ["Physical Vulnerability"]  = new(Kind.RrPhysical, -5, Label: "Physical RR"),
            ["Magical Resistance"]      = new(Kind.RrRealm, +5, Label: "RR"),
            ["Magical Vulnerability"]   = new(Kind.RrRealm, -5, Label: "RR"),

            ["Direction Sense"]         = new(Kind.Skills, +30, ["Navigation"]),
            ["Empathy"]                 = new(Kind.Skills, +50, ["Social Awareness"]),
            ["Perfect Pitch"]           = new(Kind.Skills, +10, ["Music"]),
            ["Golden Throat"]           = new(Kind.Skills, +10, ["Influence", "Leadership", "Music"]),
            ["Disturbing Voice"]        = new(Kind.Skills, -20, ["Influence", "Leadership", "Music"]),
            ["Look of Eagles"]          = new(Kind.Skills, +10, ["Leadership"]),
            ["Recurved Musculature"]    = new(Kind.Skills, +20,
                                              ["Acrobatics", "Climbing", "Jumping", "Running"]),
            ["Prodigy"]                 = new(Kind.SkillFromRestriction, +5),
            ["Inept"]                   = new(Kind.SkillFromRestriction, -5),
        };

    /// <summary>
    /// Talents whose effect the race data already carries, so applying them here would count it
    /// twice: a race's <c>Size</c> letter is what drives <see cref="RaceDef.HitMultiplier"/>, and
    /// Avinarc's reduced <c>BaseHits</c> already stands in for Light-Boned.
    /// </summary>
    public static readonly IReadOnlySet<string> NotModelled =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { "Increased Size", "Decreased Size", "Light-Boned" };

    // ── Conditional defences ─────────────────────────────────────────────────
    // These change a DB or an RR, but only against something in particular, so folding them into
    // a total would show a bonus the character does not always have. They get a line of their own
    // underneath instead.

    private static readonly IReadOnlyDictionary<string, (int PerTier, string What)> Conditional =
        new Dictionary<string, (int, string)>(StringComparer.OrdinalIgnoreCase)
        {
            ["Iron Will"]                = (+5, "RRs vs. mental spells"),
            ["Elemental Resistance"]     = (+5, "DB, RRs and Endurance"),
            ["Elemental Susceptibility"] = (-5, "DB, RRs and Endurance"),
        };

    // ── Specialization vocabularies ──────────────────────────────────────────

    public static readonly IReadOnlyList<string> Realms   = ["Channeling", "Essence", "Mentalism"];
    public static readonly IReadOnlyList<string> Elements = ["Cold/Ice", "Heat/Fire", "Electricity/Light"];

    // ── The aggregate ────────────────────────────────────────────────────────

    public sealed class TalentMods
    {
        public int Hits        { get; internal set; }
        public int Db          { get; internal set; }
        /// <summary>Tiers of Natural Armor, not an AT. AT 1 is no armour, so AT = 1 + tiers.</summary>
        public int NaturalAtTiers { get; internal set; }
        public int Initiative  { get; internal set; }
        public int Endurance   { get; internal set; }
        public int Bmr         { get; internal set; }
        /// <summary>Extra carrying capacity, as a percentage of the normal allowance.</summary>
        public int EncumbrancePct { get; internal set; }
        public int Spellcasting{ get; internal set; }

        public int RrChanneling { get; internal set; }
        public int RrEssence    { get; internal set; }
        public int RrMentalism  { get; internal set; }
        public int RrPhysical   { get; internal set; }
        public int RrFear       { get; internal set; }

        internal readonly Dictionary<string, int> SkillMods = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>Lines for the conditional modifiers, already worded for display.</summary>
        public List<string> ConditionalNotes { get; } = [];

        /// <summary>
        /// A talent naming a skill covers that skill and every specialization of it ("all
        /// specializations of one skill"), so the lookup is by skill name alone.
        /// </summary>
        public int SkillBonus(string? skillName) =>
            skillName is not null && SkillMods.TryGetValue(skillName, out int v) ? v : 0;

        public bool Any =>
            Hits != 0 || Db != 0 || NaturalAtTiers != 0 || Initiative != 0 || Endurance != 0
            || Bmr != 0 || EncumbrancePct != 0 || Spellcasting != 0 || RrChanneling != 0
            || RrEssence != 0 || RrMentalism != 0 || RrPhysical != 0 || RrFear != 0
            || SkillMods.Count > 0;
    }

    public static readonly TalentMods None = new();

    // ── Building it ──────────────────────────────────────────────────────────

    /// <summary>A racial trait's tier 0 means "flat", not "none" — see RaceRules' trait table.</summary>
    private static int EffectiveTier(int tier) => tier < 1 ? 1 : tier;

    public static TalentMods For(IEnumerable<(string Name, int Tier, string? Restriction)> held)
    {
        var m = new TalentMods();

        foreach (var (name, rawTier, restriction) in held)
        {
            int tier = EffectiveTier(rawTier);

            if (Conditional.TryGetValue(name, out var c))
            {
                string against = restriction is not null ? $"vs. {restriction}" : "";
                m.ConditionalNotes.Add(
                    $"{Signed(c.PerTier * tier)} to {c.What} {against}".Replace("  ", " ").TrimEnd()
                    + $" ({name} {Roman(tier)})");
                continue;
            }

            if (!Applied.TryGetValue(name, out var e)) continue;

            int amount = e.PerTier * tier;
            switch (e.Kind)
            {
                case Kind.Hits:         m.Hits           += amount; break;
                case Kind.Db:           m.Db             += amount; break;
                case Kind.NaturalAt:    m.NaturalAtTiers += tier;   break;
                case Kind.Initiative:   m.Initiative     += amount; break;
                case Kind.Endurance:    m.Endurance      += amount; break;
                case Kind.Bmr:          m.Bmr            += amount; break;
                case Kind.Encumbrance:  m.EncumbrancePct += amount; break;
                case Kind.Spellcasting: m.Spellcasting   += amount; break;
                case Kind.RrPhysical:   m.RrPhysical     += amount; break;

                case Kind.RrRealm:
                    switch (ResolveRealm(restriction))
                    {
                        case "Channeling": m.RrChanneling += amount; break;
                        case "Essence":    m.RrEssence    += amount; break;
                        case "Mentalism":  m.RrMentalism  += amount; break;
                        // An unrecognised realm contributes nothing; the sheet marks the row
                        // rather than quietly swallowing it.
                    }
                    break;

                case Kind.Skills:
                    foreach (var s in e.Skills!) Add(m.SkillMods, s, amount);
                    break;

                case Kind.SkillFromRestriction:
                    if (ResolveSkill(restriction) is { } skill) Add(m.SkillMods, skill, amount);
                    break;
            }
        }

        return m;
    }

    public static TalentMods ForTalents(IEnumerable<CharacterTalent>? talents) =>
        talents is null ? None
            : For(talents.Select(t => (t.TalentName, t.Tier, t.Restriction)));

    public static TalentMods ForTraits(IEnumerable<RacialTrait>? traits) =>
        traits is null ? None
            : For(traits.Select(t => (t.Name, t.Tier, t.Restriction)));

    private static void Add(Dictionary<string, int> into, string key, int amount) =>
        into[key] = into.TryGetValue(key, out int v) ? v + amount : amount;

    // ── Naming what was applied, for the sheet ───────────────────────────────

    /// <summary>
    /// How this one talent changed the sheet, or null when nothing was applied — either because
    /// the effect is situational or because its restriction could not be resolved
    /// (see <see cref="RestrictionUnresolved"/>, which tells the two apart).
    /// </summary>
    public static string? Describe(string name, int rawTier, string? restriction)
    {
        int tier = EffectiveTier(rawTier);

        if (Conditional.ContainsKey(name)) return null;
        if (!Applied.TryGetValue(name, out var e)) return null;

        int amount = e.PerTier * tier;
        return e.Kind switch
        {
            // AT 1 is no armour, and worn armour wins if it is heavier.
            Kind.NaturalAt => $"AT {1 + tier} if higher than worn",
            Kind.Encumbrance => $"{Signed(amount)}% {e.Label}",
            Kind.RrRealm => ResolveRealm(restriction) is { } realm
                ? $"{Signed(amount)} {realm} {e.Label}"
                : null,
            Kind.Skills => $"{Signed(amount)} to {string.Join(", ", e.Skills!)}",
            Kind.SkillFromRestriction => ResolveSkill(restriction) is { } skill
                ? $"{Signed(amount)} to {skill}"
                : null,
            _ => $"{Signed(amount)} {e.Label}",
        };
    }

    /// <summary>
    /// True when a talent needs a specialization the app can act on and the stored one is not it —
    /// "Magical Resistance (Fire)", say, where Fire is an element and the talent takes a realm.
    /// Worth showing, because such a talent silently contributes nothing.
    /// </summary>
    public static bool RestrictionUnresolved(string name, string? restriction) =>
        Applied.TryGetValue(name, out var e)
        && ((e.Kind == Kind.RrRealm && ResolveRealm(restriction) is null)
            || (e.Kind == Kind.SkillFromRestriction && ResolveSkill(restriction) is null));

    /// <summary>True when the app knowingly leaves this one to the race data.</summary>
    public static bool IsNotModelled(string name) => NotModelled.Contains(name);

    private static string? ResolveRealm(string? restriction) =>
        restriction is null ? null
            : Realms.FirstOrDefault(r => string.Equals(r, restriction.Trim(),
                                                       StringComparison.OrdinalIgnoreCase));

    private static string? ResolveSkill(string? restriction)
    {
        if (string.IsNullOrWhiteSpace(restriction)) return null;
        // Stored as "Climbing" or, for a specialization, "Music (Voice/Singing)" — the talent
        // covers the whole skill either way, so only the skill name matters.
        string name = restriction.Trim();
        int paren = name.IndexOf('(');
        if (paren > 0) name = name[..paren].Trim();
        return SkillRules.SkillByName.Keys.FirstOrDefault(
            k => string.Equals(k, name, StringComparison.OrdinalIgnoreCase));
    }

    private static string Signed(int v) => v >= 0 ? $"+{v}" : v.ToString();

    private static readonly string[] Numerals =
        ["", "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X"];

    private static string Roman(int tier) => Numerals[Math.Clamp(tier, 0, 10)];
}
