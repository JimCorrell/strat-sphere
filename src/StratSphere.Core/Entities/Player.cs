using StratSphere.Shared.Enums;

namespace StratSphere.Core.Entities;

/// <summary>
/// Represents a real baseball player from MLB/MiLB/Amateur ranks.
/// Players are shared across all leagues - only roster assignments are league-specific.
/// </summary>
public class Player : BaseEntity
{
    public string? BatsHand { get; set; } // L, R, S
    public string? BirthCountry { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? CurrentMlbOrg { get; set; } // Organization (for minors)
    public string? CurrentMlbTeam { get; set; } // Real-life team
    public ICollection<DraftPick> DraftSelections { get; set; } = new List<DraftPick>();

    // Bio
    public string FirstName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string LastName { get; set; } = string.Empty;

    // Current status
    public PlayerLevel Level { get; set; } = PlayerLevel.Major;
    // External IDs for data integration
    public string? MlbId { get; set; }
    public string? MlbamId { get; set; }
    public Position PrimaryPosition { get; set; }

    // Navigation properties
    public ICollection<RosterEntry> RosterEntries { get; set; } = new List<RosterEntry>();
    public ICollection<ScoutingReport> ScoutingReports { get; set; } = new List<ScoutingReport>();
    public string? SecondaryPositions { get; set; } // Comma-separated
    public ICollection<PlayerStats> Stats { get; set; } = new List<PlayerStats>();
    public string? StratCardNumber { get; set; }

    // Strat-o-matic card reference (for future integration)
    public int? StratYear { get; set; }
    public string? ThrowsHand { get; set; } // L, R
}
