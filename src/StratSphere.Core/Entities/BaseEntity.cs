namespace StratSphere.Core.Entities;

public abstract class BaseEntity
{
    public DateTime CreatedAt { get; set; }
    public Guid Id { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public abstract class TenantEntity : BaseEntity
{
    public League League { get; set; } = null!;
    public Guid LeagueId { get; set; }
}
