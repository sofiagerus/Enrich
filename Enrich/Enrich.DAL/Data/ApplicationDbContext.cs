using System.Reflection;
using Enrich.DAL.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Enrich.DAL.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : IdentityDbContext<User>(options)
    {
        public DbSet<Word> Words { get; set; }

        public DbSet<Bundle> Bundles { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Tag> Tags { get; set; }

        public DbSet<UserWord> UserWords { get; set; }

        public DbSet<UserBundle> UserBundles { get; set; }

        public DbSet<WordProgress> WordProgresses { get; set; }

        public DbSet<TrainingSession> TrainingSessions { get; set; }

        public DbSet<SessionResult> SessionResults { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.State is EntityState.Added or EntityState.Modified);

            foreach (var entityEntry in entries)
            {
                if (entityEntry.Entity is Word word)
                {
                    word.UpdatedAt = DateTime.UtcNow;

                    if (entityEntry.State == EntityState.Added)
                    {
                        word.CreatedAt = DateTime.UtcNow;
                    }
                }

                // Тут можна додати логіку для інших сутностей, якщо у них з'являться дати
            }

            return base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
                v => v.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v, DateTimeKind.Utc) : v.ToUniversalTime(),
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            var nullableDateTimeConverter = new ValueConverter<DateTime?, DateTime?>(
                v => !v.HasValue ? v : (v.Value.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v.Value.ToUniversalTime()),
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime))
                    {
                        property.SetValueConverter(dateTimeConverter);
                    }
                    else if (property.ClrType == typeof(DateTime?))
                    {
                        property.SetValueConverter(nullableDateTimeConverter);
                    }
                }
            }
        }
    }
}