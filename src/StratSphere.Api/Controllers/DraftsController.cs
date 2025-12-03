using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StratSphere.Api.Hubs;
using StratSphere.Core.Entities;
using StratSphere.Infrastructure.Data;
using StratSphere.Shared.DTOs;
using StratSphere.Shared.Enums;

namespace StratSphere.Api.Controllers;

[ApiController]
[Route("api/leagues/{leagueId:guid}/[controller]")]
public class DraftsController : ControllerBase
{
    private readonly StratLeagueDbContext _context;
    private readonly IDraftNotificationService _draftNotifications;
    private readonly ILogger<DraftsController> _logger;

    public DraftsController(
        StratLeagueDbContext context,
        IDraftNotificationService draftNotifications,
        ILogger<DraftsController> logger)
    {
        _context = context;
        _draftNotifications = draftNotifications;
        _logger = logger;
    }

    /// <summary>
    /// Create a new draft.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<DraftResponse>> CreateDraft(
        Guid leagueId,
        [FromBody] CreateDraftRequest request)
    {
        var league = await _context.Leagues.FindAsync(leagueId);
        if (league == null)
        {
            return NotFound("League not found");
        }

        var draft = new Draft
        {
            LeagueId = leagueId,
            Name = request.Name,
            Mode = request.Mode,
            TotalRounds = request.TotalRounds,
            ScheduledStartTime = request.ScheduledStartTime,
            PickTimeLimitSeconds = request.PickTimeLimitSeconds,
            SnakeDraft = request.SnakeDraft,
            AllowTrading = request.AllowTrading,
            Status = DraftStatus.Scheduled,
            CurrentRound = 1,
            CurrentPick = 1
        };

        _context.Drafts.Add(draft);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created draft {DraftId} '{DraftName}' in league {LeagueId}",
            draft.Id, draft.Name, leagueId);

        return await GetDraft(leagueId, draft.Id);
    }

    /// <summary>
    /// Get a specific draft with full details.
    /// </summary>
    [HttpGet("{draftId:guid}")]
    public async Task<ActionResult<DraftResponse>> GetDraft(Guid leagueId, Guid draftId)
    {
        var draft = await _context.Drafts
            .Include(d => d.Picks)
            .Where(d => d.LeagueId == leagueId && d.Id == draftId)
            .FirstOrDefaultAsync();

        if (draft == null)
        {
            return NotFound();
        }

        string? currentTeamName = null;
        if (draft.CurrentTeamOnClock.HasValue)
        {
            currentTeamName = await _context.Teams
                .Where(t => t.Id == draft.CurrentTeamOnClock.Value)
                .Select(t => t.Name)
                .FirstOrDefaultAsync();
        }

        var response = new DraftResponse(
            draft.Id,
            draft.Name,
            draft.Mode,
            draft.Status,
            draft.ScheduledStartTime,
            draft.ActualStartTime,
            draft.CompletedTime,
            draft.TotalRounds,
            draft.CurrentRound,
            draft.CurrentPick,
            draft.PickTimeLimitSeconds,
            draft.CurrentPickDeadline,
            draft.CurrentTeamOnClock,
            currentTeamName,
            draft.SnakeDraft,
            draft.AllowTrading,
            draft.Picks.Count,
            draft.Picks.Count(p => p.PlayerId.HasValue)
        );

        return Ok(response);
    }

    /// <summary>
    /// Get all drafts in a league.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<DraftSummaryResponse>>> GetDrafts(Guid leagueId)
    {
        var drafts = await _context.Drafts
            .Where(d => d.LeagueId == leagueId)
            .OrderByDescending(d => d.ScheduledStartTime)
            .Select(d => new DraftSummaryResponse(
                d.Id,
                d.Name,
                d.Mode,
                d.Status,
                d.ScheduledStartTime,
                d.TotalRounds
            ))
            .ToListAsync();

        return Ok(drafts);
    }

    /// <summary>
    /// Get all picks for a draft.
    /// </summary>
    [HttpGet("{draftId:guid}/picks")]
    public async Task<ActionResult<List<DraftPickResponse>>> GetPicks(Guid leagueId, Guid draftId)
    {
        var picks = await _context.DraftPicks
            .Include(p => p.Team)
            .Include(p => p.Player)
            .Where(p => p.LeagueId == leagueId && p.DraftId == draftId)
            .OrderBy(p => p.OverallPickNumber)
            .Select(p => new DraftPickResponse(
                p.Id,
                p.Round,
                p.OverallPickNumber,
                p.TeamId,
                p.Team.Name,
                p.PlayerId,
                p.Player != null ? p.Player.FullName : null,
                p.Player != null ? p.Player.PrimaryPosition.ToString() : null,
                p.PickMadeAt,
                p.IsAutoPick,
                p.OriginalTeamId,
                null
            ))
            .ToListAsync();

        return Ok(picks);
    }

    /// <summary>
    /// Make a draft pick.
    /// </summary>
    [HttpPost("{draftId:guid}/pick")]
    public async Task<ActionResult<DraftPickResponse>> MakePick(
        Guid leagueId,
        Guid draftId,
        [FromBody] MakePickRequest request,
        [FromQuery] Guid teamId) // TODO: Get from auth
    {
        var draft = await _context.Drafts
            .FirstOrDefaultAsync(d => d.LeagueId == leagueId && d.Id == draftId);

        if (draft == null)
        {
            return NotFound();
        }

        if (draft.Status != DraftStatus.InProgress)
        {
            return BadRequest("Draft is not in progress");
        }

        if (draft.CurrentTeamOnClock != teamId)
        {
            return BadRequest("It is not your turn to pick");
        }

        // Check if player is already drafted
        var alreadyDrafted = await _context.DraftPicks
            .AnyAsync(p => p.DraftId == draftId && p.PlayerId == request.PlayerId);
        if (alreadyDrafted)
        {
            return BadRequest("Player has already been drafted");
        }

        // Get the current pick
        var currentPick = await _context.DraftPicks
            .FirstOrDefaultAsync(p => p.DraftId == draftId && p.OverallPickNumber == draft.CurrentPick);

        if (currentPick == null)
        {
            return BadRequest("Could not find current pick");
        }

        // Make the pick
        currentPick.PlayerId = request.PlayerId;
        currentPick.PickMadeAt = DateTime.UtcNow;

        // Get player info for response
        var player = await _context.Players.FindAsync(request.PlayerId);
        var team = await _context.Teams.FindAsync(teamId);

        // Advance to next pick
        var nextPick = await _context.DraftPicks
            .Where(p => p.DraftId == draftId && p.OverallPickNumber == draft.CurrentPick + 1)
            .FirstOrDefaultAsync();

        Guid? nextTeamId = null;
        DateTime? nextDeadline = null;

        if (nextPick != null)
        {
            draft.CurrentPick++;
            draft.CurrentRound = nextPick.Round;
            draft.CurrentTeamOnClock = nextPick.TeamId;
            draft.CurrentPickDeadline = DateTime.UtcNow.AddSeconds(draft.PickTimeLimitSeconds);
            nextTeamId = nextPick.TeamId;
            nextDeadline = draft.CurrentPickDeadline;
        }
        else
        {
            // Draft complete
            draft.Status = DraftStatus.Completed;
            draft.CompletedTime = DateTime.UtcNow;
            draft.CurrentTeamOnClock = null;
            draft.CurrentPickDeadline = null;
        }

        await _context.SaveChangesAsync();

        var pickResponse = new DraftPickResponse(
            currentPick.Id,
            currentPick.Round,
            currentPick.OverallPickNumber,
            currentPick.TeamId,
            team?.Name ?? "",
            currentPick.PlayerId,
            player?.FullName,
            player?.PrimaryPosition.ToString(),
            currentPick.PickMadeAt,
            currentPick.IsAutoPick,
            currentPick.OriginalTeamId,
            null
        );

        // Notify clients
        await _draftNotifications.NotifyPickMade(draftId, new PickMadeEvent(
            draftId,
            pickResponse,
            nextTeamId,
            nextDeadline,
            draft.CurrentRound,
            draft.CurrentPick
        ));

        if (draft.Status == DraftStatus.Completed)
        {
            await _draftNotifications.NotifyDraftCompleted(draftId, new DraftCompletedEvent(
                draftId,
                draft.CompletedTime!.Value
            ));
        }

        _logger.LogInformation("Pick made in draft {DraftId}: {PlayerName} to {TeamName}",
            draftId, player?.FullName, team?.Name);

        return Ok(pickResponse);
    }

    /// <summary>
    /// Set the draft order for a draft.
    /// </summary>
    [HttpPost("{draftId:guid}/order")]
    public async Task<ActionResult<List<DraftOrderResponse>>> SetDraftOrder(
        Guid leagueId,
        Guid draftId,
        [FromBody] SetDraftOrderRequest request)
    {
        var draft = await _context.Drafts
            .FirstOrDefaultAsync(d => d.LeagueId == leagueId && d.Id == draftId);

        if (draft == null)
        {
            return NotFound();
        }

        if (draft.Status != DraftStatus.Scheduled)
        {
            return BadRequest("Cannot modify draft order after draft has started");
        }

        // Remove existing order
        var existingOrder = await _context.DraftOrders
            .Where(o => o.DraftId == draftId)
            .ToListAsync();
        _context.DraftOrders.RemoveRange(existingOrder);

        // Remove existing picks
        var existingPicks = await _context.DraftPicks
            .Where(p => p.DraftId == draftId)
            .ToListAsync();
        _context.DraftPicks.RemoveRange(existingPicks);

        // Create new order and picks for each round
        var teams = await _context.Teams
            .Where(t => t.LeagueId == leagueId)
            .ToDictionaryAsync(t => t.Id, t => t.Name);

        int overallPick = 1;
        for (int round = 1; round <= draft.TotalRounds; round++)
        {
            var roundOrder = request.Order.ToList();

            // Reverse order for even rounds if snake draft
            if (draft.SnakeDraft && round % 2 == 0)
            {
                roundOrder.Reverse();
            }

            int positionInRound = 1;
            foreach (var entry in roundOrder)
            {
                var draftOrder = new DraftOrder
                {
                    LeagueId = leagueId,
                    DraftId = draftId,
                    TeamId = entry.TeamId,
                    Round = round,
                    PickNumber = overallPick,
                    PositionInRound = positionInRound
                };
                _context.DraftOrders.Add(draftOrder);

                var draftPick = new DraftPick
                {
                    LeagueId = leagueId,
                    DraftId = draftId,
                    TeamId = entry.TeamId,
                    Round = round,
                    OverallPickNumber = overallPick
                };
                _context.DraftPicks.Add(draftPick);

                overallPick++;
                positionInRound++;
            }
        }

        await _context.SaveChangesAsync();

        var response = await _context.DraftOrders
            .Where(o => o.DraftId == draftId)
            .OrderBy(o => o.PickNumber)
            .Select(o => new DraftOrderResponse(
                o.TeamId,
                teams[o.TeamId],
                o.Round,
                o.PickNumber,
                o.PositionInRound,
                o.OriginalTeamId,
                o.OriginalTeamId.HasValue ? teams[o.OriginalTeamId.Value] : null
            ))
            .ToListAsync();

        return Ok(response);
    }

    /// <summary>
    /// Start a draft.
    /// </summary>
    [HttpPost("{draftId:guid}/start")]
    public async Task<ActionResult<DraftResponse>> StartDraft(Guid leagueId, Guid draftId)
    {
        var draft = await _context.Drafts
            .Include(d => d.DraftOrder)
            .FirstOrDefaultAsync(d => d.LeagueId == leagueId && d.Id == draftId);

        if (draft == null)
        {
            return NotFound();
        }

        if (draft.Status != DraftStatus.Scheduled)
        {
            return BadRequest("Draft is not in scheduled status");
        }

        if (!draft.DraftOrder.Any())
        {
            return BadRequest("Draft order must be set before starting");
        }

        var firstPick = draft.DraftOrder.OrderBy(o => o.PickNumber).First();

        draft.Status = DraftStatus.InProgress;
        draft.ActualStartTime = DateTime.UtcNow;
        draft.CurrentRound = 1;
        draft.CurrentPick = 1;
        draft.CurrentTeamOnClock = firstPick.TeamId;
        draft.CurrentPickDeadline = DateTime.UtcNow.AddSeconds(draft.PickTimeLimitSeconds);

        await _context.SaveChangesAsync();

        // Notify connected clients
        await _draftNotifications.NotifyDraftStarted(draftId, new DraftStartedEvent(
            draftId,
            firstPick.TeamId,
            draft.CurrentPickDeadline.Value
        ));

        _logger.LogInformation("Started draft {DraftId}", draftId);

        return await GetDraft(leagueId, draftId);
    }
}
