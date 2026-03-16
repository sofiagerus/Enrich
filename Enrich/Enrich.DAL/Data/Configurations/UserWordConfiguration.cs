using Enrich.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Enrich.DAL.Data.Configurations;

public class UserWordConfiguration : IEntityTypeConfiguration<UserWord>
{
    public void Configure(EntityTypeBuilder<UserWord> builder)
    {
        builder.ToTable("UserWord");

        builder.HasIndex(uw => new { uw.UserId, uw.WordId }).IsUnique();

        builder.Property(uw => uw.SavedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnType("timestamp without time zone");

        builder.HasOne(uw => uw.User)
            .WithMany(u => u.UserWords)
            .HasForeignKey(uw => uw.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(uw => uw.Word)
            .WithMany(w => w.UserWords)
            .HasForeignKey(uw => uw.WordId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
