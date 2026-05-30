using RolemasterCharacterCreation.Models;

namespace RolemasterCharacterCreation.Rules;

// Table 2-2a — Race definitions
// StatMods array indexed by StatName enum: 0=Ag 1=Co 2=Em 3=In 4=Me 5=Pr 6=Qu 7=Re 8=SD 9=St
// ResistMods array: 0=Channeling 1=Essence 2=Mentalism 3=Physical
public static class RaceRules
{
    public static readonly IReadOnlyList<RaceDef> All = new List<RaceDef>
    {
        //              name               dp  sz  hits end   rec    Ag   Co   Em   In   Me   Pr   Qu   Re   SD   St    Ch   Es   Mn   Ph
        R("Avinarc",                       35, "M", 20,   0, 1.0f,  +4,  -1,  -2,   0,  -2,  -1,  +3,   0,  +2,  -2,  +5,  +5,  +5, -10),
        R("Dwarf",                          6, "M", 30, +20, 0.5f,  -1,  +6,  -6,   0,   0,  -3,  -1,   0,   0,  +2,   0, +15, +15, +10),
        R("Elf, fair",                      0, "M", 20, +10, 2.0f,   0,  -2,  +3,  +2,  +2,  +3,  +1,  +2,  -5,  -2, -10, -10, -10,   0),
        R("Elf, grey",                      2, "M", 20, +10, 2.0f,  +2,   0,  +3,   0,  +1,  +3,  +3,   0,  -5,   0,  -5,  -5,  -5, +10),
        R("Elf, high",                      3, "M", 25, +10, 2.0f,  +3,  -1,  +2,   0,  +2,  +3,  +2,  +1,  -5,  -1,  -5,  -5,  -5, +10),
        R("Elf, wood",                      3, "M", 20, +10, 2.0f,  +2,   0,  +2,   0,  +1,  +2,  +3,   0,  -5,  -2,  -5,  -5,  -5, +20),
        R("Gnoll",                         17, "S", 20,   0, 0.5f,  +2,  -2,  +4,  -2,  +1,  -2,  +2,  +1,  +1,  -3,  +5,   0,  +5, +10),
        R("Gnome",                          4, "M", 25,   0, 0.5f,   0,  -2,  +2,   0,  +4,   0,  +2,  +2,  +1,  -2, +10,   0,   0,  +5),
        R("Goblin",                        46, "S", 25,   0, 0.5f,  +5,  +5,  -3,   0,   0,  -3,  +2,   0,  -5,   0,   0,   0,   0,  +5),
        R("Gratar",                        11, "M", 25,   0, 1.0f,  +1,  +3,  -2,   0,   0,  -2,   0,   0,  -2,  +2,   0,   0,   0,   0),
        R("Half-Elf",                      18, "M", 25,  +5, 1.0f,  +2,   0,   0,   0,   0,  +2,  +2,   0,  -3,  +2,  -5,  -5,  -5,  +5),
        R("Halfling",                      29, "S", 25,   0, 1.0f,  +5,  +4,  -2,   0,   0,  -5,  +4,   0,  -4,   0,   0, +25, +20, +10),
        R("Hobgoblin",                     30, "M", 25,   0, 0.5f,  +3,  +2,  -2,   0,  -2,  -2,  +1,   0,  -2,  +1,   0,   0,   0,   0),
        R("Human, cave",                   41, "M", 25, +25, 1.0f,   0,  +1,   0,   0,  -1,   0,   0,  -1,   0,  +1,   0,   0,  -5,   0),
        R("Human, common",                 50, "M", 25,   0, 1.0f,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0),
        R("Human, high",                   21, "M", 30,   0, 1.0f,  -2,  +3,   0,   0,   0,  +3,  -1,  +1,   0,  +3,  +5,  -5,  -5,   0),
        R("Human, mixed",                  34, "M", 30,   0, 1.0f,   0,  +1,   0,   0,   0,  +1,   0,   0,   0,  +1,   0,   0,   0,   0),
        R("Hvasstonn",                     27, "B", 35, +15, 1.0f,  -1,  +1,  -2,   0,   0,  -2,   0,  -1,  -4,  +2,   0,   0,   0, +10),
        R("Idiyva",                        10, "M", 25, +10, 1.0f,  +4,   0,  -4,   0,   0,  -1,  +4,   0,  -2,  +2,   0,   0,   0,  +5),
        R("Kobold",                        75, "S", 20, +10, 0.5f,  +5,  +1,  -3,  -1,   0,  -3,  +4,   0,  -3,  -1,   0,   0,   0,   0),
        R("Nycamerith",                    26, "M", 20,   0, 1.0f,   0,   0,  +2,  +2,  +2,  +2,  +2,   0,  -4,  -2,   0,   0,   0,   0),
        R("Orc, greater",                  30, "M", 30, +10, 0.5f,   0,  +5,  -4,   0,  -2,  -2,   0,  -2,  -4,  +3,   0,   0,   0, +10),
        R("Orc, grey",                     60, "M", 20,   0, 0.5f,   0,  -2,   0,   0,  +1,   0,   0,  +1,  -2,   0,  -5,  -5, -10,  +5),
        R("Orc, lesser",                   75, "M", 25,  +5, 0.5f,   0,  +2,  -3,   0,  -2,  -3,   0,  -2,  -4,  +1,   0,   0,   0,  +5),
        R("Orc, scrug",                     0, "B", 35, +10, 0.5f,   0,  +4,  -2,   0,  -2,  -2,   0,  -2,  -4,  +2,  +5,  +5,  +5, +10),
        R("Orc, vard",                     18, "M", 30,   0, 0.5f,   0,  +4,  -2,   0,  -2,  -2,   0,  -2,  -4,  +2,   0,   0,   0,  +5),
        R("Plynos",                         8, "M", 25, +25, 1.0f,  +2,  +3,  -2,  +1,   0,  -1,  +2,   0,  -2,   0,  -5,   0,  +5, +10),
        R("Sea-kral",                       3, "M", 25, +10, 1.0f,  +2,  +3,  -2,  +1,  -1,  -1,   0,  -1,  +1,  +1,  +5,   0,   0, +10),
        R("Sibbicai",                       0, "M", 25, +10, 1.0f,  +1,  +2,  -3,  +2,   0,  -1,   0,   0,  -2,  +2,   0,   0,   0,  -5),
        R("Sohleugir",                      2, "M", 25, +10, 1.0f,   0,  +3,   0,   0,  -2,  -2,   0,   0,  -2,  +2,   0,  -5,   0,   0),
        R("Sstoi'isslythi",                 0, "M", 20,   0, 2.0f,  +2,   0,  +1,   0,   0,  +1,  +2,   0,   0,  -1,   0,   0,   0,   0),
        R("Troll",                         29, "B", 25, +10, 1.0f,  -1,  +2,  -5,   0,  -3,   0,   0,  -3,  -3,  +5, -10, -10, -10, +15),
        R("Vulfen",                         0, "M", 30, +10, 1.0f,   0,  +3,  -3,  +1,   0,   0,  +2,  -1,  -4,  +2,   0,   0,   0,   0),
    };

