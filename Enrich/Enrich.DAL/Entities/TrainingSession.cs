namespace Enrich.DAL.Entities
{
    public class TrainingSession
    {
        public int Id { get; set; }

        public required string UserId { get; set; }

        public int? BundleId { get; set; }

        public DateTime StartedAt { get; set; }

        public DateTime? FinishedAt { get; set; }

        public int TotalCards { get; set; }

        public int KnownCount { get; set; }

        public int UnknownCount { get; set; }

        public virtual User User { get; set; } = null!;

        public virtual Bundle? Bundle { get; set; }

        public virtual ICollection<SessionResult> SessionResults { get; set; } = new List<SessionResult>();
    }
}
