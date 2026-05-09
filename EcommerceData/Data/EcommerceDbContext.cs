using EcommerceData.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EcommerceData.Data;

public class EcommerceDbContext : DbContext
{
    public EcommerceDbContext(DbContextOptions<EcommerceDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserExternalLogin> UserExternalLogins => Set<UserExternalLogin>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(b =>
        {
            b.ToTable("Products");
            b.HasKey(p => p.Id);
            b.Property(p => p.Name).IsRequired().HasMaxLength(200);
            b.Property(p => p.Description).IsRequired().HasMaxLength(2000);
            b.Property(p => p.Price).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<User>(b =>
        {
            b.ToTable("Users");
            b.HasKey(u => u.Id);
            b.Property(u => u.Email).IsRequired().HasMaxLength(256);
            b.Property(u => u.Name).IsRequired().HasMaxLength(200);
            b.Property(u => u.PasswordHash).HasMaxLength(500);
            b.HasIndex(u => u.Email).IsUnique();
            b.HasMany(u => u.ExternalLogins).WithOne(l => l.User!).HasForeignKey(l => l.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserExternalLogin>(b =>
        {
            b.ToTable("UserExternalLogins");
            b.HasKey(l => l.Id);
            b.Property(l => l.Provider).IsRequired().HasMaxLength(50);
            b.Property(l => l.ProviderUserId).IsRequired().HasMaxLength(200);
            b.HasIndex(l => new { l.Provider, l.ProviderUserId }).IsUnique();
        });

        modelBuilder.Entity<Session>(b =>
        {
            b.ToTable("Sessions");
            b.HasKey(s => s.Token);
            b.Property(s => s.Token).HasMaxLength(64);
            b.HasOne(s => s.User).WithMany().HasForeignKey(s => s.UserId).OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(s => s.UserId);
            b.HasIndex(s => s.ExpiresAt);
        });

        modelBuilder.Entity<PasswordResetToken>(b =>
        {
            b.ToTable("PasswordResetTokens");
            b.HasKey(t => t.Token);
            b.Property(t => t.Token).HasMaxLength(64);
            b.HasOne(t => t.User).WithMany().HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(t => t.UserId);
            b.HasIndex(t => t.ExpiresAt);
        });

        // SQLite (used in tests) cannot translate DateTimeOffset comparisons natively.
        // Apply the binary converter for that provider only; SQL Server handles DateTimeOffset directly.
        if (Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
        {
            var converter = new DateTimeOffsetToBinaryConverter();
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var prop in entity.GetProperties())
                {
                    if (prop.ClrType == typeof(DateTimeOffset) || prop.ClrType == typeof(DateTimeOffset?))
                    {
                        prop.SetValueConverter(converter);
                    }
                }
            }
        }
    }
}
