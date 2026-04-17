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
        }
    }
}
