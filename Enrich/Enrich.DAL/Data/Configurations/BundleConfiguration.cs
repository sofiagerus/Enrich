using Enrich.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Enrich.DAL.Data.Configurations;

public class BundleConfiguration : IEntityTypeConfiguration<Bundle>
{
    public void Configure(EntityTypeBuilder<Bundle> builder)
    {
        builder.ToTable("Bundle");

        builder.HasIndex(b => b.ShareCode).IsUnique();

        builder.Property(b => b.Title).IsRequired().HasMaxLength(150);
        builder.Property(b => b.Description).HasMaxLength(1000);
        builder.Property(b => b.DifficultyLevels).HasColumnType("text[]");
        builder.Property(b => b.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(b => b.IsSystem).HasDefaultValue(false);
        builder.Property(b => b.IsPublic).HasDefaultValue(false);
        builder.Property(b => b.ShareCode).HasMaxLength(10);
        builder.Property(b => b.ImageUrl).HasMaxLength(500);
        builder.Property(b => b.ReviewedAt).HasColumnType("timestamp without time zone");
        builder.Property(b => b.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnType("timestamp without time zone");
        builder.Property(b => b.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnType("timestamp without time zone");

        builder.HasOne(b => b.Owner)
            .WithMany(u => u.BundleOwners)
            .HasForeignKey(b => b.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.ReviewedByAdmin)
            .WithMany(u => u.BundleReviews)
            .HasForeignKey(b => b.ReviewedByAdminId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(b => b.Words)
            .WithMany(w => w.Bundles)
            .UsingEntity<Dictionary<string, object>>(
                "BundleWord",
                r => r.HasOne<Word>().WithMany().HasForeignKey("WordId").OnDelete(DeleteBehavior.Cascade),
                l => l.HasOne<Bundle>().WithMany().HasForeignKey("BundleId").OnDelete(DeleteBehavior.Cascade),
                j => { j.HasKey("BundleId", "WordId"); j.ToTable("BundleWord"); });

        builder.HasMany(b => b.Categories)
            .WithMany(c => c.Bundles)
            .UsingEntity<Dictionary<string, object>>(
                "BundleCategory",
                r => r.HasOne<Category>().WithMany().HasForeignKey("CategoryId").OnDelete(DeleteBehavior.Cascade),
                l => l.HasOne<Bundle>().WithMany().HasForeignKey("BundleId").OnDelete(DeleteBehavior.Cascade),
                j => { j.HasKey("BundleId", "CategoryId"); j.ToTable("BundleCategory"); });

        builder.HasMany(b => b.Tags)
            .WithMany(t => t.Bundles)
            .UsingEntity<Dictionary<string, object>>(
                "BundleTag",
                r => r.HasOne<Tag>().WithMany().HasForeignKey("TagId").OnDelete(DeleteBehavior.Cascade),
                l => l.HasOne<Bundle>().WithMany().HasForeignKey("BundleId").OnDelete(DeleteBehavior.Cascade),
                j => { j.HasKey("BundleId", "TagId"); j.ToTable("BundleTag"); });
    }
}
