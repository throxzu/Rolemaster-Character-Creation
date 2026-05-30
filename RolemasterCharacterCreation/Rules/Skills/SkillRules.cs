namespace RolemasterCharacterCreation.Rules;

public static class SkillRules
{
    // Table 2-5a
    public static int StatBonus(int stat) => stat switch
    {
        1 => -15,
        2 => -14,
        3 => -13,
        4 => -12,
        5 => -11,
        6 => -10,
        <= 8 => -9,
        <= 11 => -8,
        <= 14 => -7,
        <= 17 => -6,
        <= 23 => -5,
        <= 29 => -4,
        <= 35 => -3,
        <= 41 => -2,
        <= 47 => -1,
        <= 53 => 0,
        <= 59 => 1,
        <= 65 => 2,
        <= 71 => 3,
        <= 77 => 4,
        <= 83 => 5,
        <= 86 => 6,
        <= 89 => 7,
        <= 92 => 8,
        <= 94 => 9,
        95 => 10,
        96 => 11,
        97 => 12,
        98 => 13,
        99 => 14,
        _ => 15
    };

    // Table 3-0b
    public static int RankBonus(int ranks)
    {
        if (ranks <= 0) return -25;
        int bonus = 0;
        int remaining = ranks;
        int tier1 = Math.Min(remaining, 10); bonus += tier1 * 5; remaining -= tier1;
        int tier2 = Math.Min(remaining, 10); bonus += tier2 * 3; remaining -= tier2;
        int tier3 = Math.Min(remaining, 10); bonus += tier3 * 2; remaining -= tier3;
        bonus += remaining * 1;
        return bonus;
    }

    public static int SkillBonus(
        SkillDef skill,
        int stat1, int stat2, int statSkill,
        int ranks, bool isPro, bool isKnack)
    {
        int rankBonus = RankBonus(ranks);
        int statBonus = skill.NoStats ? 0 : StatBonus(stat1) + StatBonus(stat2) + StatBonus(statSkill);
        int profBonus = isPro ? Math.Min(ranks, 30) : 0;
        int knackBonus = isKnack ? 5 : 0;
        return rankBonus + statBonus + profBonus + knackBonus;
    }

