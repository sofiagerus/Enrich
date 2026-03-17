using System.Reflection;
using Enrich.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace Enrich.DAL.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }

        public DbSet<Word> Words { get; set; }

        public DbSet<Bundle> Bundles { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Tag> Tags { get; set; }

        public DbSet<UserWord> UserWords { get; set; }

        public DbSet<UserBundle> UserBundles { get; set; }

        public DbSet<WordProgress> WordProgresses { get; set; }

        public DbSet<TrainingSession> TrainingSessions { get; set; }

        public DbSet<SessionResult> SessionResults { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            _ = modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}