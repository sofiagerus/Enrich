using Enrich.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Enrich.DAL.Data.Configurations;

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable("Tag");

        builder.HasIndex(t => t.Name).IsUnique();

        builder.Property(t => t.Name).IsRequired().HasMaxLength(50);
    }
}
