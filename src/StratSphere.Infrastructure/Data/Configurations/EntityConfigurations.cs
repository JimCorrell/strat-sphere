using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StratSphere.Core.Entities;

namespace StratSphere.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(256);
        builder.Property(u => u.Username).IsRequired().HasMaxLength(50);
        builder.Property(u => u.DisplayName).IsRequired().HasMaxLength(100);
        builder.Property(u => u.PasswordHash).IsRequired();
        
        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.Username).IsUnique();
    }
}

public class LeagueConfiguration : IEntityTypeConfiguration<League>
{
    public void Configure(EntityTypeBuilder<League> builder)
    {
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Name).IsRequired().HasMaxLength(100);
        builder.Property(l => l.Slug).IsRequired().HasMaxLength(50);
        builder.Property(l => l.Description).HasMaxLength(500);
        
        builder.HasIndex(l => l.Slug).IsUnique();
    }
}

public class LeagueMemberConfiguration : IEntityTypeConfiguration<LeagueMember>
{
    public void Configure(EntityTypeBuilder<LeagueMember> builder)
    {
        builder.HasKey(lm => lm.Id);
        
        builder.HasOne(lm => lm.League)
            .WithMany(l => l.Members)
            .HasForeignKey(lm => lm.LeagueId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(lm => lm.User)
            .WithMany(u => u.LeagueMemberships)
            .HasForeignKey(lm => lm.UserId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasIndex(lm => new { lm.LeagueId, lm.UserId }).IsUnique();
    }
}

public class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Name).IsRequired().HasMaxLength(100);
        builder.Property(t => t.Abbreviation).IsRequired().HasMaxLength(5);
        builder.Property(t => t.City).HasMaxLength(50);
        builder.Property(t => t.Division).HasMaxLength(50);
        builder.Property(t => t.Conference).HasMaxLength(50);
        
        builder.HasOne(t => t.League)
            .WithMany(l => l.Teams)
            .HasForeignKey(t => t.LeagueId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(t => t.Owner)
            .WithMany(u => u.Teams)
            .HasForeignKey(t => t.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasIndex(t => new { t.LeagueId, t.Name }).IsUnique();
        builder.HasIndex(t => new { t.LeagueId, t.Abbreviation }).IsUnique();
    }
}

public class PlayerConfiguration : IEntityTypeConfiguration<Player>
{
    public void Configure(EntityTypeBuilder<Player> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.FirstName).IsRequired().HasMaxLength(50);
        builder.Property(p => p.LastName).IsRequired().HasMaxLength(50);
        builder.Property(p => p.MlbId).HasMaxLength(20);
        builder.Property(p => p.MlbamId).HasMaxLength(20);
        builder.Property(p => p.BirthCountry).HasMaxLength(50);
        builder.Property(p => p.BatsHand).HasMaxLength(1);
        builder.Property(p => p.ThrowsHand).HasMaxLength(1);
        builder.Property(p => p.CurrentMlbTeam).HasMaxLength(50);
        builder.Property(p => p.CurrentMlbOrg).HasMaxLength(50);
        builder.Property(p => p.SecondaryPositions).HasMaxLength(50);
        builder.Property(p => p.StratCardNumber).HasMaxLength(20);
        
        builder.HasIndex(p => p.MlbId);
        builder.HasIndex(p => p.MlbamId);
        builder.HasIndex(p => new { p.LastName, p.FirstName });
    }
}

public class RosterEntryConfiguration : IEntityTypeConfiguration<RosterEntry>
{
    public void Configure(EntityTypeBuilder<RosterEntry> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.ContractSalary).HasPrecision(12, 2);
        
