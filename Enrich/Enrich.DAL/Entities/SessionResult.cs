
namespace Enrich.DAL.Entities;

public class SessionResult
{
    public int Id { get; set; }

    public int SessionId { get; set; }

    public int WordId { get; set; }

    public bool IsKnown { get; set; }

    public int PointsAwarded { get; set; }

    public virtual TrainingSession Session { get; set; } = null!;

    public virtual Word Word { get; set; } = null!;
}
