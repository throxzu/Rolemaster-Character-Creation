namespace RolemasterCharacterCreation.Rules;

public static class ProfessionRules
{
    // Skill category costs are Table 2-4a, transcribed from the rulebook text row by row.
    //
    // They were previously taken from docs/game-data/skills.md, which is shifted one row from
    // Combat Science downwards — every category held the next one's price, so a Fighter's
    // Composition read 2/4 instead of 3/4 and so on for 488 of the 704 values. The giveaway is
    // that under that reading Combat Training 4 costs less than Combat Training 1.
    //
    // Verified a second way: every profession also prints its own costs on its description
    // page, where the label and the value share a line. All 22 agree with the figures here.
    //
    // There is no "Combat Science" category, though an earlier version of this table had one.
    // In the printed cost box the line reads "Combat" — a group header for the four indented
    // "- Training 1..4" entries, just as "Spells" heads the five spell entries — with the
    // ordinary "Science" entry beside it in the right-hand column. Text extraction glues the
    // two into "CombatScience", and its value is always simply Science's. Chapter 3 confirms
    // it: Table 3-0a lists 25 categories and sections 3.1-3.25 cover them, going straight
    // from 3.6 Combat Expertise to 3.7 Combat Training. 25 categories, with Combat Training
    // and Spellcasting each priced per slot, is the 32 rows below.
    public static readonly IReadOnlyList<ProfessionDef> All = new List<ProfessionDef>
    {
        new("No Profession",  ProfGroup.Arms,         Realm: null,
            Costs: new(){
                ["Animal"]              = (2,4),    ["Awareness"]           = (2,4),    ["Battle Expertise"]    = (3,5),
                ["Body Discipline"]     = (4,6),    ["Brawn"]               = (3,4),    ["Combat Expertise"]    = (3,5),
                ["Combat Training 1"]   = (3,4),    ["Combat Training 2"]   = (3,5),
                ["Combat Training 3"]   = (5,7),    ["Combat Training 4"]   = (7,10),   ["Composition"]         = (2,4),
                ["Crafting"]            = (2,4),    ["Delving"]             = (4,6),    ["Environmental"]       = (3,4),
                ["Gymnastic"]           = (3,4),    ["Lore"]                = (1,3),    ["Magical Expertise"]   = (4,6),
                ["Medical"]             = (3,4),    ["Mental Discipline"]   = (3,4),    ["Movement"]            = (2,4),
                ["Performance Art"]     = (2,4),    ["Power Manipulation"]  = (4,6),    ["Science"]             = (4,6),
                ["Social"]              = (2,4),    ["Spells: Base/Open"]   = (4,6),    ["Spells: Ritual Magic"]= (5,7),
                ["Spells: Closed"]      = (7,10),   ["Spells: Arcane"]      = (15,20),  ["Spells: Restricted"]  = (15,20),
                ["Subterfuge"]          = (3,5),    ["Technical"]           = (3,5),    ["Vocation"]            = (3,4),
            },
            ProfessionalSkills: new[]{ "Any 10 skills" }),

        new("Fighter",        ProfGroup.Arms,         Realm: null,
            Costs: new(){
                ["Animal"]              = (2,3),    ["Awareness"]           = (2,4),    ["Battle Expertise"]    = (1,2),
                ["Body Discipline"]     = (4,6),    ["Brawn"]               = (1,3),    ["Combat Expertise"]    = (1,3),
                ["Combat Training 1"]   = (1,2),    ["Combat Training 2"]   = (1,2),
                ["Combat Training 3"]   = (2,3),    ["Combat Training 4"]   = (3,4),    ["Composition"]         = (3,4),
                ["Crafting"]            = (2,4),    ["Delving"]             = (7,10),   ["Environmental"]       = (2,4),
                ["Gymnastic"]           = (3,4),    ["Lore"]                = (2,4),    ["Magical Expertise"]   = (7,10),
                ["Medical"]             = (3,4),    ["Mental Discipline"]   = (5,7),    ["Movement"]            = (2,4),
                ["Performance Art"]     = (3,4),    ["Power Manipulation"]  = (9,12),   ["Science"]             = (5,7),
                ["Social"]              = (2,4),    ["Spells: Base/Open"]   = (7,10),   ["Spells: Ritual Magic"]= (9,12),
                ["Spells: Closed"]      = (15,20),  ["Spells: Arcane"]      = (15,20),  ["Spells: Restricted"]  = (15,20),
                ["Subterfuge"]          = (3,5),    ["Technical"]           = (5,7),    ["Vocation"]            = (3,4),
            },
            ProfessionalSkills: new[]{
                "Riding","Body Development","Fortitude","Weight-training","Maneuvering in Armor",
                "Mounted Combat","Protect","Disarm","Multiple Attacks","Melee Weapons",
                "Ranged Weapons","Shield","Metalcraft","Running","Leadership" }),

        new("Warrior Monk",   ProfGroup.Arms,         Realm: null,
            Costs: new(){
                ["Animal"]              = (3,5),    ["Awareness"]           = (2,4),    ["Battle Expertise"]    = (3,5),
                ["Body Discipline"]     = (1,3),    ["Brawn"]               = (3,4),    ["Combat Expertise"]    = (1,2),
                ["Combat Training 1"]   = (1,2),    ["Combat Training 2"]   = (1,3),
                ["Combat Training 3"]   = (2,4),    ["Combat Training 4"]   = (3,5),    ["Composition"]         = (3,4),
                ["Crafting"]            = (3,4),    ["Delving"]             = (7,10),   ["Environmental"]       = (3,5),
                ["Gymnastic"]           = (1,2),    ["Lore"]                = (2,4),    ["Magical Expertise"]   = (7,10),
                ["Medical"]             = (4,6),    ["Mental Discipline"]   = (2,3),    ["Movement"]            = (1,2),
                ["Performance Art"]     = (3,4),    ["Power Manipulation"]  = (9,12),   ["Science"]             = (5,7),
                ["Social"]              = (3,4),    ["Spells: Base/Open"]   = (7,10),   ["Spells: Ritual Magic"]= (9,12),
                ["Spells: Closed"]      = (15,20),  ["Spells: Arcane"]      = (15,20),  ["Spells: Restricted"]  = (15,20),
                ["Subterfuge"]          = (2,4),    ["Technical"]           = (5,7),    ["Vocation"]            = (3,4),
            },
            ProfessionalSkills: new[]{
                "Adrenal Defense","Adrenal Speed","Adrenal Strength","Body Development","Blind Fighting",
                "Multiple Attacks","Reverse Strike","Subduing","Unarmed","Acrobatics",
                "Jumping","Meditation","Climbing","Running","Swimming" }),

        new("Thief",          ProfGroup.Arms,         Realm: null,
            Costs: new(){
                ["Animal"]              = (2,3),    ["Awareness"]           = (1,2),    ["Battle Expertise"]    = (3,5),
                ["Body Discipline"]     = (6,8),    ["Brawn"]               = (4,6),    ["Combat Expertise"]    = (3,5),
                ["Combat Training 1"]   = (2,4),    ["Combat Training 2"]   = (3,4),
                ["Combat Training 3"]   = (4,6),    ["Combat Training 4"]   = (6,8),    ["Composition"]         = (3,4),
                ["Crafting"]            = (2,3),    ["Delving"]             = (5,7),    ["Environmental"]       = (2,4),
                ["Gymnastic"]           = (2,3),    ["Lore"]                = (2,4),    ["Magical Expertise"]   = (7,10),
                ["Medical"]             = (3,4),    ["Mental Discipline"]   = (4,6),    ["Movement"]            = (1,3),
                ["Performance Art"]     = (2,3),    ["Power Manipulation"]  = (9,12),   ["Science"]             = (4,6),
                ["Social"]              = (1,3),    ["Spells: Base/Open"]   = (7,10),   ["Spells: Ritual Magic"]= (9,12),
                ["Spells: Closed"]      = (15,20),  ["Spells: Arcane"]      = (15,20),  ["Spells: Restricted"]  = (15,20),
                ["Subterfuge"]          = (1,2),    ["Technical"]           = (1,2),    ["Vocation"]            = (3,4),
            },
            ProfessionalSkills: new[]{
                "Perception","Melee Weapons","Ranged Weapons","Acrobatics","Contortions",
                "Climbing","Running","Influence","Social Awareness","Trading",
                "Concealment","Stalking","Trickery","Locks","Traps" }),

        new("Rogue",          ProfGroup.Arms,         Realm: null,
            Costs: new(){
                ["Animal"]              = (2,4),    ["Awareness"]           = (2,3),    ["Battle Expertise"]    = (3,4),
                ["Body Discipline"]     = (4,6),    ["Brawn"]               = (2,4),    ["Combat Expertise"]    = (3,4),
                ["Combat Training 1"]   = (1,3),    ["Combat Training 2"]   = (2,3),
                ["Combat Training 3"]   = (3,4),    ["Combat Training 4"]   = (4,6),    ["Composition"]         = (3,4),
                ["Crafting"]            = (2,4),    ["Delving"]             = (7,10),   ["Environmental"]       = (2,4),
                ["Gymnastic"]           = (2,3),    ["Lore"]                = (2,4),    ["Magical Expertise"]   = (7,10),
                ["Medical"]             = (3,4),    ["Mental Discipline"]   = (5,7),    ["Movement"]            = (1,3),
                ["Performance Art"]     = (2,4),    ["Power Manipulation"]  = (9,12),   ["Science"]             = (5,7),
                ["Social"]              = (2,3),    ["Spells: Base/Open"]   = (7,10),   ["Spells: Ritual Magic"]= (9,12),
                ["Spells: Closed"]      = (15,20),  ["Spells: Arcane"]      = (15,20),  ["Spells: Restricted"]  = (15,20),
                ["Subterfuge"]          = (1,3),    ["Technical"]           = (2,3),    ["Vocation"]            = (3,4),
            },
            ProfessionalSkills: new[]{
                "Riding","Perception","Body Development","Maneuvering in Armor","Restricted Quarters",
                "Blind Fighting","Melee Weapons","Ranged Weapons","Survival","Jumping",
                "Poison Mastery","Climbing","Running","Ambush","Stalking" }),

        new("Laborer",        ProfGroup.Arms,         Realm: null,
            Costs: new(){
                ["Animal"]              = (1,3),    ["Awareness"]           = (3,4),    ["Battle Expertise"]    = (4,6),
                ["Body Discipline"]     = (6,8),    ["Brawn"]               = (1,2),    ["Combat Expertise"]    = (4,6),
                ["Combat Training 1"]   = (2,3),    ["Combat Training 2"]   = (2,4),
                ["Combat Training 3"]   = (3,5),    ["Combat Training 4"]   = (5,7),    ["Composition"]         = (3,4),
                ["Crafting"]            = (1,3),    ["Delving"]             = (7,10),   ["Environmental"]       = (1,3),
                ["Gymnastic"]           = (2,3),    ["Lore"]                = (2,3),    ["Magical Expertise"]   = (7,10),
                ["Medical"]             = (2,4),    ["Mental Discipline"]   = (4,6),    ["Movement"]            = (1,3),
                ["Performance Art"]     = (2,3),    ["Power Manipulation"]  = (9,12),   ["Science"]             = (4,6),
                ["Social"]              = (2,3),    ["Spells: Base/Open"]   = (7,10),   ["Spells: Ritual Magic"]= (9,12),
                ["Spells: Closed"]      = (15,20),  ["Spells: Arcane"]      = (15,20),  ["Spells: Restricted"]  = (15,20),
                ["Subterfuge"]          = (3,4),    ["Technical"]           = (2,3),    ["Vocation"]            = (3,4),
            },
            ProfessionalSkills: new[]{
                "Animal Handling","Body Development","Weight-training","Culinary","Drawing/Painting",
                "Fabric Craft","Leathercraft","Metalcraft","Stonecraft","Woodcraft",
                "Piloting","Running","Mechanics","Service","Trade" }),

        new("Scholar",        ProfGroup.Arms,         Realm: null,
            Costs: new(){
                ["Animal"]              = (3,4),    ["Awareness"]           = (1,3),    ["Battle Expertise"]    = (7,10),
                ["Body Discipline"]     = (6,8),    ["Brawn"]               = (6,8),    ["Combat Expertise"]    = (7,10),
                ["Combat Training 1"]   = (5,7),    ["Combat Training 2"]   = (6,8),
                ["Combat Training 3"]   = (9,12),   ["Combat Training 4"]   = (15,20),  ["Composition"]         = (1,2),
                ["Crafting"]            = (2,3),    ["Delving"]             = (2,3),    ["Environmental"]       = (2,4),
                ["Gymnastic"]           = (3,5),    ["Lore"]                = (1,2),    ["Magical Expertise"]   = (4,6),
                ["Medical"]             = (1,3),    ["Mental Discipline"]   = (1,3),    ["Movement"]            = (3,5),
                ["Performance Art"]     = (1,3),    ["Power Manipulation"]  = (4,6),    ["Science"]             = (1,3),
                ["Social"]              = (2,3),    ["Spells: Base/Open"]   = (4,6),    ["Spells: Ritual Magic"]= (5,7),
                ["Spells: Closed"]      = (7,10),   ["Spells: Arcane"]      = (15,20),  ["Spells: Restricted"]  = (15,20),
                ["Subterfuge"]          = (4,6),    ["Technical"]           = (2,4),    ["Vocation"]            = (3,4),
            },
            ProfessionalSkills: new[]{
                "Writing","Creature Lore","Historic Lore","Language","Materials Lore",
                "Racial Lore","Region Lore","Religion/Philosophy","Architecture","Astronomy",
                "Engineering","Mathematics","Administration","Service","Trade" }),

        new("Ranger",         ProfGroup.SemiSpell,    Realm: "Channeling",
            Costs: new(){
                ["Animal"]              = (1,3),    ["Awareness"]           = (1,3),    ["Battle Expertise"]    = (4,6),
                ["Body Discipline"]     = (6,8),    ["Brawn"]               = (4,6),    ["Combat Expertise"]    = (3,5),
                ["Combat Training 1"]   = (3,4),    ["Combat Training 2"]   = (3,5),
                ["Combat Training 3"]   = (5,7),    ["Combat Training 4"]   = (7,10),   ["Composition"]         = (3,4),
                ["Crafting"]            = (2,4),    ["Delving"]             = (6,8),    ["Environmental"]       = (1,3),
                ["Gymnastic"]           = (3,4),    ["Lore"]                = (2,4),    ["Magical Expertise"]   = (6,8),
                ["Medical"]             = (3,5),    ["Mental Discipline"]   = (5,7),    ["Movement"]            = (1,3),
                ["Performance Art"]     = (3,4),    ["Power Manipulation"]  = (4,6),    ["Science"]             = (5,7),
                ["Social"]              = (3,4),    ["Spells: Base/Open"]   = (3,4),    ["Spells: Ritual Magic"]= (3,5),
                ["Spells: Closed"]      = (5,7),    ["Spells: Arcane"]      = (12,15),  ["Spells: Restricted"]  = (15,20),
                ["Subterfuge"]          = (2,4),    ["Technical"]           = (5,7),    ["Vocation"]            = (3,4),
            },
            ProfessionalSkills: new[]{
                "Animal Handling","Riding","Perception","Tracking","Melee Weapons",
                "Ranged Weapons","Navigation","Survival","Creature Lore","Region Lore",
                "Climbing","Running","Swimming","Concealment","Stalking" }),

        new("Paladin",        ProfGroup.SemiSpell,    Realm: "Channeling",
            Costs: new(){
                ["Animal"]              = (2,3),    ["Awareness"]           = (3,5),    ["Battle Expertise"]    = (2,3),
                ["Body Discipline"]     = (5,7),    ["Brawn"]               = (2,4),    ["Combat Expertise"]    = (3,5),
                ["Combat Training 1"]   = (2,4),    ["Combat Training 2"]   = (3,4),
                ["Combat Training 3"]   = (4,6),    ["Combat Training 4"]   = (6,8),    ["Composition"]         = (3,4),
                ["Crafting"]            = (2,4),    ["Delving"]             = (6,8),    ["Environmental"]       = (3,5),
                ["Gymnastic"]           = (2,4),    ["Lore"]                = (2,4),    ["Magical Expertise"]   = (3,5),
                ["Medical"]             = (4,6),    ["Mental Discipline"]   = (5,7),    ["Movement"]            = (2,3),
                ["Performance Art"]     = (3,4),    ["Power Manipulation"]  = (4,6),    ["Science"]             = (5,7),
                ["Social"]              = (2,3),    ["Spells: Base/Open"]   = (3,5),    ["Spells: Ritual Magic"]= (4,6),
                ["Spells: Closed"]      = (6,8),    ["Spells: Arcane"]      = (15,20),  ["Spells: Restricted"]  = (15,20),
                ["Subterfuge"]          = (4,6),    ["Technical"]           = (5,7),    ["Vocation"]            = (3,4),
            },
            ProfessionalSkills: new[]{
                "Riding","Body Development","Maneuvering in Armor","Mounted Combat","Protect",
                "Melee Weapons","Shield","Metalcraft","Religion/Philosophy","Transcendence",
                "Mental Focus","Running","Channeling","Influence","Leadership" }),

        new("Monk",           ProfGroup.SemiSpell,    Realm: "Mentalism",
            Costs: new(){
                ["Animal"]              = (3,5),    ["Awareness"]           = (3,4),    ["Battle Expertise"]    = (6,8),
                ["Body Discipline"]     = (2,4),    ["Brawn"]               = (4,6),    ["Combat Expertise"]    = (2,4),
                ["Combat Training 1"]   = (2,4),    ["Combat Training 2"]   = (3,4),
                ["Combat Training 3"]   = (4,6),    ["Combat Training 4"]   = (6,8),    ["Composition"]         = (3,4),
                ["Crafting"]            = (3,4),    ["Delving"]             = (5,7),    ["Environmental"]       = (2,4),
                ["Gymnastic"]           = (2,3),    ["Lore"]                = (2,3),    ["Magical Expertise"]   = (6,8),
                ["Medical"]             = (3,4),    ["Mental Discipline"]   = (4,6),    ["Movement"]            = (1,3),
                ["Performance Art"]     = (3,4),    ["Power Manipulation"]  = (4,6),    ["Science"]             = (5,7),
                ["Social"]              = (3,4),    ["Spells: Base/Open"]   = (3,5),    ["Spells: Ritual Magic"]= (4,6),
                ["Spells: Closed"]      = (6,8),    ["Spells: Arcane"]      = (15,20),  ["Spells: Restricted"]  = (15,20),
                ["Subterfuge"]          = (3,5),    ["Technical"]           = (4,6),    ["Vocation"]            = (3,4),
            },
            ProfessionalSkills: new[]{
                "Perception","Adrenal Defense","Adrenal Focus","Adrenal Speed","Adrenal Strength",
                "Body Development","Multiple Attacks","Unarmed","Acrobatics","Jumping",
                "Meditation","Mental Focus","Climbing","Running","Swimming" }),

        new("Magent",         ProfGroup.SemiSpell,    Realm: "Mentalism",
            Costs: new(){
                ["Animal"]              = (3,5),    ["Awareness"]           = (2,3),    ["Battle Expertise"]    = (3,5),
                ["Body Discipline"]     = (4,6),    ["Brawn"]               = (5,7),    ["Combat Expertise"]    = (3,5),
                ["Combat Training 1"]   = (3,4),    ["Combat Training 2"]   = (3,5),
                ["Combat Training 3"]   = (5,7),    ["Combat Training 4"]   = (7,10),   ["Composition"]         = (3,4),
                ["Crafting"]            = (3,4),    ["Delving"]             = (6,8),    ["Environmental"]       = (3,5),
                ["Gymnastic"]           = (2,4),    ["Lore"]                = (2,4),    ["Magical Expertise"]   = (5,7),
                ["Medical"]             = (3,5),    ["Mental Discipline"]   = (3,5),    ["Movement"]            = (2,3),
                ["Performance Art"]     = (3,4),    ["Power Manipulation"]  = (3,5),    ["Science"]             = (5,7),
                ["Social"]              = (2,3),    ["Spells: Base/Open"]   = (3,4),    ["Spells: Ritual Magic"]= (3,5),
                ["Spells: Closed"]      = (5,7),    ["Spells: Arcane"]      = (12,15),  ["Spells: Restricted"]  = (15,20),
                ["Subterfuge"]          = (2,3),    ["Technical"]           = (4,6),    ["Vocation"]            = (3,4),
            },
            ProfessionalSkills: new[]{
                "Perception","Melee Weapons","Ranged Weapons","Acrobatics","Contortions",
                "Jumping","Poison Mastery","Mental Focus","Climbing","Running",
                "Swimming","Ambush","Concealment","Stalking","Trickery" }),

        new("Bard",           ProfGroup.SemiSpell,    Realm: "Essence",
            Costs: new(){
                ["Animal"]              = (3,5),    ["Awareness"]           = (2,4),    ["Battle Expertise"]    = (4,6),
                ["Body Discipline"]     = (6,8),    ["Brawn"]               = (5,7),    ["Combat Expertise"]    = (4,6),
                ["Combat Training 1"]   = (3,5),    ["Combat Training 2"]   = (4,6),
                ["Combat Training 3"]   = (6,8),    ["Combat Training 4"]   = (9,12),   ["Composition"]         = (1,3),
                ["Crafting"]            = (2,4),    ["Delving"]             = (3,5),    ["Environmental"]       = (3,5),
                ["Gymnastic"]           = (3,5),    ["Lore"]                = (1,2),    ["Magical Expertise"]   = (4,6),
                ["Medical"]             = (3,5),    ["Mental Discipline"]   = (5,7),    ["Movement"]            = (3,5),
                ["Performance Art"]     = (1,3),    ["Power Manipulation"]  = (3,5),    ["Science"]             = (3,5),
                ["Social"]              = (2,3),    ["Spells: Base/Open"]   = (2,4),    ["Spells: Ritual Magic"]= (3,4),
                ["Spells: Closed"]      = (4,6),    ["Spells: Arcane"]      = (9,12),   ["Spells: Restricted"]  = (12,15),
                ["Subterfuge"]          = (4,6),    ["Technical"]           = (4,6),    ["Vocation"]            = (3,4),
            },
            ProfessionalSkills: new[]{
                "Musical Composition","Attunement","Historic Lore","Language","Racial Lore",
                "Region Lore","Religion/Philosophy","Spell Lore","Spell Trickery","Running",
                "Acting","Music","Influence","Social Awareness","Trading" }),

        new("Dabbler",        ProfGroup.SemiSpell,    Realm: "Essence",
            Costs: new(){
                ["Animal"]              = (4,6),    ["Awareness"]           = (1,3),    ["Battle Expertise"]    = (5,7),
                ["Body Discipline"]     = (6,8),    ["Brawn"]               = (5,7),    ["Combat Expertise"]    = (4,6),
                ["Combat Training 1"]   = (3,5),    ["Combat Training 2"]   = (4,6),
                ["Combat Training 3"]   = (6,8),    ["Combat Training 4"]   = (9,12),   ["Composition"]         = (2,4),
                ["Crafting"]            = (3,4),    ["Delving"]             = (2,3),    ["Environmental"]       = (3,5),
                ["Gymnastic"]           = (3,5),    ["Lore"]                = (2,3),    ["Magical Expertise"]   = (3,5),
                ["Medical"]             = (3,5),    ["Mental Discipline"]   = (5,7),    ["Movement"]            = (3,4),
                ["Performance Art"]     = (2,4),    ["Power Manipulation"]  = (3,5),    ["Science"]             = (5,7),
                ["Social"]              = (2,3),    ["Spells: Base/Open"]   = (3,4),    ["Spells: Ritual Magic"]= (3,5),
                ["Spells: Closed"]      = (5,7),    ["Spells: Arcane"]      = (12,15),  ["Spells: Restricted"]  = (15,20),
                ["Subterfuge"]          = (2,4),    ["Technical"]           = (2,3),    ["Vocation"]            = (3,4),
            },
            ProfessionalSkills: new[]{
                "Perception","Metalcraft","Attunement","Spell Lore","Climbing",
                "Running","Architecture","Engineering","Influence","Trading",
                "Concealment","Stalking","Locks","Mechanics","Traps" }),

        new("Cleric",         ProfGroup.PureSpell,    Realm: "Channeling",
            Costs: new(){
                ["Animal"]              = (3,4),    ["Awareness"]           = (4,6),    ["Battle Expertise"]    = (5,7),
                ["Body Discipline"]     = (6,8),    ["Brawn"]               = (5,7),    ["Combat Expertise"]    = (7,10),
                ["Combat Training 1"]   = (5,7),    ["Combat Training 2"]   = (6,8),
                ["Combat Training 3"]   = (9,12),   ["Combat Training 4"]   = (15,20),  ["Composition"]         = (3,4),
                ["Crafting"]            = (3,4),    ["Delving"]             = (3,4),    ["Environmental"]       = (3,5),
                ["Gymnastic"]           = (4,6),    ["Lore"]                = (2,3),    ["Magical Expertise"]   = (2,3),
                ["Medical"]             = (2,4),    ["Mental Discipline"]   = (3,4),    ["Movement"]            = (3,5),
                ["Performance Art"]     = (2,3),    ["Power Manipulation"]  = (2,4),    ["Science"]             = (3,4),
                ["Social"]              = (1,3),    ["Spells: Base/Open"]   = (1,2),    ["Spells: Ritual Magic"]= (1,3),
                ["Spells: Closed"]      = (2,4),    ["Spells: Arcane"]      = (5,7),    ["Spells: Restricted"]  = (6,8),
                ["Subterfuge"]          = (6,8),    ["Technical"]           = (5,7),    ["Vocation"]            = (3,4),
            },
            ProfessionalSkills: new[]{
                "Attunement","Religion/Philosophy","Spell Lore","Grace","Medicine",
                "Meditation","Channeling","Power Development","Astronomy","Influence",
                "Leadership","Magical Ritual","Open Spell Lists","Closed Spell Lists","Base Spell Lists" }),

        new("Druid",          ProfGroup.PureSpell,    Realm: "Channeling",
            Costs: new(){
                ["Animal"]              = (1,2),    ["Awareness"]           = (3,4),    ["Battle Expertise"]    = (7,10),
                ["Body Discipline"]     = (6,8),    ["Brawn"]               = (5,7),    ["Combat Expertise"]    = (7,10),
                ["Combat Training 1"]   = (5,7),    ["Combat Training 2"]   = (6,8),
                ["Combat Training 3"]   = (9,12),   ["Combat Training 4"]   = (15,20),  ["Composition"]         = (3,4),
                ["Crafting"]            = (3,4),    ["Delving"]             = (4,6),    ["Environmental"]       = (1,3),
                ["Gymnastic"]           = (4,6),    ["Lore"]                = (2,3),    ["Magical Expertise"]   = (3,4),
                ["Medical"]             = (2,4),    ["Mental Discipline"]   = (3,4),    ["Movement"]            = (3,5),
                ["Performance Art"]     = (3,4),    ["Power Manipulation"]  = (2,4),    ["Science"]             = (3,5),
                ["Social"]              = (3,4),    ["Spells: Base/Open"]   = (1,2),    ["Spells: Ritual Magic"]= (1,3),
                ["Spells: Closed"]      = (2,4),    ["Spells: Arcane"]      = (5,7),    ["Spells: Restricted"]  = (6,8),
                ["Subterfuge"]          = (4,6),    ["Technical"]           = (5,7),    ["Vocation"]            = (3,4),
            },
            ProfessionalSkills: new[]{
                "Animal Handling","Perception","Tracking","Woodcraft","Navigation",
                "Survival","Creature Lore","Region Lore","Grace","Herbalism",
                "Power Development","Open Spell Lists","Closed Spell Lists","Base Spell Lists","Concealment" }),

        new("Magician",       ProfGroup.PureSpell,    Realm: "Essence",
            Costs: new(){
                ["Animal"]              = (3,5),    ["Awareness"]           = (4,6),    ["Battle Expertise"]    = (7,10),
                ["Body Discipline"]     = (6,8),    ["Brawn"]               = (6,8),    ["Combat Expertise"]    = (7,10),
                ["Combat Training 1"]   = (5,7),    ["Combat Training 2"]   = (6,8),
                ["Combat Training 3"]   = (9,12),   ["Combat Training 4"]   = (15,20),  ["Composition"]         = (2,4),
                ["Crafting"]            = (3,4),    ["Delving"]             = (1,2),    ["Environmental"]       = (4,6),
                ["Gymnastic"]           = (4,6),    ["Lore"]                = (1,3),    ["Magical Expertise"]   = (1,2),
                ["Medical"]             = (3,4),    ["Mental Discipline"]   = (2,4),    ["Movement"]            = (4,6),
                ["Performance Art"]     = (2,4),    ["Power Manipulation"]  = (2,3),    ["Science"]             = (2,4),
                ["Social"]              = (3,4),    ["Spells: Base/Open"]   = (1,2),    ["Spells: Ritual Magic"]= (1,3),
                ["Spells: Closed"]      = (2,4),    ["Spells: Arcane"]      = (5,7),    ["Spells: Restricted"]  = (6,8),
                ["Subterfuge"]          = (6,8),    ["Technical"]           = (5,7),    ["Vocation"]            = (3,4),
            },
            ProfessionalSkills: new[]{
                "Attunement","Runes","Spell Lore","Grace","Directed Spell",
                "Power Development","Power Projection","Architecture","Astronomy","Engineering",
                "Mathematics","Magical Ritual","Open Spell Lists","Closed Spell Lists","Base Spell Lists" }),

        new("Illusionist",    ProfGroup.PureSpell,    Realm: "Essence",
            Costs: new(){
                ["Animal"]              = (4,6),    ["Awareness"]           = (2,4),    ["Battle Expertise"]    = (7,10),
                ["Body Discipline"]     = (6,8),    ["Brawn"]               = (6,8),    ["Combat Expertise"]    = (7,10),
                ["Combat Training 1"]   = (6,8),    ["Combat Training 2"]   = (7,10),
                ["Combat Training 3"]   = (12,15),  ["Combat Training 4"]   = (15,20),  ["Composition"]         = (1,3),
                ["Crafting"]            = (3,4),    ["Delving"]             = (1,2),    ["Environmental"]       = (4,6),
                ["Gymnastic"]           = (4,6),    ["Lore"]                = (1,3),    ["Magical Expertise"]   = (1,2),
                ["Medical"]             = (4,6),    ["Mental Discipline"]   = (3,5),    ["Movement"]            = (4,6),
                ["Performance Art"]     = (1,3),    ["Power Manipulation"]  = (2,3),    ["Science"]             = (3,4),
                ["Social"]              = (3,4),    ["Spells: Base/Open"]   = (1,2),    ["Spells: Ritual Magic"]= (1,3),
                ["Spells: Closed"]      = (2,4),    ["Spells: Arcane"]      = (5,7),    ["Spells: Restricted"]  = (6,8),
                ["Subterfuge"]          = (4,6),    ["Technical"]           = (5,7),    ["Vocation"]            = (3,4),
            },
            ProfessionalSkills: new[]{
                "Perception","Illusion Crafting","Drawing/Painting","Attunement","Spell Lore",
                "Grace","Spell Trickery","Acting","Music","Stage Magic",
                "Power Development","Open Spell Lists","Closed Spell Lists","Base Spell Lists","Trickery" }),

        new("Mentalist",      ProfGroup.PureSpell,    Realm: "Mentalism",
            Costs: new(){
                ["Animal"]              = (4,6),    ["Awareness"]           = (3,5),    ["Battle Expertise"]    = (7,10),
                ["Body Discipline"]     = (4,6),    ["Brawn"]               = (6,8),    ["Combat Expertise"]    = (7,10),
                ["Combat Training 1"]   = (5,7),    ["Combat Training 2"]   = (6,8),
                ["Combat Training 3"]   = (9,12),   ["Combat Training 4"]   = (15,20),  ["Composition"]         = (3,4),
                ["Crafting"]            = (3,4),    ["Delving"]             = (2,4),    ["Environmental"]       = (4,6),
                ["Gymnastic"]           = (4,6),    ["Lore"]                = (2,3),    ["Magical Expertise"]   = (1,3),
                ["Medical"]             = (3,5),    ["Mental Discipline"]   = (1,2),    ["Movement"]            = (4,6),
                ["Performance Art"]     = (3,4),    ["Power Manipulation"]  = (2,3),    ["Science"]             = (3,4),
                ["Social"]              = (1,3),    ["Spells: Base/Open"]   = (1,2),    ["Spells: Ritual Magic"]= (1,3),
                ["Spells: Closed"]      = (2,4),    ["Spells: Arcane"]      = (5,7),    ["Spells: Restricted"]  = (6,8),
                ["Subterfuge"]          = (5,7),    ["Technical"]           = (5,7),    ["Vocation"]            = (3,4),
            },
            ProfessionalSkills: new[]{
                "Attunement","Language","Racial Lore","Grace","Meditation",
                "Mental Focus","Power Development","Influence","Leadership","Social Awareness",
                "Magical Ritual","Open Spell Lists","Closed Spell Lists","Base Spell Lists","Administration" }),

        new("Lay Healer",     ProfGroup.PureSpell,    Realm: "Mentalism",
            Costs: new(){
                ["Animal"]              = (3,4),    ["Awareness"]           = (4,6),    ["Battle Expertise"]    = (7,10),
                ["Body Discipline"]     = (4,6),    ["Brawn"]               = (5,7),    ["Combat Expertise"]    = (7,10),
                ["Combat Training 1"]   = (6,8),    ["Combat Training 2"]   = (7,10),
                ["Combat Training 3"]   = (12,15),  ["Combat Training 4"]   = (15,20),  ["Composition"]         = (3,4),
                ["Crafting"]            = (3,4),    ["Delving"]             = (2,3),    ["Environmental"]       = (4,6),
                ["Gymnastic"]           = (4,6),    ["Lore"]                = (1,3),    ["Magical Expertise"]   = (2,4),
                ["Medical"]             = (1,2),    ["Mental Discipline"]   = (2,4),    ["Movement"]            = (3,5),
                ["Performance Art"]     = (3,4),    ["Power Manipulation"]  = (2,3),    ["Science"]             = (3,4),
                ["Social"]              = (2,3),    ["Spells: Base/Open"]   = (1,2),    ["Spells: Ritual Magic"]= (1,3),
                ["Spells: Closed"]      = (2,4),    ["Spells: Arcane"]      = (5,7),    ["Spells: Restricted"]  = (6,8),
                ["Subterfuge"]          = (6,8),    ["Technical"]           = (5,7),    ["Vocation"]            = (3,4),
            },
            ProfessionalSkills: new[]{
                "Fabric Craft","Woodcraft","Attunement","Racial Lore","Grace",
                "Herbalism","Medicine","Mental Focus","Power Development","Influence",
                "Social Awareness","Magical Ritual","Open Spell Lists","Closed Spell Lists","Base Spell Lists" }),

        new("Healer",         ProfGroup.Hybrid,       Realm: "Channeling+Mentalism",
            Costs: new(){
                ["Animal"]              = (3,5),    ["Awareness"]           = (4,6),    ["Battle Expertise"]    = (7,10),
                ["Body Discipline"]     = (5,7),    ["Brawn"]               = (1,3),    ["Combat Expertise"]    = (7,10),
                ["Combat Training 1"]   = (5,7),    ["Combat Training 2"]   = (6,8),
                ["Combat Training 3"]   = (9,12),   ["Combat Training 4"]   = (15,20),  ["Composition"]         = (3,4),
                ["Crafting"]            = (3,4),    ["Delving"]             = (4,6),    ["Environmental"]       = (4,6),
                ["Gymnastic"]           = (4,6),    ["Lore"]                = (2,3),    ["Magical Expertise"]   = (2,3),
                ["Medical"]             = (1,2),    ["Mental Discipline"]   = (2,4),    ["Movement"]            = (4,6),
                ["Performance Art"]     = (3,4),    ["Power Manipulation"]  = (2,4),    ["Science"]             = (4,6),
                ["Social"]              = (2,4),    ["Spells: Base/Open"]   = (1,3),    ["Spells: Ritual Magic"]= (2,3),
                ["Spells: Closed"]      = (3,4),    ["Spells: Arcane"]      = (4,6),    ["Spells: Restricted"]  = (7,10),
                ["Subterfuge"]          = (6,8),    ["Technical"]           = (5,7),    ["Vocation"]            = (3,4),
            },
            ProfessionalSkills: new[]{
                "Body Development","Attunement","Racial Lore","Grace","Herbalism",
                "Medicine","Meditation","Mental Focus","Channeling","Power Development",
                "Social Awareness","Magical Ritual","Open Spell Lists","Closed Spell Lists","Base Spell Lists" }),

        new("Mystic",         ProfGroup.Hybrid,       Realm: "Essence+Mentalism",
            Costs: new(){
                ["Animal"]              = (4,6),    ["Awareness"]           = (2,4),    ["Battle Expertise"]    = (7,10),
                ["Body Discipline"]     = (6,8),    ["Brawn"]               = (6,8),    ["Combat Expertise"]    = (7,10),
                ["Combat Training 1"]   = (4,6),    ["Combat Training 2"]   = (5,7),
                ["Combat Training 3"]   = (7,10),   ["Combat Training 4"]   = (12,15),  ["Composition"]         = (3,4),
                ["Crafting"]            = (3,4),    ["Delving"]             = (3,5),    ["Environmental"]       = (4,6),
                ["Gymnastic"]           = (3,5),    ["Lore"]                = (2,3),    ["Magical Expertise"]   = (3,4),
                ["Medical"]             = (4,6),    ["Mental Discipline"]   = (2,4),    ["Movement"]            = (3,5),
                ["Performance Art"]     = (2,4),    ["Power Manipulation"]  = (2,4),    ["Science"]             = (4,6),
                ["Social"]              = (2,3),    ["Spells: Base/Open"]   = (1,3),    ["Spells: Ritual Magic"]= (2,3),
                ["Spells: Closed"]      = (3,4),    ["Spells: Arcane"]      = (4,6),    ["Spells: Restricted"]  = (7,10),
                ["Subterfuge"]          = (3,4),    ["Technical"]           = (3,5),    ["Vocation"]            = (3,4),
            },
            ProfessionalSkills: new[]{
                "Perception","Attunement","Spell Lore","Grace","Spell Trickery",
                "Meditation","Mental Focus","Acting","Directed Spell","Power Development",
                "Open Spell Lists","Closed Spell Lists","Base Spell Lists","Stalking","Trickery" }),

        new("Sorcerer",       ProfGroup.Hybrid,       Realm: "Essence+Channeling",
            Costs: new(){
                ["Animal"]              = (4,6),    ["Awareness"]           = (4,6),    ["Battle Expertise"]    = (7,10),
                ["Body Discipline"]     = (6,8),    ["Brawn"]               = (6,8),    ["Combat Expertise"]    = (7,10),
                ["Combat Training 1"]   = (6,8),    ["Combat Training 2"]   = (7,10),
                ["Combat Training 3"]   = (12,15),  ["Combat Training 4"]   = (15,20),  ["Composition"]         = (2,4),
                ["Crafting"]            = (3,4),    ["Delving"]             = (1,3),    ["Environmental"]       = (3,5),
                ["Gymnastic"]           = (4,6),    ["Lore"]                = (1,3),    ["Magical Expertise"]   = (1,2),
                ["Medical"]             = (2,4),    ["Mental Discipline"]   = (2,4),    ["Movement"]            = (4,6),
                ["Performance Art"]     = (3,4),    ["Power Manipulation"]  = (2,4),    ["Science"]             = (3,4),
                ["Social"]              = (2,3),    ["Spells: Base/Open"]   = (1,3),    ["Spells: Ritual Magic"]= (2,3),
                ["Spells: Closed"]      = (3,4),    ["Spells: Arcane"]      = (4,6),    ["Spells: Restricted"]  = (7,10),
                ["Subterfuge"]          = (6,8),    ["Technical"]           = (3,5),    ["Vocation"]            = (3,4),
            },
            ProfessionalSkills: new[]{
                "Attunement","Runes","Creature Lore","Materials Lore","Spell Lore",
                "Grace","Channeling","Directed Spell","Power Development","Architecture",
                "Engineering","Magical Ritual","Open Spell Lists","Closed Spell Lists","Base Spell Lists" }),
    };

    public static readonly IReadOnlyDictionary<string, ProfessionDef> ByName =
        All.ToDictionary(p => p.Name);

    // DP cost to buy a rank in a category for this profession.
    // rankNumber is 1-based (1 = first rank, 2 = second rank, 3+ = second-rank cost).
    public static int RankCost(ProfessionDef prof, string category, int rankNumber)
    {
        if (!prof.Costs.TryGetValue(category, out var c)) return 99;
        return rankNumber <= 1 ? c.First : c.Second;
    }
}

public enum ProfGroup { Arms, SemiSpell, PureSpell, Hybrid }

public record ProfessionDef(
    string Name,
    ProfGroup Group,
    string? Realm,
    Dictionary<string, (int First, int Second)> Costs,
    string[] ProfessionalSkills)
{
    public bool IsSpellcaster => Group != ProfGroup.Arms;
}
