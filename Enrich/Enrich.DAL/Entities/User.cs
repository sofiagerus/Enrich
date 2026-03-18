using Enrich.DAL.Entities.Enums;
using Microsoft.AspNetCore.Identity;

namespace Enrich.DAL.Entities
{
    public class User : IdentityUser
    {
        public UserRole Role { get; set; } = UserRole.User;

        public string? ThemePreference { get; set; }

        public string? LocalePreference { get; set; }

        public string? AvatarUrl { get; set; }

        public string? Bio { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public virtual ICollection<Word> Words { get; set; } = new List<Word>();

        public virtual ICollection<Bundle> BundleOwners { get; set; } = new List<Bundle>();

        public virtual ICollection<Bundle> BundleReviews { get; set; } = new List<Bundle>();

        public virtual ICollection<UserWord> UserWords { get; set; } = new List<UserWord>();

        public virtual ICollection<UserBundle> UserBundles { get; set; } = new List<UserBundle>();

        public virtual ICollection<WordProgress> WordProgresses { get; set; } = new List<WordProgress>();

        public virtual ICollection<TrainingSession> TrainingSessions { get; set; } = new List<TrainingSession>();
    }
}
