namespace Enrich.DAL.Entities;

public class Word
{
    public int Id { get; set; }

    public string Term { get; set; } = null!;

    public string? Translation { get; set; }

    public string? Transcription { get; set; }

    public string? Meaning { get; set; }

    public string? PartOfSpeech { get; set; }

    public string? Example { get; set; }

    public string? ImageUrl { get; set; }

    public string? DifficultyLevel { get; set; }

    public bool IsGlobal { get; set; }

    public int? CreatorId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User? Creator { get; set; }

    public virtual ICollection<Bundle> Bundles { get; set; } = new List<Bundle>();

    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();

    public virtual ICollection<UserWord> UserWords { get; set; } = new List<UserWord>();

    public virtual ICollection<WordProgress> WordProgresses { get; set; } = new List<WordProgress>();

    public virtual ICollection<SessionResult> SessionResults { get; set; } = new List<SessionResult>();
}
