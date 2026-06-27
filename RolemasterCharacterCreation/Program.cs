using Anthropic;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RolemasterCharacterCreation.Client;
using RolemasterCharacterCreation.Client.Pages;
using RolemasterCharacterCreation.Components;
using RolemasterCharacterCreation.Data;
using RolemasterCharacterCreation.Identity;
using RolemasterCharacterCreation.Models;
using RolemasterCharacterCreation.Services;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequiredLength = 8;
        // Passwords use only digits + upper/lower letters (easy to type from an SMS).
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddIdentityCookies();

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, PersistingAuthenticationStateProvider>();

// Rules assistant (Claude answers + Voyage embeddings RAG).
// API keys come from ANTHROPIC_API_KEY / VOYAGE_API_KEY (env vars or user-secrets), never config files.
builder.Services.Configure<RulesAssistantOptions>(builder.Configuration.GetSection(RulesAssistantOptions.SectionName));
builder.Services.AddHttpClient<IEmbeddingClient, VoyageEmbeddingClient>();
builder.Services.AddSingleton(_ =>
{
    var key = builder.Configuration["ANTHROPIC_API_KEY"];
    return string.IsNullOrWhiteSpace(key) ? new AnthropicClient() : new AnthropicClient { ApiKey = key };
});
builder.Services.AddSingleton<RulesIndexService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<RulesIndexService>());

// Real-time chat broker (in-memory pub/sub + presence; persistence is done by pages)
builder.Services.AddSingleton<ChatService>();

// SMS invites (Textbelt). BaseUrl from the "Textbelt" config section; the API key comes from
// TEXTBELT_API_KEY (env var / user-secret), read inside the sender — never from appsettings.
builder.Services.Configure<TextbeltOptions>(builder.Configuration.GetSection(TextbeltOptions.SectionName));
builder.Services.AddHttpClient<ISmsSender, TextbeltSmsSender>();

// Needed so App.razor can detect a phone player during SSR (chat-only lockdown).
builder.Services.AddHttpContextAccessor();

// Creature lookup
builder.Services.AddSingleton<CreatureService>();

// Misc reference tables (static, loaded from docs/game-data/reference-tables.json)
builder.Services.AddSingleton<ReferenceTableService>();

// Magic items (static, GM-only, loaded from docs/game-data/magic-items.json)
builder.Services.AddSingleton<MagicItemService>();

// LLM-backed magic item generator (Claude); GM-only Create Magic Item page
builder.Services.AddSingleton<MagicItemGenerator>();

// Creature overview stat tables (static, GM-only, loaded from docs/game-data/creature-tables.json)
builder.Services.AddSingleton<CreatureTableService>();

// Campaign town map geometry parser (stateless)
builder.Services.AddSingleton<TownMapService>();

// Campaign world (hex region) map parser (stateless)
builder.Services.AddSingleton<WorldMapService>();

// Campaign dungeon (One-Page Dungeon grid) map parser (stateless)
builder.Services.AddSingleton<DungeonMapService>();

// Campaign cave (uploaded SVG + grid overlay) map preparer (stateless)
builder.Services.AddSingleton<CaveMapService>();

// Campaign building (uploaded multi-floor SVG + per-floor grid) map preparer (stateless)
builder.Services.AddSingleton<BuildingMapService>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

var app = builder.Build();

// Dev tool: `dotnet run -- --export-creatures` dumps parser-extracted descriptions
// to docs/game-data/creature-descriptions.json for review, then exits.
if (args.Contains("--export-creatures"))
{
    ExportCreatureDescriptions(app.Services);
    return;
}

