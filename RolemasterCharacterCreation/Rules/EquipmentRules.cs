namespace RolemasterCharacterCreation.Rules;

// Equipment data from Tables 6-3 and 6-5 (RMU Core Law)
// Excludes items not normally carried by an individual adventurer
// (boats, horses, wagons, carts, mules, cages, ladders, planks, lodging services, oars).
public static class EquipmentRules
{
    public record GeneralItem(string Name, string Cost, double WeightLbs, string? Notes = null);

    public record WeaponItem(
        string Name,
        string Category,
        string Cost,
        double WeightLbs,
        bool TwoHanded = false,
        string? Notes = null);

    // ── General Equipment (Table 6-3, filtered) ───────────────────────────────

    public static readonly IReadOnlyList<GeneralItem> General = new GeneralItem[]
    {
        new("Arrows (20)",         "4 bp",   3.0,   null),
        new("Backpack",            "2 bp",   3.0,   "Holds 20 lbs, 1 cu'"),
        new("Bedroll, heavy",      "7 bp",   9.0,   "Wool/fur. 4 season"),
        new("Bedroll, light",      "2 bp",   5.0,   "Wool blanket. 2 season"),
        new("Belt",                "1 bp",   0.5,   null),
        new("Belt pouch",          "1 cp",   0.1,   null),
        new("Blowdarts (20)",      "3 bp",   1.0,   null),
        new("Boots",               "1 sp",   3.5,   null),
        new("Brush, writing",      "5 cp",   0.25,  null),
        new("Bucket",              "4 bp",   2.5,   "Holds 3 gallons"),
        new("Caltrops (5)",        "8 bp",   2.0,   "Portable spike traps"),
        new("Candle",              "4 cp",   0.25,  "Burns 2 hrs, 10' light"),
        new("Case",                "4 sp",   1.0,   "Water-resistant leather"),
        new("Cask",                "24 bp",  5.0,   "Holds 4 gallons"),
        new("Chain",               "6 bp",   9.0,   "Iron, 10'"),
        new("Chalk (10)",          "2 bp",   0.25,  null),
        new("Charcoal",            "22 cp",  1.0,   "Hot 4 hr fire"),
        new("Chisel",              "9 bp",   1.0,   null),
        new("Climbing pick",       "28 bp",  2.0,   null),
        new("Cloak",               "9 bp",   2.5,   null),
        new("Clothes, fine",       "11 bp",  2.5,   null),
        new("Clothes, normal",     "5 bp",   2.5,   null),
        new("Coat",                "15 bp",  7.0,   null),
        new("Crossbow bolts (20)", "11 bp",  3.0,   null),
        new("Crowbar, heavy",      "8 bp",   5.0,   "×2 advantage feats of strength"),
        new("Crowbar, light",      "6 bp",   3.0,   "×1.5 advantage feats of strength"),
        new("Dice, pair",          "35 bp",  0.25,  "Bone"),
        new("Drum, large",         "2 sp",   8.0,   null),
        new("Drum, small",         "12 bp",  3.0,   null),
        new("Fire-starting bow",   "1 cp",   0.5,   "Starts fire in 5 min"),
        new("Flint and steel",     "1 bp",   0.5,   "Starts fire in 3 min"),
        new("Framepack",           "33 cp",  3.5,   "Holds 45 lbs, 2 cu'"),
        new("Gloves",              "2 bp",   0.5,   "Heavy leather, lined"),
        new("Grappling hook",      "1 sp",   1.0,   null),
        new("Hammer",              "1 sp",   1.0,   null),
        new("Hammock",             "1 bp",   2.5,   null),
        new("Harness",             "1 sp",   4.0,   "Includes bit, reins"),
        new("Hat",                 "6 bp",   1.0,   null),
        new("Holy symbol",         "1+ bp",  0.0,   null),
        new("Hood",                "16 cp",  0.5,   "Covers head & shoulders"),
        new("Horn",                "2 sp",   2.0,   "Signaling, range 1+ mi"),
        new("Ink",                 "14 cp",  0.25,  "Black, non-soluble"),
        new("Instrument, stringed","12 sp",  4.0,   "e.g. lute, lyre, viol"),
        new("Lantern",             "12 bp",  1.5,   "10' light, 30' dim"),
        new("Lock pick kit",       "1 sp",   0.5,   null),
        new("Mirror",              "35 bp",  0.5,   "Silvered glass, 6\"×4\""),
        new("Nails (20)",          "1 cp",   0.5,   "Iron, 3\""),
        new("Oil flask",           "3 bp",   1.0,   "6 hr lantern fuel"),
        new("Padded undercoat",    "6 bp",   3.0,   null),
        new("Padlock, basic",      "23 bp",  1.0,   "Medium difficulty"),
        new("Padlock, superior",   "35 bp",  1.0,   "Hard difficulty"),
        new("Paper (10)",          "12 bp",  0.25,  "10 sheets"),
        new("Parchment (10)",      "2 sp",   0.25,  "10 sheets, very durable"),
        new("Pegs (10)",           "9 tp",   2.0,   "Wood"),
        new("Pitons (10)",         "2 bp",   2.5,   "Iron"),
        new("Pole",                "5 cp",   7.0,   "Wood, 10'"),
        new("Pot, cooking",        "7 bp",   2.5,   "Iron, holds 2 gallons"),
        new("Pouch",               "1 cp",   0.2,   null),
        new("Prayer beads",        "2 cp",   0.0,   null),
        new("Quill-pens (10)",     "4 cp",   0.25,  null),
        new("Quiver",              "1 bp",   0.5,   "Holds 20 arrows/bolts"),
        new("Rations",             "5 cp",   18.0,  "1 week food"),
        new("Rations, trail",      "1 bp",   14.0,  "1 week preserved food"),
        new("Robe, fine",          "2 sp",   4.0,   null),
        new("Robe, normal",        "1 sp",   4.5,   null),
        new("Rope",                "4 bp",   5.0,   "Hemp, 50'"),
        new("Rope, superior",      "12 bp",  3.0,   "Reinforced hemp, 50'"),
        new("Sack (50 lb)",        "8 cp",   2.5,   "Holds 50 lbs, 3 cu'"),
        new("Saddle",              "5 sp",   11.0,  "Includes stirrups, blanket"),
        new("Saddle bag",          "8 bp",   5.0,   "Holds 15 lbs, 1.5 cu'"),
        new("Sandals",             "5 bp",   1.5,   null),
        new("Saw",                 "23 bp",  2.5,   null),
        new("Scabbard, belt",      "25 bp",  1.0,   "Holds one 1-h weapon"),
        new("Scabbard, shoulder",  "3 sp",   1.5,   "Holds one 2-h weapon"),
        new("Sling bullets (10)",  "1 bp",   5.0,   "Lead/metal, +1 size"),
        new("Sling stones (10)",   "1 cp",   5.0,   null),
        new("Spade",               "16 bp",  3.5,   null),
        new("Surcoat",             "9 bp",   1.2,   "Linen"),
        new("Tarp",                "1 bp",   4.0,   "Canvas, 5'×8'"),
        new("Tent, large",         "16 sp",  35.0,  "Canvas, 14'×17'. 8-man"),
        new("Tent, medium",        "8 sp",   20.0,  "Canvas, 8'×10'. 4-man"),
        new("Tent, small",         "2 sp",   9.0,   "Canvas, 5'×8'. 2-man"),
        new("Tinderbox",           "2 cp",   0.25,  "Enough for 7 fires"),
        new("Torch (3)",           "1 cp",   3.0,   "6 hrs each, 30' shadowy"),
        new("Vial",                "2 bp",   0.25,  "Glass, 2 oz"),
        new("Waterskin",           "1 cp",   0.25,  "Holds 1 pt"),
        new("Weapon belt",         "5 bp",   1.0,   "Holds 2 scabbards, 3 pouches"),
        new("Wedge, splitting",    "3 cp",   3.0,   "Iron"),
        new("Wedge, staying",      "1 cp",   1.0,   "Hardwood"),
        new("Whistle",             "2 sp",   0.5,   "Wood/iron, range 1+ mi"),
    };

