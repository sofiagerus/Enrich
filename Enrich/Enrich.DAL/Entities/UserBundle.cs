namespace Enrich.DAL.Entities
{
    public class UserBundle
    {
        public int Id { get; set; }

        public required string UserId { get; set; }

        public int BundleId { get; set; }

        public DateTime SavedAt { get; set; }

        public virtual User User { get; set; } = null!;

        public virtual Bundle Bundle { get; set; } = null!;
    }
}