// Dev tool: `dotnet run -- --list-names` dumps every parser creature name (the
// names the page looks up by) to scripts/creature-names.json, then exits.
if (args.Contains("--list-names"))
{
    var svc = app.Services.GetRequiredService<CreatureService>();
    var names = svc.All.Select(c => c.Name).Distinct().OrderBy(n => n).ToList();
    var path = Path.GetFullPath(Path.Combine(
        app.Services.GetRequiredService<IWebHostEnvironment>().ContentRootPath,
        "../scripts/creature-names.json"));
    File.WriteAllText(path, System.Text.Json.JsonSerializer.Serialize(names,
        new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
    Console.WriteLine($"Wrote {names.Count} creature names to {path}");
    return;
}

// Apply pending EF migrations and seed on every startup.
await MigrateAsync(app.Services);
await SeedAsync(app.Services);
await SeedCampaignSettingsAsync(app.Services);
await SeedCreatureDescriptionsAsync(app.Services);
await SeedAttackTablesAsync(app.Services);
await SeedCriticalTablesAsync(app.Services);
await SeedFumbleTablesAsync(app.Services);
await SeedSpellFailureTablesAsync(app.Services);
await SeedSpellListsAsync(app.Services);
await SeedMapCategoriesAsync(app.Services);
await SeedMapCategoryNamesAsync(app.Services);
await SeedVillageCategoriesAsync(app.Services);
await SeedVillageCategoryNamesAsync(app.Services);
await SeedDungeonCategoriesAsync(app.Services);
await SeedDungeonCategoryNamesAsync(app.Services);
await SeedCaveCategoriesAsync(app.Services);
await SeedCaveCategoryNamesAsync(app.Services);
await SeedBuildingCategoriesAsync(app.Services);
await SeedBuildingCategoryNamesAsync(app.Services);
await SeedWorldCategoriesAsync(app.Services);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// First-login gate: an invited player still carrying the "profile setup required" claim is
// confined to the complete-profile page (set email + a real password) until they finish. The
// claim rides in the auth cookie, so this costs no database hit, and is cleared by a refreshed
// sign-in once the profile is completed.
app.Use(async (context, next) =>
{
    var user = context.User;
    if (user.Identity?.IsAuthenticated == true
        && user.HasClaim(InviteClaims.Type, InviteClaims.Required)
        && !ProfileSetupAllowedPath(context.Request.Path))
    {
        context.Response.Redirect("/account/complete-profile");
        return;
    }
    await next();
});

// Chat-only lockdown for phones: an authenticated Player (not the GM) on a phone is
// confined to the chat app — any other request is redirected to /chat. This both lands
// them in chat right after login and blocks every other route. Tablets/desktops are
// unaffected, and so is the GM.
app.Use(async (context, next) =>
{
    var user = context.User;
    if (user.Identity?.IsAuthenticated == true
        && user.IsInRole(Roles.Player)
        && !user.IsInRole(Roles.Gamemaster)
        && MobileDetection.IsPhone(context.Request.Headers.UserAgent.ToString())
        && !PhoneAllowedPath(context.Request.Path))
    {
        context.Response.Redirect("/chat");
        return;
    }
    await next();
});

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(RolemasterCharacterCreation.Client._Imports).Assembly);

app.Run();

static async Task MigrateAsync(IServiceProvider services)
{
    await using var scope = services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

// Paths a locked-down phone player may still reach: the chat app itself, auth endpoints,
// Blazor/framework plumbing, error pages, and static asset files (those carry a '.').
static bool PhoneAllowedPath(PathString path)
{
    var p = path.HasValue ? path.Value! : "/";
    return p.StartsWith("/chat", StringComparison.OrdinalIgnoreCase)
        || p.StartsWith("/account", StringComparison.OrdinalIgnoreCase)
        || p.StartsWith("/_blazor", StringComparison.OrdinalIgnoreCase)
        || p.StartsWith("/_framework", StringComparison.OrdinalIgnoreCase)
        || p.StartsWith("/_content", StringComparison.OrdinalIgnoreCase)
        || p.StartsWith("/Error", StringComparison.OrdinalIgnoreCase)
        || p.StartsWith("/not-found", StringComparison.OrdinalIgnoreCase)
        || p.Contains('.');
}

// Paths an invited player awaiting profile completion may still reach: the complete-profile
// page itself, auth endpoints (so they can log out), Blazor/framework plumbing, and static
// asset files (those carry a '.'). Everything else redirects to complete-profile.
static bool ProfileSetupAllowedPath(PathString path)
{
    var p = path.HasValue ? path.Value! : "/";
    return p.StartsWith("/account", StringComparison.OrdinalIgnoreCase)
        || p.StartsWith("/_blazor", StringComparison.OrdinalIgnoreCase)
        || p.StartsWith("/_framework", StringComparison.OrdinalIgnoreCase)
        || p.StartsWith("/_content", StringComparison.OrdinalIgnoreCase)
        || p.StartsWith("/Error", StringComparison.OrdinalIgnoreCase)
        || p.StartsWith("/not-found", StringComparison.OrdinalIgnoreCase)
        || p.Contains('.');
}

static async Task SeedAsync(IServiceProvider services)
{
    await using var scope = services.CreateAsyncScope();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    foreach (var role in new[] { Roles.Gamemaster, Roles.Player })
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    const string gmEmail = "gamemaster@rolemaster.local";
    const string gmPassword = "GM@rolemaster1";

    if (await userManager.FindByEmailAsync(gmEmail) is null)
    {
        var gm = new ApplicationUser { UserName = gmEmail, Email = gmEmail, DisplayName = "Gamemaster" };
        var result = await userManager.CreateAsync(gm, gmPassword);
        if (result.Succeeded)
            await userManager.AddToRoleAsync(gm, Roles.Gamemaster);
    }
}

// Seeds the single campaign-settings row (name + login URL used in SMS invites). Idempotent:
// only inserts when the table is empty, so GM edits via the Campaign Settings page are kept.
static async Task SeedCampaignSettingsAsync(IServiceProvider services)
{
    await using var scope = services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (await db.CampaignSettings.AnyAsync()) return;

    db.CampaignSettings.Add(new CampaignSettings
    {
        Name = "Rolemaster Campaign",
        LoginUrl = "https://rolemaster.isager.dk",
    });
    await db.SaveChangesAsync();
}

// Path to the curated description file, relative to the content root.
static string CreatureDescriptionsPath(IWebHostEnvironment env) =>
    Path.GetFullPath(Path.Combine(DocsLocator.Root(env.ContentRootPath), "game-data/creature-descriptions.json"));

// Dev tool: serialise the parser's descriptions to JSON for manual review/cleanup.
static void ExportCreatureDescriptions(IServiceProvider services)
{
    var env  = services.GetRequiredService<IWebHostEnvironment>();
    var svc  = services.GetRequiredService<CreatureService>();
    var rows = svc.All
        .Where(c => !string.IsNullOrWhiteSpace(c.Description))
        .GroupBy(c => c.Name)
        .Select(g => new CreatureDescriptionDto(g.Key, g.First().Description))
        .OrderBy(d => d.Name, StringComparer.OrdinalIgnoreCase)
        .ToList();

    var path = CreatureDescriptionsPath(env);
    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
    var json = System.Text.Json.JsonSerializer.Serialize(rows,
        new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true,
            // Emit readable UTF-8 (real apostrophes/quotes) so the file is easy to hand-curate.
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        });
    File.WriteAllText(path, json, new System.Text.UTF8Encoding(false));
    Console.WriteLine($"Exported {rows.Count} creature descriptions to {path}");
}

// Seeds the CreatureDescriptions table from the curated JSON. Idempotent: inserts
// new names and updates changed text so re-running picks up hand edits to the file.
static async Task SeedCreatureDescriptionsAsync(IServiceProvider services)
{
    var env = services.GetRequiredService<IWebHostEnvironment>();
    var path = CreatureDescriptionsPath(env);
    if (!File.Exists(path)) return;

    var rows = System.Text.Json.JsonSerializer.Deserialize<List<CreatureDescriptionDto>>(
        await File.ReadAllTextAsync(path)) ?? [];
    if (rows.Count == 0) return;

    await using var scope = services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    var existing = await db.CreatureDescriptions.ToDictionaryAsync(c => c.Name, StringComparer.OrdinalIgnoreCase);
    var jsonNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    foreach (var row in rows)
    {
        if (string.IsNullOrWhiteSpace(row.Name)) continue;
        jsonNames.Add(row.Name);

        if (existing.TryGetValue(row.Name, out var entity))
        {
            if (entity.Description != row.Description)
                entity.Description = row.Description;
        }
        else
        {
            db.CreatureDescriptions.Add(new CreatureDescription
            {
                Name = row.Name,
                Description = row.Description
            });
        }
    }

    // The curated JSON is the source of truth: drop rows it no longer contains.
    foreach (var (name, entity) in existing)
        if (!jsonNames.Contains(name))
            db.CreatureDescriptions.Remove(entity);

    await db.SaveChangesAsync();
}

// Seeds the attack tables (Core Law Chapter 10) from the curated JSON. Idempotent:
// each table carries a content Signature, so an unchanged table is skipped and an
// edited one is rebuilt (scalars updated, child Weapons/Rows replaced). The
// AttackTable row itself is preserved on update so CharacterFavoriteAttack FKs
// survive; tables dropped from the JSON are removed (favorites cascade away).
static async Task SeedAttackTablesAsync(IServiceProvider services)
{
    var env = services.GetRequiredService<IWebHostEnvironment>();
    var path = Path.GetFullPath(Path.Combine(
        DocsLocator.Root(env.ContentRootPath), "game-data/attack-tables.json"));
    if (!File.Exists(path)) return;

    var opts = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    var tables = System.Text.Json.JsonSerializer.Deserialize<List<AttackTableDto>>(
        await File.ReadAllTextAsync(path), opts) ?? [];
    if (tables.Count == 0) return;

    await using var scope = services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    var existing = await db.AttackTables
        .Include(t => t.Weapons)
        .Include(t => t.Rows)
        .ToDictionaryAsync(t => t.Name, StringComparer.OrdinalIgnoreCase);
    var jsonNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    foreach (var dto in tables)
    {
        if (string.IsNullOrWhiteSpace(dto.Name)) continue;
        jsonNames.Add(dto.Name);

        var signature = Signature(dto, opts);
        if (existing.TryGetValue(dto.Name, out var entity))
        {
            if (entity.Signature == signature) continue;   // unchanged → skip
            db.AttackTableWeapons.RemoveRange(entity.Weapons);
            db.AttackTableRows.RemoveRange(entity.Rows);
        }
        else
        {
            entity = new AttackTable { Name = dto.Name };
            db.AttackTables.Add(entity);
        }

        entity.Category   = dto.Category ?? "Weapon";
        entity.CritTypes  = dto.CritTypes ?? "";
        entity.DisarmMod  = Blank(dto.DisarmMod);
        entity.SubdualMod = Blank(dto.SubdualMod);
        entity.Notes      = Blank(dto.Notes);
        entity.Signature  = signature;
        entity.Weapons    = (dto.Weapons ?? []).Select(w => new AttackTableWeapon
        {
            Name = w.Name ?? "", SizeMod = w.SizeMod, Length = w.Length,
            Strength = w.Strength, Weight = w.Weight, Fumble = w.Fumble,
        }).ToList();
        entity.Rows = (dto.Sizes ?? new()).SelectMany(kv =>
            (kv.Value ?? []).Select(r => new AttackTableRow
            {
                Size = kv.Key,
                RollLow = r.RollLow, RollHigh = r.RollHigh,
                At1 = Cell(r, 0), At2 = Cell(r, 1), At3 = Cell(r, 2), At4 = Cell(r, 3),
                At5 = Cell(r, 4), At6 = Cell(r, 5), At7 = Cell(r, 6), At8 = Cell(r, 7),
                At9 = Cell(r, 8), At10 = Cell(r, 9),
            })).ToList();
    }

    foreach (var (name, entity) in existing)
        if (!jsonNames.Contains(name))
            db.AttackTables.Remove(entity);

    await db.SaveChangesAsync();

    static string? Blank(string? s) => string.IsNullOrWhiteSpace(s) ? null : s;
    static string? Cell(AttackRowDto r, int i) =>
        r.Cells is { } c && i < c.Count ? Blank(c[i]) : null;
    static string Signature(AttackTableDto dto, System.Text.Json.JsonSerializerOptions o) =>
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(dto, o))));
}

