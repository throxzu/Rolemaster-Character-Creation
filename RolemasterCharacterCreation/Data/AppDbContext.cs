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