    static RaceDef R(string name, int dp, string size, int hits, int end, float rec,
        int ag, int co, int em, int @in, int me, int pr, int qu, int re, int sd, int st,
        int ch, int es, int mn, int ph)
        => new(name, dp, size, hits, end, rec,
            [ag, co, em, @in, me, pr, qu, re, sd, st],
            [ch, es, mn, ph]);

    public static readonly IReadOnlyDictionary<string, RaceDef> ByName =
        All.ToDictionary(r => r.Name);
}

public record RaceDef(
    string Name,
    int BonusDP,
    string Size,           // "S" = Small (×0.75 hits), "M" = Medium, "B" = Big (×1.5 hits)
    int BaseHits,
    int EnduranceBonus,
    float RecoveryMult,
    int[] StatMods,        // indexed by (int)StatName: Ag=0 Co=1 Em=2 In=3 Me=4 Pr=5 Qu=6 Re=7 SD=8 St=9
    int[] ResistMods       // [0]=Channeling [1]=Essence [2]=Mentalism [3]=Physical
)
{
    public float HitMultiplier => Size == "S" ? 0.75f : Size == "B" ? 1.5f : 1.0f;
    public int GetStatMod(StatName stat) => StatMods[(int)stat];
    public int ResistChanneling => ResistMods[0];
    public int ResistEssence    => ResistMods[1];
    public int ResistMentalism  => ResistMods[2];
    public int ResistPhysical   => ResistMods[3];
}
