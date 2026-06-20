using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using RolemasterCharacterCreation.Models;

namespace RolemasterCharacterCreation.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Character> Characters => Set<Character>();
    public DbSet<CharacterStat> CharacterStats => Set<CharacterStat>();
    public DbSet<CharacterSkill> CharacterSkills => Set<CharacterSkill>();
    public DbSet<CharacterTalent> CharacterTalents => Set<CharacterTalent>();
    public DbSet<CharacterEquipmentItem> CharacterEquipmentItems => Set<CharacterEquipmentItem>();
    public DbSet<CharacterAuditLog> CharacterAuditLogs => Set<CharacterAuditLog>();
    public DbSet<CreatureDescription> CreatureDescriptions => Set<CreatureDescription>();
    public DbSet<AttackTable> AttackTables => Set<AttackTable>();
    public DbSet<AttackTableWeapon> AttackTableWeapons => Set<AttackTableWeapon>();
    public DbSet<AttackTableRow> AttackTableRows => Set<AttackTableRow>();
    public DbSet<CharacterFavoriteAttack> CharacterFavoriteAttacks => Set<CharacterFavoriteAttack>();
    public DbSet<CriticalTable> CriticalTables => Set<CriticalTable>();
    public DbSet<CriticalTableRow> CriticalTableRows => Set<CriticalTableRow>();
    public DbSet<FumbleTable> FumbleTables => Set<FumbleTable>();
    public DbSet<FumbleTableRow> FumbleTableRows => Set<FumbleTableRow>();
    public DbSet<SpellFailureTable> SpellFailureTables => Set<SpellFailureTable>();
    public DbSet<SpellFailureTableRow> SpellFailureTableRows => Set<SpellFailureTableRow>();
    public DbSet<SpellList> SpellLists => Set<SpellList>();
    public DbSet<Spell> Spells => Set<Spell>();
    public DbSet<Town> Towns => Set<Town>();
    public DbSet<TownLocation> TownLocations => Set<TownLocation>();
    public DbSet<MapCategory> MapCategories => Set<MapCategory>();
    public DbSet<MapCategoryName> MapCategoryNames => Set<MapCategoryName>();
    public DbSet<Village> Villages => Set<Village>();
    public DbSet<VillageLocation> VillageLocations => Set<VillageLocation>();
    public DbSet<VillageCategory> VillageCategories => Set<VillageCategory>();
    public DbSet<VillageCategoryName> VillageCategoryNames => Set<VillageCategoryName>();
    public DbSet<DungeonMap> DungeonMaps => Set<DungeonMap>();
    public DbSet<DungeonLocation> DungeonLocations => Set<DungeonLocation>();
    public DbSet<DungeonCategory> DungeonCategories => Set<DungeonCategory>();
    public DbSet<DungeonCategoryName> DungeonCategoryNames => Set<DungeonCategoryName>();
    public DbSet<DungeonReveal> DungeonReveals => Set<DungeonReveal>();
    public DbSet<DungeonNote> DungeonNotes => Set<DungeonNote>();
    public DbSet<CaveMap> CaveMaps => Set<CaveMap>();
    public DbSet<CaveLocation> CaveLocations => Set<CaveLocation>();
    public DbSet<CaveCategory> CaveCategories => Set<CaveCategory>();
    public DbSet<CaveCategoryName> CaveCategoryNames => Set<CaveCategoryName>();
    public DbSet<CaveReveal> CaveReveals => Set<CaveReveal>();
    public DbSet<CaveNote> CaveNotes => Set<CaveNote>();
    public DbSet<BuildingMap> BuildingMaps => Set<BuildingMap>();
    public DbSet<BuildingLocation> BuildingLocations => Set<BuildingLocation>();
    public DbSet<BuildingCategory> BuildingCategories => Set<BuildingCategory>();
    public DbSet<BuildingCategoryName> BuildingCategoryNames => Set<BuildingCategoryName>();
    public DbSet<BuildingReveal> BuildingReveals => Set<BuildingReveal>();
    public DbSet<BuildingNote> BuildingNotes => Set<BuildingNote>();
    public DbSet<WorldMap> WorldMaps => Set<WorldMap>();
    public DbSet<WorldLocation> WorldLocations => Set<WorldLocation>();
    public DbSet<WorldCategory> WorldCategories => Set<WorldCategory>();
    public DbSet<WorldReveal> WorldReveals => Set<WorldReveal>();
    public DbSet<UsefulLink> UsefulLinks => Set<UsefulLink>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<ChatRead> ChatReads => Set<ChatRead>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Character>()
            .Property(c => c.Name)
            .HasMaxLength(100)
            .IsRequired();

        modelBuilder.Entity<Character>()
            .HasOne(c => c.User)
            .WithMany(u => u.Characters)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<CharacterStat>()
            .HasIndex(s => new { s.CharacterId, s.Stat })
            .IsUnique();

        modelBuilder.Entity<CharacterStat>()
            .HasOne(s => s.Character)
            .WithMany(c => c.Stats)
            .HasForeignKey(s => s.CharacterId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CharacterSkill>()
            .HasIndex(s => new { s.CharacterId, s.SkillName, s.Specialization })
            .IsUnique();

        modelBuilder.Entity<CharacterSkill>()
            .HasOne(s => s.Character)
            .WithMany(c => c.Skills)
            .HasForeignKey(s => s.CharacterId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CharacterTalent>()
            .HasOne(t => t.Character)
            .WithMany(c => c.Talents)
            .HasForeignKey(t => t.CharacterId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CharacterEquipmentItem>()
            .HasOne(e => e.Character)
            .WithMany(c => c.EquipmentItems)
            .HasForeignKey(e => e.CharacterId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CharacterEquipmentItem>()
            .HasIndex(e => new { e.CharacterId, e.Name })
            .IsUnique();

        modelBuilder.Entity<CharacterAuditLog>()
            .HasOne(a => a.Character)
            .WithMany(c => c.AuditLogs)
            .HasForeignKey(a => a.CharacterId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CharacterAuditLog>()
            .HasOne(a => a.ChangedByUser)
            .WithMany()
            .HasForeignKey(a => a.ChangedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<CreatureDescription>()
            .Property(c => c.Name)
            .HasMaxLength(200)
            .IsRequired();

        modelBuilder.Entity<CreatureDescription>()
            .HasIndex(c => c.Name)
            .IsUnique();

        modelBuilder.Entity<AttackTable>()
            .Property(a => a.Name)
            .HasMaxLength(100)
            .IsRequired();

        modelBuilder.Entity<AttackTable>()
            .HasIndex(a => a.Name)
            .IsUnique();

        modelBuilder.Entity<AttackTableWeapon>()
            .HasOne(w => w.AttackTable)
            .WithMany(a => a.Weapons)
            .HasForeignKey(w => w.AttackTableId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AttackTableRow>()
            .HasOne(r => r.AttackTable)
            .WithMany(a => a.Rows)
            .HasForeignKey(r => r.AttackTableId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AttackTableRow>()
            .HasIndex(r => new { r.AttackTableId, r.Size });

        modelBuilder.Entity<AttackTableRow>()
            .Ignore(r => r.Cells);

        modelBuilder.Entity<CharacterFavoriteAttack>()
            .HasOne(f => f.Character)
            .WithMany(c => c.FavoriteAttacks)
            .HasForeignKey(f => f.CharacterId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CharacterFavoriteAttack>()
            .HasOne(f => f.AttackTable)
            .WithMany()
            .HasForeignKey(f => f.AttackTableId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CharacterFavoriteAttack>()
            .HasIndex(f => new { f.CharacterId, f.AttackTableId })
            .IsUnique();

        modelBuilder.Entity<CriticalTable>()
            .Property(c => c.Name)
            .HasMaxLength(50)
            .IsRequired();

        modelBuilder.Entity<CriticalTable>()
            .HasIndex(c => c.Name)
            .IsUnique();

        modelBuilder.Entity<CriticalTableRow>()
            .HasOne(r => r.CriticalTable)
            .WithMany(c => c.Rows)
            .HasForeignKey(r => r.CriticalTableId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<FumbleTable>()
            .Property(f => f.Name)
            .HasMaxLength(50)
            .IsRequired();

        modelBuilder.Entity<FumbleTable>()
            .HasIndex(f => f.Name)
            .IsUnique();

        modelBuilder.Entity<FumbleTable>()
            .Ignore(f => f.ColumnNames);

        modelBuilder.Entity<FumbleTableRow>()
            .HasOne(r => r.FumbleTable)
            .WithMany(f => f.Rows)
            .HasForeignKey(r => r.FumbleTableId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<FumbleTableRow>()
            .Ignore(r => r.Cells);

        modelBuilder.Entity<SpellFailureTable>()
            .Property(f => f.Name)
            .HasMaxLength(50)
            .IsRequired();

        modelBuilder.Entity<SpellFailureTable>()
            .HasIndex(f => f.Name)
            .IsUnique();

        modelBuilder.Entity<SpellFailureTable>()
            .Ignore(f => f.ColumnNames);

        modelBuilder.Entity<SpellFailureTableRow>()
            .HasOne(r => r.SpellFailureTable)
            .WithMany(f => f.Rows)
            .HasForeignKey(r => r.SpellFailureTableId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SpellFailureTableRow>()
            .Ignore(r => r.Cells);

        modelBuilder.Entity<SpellList>()
            .Property(s => s.Name)
            .HasMaxLength(100)
            .IsRequired();

        modelBuilder.Entity<SpellList>()
            .HasIndex(s => new { s.Category, s.Name });

        modelBuilder.Entity<Spell>()
            .HasOne(s => s.SpellList)
            .WithMany(l => l.Spells)
            .HasForeignKey(s => s.SpellListId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Town>()
            .Property(t => t.Name)
            .HasMaxLength(120)
            .IsRequired();

        modelBuilder.Entity<MapCategory>()
            .Property(c => c.Name)
            .HasMaxLength(60)
            .IsRequired();

        modelBuilder.Entity<MapCategory>()
            .Property(c => c.ColorHex)
            .HasMaxLength(9)
            .IsRequired();

        modelBuilder.Entity<TownLocation>()
            .Property(l => l.FeatureKind)
            .HasMaxLength(20)
            .IsRequired();

        modelBuilder.Entity<TownLocation>()
            .HasOne(l => l.Town)
            .WithMany(t => t.Locations)
            .HasForeignKey(l => l.TownId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict so a category that is in use on any map cannot be deleted silently.
        modelBuilder.Entity<TownLocation>()
            .HasOne(l => l.MapCategory)
            .WithMany(c => c.Locations)
            .HasForeignKey(l => l.MapCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TownLocation>()
            .HasIndex(l => new { l.TownId, l.FeatureKind, l.FeatureIndex })
            .IsUnique();

        // Optional link to a building map; clearing the building just unlinks the place.
        modelBuilder.Entity<TownLocation>()
            .HasOne(l => l.LinkedBuilding)
            .WithMany()
            .HasForeignKey(l => l.LinkedBuildingId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<MapCategoryName>()
            .Property(n => n.Name)
            .HasMaxLength(120)
            .IsRequired();

        modelBuilder.Entity<MapCategoryName>()
            .HasOne(n => n.MapCategory)
            .WithMany(c => c.Names)
            .HasForeignKey(n => n.MapCategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Village>()
            .Property(v => v.Name)
            .HasMaxLength(120)
            .IsRequired();

        modelBuilder.Entity<VillageCategory>()
            .Property(c => c.Name)
            .HasMaxLength(60)
            .IsRequired();

        modelBuilder.Entity<VillageCategory>()
            .Property(c => c.ColorHex)
            .HasMaxLength(9)
            .IsRequired();

        modelBuilder.Entity<VillageLocation>()
            .Property(l => l.FeatureKind)
            .HasMaxLength(20)
            .IsRequired();

        modelBuilder.Entity<VillageLocation>()
            .HasOne(l => l.Village)
            .WithMany(v => v.Locations)
            .HasForeignKey(l => l.VillageId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict so a category that is in use on any map cannot be deleted silently.
        modelBuilder.Entity<VillageLocation>()
            .HasOne(l => l.VillageCategory)
            .WithMany(c => c.Locations)
            .HasForeignKey(l => l.VillageCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<VillageLocation>()
            .HasIndex(l => new { l.VillageId, l.FeatureKind, l.FeatureIndex })
            .IsUnique();

        // Optional link to a building map; clearing the building just unlinks the place.
        modelBuilder.Entity<VillageLocation>()
            .HasOne(l => l.LinkedBuilding)
            .WithMany()
            .HasForeignKey(l => l.LinkedBuildingId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<VillageCategoryName>()
            .Property(n => n.Name)
            .HasMaxLength(120)
            .IsRequired();

        modelBuilder.Entity<VillageCategoryName>()
            .HasOne(n => n.VillageCategory)
            .WithMany(c => c.Names)
            .HasForeignKey(n => n.VillageCategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DungeonMap>()
            .Property(d => d.Name)
            .HasMaxLength(120)
            .IsRequired();

        modelBuilder.Entity<DungeonCategory>()
            .Property(c => c.Name)
            .HasMaxLength(60)
            .IsRequired();

        modelBuilder.Entity<DungeonCategory>()
            .Property(c => c.ColorHex)
            .HasMaxLength(9)
            .IsRequired();

        modelBuilder.Entity<DungeonLocation>()
            .HasOne(l => l.DungeonMap)
            .WithMany(d => d.Locations)
            .HasForeignKey(l => l.DungeonMapId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict so a category that is in use on any map cannot be deleted silently.
        modelBuilder.Entity<DungeonLocation>()
            .HasOne(l => l.DungeonCategory)
            .WithMany(c => c.Locations)
            .HasForeignKey(l => l.DungeonCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DungeonLocation>()
            .HasIndex(l => new { l.DungeonMapId, l.CellX, l.CellY })
            .IsUnique();

        modelBuilder.Entity<DungeonCategoryName>()
            .Property(n => n.Name)
            .HasMaxLength(120)
            .IsRequired();

        modelBuilder.Entity<DungeonCategoryName>()
            .HasOne(n => n.DungeonCategory)
            .WithMany(c => c.Names)
            .HasForeignKey(n => n.DungeonCategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DungeonReveal>()
            .HasOne(rv => rv.DungeonMap)
            .WithMany()
            .HasForeignKey(rv => rv.DungeonMapId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DungeonReveal>()
            .HasIndex(rv => new { rv.DungeonMapId, rv.RectIndex })
            .IsUnique();

        modelBuilder.Entity<DungeonNote>()
            .HasOne(nt => nt.DungeonMap)
            .WithMany()
            .HasForeignKey(nt => nt.DungeonMapId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CaveMap>()
            .Property(c => c.Name)
            .HasMaxLength(120)
            .IsRequired();

        modelBuilder.Entity<CaveCategory>()
            .Property(c => c.Name)
            .HasMaxLength(60)
            .IsRequired();

        modelBuilder.Entity<CaveCategory>()
            .Property(c => c.ColorHex)
            .HasMaxLength(9)
            .IsRequired();

        modelBuilder.Entity<CaveLocation>()
            .HasOne(l => l.CaveMap)
            .WithMany(c => c.Locations)
            .HasForeignKey(l => l.CaveMapId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict so a category in use on any map cannot be deleted silently.
        modelBuilder.Entity<CaveLocation>()
            .HasOne(l => l.CaveCategory)
            .WithMany(c => c.Locations)
            .HasForeignKey(l => l.CaveCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CaveLocation>()
            .HasIndex(l => new { l.CaveMapId, l.CellX, l.CellY })
            .IsUnique();

        modelBuilder.Entity<CaveCategoryName>()
            .Property(n => n.Name)
            .HasMaxLength(120)
            .IsRequired();

        modelBuilder.Entity<CaveCategoryName>()
            .HasOne(n => n.CaveCategory)
            .WithMany(c => c.Names)
            .HasForeignKey(n => n.CaveCategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CaveReveal>()
            .HasOne(rv => rv.CaveMap)
            .WithMany()
            .HasForeignKey(rv => rv.CaveMapId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CaveReveal>()
            .HasIndex(rv => new { rv.CaveMapId, rv.CellX, rv.CellY })
            .IsUnique();

        modelBuilder.Entity<CaveNote>()
            .HasOne(nt => nt.CaveMap)
            .WithMany()
            .HasForeignKey(nt => nt.CaveMapId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BuildingMap>()
            .Property(b => b.Name)
            .HasMaxLength(120)
            .IsRequired();

        modelBuilder.Entity<BuildingCategory>()
            .Property(c => c.Name)
            .HasMaxLength(60)
            .IsRequired();

        modelBuilder.Entity<BuildingCategory>()
            .Property(c => c.ColorHex)
            .HasMaxLength(9)
            .IsRequired();

        modelBuilder.Entity<BuildingLocation>()
            .HasOne(l => l.BuildingMap)
            .WithMany(b => b.Locations)
            .HasForeignKey(l => l.BuildingMapId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict so a category in use on any map cannot be deleted silently.
        modelBuilder.Entity<BuildingLocation>()
            .HasOne(l => l.BuildingCategory)
            .WithMany(c => c.Locations)
            .HasForeignKey(l => l.BuildingCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<BuildingLocation>()
            .HasIndex(l => new { l.BuildingMapId, l.FloorIndex, l.CellX, l.CellY })
            .IsUnique();

        modelBuilder.Entity<BuildingCategoryName>()
            .Property(n => n.Name)
            .HasMaxLength(120)
            .IsRequired();

        modelBuilder.Entity<BuildingCategoryName>()
            .HasOne(n => n.BuildingCategory)
            .WithMany(c => c.Names)
            .HasForeignKey(n => n.BuildingCategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BuildingReveal>()
            .HasOne(rv => rv.BuildingMap)
            .WithMany()
            .HasForeignKey(rv => rv.BuildingMapId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BuildingReveal>()
            .HasIndex(rv => new { rv.BuildingMapId, rv.FloorIndex, rv.CellX, rv.CellY })
            .IsUnique();

        modelBuilder.Entity<BuildingNote>()
            .HasOne(nt => nt.BuildingMap)
            .WithMany()
            .HasForeignKey(nt => nt.BuildingMapId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<WorldMap>()
            .Property(w => w.Name)
            .HasMaxLength(120)
            .IsRequired();

        modelBuilder.Entity<WorldCategory>()
            .Property(c => c.Name)
            .HasMaxLength(60)
            .IsRequired();

        modelBuilder.Entity<WorldCategory>()
            .Property(c => c.ColorHex)
            .HasMaxLength(9)
            .IsRequired();

        modelBuilder.Entity<WorldLocation>()
            .HasOne(l => l.WorldMap)
            .WithMany(w => w.Locations)
            .HasForeignKey(l => l.WorldMapId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<WorldLocation>()
            .HasOne(l => l.WorldCategory)
            .WithMany(c => c.Locations)
            .HasForeignKey(l => l.WorldCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<WorldLocation>()
            .HasIndex(l => new { l.WorldMapId, l.HexQ, l.HexR })
            .IsUnique();

        // Optional link to a town map; clearing the town just unlinks the POI.
        modelBuilder.Entity<WorldLocation>()
            .HasOne(l => l.LinkedTown)
            .WithMany()
            .HasForeignKey(l => l.LinkedTownId)
            .OnDelete(DeleteBehavior.SetNull);

        // Optional link to a village map; clearing the village just unlinks the POI.
        modelBuilder.Entity<WorldLocation>()
            .HasOne(l => l.LinkedVillage)
            .WithMany()
            .HasForeignKey(l => l.LinkedVillageId)
            .OnDelete(DeleteBehavior.SetNull);

        // Optional link to a dungeon map; clearing the dungeon just unlinks the POI.
        modelBuilder.Entity<WorldLocation>()
            .HasOne(l => l.LinkedDungeon)
            .WithMany()
            .HasForeignKey(l => l.LinkedDungeonId)
            .OnDelete(DeleteBehavior.SetNull);

        // Optional link to a cave map; clearing the cave just unlinks the POI.
        modelBuilder.Entity<WorldLocation>()
            .HasOne(l => l.LinkedCave)
            .WithMany()
            .HasForeignKey(l => l.LinkedCaveId)
            .OnDelete(DeleteBehavior.SetNull);

        // Optional link to a building map; clearing the building just unlinks the POI.
        modelBuilder.Entity<WorldLocation>()
            .HasOne(l => l.LinkedBuilding)
            .WithMany()
            .HasForeignKey(l => l.LinkedBuildingId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<WorldReveal>()
            .HasOne(rv => rv.WorldMap)
            .WithMany()
            .HasForeignKey(rv => rv.WorldMapId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<WorldReveal>()
            .HasIndex(rv => new { rv.WorldMapId, rv.HexQ, rv.HexR })
            .IsUnique();

        modelBuilder.Entity<UsefulLink>()
            .Property(l => l.Title)
            .HasMaxLength(160)
            .IsRequired();

        modelBuilder.Entity<UsefulLink>()
            .Property(l => l.Url)
            .HasMaxLength(2000)
            .IsRequired();

        modelBuilder.Entity<ChatMessage>()
            .Property(m => m.Text)
            .HasMaxLength(4000)
            .IsRequired();

        modelBuilder.Entity<ChatMessage>()
            .Property(m => m.SenderName)
            .HasMaxLength(256);

        // Covers both Party reads (RecipientId null) and the per-recipient DM scans.
        modelBuilder.Entity<ChatMessage>()
            .HasIndex(m => new { m.RecipientId, m.SentAt });

        modelBuilder.Entity<ChatRead>()
            .Property(r => r.ConversationKey)
            .HasMaxLength(100)
            .IsRequired();

        modelBuilder.Entity<ChatRead>()
            .HasIndex(r => new { r.UserId, r.ConversationKey })
            .IsUnique();
    }

    // Writes audit log entries for tracked Character changes.
    // Call this before SaveChangesAsync, passing the current user's ID.
    public void WriteAuditLogs(string? changedByUserId)
    {
        foreach (var entry in ChangeTracker.Entries<Character>()
            .Where(e => e.State == EntityState.Modified))
        {
            foreach (var prop in entry.Properties.Where(p => p.IsModified))
            {
                CharacterAuditLogs.Add(new CharacterAuditLog
                {
                    CharacterId = entry.Entity.Id,
                    ChangedByUserId = changedByUserId,
                    FieldName = prop.Metadata.Name,
                    OldValue = prop.OriginalValue?.ToString(),
                    NewValue = prop.CurrentValue?.ToString(),
                    ChangedAt = DateTime.UtcNow
                });
            }
        }
    }
}
