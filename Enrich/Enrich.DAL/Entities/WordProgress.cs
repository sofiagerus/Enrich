namespace Enrich.DAL.Entities;

public class WordProgress
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int WordId { get; set; }

    public int Points { get; set; } // Accumulated score: +10 for "know", +5 for "don't know". Max 100.

    public bool IsLearned { get; set; } // Set to true when Points reach 100.

    public DateTime? LastReviewedAt { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual Word Word { get; set; } = null!;
}
