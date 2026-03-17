using Enrich.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Enrich.DAL.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            _ = builder.ToTable("User");

            _ = builder.HasIndex(u => u.Email).IsUnique();
            _ = builder.HasIndex(u => u.Username).IsUnique();

            _ = builder.Property(u => u.Username).IsRequired().HasMaxLength(50);
            _ = builder.Property(u => u.Email).IsRequired().HasMaxLength(255);
            _ = builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(255);
            _ = builder.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);
            _ = builder.Property(u => u.ThemePreference).HasMaxLength(10);
            _ = builder.Property(u => u.LocalePreference).HasMaxLength(5);
            _ = builder.Property(u => u.AvatarUrl).HasMaxLength(500);
            _ = builder.Property(u => u.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            _ = builder.Property(u => u.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
        }
    }
}
