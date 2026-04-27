using Enrich.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Enrich.DAL.Data.Configurations
{
    public class TrainingSessionConfiguration : IEntityTypeConfiguration<TrainingSession>
    {
        public void Configure(EntityTypeBuilder<TrainingSession> builder)
        {
            _ = builder.ToTable("TrainingSession");

            _ = builder.Property(ts => ts.StartedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone");
            _ = builder.Property(ts => ts.FinishedAt).HasColumnType("timestamp with time zone");
            _ = builder.Property(ts => ts.TotalCards).HasDefaultValue(0);
            _ = builder.Property(ts => ts.KnownCount).HasDefaultValue(0);
            _ = builder.Property(ts => ts.UnknownCount).HasDefaultValue(0);

            _ = builder.HasOne(ts => ts.User)
                .WithMany(u => u.TrainingSessions)
                .HasForeignKey(ts => ts.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            _ = builder.HasOne(ts => ts.Bundle)
                .WithMany(b => b.TrainingSessions)
                .HasForeignKey(ts => ts.BundleId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
