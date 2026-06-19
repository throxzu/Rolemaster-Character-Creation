using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using RolemasterCharacterCreation.Data;
using RolemasterCharacterCreation.Identity;
using RolemasterCharacterCreation.Models;
using RolemasterCharacterCreation.Rules;

namespace RolemasterCharacterCreation.Components.Pages;

public partial class CharacterWizard
{
    [Parameter] public int Id { get; set; }

    [Inject] IJSRuntime JS { get; set; } = default!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await JS.InvokeVoidAsync("initTooltips");
    }

    static readonly string[] Steps = ["Concept", "Stats", "Prof. Skills", "Skills", "Talents", "Equipment"];
    static readonly (int Index, string Label)[] LevelUpSteps = [(1, "Stat Gains"), (3, "Skills"), (4, "Talents")];

    // ── Height / weight ranges per race ──────────────────────────────────────
    // HMin in cm, WMin in kg. Step sizes are determined by race size:
    //   S → h step 2 cm, w step 2 kg  (20 options → range +38 cm / +38 kg)
    //   M → h step 3 cm, w step 5 kg  (20 options → range +57 cm / +95 kg)
    //   B → h step 4 cm, w step 5 kg  (20 options → range +76 cm / +95 kg)
    static readonly Dictionary<string, (int HMin, int WMin)> _raceBuild = new()
    {
        // S
        ["Gnoll"]    = (150, 55),
        ["Goblin"]   = (100, 28),
        ["Halfling"] = ( 90, 24),
        ["Kobold"]   = ( 75, 18),
        // M
        ["Avinarc"]          = (158, 48),
        ["Dwarf"]            = (110, 60),
        ["Elf, fair"]        = (158, 40),
        ["Elf, grey"]        = (155, 40),
        ["Elf, high"]        = (160, 45),
        ["Elf, wood"]        = (153, 38),
        ["Gnome"]            = ( 88, 22),
        ["Gratar"]           = (153, 52),
        ["Half-Elf"]         = (155, 45),
        ["Hobgoblin"]        = (155, 62),
        ["Human, cave"]      = (150, 45),
        ["Human, common"]    = (153, 45),
        ["Human, high"]      = (155, 45),
        ["Human, mixed"]     = (153, 45),
        ["Idiyva"]           = (153, 48),
        ["Nycamerith"]       = (153, 45),
        ["Orc, greater"]     = (172, 82),
        ["Orc, grey"]        = (163, 72),
        ["Orc, lesser"]      = (160, 70),
        ["Orc, vard"]        = (160, 72),
        ["Plynos"]           = (153, 50),
        ["Sea-kral"]         = (153, 52),
        ["Sibbicai"]         = (148, 45),
        ["Sohleugir"]        = (153, 52),
        ["Sstoi'isslythi"]   = (150, 45),
        ["Vulfen"]           = (163, 62),
        // B
        ["Hvasstonn"]  = (196, 108),
        ["Orc, scrug"] = (196, 118),
        ["Troll"]      = (220, 148),
    };

    static string FmtHeight(int cm)
    {
        double totalIn = cm / 2.54;
        int feet = (int)(totalIn / 12);
        int inches = (int)Math.Round(totalIn % 12);
        if (inches == 12) { feet++; inches = 0; }
        return $"{cm} cm ({feet}'{inches}\")";
    }

    static string FmtWeight(int kg)
    {
        int lb = (int)Math.Round(kg * 2.20462);
        return $"{kg} kg ({lb} lb)";
    }

    static IReadOnlyList<int> HeightOptions(string raceName)
    {
        if (!_raceBuild.TryGetValue(raceName, out var b)) return [];
        int step = RaceRules.ByName.GetValueOrDefault(raceName)?.Size switch { "S" => 2, "B" => 4, _ => 3 };
        return Enumerable.Range(0, 20).Select(i => b.HMin + i * step).ToArray();
    }

    static IReadOnlyList<int> WeightOptions(string raceName)
    {
        if (!_raceBuild.TryGetValue(raceName, out var b)) return [];
        int step = RaceRules.ByName.GetValueOrDefault(raceName)?.Size == "S" ? 2 : 5;
        return Enumerable.Range(0, 20).Select(i => b.WMin + i * step).ToArray();
    }

    Character? _char;
    string? _currentUserId;
    bool _isGm;
    bool _authorized;
    int _step;
    string? _error;

    // ── Level-up mode ────────────────────────────────────────────────────────
    bool _isLevelUp;
    Dictionary<string, int> _baselinePurchased = new();
    Dictionary<(string Skill, string Spec), int> _baselineWeaponAllocs = new();
    Dictionary<(string Skill, string Spec), int> _baselineSpellAllocs = new();
    HashSet<int> _baselineTalentIds = new();
    // Stat gain rolls: stat → (roll result, did it succeed)
    Dictionary<StatName, (int Roll, bool Gained)> _statGainRolls = new();
    // Baseline stat values so re-rolls don't stack
    Dictionary<StatName, (int Temp, int Potential)> _statBaseline = new();
    // Manual roll inputs: stat → text input value
    Dictionary<StatName, string> _manualRollInputs = new();

    // Used for deserializing the frozen skill-rank snapshot stored in Character.LevelUpBaselineJson
    record SkillSnap(string SkillName, string? Specialization, string Category, int PurchasedRanks);

    // ── Step 0: pool allocation state ────────────────────────────────────────
    record PoolEntry(string SkillName, string? Specialization, int Ranks);
    Dictionary<string, List<PoolEntry>> _poolAllocs = new();
    string? _activePoolAdd;
    string _poolAddSkill = "";
    string _poolAddSpec = "";
    int _poolAddRanks = 1;

    // ── Step 1: stat rolling state ───────────────────────────────────────────
    record StatRoll(int Temp, int Potential);
    Dictionary<StatName, StatRoll> _rolls = new();
    int _boostsRemaining = 2;
    int _swapsRemaining = 2;
    StatName? _swapA;
    int? _pendingBoostType;

    // ── Step 2: professional skill selection ─────────────────────────────────
    HashSet<string> _chosenPro = new();
    HashSet<string> _chosenKnack = new();

    // ── Step 3: skill rank purchases ─────────────────────────────────────────
    Dictionary<string, Dictionary<string, int>> _purchased = new();

    // weapon specialization purchases (separate from _purchased to allow per-weapon CT tier)
    record WeaponAlloc(string SkillName, string Specialization, int Ranks);
    List<WeaponAlloc> _weaponAllocs = new();
    string? _activeWeaponAdd;
    string _weaponAddSpec = "";
    int _weaponAddRanks = 1;

    // spell list purchases (separate from _purchased to allow per-list specialization)
    record SpellAlloc(string SkillName, string Specialization, int Ranks);
    List<SpellAlloc> _spellAllocs = new();
    string? _activeSpellAdd;
    string _spellAddName = "";
    int _spellAddRanks = 1;

    // generic specialized skill purchases (Language: Elvish, Administration: Officer, etc.)
    record GenericAlloc(string SkillName, string Specialization, int Ranks);
    List<GenericAlloc> _genericAllocs = new();
    Dictionary<(string Skill, string Spec), int> _baselineGenericAllocs = new();
    string _genericAddSkill = "";
    string _genericAddSpec  = "";

    int _dpBudget;
    int _dpSpent        => CalcDpSpent();
    int _talentDpSpent  => _isLevelUp
        ? (_char?.Talents.Where(t => !_baselineTalentIds.Contains(t.Id)).Sum(t => TalentRules.TierCost(t.TalentName, t.Tier)) ?? 0)
        : (_char?.Talents.Sum(t => TalentRules.TierCost(t.TalentName, t.Tier)) ?? 0);
    int _dpRemaining    => _dpBudget - _dpSpent - _talentDpSpent;

    // ── Step 4: talent selection state ───────────────────────────────────────
    string _talentName        = "";
    int    _talentTier        = 1;
    string _talentRestriction = "";
    string _talentCategory    = "All";

    // ── Step 5: equipment item quantities (item name → qty) ───────────────────
    Dictionary<string, int> _equipQty = new();

    protected override async Task OnInitializedAsync()
    {
        var auth = await AuthState.GetAuthenticationStateAsync();
        _currentUserId = UserManager.GetUserId(auth.User);
        _isGm = auth.User.IsInRole(Roles.Gamemaster);

        _char = await Db.Characters
            .Include(c => c.Stats)
            .Include(c => c.Skills)
            .Include(c => c.Talents)
            .Include(c => c.EquipmentItems)
            .FirstOrDefaultAsync(c => c.Id == Id);

        if (_char is null) return;

        _authorized = _isGm || _char.UserId == _currentUserId;
        if (!_authorized) return;

        _step = _char.WizardStep;
        _dpBudget = 60 + Math.Min(25, _char.RaceBonusDp);

        LoadProfSkillState();
        LoadPurchasedState();
        LoadWeaponAllocsState();
        LoadSpellAllocsState();
        LoadGenericAllocsState();
        LoadPoolAllocsState();
        LoadEquipmentState();
        EnsureStats();

        _isLevelUp = _char.Level > 1;
        if (_isLevelUp)
        {
            // Ensure we're on a valid level-up step; any other step falls back to Stat Gains
            if (!LevelUpSteps.Any(s => s.Index == _step))
            {
                _step = 1;
                _char.WizardStep = 1;
            }

            // Skill baselines — use the frozen snapshot set when Level Up was clicked.
            // This prevents a player from restarting the wizard to get a fresh DP budget.
            var jsonOpts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            if (!string.IsNullOrEmpty(_char.LevelUpBaselineJson))
            {
                var snaps = JsonSerializer.Deserialize<List<SkillSnap>>(_char.LevelUpBaselineJson, jsonOpts) ?? [];
                _baselinePurchased = snaps
                    .Where(s => s.Specialization == null
                             && !WeaponRules.BySkill.ContainsKey(s.SkillName)
                             && s.Category != "Spellcasting")
                    .ToDictionary(s => s.SkillName, s => s.PurchasedRanks);
                _baselineWeaponAllocs = snaps
                    .Where(s => s.Specialization != null && WeaponRules.BySkill.ContainsKey(s.SkillName))
                    .ToDictionary(s => (s.SkillName, s.Specialization!), s => s.PurchasedRanks);
                _baselineSpellAllocs = snaps
                    .Where(s => s.Specialization != null && s.Category == "Spellcasting")
                    .ToDictionary(s => (s.SkillName, s.Specialization!), s => s.PurchasedRanks);
                _baselineGenericAllocs = snaps
                    .Where(s => s.Specialization != null && IsGenericSpecialized(s.SkillName))
                    .ToDictionary(s => (s.SkillName, s.Specialization!), s => s.PurchasedRanks);
            }
            else
            {
                // Fallback for characters that levelled up before snapshot support was added
                _baselinePurchased = _char.Skills
                    .Where(s => !(WeaponRules.BySkill.ContainsKey(s.SkillName) && s.Specialization != null)
                             && !(s.Category == "Spellcasting" && s.Specialization != null))
                    .GroupBy(s => s.SkillName)
                    .ToDictionary(g => g.Key, g => g.Sum(s => s.PurchasedRanks));
                _baselineWeaponAllocs  = _weaponAllocs.ToDictionary(w => (w.SkillName, w.Specialization), w => w.Ranks);
                _baselineSpellAllocs   = _spellAllocs.ToDictionary(s => (s.SkillName, s.Specialization), s => s.Ranks);
                _baselineGenericAllocs = _genericAllocs.ToDictionary(g => (g.SkillName, g.Specialization), g => g.Ranks);
            }

            _baselineTalentIds = _char.Talents.Select(t => t.Id).ToHashSet();

            // Stat baseline — use frozen snapshot so stat gains can't be re-rolled after restart
            if (!string.IsNullOrEmpty(_char.StatBaselineJson))
            {
                var statSnaps = JsonSerializer.Deserialize<Dictionary<string, int[]>>(_char.StatBaselineJson, jsonOpts) ?? new();
                _statBaseline = statSnaps
                    .Where(kvp => Enum.TryParse<StatName>(kvp.Key, out _) && kvp.Value.Length >= 2)
                    .ToDictionary(kvp => Enum.Parse<StatName>(kvp.Key), kvp => (kvp.Value[0], kvp.Value[1]));
            }
            else
            {
                _statBaseline = _char.Stats.ToDictionary(s => s.Stat, s => (s.Temporary, s.Potential));
            }
        }
    }

    void EnsureStats()
    {
        foreach (StatName sn in Enum.GetValues<StatName>())
        {
            if (!_char!.Stats.Any(s => s.Stat == sn))
            {
                var st = new CharacterStat { CharacterId = _char.Id, Stat = sn, Temporary = 50, Potential = 60 };
                _char.Stats.Add(st);
            }
        }
    }

    void LoadProfSkillState()
    {
        foreach (var sk in _char!.Skills)
        {
            if (sk.IsProfessionalSkill) _chosenPro.Add(sk.SkillName);
            if (sk.IsKnack) _chosenKnack.Add(sk.SkillName);
        }
    }

    void LoadPurchasedState()
    {
        foreach (var sk in _char!.Skills)
        {
            // Weapon specializations are tracked in _weaponAllocs, not _purchased
            if (WeaponRules.BySkill.ContainsKey(sk.SkillName) && sk.Specialization is not null) continue;
            // Spell list specializations are tracked in _spellAllocs, not _purchased
            if (sk.Category == "Spellcasting" && sk.Specialization is not null) continue;
            // Generic specialized skills (Language, Lore, Vocation, etc.) tracked in _genericAllocs
            if (sk.Specialization is not null && IsGenericSpecialized(sk.SkillName)) continue;
            if (!_purchased.ContainsKey(sk.Category))
                _purchased[sk.Category] = new();
            _purchased[sk.Category][sk.SkillName] = sk.PurchasedRanks;
        }
    }

    void LoadGenericAllocsState()
    {
        _genericAllocs.Clear();
        foreach (var sk in _char!.Skills
            .Where(s => s.Specialization is not null
                     && IsGenericSpecialized(s.SkillName)
                     && s.PurchasedRanks > 0))
        {
            _genericAllocs.Add(new GenericAlloc(sk.SkillName, sk.Specialization!, sk.PurchasedRanks));
        }
    }

    void LoadWeaponAllocsState()
    {
        _weaponAllocs.Clear();
        foreach (var sk in _char!.Skills
            .Where(s => WeaponRules.BySkill.ContainsKey(s.SkillName)
                        && s.Specialization is not null
                        && s.PurchasedRanks > 0))
        {
            _weaponAllocs.Add(new WeaponAlloc(sk.SkillName, sk.Specialization!, sk.PurchasedRanks));
        }
    }

    void LoadSpellAllocsState()
    {
        _spellAllocs.Clear();
        foreach (var sk in _char!.Skills
            .Where(s => s.Category == "Spellcasting"
                        && s.Specialization is not null
                        && s.PurchasedRanks > 0))
        {
            _spellAllocs.Add(new SpellAlloc(sk.SkillName, sk.Specialization!, sk.PurchasedRanks));
        }
    }

    void LoadPoolAllocsState()
    {
        _poolAllocs.Clear();
        if (_char?.Culture is null) return;
        if (!CultureRules.ByName.TryGetValue(_char.Culture, out var cult)) return;

        foreach (var pool in cult.Skills.Where(s => s.IsPool))
        {
            var eligible = GetPoolSkills(pool.Skill).ToHashSet();
            foreach (var sk in _char.Skills.Where(s => s.CulturalRanks > 0 && eligible.Contains(s.SkillName)))
            {
                if (!_poolAllocs.ContainsKey(pool.Skill))
                    _poolAllocs[pool.Skill] = new();
                _poolAllocs[pool.Skill].Add(new PoolEntry(sk.SkillName, sk.Specialization, sk.CulturalRanks));
            }
        }
    }

    // ── Navigation ────────────────────────────────────────────────────────────

    async Task NextStep()
    {
        _error = null;
        if (!ValidateStep(_step)) return;
        await SaveStepAsync();
        _step = _isLevelUp ? NextLevelUpStep(_step) : _step + 1;
        _char!.WizardStep = _step;
        await Db.SaveChangesAsync();
    }

    async Task PrevStep()
    {
        _error = null;
        if (_isLevelUp)
        {
            int prev = PrevLevelUpStep(_step);
            if (prev < 0) { Nav.NavigateTo($"/character/{Id}/sheet"); return; }
            _step = prev;
        }
        else
        {
            _step--;
        }
        _char!.WizardStep = _step;
        await Db.SaveChangesAsync();
    }

    async Task FinishAsync()
    {
        _error = null;
        if (!ValidateStep(_step)) return;
        await SaveStepAsync();

        // Consume bonus DPs from the racial pool (capped at 25 per level).
        int bonusDpAvailable = Math.Min(25, _char!.RaceBonusDp);
        int bonusDpConsumed  = Math.Max(0, Math.Min(bonusDpAvailable, _dpSpent + _talentDpSpent - 60));
        _char.RaceBonusDp   -= bonusDpConsumed;

        if (_isLevelUp)
        {
            _char.WizardStep = 1;           // ready for next level-up
            _char.LevelUpBaselineJson = null; // clear frozen snapshots
            _char.StatBaselineJson    = null;
        }
        await Db.SaveChangesAsync();
        Nav.NavigateTo($"/character/{Id}/sheet");
    }

    static int NextLevelUpStep(int s) => s switch { 1 => 3, 3 => 4, _ => 99 };
    static int PrevLevelUpStep(int s) => s switch { 1 => -1, 3 => 1, _ => 3 };

    bool ValidateStep(int step) => step switch
    {
        0 => ValidateConcept(),
        1 => true,
        2 => ValidateProfSkills(),
        _ => true
    };

    bool ValidateConcept()
    {
        if (string.IsNullOrWhiteSpace(_char!.Name))      { _error = "Name is required."; return false; }
        if (string.IsNullOrWhiteSpace(_char.Race))        { _error = "Race is required."; return false; }
        if (string.IsNullOrWhiteSpace(_char.Culture))     { _error = "Culture is required."; return false; }
        if (string.IsNullOrWhiteSpace(_char.Profession))  { _error = "Profession is required."; return false; }
        return true;
    }

    bool ValidateProfSkills()
    {
        if (_chosenPro.Count != 10) { _error = "Choose exactly 10 Professional Skills."; return false; }
        if (_chosenKnack.Count != 2) { _error = "Choose exactly 2 Knacks."; return false; }
        return true;
    }

    async Task SaveStepAsync()
    {
        switch (_step)
        {
            case 0: await SaveConceptAsync(); break;
            case 1: await SaveStatsAsync(); break;
            case 2: await SaveProfSkillsAsync(); break;
            case 3: await SaveSkillsAsync(); break;
            case 4: await Db.SaveChangesAsync(); break;   // talents (EF-tracked entities)
            case 5: await SaveEquipmentAsync(); break;
        }
    }

    async Task SaveConceptAsync()
    {
        foreach (var (_, entries) in _poolAllocs)
        {
            foreach (var entry in entries)
            {
                var cat = SkillRules.CategoryOf(entry.SkillName)?.Name ?? "";
                var existing = _char!.Skills.FirstOrDefault(s =>
                    s.SkillName == entry.SkillName && s.Specialization == entry.Specialization);
                if (existing is null)
                {
                    existing = new CharacterSkill
                    {
                        CharacterId = _char.Id, Category = cat,
                        SkillName = entry.SkillName, Specialization = entry.Specialization
                    };
                    _char.Skills.Add(existing);
                    Db.CharacterSkills.Add(existing);
                }
                existing.CulturalRanks = entry.Ranks;
            }
        }
        Db.WriteAuditLogs(_currentUserId);
        await Db.SaveChangesAsync();
    }

    async Task SaveStatsAsync()
    {
        if (!_isLevelUp)
        {
            foreach (var st in _char!.Stats)
            {
                if (_rolls.TryGetValue(st.Stat, out var r))
                {
                    st.Temporary = r.Temp;
                    st.Potential = r.Potential;
                }
            }
        }
        // For level-up, stat objects were already mutated in-place by RollStatGain.
        await Db.SaveChangesAsync();
    }

    async Task SaveProfSkillsAsync()
    {
        var prof = ProfessionRules.ByName.GetValueOrDefault(_char!.Profession ?? "");
        if (prof is null) return;

        foreach (var skillName in _chosenPro.Union(_chosenKnack))
        {
            var cat = SkillRules.CategoryOf(skillName)?.Name ?? "";
            var existing = _char.Skills.FirstOrDefault(s => s.SkillName == skillName);
            if (existing is null)
            {
                existing = new CharacterSkill { CharacterId = _char.Id, Category = cat, SkillName = skillName };
                _char.Skills.Add(existing);
                Db.CharacterSkills.Add(existing);
            }
            existing.IsProfessionalSkill = _chosenPro.Contains(skillName);
            existing.IsKnack = _chosenKnack.Contains(skillName);
        }
        await Db.SaveChangesAsync();
    }

    async Task SaveSkillsAsync()
    {
        foreach (var (catName, skills) in _purchased)
        {
            foreach (var (skillName, ranks) in skills)
            {
                if (WeaponRules.BySkill.ContainsKey(skillName)) continue; // saved separately
                var cat = SkillRules.CategoryOf(skillName)?.Name ?? catName;
                var existing = _char!.Skills.FirstOrDefault(s => s.SkillName == skillName && s.Specialization == null);
                if (existing is null)
                {
                    existing = new CharacterSkill { CharacterId = _char.Id, Category = cat, SkillName = skillName };
                    _char.Skills.Add(existing);
                    Db.CharacterSkills.Add(existing);
                }
                existing.PurchasedRanks = ranks;
            }
        }

        // Weapon specializations
        foreach (var wa in _weaponAllocs)
        {
            const string cat = "Combat Training";
            var existing = _char!.Skills.FirstOrDefault(s =>
                s.SkillName == wa.SkillName && s.Specialization == wa.Specialization);
            if (existing is null)
            {
                existing = new CharacterSkill
                {
                    CharacterId = _char.Id, Category = cat,
                    SkillName = wa.SkillName, Specialization = wa.Specialization
                };
                _char.Skills.Add(existing);
                Db.CharacterSkills.Add(existing);
            }
            existing.PurchasedRanks = wa.Ranks;
        }

        // Spell list allocs
        foreach (var sa in _spellAllocs)
        {
            var existing = _char!.Skills.FirstOrDefault(s =>
                s.SkillName == sa.SkillName && s.Specialization == sa.Specialization);
            if (existing is null)
            {
                existing = new CharacterSkill
                {
                    CharacterId = _char.Id, Category = "Spellcasting",
                    SkillName = sa.SkillName, Specialization = sa.Specialization
                };
                _char.Skills.Add(existing);
                Db.CharacterSkills.Add(existing);
            }
            existing.PurchasedRanks = sa.Ranks;
        }

        // Generic specialized skill allocs — first remove any zero-rank orphans left from prior sessions
        var orphanedGeneric = _char!.Skills
            .Where(s => s.Specialization != null
                     && IsGenericSpecialized(s.SkillName)
                     && s.PurchasedRanks == 0
                     && s.CulturalRanks == 0)
            .ToList();
        foreach (var orphan in orphanedGeneric)
        {
            _char.Skills.Remove(orphan);
            Db.CharacterSkills.Remove(orphan);
        }

        foreach (var ga in _genericAllocs)
        {
            if (ga.Ranks == 0) continue; // never persist zero-rank allocs
            var cat = SkillRules.CategoryOf(ga.SkillName)?.Name ?? "";
            var existing = _char.Skills.FirstOrDefault(s =>
                s.SkillName == ga.SkillName && s.Specialization == ga.Specialization);
            if (existing is null)
            {
                existing = new CharacterSkill
                {
                    CharacterId = _char.Id, Category = cat,
                    SkillName = ga.SkillName, Specialization = ga.Specialization
                };
                _char.Skills.Add(existing);
                Db.CharacterSkills.Add(existing);
            }
            existing.PurchasedRanks = ga.Ranks;
        }

        await Db.SaveChangesAsync();
    }

    // ── DP cost calculation ───────────────────────────────────────────────────

    int CalcDpSpent()
    {
        if (_char?.Profession is null) return 0;
        var prof = ProfessionRules.ByName.GetValueOrDefault(_char.Profession);
        if (prof is null) return 0;

        int total = 0;

        // Generic purchased skills (excludes weapon specializations)
        foreach (var (catName, skills) in _purchased)
        {
            foreach (var (skillName, ranks) in skills)
            {
                if (ranks <= 0) continue;
                if (WeaponRules.BySkill.ContainsKey(skillName)) continue; // handled separately
                // Level-up: only count ranks added this session
                int countable = _isLevelUp
                    ? Math.Max(0, ranks - _baselinePurchased.GetValueOrDefault(skillName, 0))
                    : ranks;
                if (countable <= 0) continue;
                var costKey = ResolveCostKey(catName, skillName);
                if (!prof.Costs.TryGetValue(costKey, out var costs)) continue;
                total += costs.First;
                if (countable >= 2) total += costs.Second;
            }
        }

        // Weapon specializations — position in list determines CT tier (1st=CT1, 2nd=CT2, …)
        for (int i = 0; i < _weaponAllocs.Count; i++)
        {
            var wa = _weaponAllocs[i];
            if (wa.Ranks <= 0) continue;
            int countable = _isLevelUp
                ? Math.Max(0, wa.Ranks - _baselineWeaponAllocs.GetValueOrDefault((wa.SkillName, wa.Specialization), 0))
                : wa.Ranks;
            if (countable <= 0) continue;
            var costKey = WeaponRules.CostKeyForSlot(i);
            if (!prof.Costs.TryGetValue(costKey, out var wc)) continue;
            total += wc.First;
            if (countable >= 2) total += wc.Second;
        }

        // Spell list allocs
        foreach (var sa in _spellAllocs)
        {
            if (sa.Ranks <= 0) continue;
            int countable = _isLevelUp
                ? Math.Max(0, sa.Ranks - _baselineSpellAllocs.GetValueOrDefault((sa.SkillName, sa.Specialization), 0))
                : sa.Ranks;
            if (countable <= 0) continue;
            var costKey = ResolveSpellCostKey(sa.SkillName);
            if (!prof.Costs.TryGetValue(costKey, out var sc)) continue;
            total += sc.First;
            if (countable >= 2) total += sc.Second;
        }

        // Generic specialized skill allocs (Language: X, Administration: Y, etc.)
        foreach (var ga in _genericAllocs)
        {
            if (ga.Ranks <= 0) continue;
            int countable = _isLevelUp
                ? Math.Max(0, ga.Ranks - _baselineGenericAllocs.GetValueOrDefault((ga.SkillName, ga.Specialization), 0))
                : ga.Ranks;
            if (countable <= 0) continue;
            var gaCat = SkillRules.CategoryOf(ga.SkillName)?.Name ?? "";
            var gaCostKey = ResolveCostKey(gaCat, ga.SkillName);
            if (!prof.Costs.TryGetValue(gaCostKey, out var gc)) continue;
            total += gc.First;
            if (countable >= 2) total += gc.Second;
        }

        return total;
    }

    // Ranks added during this wizard session (for level-up rank limit and DP cost tracking).
    int NewRanksThisLevel(string skillName) => _isLevelUp
        ? Math.Max(0, GetPurchased(skillName) - _baselinePurchased.GetValueOrDefault(skillName, 0))
        : GetPurchased(skillName);

    int NewWeaponRanksThisLevel(WeaponAlloc wa) => _isLevelUp
        ? Math.Max(0, wa.Ranks - _baselineWeaponAllocs.GetValueOrDefault((wa.SkillName, wa.Specialization), 0))
        : wa.Ranks;

    int NewSpellRanksThisLevel(SpellAlloc sa) => _isLevelUp
        ? Math.Max(0, sa.Ranks - _baselineSpellAllocs.GetValueOrDefault((sa.SkillName, sa.Specialization), 0))
        : sa.Ranks;

    int NewGenericRanksThisLevel(GenericAlloc ga) => _isLevelUp
        ? Math.Max(0, ga.Ranks - _baselineGenericAllocs.GetValueOrDefault((ga.SkillName, ga.Specialization), 0))
        : ga.Ranks;

    int MinPurchasedRanks(string skillName) => _isLevelUp
        ? _baselinePurchased.GetValueOrDefault(skillName, 0)
        : 0;

    static string ResolveCostKey(string category, string skillName = "") => category switch
    {
        "Battle Expertise" => "Battle Expertise",
        "Combat Expertise" => "Combat Expertise",
        "Combat Training"  => "Combat Training 1",
        "Spellcasting"     => ResolveSpellCostKey(skillName),
        _                  => category
    };

    static string ResolveSpellCostKey(string skillName) => skillName switch
    {
        "Magical Ritual"            => "Spells: Ritual Magic",
        "Closed Spell Lists"        => "Spells: Closed",
        "Arcane Spell Lists"        => "Spells: Arcane",
        "Restricted Spell Lists"    => "Spells: Restricted",
        _                           => "Spells: Base/Open",
    };

    int GetPurchased(string skillName) =>
        SkillRules.CategoryOf(skillName) is { } cat
        && _purchased.TryGetValue(cat.Name, out var d)
        && d.TryGetValue(skillName, out var r) ? r : 0;

    void SetPurchased(string skillName, int ranks)
    {
        var cat = SkillRules.CategoryOf(skillName)?.Name;
        if (cat is null) return;
        if (!_purchased.ContainsKey(cat)) _purchased[cat] = new();
        _purchased[cat][skillName] = ranks;
    }

    int GetCulturalRanks(string skillName) =>
        _char?.Skills.FirstOrDefault(s => s.SkillName == skillName)?.CulturalRanks ?? 0;

    // ── Stat helpers ──────────────────────────────────────────────────────────

    int GetTemp(StatName sn) => _rolls.TryGetValue(sn, out var r) ? r.Temp
        : _char?.Stats.FirstOrDefault(s => s.Stat == sn)?.Temporary ?? 50;

    int GetPotential(StatName sn) => _rolls.TryGetValue(sn, out var r) ? r.Potential
        : _char?.Stats.FirstOrDefault(s => s.Stat == sn)?.Potential ?? 60;

    int GetRacialMod(StatName sn)
    {
        if (_char?.Race is null) return 0;
        return RaceRules.ByName.TryGetValue(_char.Race, out var race) ? race.GetStatMod(sn) : 0;
    }

    void RollAllStats()
    {
        var rng = new Random();
        _rolls.Clear();
        foreach (StatName sn in Enum.GetValues<StatName>())
        {
            int a = RollStat(rng), b = RollStat(rng), c = RollStat(rng);
            var sorted = new[] { a, b, c }.OrderDescending().ToArray();
            _rolls[sn] = new StatRoll(sorted[1], sorted[0]);
        }
        _boostsRemaining = 2;
        _swapsRemaining = 2;
        _swapA = null;
        _pendingBoostType = null;
    }

    static int RollStat(Random rng)
    {
        int r;
        do r = rng.Next(1, 101); while (r < 11);
        return r;
    }

    void ApplyBoost(int boostType)
    {
        if (_boostsRemaining <= 0 || !_rolls.Any()) return;
        // Types 0 and 3 require the player to pick a stat row
        if (boostType is 0 or 3) { _pendingBoostType = boostType; return; }

        var sorted = _rolls.OrderByDescending(kv => kv.Value.Temp).ToList();
        _boostsRemaining--;
        switch (boostType)
        {
            case 1:
                var top = sorted[0].Key;
                _rolls[top] = new StatRoll(90, Math.Min(100, _rolls[top].Potential + 10));
                break;
            case 2:
                if (sorted.Count >= 2)
                {
                    var sec = sorted[1].Key;
                    _rolls[sec] = new StatRoll(85, Math.Min(100, _rolls[sec].Potential + 10));
                }
                break;
        }
    }

    void ApplyBoostToStat(StatName sn)
    {
        if (_boostsRemaining <= 0 || _pendingBoostType is null) return;
        var rng = new Random();
        switch (_pendingBoostType.Value)
        {
            case 0:
                _rolls[sn] = new StatRoll(56, 78);
                _boostsRemaining--;
                break;
            case 3:
                int cur = GetTemp(sn), pot = GetPotential(sn);
                int gain = GainRoll(rng, cur) + GainRoll(rng, cur);
                _rolls[sn] = new StatRoll(Math.Min(pot, cur + gain), pot);
                _boostsRemaining--;
                break;
        }
        _pendingBoostType = null;
    }

    static int GainRoll(Random rng, int temp) => temp switch
    {
        <= 6  => Math.Max(0, rng.Next(1, 4) - 1),
        <= 8  => rng.Next(1, 4),
        <= 18 => rng.Next(1, 7),
        <= 81 => rng.Next(1, 11),
        <= 90 => rng.Next(1, 7),
        <= 92 => rng.Next(1, 4),
        _     => Math.Max(0, rng.Next(1, 4) - 1)
    };

    // ── Stat gain (level-up only) ─────────────────────────────────────────────

    void RollStatGain(StatName sn)
    {
        var stat = _char!.Stats.FirstOrDefault(s => s.Stat == sn);
        if (stat is null) return;
        ApplyStatGainRoll(sn, stat, OpenEndedD100(new Random()));
    }

    void ApplyManualRoll(StatName sn)
    {
        var stat = _char!.Stats.FirstOrDefault(s => s.Stat == sn);
        if (stat is null) return;
        if (!_manualRollInputs.TryGetValue(sn, out var raw)) return;
        if (!int.TryParse(raw, out int roll) || roll < 1) return;
        ApplyStatGainRoll(sn, stat, roll);
    }

    void ApplyStatGainRoll(StatName sn, CharacterStat stat, int roll)
    {
        // Restore to baseline first so re-rolls don't stack gains
        if (_statBaseline.TryGetValue(sn, out var bl))
        {
            stat.Temporary  = bl.Temp;
            stat.Potential  = bl.Potential;
        }
        bool gained = roll > stat.Temporary;
        if (gained)
        {
            stat.Temporary++;
            if (stat.Temporary >= stat.Potential) stat.Potential++;
        }
        _statGainRolls[sn] = (roll, gained);
    }

    void RollAllStatGains()
    {
        foreach (StatName sn in Enum.GetValues<StatName>())
            if (!_statGainRolls.ContainsKey(sn))
                RollStatGain(sn);
    }

    static int OpenEndedD100(Random rng)
    {
        int result = 0;
        int chunk;
        do { chunk = rng.Next(1, 101); result += chunk; } while (chunk == 100);
        return result;
    }

    void StartSwap(StatName sn)
    {
        if (_swapA is null) { _swapA = sn; return; }
        if (_swapA == sn)  { _swapA = null; return; }
        if (_swapsRemaining <= 0) { _swapA = null; return; }

        var a = _rolls.TryGetValue(_swapA.Value, out var ra) ? ra
            : new StatRoll(GetTemp(_swapA.Value), GetPotential(_swapA.Value));
        var b = _rolls.TryGetValue(sn, out var rb) ? rb
            : new StatRoll(GetTemp(sn), GetPotential(sn));

        _rolls[_swapA.Value] = b;
        _rolls[sn] = a;
        _swapsRemaining--;
        _swapA = null;
    }

    // ── Race / culture helpers ────────────────────────────────────────────────

    void OnRaceChanged(string? raceName)
    {
        _char!.Race = raceName;
        _char.HeightCm = null;
        _char.WeightKg = null;
        if (raceName is not null && RaceRules.ByName.TryGetValue(raceName, out var race))
        {
            _char.RaceBonusDp = race.BonusDP;
            _dpBudget = 60 + Math.Min(25, race.BonusDP);
        }
        else
        {
            _char.RaceBonusDp = 0;
            _dpBudget = 60;
        }
    }

    // ── Weapon alloc helpers ──────────────────────────────────────────────────

    int GetWeaponPurchased(string skillName) =>
        _weaponAllocs.Where(w => w.SkillName == skillName).Sum(w => w.Ranks);

    void AddWeaponAlloc(string skillName)
    {
        if (string.IsNullOrWhiteSpace(_weaponAddSpec)) return;
        if (_weaponAddRanks <= 0) return;
        // Don't add a weapon that's already in the list (use +/- buttons instead)
        if (_weaponAllocs.Any(w => w.Specialization == _weaponAddSpec)) return;

        var prof = ProfessionRules.ByName.GetValueOrDefault(_char?.Profession ?? "");
        if (prof is null) return;

        // New weapon goes into the next global slot
        int newSlot = _weaponAllocs.Count;
        var costKey = WeaponRules.CostKeyForSlot(newSlot);
        if (!prof.Costs.TryGetValue(costKey, out var wc)) return;

        int addCost = wc.First;
        if (_weaponAddRanks >= 2) addCost += wc.Second;
        if (_dpBudget - _dpSpent < addCost) return;

        _weaponAllocs.Add(new WeaponAlloc(skillName, _weaponAddSpec, _weaponAddRanks));
        _weaponAddSpec = "";
        _weaponAddRanks = 1;
        _activeWeaponAdd = null;
    }

    void AdjWeaponRanks(WeaponAlloc wa, int delta)
    {
        int idx = _weaponAllocs.IndexOf(wa);
        if (idx < 0) return;
        int minRanks = _isLevelUp ? _baselineWeaponAllocs.GetValueOrDefault((wa.SkillName, wa.Specialization), 0) : 0;
        int next = Math.Max(minRanks, wa.Ranks + delta);
        if (next == wa.Ranks) return;

        if (delta > 0)
        {
            int newRanksCur = NewWeaponRanksThisLevel(wa);
            if (newRanksCur >= 2) return;
            var prof = ProfessionRules.ByName.GetValueOrDefault(_char?.Profession ?? "");
            if (prof is null) return;
            var costKey = WeaponRules.CostKeyForSlot(idx);
            if (!prof.Costs.TryGetValue(costKey, out var wc)) return;
            int addCost = newRanksCur == 0 ? wc.First : wc.Second;
            if (_dpBudget - _dpSpent < addCost) return;
        }

        if (next == 0)
            _weaponAllocs.RemoveAt(idx);
        else
            _weaponAllocs[idx] = wa with { Ranks = next };
    }

    void AddSpellAlloc(string skillName)
    {
        if (string.IsNullOrWhiteSpace(_spellAddName)) return;
        if (_spellAddRanks <= 0) return;
        string listName = _spellAddName.Trim();
        if (_spellAllocs.Any(s => s.SkillName == skillName && s.Specialization == listName)) return;

        var prof = ProfessionRules.ByName.GetValueOrDefault(_char?.Profession ?? "");
        if (prof is null) return;

        var costKey = ResolveSpellCostKey(skillName);
        if (!prof.Costs.TryGetValue(costKey, out var sc)) return;

        int addCost = sc.First;
        if (_spellAddRanks >= 2) addCost += sc.Second;
        if (_dpBudget - _dpSpent < addCost) return;

        _spellAllocs.Add(new SpellAlloc(skillName, listName, _spellAddRanks));
        _spellAddName = "";
        _spellAddRanks = 1;
        _activeSpellAdd = null;
    }

    void AdjSpellRanks(SpellAlloc sa, int delta)
    {
        int idx = _spellAllocs.IndexOf(sa);
        if (idx < 0) return;
        int minRanks = _isLevelUp ? _baselineSpellAllocs.GetValueOrDefault((sa.SkillName, sa.Specialization), 0) : 0;
        int next = Math.Max(minRanks, sa.Ranks + delta);
        if (next == sa.Ranks) return;

        if (delta > 0)
        {
            int newRanksCur = NewSpellRanksThisLevel(sa);
            if (newRanksCur >= 2) return;
            var prof = ProfessionRules.ByName.GetValueOrDefault(_char?.Profession ?? "");
            if (prof is null) return;
            var costKey = ResolveSpellCostKey(sa.SkillName);
            if (!prof.Costs.TryGetValue(costKey, out var sc)) return;
            int addCost = newRanksCur == 0 ? sc.First : sc.Second;
            if (_dpBudget - _dpSpent < addCost) return;
        }

        if (next == 0)
            _spellAllocs.RemoveAt(idx);
        else
            _spellAllocs[idx] = sa with { Ranks = next };
    }

    // ── Generic specialized skill helpers ─────────────────────────────────────

    static bool IsGenericSpecialized(string skillName)
    {
        var def = SkillRules.FindSkill(skillName);
        return def?.Specialized == true
            && !WeaponRules.BySkill.ContainsKey(skillName)
            && SkillRules.CategoryOf(skillName)?.Name != "Spellcasting";
    }

    void AddGenericAlloc()
    {
        if (string.IsNullOrWhiteSpace(_genericAddSkill) || string.IsNullOrWhiteSpace(_genericAddSpec)) return;
        string spec = _genericAddSpec.Trim();
        if (_genericAllocs.Any(g => g.SkillName == _genericAddSkill && g.Specialization == spec)) return;
        _genericAllocs.Add(new GenericAlloc(_genericAddSkill, spec, 0));
        _genericAddSkill = string.Empty;
        _genericAddSpec  = string.Empty;
    }

    void AdjGenericRanks(GenericAlloc ga, int delta)
    {
        int idx = _genericAllocs.IndexOf(ga);
        if (idx < 0) return;
        int minRanks = _isLevelUp ? _baselineGenericAllocs.GetValueOrDefault((ga.SkillName, ga.Specialization), 0) : 0;
        int next = Math.Max(minRanks, ga.Ranks + delta);
        if (next == ga.Ranks) return;

        if (delta > 0)
        {
            int newRanksCur = NewGenericRanksThisLevel(ga);
            if (newRanksCur >= 2) return;
            var prof = ProfessionRules.ByName.GetValueOrDefault(_char?.Profession ?? "");
            if (prof is null) return;
            var gaCat = SkillRules.CategoryOf(ga.SkillName)?.Name ?? "";
            var costKey = ResolveCostKey(gaCat, ga.SkillName);
            if (!prof.Costs.TryGetValue(costKey, out var gc)) return;
            int addCost = newRanksCur == 0 ? gc.First : gc.Second;
            if (_dpBudget - _dpSpent < addCost) return;
        }

        if (next == 0)
            _genericAllocs.RemoveAt(idx);
        else
            _genericAllocs[idx] = ga with { Ranks = next };
    }

    void OnProfessionChanged(string? profName)
    {
        _char!.Profession = profName;
        var prof = ProfessionRules.ByName.GetValueOrDefault(profName ?? "");
        if (prof?.Realm is { } r && !r.Contains("+"))
            _char.Realm = r;
        else if (prof?.IsSpellcaster == false)
            _char.Realm = null;
    }

    static (string Skill, string? Spec) MapCultureSkill(string n) => n switch
    {
        "Region (Own)"           => ("Region Lore", "Own"),
        "Region (Neighboring)"   => ("Region Lore", "Neighboring"),
        "Survival (Own region)"  => ("Survival", "Own region"),
        _                        => (n, null)
    };

    void ApplyCulturalRanks()
    {
        if (_char?.Culture is null) return;
        if (!CultureRules.ByName.TryGetValue(_char.Culture, out var cult)) return;

        foreach (var sk in _char.Skills) sk.CulturalRanks = 0;
        _poolAllocs.Clear();
        _activePoolAdd = null;

        foreach (var csk in cult.Skills.Where(s => s.Ranks > 0 && !s.IsPool))
        {
            var (skillName, spec) = MapCultureSkill(csk.Skill);
            var cat = SkillRules.CategoryOf(skillName)?.Name ?? "";
            var existing = _char.Skills.FirstOrDefault(s =>
                s.SkillName == skillName && s.Specialization == spec);
            if (existing is null)
            {
                existing = new CharacterSkill
                {
                    CharacterId = _char.Id, Category = cat,
                    SkillName = skillName, Specialization = spec
                };
                _char.Skills.Add(existing);
                Db.CharacterSkills.Add(existing);
            }
            existing.CulturalRanks = csk.Ranks;
        }
    }

    // ── Pool allocation helpers ───────────────────────────────────────────────

    static IEnumerable<string> GetPoolSkills(string poolName) => poolName switch
    {
        "Languages"               => ["Language"],
        "Crafting/Vocation"       => SkillRules.Categories
                                        .Where(c => c.Name is "Crafting" or "Vocation")
                                        .SelectMany(c => c.Skills).Select(s => s.Name),
        "Composition/Performance" => SkillRules.Categories
                                        .Where(c => c.Name is "Composition" or "Performance Art")
                                        .SelectMany(c => c.Skills).Select(s => s.Name),
        "Other Lores"             => SkillRules.Categories
                                        .Where(c => c.Name == "Lore")
                                        .SelectMany(c => c.Skills)
                                        .Where(s => s.Name != "Language")
                                        .Select(s => s.Name),
        _                         => []
    };

    static bool PoolSkillNeedsSpec(string skillName) =>
        skillName == "Language" || (SkillRules.FindSkill(skillName)?.Specialized ?? false);

    int PoolUsed(string poolName) =>
        _poolAllocs.TryGetValue(poolName, out var entries) ? entries.Sum(e => e.Ranks) : 0;

    void AddPoolEntry(string poolName, int poolMax)
    {
        if (string.IsNullOrWhiteSpace(_poolAddSkill)) return;
        bool needsSpec = PoolSkillNeedsSpec(_poolAddSkill);
        if (needsSpec && string.IsNullOrWhiteSpace(_poolAddSpec)) return;
        int available = poolMax - PoolUsed(poolName);
        if (available <= 0 || _poolAddRanks <= 0) return;
        int ranks = Math.Min(_poolAddRanks, available);
        string? spec = needsSpec ? _poolAddSpec.Trim() : null;

        if (!_poolAllocs.ContainsKey(poolName)) _poolAllocs[poolName] = new();

        var existing = _poolAllocs[poolName].FirstOrDefault(e =>
            e.SkillName == _poolAddSkill && e.Specialization == spec);
        if (existing is not null)
        {
            int idx = _poolAllocs[poolName].IndexOf(existing);
            _poolAllocs[poolName][idx] = existing with { Ranks = existing.Ranks + ranks };
        }
        else
        {
            _poolAllocs[poolName].Add(new PoolEntry(_poolAddSkill, spec, ranks));
        }

        _poolAddSkill = "";
        _poolAddSpec = "";
        _poolAddRanks = 1;
        _activePoolAdd = null;
    }

    void RemovePoolEntry(string poolName, PoolEntry entry)
    {
        if (_poolAllocs.TryGetValue(poolName, out var entries))
            entries.Remove(entry);
    }

    // ── Equipment helpers ─────────────────────────────────────────────────────

    void LoadEquipmentState()
    {
        _equipQty.Clear();
        foreach (var item in _char!.EquipmentItems)
            _equipQty[item.Name] = item.Qty;
    }

    async Task SaveEquipmentAsync()
    {
        // Remove all existing items, then re-insert non-zero quantities
        Db.CharacterEquipmentItems.RemoveRange(_char!.EquipmentItems);
        _char.EquipmentItems.Clear();

        foreach (var (name, qty) in _equipQty.Where(kv => kv.Value > 0))
        {
            var item = new CharacterEquipmentItem { CharacterId = _char.Id, Name = name, Qty = qty };
            _char.EquipmentItems.Add(item);
            Db.CharacterEquipmentItems.Add(item);
        }

        await Db.SaveChangesAsync();
    }

    int GetEquipQty(string name) => _equipQty.TryGetValue(name, out var q) ? q : 0;

    void AdjEquipQty(string name, int delta)
    {
        int current = GetEquipQty(name);
        int next = Math.Max(0, current + delta);
        if (next == 0)
            _equipQty.Remove(name);
        else
            _equipQty[name] = next;
    }

    int TotalEquipWeight =>
        _equipQty.Sum(kv =>
        {
            double wt = EquipmentRules.General.FirstOrDefault(i => i.Name == kv.Key)?.WeightLbs
                     ?? EquipmentRules.Weapons.FirstOrDefault(i => i.Name == kv.Key)?.WeightLbs
                     ?? 0;
            return (int)(wt * kv.Value);
        });
}
