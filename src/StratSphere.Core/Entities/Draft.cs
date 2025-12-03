using StratSphere.Shared.Enums;

namespace StratSphere.Core.Entities;

/// <summary>
/// Represents a draft event within a league.
/// </summary>
public class Draft : TenantEntity
{
    public DateTime? ActualStartTime { get; set; }
    public bool AllowTrading { get; set; } = true;
    public DateTime? CompletedTime { get; set; }
    public int CurrentPick { get; set; }
    public DateTime? CurrentPickDeadline { get; set; }
    public int CurrentRound { get; set; }
    public Guid? CurrentTeamOnClock { get; set; }
    public ICollection<DraftOrder> DraftOrder { get; set; } = new List<DraftOrder>();
    public DraftMode Mode { get; set; } = DraftMode.Synchronous;
    public string Name { get; set; } = string.Empty;

    // For sync drafts
    public int PickTimeLimitSeconds { get; set; } = 120;

    // Navigation properties
    public ICollection<DraftPick> Picks { get; set; } = new List<DraftPick>();

    public DateTime? ScheduledStartTime { get; set; }

    // Settings
    public bool SnakeDraft { get; set; } = true;
    public DraftStatus Status { get; set; } = DraftStatus.Scheduled;

    public int TotalRounds { get; set; }
}

/// <summary>
/// Represents the draft order for each team in each round.
/// </summary>
public class DraftOrder : TenantEntity
{
    public Draft Draft { get; set; } = null!;
    public Guid DraftId { get; set; }
    public Team? OriginalTeam { get; set; }

    // For traded picks
    public Guid? OriginalTeamId { get; set; }
    public int PickNumber { get; set; } // Overall pick number
    public int PositionInRound { get; set; }

    public int Round { get; set; }
    public Team Team { get; set; } = null!;

    public Guid TeamId { get; set; }
}

/// <summary>
/// Represents an individual draft selection.
/// </summary>
public class DraftPick : TenantEntity
{
    public Draft Draft { get; set; } = null!;
    public Guid DraftId { get; set; }
    public bool IsAutoPick { get; set; } // True if picked by system due to timeout

    // For traded picks
    public Guid? OriginalTeamId { get; set; }
    public int OverallPickNumber { get; set; }

    public DateTime? PickMadeAt { get; set; }
    public Player? Player { get; set; }

    public Guid? PlayerId { get; set; } // Null until pick is made

    public int Round { get; set; }
    public Team Team { get; set; } = null!;

    public Guid TeamId { get; set; }
}
