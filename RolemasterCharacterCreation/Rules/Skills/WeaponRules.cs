namespace RolemasterCharacterCreation.Rules;

public static class WeaponRules
{
    public record WeaponDef(string Name, string SkillName);

    public static readonly IReadOnlyList<WeaponDef> All = new List<WeaponDef>
    {
        // ── Melee Weapons ──────────────────────────────────────────────────────
        new("Broadsword",       "Melee Weapons"),
        new("Longsword",        "Melee Weapons"),
        new("Shortsword",       "Melee Weapons"),
        new("Dagger",           "Melee Weapons"),
        new("Hand Axe",         "Melee Weapons"),
        new("Mace",             "Melee Weapons"),
        new("Club",             "Melee Weapons"),
        new("Spear",            "Melee Weapons"),
        new("Falchion",         "Melee Weapons"),
        new("Rapier",           "Melee Weapons"),
        new("Scimitar",         "Melee Weapons"),
        new("Cutlass",          "Melee Weapons"),
        new("Battle Axe",       "Melee Weapons"),
        new("Hand Hammer",      "Melee Weapons"),
        new("Flail",            "Melee Weapons"),
        new("Quarterstaff",     "Melee Weapons"),
        new("Morning Star",     "Melee Weapons"),
        new("Javelin",          "Melee Weapons"),
        new("Pick",             "Melee Weapons"),
        new("Greatsword",       "Melee Weapons"),
        new("Great Axe",        "Melee Weapons"),
        new("War Hammer",       "Melee Weapons"),
        new("Halberd",          "Melee Weapons"),
        new("Polearm",          "Melee Weapons"),
        new("Lance",            "Melee Weapons"),
        new("Whip",             "Melee Weapons"),
        new("Net",              "Melee Weapons"),

        // ── Ranged Weapons ────────────────────────────────────────────────────
        new("Short Bow",        "Ranged Weapons"),
        new("Thrown Dagger",    "Ranged Weapons"),
        new("Long Bow",         "Ranged Weapons"),
        new("Light Crossbow",   "Ranged Weapons"),
        new("Thrown Hand Axe",  "Ranged Weapons"),
        new("Thrown Spear",     "Ranged Weapons"),
        new("Composite Bow",    "Ranged Weapons"),
        new("Heavy Crossbow",   "Ranged Weapons"),
        new("Arbalest",         "Ranged Weapons"),
        new("Sling",            "Ranged Weapons"),
        new("Dart",             "Ranged Weapons"),
        new("Throwing Star",    "Ranged Weapons"),
        new("Bola",             "Ranged Weapons"),

        // ── Unarmed ───────────────────────────────────────────────────────────
        new("Striking",         "Unarmed"),
        new("Grappling",        "Unarmed"),
        new("Sweeps & Throws",  "Unarmed"),
    };

    public static readonly IReadOnlyDictionary<string, IReadOnlyList<WeaponDef>> BySkill =
        All.GroupBy(w => w.SkillName)
           .ToDictionary(g => g.Key, g => (IReadOnlyList<WeaponDef>)g.ToList());

    // Cost key is determined by selection order (0-based slot index), not weapon type
    public static string CostKeyForSlot(int zeroBasedSlot) =>
        $"Combat Training {Math.Min(zeroBasedSlot + 1, 4)}";
}