// Seeds the critical strike tables (Core Law Section 11) from the curated JSON.
// Idempotent via a content Signature (same pattern as the attack tables): unchanged
// tables are skipped, edited ones rebuilt with their child rows replaced.
static async Task SeedCriticalTablesAsync(IServiceProvider services)
{
    var env = services.GetRequiredService<IWebHostEnvironment>();
    var path = Path.GetFullPath(Path.Combine(
        DocsLocator.Root(env.ContentRootPath), "game-data/critical-tables.json"));
    if (!File.Exists(path)) return;

    var opts = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    var tables = System.Text.Json.JsonSerializer.Deserialize<List<CriticalTableDto>>(
        await File.ReadAllTextAsync(path), opts) ?? [];
    if (tables.Count == 0) return;

    await using var scope = services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    var existing = await db.CriticalTables
        .Include(t => t.Rows)
        .ToDictionaryAsync(t => t.Name, StringComparer.OrdinalIgnoreCase);
    var jsonNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    foreach (var dto in tables)
    {
        if (string.IsNullOrWhiteSpace(dto.Name)) continue;
        jsonNames.Add(dto.Name);

        var signature = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(dto, opts))));
        if (existing.TryGetValue(dto.Name, out var entity))
        {
            if (entity.Signature == signature) continue;
            db.CriticalTableRows.RemoveRange(entity.Rows);
        }
        else
        {
            entity = new CriticalTable { Name = dto.Name };
            db.CriticalTables.Add(entity);
        }

        entity.Signature = signature;
        entity.Rows = (dto.Rows ?? []).Select(r => new CriticalTableRow
        {
            RollLow = r.RollLow, RollHigh = r.RollHigh, Location = r.Location,
            A = r.A ?? "", B = r.B ?? "", C = r.C ?? "", D = r.D ?? "", E = r.E ?? "",
        }).ToList();
    }

    foreach (var (name, entity) in existing)
        if (!jsonNames.Contains(name))
            db.CriticalTables.Remove(entity);

    await db.SaveChangesAsync();
}

