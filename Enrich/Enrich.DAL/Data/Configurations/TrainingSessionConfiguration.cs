using Enrich.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Enrich.DAL.Data.Configurations;

public class TrainingSessionConfiguration : IEntityTypeConfiguration<TrainingSession>
{
    public void Configure(EntityTypeBuilder<TrainingSession> builder)
    {
        builder.ToTable("TrainingSession");

        builder.Property(ts => ts.StartedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnType("timestamp without time zone");
        builder.Property(ts => ts.FinishedAt).HasColumnType("timestamp without time zone");
        builder.Property(ts => ts.TotalCards).HasDefaultValue(0);
        builder.Property(ts => ts.KnownCount).HasDefaultValue(0);
        builder.Property(ts => ts.UnknownCount).HasDefaultValue(0);

        builder.HasOne(ts => ts.User)
            .WithMany(u => u.TrainingSessions)
            .HasForeignKey(ts => ts.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ts => ts.Bundle)
            .WithMany(b => b.TrainingSessions)
            .HasForeignKey(ts => ts.BundleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
