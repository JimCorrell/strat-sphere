using Microsoft.AspNetCore.SignalR;
using StratSphere.Shared.DTOs;

namespace StratSphere.Api.Hubs;

/// <summary>
/// SignalR hub for real-time draft functionality.
/// Handles live draft events, pick timers, and client synchronization.
/// </summary>
public class DraftHub : Hub
{
    private readonly ILogger<DraftHub> _logger;

    public DraftHub(ILogger<DraftHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Join a draft room to receive real-time updates.
    /// </summary>
    public async Task JoinDraft(Guid draftId)
    {
        var groupName = GetDraftGroupName(draftId);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("Client {ConnectionId} joined draft {DraftId}", Context.ConnectionId, draftId);
        
        // Optionally send current draft state to the joining client
        // await Clients.Caller.SendAsync("DraftState", currentState);
    }

    /// <summary>
    /// Leave a draft room.
    /// </summary>
    public async Task LeaveDraft(Guid draftId)
    {
        var groupName = GetDraftGroupName(draftId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("Client {ConnectionId} left draft {DraftId}", Context.ConnectionId, draftId);
    }

    /// <summary>
    /// Broadcast that a draft has started.
    /// </summary>
    public async Task NotifyDraftStarted(Guid draftId, DraftStartedEvent evt)
    {
        await Clients.Group(GetDraftGroupName(draftId)).SendAsync("DraftStarted", evt);
    }

    /// <summary>
    /// Broadcast that a pick was made.
    /// </summary>
    public async Task NotifyPickMade(Guid draftId, PickMadeEvent evt)
    {
        await Clients.Group(GetDraftGroupName(draftId)).SendAsync("PickMade", evt);
    }

    /// <summary>
    /// Broadcast timer updates (called periodically during a pick).
    /// </summary>
    public async Task NotifyTimerUpdate(Guid draftId, TimerUpdateEvent evt)
    {
        await Clients.Group(GetDraftGroupName(draftId)).SendAsync("TimerUpdate", evt);
    }

    /// <summary>
    /// Broadcast that the draft was paused.
    /// </summary>
    public async Task NotifyDraftPaused(Guid draftId, DraftPausedEvent evt)
    {
        await Clients.Group(GetDraftGroupName(draftId)).SendAsync("DraftPaused", evt);
    }

    /// <summary>
    /// Broadcast that the draft was resumed.
    /// </summary>
    public async Task NotifyDraftResumed(Guid draftId, DraftResumedEvent evt)
    {
        await Clients.Group(GetDraftGroupName(draftId)).SendAsync("DraftResumed", evt);
    }

    /// <summary>
    /// Broadcast that the draft has completed.
    /// </summary>
    public async Task NotifyDraftCompleted(Guid draftId, DraftCompletedEvent evt)
    {
        await Clients.Group(GetDraftGroupName(draftId)).SendAsync("DraftCompleted", evt);
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    private static string GetDraftGroupName(Guid draftId) => $"draft-{draftId}";
}

/// <summary>
/// Service interface for sending draft events from outside the hub.
/// </summary>
public interface IDraftNotificationService
{
    Task NotifyDraftStarted(Guid draftId, DraftStartedEvent evt);
    Task NotifyPickMade(Guid draftId, PickMadeEvent evt);
    Task NotifyTimerUpdate(Guid draftId, TimerUpdateEvent evt);
    Task NotifyDraftPaused(Guid draftId, DraftPausedEvent evt);
    Task NotifyDraftResumed(Guid draftId, DraftResumedEvent evt);
    Task NotifyDraftCompleted(Guid draftId, DraftCompletedEvent evt);
}

/// <summary>
/// Implementation that uses IHubContext to send messages from services.
/// </summary>
public class DraftNotificationService : IDraftNotificationService
{
    private readonly IHubContext<DraftHub> _hubContext;

    public DraftNotificationService(IHubContext<DraftHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyDraftStarted(Guid draftId, DraftStartedEvent evt)
    {
        await _hubContext.Clients.Group($"draft-{draftId}").SendAsync("DraftStarted", evt);
    }

    public async Task NotifyPickMade(Guid draftId, PickMadeEvent evt)
    {
        await _hubContext.Clients.Group($"draft-{draftId}").SendAsync("PickMade", evt);
    }

    public async Task NotifyTimerUpdate(Guid draftId, TimerUpdateEvent evt)
    {
        await _hubContext.Clients.Group($"draft-{draftId}").SendAsync("TimerUpdate", evt);
    }

    public async Task NotifyDraftPaused(Guid draftId, DraftPausedEvent evt)
    {
        await _hubContext.Clients.Group($"draft-{draftId}").SendAsync("DraftPaused", evt);
    }

    public async Task NotifyDraftResumed(Guid draftId, DraftResumedEvent evt)
    {
        await _hubContext.Clients.Group($"draft-{draftId}").SendAsync("DraftResumed", evt);
    }

    public async Task NotifyDraftCompleted(Guid draftId, DraftCompletedEvent evt)
    {
        await _hubContext.Clients.Group($"draft-{draftId}").SendAsync("DraftCompleted", evt);
    }
}
