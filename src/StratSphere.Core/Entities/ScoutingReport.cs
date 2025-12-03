using StratSphere.Shared.Enums;

namespace StratSphere.Core.Entities;

/// <summary>
/// League-specific scouting report for a player (typically prospects).
/// Each league can have its own scouting notes.
/// </summary>
public class ScoutingReport : TenantEntity
{
    public int? ArmTool { get; set; }
    public int? ChangeupTool { get; set; }
    public string? Comparable { get; set; } // Player comparison
    public int? ControlTool { get; set; }
    public int? CurveballTool { get; set; }
    public string? ETA { get; set; } // e.g., "2025", "Late 2024"

    // Pitching tools
    public int? FastballTool { get; set; }
    public int? FieldingTool { get; set; }

    // 20-80 scouting scale ratings (baseball standard)
    public int? HitTool { get; set; }

    public string? Notes { get; set; }

    // Overall assessment
    public int? OverallGrade { get; set; }
    public Player Player { get; set; } = null!;
    public Guid PlayerId { get; set; }
    public int? PotentialGrade { get; set; }
    public int? PowerTool { get; set; }
    public string? RiskLevel { get; set; } // Low, Medium, High

    public DateTime ScoutedAt { get; set; }
    public User ScoutedBy { get; set; } = null!;

    public Guid ScoutedByUserId { get; set; }
    public int? SliderTool { get; set; }
    public int? SpeedTool { get; set; }
    public string? Strengths { get; set; }
    public string? Weaknesses { get; set; }
}