    // ── Weapons (Table 6-5) ───────────────────────────────────────────────────

    public static readonly IReadOnlyList<WeaponItem> Weapons = new WeaponItem[]
    {
        // Blade
        new("Dagger",         "Blade",          "3 sp",   1.0),
        new("Short Sword",    "Blade",          "7 sp",   3.0),
        new("Arming Sword",   "Blade",          "18 sp",  2.5),
        new("Epee",           "Blade",          "16 sp",  1.5),
        new("Rapier",         "Blade",          "22 sp",  2.0),
        new("Broadsword",     "Blade",          "10 sp",  4.0),
        new("Scimitar",       "Blade",          "10 sp",  3.0),
        new("Machete",        "Blade",          "2 sp",   1.5),
        new("Falchion",       "Blade",          "10 sp",  3.0),
        // Greater Blade (2H)
        new("Claymore",       "Greater Blade",  "21 sp",  7.0,  true),
        new("Long Scimitar",  "Greater Blade",  "19 sp",  6.0,  true),
        new("Longsword",      "Greater Blade",  "22 sp",  6.0,  true),
        new("Great Falchion", "Greater Blade",  "25 sp",  7.0,  true),
        // Chain
        new("Light Flail",    "Chain",          "7 sp",   3.0),
        new("Flail",          "Chain",          "16 sp",  6.0),
        // Greater Chain (2H)
        new("Heavy Flail",    "Greater Chain",  "19 sp",  6.0,  true),
        // Hafted
        new("Hand Axe",       "Hafted",         "5 sp",   1.5),
        new("Battle Axe",     "Hafted",         "13 sp",  3.0),
        new("Blackjack",      "Hafted",         "1 cp",   2.0),
        new("Club",           "Hafted",         "1 cp",   3.0),
        new("Light Mace",     "Hafted",         "4 sp",   4.0),
        new("Mace",           "Hafted",         "6 sp",   6.0),
        new("Light Stick",    "Hafted",         "1 cp",   1.5),
        new("Fighting Stick", "Hafted",         "3 cp",   3.0),
        new("War Hammer",     "Hafted",         "15 sp",  3.5),
        // Greater Hafted (2H)
        new("Long Axe",       "Greater Hafted", "17 sp",  6.0,  true),
        new("Large Club",     "Greater Hafted", "1 sp",   6.5,  true),
        new("Great Mace",     "Greater Hafted", "12 sp",  8.0,  true),
        new("Quarterstaff",   "Greater Hafted", "5 cp",   4.0,  true),
        new("War Mattock",    "Greater Hafted", "15 sp",  6.0,  true),
        // Pole Arm (2H)
        new("Glaive",         "Pole Arm",       "8 sp",   6.0,  true),
        new("Halberd",        "Pole Arm",       "8 sp",   5.0,  true),
        new("Poleaxe",        "Pole Arm",       "10 sp",  5.0,  true),
        new("Spear",          "Pole Arm",       "9 bp",   4.5,  false, "Can be used 1H or 2H"),
        new("Long Spear",     "Pole Arm",       "12 bp",  6.5,  true),
        // Exotic Melee
        new("Whip",           "Exotic",         "2 sp",   3.0),
        new("Small Net",      "Exotic",         "4 sp",   2.5),
        new("Net",            "Exotic",         "7 sp",   6.0),
        new("Large Net",      "Exotic",         "11 sp",  10.0, true),
        // Bow (2H)
        new("Short Bow",      "Bow",            "6 sp",   2.0,  true),
        new("Composite Bow",  "Bow",            "9 sp",   3.0,  true),
        new("Long Bow",       "Bow",            "10 sp",  2.5,  true),
        // Crossbow
        new("Hand Crossbow",  "Crossbow",       "13 sp",  2.0,  false, "1H, 2H to reload"),
        new("Crossbow",       "Crossbow",       "11 sp",  6.0,  true),
        new("Heavy Crossbow", "Crossbow",       "25 sp",  10.0, true),
        // Sling
        new("Slingshot",      "Sling",          "3 bp",   1.0),
        new("Sling",          "Sling",          "9 bp",   1.0),
        new("Staff-Sling",    "Sling",          "12 bp",  4.0,  true),
        // Thrown
        new("Bola",           "Thrown",         "5 sp",   4.0),
        new("Throwing Club",  "Thrown",         "1 cp",   2.0),
        new("Javelin",        "Thrown",         "3 sp",   4.0),
        new("Dart",           "Thrown",         "5 cp",   0.5),
        // Exotic Ranged
        new("Blowpipe",       "Exotic Ranged",  "5 cp",   3.0),
    };

    public static readonly IReadOnlyList<string> WeaponCategories =
        Weapons.Select(w => w.Category).Distinct().ToList();
}
