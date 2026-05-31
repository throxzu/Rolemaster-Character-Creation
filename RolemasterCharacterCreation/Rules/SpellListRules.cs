namespace RolemasterCharacterCreation.Rules;

public static class SpellListRules
{
    static readonly string[] OpenChanneling =
    [
        "Barrier Law", "Concussion's Way", "Detection Mastery", "Light's Way",
        "Lofty Movements", "Nature's Law", "Purifications", "Sound's Way",
        "Spell Defense", "Weather Ways"
    ];

    // Closed Channeling + Evil Channeling (same DP tier)
    static readonly string[] ClosedChanneling =
    [
        "Blood Law", "Bone Law", "Calm Spirits", "Creations",
        "Locating Ways", "Lore", "Mounted Ways", "Muscle Law",
        "Nerve & Organ Law", "Symbolic Ways",
        // Evil Channeling
        "Curses", "Dark Channels", "Demonic Pacts", "Demonic Summons",
        "Disease", "Wounding"
    ];

    static readonly string[] OpenEssence =
    [
        "Delving Ways", "Detecting Ways", "Elemental Shields", "Essence Hand",
        "Essence's Perceptions", "Lesser Illusions", "Physical Enhancement",
        "Rune Mastery", "Spell Wall", "Unbarring Ways"
    ];

    // Closed Essence + Evil Essence (same DP tier)
    static readonly string[] ClosedEssence =
    [
        "Dispelling Ways", "Fluid Elements", "Gate Mastery", "Invisible Ways",
        "Living Change", "Lofty Bridge", "Luminous Elements", "Rapid Ways",
        "Shield Mastery", "Solid Elements", "Spell Enhancement", "Spell Reins",
        "Spirit Mastery",
        // Evil Essence
        "Darkness", "Dark Summons", "Essence Twisting", "Foul Transformations",
        "Necromancy Mastery", "Necromantic Ways"
    ];

    static readonly string[] OpenMentalism =
    [
        "Anticipations", "Brilliance", "Cloaking", "Damage Resistance",
        "Delving", "Detections", "Seemings", "Self Healing",
        "Spell Resistance", "Telekinesis"
    ];

    // Closed Mentalism + Evil Mentalism (same DP tier)
    static readonly string[] ClosedMentalism =
    [
        "Attack Avoidance", "Gas Manipulation", "Liquid Manipulation", "Mind's Door",
        "Mind Mastery", "Movement", "Sense Mastery", "Shifting",
        "Solid Manipulation", "Speed",
        // Evil Mentalism
        "Mind Death", "Mind Disease", "Mind Domination", "Mind Erosion",
        "Mind Illusions", "Mind Subversion"
    ];

    static readonly Dictionary<string, string[]> BaseListsByProfession = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Cleric"]      = ["Channels", "Communal Ways", "Life Mastery", "Protections", "Repulsions", "Summons"],
        ["Druid"]       = ["Animal Mastery", "Herb Mastery", "Nature's Lore", "Nature's Protection", "Nature's Wrath", "Plant Mastery"],
        ["Paladin"]     = ["Cursebreaking", "Holy Arms", "Holy Healing", "Holy Shields", "Holy War", "Inspiring Ways"],
        ["Ranger"]      = ["Beastly Ways", "Inner Walls", "Moving Ways", "Nature's Guises", "Pathmastery", "Survival's Way"],
        ["Bard"]        = ["Controlling Songs", "Entertaining Ways", "Inspiring Songs", "Item Lore", "Sound Control", "Sound Projection"],
        ["Dabbler"]     = ["Concealment Mastery", "Influences", "Movement Mastery", "Senses", "Smoke And Mirrors", "Trade Mastery"],
        ["Illusionist"] = ["Guises", "Illusion Mastery", "Light Molding", "Mind Sense Molding", "Sense Molding", "Sound Molding"],
        ["Magician"]    = ["Earth Law", "Fire Law", "Ice Law", "Light Law", "Water Law", "Wind Law"],
        ["Lay Healer"]  = ["Blood Mastery", "Bone Mastery", "Concussion Mastery", "Muscle Mastery", "Nerve And Organ Mastery", "Prosthetics"],
        ["Magent"]      = ["Assassination Mastery", "Disguise Mastery", "Escapes", "Gathering Secrets", "Misdirections", "Poison Mastery"],
        ["Mentalist"]   = ["Mind Attack", "Mind Control", "Mind Merge", "Mind Speech", "Presence", "Sense Control"],
        ["Monk"]        = ["Body Reins", "Combat Mastery", "Evasions", "Mind Over Matter", "Monk's Bridge", "Monk's Sense"],
        ["Healer"]      = ["Blood Ways", "Bone Ways", "Cleansing", "Muscle Ways", "Organ Ways", "Surface Ways"],
        ["Mystic"]      = ["Confusing Ways", "Gas Alteration", "Hiding", "Liquid Alteration", "Mystical Change", "Solid Alteration"],
        ["Sorcerer"]    = ["Flesh Destruction", "Fluid Destruction", "Gas Destruction", "Mind Destruction", "Solid Destruction", "Soul Destruction"],
    };

    /// <summary>
    /// Returns all canonical list names for a given spell-list skill type and character context.
    /// Hybrid realms (e.g. "Channeling+Mentalism") include lists from all component realms.
    /// Returns empty for Arcane and Restricted (no lists defined in core Spell Law).
    /// </summary>
    public static string[] GetAvailableLists(string skillName, string? realm, string? profession)
    {
        bool ch = realm?.Contains("Channeling") == true;
        bool es = realm?.Contains("Essence")    == true;
        bool me = realm?.Contains("Mentalism")  == true;

        return skillName switch
        {
            "Open Spell Lists"   => Combine(ch ? OpenChanneling   : [], es ? OpenEssence   : [], me ? OpenMentalism   : []),
            "Closed Spell Lists" => Combine(ch ? ClosedChanneling  : [], es ? ClosedEssence  : [], me ? ClosedMentalism  : []),
            "Base Spell Lists"   => profession is not null && BaseListsByProfession.TryGetValue(profession, out var bl) ? bl : [],
            _                    => []
        };
    }

    static string[] Combine(string[] a, string[] b, string[] c) => [.. a, .. b, .. c];
}
