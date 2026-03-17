using Enrich.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Enrich.DAL.Data.Configurations
{
    public class WordConfiguration : IEntityTypeConfiguration<Word>
    {
        public void Configure(EntityTypeBuilder<Word> builder)
        {
            _ = builder.ToTable("Word");

            _ = builder.Property(w => w.Term).IsRequired().HasMaxLength(100);
            _ = builder.Property(w => w.Translation).HasMaxLength(150);
            _ = builder.Property(w => w.Transcription).HasMaxLength(100);
            _ = builder.Property(w => w.Meaning).HasMaxLength(1000);
            _ = builder.Property(w => w.PartOfSpeech).HasMaxLength(30);
            _ = builder.Property(w => w.Example).HasMaxLength(500);
            _ = builder.Property(w => w.ImageUrl).HasMaxLength(500);
            _ = builder.Property(w => w.DifficultyLevel).HasMaxLength(5);
            _ = builder.Property(w => w.IsGlobal).HasDefaultValue(false);
            _ = builder.Property(w => w.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            _ = builder.Property(w => w.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");

            _ = builder.HasOne(w => w.Creator)
                .WithMany(u => u.Words)
                .HasForeignKey(w => w.CreatorId)
                .OnDelete(DeleteBehavior.SetNull);

            _ = builder.HasMany(w => w.Categories)
                .WithMany(c => c.Words)
                .UsingEntity<Dictionary<string, object>>(
                    "WordCategory",
                    r => r.HasOne<Category>().WithMany().HasForeignKey("CategoryId").OnDelete(DeleteBehavior.Cascade),
                    l => l.HasOne<Word>().WithMany().HasForeignKey("WordId").OnDelete(DeleteBehavior.Cascade),
                    j =>
                    {
                        _ = j.HasKey("WordId", "CategoryId");
                        _ = j.ToTable("WordCategory");
                    });

            _ = builder.HasMany(w => w.Tags)
                .WithMany(t => t.Words)
                .UsingEntity<Dictionary<string, object>>(
                    "WordTag",
                    r => r.HasOne<Tag>().WithMany().HasForeignKey("TagId").OnDelete(DeleteBehavior.Cascade),
                    l => l.HasOne<Word>().WithMany().HasForeignKey("WordId").OnDelete(DeleteBehavior.Cascade),
                    j =>
                    {
                        _ = j.HasKey("WordId", "TagId");
                        _ = j.ToTable("WordTag");
                    });
        }
    }
}
