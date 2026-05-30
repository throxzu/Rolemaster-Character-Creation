namespace RolemasterCharacterCreation.Rules;

// Starting skill ranks granted by culture (Table 2-3).
// Skills marked with * allow player to choose a specialization.
// ⚠ entries were ambiguous in the PDF; values kept as-is from skills.md.
public static class CultureRules
{
    public static readonly IReadOnlyList<CultureDef> All = new List<CultureDef>
    {
        new("Cosmopolitan", new[]
        {
            new CultureSkill("Perception",              2),
            new CultureSkill("Tracking",                1),
            new CultureSkill("Body Development",        1),
            new CultureSkill("Unarmed",                 1, Specialized: true),
            new CultureSkill("Melee Weapons",           1, Specialized: true),
            new CultureSkill("Composition/Performance", 4, IsPool: true, PoolMax: 4),  // pool: choose 1-2 ranks/skill
            new CultureSkill("Crafting/Vocation",       6, IsPool: true, PoolMax: 6),
            new CultureSkill("Navigation",              1),
            new CultureSkill("Languages",              19, IsPool: true),
            new CultureSkill("Region (Own)",            5),
            new CultureSkill("Region (Neighboring)",    2),
            new CultureSkill("Religion/Philosophy",     2),
            new CultureSkill("Other Lores",             7, IsPool: true),
            new CultureSkill("Running",                 1),
            new CultureSkill("Influence",               2, Specialized: true),
            new CultureSkill("Trading",                 1),
            new CultureSkill("Concealment",             1),
            new CultureSkill("Animal Handling",         1, Specialized: true),
        }),

        new("Harsh", new[]
        {
            new CultureSkill("Perception",              2),
            new CultureSkill("Tracking",                1),
            new CultureSkill("Maneuvering in Armor",    1),
            new CultureSkill("Body Development",        3),
            new CultureSkill("Weight-training",         1),
            new CultureSkill("Unarmed",                 1, Specialized: true),
            new CultureSkill("Melee Weapons",           1, Specialized: true),
            new CultureSkill("Composition/Performance", 1, IsPool: true, PoolMax: 1),
            new CultureSkill("Crafting/Vocation",       4, IsPool: true, PoolMax: 4),
            new CultureSkill("Languages",               8, IsPool: true),
            new CultureSkill("Region (Own)",            5),
            new CultureSkill("Other Lores",             2, IsPool: true),
            new CultureSkill("Jumping",                 1),
            new CultureSkill("Running",                 2),
            new CultureSkill("Trading",                 1),
            new CultureSkill("Stalking",                2),
            new CultureSkill("Survival (Own region)",   3),
            new CultureSkill("Animal Handling",         1, Specialized: true),
        }),

        new("Highland", new[]
        {
            new CultureSkill("Perception",              1),
            new CultureSkill("Tracking",                1),
            new CultureSkill("Maneuvering in Armor",    1),
            new CultureSkill("Body Development",        2),
            new CultureSkill("Unarmed",                 1, Specialized: true),
            new CultureSkill("Riding",                  2, Specialized: true),
            new CultureSkill("Composition/Performance", 1, IsPool: true, PoolMax: 1),
            new CultureSkill("Crafting/Vocation",       4, IsPool: true, PoolMax: 4),
            new CultureSkill("Navigation",              1),
            new CultureSkill("Acrobatics",              1),
            new CultureSkill("Jumping",                 1),
            new CultureSkill("Languages",              13, IsPool: true),
            new CultureSkill("Region (Own)",            5),
            new CultureSkill("Region (Neighboring)",    2),
            new CultureSkill("Herbalism",               1),
            new CultureSkill("Medicine",                2),
            new CultureSkill("Running",                 2),
            new CultureSkill("Social Awareness",        1),
            new CultureSkill("Trading",                 1),
            new CultureSkill("Stalking",                1),
            new CultureSkill("Survival (Own region)",   2),
            new CultureSkill("Other Lores",             4, IsPool: true),
        }),

        new("Mariner", new[]
        {
            new CultureSkill("Perception",              2),
            new CultureSkill("Tracking",                1), // ⚠ unclear
            new CultureSkill("Body Development",        1),
            new CultureSkill("Weight-training",         1),
            new CultureSkill("Unarmed",                 1, Specialized: true),
            new CultureSkill("Melee Weapons",           1, Specialized: true),
            new CultureSkill("Ranged Weapons",          1, Specialized: true),
            new CultureSkill("Composition/Performance", 1, IsPool: true, PoolMax: 1),
            new CultureSkill("Crafting/Vocation",       4, IsPool: true, PoolMax: 4),
            new CultureSkill("Navigation",              1),
            new CultureSkill("Piloting",                1),
            new CultureSkill("Languages",              15, IsPool: true),
            new CultureSkill("Region (Own)",            5),
            new CultureSkill("Region (Neighboring)",    2),
            new CultureSkill("Medicine",                1),
            new CultureSkill("Running",                 1),
            new CultureSkill("Swimming",                2),
            new CultureSkill("Influence",               1, Specialized: true),
            new CultureSkill("Trading",                 1),
            new CultureSkill("Other Lores",             5, IsPool: true),
            new CultureSkill("Survival (Own region)",   1),
        }),

        new("Nomad", new[]
        {
            new CultureSkill("Perception",              1),
            new CultureSkill("Tracking",                1),
            new CultureSkill("Body Development",        1),
            new CultureSkill("Unarmed",                 1, Specialized: true),
            new CultureSkill("Riding",                  3, Specialized: true),
            new CultureSkill("Ranged Weapons",          1, Specialized: true),
            new CultureSkill("Composition/Performance", 1, IsPool: true, PoolMax: 1),
            new CultureSkill("Crafting/Vocation",       4, IsPool: true, PoolMax: 4),
            new CultureSkill("Jumping",                 1),
            new CultureSkill("Languages",              14, IsPool: true),
            new CultureSkill("Region (Own)",            5),
            new CultureSkill("Region (Neighboring)",    1),
            new CultureSkill("Herbalism",               1),
            new CultureSkill("Medicine",                1),
            new CultureSkill("Running",                 2),
            new CultureSkill("Social Awareness",        1),
            new CultureSkill("Animal Handling",         3, Specialized: true),
            new CultureSkill("Survival (Own region)",   1),
            new CultureSkill("Other Lores",             4, IsPool: true),
            new CultureSkill("Climbing",                1),
        }),

        new("Reaver", new[]
        {
            new CultureSkill("Perception",              1),
            new CultureSkill("Tracking",                1), // ⚠
            new CultureSkill("Maneuvering in Armor",    1), // ⚠
            new CultureSkill("Body Development",        2),
            new CultureSkill("Unarmed",                 2, Specialized: true),
            new CultureSkill("Melee Weapons",           1, Specialized: true),
            new CultureSkill("Ranged Weapons",          1, Specialized: true),
            new CultureSkill("Composition/Performance", 1, IsPool: true, PoolMax: 1),
            new CultureSkill("Crafting/Vocation",       3, IsPool: true, PoolMax: 3),
            new CultureSkill("Languages",              11, IsPool: true),
            new CultureSkill("Region (Own)",            5),
            new CultureSkill("Herbalism",               1),
            new CultureSkill("Medicine",                1),
            new CultureSkill("Running",                 2),
            new CultureSkill("Swimming",                1),
            new CultureSkill("Influence",               1, Specialized: true),
            new CultureSkill("Stalking",                1),
            new CultureSkill("Survival (Own region)",   1),
            new CultureSkill("Other Lores",             3, IsPool: true),
            new CultureSkill("Climbing",                1),
        }),

        new("Rural", new[]
        {
            new CultureSkill("Perception",              1),
            new CultureSkill("Body Development",        1),
            new CultureSkill("Unarmed",                 1, Specialized: true),
            new CultureSkill("Animal Handling",         2, Specialized: true),
            new CultureSkill("Riding",                  2, Specialized: true),
            new CultureSkill("Crafting/Vocation",       4, IsPool: true, PoolMax: 4),
            new CultureSkill("Languages",              13, IsPool: true),
            new CultureSkill("Region (Own)",            5),
            new CultureSkill("Medicine",                1),
            new CultureSkill("Running",                 1),
            new CultureSkill("Climbing",                2),
            new CultureSkill("Other Lores",             5, IsPool: true),
        }),

        new("Sylvan", new[]
        {
            new CultureSkill("Perception",              1),
            new CultureSkill("Body Development",        1),
            new CultureSkill("Unarmed",                 1, Specialized: true),
            new CultureSkill("Melee Weapons",           1, Specialized: true),
            new CultureSkill("Composition/Performance", 0, IsPool: true, PoolMax: 0),
            new CultureSkill("Crafting/Vocation",       4, IsPool: true, PoolMax: 4),
            new CultureSkill("Languages",              13, IsPool: true),
            new CultureSkill("Region (Own)",            5),
            new CultureSkill("Medicine",                1),
            new CultureSkill("Running",                 2),
            new CultureSkill("Swimming",                1),  // ⚠
            new CultureSkill("Influence",               1, Specialized: true),
            new CultureSkill("Climbing",                1),
            new CultureSkill("Other Lores",             4, IsPool: true),
        }),

        new("Underground", new[]
        {
            new CultureSkill("Perception",              2),
            new CultureSkill("Tracking",                1),
            new CultureSkill("Maneuvering in Armor",    2),
            new CultureSkill("Body Development",        2),
            new CultureSkill("Unarmed",                 1, Specialized: true),
            new CultureSkill("Crafting/Vocation",       5, IsPool: true, PoolMax: 5),
            new CultureSkill("Survival (Own region)",   2),
            new CultureSkill("Languages",              13, IsPool: true),
            new CultureSkill("Region (Own)",            5),
            new CultureSkill("Medicine",                1),
            new CultureSkill("Running",                 1),
            new CultureSkill("Stalking",                2),
            new CultureSkill("Concealment",             1),
            new CultureSkill("Other Lores",             4, IsPool: true),
            new CultureSkill("Animal Handling",         1, Specialized: true),
        }),

        new("Urban", new[]
        {
            new CultureSkill("Perception",              2),
            new CultureSkill("Body Development",        1),
            new CultureSkill("Unarmed",                 1, Specialized: true),
            new CultureSkill("Melee Weapons",           1, Specialized: true),
            new CultureSkill("Animal Handling",         1, Specialized: true),
            new CultureSkill("Composition/Performance", 3, IsPool: true, PoolMax: 3),
            new CultureSkill("Crafting/Vocation",       7, IsPool: true, PoolMax: 7),
            new CultureSkill("Languages",              17, IsPool: true),
            new CultureSkill("Region (Own)",            5),
            new CultureSkill("Region (Neighboring)",    3),
            new CultureSkill("Religion/Philosophy",     1),
            new CultureSkill("Medicine",                1),
            new CultureSkill("Running",                 1),
            new CultureSkill("Influence",               1, Specialized: true),
            new CultureSkill("Social Awareness",        1),
            new CultureSkill("Trading",                 1),
            new CultureSkill("Concealment",             1),
            new CultureSkill("Stalking",                2),
            new CultureSkill("Other Lores",             7, IsPool: true),
        }),
    };

    public static readonly IReadOnlyDictionary<string, CultureDef> ByName =
        All.ToDictionary(c => c.Name);
}

public record CultureDef(string Name, CultureSkill[] Skills);

// Ranks = total ranks granted. IsPool means player distributes them freely among skills in that category.
public record CultureSkill(
    string Skill,
    int Ranks,
    bool Specialized = false,
    bool IsPool = false,
    int PoolMax = 0);
