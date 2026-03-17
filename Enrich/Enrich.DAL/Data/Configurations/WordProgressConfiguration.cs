using Enrich.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Enrich.DAL.Data.Configurations
{
    public class WordProgressConfiguration : IEntityTypeConfiguration<WordProgress>
    {
        public void Configure(EntityTypeBuilder<WordProgress> builder)
        {
            _ = builder.ToTable("WordProgress");

            _ = builder.HasIndex(wp => new { wp.UserId, wp.WordId }).IsUnique();

            _ = builder.Property(wp => wp.Points).HasDefaultValue(0);
            _ = builder.Property(wp => wp.IsLearned).HasDefaultValue(false);
            _ = builder.Property(wp => wp.LastReviewedAt).HasColumnType("timestamp without time zone");

            _ = builder.HasOne(wp => wp.User)
                .WithMany(u => u.WordProgresses)
                .HasForeignKey(wp => wp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            _ = builder.HasOne(wp => wp.Word)
                .WithMany(w => w.WordProgresses)
                .HasForeignKey(wp => wp.WordId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