// Seeds the fumble tables (Core Law Section 11) from the curated JSON. Idempotent
// via a content Signature, same pattern as the attack/critical tables.
static async Task SeedFumbleTablesAsync(IServiceProvider services)
{
    var env = services.GetRequiredService<IWebHostEnvironment>();
    var path = Path.GetFullPath(Path.Combine(
        DocsLocator.Root(env.ContentRootPath), "game-data/fumble-tables.json"));
    if (!File.Exists(path)) return;

    var opts = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    var tables = System.Text.Json.JsonSerializer.Deserialize<List<FumbleTableDto>>(
        await File.ReadAllTextAsync(path), opts) ?? [];
    if (tables.Count == 0) return;

    await using var scope = services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    var existing = await db.FumbleTables
        .Include(t => t.Rows)
        .ToDictionaryAsync(t => t.Name, StringComparer.OrdinalIgnoreCase);
    var jsonNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    foreach (var dto in tables)
    {
        if (string.IsNullOrWhiteSpace(dto.Name)) continue;
        jsonNames.Add(dto.Name);

        var signature = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(dto, opts))));
        if (existing.TryGetValue(dto.Name, out var entity))
        {
            if (entity.Signature == signature) continue;
            db.FumbleTableRows.RemoveRange(entity.Rows);
        }
        else
        {
            entity = new FumbleTable { Name = dto.Name };
            db.FumbleTables.Add(entity);
        }

        var cols = dto.Columns ?? [];
        entity.Signature = signature;
        entity.Col1Name = Col(cols, 0); entity.Col2Name = Col(cols, 1); entity.Col3Name = Col(cols, 2);
        entity.Col4Name = Col(cols, 3); entity.Col5Name = Col(cols, 4);
        entity.Rows = (dto.Rows ?? []).Select(r => new FumbleTableRow
        {
            RollLow = r.RollLow, RollHigh = r.RollHigh,
            Col1 = Cell(r, 0), Col2 = Cell(r, 1), Col3 = Cell(r, 2),
            Col4 = Cell(r, 3), Col5 = Cell(r, 4),
        }).ToList();
    }

    foreach (var (name, entity) in existing)
        if (!jsonNames.Contains(name))
            db.FumbleTables.Remove(entity);

    await db.SaveChangesAsync();

    static string Col(List<string> c, int i) => i < c.Count ? c[i] : "";
    static string Cell(FumbleRowDto r, int i) => r.Cells is { } c && i < c.Count ? c[i] ?? "" : "";
}

// Seeds the spell failure tables (Spell Law 4-4a/b/c) from the curated JSON.
// Idempotent via a content Signature, same pattern as the other reference tables.
static async Task SeedSpellFailureTablesAsync(IServiceProvider services)
{
    var env = services.GetRequiredService<IWebHostEnvironment>();
    var path = Path.GetFullPath(Path.Combine(
        DocsLocator.Root(env.ContentRootPath), "game-data/spell-failure-tables.json"));
    if (!File.Exists(path)) return;

    var opts = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    var tables = System.Text.Json.JsonSerializer.Deserialize<List<FumbleTableDto>>(
        await File.ReadAllTextAsync(path), opts) ?? [];
    if (tables.Count == 0) return;

    await using var scope = services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    var existing = await db.SpellFailureTables
        .Include(t => t.Rows)
        .ToDictionaryAsync(t => t.Name, StringComparer.OrdinalIgnoreCase);
    var jsonNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    foreach (var dto in tables)
    {
        if (string.IsNullOrWhiteSpace(dto.Name)) continue;
        jsonNames.Add(dto.Name);

        var signature = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(dto, opts))));
        if (existing.TryGetValue(dto.Name, out var entity))
        {
            if (entity.Signature == signature) continue;
            db.SpellFailureTableRows.RemoveRange(entity.Rows);
        }
        else
        {
            entity = new SpellFailureTable { Name = dto.Name };
            db.SpellFailureTables.Add(entity);
        }

        var cols = dto.Columns ?? [];
        entity.Signature = signature;
        entity.Col1Name = Col(cols, 0); entity.Col2Name = Col(cols, 1);
        entity.Col3Name = Col(cols, 2); entity.Col4Name = Col(cols, 3);
        entity.Rows = (dto.Rows ?? []).Select(r => new SpellFailureTableRow
        {
            RollLow = r.RollLow, RollHigh = r.RollHigh,
            Col1 = Cell(r, 0), Col2 = Cell(r, 1), Col3 = Cell(r, 2), Col4 = Cell(r, 3),
        }).ToList();
    }

    foreach (var (name, entity) in existing)
        if (!jsonNames.Contains(name))
            db.SpellFailureTables.Remove(entity);

    await db.SaveChangesAsync();

    static string Col(List<string> c, int i) => i < c.Count ? c[i] : "";
    static string Cell(FumbleRowDto r, int i) => r.Cells is { } c && i < c.Count ? c[i] ?? "" : "";
}

