using StratSphere.Shared.Enums;

namespace StratSphere.Core.Entities;

/// <summary>
/// Real-world baseball statistics for a player in a given season.
/// Used for scouting and player evaluation.
/// </summary>
public class PlayerStats : BaseEntity
{

    // Batting stats
    public int? AtBats { get; set; }
    public decimal? BB9 { get; set; }
    public decimal? BattingAverage { get; set; }
    public int? Doubles { get; set; }
    public decimal? ERA { get; set; }
    public int? EarnedRuns { get; set; }

    // Common stats
    public int GamesPlayed { get; set; }
    public int? Hits { get; set; }
    public int? HitsAllowed { get; set; }
    public int? HomeRuns { get; set; }

    // Pitching stats
    public decimal? InningsPitched { get; set; }
    public decimal? K9 { get; set; }
    public PlayerLevel Level { get; set; }
    public int? Losses { get; set; }
    public decimal? OPS { get; set; }
    public decimal? OnBasePercentage { get; set; }
    public int? PitchingStrikeouts { get; set; }
    public int? PitchingWalks { get; set; }
    public Player Player { get; set; } = null!;
    public Guid PlayerId { get; set; }
    public int? RBI { get; set; }
    public int? Runs { get; set; }
    public int? Saves { get; set; }

    public int Season { get; set; }
    public decimal? SluggingPercentage { get; set; }
    public int? StolenBases { get; set; }
    public int? Strikeouts { get; set; }
    public string? TeamName { get; set; } // Real-world team
    public int? Triples { get; set; }
    public decimal? WAR { get; set; }
    public decimal? WHIP { get; set; }
    public int? Walks { get; set; }
    public int? Wins { get; set; }
}
