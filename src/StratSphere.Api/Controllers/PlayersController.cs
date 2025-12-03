using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StratSphere.Core.Entities;
using StratSphere.Infrastructure.Data;
using StratSphere.Shared.DTOs;

namespace StratSphere.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayersController : ControllerBase
{
    private readonly StratSphereDbContext _context;

    public PlayersController(StratSphereDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get a specific player.
    /// </summary>
    [HttpGet("{playerId:guid}")]
    public async Task<ActionResult<PlayerResponse>> GetPlayer(Guid playerId)
    {
        var player = await _context.Players
            .Where(p => p.Id == playerId)
            .Select(p => new PlayerResponse(
                p.Id,
                p.FirstName,
                p.LastName,
                p.FullName,
                p.MlbId,
                p.BirthDate,
                p.BirthCountry,
                p.BatsHand,
                p.ThrowsHand,
                p.Level,
                p.CurrentMlbTeam,
                p.CurrentMlbOrg,
                p.PrimaryPosition,
                p.SecondaryPositions,
                p.StratYear,
                p.StratCardNumber
            ))
            .FirstOrDefaultAsync();

        if (player == null)
        {
            return NotFound();
        }

        return Ok(player);
    }

    /// <summary>
    /// Get a player's career stats.
    /// </summary>
    [HttpGet("{playerId:guid}/stats")]
    public async Task<ActionResult<List<PlayerStatsResponse>>> GetPlayerStats(Guid playerId)
    {
        var stats = await _context.PlayerStats
            .Where(s => s.PlayerId == playerId)
            .OrderByDescending(s => s.Season)
            .Select(s => new PlayerStatsResponse(
                s.Id,
                s.PlayerId,
                s.Season,
                s.Level,
                s.TeamName,
                s.GamesPlayed,
                s.AtBats,
                s.Hits,
                s.Doubles,
                s.Triples,
                s.HomeRuns,
                s.RBI,
                s.Runs,
                s.Walks,
                s.Strikeouts,
                s.StolenBases,
                s.BattingAverage,
                s.OnBasePercentage,
                s.SluggingPercentage,
                s.OPS,
                s.WAR,
                s.InningsPitched,
                s.Wins,
                s.Losses,
                s.Saves,
                s.PitchingStrikeouts,
                s.PitchingWalks,
                s.HitsAllowed,
                s.EarnedRuns,
                s.ERA,
                s.WHIP,
                s.K9,
                s.BB9
            ))
            .ToListAsync();

        return Ok(stats);
    }

    /// <summary>
    /// Search for players (global, not league-specific).
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<PlayerSearchResponse>> SearchPlayers(
        [FromQuery] PlayerSearchRequest request,
        [FromQuery] Guid? leagueId)
    {
        var query = _context.Players.AsQueryable();

        // Text search
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(p =>
                p.FirstName.ToLower().Contains(term) ||
                p.LastName.ToLower().Contains(term) ||
                (p.FirstName + " " + p.LastName).ToLower().Contains(term));
        }

        // Filters
        if (request.Level.HasValue)
        {
            query = query.Where(p => p.Level == request.Level.Value);
        }

        if (request.Position.HasValue)
        {
            query = query.Where(p => p.PrimaryPosition == request.Position.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Team))
        {
            query = query.Where(p => p.CurrentMlbTeam == request.Team);
        }

        if (!string.IsNullOrWhiteSpace(request.Organization))
        {
            query = query.Where(p => p.CurrentMlbOrg == request.Organization);
        }

        // Filter to only available players in a league
        if (request.AvailableOnly == true && leagueId.HasValue)
        {
            var rosteredPlayerIds = await _context.RosterEntries
                .Where(r => r.LeagueId == leagueId.Value)
                .Select(r => r.PlayerId)
                .ToListAsync();

            query = query.Where(p => !rosteredPlayerIds.Contains(p.Id));
        }

        var totalCount = await query.CountAsync();

        var players = await query
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new PlayerResponse(
                p.Id,
                p.FirstName,
                p.LastName,
                p.FullName,
                p.MlbId,
                p.BirthDate,
                p.BirthCountry,
                p.BatsHand,
                p.ThrowsHand,
                p.Level,
                p.CurrentMlbTeam,
                p.CurrentMlbOrg,
                p.PrimaryPosition,
                p.SecondaryPositions,
                p.StratYear,
                p.StratCardNumber
            ))
            .ToListAsync();

        var response = new PlayerSearchResponse(
            players,
            totalCount,
            request.Page,
            request.PageSize,
            (int)Math.Ceiling(totalCount / (double)request.PageSize)
        );

        return Ok(response);
    }
}