// Seeds the spell lists (Spell Law chapters 6-9) from the curated JSON.
// Idempotent via a per-list content Signature: unchanged lists are skipped,
// edited ones rebuilt with their child spells replaced.
static async Task SeedSpellListsAsync(IServiceProvider services)
{
    var env = services.GetRequiredService<IWebHostEnvironment>();
    var path = Path.GetFullPath(Path.Combine(
        DocsLocator.Root(env.ContentRootPath), "game-data/spell-lists.json"));
    if (!File.Exists(path)) return;

    var opts = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    var lists = System.Text.Json.JsonSerializer.Deserialize<List<SpellListDto>>(
        await File.ReadAllTextAsync(path), opts) ?? [];
    if (lists.Count == 0) return;

    await using var scope = services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Lists are keyed by (Category, Name) since list names can repeat across categories.
    var existing = await db.SpellLists.Include(l => l.Spells).ToListAsync();
    var byKey = existing.ToDictionary(l => (l.Category, l.Name));
    var seen = new HashSet<(string, string)>();

    foreach (var dto in lists)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Category)) continue;
        var k = (dto.Category!, dto.Name!);
        seen.Add(k);

        var signature = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(dto, opts))));
        if (byKey.TryGetValue(k, out var entity))
        {
            if (entity.Signature == signature) continue;
            db.Spells.RemoveRange(entity.Spells);
        }
        else
        {
            entity = new SpellList { Name = dto.Name! };
            db.SpellLists.Add(entity);
        }

        entity.Realm = dto.Realm ?? "";
        entity.Category = dto.Category!;
        entity.Profession = dto.Profession;
        entity.Code = dto.Code;
        entity.GmOnly = dto.GmOnly;
        entity.Signature = signature;
        entity.Spells = (dto.Spells ?? []).Select(s => new Spell
        {
            Level = s.Level, Name = s.Name ?? "", AreaOfEffect = s.Aoe ?? "",
            Duration = s.Duration ?? "", Range = s.Range ?? "", Type = s.Type ?? "",
            Description = s.Description ?? "",
        }).ToList();
    }

    foreach (var l in existing)
        if (!seen.Contains((l.Category, l.Name)))
            db.SpellLists.Remove(l);

    await db.SaveChangesAsync();
}

