using Enrich.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Enrich.DAL.Data.Configurations
{
    public class UserWordConfiguration : IEntityTypeConfiguration<UserWord>
    {
        public void Configure(EntityTypeBuilder<UserWord> builder)
        {
            _ = builder.ToTable("UserWord");

            _ = builder.HasIndex(uw => new { uw.UserId, uw.WordId }).IsUnique();

            _ = builder.Property(uw => uw.SavedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone");

            _ = builder.HasOne(uw => uw.User)
                .WithMany(u => u.UserWords)
                .HasForeignKey(uw => uw.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            _ = builder.HasOne(uw => uw.Word)
                .WithMany(w => w.UserWords)
                .HasForeignKey(uw => uw.WordId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
