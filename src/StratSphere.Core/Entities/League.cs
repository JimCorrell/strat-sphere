using StratSphere.Shared.Enums;

namespace StratSphere.Core.Entities;

public class League : BaseEntity
{
    public int ActiveRosterSize { get; set; } = 25;
    public SeasonPhase CurrentPhase { get; set; } = SeasonPhase.PreSeason;
    public int CurrentSeason { get; set; } = 1;
    public string? Description { get; set; }
    public ICollection<Draft> Drafts { get; set; } = new List<Draft>();

    // League settings
    public int MaxTeams { get; set; } = 30;

    // Navigation properties
    public ICollection<LeagueMember> Members { get; set; } = new List<LeagueMember>();
    public string Name { get; set; } = string.Empty;
    public int RosterSize { get; set; } = 40;
    public ICollection<Season> Seasons { get; set; } = new List<Season>();
    public string Slug { get; set; } = string.Empty; // URL-friendly identifier
    public LeagueStatus Status { get; set; } = LeagueStatus.Setup;
    public ICollection<Team> Teams { get; set; } = new List<Team>();
    public bool UseDH { get; set; } = true;
}