// Seeds the default fantasy map-category palette used to color-code town locations.
// Inserts the full set when empty; on existing installs, refreshes a built-in
// category's color only when it still holds its previous default (so a GM's custom
// color is never overwritten). The palette is chosen for strong visual separation.
static async Task SeedMapCategoriesAsync(IServiceProvider services)
{
    await using var scope = services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // (Name, new color, previous default color to migrate from)
    var defaults = new (string Name, string Color, string OldColor)[]
    {
        ("Tavern / Inn",            "#E67E22", "#E8B923"),
        ("Shop / Trader",           "#27AE60", "#3FA34D"),
        ("Blacksmith / Forge",      "#C0392B", "#D35400"),
        ("Temple / Shrine",         "#3498DB", "#6FB1FC"),
        ("Guild Hall",              "#8E44AD", "#8E44AD"),
        ("Apothecary / Alchemist",  "#1ABC9C", "#16A085"),
        ("Barracks / Watch",        "#2C3E50", "#5D6D7E"),
        ("Noble Manor",             "#F1C40F", "#C9A227"),
        ("Bank / Moneylender",      "#7F8C8D", "#F1C40F"),
        ("Mage Tower / Library",    "#00BCD4", "#2980B9"),
        ("Stables",                 "#8B5A2B", "#8B5A2B"),
        ("Brothel",                 "#E84393", "#E84393"),
    };

    var existing = await db.MapCategories.ToListAsync();

    if (existing.Count == 0)
    {
        for (int i = 0; i < defaults.Length; i++)
        {
            db.MapCategories.Add(new MapCategory
            {
                Name = defaults[i].Name,
                ColorHex = defaults[i].Color,
                SortOrder = i,
                IsBuiltIn = true,
            });
        }
        await db.SaveChangesAsync();
        return;
    }

    var changed = false;
    foreach (var d in defaults)
    {
        var cat = existing.FirstOrDefault(c =>
            c.IsBuiltIn && string.Equals(c.Name, d.Name, StringComparison.OrdinalIgnoreCase));
        if (cat is not null &&
            string.Equals(cat.ColorHex, d.OldColor, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(cat.ColorHex, d.Color, StringComparison.OrdinalIgnoreCase))
        {
            cat.ColorHex = d.Color;
            changed = true;
        }
    }

    if (changed) await db.SaveChangesAsync();
}

// Seeds the preset place-name lists for each built-in category from the curated JSON.
// Idempotent: only populates a category that currently has no names, matched by name,
// so GM renames and re-runs never duplicate or clobber.
static async Task SeedMapCategoryNamesAsync(IServiceProvider services)
{
    var env = services.GetRequiredService<IWebHostEnvironment>();
    var path = Path.GetFullPath(Path.Combine(
        DocsLocator.Root(env.ContentRootPath), "game-data/map-category-names.json"));
    if (!File.Exists(path)) return;

    var byCategory = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, List<string>>>(
        await File.ReadAllTextAsync(path)) ?? [];
    if (byCategory.Count == 0) return;

    await using var scope = services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    var categories = await db.MapCategories.Include(c => c.Names).ToListAsync();
    var changed = false;

    foreach (var (catName, names) in byCategory)
    {
        var category = categories.FirstOrDefault(c =>
            string.Equals(c.Name, catName, StringComparison.OrdinalIgnoreCase));
        if (category is null || category.Names.Count > 0) continue;

        foreach (var name in names.Where(n => !string.IsNullOrWhiteSpace(n)))
        {
            category.Names.Add(new MapCategoryName { Name = name.Trim() });
            changed = true;
        }
    }

    if (changed) await db.SaveChangesAsync();
}

// Seeds the default village legend palette (Tavern, General Store, Mill, …). Inserts
// the full set only when the table is empty so GM edits/additions are never overwritten.
// Kept separate from the town palette so villages have their own rural legend.
static async Task SeedVillageCategoriesAsync(IServiceProvider services)
{
    await using var scope = services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (await db.VillageCategories.AnyAsync()) return;

    var defaults = new (string Name, string Color)[]
    {
        ("Tavern / Inn",            "#E67E22"),
        ("General Store",           "#27AE60"),
        ("Blacksmith / Smithy",     "#C0392B"),
        ("Mill",                    "#8B5A2B"),
        ("Temple / Shrine",         "#3498DB"),
        ("Village Elder / Headman", "#8E44AD"),
        ("Farmstead",               "#7CB342"),
        ("Stable / Barn",           "#A0522D"),
        ("Healer / Herbalist",      "#1ABC9C"),
        ("Market Square",           "#F1C40F"),
        ("Well / Village Green",    "#00BCD4"),
        ("Militia / Watch Post",    "#2C3E50"),
    };

    for (int i = 0; i < defaults.Length; i++)
    {
        db.VillageCategories.Add(new VillageCategory
        {
            Name = defaults[i].Name,
            ColorHex = defaults[i].Color,
            SortOrder = i,
            IsBuiltIn = true,
        });
    }

    await db.SaveChangesAsync();
}

// Seeds the preset place-name lists for each built-in village category from the curated
// JSON. Idempotent: only populates a category that currently has no names, matched by
// name, so GM renames and re-runs never duplicate or clobber.
static async Task SeedVillageCategoryNamesAsync(IServiceProvider services)
{
    var env = services.GetRequiredService<IWebHostEnvironment>();
    var path = Path.GetFullPath(Path.Combine(
        DocsLocator.Root(env.ContentRootPath), "game-data/village-category-names.json"));
    if (!File.Exists(path)) return;

    var byCategory = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, List<string>>>(
        await File.ReadAllTextAsync(path)) ?? [];
    if (byCategory.Count == 0) return;

    await using var scope = services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    var categories = await db.VillageCategories.Include(c => c.Names).ToListAsync();
    var changed = false;

    foreach (var (catName, names) in byCategory)
    {
        var category = categories.FirstOrDefault(c =>
            string.Equals(c.Name, catName, StringComparison.OrdinalIgnoreCase));
        if (category is null || category.Names.Count > 0) continue;

        foreach (var name in names.Where(n => !string.IsNullOrWhiteSpace(n)))
        {
            category.Names.Add(new VillageCategoryName { Name = name.Trim() });
            changed = true;
        }
    }

    if (changed) await db.SaveChangesAsync();
}

// Seeds the default dungeon legend (Monster Lair, Trap, Secret Door, …). Inserts the
// full set only when the table is empty so GM edits/additions are never overwritten.
// Some legends are "hidden" — their markings are shown to the GM only.
static async Task SeedDungeonCategoriesAsync(IServiceProvider services)
{
    await using var scope = services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (await db.DungeonCategories.AnyAsync()) return;

    // (Name, color, hidden-from-players)
    var defaults = new (string Name, string Color, bool Hidden)[]
    {
        ("Monster Lair",          "#C0392B", false),
        ("Boss Chamber",          "#7B241C", false),
        ("Treasure Vault",        "#F1C40F", false),
        ("Altar / Shrine",        "#8E44AD", false),
        ("Prison / Cells",        "#5D6D7E", false),
        ("Tomb / Crypt",          "#6E4B3A", false),
        ("Armory",                "#2C3E50", false),
        ("Library / Study",       "#2980B9", false),
        ("Alchemy Lab",           "#1ABC9C", false),
        ("Fountain / Pool",       "#00BCD4", false),
        ("Statue / Monument",     "#95A5A6", false),
        ("Throne Room",           "#D4AC0D", false),
        ("Trap",                  "#E74C3C", true),
        ("Secret Door / Passage", "#9B59B6", true),
        ("Hidden Cache",          "#E67E22", true),
        ("Ambush",                "#34495E", true),
    };

    for (int i = 0; i < defaults.Length; i++)
    {
        db.DungeonCategories.Add(new DungeonCategory
        {
            Name = defaults[i].Name,
            ColorHex = defaults[i].Color,
            SortOrder = i,
            IsBuiltIn = true,
            IsHidden = defaults[i].Hidden,
        });
    }

    await db.SaveChangesAsync();
}

// Seeds the preset name lists for each built-in dungeon legend from the curated JSON.
// Idempotent: only populates a category that currently has no names, matched by name.
static async Task SeedDungeonCategoryNamesAsync(IServiceProvider services)
{
    var env = services.GetRequiredService<IWebHostEnvironment>();
    var path = Path.GetFullPath(Path.Combine(
        DocsLocator.Root(env.ContentRootPath), "game-data/dungeon-category-names.json"));
    if (!File.Exists(path)) return;

    var byCategory = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, List<string>>>(
        await File.ReadAllTextAsync(path)) ?? [];
    if (byCategory.Count == 0) return;

    await using var scope = services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    var categories = await db.DungeonCategories.Include(c => c.Names).ToListAsync();
    var changed = false;

    foreach (var (catName, names) in byCategory)
    {
        var category = categories.FirstOrDefault(c =>
            string.Equals(c.Name, catName, StringComparison.OrdinalIgnoreCase));
        if (category is null || category.Names.Count > 0) continue;

        foreach (var name in names.Where(n => !string.IsNullOrWhiteSpace(n)))
        {
            category.Names.Add(new DungeonCategoryName { Name = name.Trim() });
            changed = true;
        }
    }

    if (changed) await db.SaveChangesAsync();
}

// Seeds the default cave legend (Underground Lake, Crystal Vein, Beast Den, …). Inserts
// the full set only when the table is empty so GM edits/additions are never overwritten.
// Some legends are "hidden" — their markings are shown to the GM only.
static async Task SeedCaveCategoriesAsync(IServiceProvider services)
{
    await using var scope = services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (await db.CaveCategories.AnyAsync()) return;

    // (Name, color, hidden-from-players)
    var defaults = new (string Name, string Color, bool Hidden)[]
    {
        ("Underground Lake",      "#2980B9", false),
        ("Underground Stream",    "#00BCD4", false),
        ("Crystal Vein",          "#9B59B6", false),
        ("Mineral Deposit",       "#D4AC0D", false),
        ("Fungal Grove",          "#1ABC9C", false),
        ("Stalagmite Hall",       "#95A5A6", false),
        ("Bat Roost",             "#5D6D7E", false),
        ("Beast Den",             "#C0392B", false),
        ("Bottomless Chasm",      "#34495E", false),
        ("Cave-In / Rubble",      "#7F8C8D", false),
        ("Old Campsite",          "#27AE60", false),
        ("Cave Painting / Shrine","#8E44AD", false),
        ("Trap",                  "#E74C3C", true),
        ("Hidden Cache",          "#E67E22", true),
        ("Secret Passage",        "#6E4B3A", true),
        ("Ambush",                "#2C3E50", true),
    };

    for (int i = 0; i < defaults.Length; i++)
    {
        db.CaveCategories.Add(new CaveCategory
        {
            Name = defaults[i].Name,
            ColorHex = defaults[i].Color,
            SortOrder = i,
            IsBuiltIn = true,
            IsHidden = defaults[i].Hidden,
        });
    }

    await db.SaveChangesAsync();
}

// Seeds the preset name lists for the hidden cave legends from the curated JSON.
// Idempotent: only populates a category that currently has no names, matched by name.
static async Task SeedCaveCategoryNamesAsync(IServiceProvider services)
{
    var env = services.GetRequiredService<IWebHostEnvironment>();
    var path = Path.GetFullPath(Path.Combine(
        DocsLocator.Root(env.ContentRootPath), "game-data/cave-category-names.json"));
    if (!File.Exists(path)) return;

    var byCategory = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, List<string>>>(
        await File.ReadAllTextAsync(path)) ?? [];
    if (byCategory.Count == 0) return;

    await using var scope = services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    var categories = await db.CaveCategories.Include(c => c.Names).ToListAsync();
    var changed = false;

    foreach (var (catName, names) in byCategory)
    {
        var category = categories.FirstOrDefault(c =>
            string.Equals(c.Name, catName, StringComparison.OrdinalIgnoreCase));
        if (category is null || category.Names.Count > 0) continue;

        foreach (var name in names.Where(n => !string.IsNullOrWhiteSpace(n)))
        {
            category.Names.Add(new CaveCategoryName { Name = name.Trim() });
            changed = true;
        }
    }

    if (changed) await db.SaveChangesAsync();
}

// Seeds the default building legend (Bedroom, Kitchen, Cellar, …). Inserts the full set
// only when the table is empty so GM edits/additions are never overwritten. Some legends
// are "hidden" — their markings are shown to the GM only.
static async Task SeedBuildingCategoriesAsync(IServiceProvider services)
{
    await using var scope = services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (await db.BuildingCategories.AnyAsync()) return;

    // (Name, color, hidden-from-players)
    var defaults = new (string Name, string Color, bool Hidden)[]
    {
        ("Bedroom",            "#2980B9", false),
        ("Kitchen",            "#E67E22", false),
        ("Hearth / Fireplace", "#C0392B", false),
        ("Dining Hall",        "#D4AC0D", false),
        ("Study / Library",    "#8E44AD", false),
        ("Storage / Pantry",   "#7F8C8D", false),
        ("Cellar",             "#34495E", false),
        ("Workshop",           "#16A085", false),
        ("Shrine / Altar",     "#1ABC9C", false),
        ("Privy",              "#95A5A6", false),
        ("Stairs",             "#5D6D7E", false),
        ("Well",               "#00BCD4", false),
        ("Trap",               "#E74C3C", true),
        ("Hidden Cache",       "#E67E22", true),
        ("Secret Door",        "#6E4B3A", true),
        ("Ambush",             "#2C3E50", true),
    };

    for (int i = 0; i < defaults.Length; i++)
    {
        db.BuildingCategories.Add(new BuildingCategory
        {
            Name = defaults[i].Name,
            ColorHex = defaults[i].Color,
            SortOrder = i,
            IsBuiltIn = true,
            IsHidden = defaults[i].Hidden,
        });
    }

    await db.SaveChangesAsync();
}

// Seeds the preset name lists for the hidden building legends from the curated JSON.
// Idempotent: only populates a category that currently has no names, matched by name.
static async Task SeedBuildingCategoryNamesAsync(IServiceProvider services)
{
    var env = services.GetRequiredService<IWebHostEnvironment>();
    var path = Path.GetFullPath(Path.Combine(
        DocsLocator.Root(env.ContentRootPath), "game-data/building-category-names.json"));
    if (!File.Exists(path)) return;

    var byCategory = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, List<string>>>(
        await File.ReadAllTextAsync(path)) ?? [];
    if (byCategory.Count == 0) return;

    await using var scope = services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    var categories = await db.BuildingCategories.Include(c => c.Names).ToListAsync();
    var changed = false;

    foreach (var (catName, names) in byCategory)
    {
        var category = categories.FirstOrDefault(c =>
            string.Equals(c.Name, catName, StringComparison.OrdinalIgnoreCase));
        if (category is null || category.Names.Count > 0) continue;

        foreach (var name in names.Where(n => !string.IsNullOrWhiteSpace(n)))
        {
            category.Names.Add(new BuildingCategoryName { Name = name.Trim() });
            changed = true;
        }
    }

    if (changed) await db.SaveChangesAsync();
}

// Seeds the default world-map POI palette (City, Town, Dungeon, …). Inserts only when
// the table is empty so GM edits/additions are never overwritten.
static async Task SeedWorldCategoriesAsync(IServiceProvider services)
{
    await using var scope = services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (await db.WorldCategories.AnyAsync()) return;

    var defaults = new (string Name, string Color)[]
    {
        ("City",         "#C0392B"),
        ("Town",         "#E67E22"),
        ("Village",      "#F1C40F"),
        ("Castle / Keep","#8E44AD"),
        ("Dungeon",      "#2C3E50"),
        ("Ruins",        "#7F8C8D"),
        ("Temple",       "#3498DB"),
        ("Cave",         "#6E4B3A"),
        ("Building",     "#A0522D"),
        ("Camp",         "#27AE60"),
        ("Landmark",     "#16A085"),
        ("Port",         "#00BCD4"),
        ("Mine",         "#D4AC0D"),
    };

    for (int i = 0; i < defaults.Length; i++)
    {
        db.WorldCategories.Add(new WorldCategory
        {
            Name = defaults[i].Name,
            ColorHex = defaults[i].Color,
            SortOrder = i,
            IsBuiltIn = true,
        });
    }

    await db.SaveChangesAsync();
}

internal sealed record CreatureDescriptionDto(string Name, string Description);

internal sealed class SpellListDto
{
    public string? Name { get; set; }
    public string? Realm { get; set; }
    public string? Category { get; set; }
    public string? Profession { get; set; }
    public string? Code { get; set; }
    public bool GmOnly { get; set; }
    public List<SpellDto>? Spells { get; set; }
}

internal sealed class SpellDto
{
    public int Level { get; set; }
    public string? Name { get; set; }
    public string? Aoe { get; set; }
    public string? Duration { get; set; }
    public string? Range { get; set; }
    public string? Type { get; set; }
    public string? Description { get; set; }
}

internal sealed class FumbleTableDto
{
    public string? Name { get; set; }
    public List<string>? Columns { get; set; }
    public List<FumbleRowDto>? Rows { get; set; }
}

internal sealed class FumbleRowDto
{
    public int RollLow { get; set; }
    public int RollHigh { get; set; }
    public List<string?>? Cells { get; set; }
}

internal sealed class CriticalTableDto
{
    public string? Name { get; set; }
    public List<CriticalRowDto>? Rows { get; set; }
}

internal sealed class CriticalRowDto
{
    public int RollLow { get; set; }
    public int RollHigh { get; set; }
    public string? Location { get; set; }
    public string? A { get; set; }
    public string? B { get; set; }
    public string? C { get; set; }
    public string? D { get; set; }
    public string? E { get; set; }
}

internal sealed class AttackTableDto
{
    public string? Name { get; set; }
    public string? Category { get; set; }
    public string? CritTypes { get; set; }
    public string? DisarmMod { get; set; }
    public string? SubdualMod { get; set; }
    public string? Notes { get; set; }
    public List<AttackWeaponDto>? Weapons { get; set; }
    public Dictionary<string, List<AttackRowDto>>? Sizes { get; set; }
}

internal sealed class AttackWeaponDto
{
    public string? Name { get; set; }
    public string? SizeMod { get; set; }
    public string? Length { get; set; }
    public string? Strength { get; set; }
    public string? Weight { get; set; }
    public string? Fumble { get; set; }
}

internal sealed class AttackRowDto
{
    public int RollLow { get; set; }
    public int RollHigh { get; set; }
    public List<string?>? Cells { get; set; }
}
