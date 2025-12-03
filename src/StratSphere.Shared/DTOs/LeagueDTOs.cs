using StratSphere.Shared.Enums;

namespace StratSphere.Shared.DTOs;

public enum LeagueRole
{
    Commissioner,
    CoCommissioner,
    Member
}

// League DTOs
public record CreateLeagueRequest(
    string Name,
    string? Description,
    int MaxTeams = 30,
    int RosterSize = 40,
    int ActiveRosterSize = 25,
    bool UseDH = true
);

public record UpdateLeagueRequest(
    string? Name,
    string? Description,
    int? MaxTeams,
    int? RosterSize,
    int? ActiveRosterSize,
    bool? UseDH,
    LeagueStatus? Status
);

public record LeagueResponse(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    LeagueStatus Status,
    int CurrentSeason,
    SeasonPhase CurrentPhase,
    int MaxTeams,
    int RosterSize,
    int ActiveRosterSize,
    bool UseDH,
    int TeamCount,
    int MemberCount,
    DateTime CreatedAt
);

public record LeagueSummaryResponse(
    Guid Id,
    string Name,
    string Slug,
    LeagueStatus Status,
    int TeamCount
);

// Team DTOs
public record CreateTeamRequest(
    string Name,
    string Abbreviation,
    string? City,
    string? Division,
    string? Conference
);

public record UpdateTeamRequest(
    string? Name,
    string? Abbreviation,
    string? City,
    string? Division,
    string? Conference,
    string? LogoUrl
);

public record TeamResponse(
    Guid Id,
    string Name,
    string Abbreviation,
    string? City,
    string? LogoUrl,
    string? Division,
    string? Conference,
    Guid OwnerId,
    string OwnerName,
    int RosterCount
);

// User DTOs
public record RegisterUserRequest(
    string Email,
    string Username,
    string Password,
    string DisplayName
);

public record LoginRequest(
    string EmailOrUsername,
    string Password
);

public record AuthResponse(
    string Token,
    DateTime ExpiresAt,
    UserResponse User
);

public record UserResponse(
    Guid Id,
    string Email,
    string Username,
    string DisplayName,
    DateTime CreatedAt
);

// League Member DTOs
public record AddMemberRequest(
    Guid UserId,
    LeagueRole Role = LeagueRole.Member
);

public record LeagueMemberResponse(
    Guid Id,
    Guid UserId,
    string Username,
    string DisplayName,
    LeagueRole Role,
    DateTime JoinedAt,
    bool IsActive
);
