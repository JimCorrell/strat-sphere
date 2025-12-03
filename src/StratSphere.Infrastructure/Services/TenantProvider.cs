using StratSphere.Infrastructure.Data;

namespace StratSphere.Infrastructure.Services;

/// <summary>
/// Provides the current tenant (league) context for the request.
/// This is typically set by middleware based on the route or auth claims.
/// </summary>
public class TenantProvider : ITenantProvider
{
    private Guid? _currentLeagueId;

    public Guid? GetCurrentLeagueId() => _currentLeagueId;

    public void SetCurrentLeagueId(Guid leagueId)
    {
        _currentLeagueId = leagueId;
    }
}
