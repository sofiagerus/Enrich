namespace Enrich.DAL.Entities
{
    public class Notification
    {
        public int Id { get; set; }

        public required string UserId { get; set; }

        public required string Type { get; set; }

        public required string Message { get; set; }

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; }

        public virtual User User { get; set; } = null!;
    }
}
