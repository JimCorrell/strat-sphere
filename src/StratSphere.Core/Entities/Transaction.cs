using StratSphere.Shared.Enums;

namespace StratSphere.Core.Entities;

/// <summary>
/// Records all roster transactions within a league.
/// Provides an audit trail of player movement.
/// </summary>
public class Transaction : TenantEntity
{
    public Draft? Draft { get; set; }

    // For draft picks
    public Guid? DraftId { get; set; }
    public int? DraftPickNumber { get; set; }
    public int? DraftRound { get; set; }

    public string? Notes { get; set; }
    public Team? OtherTeam { get; set; }

    // For trades - the other team
    public Guid? OtherTeamId { get; set; }
    public Player Player { get; set; } = null!;

    public Guid PlayerId { get; set; }
    public Team Team { get; set; } = null!;

    // Team involved (or primary team for trades)
    public Guid TeamId { get; set; }
    public DateTime TransactionDate { get; set; }

    // For grouped transactions (e.g., multi-player trades)
    public Guid? TransactionGroupId { get; set; }
    public TransactionType Type { get; set; }
}
