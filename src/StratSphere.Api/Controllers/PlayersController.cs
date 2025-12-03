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
    private readonly StratLeagueDbContext _context;
    private readonly ILogger<PlayersController> _logger;

    public PlayersController(StratLeagueDbContext context, ILogger<PlayersController> logger)
    {
        _context = context;
        _logger = logger;
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
}

/// <summary>
/// League-scoped scouting reports controller.
/// </summary>
[ApiController]
[Route("api/leagues/{leagueId:guid}/scouting")]
public class ScoutingController : ControllerBase
{
    private readonly StratLeagueDbContext _context;
    private readonly ILogger<ScoutingController> _logger;

    public ScoutingController(StratLeagueDbContext context, ILogger<ScoutingController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get scouting reports for a player in a league.
    /// </summary>
    [HttpGet("player/{playerId:guid}")]
    public async Task<ActionResult<List<ScoutingReportResponse>>> GetPlayerReports(
        Guid leagueId,
        Guid playerId)
    {
        var reports = await _context.ScoutingReports
            .Include(r => r.Player)
            .Include(r => r.ScoutedBy)
            .Where(r => r.LeagueId == leagueId && r.PlayerId == playerId)
            .OrderByDescending(r => r.ScoutedAt)
            .Select(r => new ScoutingReportResponse(
                r.Id,
                r.PlayerId,
                r.Player.FullName,
                r.ScoutedByUserId,
                r.ScoutedBy.DisplayName,
                r.ScoutedAt,
                r.HitTool,
                r.PowerTool,
                r.SpeedTool,
                r.FieldingTool,
                r.ArmTool,
                r.FastballTool,
                r.CurveballTool,
                r.SliderTool,
                r.ChangeupTool,
                r.ControlTool,
                r.OverallGrade,
                r.PotentialGrade,
                r.RiskLevel,
                r.ETA,
                r.Notes,
                r.Strengths,
                r.Weaknesses,
                r.Comparable
            ))
            .ToListAsync();

        return Ok(reports);
    }

    /// <summary>
    /// Create a scouting report.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ScoutingReportResponse>> CreateReport(
        Guid leagueId,
        [FromBody] CreateScoutingReportRequest request,
        [FromQuery] Guid scoutUserId) // TODO: Get from auth
    {
        var player = await _context.Players.FindAsync(request.PlayerId);
        if (player == null)
        {
            return NotFound("Player not found");
        }

        var scout = await _context.Users.FindAsync(scoutUserId);
        if (scout == null)
        {
            return NotFound("Scout user not found");
        }

        var report = new ScoutingReport
        {
            LeagueId = leagueId,
            PlayerId = request.PlayerId,
            ScoutedByUserId = scoutUserId,
            ScoutedAt = DateTime.UtcNow,
            HitTool = request.HitTool,
            PowerTool = request.PowerTool,
            SpeedTool = request.SpeedTool,
            FieldingTool = request.FieldingTool,
            ArmTool = request.ArmTool,
            FastballTool = request.FastballTool,
            CurveballTool = request.CurveballTool,
            SliderTool = request.SliderTool,
            ChangeupTool = request.ChangeupTool,
            ControlTool = request.ControlTool,
            OverallGrade = request.OverallGrade,
            PotentialGrade = request.PotentialGrade,
            RiskLevel = request.RiskLevel,
            ETA = request.ETA,
            Notes = request.Notes,
            Strengths = request.Strengths,
            Weaknesses = request.Weaknesses,
            Comparable = request.Comparable
        };

        _context.ScoutingReports.Add(report);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created scouting report for player {PlayerId} in league {LeagueId}",
            request.PlayerId, leagueId);

        var response = new ScoutingReportResponse(
            report.Id,
            report.PlayerId,
            player.FullName,
            report.ScoutedByUserId,
            scout.DisplayName,
            report.ScoutedAt,
            report.HitTool,
            report.PowerTool,
            report.SpeedTool,
            report.FieldingTool,
            report.ArmTool,
            report.FastballTool,
            report.CurveballTool,
            report.SliderTool,
            report.ChangeupTool,
            report.ControlTool,
            report.OverallGrade,
            report.PotentialGrade,
            report.RiskLevel,
            report.ETA,
            report.Notes,
            report.Strengths,
            report.Weaknesses,
            report.Comparable
        );

        return Created($"/api/leagues/{leagueId}/scouting/{report.Id}", response);
    }
}
