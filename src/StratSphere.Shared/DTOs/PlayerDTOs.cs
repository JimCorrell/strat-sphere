using StratSphere.Shared.Enums;

namespace StratSphere.Shared.DTOs;

// Player DTOs
public record PlayerResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string FullName,
    string? MlbId,
    DateTime? BirthDate,
    string? BirthCountry,
    string? BatsHand,
    string? ThrowsHand,
    PlayerLevel Level,
    string? CurrentMlbTeam,
    string? CurrentMlbOrg,
    Position PrimaryPosition,
    string? SecondaryPositions,
    int? StratYear,
    string? StratCardNumber
);

public record PlayerSearchRequest(
    string? SearchTerm,
    PlayerLevel? Level,
    Position? Position,
    string? Team,
    string? Organization,
    bool? AvailableOnly, // Only players not on a roster in current league
    int Page = 1,
    int PageSize = 50
);

public record PlayerSearchResponse(
    List<PlayerResponse> Players,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);

// Player Stats DTOs
public record PlayerStatsResponse(
    Guid Id,
    Guid PlayerId,
    int Season,
    PlayerLevel Level,
    string? TeamName,
    int GamesPlayed,
    // Batting
    int? AtBats,
    int? Hits,
    int? Doubles,
    int? Triples,
    int? HomeRuns,
    int? RBI,
    int? Runs,
    int? Walks,
    int? Strikeouts,
    int? StolenBases,
    decimal? BattingAverage,
    decimal? OnBasePercentage,
    decimal? SluggingPercentage,
    decimal? OPS,
    decimal? WAR,
    // Pitching
    decimal? InningsPitched,
    int? Wins,
    int? Losses,
    int? Saves,
    int? PitchingStrikeouts,
    int? PitchingWalks,
    int? HitsAllowed,
    int? EarnedRuns,
    decimal? ERA,
    decimal? WHIP,
    decimal? K9,
    decimal? BB9
);

// Roster DTOs
public record AddToRosterRequest(
    Guid PlayerId,
    TransactionType AcquiredVia,
    bool IsActive = true,
    Position? RosterPosition = null,
    int? ContractYears = null,
    decimal? ContractSalary = null
);

public record UpdateRosterEntryRequest(
    bool? IsActive,
    Position? RosterPosition,
    int? ContractYearsRemaining,
    decimal? ContractSalary
);

public record RosterEntryResponse(
    Guid Id,
    Guid TeamId,
    Guid PlayerId,
    string PlayerName,
    Position PlayerPosition,
    PlayerLevel PlayerLevel,
    DateTime AcquiredDate,
    TransactionType AcquiredVia,
    bool IsActive,
    Position? RosterPosition,
    int? ContractYears,
    decimal? ContractSalary,
    int? ContractYearRemaining
);

// Scouting Report DTOs
public record CreateScoutingReportRequest(
    Guid PlayerId,
    // Position player tools (20-80 scale)
    int? HitTool,
    int? PowerTool,
    int? SpeedTool,
    int? FieldingTool,
    int? ArmTool,
    // Pitcher tools
    int? FastballTool,
    int? CurveballTool,
    int? SliderTool,
    int? ChangeupTool,
    int? ControlTool,
    // Overall
    int? OverallGrade,
    int? PotentialGrade,
    string? RiskLevel,
    string? ETA,
    string? Notes,
    string? Strengths,
    string? Weaknesses,
    string? Comparable
);

public record ScoutingReportResponse(
    Guid Id,
    Guid PlayerId,
    string PlayerName,
    Guid ScoutedByUserId,
    string ScoutedByName,
    DateTime ScoutedAt,
    // Position player tools
    int? HitTool,
    int? PowerTool,
    int? SpeedTool,
    int? FieldingTool,
    int? ArmTool,
    // Pitcher tools
    int? FastballTool,
    int? CurveballTool,
    int? SliderTool,
    int? ChangeupTool,
    int? ControlTool,
    // Overall
    int? OverallGrade,
    int? PotentialGrade,
    string? RiskLevel,
    string? ETA,
    string? Notes,
    string? Strengths,
    string? Weaknesses,
    string? Comparable
);
