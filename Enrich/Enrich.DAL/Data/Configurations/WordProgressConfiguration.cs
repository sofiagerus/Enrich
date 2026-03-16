using Enrich.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Enrich.DAL.Data.Configurations;

public class WordProgressConfiguration : IEntityTypeConfiguration<WordProgress>
{
    public void Configure(EntityTypeBuilder<WordProgress> builder)
    {
        builder.ToTable("WordProgress");

        builder.HasIndex(wp => new { wp.UserId, wp.WordId }).IsUnique();

        builder.Property(wp => wp.Points).HasDefaultValue(0);
        builder.Property(wp => wp.IsLearned).HasDefaultValue(false);
        builder.Property(wp => wp.LastReviewedAt).HasColumnType("timestamp without time zone");

        builder.HasOne(wp => wp.User)
            .WithMany(u => u.WordProgresses)
            .HasForeignKey(wp => wp.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(wp => wp.Word)
            .WithMany(w => w.WordProgresses)
            .HasForeignKey(wp => wp.WordId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
