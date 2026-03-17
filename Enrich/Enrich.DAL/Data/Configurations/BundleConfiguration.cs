using Enrich.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Enrich.DAL.Data.Configurations
{
    public class BundleConfiguration : IEntityTypeConfiguration<Bundle>
    {
        public void Configure(EntityTypeBuilder<Bundle> builder)
        {
            _ = builder.ToTable("Bundle");

            _ = builder.HasIndex(b => b.ShareCode).IsUnique();

            _ = builder.Property(b => b.Title).IsRequired().HasMaxLength(150);
            _ = builder.Property(b => b.Description).HasMaxLength(1000);
            _ = builder.Property(b => b.DifficultyLevels).HasColumnType("text[]");
            _ = builder.Property(b => b.Status).HasConversion<string>().HasMaxLength(20);
            _ = builder.Property(b => b.IsSystem).HasDefaultValue(false);
            _ = builder.Property(b => b.IsPublic).HasDefaultValue(false);
            _ = builder.Property(b => b.ShareCode).HasMaxLength(10);
            _ = builder.Property(b => b.ImageUrl).HasMaxLength(500);
            _ = builder.Property(b => b.ReviewedAt).HasColumnType("timestamp without time zone");
            _ = builder.Property(b => b.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            _ = builder.Property(b => b.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");

            _ = builder.HasOne(b => b.Owner)
                .WithMany(u => u.BundleOwners)
                .HasForeignKey(b => b.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            _ = builder.HasOne(b => b.ReviewedByAdmin)
                .WithMany(u => u.BundleReviews)
                .HasForeignKey(b => b.ReviewedByAdminId)
                .OnDelete(DeleteBehavior.SetNull);

            _ = builder.HasMany(b => b.Words)
                .WithMany(w => w.Bundles)
                .UsingEntity<Dictionary<string, object>>(
                    "BundleWord",
                    r => r.HasOne<Word>().WithMany().HasForeignKey("WordId").OnDelete(DeleteBehavior.Cascade),
                    l => l.HasOne<Bundle>().WithMany().HasForeignKey("BundleId").OnDelete(DeleteBehavior.Cascade),
                    j =>
                    {
                        _ = j.HasKey("BundleId", "WordId");
                        _ = j.ToTable("BundleWord");
                    });

            _ = builder.HasMany(b => b.Categories)
                .WithMany(c => c.Bundles)
                .UsingEntity<Dictionary<string, object>>(
                    "BundleCategory",
                    r => r.HasOne<Category>().WithMany().HasForeignKey("CategoryId").OnDelete(DeleteBehavior.Cascade),
                    l => l.HasOne<Bundle>().WithMany().HasForeignKey("BundleId").OnDelete(DeleteBehavior.Cascade),
                    j =>
                    {
                        _ = j.HasKey("BundleId", "CategoryId");
                        _ = j.ToTable("BundleCategory");
                    });

            _ = builder.HasMany(b => b.Tags)
                .WithMany(t => t.Bundles)
                .UsingEntity<Dictionary<string, object>>(
                    "BundleTag",
                    r => r.HasOne<Tag>().WithMany().HasForeignKey("TagId").OnDelete(DeleteBehavior.Cascade),
                    l => l.HasOne<Bundle>().WithMany().HasForeignKey("BundleId").OnDelete(DeleteBehavior.Cascade),
                    j =>
                    {
                        _ = j.HasKey("BundleId", "TagId");
                        _ = j.ToTable("BundleTag");
                    });
        }
    }
}
