using Enrich.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Enrich.DAL.Data.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            _ = builder.ToTable("Category");

            _ = builder.HasIndex(c => c.Name).IsUnique();

            _ = builder.Property(c => c.Name).IsRequired().HasMaxLength(50);
            _ = builder.Property(c => c.Description).HasMaxLength(200);
        }
    }
}
