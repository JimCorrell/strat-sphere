using Microsoft.EntityFrameworkCore;
using StratSphere.Core.Entities;

namespace StratSphere.Infrastructure.Data;

public interface ITenantProvider
{
    Guid? GetCurrentLeagueId();
    void SetCurrentLeagueId(Guid leagueId);
}

public class StratLeagueDbContext : DbContext
{
    private readonly Guid? _currentLeagueId;

    public StratLeagueDbContext(DbContextOptions<StratLeagueDbContext> options)
        : base(options)
    {
    }

    public StratLeagueDbContext(DbContextOptions<StratLeagueDbContext> options, ITenantProvider tenantProvider)
        : base(options)
    {
        _currentLeagueId = tenantProvider.GetCurrentLeagueId();
    }
    public DbSet<DraftOrder> DraftOrders => Set<DraftOrder>();
    public DbSet<DraftPick> DraftPicks => Set<DraftPick>();

    // Draft
    public DbSet<Draft> Drafts => Set<Draft>();
    public DbSet<GameResult> GameResults => Set<GameResult>();
    public DbSet<LeagueMember> LeagueMembers => Set<LeagueMember>();
    public DbSet<League> Leagues => Set<League>();
    public DbSet<PlayerStats> PlayerStats => Set<PlayerStats>();

    // Player data (shared across leagues)
    public DbSet<Player> Players => Set<Player>();

    // League-scoped data
    public DbSet<RosterEntry> RosterEntries => Set<RosterEntry>();
    public DbSet<ScoutingReport> ScoutingReports => Set<ScoutingReport>();

    // Season/Standings
    public DbSet<Season> Seasons => Set<Season>();
    public DbSet<StandingsEntry> StandingsEntries => Set<StandingsEntry>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    // Core entities
    public DbSet<User> Users => Set<User>();

    public override int SaveChanges()
    {
        SetAuditFields();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(StratLeagueDbContext).Assembly);

        // Apply global query filters for multi-tenancy
        if (_currentLeagueId.HasValue)
        {
            modelBuilder.Entity<Team>().HasQueryFilter(t => t.LeagueId == _currentLeagueId);
            modelBuilder.Entity<LeagueMember>().HasQueryFilter(lm => lm.LeagueId == _currentLeagueId);
            modelBuilder.Entity<RosterEntry>().HasQueryFilter(r => r.LeagueId == _currentLeagueId);
            modelBuilder.Entity<ScoutingReport>().HasQueryFilter(s => s.LeagueId == _currentLeagueId);
            modelBuilder.Entity<Transaction>().HasQueryFilter(t => t.LeagueId == _currentLeagueId);
            modelBuilder.Entity<Draft>().HasQueryFilter(d => d.LeagueId == _currentLeagueId);
            modelBuilder.Entity<DraftOrder>().HasQueryFilter(d => d.LeagueId == _currentLeagueId);
            modelBuilder.Entity<DraftPick>().HasQueryFilter(d => d.LeagueId == _currentLeagueId);
            modelBuilder.Entity<Season>().HasQueryFilter(s => s.LeagueId == _currentLeagueId);
            modelBuilder.Entity<GameResult>().HasQueryFilter(g => g.LeagueId == _currentLeagueId);
            modelBuilder.Entity<StandingsEntry>().HasQueryFilter(s => s.LeagueId == _currentLeagueId);
        }
    }

    private void SetAuditFields()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();
        var now = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.UpdatedAt = now;

                if (entry.Entity.Id == Guid.Empty)
                {
                    entry.Entity.Id = Guid.NewGuid();
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }

        // Set LeagueId for tenant entities if not already set
        if (_currentLeagueId.HasValue)
        {
            var tenantEntries = ChangeTracker.Entries<TenantEntity>()
                .Where(e => e.State == EntityState.Added && e.Entity.LeagueId == Guid.Empty);

            foreach (var entry in tenantEntries)
            {
                entry.Entity.LeagueId = _currentLeagueId.Value;
            }
        }
    }
}
