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
            new SkillDef("Animal Handling", "Pr", Specialized: true,
                Description: "Handle, train, and calm animals of a specific type. Uses Pr stat."),
            new SkillDef("Riding",          "Pr", Specialized: true,
                Description: "Ride and control a specific mount type; unskilled riders cannot act while the mount moves and must roll when galloping. Uses Pr stat.")),

        new("Awareness",           "In", "Re", false,
            new SkillDef("Perception", "SD",
                Description: "Notice hidden details, spot ambushes, and detect changes in the environment. Uses SD stat."),
            new SkillDef("Tracking",   "SD",
                Description: "Follow a creature's trail across terrain using footprints, disturbed foliage, and other physical signs. Uses SD stat.")),

        new("Battle Expertise",    "",   "",   true,
            new SkillDef("Maneuvering in Armor", "",
                Description: "Move and fight effectively in heavier armor; skill bonus reduces the armor's maneuver penalty (to a minimum of 0). No stat bonus."),
            new SkillDef("Mounted Combat",        "",
                Description: "Fight effectively while mounted without penalty; required for most combat actions on horseback. No stat bonus."),
            new SkillDef("Protect",               "",
                Description: "Shield an adjacent ally by diverting attacks targeting them to yourself. No stat bonus."),
            new SkillDef("Restricted Quarters",   "",
                Description: "Fight without penalty in tight spaces such as corridors, ships, or crowds. No stat bonus."),
            new SkillDef("Subduing",              "",
                Description: "Incapacitate an opponent nonlethally, converting lethal results to stun and concussion damage. No stat bonus.")),

        new("Body Discipline",     "Co", "SD", false,
            new SkillDef("Adrenal Defense",  "Ag",
                Description: "Dodge and evade attacks through trained adrenal reflexes; adds a defensive bonus each round it is used. Uses Ag stat."),
            new SkillDef("Adrenal Focus",    "SD",
                Description: "Concentrate mental and physical energy for precise adrenal control and heightened awareness. Uses SD stat."),
            new SkillDef("Adrenal Speed",    "Qu",
                Description: "Temporarily enhance movement and reaction speed beyond normal physical limits. Uses Qu stat."),
            new SkillDef("Adrenal Strength", "St",
                Description: "Tap reserves of muscular power for feats of great strength beyond normal capacity. Uses St stat.")),

        new("Brawn",               "Co", "SD", false,
            new SkillDef("Body Development", "Co",
                Description: "Train the body's physical endurance; skill bonus adds directly to total hit points. Character dies when negative hits exceed this bonus. Uses Co stat."),
            new SkillDef("Fortitude",        "SD",
                Description: "Resist exhaustion, pain, disease, and extended endurance challenges. Uses SD stat."),
            new SkillDef("Weight-training",  "St",
                Description: "Develop raw muscular strength and lifting capacity through physical conditioning. Uses St stat.")),

        new("Combat Expertise",    "",   "",   true,
            new SkillDef("Blind Fighting",   "",
                Description: "Fight effectively without sight, negating darkness and blindness combat penalties. No stat bonus."),
            new SkillDef("Disarm",           "",
                Description: "Knock or strip a weapon from an opponent's grip in melee combat. No stat bonus."),
            new SkillDef("Footwork",         "",
                Description: "Maintain optimal tactical positioning to gain initiative advantage in melee. No stat bonus."),
            new SkillDef("Multiple Attacks", "",
                Description: "Strike more than once per round at a reduced attack penalty per extra strike. No stat bonus."),
            new SkillDef("Reverse Strike",   "",
                Description: "Attack from an unexpected angle to bypass a defender's guard and shield. No stat bonus.")),

        new("Combat Training",     "Ag", "St", false,
            new SkillDef("Melee Weapons",  "St",      Specialized: true,
                Description: "Wield a specific type of melee weapon effectively in combat. Uses St stat."),
            new SkillDef("Ranged Weapons", "Ag",      Specialized: true,
                Description: "Aim and fire a specific type of ranged weapon effectively. Uses Ag stat."),
            new SkillDef("Shield",         "St",
                Description: "Use a shield to deflect attacks and add a defensive bonus in combat. Uses St stat."),
            new SkillDef("Unarmed",        "St or Ag", Specialized: true,
                Description: "Fight without weapons using punches, kicks, grapples, and throws. Uses St or Ag stat.")),

        new("Composition",         "Em", "In", false,
            new SkillDef("Illusion Crafting",   "Pr",
                Description: "Create and sustain believable illusions with convincing detail and duration. Uses Pr stat."),
            new SkillDef("Musical Composition", "Pr",
                Description: "Compose original music and understand advanced music theory and notation. Uses Pr stat."),
            new SkillDef("Writing",             "Re",
                Description: "Craft well-structured prose, poetry, histories, or formal documents. Uses Re stat.")),

        new("Crafting",            "Ag", "Me", false,
            new SkillDef("Culinary",        "SD",
                Description: "Prepare food and beverages; skilled cooks can identify herbs and detect poisons in food. Uses SD stat."),
            new SkillDef("Drawing/Painting","In",
                Description: "Create visual art, accurate maps, or technical illustrations. Uses In stat."),
            new SkillDef("Fabric Craft",    "SD",
                Description: "Make clothing, tapestries, rope, and fabric goods from raw material. Uses SD stat."),
            new SkillDef("Leathercraft",    "SD",
                Description: "Work leather into armor, harnesses, boots, sheaths, and similar goods. Uses SD stat."),
            new SkillDef("Metalcraft",      "St",
                Description: "Smelt, forge, and shape metal into tools, armor, weapons, and hardware. Uses St stat."),
            new SkillDef("Stonecraft",      "St",
                Description: "Cut, dress, and shape stone for construction, carving, or artistic work. Uses St stat."),
            new SkillDef("Woodcraft",       "SD",
                Description: "Work wood into furniture, tools, bows, boats, and structures. Uses SD stat.")),

        new("Delving",             "Em", "In", false,
            new SkillDef("Attunement", "Pr",
                Description: "Sense and interpret magical emanations in objects, places, and creatures; detect ambient magic. Uses Pr stat."),
            new SkillDef("Runes",      "Pr",
                Description: "Read, write, and create magical inscriptions and runic glyphs; required for rune-based spells. Uses Pr stat.")),

        new("Environmental",       "In", "Me", false,
            new SkillDef("Navigation", "Re",
                Description: "Determine location and plot routes across land or sea using landmarks, stars, and maps. Uses Re stat."),
            new SkillDef("Piloting",   "Ag", Specialized: true,
                Description: "Operate a specific type of vehicle such as a river boat, sailing ship, or war chariot. Uses Ag stat."),
            new SkillDef("Survival",   "Re", Specialized: true,
                Description: "Find food, water, shelter, and safety in a specific natural environment (own region). Uses Re stat.")),

        new("Gymnastic",           "Ag", "Qu", false,
            new SkillDef("Acrobatics",  "St",
                Description: "Perform flips, rolls, and agile maneuvers; a successful roll can reduce falling damage. Uses St stat."),
            new SkillDef("Contortions", "SD",
                Description: "Escape bonds, squeeze through tight gaps, and bend the body into unusual positions. Uses SD stat."),
            new SkillDef("Jumping",     "St",
                Description: "Leap over obstacles or gaps; skill bonus adds directly to jump distance and height. Uses St stat.")),

        new("Lore",                "Me", "Me", false,
            new SkillDef("Creature Lore",      "Re", Specialized: true,
                Description: "Know the anatomy, habits, weaknesses, and lore of a specific type of creature. Uses Re stat."),
            new SkillDef("Historic Lore",      "Re", Specialized: true,
                Description: "Know events, figures, and cultures of a specific historical period or region. Uses Re stat."),
            new SkillDef("Language",           "Re", Specialized: true,
                Description: "Speak and read a specific language; ranks 1-3 are broken, 4-6 proficient, 10+ fully fluent with dialects. Uses Re stat."),
            new SkillDef("Materials Lore",     "Re", Specialized: true,
                Description: "Identify and understand the properties and value of metals, stones, herbs, and other substances. Uses Re stat."),
            new SkillDef("Racial Lore",        "Re", Specialized: true,
                Description: "Know the customs, history, biology, and cultural practices of a specific race. Uses Re stat."),
            new SkillDef("Region Lore",        "Re", Specialized: true,
                Description: "Know the geography, politics, notable settlements, and lore of a specific region. Uses Re stat."),
            new SkillDef("Religion/Philosophy","Re", Specialized: true,
                Description: "Know the doctrine, history, rituals, and clergy of a specific faith or school of philosophy. Uses Re stat."),
            new SkillDef("Spell Lore",         "In",
                Description: "Understand magic theory; identify active spell effects, detect magical traps, and recognize enchanted items. Uses In stat.")),

        new("Magical Expertise",   "",   "",   true,
            new SkillDef("Grace",         "",
                Description: "Perform magical actions smoothly without telegraphing intent; reduces spellcasting fumble range and penalties. No stat bonus."),
            new SkillDef("Spell Trickery","", Specialized: true,
                Description: "Disguise or alter the visible appearance of spell effects to deceive observers; specialized by effect type. No stat bonus."),
            new SkillDef("Transcendence", "",
                Description: "Overcome physical limitations from injury or fatigue when channeling magical energy. No stat bonus.")),

        new("Medical",             "In", "Me", false,
            new SkillDef("Herbalism",      "Re",
                Description: "Identify, gather, prepare, and use medicinal and poisonous plants for treatment or harm. Uses Re stat."),
            new SkillDef("Medicine",       "Re",
                Description: "Diagnose and treat injuries and diseases; successful treatment reduces natural healing time. Uses Re stat."),
            new SkillDef("Poison Mastery", "Re",
                Description: "Identify, create, apply, delay, and neutralize poisons of all types and potencies. Uses Re stat.")),

        new("Mental Discipline",   "Pr", "SD", false,
            new SkillDef("Control Lycanthropy", "SD",
                Description: "Suppress or manage involuntary transformation caused by lycanthropy through mental discipline. Uses SD stat."),
            new SkillDef("Meditation",          "SD",
                Description: "Focus the mind to recover Power Points faster and resist mental influence and fear. Uses SD stat."),
            new SkillDef("Mental Focus",        "SD",
                Description: "Sustain concentration under duress; reduces penalties for performing multiple simultaneous tasks. Uses SD stat.")),

        new("Movement",            "Ag", "St", false,
            new SkillDef("Climbing",  "Co",
                Description: "Scale walls, cliffs, trees, and other vertical surfaces without falling. Uses Co stat."),
            new SkillDef("Flying",    "Co",
                Description: "Move through the air using wings or magical aid; relevant when such means are available. Uses Co stat."),
            new SkillDef("Running",   "Co",
                Description: "Move quickly over distances; improves sprint speed and long-distance endurance. Uses Co stat."),
            new SkillDef("Swimming",  "Co",
                Description: "Move and survive in water; resist drowning and fatigue in aquatic environments. Uses Co stat.")),

        new("Performance Art",     "Em", "Pr", false,
            new SkillDef("Acting",      "Me",
                Description: "Portray a character or emotion convincingly; directly aids disguise and social deception. Uses Me stat."),
            new SkillDef("Music",       "Me", Specialized: true,
                Description: "Perform on a specific instrument or in a vocal style to entertain, inspire, or unsettle. Uses Me stat."),
            new SkillDef("Stage Magic", "Ag",
                Description: "Perform sleight-of-hand tricks and illusions that appear magical but require no Power Points. Uses Ag stat.")),

        new("Power Manipulation",  "R*", "R*", false,
            new SkillDef("Channeling",       "SD",
                Description: "Draw power from a deity or external source to fuel spells; primary casting skill for Clerics and Druids. Uses SD stat."),
            new SkillDef("Directed Spell",   "Ag", Specialized: true,
                Description: "Aim a ranged spell attack at a specific target or precise location; specialized by spell type. Uses Ag stat."),
            new SkillDef("Power Development","Co",
                Description: "Expand the character's total pool of Power Points available for spellcasting. Uses Co stat."),
            new SkillDef("Power Projection", "SD",
                Description: "Extend the range or force of spells; deliver touch spells at short range. Uses SD stat.")),

        new("Science",             "Me", "Re", false,
            new SkillDef("Architecture", "Re",
                Description: "Design buildings and structures; analyze load-bearing features and identify structural weaknesses. Uses Re stat."),
            new SkillDef("Astronomy",    "Re",
                Description: "Read celestial events for navigation, calendar reckoning, and timing magical rituals. Uses Re stat."),
            new SkillDef("Engineering",  "Re",
                Description: "Design and construct machines, siege engines, complex devices, and fortifications. Uses Re stat."),
            new SkillDef("Mathematics",  "Re",
                Description: "Solve complex calculations; prerequisite or complement to many technical and scientific tasks. Uses Re stat.")),

        new("Social",              "Em", "In", false,
            new SkillDef("Influence",       "Pr", Specialized: true,
                Description: "Persuade, barter with, or charm a specific type of person or creature. Uses Pr stat."),
            new SkillDef("Leadership",      "Pr",
                Description: "Inspire and command allies; improves group morale and coordinated combat maneuvers. Uses Pr stat."),
            new SkillDef("Social Awareness","Em",
                Description: "Read social dynamics, detect lies, and sense the emotional state of individuals in a group. Uses Em stat."),
            new SkillDef("Trading",         "Pr",
                Description: "Negotiate fair prices, accurately appraise goods, and conduct advantageous commerce. Uses Pr stat.")),

        new("Spellcasting",        "R*", "R*", false,
            new SkillDef("Magical Ritual",       "Me", Specialized: true,
                Description: "Cast spells through extended ritual rather than instant channeling; specialized by ritual type. Uses Me stat."),
            new SkillDef("Open Spell Lists",     "Me", Specialized: true,
                Description: "Learn spells from open lists available to all in your realm. Ranks = maximum castable spell level. Uses Me stat."),
            new SkillDef("Closed Spell Lists",   "Me", Specialized: true,
                Description: "Learn spells from closed lists restricted to your specific realm. Ranks = maximum castable spell level. Uses Me stat."),
            new SkillDef("Base Spell Lists",     "Me", Specialized: true,
                Description: "Learn spells from base lists unique to your profession. Ranks = maximum castable spell level. Uses Me stat."),
            new SkillDef("Arcane Spell Lists",   "Me", Specialized: true,
                Description: "Learn spells from powerful arcane lists that transcend normal realm boundaries. Ranks = maximum castable spell level. Uses Me stat."),
            new SkillDef("Restricted Spell Lists","Me", Specialized: true,
                Description: "Access specially restricted spell lists not normally available to your profession. Ranks = maximum castable spell level. Uses Me stat.")),

        new("Subterfuge",          "Ag", "SD", false,
            new SkillDef("Ambush",     "In", Specialized: true,
                Description: "Adjust a critical strike result up or down by up to your Ambush ranks; ranks halved if the target is aware of you. Uses In stat."),
            new SkillDef("Concealment","In",
                Description: "Hide yourself or objects without moving; used for static hiding in cover, shadow, or camouflage. Uses In stat."),
            new SkillDef("Stalking",   "In",
                Description: "Move silently and avoid detection while in motion; used for trailing targets and infiltrating. Uses In stat."),
            new SkillDef("Trickery",   "In",
                Description: "Deceive, misdirect, and perform legerdemain in general social and criminal situations. Uses In stat.")),

        new("Technical",           "In", "Re", false,
            new SkillDef("Locks",     "Ag",
                Description: "Pick mechanical locks and understand locking mechanisms; higher ranks allow working without tools. Uses Ag stat."),
            new SkillDef("Mechanics", "Me", Specialized: true,
                Description: "Understand, repair, and build mechanical devices; specialized by device type. Uses Me stat."),
            new SkillDef("Traps",     "Ag",
                Description: "Detect, disarm, reset, and construct mechanical and magical traps of all kinds. Uses Ag stat.")),

        new("Vocation",            "Em", "Me", false,
            new SkillDef("Administration","Re", Specialized: true,
                Description: "Manage organizations, records, logistics, and personnel in a specific administrative role. Uses Re stat."),
            new SkillDef("Service",       "Pr", Specialized: true,
                Description: "Provide skilled professional services in a specific occupation such as innkeeping or guarding. Uses Pr stat."),
            new SkillDef("Trade",         "Re", Specialized: true,
                Description: "Practice a trade or craft profession commercially; specialized by trade such as farming, smithing, or sailing. Uses Re stat.")),
    };

    public static readonly IReadOnlyDictionary<string, SkillCategory> CategoryByName =
        Categories.ToDictionary(c => c.Name);

    public static readonly IReadOnlyDictionary<string, SkillDef> SkillByName =
        Categories.SelectMany(c => c.Skills).ToDictionary(s => s.Name);

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

public record SkillDef(string Name, string SkillStat, bool Specialized = false, string Description = "")
{
    public bool NoStats => SkillStat == "";
}
