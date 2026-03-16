namespace Enrich.DAL.Entities;

public class Tag
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Word> Words { get; set; } = new List<Word>();

    public virtual ICollection<Bundle> Bundles { get; set; } = new List<Bundle>();
}
