namespace Enrich.DAL.Entities
{
    public class UserWord
    {
        public int Id { get; set; }

        public required string UserId { get; set; }

        public int WordId { get; set; }

        public DateTime SavedAt { get; set; }

        public virtual User User { get; set; } = null!;

        public virtual Word Word { get; set; } = null!;
    }
}
