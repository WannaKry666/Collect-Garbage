using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using GarbageCollection.Common.Enums;
using GarbageCollection.Common.Models;
using System.Text.Json;

namespace GarbageCollection.DataAccess.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Citizen> Citizens { get; set; }
        public DbSet<WasteReport> WasteReports { get; set; }
        public DbSet<User> Users => Set<User>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<EmailOtp> EmailOtps => Set<EmailOtp>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<WasteReport>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.ImageUrls)
                      .IsRequired()
                      .HasColumnType("text")
                      .HasConversion(
                          v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                          v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
                      )
                      .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                          (a, b) => a != null && b != null && a.SequenceEqual(b),
                          v => v.Aggregate(0, (acc, s) => HashCode.Combine(acc, s.GetHashCode())),
                          v => v.ToList()
                      ));

                entity.Property(e => e.WasteTypes)
                      .IsRequired()
                      .HasColumnType("text")
                      .HasConversion(
                          v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                          v => JsonSerializer.Deserialize<List<WasteType>>(v, (JsonSerializerOptions?)null) ?? new List<WasteType>()
                      )
                      .Metadata.SetValueComparer(new ValueComparer<List<WasteType>>(
                          (a, b) => a != null && b != null && a.SequenceEqual(b),
                          v => v.Aggregate(0, (acc, w) => HashCode.Combine(acc, w.GetHashCode())),
                          v => v.ToList()
                      ));

                entity.Property(e => e.Description)
                      .IsRequired()
                      .HasMaxLength(500);

                entity.Property(e => e.Size)
                      .HasConversion<string>();

                entity.Property(e => e.Status)
                      .HasConversion<string>();

                entity.HasOne(e => e.Citizen)
                      .WithMany(c => c.WasteReports)
                      .HasForeignKey(e => e.CitizenId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<User>(e =>
            {
                e.ToTable("users");
                e.HasKey(u => u.Id);

                e.Property(u => u.Id).HasColumnName("id");
                e.Property(u => u.Email).HasColumnName("email").IsRequired().HasMaxLength(320);
                e.Property(u => u.EmailVerified).HasColumnName("email_verified");
                e.Property(u => u.GoogleId).HasColumnName("google_id").HasMaxLength(128);
                e.Property(u => u.Provider).HasColumnName("provider").IsRequired().HasMaxLength(64);
                e.Property(u => u.FullName).HasColumnName("full_name").IsRequired().HasMaxLength(256);
                e.Property(u => u.AvatarUrl).HasColumnName("avatar_url").HasMaxLength(1024);
                e.Property(u => u.PasswordHash).HasColumnName("password_hash").HasMaxLength(512);
                e.Property(u => u.IsBanned).HasColumnName("is_banned");
                e.Property(u => u.IsLogin).HasColumnName("is_login");
                e.Property(u => u.LoginTerm).HasColumnName("login_term");
                e.Property(u => u.Role).HasColumnName("role").IsRequired().HasMaxLength(64);
                e.Property(u => u.Address).HasColumnName("address").HasMaxLength(512);
                e.Property(u => u.CreatedAt).HasColumnName("created_at");
                e.Property(u => u.UpdatedAt).HasColumnName("updated_at");

                e.HasIndex(u => u.Email).IsUnique();
                e.HasIndex(u => u.GoogleId);
            });

            // ── RefreshToken ─────────────────────────────────────────────────
            modelBuilder.Entity<RefreshToken>(e =>
            {
                e.ToTable("refresh_tokens");
                e.HasKey(rt => rt.Id);

                e.Property(rt => rt.Id).HasColumnName("id");
                e.Property(rt => rt.UserId).HasColumnName("user_id");
                e.Property(rt => rt.TokenHash).HasColumnName("token_hash").IsRequired().HasMaxLength(128);
                e.Property(rt => rt.Email).HasColumnName("email").IsRequired().HasMaxLength(320);
                e.Property(rt => rt.ExpiresAt).HasColumnName("expires_at");
                e.Property(rt => rt.IsRevoked).HasColumnName("is_revoked");
                e.Property(rt => rt.CreatedAt).HasColumnName("created_at");

                e.HasOne(rt => rt.User)
                 .WithMany(u => u.RefreshTokens)
                 .HasForeignKey(rt => rt.UserId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasIndex(rt => rt.TokenHash).IsUnique();
            });
            // ── EmailOtp ──────────────────────────────────────────────────────
            modelBuilder.Entity<EmailOtp>(e =>
            {
                e.ToTable("email_otps");
                e.HasKey(o => o.Id);

                e.Property(o => o.Id).HasColumnName("id");
                e.Property(o => o.Email).HasColumnName("email").IsRequired().HasMaxLength(320);
                e.Property(o => o.OtpCode).HasColumnName("otp_code").IsRequired().HasMaxLength(6);
                e.Property(o => o.ExpiresAt).HasColumnName("expires_at");
                e.Property(o => o.IsUsed).HasColumnName("is_used");
                e.Property(o => o.CreatedAt).HasColumnName("created_at");

                e.HasIndex(o => o.Email);
            });
        }
    }
}