    // All skill categories — ordered as Table 3-0a
    public static readonly IReadOnlyList<SkillCategory> Categories = new List<SkillCategory>
    {
        new("Animal",              "Ag", "Em", false,
            new SkillDef("Animal Handling", "Pr", Specialized: true),
            new SkillDef("Riding",          "Pr", Specialized: true)),

        new("Awareness",           "In", "Re", false,
            new SkillDef("Perception", "SD"),
            new SkillDef("Tracking",   "SD")),

        new("Battle Expertise",    "",   "",   true,
            new SkillDef("Maneuvering in Armor", ""),
            new SkillDef("Mounted Combat",        ""),
            new SkillDef("Protect",               ""),
            new SkillDef("Restricted Quarters",   ""),
            new SkillDef("Subduing",              "")),

        new("Body Discipline",     "Co", "SD", false,
            new SkillDef("Adrenal Defense",  "Ag"),
            new SkillDef("Adrenal Focus",    "SD"),
            new SkillDef("Adrenal Speed",    "Qu"),
            new SkillDef("Adrenal Strength", "St")),

        new("Brawn",               "Co", "SD", false,
            new SkillDef("Body Development", "Co"),
            new SkillDef("Fortitude",        "SD"),
            new SkillDef("Weight-training",  "St")),

        new("Combat Expertise",    "",   "",   true,
            new SkillDef("Blind Fighting",   ""),
            new SkillDef("Disarm",           ""),
            new SkillDef("Footwork",         ""),
            new SkillDef("Multiple Attacks", ""),
            new SkillDef("Reverse Strike",   "")),

        new("Combat Training",     "Ag", "St", false,
            new SkillDef("Melee Weapons",  "St",      Specialized: true),
            new SkillDef("Ranged Weapons", "Ag",      Specialized: true),
            new SkillDef("Shield",         "St"),
            new SkillDef("Unarmed",        "St or Ag", Specialized: true)),

        new("Composition",         "Em", "In", false,
            new SkillDef("Illusion Crafting",   "Pr"),
            new SkillDef("Musical Composition", "Pr"),
            new SkillDef("Writing",             "Re")),

        new("Crafting",            "Ag", "Me", false,
            new SkillDef("Culinary",        "SD"),
            new SkillDef("Drawing/Painting","In"),
            new SkillDef("Fabric Craft",    "SD"),
            new SkillDef("Leathercraft",    "SD"),
            new SkillDef("Metalcraft",      "St"),
            new SkillDef("Stonecraft",      "St"),
            new SkillDef("Woodcraft",       "SD")),

        new("Delving",             "Em", "In", false,
            new SkillDef("Attunement", "Pr"),
            new SkillDef("Runes",      "Pr")),

        new("Environmental",       "In", "Me", false,
            new SkillDef("Navigation", "Re"),
            new SkillDef("Piloting",   "Ag", Specialized: true),
            new SkillDef("Survival",   "Re", Specialized: true)),

        new("Gymnastic",           "Ag", "Qu", false,
            new SkillDef("Acrobatics",  "St"),
            new SkillDef("Contortions", "SD"),
            new SkillDef("Jumping",     "St")),

        new("Lore",                "Me", "Me", false,
            new SkillDef("Creature Lore",      "Re", Specialized: true),
            new SkillDef("Historic Lore",      "Re", Specialized: true),
            new SkillDef("Language",           "Re", Specialized: true),
            new SkillDef("Materials Lore",     "Re", Specialized: true),
            new SkillDef("Racial Lore",        "Re", Specialized: true),
            new SkillDef("Region Lore",        "Re", Specialized: true),
            new SkillDef("Religion/Philosophy","Re", Specialized: true),
            new SkillDef("Spell Lore",         "In")),

        new("Magical Expertise",   "",   "",   true,
            new SkillDef("Grace",         ""),
            new SkillDef("Spell Trickery","", Specialized: true),
            new SkillDef("Transcendence", "")),

        new("Medical",             "In", "Me", false,
            new SkillDef("Herbalism",      "Re"),
            new SkillDef("Medicine",       "Re"),
            new SkillDef("Poison Mastery", "Re")),

        new("Mental Discipline",   "Pr", "SD", false,
            new SkillDef("Control Lycanthropy", "SD"),
            new SkillDef("Meditation",          "SD"),
            new SkillDef("Mental Focus",        "SD")),

        new("Movement",            "Ag", "St", false,
            new SkillDef("Climbing",  "Co"),
            new SkillDef("Flying",    "Co"),
            new SkillDef("Running",   "Co"),
            new SkillDef("Swimming",  "Co")),

        new("Performance Art",     "Em", "Pr", false,
            new SkillDef("Acting",      "Me"),
            new SkillDef("Music",       "Me", Specialized: true),
            new SkillDef("Stage Magic", "Ag")),

        new("Power Manipulation",  "R*", "R*", false,
            new SkillDef("Channeling",       "SD"),
            new SkillDef("Directed Spell",   "Ag", Specialized: true),
            new SkillDef("Power Development","Co"),
            new SkillDef("Power Projection", "SD")),

        new("Science",             "Me", "Re", false,
            new SkillDef("Architecture", "Re"),
            new SkillDef("Astronomy",    "Re"),
            new SkillDef("Engineering",  "Re"),
            new SkillDef("Mathematics",  "Re")),

        new("Social",              "Em", "In", false,
            new SkillDef("Influence",       "Pr", Specialized: true),
            new SkillDef("Leadership",      "Pr"),
            new SkillDef("Social Awareness","Em"),
            new SkillDef("Trading",         "Pr")),

        new("Spellcasting",        "R*", "R*", false,
            new SkillDef("Magical Ritual",       "Me", Specialized: true),
            new SkillDef("Open Spell Lists",     "Me", Specialized: true),
            new SkillDef("Closed Spell Lists",   "Me", Specialized: true),
            new SkillDef("Base Spell Lists",     "Me", Specialized: true),
            new SkillDef("Arcane Spell Lists",   "Me", Specialized: true),
            new SkillDef("Restricted Spell Lists","Me", Specialized: true)),

        new("Subterfuge",          "Ag", "SD", false,
            new SkillDef("Ambush",     "In", Specialized: true),
            new SkillDef("Concealment","In"),
            new SkillDef("Stalking",   "In"),
            new SkillDef("Trickery",   "In")),

        new("Technical",           "In", "Re", false,
            new SkillDef("Locks",     "Ag"),
            new SkillDef("Mechanics", "Me", Specialized: true),
            new SkillDef("Traps",     "Ag")),

        new("Vocation",            "Em", "Me", false,
            new SkillDef("Administration","Re", Specialized: true),
            new SkillDef("Service",       "Pr", Specialized: true),
            new SkillDef("Trade",         "Re", Specialized: true)),
    };

    public static readonly IReadOnlyDictionary<string, SkillCategory> CategoryByName =
        Categories.ToDictionary(c => c.Name);

    public static SkillDef? FindSkill(string skillName)
    {
        foreach (var cat in Categories)
            foreach (var sk in cat.Skills)
                if (sk.Name == skillName) return sk;
        return null;
    }

    public static SkillCategory? CategoryOf(string skillName)
    {
        foreach (var cat in Categories)
            foreach (var sk in cat.Skills)
                if (sk.Name == skillName) return cat;
        return null;
    }
}

public record SkillCategory(string Name, string Stat1, string Stat2, bool NoStats, params SkillDef[] Skills);

public record SkillDef(string Name, string SkillStat, bool Specialized = false)
{
    public bool NoStats => SkillStat == "";
}