        builder.HasOne(r => r.League)
            .WithMany()
            .HasForeignKey(r => r.LeagueId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(r => r.Team)
            .WithMany(t => t.Roster)
            .HasForeignKey(r => r.TeamId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(r => r.Player)
            .WithMany(p => p.RosterEntries)
            .HasForeignKey(r => r.PlayerId)
            .OnDelete(DeleteBehavior.Restrict);
            
        // A player can only be on one team per league
        builder.HasIndex(r => new { r.LeagueId, r.PlayerId }).IsUnique();
    }
}

public class DraftConfiguration : IEntityTypeConfiguration<Draft>
{
    public void Configure(EntityTypeBuilder<Draft> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Name).IsRequired().HasMaxLength(100);
        
        builder.HasOne(d => d.League)
            .WithMany(l => l.Drafts)
            .HasForeignKey(d => d.LeagueId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class DraftPickConfiguration : IEntityTypeConfiguration<DraftPick>
{
    public void Configure(EntityTypeBuilder<DraftPick> builder)
    {
        builder.HasKey(dp => dp.Id);
        
        builder.HasOne(dp => dp.Draft)
            .WithMany(d => d.Picks)
            .HasForeignKey(dp => dp.DraftId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(dp => dp.Team)
            .WithMany(t => t.DraftPicks)
            .HasForeignKey(dp => dp.TeamId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(dp => dp.Player)
            .WithMany(p => p.DraftSelections)
            .HasForeignKey(dp => dp.PlayerId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasIndex(dp => new { dp.DraftId, dp.OverallPickNumber }).IsUnique();
    }
}

public class SeasonConfiguration : IEntityTypeConfiguration<Season>
{
    public void Configure(EntityTypeBuilder<Season> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Name).IsRequired().HasMaxLength(50);
        
        builder.HasOne(s => s.League)
            .WithMany(l => l.Seasons)
            .HasForeignKey(s => s.LeagueId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasIndex(s => new { s.LeagueId, s.Year }).IsUnique();
    }
}

public class GameResultConfiguration : IEntityTypeConfiguration<GameResult>
{
    public void Configure(EntityTypeBuilder<GameResult> builder)
    {
        builder.HasKey(g => g.Id);
        builder.Property(g => g.PlayoffRound).HasMaxLength(50);
        builder.Property(g => g.Notes).HasMaxLength(500);
        
        builder.HasOne(g => g.Season)
            .WithMany(s => s.Games)
            .HasForeignKey(g => g.SeasonId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(g => g.HomeTeam)
            .WithMany(t => t.HomeGames)
            .HasForeignKey(g => g.HomeTeamId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(g => g.AwayTeam)
            .WithMany(t => t.AwayGames)
            .HasForeignKey(g => g.AwayTeamId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class StandingsEntryConfiguration : IEntityTypeConfiguration<StandingsEntry>
{
    public void Configure(EntityTypeBuilder<StandingsEntry> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.WinningPercentage).HasPrecision(5, 3);
        builder.Property(s => s.GamesBack).HasPrecision(5, 1);
        builder.Property(s => s.Streak).HasMaxLength(10);
        builder.Property(s => s.Division).HasMaxLength(50);
        
        builder.HasOne(s => s.Season)
            .WithMany(se => se.Standings)
            .HasForeignKey(s => s.SeasonId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(s => s.Team)
            .WithMany(t => t.Standings)
            .HasForeignKey(s => s.TeamId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasIndex(s => new { s.SeasonId, s.TeamId }).IsUnique();
    }
}

public class ScoutingReportConfiguration : IEntityTypeConfiguration<ScoutingReport>
{
    public void Configure(EntityTypeBuilder<ScoutingReport> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.RiskLevel).HasMaxLength(20);
        builder.Property(s => s.ETA).HasMaxLength(20);
        builder.Property(s => s.Notes).HasMaxLength(2000);
        builder.Property(s => s.Strengths).HasMaxLength(500);
        builder.Property(s => s.Weaknesses).HasMaxLength(500);
        builder.Property(s => s.Comparable).HasMaxLength(100);
        
        builder.HasOne(s => s.Player)
            .WithMany(p => p.ScoutingReports)
            .HasForeignKey(s => s.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(s => s.ScoutedBy)
            .WithMany()
            .HasForeignKey(s => s.ScoutedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
