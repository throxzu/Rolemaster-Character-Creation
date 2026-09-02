namespace RolemasterCharacterCreation.Models;

// What a token on the combat board represents. Enemies get a full tracker row;
// players and allies are board tokens shared by every row.
public enum CombatantKind { Enemy = 0, Player = 1, Ally = 2 }

// A fight the GM tracks at the table. An encounter is either a reusable *template*
// (predefined roster, no live state) or a *fight* — one running or finished combat,
// cloned from a template or built from scratch. Fights keep their state in the DB so
// a refresh or a dropped circuit never loses the table's progress.
public class Encounter
{
    public int Id { get; set; }

    public required string Name { get; set; }
    public string? Notes { get; set; }

    // true  = saved definition, loaded by cloning it into a new fight
    // false = a fight (live or finished)
    public bool IsTemplate { get; set; }

    // Current combat round; only meaningful on a fight.
    public int Round { get; set; } = 1;

    public string? CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastUsedAt { get; set; } = DateTime.UtcNow;

    public List<EncounterCombatant> Combatants { get; set; } = [];
}

// One enemy row, or one player/ally token, inside an encounter.
public class EncounterCombatant
{
    public int Id { get; set; }

    public int EncounterId { get; set; }
    public Encounter? Encounter { get; set; }

    public CombatantKind Kind { get; set; }

    // GM-facing mob number shown in the first column ("Mob 1", "Mob 2", …).
    public int MobNumber { get; set; }

    public required string Name { get; set; }

    // Set for Kind == Player: the party character this token stands for. Nulled out
    // (rather than cascading) if the character is later deleted.
    public int? CharacterId { get; set; }
    public Character? Character { get; set; }

    // Position on the board, as a 0–1 fraction of its width/height, so the same
    // coordinates render correctly in every row's copy of the board.
    public double BoardX { get; set; } = 0.5;
    public double BoardY { get; set; } = 0.5;

    // Hit totals. MaxHits is set by the GM at the start of the fight; CurrentHits is
    // what damage is subtracted from.
    public int MaxHits { get; set; }
    public int CurrentHits { get; set; }

    // Rounds of stun still to run off (ticked down by Next round).
    public int StunRounds { get; set; }

    // Hits lost automatically at the start of every round.
    public int BleedPerRound { get; set; }

    // Rounds until a mortal wound kills this combatant; 0 = not dying.
    public int DiesInRounds { get; set; }

    public bool Prone { get; set; }

    // Unchecked once the combatant is dead, fled or otherwise out of the fight.
    public bool Active { get; set; } = true;

    public string? Notes { get; set; }

    public int SortOrder { get; set; }
}
