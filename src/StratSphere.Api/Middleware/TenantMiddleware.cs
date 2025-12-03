using StratSphere.Infrastructure.Data;

namespace StratSphere.Api.Middleware;

/// <summary>
/// Middleware to resolve the current tenant (league) from the request.
/// Looks for league ID in route, header, or query string.
/// </summary>
public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantProvider tenantProvider)
    {
        // Try to get league ID from various sources
        Guid? leagueId = null;

        // 1. Route value (e.g., /api/leagues/{leagueId}/teams)
        if (context.Request.RouteValues.TryGetValue("leagueId", out var routeLeagueId)
            && Guid.TryParse(routeLeagueId?.ToString(), out var parsedRouteId))
        {
            leagueId = parsedRouteId;
        }
        // 2. Header (X-League-Id)
        else if (context.Request.Headers.TryGetValue("X-League-Id", out var headerLeagueId)
                 && Guid.TryParse(headerLeagueId.FirstOrDefault(), out var parsedHeaderId))
        {
            leagueId = parsedHeaderId;
        }
        // 3. Query string (?leagueId=)
        else if (context.Request.Query.TryGetValue("leagueId", out var queryLeagueId)
                 && Guid.TryParse(queryLeagueId.FirstOrDefault(), out var parsedQueryId))
        {
            leagueId = parsedQueryId;
        }

        if (leagueId.HasValue)
        {
            tenantProvider.SetCurrentLeagueId(leagueId.Value);
        }

        await _next(context);
    }
}
