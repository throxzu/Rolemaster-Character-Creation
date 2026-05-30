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
    public DbSet<CharacterAuditLog> CharacterAuditLogs => Set<CharacterAuditLog>();

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
