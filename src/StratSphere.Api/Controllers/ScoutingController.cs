using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StratSphere.Core.Entities;
using StratSphere.Infrastructure.Data;
using StratSphere.Shared.DTOs;

namespace StratSphere.Api.Controllers;

/// <summary>
/// League-scoped scouting reports controller.
/// </summary>
[ApiController]
[Route("api/leagues/{leagueId:guid}/scouting")]
public class ScoutingController : ControllerBase
{
    private readonly StratSphereDbContext _context;
    private readonly ILogger<ScoutingController> _logger;

    public ScoutingController(StratSphereDbContext context, ILogger<ScoutingController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Create a scouting report.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ScoutingReportResponse>> CreateReport(
        Guid leagueId,
        [FromBody] CreateScoutingReportRequest request)
    {
        var scoutUserId = GetUserId();

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

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User ID not found in token");
        }
        return userId;
    }
}
