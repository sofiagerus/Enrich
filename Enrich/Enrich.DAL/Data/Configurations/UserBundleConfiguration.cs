using Enrich.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Enrich.DAL.Data.Configurations
{
    public class UserBundleConfiguration : IEntityTypeConfiguration<UserBundle>
    {
        public void Configure(EntityTypeBuilder<UserBundle> builder)
        {
            _ = builder.ToTable("UserBundle");

            _ = builder.HasIndex(ub => new { ub.UserId, ub.BundleId }).IsUnique();

            _ = builder.Property(ub => ub.SavedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");

            _ = builder.HasOne(ub => ub.User)
                .WithMany(u => u.UserBundles)
                .HasForeignKey(ub => ub.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            _ = builder.HasOne(ub => ub.Bundle)
                .WithMany(b => b.UserBundles)
                .HasForeignKey(ub => ub.BundleId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
