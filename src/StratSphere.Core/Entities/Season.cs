using StratSphere.Shared.Enums;

namespace StratSphere.Core.Entities;

/// <summary>
/// Represents the result of a game between two teams.
/// </summary>
public class GameResult : TenantEntity
{
    public int? AwayScore { get; set; }
    public Team AwayTeam { get; set; } = null!;

    public Guid AwayTeamId { get; set; }

    public DateTime GameDate { get; set; }
    public int? GameNumber { get; set; } // For doubleheaders

    public int? HomeScore { get; set; }
    public Team HomeTeam { get; set; } = null!;

    public Guid HomeTeamId { get; set; }

    public bool IsComplete { get; set; }
    public bool IsPlayoff { get; set; }

    public string? Notes { get; set; }
    public string? PlayoffRound { get; set; }
    public Season Season { get; set; } = null!;
    public Guid SeasonId { get; set; }
}

/// <summary>
/// Represents a season within a league.
/// </summary>
public class Season : TenantEntity
{
    public DateTime? EndDate { get; set; }

    public ICollection<GameResult> Games { get; set; } = new List<GameResult>();

    public bool IsCurrentSeason { get; set; }
    public string Name { get; set; } = string.Empty;
    public SeasonPhase Phase { get; set; } = SeasonPhase.PreSeason;
    public ICollection<StandingsEntry> Standings { get; set; } = new List<StandingsEntry>();

    public DateTime? StartDate { get; set; }
    public int Year { get; set; }
}

/// <summary>
/// Standings entry for a team in a season.
/// </summary>
public class StandingsEntry : TenantEntity
{

    public string? Division { get; set; }
    public int? DivisionRank { get; set; }
    public decimal? GamesBack { get; set; }
    public int? Last10Losses { get; set; }
    public int? Last10Wins { get; set; }
    public int? LeagueRank { get; set; }
    public int Losses { get; set; }
    public int? RunDifferential => RunsScored - RunsAllowed;
    public int? RunsAllowed { get; set; }

    public int? RunsScored { get; set; }
    public Season Season { get; set; } = null!;
    public Guid SeasonId { get; set; }

    public string? Streak { get; set; } // e.g., "W5", "L2"
    public Team Team { get; set; } = null!;

    public Guid TeamId { get; set; }
    public int? Ties { get; set; }

    public decimal WinningPercentage { get; set; }

    public int Wins { get; set; }
}
