using Enrich.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Enrich.DAL.Data.Configurations;

public class WordConfiguration : IEntityTypeConfiguration<Word>
{
    public void Configure(EntityTypeBuilder<Word> builder)
    {
        builder.ToTable("Word");

        builder.Property(w => w.Term).IsRequired().HasMaxLength(100);
        builder.Property(w => w.Translation).HasMaxLength(150);
        builder.Property(w => w.Transcription).HasMaxLength(100);
        builder.Property(w => w.Meaning).HasMaxLength(1000);
        builder.Property(w => w.PartOfSpeech).HasMaxLength(30);
        builder.Property(w => w.Example).HasMaxLength(500);
        builder.Property(w => w.ImageUrl).HasMaxLength(500);
        builder.Property(w => w.DifficultyLevel).HasMaxLength(5);
        builder.Property(w => w.IsGlobal).HasDefaultValue(false);
        builder.Property(w => w.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnType("timestamp without time zone");
        builder.Property(w => w.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnType("timestamp without time zone");

        builder.HasOne(w => w.Creator)
            .WithMany(u => u.Words)
            .HasForeignKey(w => w.CreatorId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(w => w.Categories)
            .WithMany(c => c.Words)
            .UsingEntity<Dictionary<string, object>>(
                "WordCategory",
                r => r.HasOne<Category>().WithMany().HasForeignKey("CategoryId").OnDelete(DeleteBehavior.Cascade),
                l => l.HasOne<Word>().WithMany().HasForeignKey("WordId").OnDelete(DeleteBehavior.Cascade),
                j => { j.HasKey("WordId", "CategoryId"); j.ToTable("WordCategory"); });

        builder.HasMany(w => w.Tags)
            .WithMany(t => t.Words)
            .UsingEntity<Dictionary<string, object>>(
                "WordTag",
                r => r.HasOne<Tag>().WithMany().HasForeignKey("TagId").OnDelete(DeleteBehavior.Cascade),
                l => l.HasOne<Word>().WithMany().HasForeignKey("WordId").OnDelete(DeleteBehavior.Cascade),
                j => { j.HasKey("WordId", "TagId"); j.ToTable("WordTag"); });
    }
}
