using FeedbackMap.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FeedbackMap.Data;

/// <summary>Databasekonteksten (EF Core mot MariaDB) med Identity-tabeller og applikasjonens egne tabeller.</summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<PublicActorProfile> PublicActorProfiles => Set<PublicActorProfile>();
    public DbSet<ResourceProviderProfile> ResourceProviderProfiles => Set<ResourceProviderProfile>();
    public DbSet<Need> Needs => Set<Need>();
    public DbSet<Resource> Resources => Set<Resource>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<NeedStatusLog> NeedStatusLogs => Set<NeedStatusLog>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // Class table inheritance: profil-PK er også FK til ACTOR (AspNetUsers)
        b.Entity<PublicActorProfile>(e =>
        {
            e.ToTable("PublicActor");
            e.HasKey(p => p.UserId);
            e.Property(p => p.BaseLatitude).HasPrecision(9, 6);
            e.Property(p => p.BaseLongitude).HasPrecision(9, 6);
            e.HasOne(p => p.User).WithOne(u => u.PublicActorProfile)
                .HasForeignKey<PublicActorProfile>(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<ResourceProviderProfile>(e =>
        {
            e.ToTable("ResourceProvider");
            e.HasKey(p => p.UserId);
            e.Property(p => p.ProviderType).HasConversion<string>().HasMaxLength(40);
            e.HasOne(p => p.User).WithOne(u => u.ResourceProviderProfile)
                .HasForeignKey<ResourceProviderProfile>(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<Need>(e =>
        {
            e.Property(n => n.Latitude).HasPrecision(9, 6);
            e.Property(n => n.Longitude).HasPrecision(9, 6);
            e.Property(n => n.Priority).HasConversion<string>().HasMaxLength(20);
            e.HasOne(n => n.PublicActor).WithMany(p => p.Needs)
                .HasForeignKey(n => n.PublicActorId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(n => new { n.Latitude, n.Longitude });
        });

        b.Entity<Resource>(e =>
        {
            e.Property(r => r.Latitude).HasPrecision(9, 6);
            e.Property(r => r.Longitude).HasPrecision(9, 6);
            e.Property(r => r.Status).HasConversion<string>().HasMaxLength(20);
            e.HasOne(r => r.ResourceProvider).WithMany(p => p.Resources)
                .HasForeignKey(r => r.ResourceProviderId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(r => new { r.Latitude, r.Longitude });
        });

        // Restrict: historikk skal ikke forsvinne ved sletting
        b.Entity<Assignment>(e =>
        {
            e.Property(a => a.MatchStatus).HasConversion<string>().HasMaxLength(20);
            e.HasOne(a => a.Need).WithMany(n => n.Assignments)
                .HasForeignKey(a => a.NeedId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(a => a.Resource).WithMany(r => r.Assignments)
                .HasForeignKey(a => a.ResourceId).OnDelete(DeleteBehavior.Restrict);
            // En ressurs kan bare ha én aktiv tildeling (flere NULL er lov i unik indeks)
            e.HasIndex(a => a.ActiveResourceKey).IsUnique();
        });

        b.Entity<NeedStatusLog>(e =>
        {
            e.ToTable("NeedStatusLog");
            e.Property(s => s.Status).HasConversion<string>().HasMaxLength(20);
            e.HasOne(s => s.Need).WithMany(n => n.StatusLog)
                .HasForeignKey(s => s.NeedId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(s => s.ChangedByActor).WithMany()
                .HasForeignKey(s => s.ChangedByActorId).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<AuditLog>(e =>
        {
            e.HasIndex(a => a.Timestamp);
        });

        // 1 : 0..1 – unik AssignmentId følger av one-to-one
        b.Entity<Notification>(e =>
        {
            e.HasOne(n => n.Assignment).WithOne(a => a.Notification)
                .HasForeignKey<Notification>(n => n.AssignmentId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
