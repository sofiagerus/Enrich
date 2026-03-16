using Enrich.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Enrich.DAL.Data.Configurations;

public class SessionResultConfiguration : IEntityTypeConfiguration<SessionResult>
{
    public void Configure(EntityTypeBuilder<SessionResult> builder)
    {
        builder.ToTable("SessionResult");

        builder.HasOne(sr => sr.Session)
            .WithMany(ts => ts.SessionResults)
            .HasForeignKey(sr => sr.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sr => sr.Word)
            .WithMany(w => w.SessionResults)
            .HasForeignKey(sr => sr.WordId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
