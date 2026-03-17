using Enrich.DAL.Entities.Enums;

namespace Enrich.DAL.Entities
{
    public class Bundle
    {
        public int Id { get; set; }

        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        public string[] DifficultyLevels { get; set; } = Array.Empty<string>();

        public string? ImageUrl { get; set; }

        public BundleStatus Status { get; set; } = BundleStatus.Draft;

        public bool IsSystem { get; set; }

        public bool IsPublic { get; set; }

        public string? ShareCode { get; set; }

        public int OwnerId { get; set; }

        public int? ReviewedByAdminId { get; set; }

        public DateTime? ReviewedAt { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public virtual User Owner { get; set; } = null!;

        public virtual User? ReviewedByAdmin { get; set; }

        public virtual ICollection<Word> Words { get; set; } = new List<Word>();

        public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

        public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();

        public virtual ICollection<UserBundle> UserBundles { get; set; } = new List<UserBundle>();

        public virtual ICollection<TrainingSession> TrainingSessions { get; set; } = new List<TrainingSession>();
    }
}
