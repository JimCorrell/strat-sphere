using StratSphere.Shared.Enums;

namespace StratSphere.Shared.DTOs;

// Draft DTOs
public record CreateDraftRequest(
    string Name,
    DraftMode Mode,
    int TotalRounds,
    DateTime? ScheduledStartTime,
    int PickTimeLimitSeconds = 120,
    bool SnakeDraft = true,
    bool AllowTrading = true
);

public record UpdateDraftRequest(
    string? Name,
    DateTime? ScheduledStartTime,
    int? PickTimeLimitSeconds,
    bool? AllowTrading
);

public record DraftResponse(
    Guid Id,
    string Name,
    DraftMode Mode,
    DraftStatus Status,
    DateTime? ScheduledStartTime,
    DateTime? ActualStartTime,
    DateTime? CompletedTime,
    int TotalRounds,
    int CurrentRound,
    int CurrentPick,
    int PickTimeLimitSeconds,
    DateTime? CurrentPickDeadline,
    Guid? CurrentTeamOnClock,
    string? CurrentTeamName,
    bool SnakeDraft,
    bool AllowTrading,
    int TotalPicks,
    int PicksMade
);

public record DraftSummaryResponse(
    Guid Id,
    string Name,
    DraftMode Mode,
    DraftStatus Status,
    DateTime? ScheduledStartTime,
    int TotalRounds
);

// Draft Pick DTOs
public record MakePickRequest(
    Guid PlayerId
);

public record DraftPickResponse(
    Guid Id,
    int Round,
    int OverallPickNumber,
    Guid TeamId,
    string TeamName,
    Guid? PlayerId,
    string? PlayerName,
    string? PlayerPosition,
    DateTime? PickMadeAt,
    bool IsAutoPick,
    Guid? OriginalTeamId,
    string? OriginalTeamName
);

// Draft Order DTOs
public record SetDraftOrderRequest(
    List<DraftOrderEntry> Order
);

public record DraftOrderEntry(
    Guid TeamId,
    int Position
);

public record DraftOrderResponse(
    Guid TeamId,
    string TeamName,
    int Round,
    int PickNumber,
    int PositionInRound,
    Guid? OriginalTeamId,
    string? OriginalTeamName
);

// SignalR Draft Events
public record DraftStartedEvent(
    Guid DraftId,
    Guid FirstTeamId,
    DateTime PickDeadline
);

public record PickMadeEvent(
    Guid DraftId,
    DraftPickResponse Pick,
    Guid? NextTeamId,
    DateTime? NextPickDeadline,
    int CurrentRound,
    int CurrentPick
);

public record DraftPausedEvent(
    Guid DraftId,
    string Reason
);

public record DraftResumedEvent(
    Guid DraftId,
    Guid CurrentTeamId,
    DateTime PickDeadline
);

public record DraftCompletedEvent(
    Guid DraftId,
    DateTime CompletedTime
);

public record TimerUpdateEvent(
    Guid DraftId,
    int SecondsRemaining
);
