namespace StratSphere.Core.Entities;

public enum LeagueRole
{
    Commissioner,
    CoCommissioner,
    Member
}

public class LeagueMember : TenantEntity
{
    public bool IsActive { get; set; } = true;
    public DateTime JoinedAt { get; set; }

    public LeagueRole Role { get; set; } = LeagueRole.Member;
    public User User { get; set; } = null!;
    public Guid UserId { get; set; }
}
