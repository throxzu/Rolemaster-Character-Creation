namespace RolemasterCharacterCreation.Rules;

public static class ProfessionRules
{
    public static readonly IReadOnlyList<ProfessionDef> All = new List<ProfessionDef>
    {
        new("No Profession",  ProfGroup.Arms,         Realm: null,
            Costs: new(){
                ["Animal"]            = (2,4), ["Awareness"]          = (2,4), ["Battle Expertise"]  = (3,5),
                ["Body Discipline"]   = (4,6), ["Brawn"]              = (3,4), ["Combat Expertise"]  = (3,5),
                ["Combat Science"]    = (3,4), ["Combat Training 1"]  = (3,5), ["Combat Training 2"] = (5,7),
                ["Combat Training 3"] = (7,10),["Combat Training 4"]  = (2,4), ["Composition"]       = (2,4),
                ["Crafting"]          = (4,6), ["Delving"]            = (3,4), ["Environmental"]     = (3,4),
                ["Gymnastic"]         = (1,3), ["Lore"]               = (4,6), ["Magical Expertise"] = (3,4),
                ["Medical"]           = (3,4), ["Mental Discipline"]  = (2,4), ["Movement"]          = (2,4),
                ["Performance Art"]   = (4,6), ["Power Manipulation"] = (4,6), ["Science"]           = (2,4),
                ["Social"]            = (4,6), ["Spells: Base/Open"]  = (5,7), ["Spells: Ritual Magic"]=(7,10),
                ["Spells: Closed"]    =(15,20),["Spells: Arcane"]     =(15,20),["Spells: Restricted"] = (3,5),
                ["Subterfuge"]        = (3,5), ["Technical"]          = (3,4), ["Vocation"]          = (3,4),
            },
            ProfessionalSkills: new[]{ "Any 10 skills" }),

        new("Fighter",        ProfGroup.Arms,         Realm: null,
            Costs: new(){
                ["Animal"]            = (2,3), ["Awareness"]          = (2,4), ["Battle Expertise"]  = (1,2),
                ["Body Discipline"]   = (4,6), ["Brawn"]              = (1,3), ["Combat Expertise"]  = (1,3),
                ["Combat Science"]    = (1,2), ["Combat Training 1"]  = (1,2), ["Combat Training 2"] = (2,3),
                ["Combat Training 3"] = (3,4), ["Combat Training 4"]  = (3,4), ["Composition"]       = (2,4),
                ["Crafting"]          = (7,10),["Delving"]            = (2,4), ["Environmental"]     = (3,4),
                ["Gymnastic"]         = (2,4), ["Lore"]               = (7,10),["Magical Expertise"] = (3,4),
                ["Medical"]           = (5,7), ["Mental Discipline"]  = (2,4), ["Movement"]          = (3,4),
                ["Performance Art"]   = (9,12),["Power Manipulation"] = (5,7), ["Science"]           = (2,4),
                ["Social"]            = (7,10),["Spells: Base/Open"]  = (9,12),["Spells: Ritual Magic"]=(15,20),
                ["Spells: Closed"]    =(15,20),["Spells: Arcane"]     =(15,20),["Spells: Restricted"] = (3,5),
                ["Subterfuge"]        = (5,7), ["Technical"]          = (3,4), ["Vocation"]          = (3,4),
            },
            ProfessionalSkills: new[]{
                "Riding","Body Development","Fortitude","Weight-training","Maneuvering in Armor",
                "Mounted Combat","Protect","Disarm","Multiple Attacks","Melee Weapons",
                "Ranged Weapons","Shield","Metalcraft","Running","Leadership" }),

        new("Warrior Monk",   ProfGroup.Arms,         Realm: null,
            Costs: new(){
                ["Animal"]            = (3,5), ["Awareness"]          = (2,4), ["Battle Expertise"]  = (3,5),
                ["Body Discipline"]   = (1,3), ["Brawn"]              = (3,4), ["Combat Expertise"]  = (1,2),
                ["Combat Science"]    = (1,2), ["Combat Training 1"]  = (1,3), ["Combat Training 2"] = (2,4),
                ["Combat Training 3"] = (3,5), ["Combat Training 4"]  = (3,4), ["Composition"]       = (3,4),
                ["Crafting"]          = (7,10),["Delving"]            = (3,5), ["Environmental"]     = (1,2),
                ["Gymnastic"]         = (2,4), ["Lore"]               = (7,10),["Magical Expertise"] = (4,6),
                ["Medical"]           = (2,3), ["Mental Discipline"]  = (1,2), ["Movement"]          = (3,4),
                ["Performance Art"]   = (9,12),["Power Manipulation"] = (5,7), ["Science"]           = (3,4),
                ["Social"]            = (7,10),["Spells: Base/Open"]  = (9,12),["Spells: Ritual Magic"]=(15,20),
                ["Spells: Closed"]    =(15,20),["Spells: Arcane"]     =(15,20),["Spells: Restricted"] = (2,4),
                ["Subterfuge"]        = (5,7), ["Technical"]          = (3,4), ["Vocation"]          = (3,4),
            },
            ProfessionalSkills: new[]{
                "Adrenal Defense","Adrenal Speed","Adrenal Strength","Body Development","Blind Fighting",
                "Multiple Attacks","Reverse Strike","Subduing","Unarmed","Acrobatics",
                "Jumping","Meditation","Climbing","Running","Swimming" }),

        new("Thief",          ProfGroup.Arms,         Realm: null,
            Costs: new(){
                ["Animal"]            = (2,3), ["Awareness"]          = (1,2), ["Battle Expertise"]  = (3,5),
                ["Body Discipline"]   = (6,8), ["Brawn"]              = (4,6), ["Combat Expertise"]  = (3,5),
                ["Combat Science"]    = (2,4), ["Combat Training 1"]  = (3,4), ["Combat Training 2"] = (4,6),
                ["Combat Training 3"] = (6,8), ["Combat Training 4"]  = (3,4), ["Composition"]       = (2,3),
                ["Crafting"]          = (5,7), ["Delving"]            = (2,4), ["Environmental"]     = (2,3),
                ["Gymnastic"]         = (2,4), ["Lore"]               = (7,10),["Magical Expertise"] = (3,4),
                ["Medical"]           = (4,6), ["Mental Discipline"]  = (1,3), ["Movement"]          = (2,3),
                ["Performance Art"]   = (9,12),["Power Manipulation"] = (4,6), ["Science"]           = (1,3),
                ["Social"]            = (7,10),["Spells: Base/Open"]  = (9,12),["Spells: Ritual Magic"]=(15,20),
                ["Spells: Closed"]    =(15,20),["Spells: Arcane"]     =(15,20),["Spells: Restricted"] = (1,2),
                ["Subterfuge"]        = (1,2), ["Technical"]          = (3,4), ["Vocation"]          = (3,4),
            },
            ProfessionalSkills: new[]{
                "Perception","Melee Weapons","Ranged Weapons","Acrobatics","Contortions",
                "Climbing","Running","Influence","Social Awareness","Trading",
                "Concealment","Stalking","Trickery","Locks","Traps" }),

        new("Rogue",          ProfGroup.Arms,         Realm: null,
            Costs: new(){
                ["Animal"]            = (2,4), ["Awareness"]          = (2,3), ["Battle Expertise"]  = (3,4),
                ["Body Discipline"]   = (4,6), ["Brawn"]              = (2,4), ["Combat Expertise"]  = (3,4),
                ["Combat Science"]    = (1,3), ["Combat Training 1"]  = (2,3), ["Combat Training 2"] = (3,4),
                ["Combat Training 3"] = (4,6), ["Combat Training 4"]  = (3,4), ["Composition"]       = (2,4),
                ["Crafting"]          = (7,10),["Delving"]            = (2,4), ["Environmental"]     = (2,3),
                ["Gymnastic"]         = (2,4), ["Lore"]               = (7,10),["Magical Expertise"] = (3,4),
                ["Medical"]           = (5,7), ["Mental Discipline"]  = (1,3), ["Movement"]          = (2,4),
                ["Performance Art"]   = (9,12),["Power Manipulation"] = (5,7), ["Science"]           = (2,3),
                ["Social"]            = (7,10),["Spells: Base/Open"]  = (9,12),["Spells: Ritual Magic"]=(15,20),
                ["Spells: Closed"]    =(15,20),["Spells: Arcane"]     =(15,20),["Spells: Restricted"] = (1,3),
                ["Subterfuge"]        = (2,3), ["Technical"]          = (3,4), ["Vocation"]          = (3,4),
            },
            ProfessionalSkills: new[]{
                "Riding","Perception","Body Development","Maneuvering in Armor","Restricted Quarters",
                "Blind Fighting","Melee Weapons","Ranged Weapons","Survival","Jumping",
                "Poison Mastery","Climbing","Running","Ambush","Stalking" }),

        new("Laborer",        ProfGroup.Arms,         Realm: null,
            Costs: new(){
                ["Animal"]            = (1,3), ["Awareness"]          = (3,4), ["Battle Expertise"]  = (4,6),
                ["Body Discipline"]   = (6,8), ["Brawn"]              = (1,2), ["Combat Expertise"]  = (4,6),
                ["Combat Science"]    = (2,3), ["Combat Training 1"]  = (2,4), ["Combat Training 2"] = (3,5),
                ["Combat Training 3"] = (5,7), ["Combat Training 4"]  = (3,4), ["Composition"]       = (1,3),
                ["Crafting"]          = (7,10),["Delving"]            = (1,3), ["Environmental"]     = (2,3),
                ["Gymnastic"]         = (2,3), ["Lore"]               = (7,10),["Magical Expertise"] = (2,4),
                ["Medical"]           = (4,6), ["Mental Discipline"]  = (1,3), ["Movement"]          = (2,3),
                ["Performance Art"]   = (9,12),["Power Manipulation"] = (4,6), ["Science"]           = (2,3),
                ["Social"]            = (7,10),["Spells: Base/Open"]  = (9,12),["Spells: Ritual Magic"]=(15,20),
                ["Spells: Closed"]    =(15,20),["Spells: Arcane"]     =(15,20),["Spells: Restricted"] = (3,4),
                ["Subterfuge"]        = (2,3), ["Technical"]          = (3,4), ["Vocation"]          = (3,4),
            },
            ProfessionalSkills: new[]{
                "Animal Handling","Body Development","Weight-training","Culinary","Drawing/Painting",
                "Fabric Craft","Leathercraft","Metalcraft","Stonecraft","Woodcraft",
                "Piloting","Running","Mechanics","Service","Trade" }),

        new("Scholar",        ProfGroup.Arms,         Realm: null,
            Costs: new(){
                ["Animal"]            = (3,4), ["Awareness"]          = (1,3), ["Battle Expertise"]  = (7,10),
                ["Body Discipline"]   = (6,8), ["Brawn"]              = (6,8), ["Combat Expertise"]  = (7,10),
                ["Combat Science"]    = (5,7), ["Combat Training 1"]  = (6,8), ["Combat Training 2"] = (9,12),
                ["Combat Training 3"] =(15,20),["Combat Training 4"]  = (1,2), ["Composition"]       = (2,3),
                ["Crafting"]          = (2,3), ["Delving"]            = (2,4), ["Environmental"]     = (3,5),
                ["Gymnastic"]         = (1,2), ["Lore"]               = (4,6), ["Magical Expertise"] = (1,3),
                ["Medical"]           = (1,3), ["Mental Discipline"]  = (3,5), ["Movement"]          = (1,3),
                ["Performance Art"]   = (4,6), ["Power Manipulation"] = (1,3), ["Science"]           = (2,3),
                ["Social"]            = (4,6), ["Spells: Base/Open"]  = (5,7), ["Spells: Ritual Magic"]=(7,10),
                ["Spells: Closed"]    =(15,20),["Spells: Arcane"]     =(15,20),["Spells: Restricted"] = (4,6),
                ["Subterfuge"]        = (2,4), ["Technical"]          = (3,4), ["Vocation"]          = (3,4),
            },
            ProfessionalSkills: new[]{
                "Writing","Creature Lore","Historic Lore","Language","Materials Lore",
                "Racial Lore","Region Lore","Religion/Philosophy","Architecture","Astronomy",
                "Engineering","Mathematics","Administration","Service","Trade" }),

        new("Ranger",         ProfGroup.SemiSpell,    Realm: "Channeling",
            Costs: new(){
                ["Animal"]            = (1,3), ["Awareness"]          = (1,3), ["Battle Expertise"]  = (4,6),
                ["Body Discipline"]   = (6,8), ["Brawn"]              = (4,6), ["Combat Expertise"]  = (3,5),
                ["Combat Science"]    = (3,4), ["Combat Training 1"]  = (3,5), ["Combat Training 2"] = (5,7),
                ["Combat Training 3"] = (7,10),["Combat Training 4"]  = (3,4), ["Composition"]       = (2,4),
                ["Crafting"]          = (6,8), ["Delving"]            = (1,3), ["Environmental"]     = (3,4),
                ["Gymnastic"]         = (2,4), ["Lore"]               = (6,8), ["Magical Expertise"] = (3,5),
                ["Medical"]           = (5,7), ["Mental Discipline"]  = (1,3), ["Movement"]          = (3,4),
                ["Performance Art"]   = (4,6), ["Power Manipulation"] = (5,7), ["Science"]           = (3,4),
                ["Social"]            = (3,4), ["Spells: Base/Open"]  = (3,5), ["Spells: Ritual Magic"]=(5,7),
                ["Spells: Closed"]    =(12,15),["Spells: Arcane"]     =(15,20),["Spells: Restricted"] = (2,4),
                ["Subterfuge"]        = (5,7), ["Technical"]          = (3,4), ["Vocation"]          = (3,4),
            },
            ProfessionalSkills: new[]{
                "Animal Handling","Riding","Perception","Tracking","Melee Weapons",
                "Ranged Weapons","Navigation","Survival","Creature Lore","Region Lore",
                "Climbing","Running","Swimming","Concealment","Stalking" }),

        new("Paladin",        ProfGroup.SemiSpell,    Realm: "Channeling",
            Costs: new(){
                ["Animal"]            = (2,3), ["Awareness"]          = (3,5), ["Battle Expertise"]  = (2,3),
                ["Body Discipline"]   = (5,7), ["Brawn"]              = (2,4), ["Combat Expertise"]  = (3,5),
                ["Combat Science"]    = (2,4), ["Combat Training 1"]  = (3,4), ["Combat Training 2"] = (4,6),
                ["Combat Training 3"] = (6,8), ["Combat Training 4"]  = (3,4), ["Composition"]       = (2,4),
                ["Crafting"]          = (6,8), ["Delving"]            = (3,5), ["Environmental"]     = (2,4),
                ["Gymnastic"]         = (2,4), ["Lore"]               = (3,5), ["Magical Expertise"] = (4,6),
                ["Medical"]           = (5,7), ["Mental Discipline"]  = (2,3), ["Movement"]          = (3,4),
                ["Performance Art"]   = (4,6), ["Power Manipulation"] = (5,7), ["Science"]           = (2,3),
                ["Social"]            = (3,5), ["Spells: Base/Open"]  = (4,6), ["Spells: Ritual Magic"]=(6,8),
                ["Spells: Closed"]    =(15,20),["Spells: Arcane"]     =(15,20),["Spells: Restricted"] = (4,6),
                ["Subterfuge"]        = (5,7), ["Technical"]          = (3,4), ["Vocation"]          = (3,4),
            },
            ProfessionalSkills: new[]{
                "Riding","Body Development","Maneuvering in Armor","Mounted Combat","Protect",
                "Melee Weapons","Shield","Metalcraft","Religion/Philosophy","Transcendence",
                "Mental Focus","Running","Channeling","Influence","Leadership" }),

        new("Monk",           ProfGroup.SemiSpell,    Realm: "Mentalism",
            Costs: new(){
                ["Animal"]            = (3,5), ["Awareness"]          = (3,4), ["Battle Expertise"]  = (6,8),
                ["Body Discipline"]   = (2,4), ["Brawn"]              = (4,6), ["Combat Expertise"]  = (2,4),
                ["Combat Science"]    = (2,4), ["Combat Training 1"]  = (3,4), ["Combat Training 2"] = (4,6),
                ["Combat Training 3"] = (6,8), ["Combat Training 4"]  = (3,4), ["Composition"]       = (3,4),
                ["Crafting"]          = (5,7), ["Delving"]            = (2,4), ["Environmental"]     = (2,3),
                ["Gymnastic"]         = (2,3), ["Lore"]               = (6,8), ["Magical Expertise"] = (3,4),
                ["Medical"]           = (4,6), ["Mental Discipline"]  = (1,3), ["Movement"]          = (3,4),
                ["Performance Art"]   = (4,6), ["Power Manipulation"] = (5,7), ["Science"]           = (3,4),
                ["Social"]            = (3,5), ["Spells: Base/Open"]  = (4,6), ["Spells: Ritual Magic"]=(6,8),
                ["Spells: Closed"]    =(15,20),["Spells: Arcane"]     =(15,20),["Spells: Restricted"] = (3,5),
                ["Subterfuge"]        = (4,6), ["Technical"]          = (3,4), ["Vocation"]          = (3,4),
            },
            ProfessionalSkills: new[]{
                "Perception","Adrenal Defense","Adrenal Focus","Adrenal Speed","Adrenal Strength",
                "Body Development","Multiple Attacks","Unarmed","Acrobatics","Jumping",
                "Meditation","Mental Focus","Climbing","Running","Swimming" }),

        new("Magent",         ProfGroup.SemiSpell,    Realm: "Mentalism",
            Costs: new(){
                ["Animal"]            = (3,5), ["Awareness"]          = (2,3), ["Battle Expertise"]  = (3,5),
                ["Body Discipline"]   = (4,6), ["Brawn"]              = (5,7), ["Combat Expertise"]  = (3,5),
                ["Combat Science"]    = (3,4), ["Combat Training 1"]  = (3,5), ["Combat Training 2"] = (5,7),
                ["Combat Training 3"] = (7,10),["Combat Training 4"]  = (3,4), ["Composition"]       = (3,4),
                ["Crafting"]          = (6,8), ["Delving"]            = (3,5), ["Environmental"]     = (2,4),
                ["Gymnastic"]         = (2,4), ["Lore"]               = (5,7), ["Magical Expertise"] = (3,5),
                ["Medical"]           = (3,5), ["Mental Discipline"]  = (2,3), ["Movement"]          = (3,4),
                ["Performance Art"]   = (3,5), ["Power Manipulation"] = (5,7), ["Science"]           = (2,3),
                ["Social"]            = (3,4), ["Spells: Base/Open"]  = (3,5), ["Spells: Ritual Magic"]=(5,7),
                ["Spells: Closed"]    =(12,15),["Spells: Arcane"]     =(15,20),["Spells: Restricted"] = (2,3),
                ["Subterfuge"]        = (4,6), ["Technical"]          = (3,4), ["Vocation"]          = (3,4),
            },
            ProfessionalSkills: new[]{
                "Perception","Melee Weapons","Ranged Weapons","Acrobatics","Contortions",
                "Jumping","Poison Mastery","Mental Focus","Climbing","Running",
                "Swimming","Ambush","Concealment","Stalking","Trickery" }),

        new("Bard",           ProfGroup.SemiSpell,    Realm: "Essence",
            Costs: new(){
                ["Animal"]            = (3,5), ["Awareness"]          = (2,4), ["Battle Expertise"]  = (4,6),
                ["Body Discipline"]   = (6,8), ["Brawn"]              = (5,7), ["Combat Expertise"]  = (4,6),
                ["Combat Science"]    = (3,5), ["Combat Training 1"]  = (4,6), ["Combat Training 2"] = (6,8),
                ["Combat Training 3"] = (9,12),["Combat Training 4"]  = (1,3), ["Composition"]       = (2,4),
                ["Crafting"]          = (3,5), ["Delving"]            = (3,5), ["Environmental"]     = (3,5),
                ["Gymnastic"]         = (1,2), ["Lore"]               = (4,6), ["Magical Expertise"] = (3,5),
                ["Medical"]           = (5,7), ["Mental Discipline"]  = (3,5), ["Movement"]          = (1,3),
                ["Performance Art"]   = (3,5), ["Power Manipulation"] = (3,5), ["Science"]           = (2,3),
                ["Social"]            = (2,4), ["Spells: Base/Open"]  = (3,4), ["Spells: Ritual Magic"]=(4,6),
                ["Spells: Closed"]    = (9,12),["Spells: Arcane"]     =(12,15),["Spells: Restricted"] = (4,6),
                ["Subterfuge"]        = (4,6), ["Technical"]          = (3,4), ["Vocation"]          = (3,4),
            },
            ProfessionalSkills: new[]{
                "Musical Composition","Attunement","Historic Lore","Language","Racial Lore",
                "Region Lore","Religion/Philosophy","Spell Lore","Spell Trickery","Running",
                "Acting","Music","Influence","Social Awareness","Trading" }),

        new("Dabbler",        ProfGroup.SemiSpell,    Realm: "Essence",
            Costs: new(){
                ["Animal"]            = (4,6), ["Awareness"]          = (1,3), ["Battle Expertise"]  = (5,7),
                ["Body Discipline"]   = (6,8), ["Brawn"]              = (5,7), ["Combat Expertise"]  = (4,6),
                ["Combat Science"]    = (3,5), ["Combat Training 1"]  = (4,6), ["Combat Training 2"] = (6,8),
                ["Combat Training 3"] = (9,12),["Combat Training 4"]  = (2,4), ["Composition"]       = (3,4),
                ["Crafting"]          = (2,3), ["Delving"]            = (3,5), ["Environmental"]     = (3,5),
                ["Gymnastic"]         = (2,3), ["Lore"]               = (3,5), ["Magical Expertise"] = (3,5),
                ["Medical"]           = (5,7), ["Mental Discipline"]  = (3,4), ["Movement"]          = (2,4),
                ["Performance Art"]   = (3,5), ["Power Manipulation"] = (5,7), ["Science"]           = (2,3),
                ["Social"]            = (3,4), ["Spells: Base/Open"]  = (3,5), ["Spells: Ritual Magic"]=(5,7),
                ["Spells: Closed"]    =(12,15),["Spells: Arcane"]     =(15,20),["Spells: Restricted"] = (2,4),
                ["Subterfuge"]        = (2,3), ["Technical"]          = (3,4), ["Vocation"]          = (3,4),
            },
            ProfessionalSkills: new[]{
                "Perception","Metalcraft","Attunement","Spell Lore","Climbing",
                "Running","Architecture","Engineering","Influence","Trading",
                "Concealment","Stalking","Locks","Mechanics","Traps" }),

        new("Cleric",         ProfGroup.PureSpell,    Realm: "Channeling",
            Costs: new(){
                ["Animal"]            = (3,4), ["Awareness"]          = (4,6), ["Battle Expertise"]  = (5,7),
                ["Body Discipline"]   = (6,8), ["Brawn"]              = (5,7), ["Combat Expertise"]  = (7,10),
                ["Combat Science"]    = (5,7), ["Combat Training 1"]  = (6,8), ["Combat Training 2"] = (9,12),
                ["Combat Training 3"] =(15,20),["Combat Training 4"]  = (3,4), ["Composition"]       = (3,4),
                ["Crafting"]          = (3,4), ["Delving"]            = (3,5), ["Environmental"]     = (4,6),
                ["Gymnastic"]         = (2,3), ["Lore"]               = (2,3), ["Magical Expertise"] = (2,4),
                ["Medical"]           = (3,4), ["Mental Discipline"]  = (3,5), ["Movement"]          = (2,3),
                ["Performance Art"]   = (2,4), ["Power Manipulation"] = (3,4), ["Science"]           = (1,3),
                ["Social"]            = (1,2), ["Spells: Base/Open"]  = (1,3), ["Spells: Ritual Magic"]=(2,4),
                ["Spells: Closed"]    = (5,7), ["Spells: Arcane"]     = (6,8), ["Spells: Restricted"] = (6,8),
                ["Subterfuge"]        = (5,7), ["Technical"]          = (3,4), ["Vocation"]          = (3,4),
            },
            ProfessionalSkills: new[]{
                "Attunement","Religion/Philosophy","Spell Lore","Grace","Medicine",
                "Meditation","Channeling","Power Development","Astronomy","Influence",
                "Leadership","Magical Ritual","Open Spell Lists","Closed Spell Lists","Base Spell Lists" }),

        new("Druid",          ProfGroup.PureSpell,    Realm: "Channeling",
            Costs: new(){
                ["Animal"]            = (1,2), ["Awareness"]          = (3,4), ["Battle Expertise"]  = (7,10),
                ["Body Discipline"]   = (6,8), ["Brawn"]              = (5,7), ["Combat Expertise"]  = (7,10),
                ["Combat Science"]    = (5,7), ["Combat Training 1"]  = (6,8), ["Combat Training 2"] = (9,12),
                ["Combat Training 3"] =(15,20),["Combat Training 4"]  = (3,4), ["Composition"]       = (3,4),
                ["Crafting"]          = (4,6), ["Delving"]            = (1,3), ["Environmental"]     = (4,6),
                ["Gymnastic"]         = (2,3), ["Lore"]               = (3,4), ["Magical Expertise"] = (2,4),
                ["Medical"]           = (3,4), ["Mental Discipline"]  = (3,5), ["Movement"]          = (3,4),
                ["Performance Art"]   = (2,4), ["Power Manipulation"] = (3,5), ["Science"]           = (3,4),
                ["Social"]            = (1,2), ["Spells: Base/Open"]  = (1,3), ["Spells: Ritual Magic"]=(2,4),
                ["Spells: Closed"]    = (5,7), ["Spells: Arcane"]     = (6,8), ["Spells: Restricted"] = (4,6),
                ["Subterfuge"]        = (5,7), ["Technical"]          = (3,4), ["Vocation"]          = (3,4),
            },
            ProfessionalSkills: new[]{
                "Animal Handling","Perception","Tracking","Woodcraft","Navigation",
                "Survival","Creature Lore","Region Lore","Grace","Herbalism",
                "Power Development","Open Spell Lists","Closed Spell Lists","Base Spell Lists","Concealment" }),

        new("Magician",       ProfGroup.PureSpell,    Realm: "Essence",
            Costs: new(){
                ["Animal"]            = (3,5), ["Awareness"]          = (4,6), ["Battle Expertise"]  = (7,10),
                ["Body Discipline"]   = (6,8), ["Brawn"]              = (6,8), ["Combat Expertise"]  = (7,10),
                ["Combat Science"]    = (5,7), ["Combat Training 1"]  = (6,8), ["Combat Training 2"] = (9,12),
                ["Combat Training 3"] =(15,20),["Combat Training 4"]  = (2,4), ["Composition"]       = (3,4),
                ["Crafting"]          = (1,2), ["Delving"]            = (4,6), ["Environmental"]     = (4,6),
                ["Gymnastic"]         = (1,3), ["Lore"]               = (1,2), ["Magical Expertise"] = (3,4),
                ["Medical"]           = (2,4), ["Mental Discipline"]  = (4,6), ["Movement"]          = (2,4),
                ["Performance Art"]   = (2,3), ["Power Manipulation"] = (2,4), ["Science"]           = (3,4),
                ["Social"]            = (1,2), ["Spells: Base/Open"]  = (1,3), ["Spells: Ritual Magic"]=(2,4),
                ["Spells: Closed"]    = (5,7), ["Spells: Arcane"]     = (6,8), ["Spells: Restricted"] = (6,8),
                ["Subterfuge"]        = (5,7), ["Technical"]          = (3,4), ["Vocation"]          = (3,4),
            },
            ProfessionalSkills: new[]{
                "Attunement","Runes","Spell Lore","Grace","Directed Spell",
                "Power Development","Power Projection","Architecture","Astronomy","Engineering",
                "Mathematics","Magical Ritual","Open Spell Lists","Closed Spell Lists","Base Spell Lists" }),

        new("Illusionist",    ProfGroup.PureSpell,    Realm: "Essence",
            Costs: new(){
                ["Animal"]            = (4,6), ["Awareness"]          = (2,4), ["Battle Expertise"]  = (7,10),
                ["Body Discipline"]   = (6,8), ["Brawn"]              = (6,8), ["Combat Expertise"]  = (7,10),
                ["Combat Science"]    = (6,8), ["Combat Training 1"]  = (7,10), ["Combat Training 2"] =(12,15),
                ["Combat Training 3"] =(15,20),["Combat Training 4"]  = (1,3), ["Composition"]       = (3,4),
                ["Crafting"]          = (1,2), ["Delving"]            = (4,6), ["Environmental"]     = (4,6),
                ["Gymnastic"]         = (1,3), ["Lore"]               = (1,2), ["Magical Expertise"] = (4,6),
                ["Medical"]           = (3,5), ["Mental Discipline"]  = (4,6), ["Movement"]          = (1,3),
                ["Performance Art"]   = (2,3), ["Power Manipulation"] = (3,4), ["Science"]           = (3,4),
                ["Social"]            = (1,2), ["Spells: Base/Open"]  = (1,3), ["Spells: Ritual Magic"]=(2,4),
                ["Spells: Closed"]    = (5,7), ["Spells: Arcane"]     = (6,8), ["Spells: Restricted"] = (4,6),
                ["Subterfuge"]        = (5,7), ["Technical"]          = (3,4), ["Vocation"]          = (3,4),
            },
            ProfessionalSkills: new[]{
                "Perception","Illusion Crafting","Drawing/Painting","Attunement","Spell Lore",
                "Grace","Spell Trickery","Acting","Music","Stage Magic",
                "Power Development","Open Spell Lists","Closed Spell Lists","Base Spell Lists","Trickery" }),

        new("Mentalist",      ProfGroup.PureSpell,    Realm: "Mentalism",
            Costs: new(){
                ["Animal"]            = (4,6), ["Awareness"]          = (3,5), ["Battle Expertise"]  = (7,10),
                ["Body Discipline"]   = (4,6), ["Brawn"]              = (6,8), ["Combat Expertise"]  = (7,10),
                ["Combat Science"]    = (5,7), ["Combat Training 1"]  = (6,8), ["Combat Training 2"] = (9,12),
                ["Combat Training 3"] =(15,20),["Combat Training 4"]  = (3,4), ["Composition"]       = (3,4),
                ["Crafting"]          = (2,4), ["Delving"]            = (4,6), ["Environmental"]     = (4,6),
                ["Gymnastic"]         = (2,3), ["Lore"]               = (1,3), ["Magical Expertise"] = (3,5),
                ["Medical"]           = (1,2), ["Mental Discipline"]  = (4,6), ["Movement"]          = (3,4),
                ["Performance Art"]   = (2,3), ["Power Manipulation"] = (3,4), ["Science"]           = (1,3),
                ["Social"]            = (1,2), ["Spells: Base/Open"]  = (1,3), ["Spells: Ritual Magic"]=(2,4),
                ["Spells: Closed"]    = (5,7), ["Spells: Arcane"]     = (6,8), ["Spells: Restricted"] = (5,7),
                ["Subterfuge"]        = (5,7), ["Technical"]          = (3,4), ["Vocation"]          = (3,4),
            },
            ProfessionalSkills: new[]{
                "Attunement","Language","Racial Lore","Grace","Meditation",
                "Mental Focus","Power Development","Influence","Leadership","Social Awareness",
                "Magical Ritual","Open Spell Lists","Closed Spell Lists","Base Spell Lists","Administration" }),

        new("Lay Healer",     ProfGroup.PureSpell,    Realm: "Mentalism",
            Costs: new(){
                ["Animal"]            = (3,4), ["Awareness"]          = (4,6), ["Battle Expertise"]  = (7,10),
                ["Body Discipline"]   = (4,6), ["Brawn"]              = (5,7), ["Combat Expertise"]  = (7,10),
                ["Combat Science"]    = (6,8), ["Combat Training 1"]  = (7,10), ["Combat Training 2"] =(12,15),
                ["Combat Training 3"] =(15,20),["Combat Training 4"]  = (3,4), ["Composition"]       = (3,4),
                ["Crafting"]          = (2,3), ["Delving"]            = (4,6), ["Environmental"]     = (4,6),
                ["Gymnastic"]         = (1,3), ["Lore"]               = (2,4), ["Magical Expertise"] = (1,2),
                ["Medical"]           = (2,4), ["Mental Discipline"]  = (3,5), ["Movement"]          = (3,4),
                ["Performance Art"]   = (2,3), ["Power Manipulation"] = (3,4), ["Science"]           = (2,3),
                ["Social"]            = (1,2), ["Spells: Base/Open"]  = (1,3), ["Spells: Ritual Magic"]=(2,4),
                ["Spells: Closed"]    = (5,7), ["Spells: Arcane"]     = (6,8), ["Spells: Restricted"] = (6,8),
                ["Subterfuge"]        = (5,7), ["Technical"]          = (3,4), ["Vocation"]          = (3,4),
            },
            ProfessionalSkills: new[]{
                "Fabric Craft","Woodcraft","Attunement","Racial Lore","Grace",
                "Herbalism","Medicine","Mental Focus","Power Development","Influence",
                "Social Awareness","Magical Ritual","Open Spell Lists","Closed Spell Lists","Base Spell Lists" }),

        new("Healer",         ProfGroup.Hybrid,       Realm: "Channeling+Mentalism",
            Costs: new(){
                ["Animal"]            = (3,5), ["Awareness"]          = (4,6), ["Battle Expertise"]  = (7,10),
                ["Body Discipline"]   = (5,7), ["Brawn"]              = (1,3), ["Combat Expertise"]  = (7,10),
                ["Combat Science"]    = (5,7), ["Combat Training 1"]  = (6,8), ["Combat Training 2"] = (9,12),
                ["Combat Training 3"] =(15,20),["Combat Training 4"]  = (3,4), ["Composition"]       = (3,4),
                ["Crafting"]          = (4,6), ["Delving"]            = (4,6), ["Environmental"]     = (4,6),
                ["Gymnastic"]         = (2,3), ["Lore"]               = (2,3), ["Magical Expertise"] = (1,2),
                ["Medical"]           = (2,4), ["Mental Discipline"]  = (4,6), ["Movement"]          = (3,4),
                ["Performance Art"]   = (2,4), ["Power Manipulation"] = (4,6), ["Science"]           = (2,4),
                ["Social"]            = (1,3), ["Spells: Base/Open"]  = (2,3), ["Spells: Ritual Magic"]=(3,4),
                ["Spells: Closed"]    = (4,6), ["Spells: Arcane"]     = (7,10), ["Spells: Restricted"] = (6,8),
                ["Subterfuge"]        = (5,7), ["Technical"]          = (3,4), ["Vocation"]          = (3,4),
            },
            ProfessionalSkills: new[]{
                "Body Development","Attunement","Racial Lore","Grace","Herbalism",
                "Medicine","Meditation","Mental Focus","Channeling","Power Development",
                "Social Awareness","Magical Ritual","Open Spell Lists","Closed Spell Lists","Base Spell Lists" }),

        new("Mystic",         ProfGroup.Hybrid,       Realm: "Essence+Mentalism",
            Costs: new(){
                ["Animal"]            = (4,6), ["Awareness"]          = (2,4), ["Battle Expertise"]  = (7,10),
                ["Body Discipline"]   = (6,8), ["Brawn"]              = (6,8), ["Combat Expertise"]  = (7,10),
                ["Combat Science"]    = (4,6), ["Combat Training 1"]  = (5,7), ["Combat Training 2"] = (7,10),
                ["Combat Training 3"] =(12,15),["Combat Training 4"]  = (3,4), ["Composition"]       = (3,4),
                ["Crafting"]          = (3,5), ["Delving"]            = (4,6), ["Environmental"]     = (3,5),
                ["Gymnastic"]         = (2,3), ["Lore"]               = (3,4), ["Magical Expertise"] = (4,6),
                ["Medical"]           = (2,4), ["Mental Discipline"]  = (3,5), ["Movement"]          = (2,4),
                ["Performance Art"]   = (2,4), ["Power Manipulation"] = (4,6), ["Science"]           = (2,3),
                ["Social"]            = (1,3), ["Spells: Base/Open"]  = (2,3), ["Spells: Ritual Magic"]=(3,4),
                ["Spells: Closed"]    = (4,6), ["Spells: Arcane"]     = (7,10), ["Spells: Restricted"] = (3,4),
                ["Subterfuge"]        = (3,5), ["Technical"]          = (3,4), ["Vocation"]          = (3,4),
            },
            ProfessionalSkills: new[]{
                "Perception","Attunement","Spell Lore","Grace","Spell Trickery",
                "Meditation","Mental Focus","Acting","Directed Spell","Power Development",
                "Open Spell Lists","Closed Spell Lists","Base Spell Lists","Stalking","Trickery" }),

        new("Sorcerer",       ProfGroup.Hybrid,       Realm: "Essence+Channeling",
            Costs: new(){
                ["Animal"]            = (4,6), ["Awareness"]          = (4,6), ["Battle Expertise"]  = (7,10),
                ["Body Discipline"]   = (6,8), ["Brawn"]              = (6,8), ["Combat Expertise"]  = (7,10),
                ["Combat Science"]    = (6,8), ["Combat Training 1"]  = (7,10), ["Combat Training 2"] =(12,15),
                ["Combat Training 3"] =(15,20),["Combat Training 4"]  = (2,4), ["Composition"]       = (3,4),
                ["Crafting"]          = (1,3), ["Delving"]            = (3,5), ["Environmental"]     = (4,6),
                ["Gymnastic"]         = (1,3), ["Lore"]               = (1,2), ["Magical Expertise"] = (2,4),
                ["Medical"]           = (2,4), ["Mental Discipline"]  = (4,6), ["Movement"]          = (3,4),
                ["Performance Art"]   = (2,4), ["Power Manipulation"] = (3,4), ["Science"]           = (2,3),
                ["Social"]            = (1,3), ["Spells: Base/Open"]  = (2,3), ["Spells: Ritual Magic"]=(3,4),
                ["Spells: Closed"]    = (4,6), ["Spells: Arcane"]     = (7,10), ["Spells: Restricted"] = (6,8),
                ["Subterfuge"]        = (3,5), ["Technical"]          = (3,4), ["Vocation"]          = (3,4),
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
