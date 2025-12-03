using StratSphere.Shared.Enums;

namespace StratSphere.Core.Entities;

/// <summary>
/// Links a player to a team within a specific league.
/// This is the tenant-scoped ownership record.
/// </summary>
public class RosterEntry : TenantEntity
{

    public DateTime AcquiredDate { get; set; }
    public TransactionType AcquiredVia { get; set; }
    public decimal? ContractSalary { get; set; }
    public int? ContractYearRemaining { get; set; }

    // Contract info (if league uses contracts)
    public int? ContractYears { get; set; }

    public bool IsActive { get; set; } = true; // On active roster vs minors/reserve
    public Player Player { get; set; } = null!;

    public Guid PlayerId { get; set; }
    public Position? RosterPosition { get; set; } // Position on fantasy roster
    public Team Team { get; set; } = null!;
    public Guid TeamId { get; set; }
}
