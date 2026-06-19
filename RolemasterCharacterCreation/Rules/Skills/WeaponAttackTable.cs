namespace RolemasterCharacterCreation.Rules;

/// <summary>
/// Maps each selectable character weapon (<see cref="WeaponRules"/>) to its Core
/// Law Chapter 10 attack table and the table's base fumble number. Used on the
/// character sheet to link a weapon to its attack table and show its fumble.
/// </summary>
public static class WeaponAttackTable
{
    public record Entry(string Table, int Fumble);

    public static readonly IReadOnlyDictionary<string, Entry> ByWeapon =
        new Dictionary<string, Entry>(StringComparer.OrdinalIgnoreCase)
        {
            // ── Melee ──────────────────────────────────────────────────────────
            ["Broadsword"]   = new("Broadsword", 4),
            ["Longsword"]    = new("Arming Sword", 4),
            ["Shortsword"]   = new("Arming Sword", 3),
            ["Dagger"]       = new("Dagger", 3),
            ["Hand Axe"]     = new("Battle Axe", 3),
            ["Mace"]         = new("Mace", 4),
            ["Club"]         = new("Club", 3),
            ["Spear"]        = new("Spear", 4),
            ["Falchion"]     = new("Falchion", 5),
            ["Rapier"]       = new("Rapier", 4),
            ["Scimitar"]     = new("Scimitar", 4),
            ["Cutlass"]      = new("Scimitar", 4),
            ["Battle Axe"]   = new("Battle Axe", 5),
            ["Hand Hammer"]  = new("War Hammer", 4),
            ["Flail"]        = new("Flail", 10),
            ["Quarterstaff"] = new("Fighting Stick", 4),
            ["Morning Star"] = new("Mace", 4),
            ["Javelin"]      = new("Spear", 4),
            ["Pick"]         = new("War Hammer", 4),
            ["Greatsword"]   = new("Broadsword", 5),
            ["Great Axe"]    = new("Battle Axe", 5),
            ["War Hammer"]   = new("War Hammer", 4),
            ["Halberd"]      = new("Battle Axe", 7),
            ["Polearm"]      = new("Spear", 8),
            ["Lance"]        = new("Spear", 4),
            ["Whip"]         = new("Whip", 7),
            ["Net"]          = new("Bola", 8),

            // ── Ranged ─────────────────────────────────────────────────────────
            ["Short Bow"]       = new("Bow, Short", 4),
            ["Thrown Dagger"]   = new("Dagger", 3),
            ["Long Bow"]        = new("Bow, Long", 5),
            ["Light Crossbow"]  = new("Crossbow", 4),
            ["Thrown Hand Axe"] = new("Battle Axe", 3),
            ["Thrown Spear"]    = new("Spear", 4),
            ["Composite Bow"]   = new("Bow, Short", 4),
            ["Heavy Crossbow"]  = new("Crossbow", 4),
            ["Arbalest"]        = new("Crossbow", 4),
            ["Sling"]           = new("Sling", 6),
            ["Dart"]            = new("Stinger", 3),
            ["Throwing Star"]   = new("Stinger", 4),
            ["Bola"]            = new("Bola", 8),

            // ── Unarmed ────────────────────────────────────────────────────────
            ["Striking"]        = new("Unarmed Strikes", 2),
            ["Grappling"]       = new("Grapple", 2),
            ["Sweeps & Throws"] = new("Unarmed Sweeps", 2),
        };

    public static Entry? For(string? weapon) =>
        weapon is not null && ByWeapon.TryGetValue(weapon, out var e) ? e : null;
}
