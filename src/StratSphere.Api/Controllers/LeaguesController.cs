using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StratSphere.Core.Entities;
using StratSphere.Infrastructure.Data;
using StratSphere.Shared.DTOs;

namespace StratSphere.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LeaguesController : ControllerBase
{
    private readonly StratLeagueDbContext _context;
    private readonly ILogger<LeaguesController> _logger;

    public LeaguesController(StratLeagueDbContext context, ILogger<LeaguesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all leagues (optionally filtered by user membership).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<LeagueSummaryResponse>>> GetLeagues([FromQuery] Guid? userId)
    {
        var query = _context.Leagues.AsQueryable();

        if (userId.HasValue)
        {
            query = query.Where(l => l.Members.Any(m => m.UserId == userId.Value && m.IsActive));
        }

        var leagues = await query
            .Select(l => new LeagueSummaryResponse(
                l.Id,
                l.Name,
                l.Slug,
                l.Status,
                l.Teams.Count
            ))
            .ToListAsync();

        return Ok(leagues);
    }

    /// <summary>
    /// Get a specific league by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<LeagueResponse>> GetLeague(Guid id)
    {
        var league = await _context.Leagues
            .Where(l => l.Id == id)
            .Select(l => new LeagueResponse(
                l.Id,
                l.Name,
                l.Slug,
                l.Description,
                l.Status,
                l.CurrentSeason,
                l.CurrentPhase,
                l.MaxTeams,
                l.RosterSize,
                l.ActiveRosterSize,
                l.UseDH,
                l.Teams.Count,
                l.Members.Count(m => m.IsActive),
                l.CreatedAt
            ))
            .FirstOrDefaultAsync();

        if (league == null)
        {
            return NotFound();
        }

        return Ok(league);
    }

    /// <summary>
    /// Get a league by its URL slug.
    /// </summary>
    [HttpGet("by-slug/{slug}")]
    public async Task<ActionResult<LeagueResponse>> GetLeagueBySlug(string slug)
    {
        var league = await _context.Leagues
            .Where(l => l.Slug == slug.ToLower())
            .Select(l => new LeagueResponse(
                l.Id,
                l.Name,
                l.Slug,
                l.Description,
                l.Status,
                l.CurrentSeason,
                l.CurrentPhase,
                l.MaxTeams,
                l.RosterSize,
                l.ActiveRosterSize,
                l.UseDH,
                l.Teams.Count,
                l.Members.Count(m => m.IsActive),
                l.CreatedAt
            ))
            .FirstOrDefaultAsync();

        if (league == null)
        {
            return NotFound();
        }

        return Ok(league);
    }

    /// <summary>
    /// Create a new league.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<LeagueResponse>> CreateLeague(
        [FromBody] CreateLeagueRequest request,
        [FromQuery] Guid creatorUserId) // TODO: Get from auth
    {
        // Generate slug from name
        var slug = GenerateSlug(request.Name);
        
        // Check if slug already exists
        var existingSlug = await _context.Leagues.AnyAsync(l => l.Slug == slug);
        if (existingSlug)
        {
            slug = $"{slug}-{Guid.NewGuid().ToString()[..8]}";
        }

        var league = new League
        {
            Name = request.Name,
            Description = request.Description,
            Slug = slug,
            MaxTeams = request.MaxTeams,
            RosterSize = request.RosterSize,
            ActiveRosterSize = request.ActiveRosterSize,
            UseDH = request.UseDH
        };

        _context.Leagues.Add(league);

        // Add creator as commissioner
        var membership = new LeagueMember
        {
            LeagueId = league.Id,
            UserId = creatorUserId,
            Role = Core.Entities.LeagueRole.Commissioner,
            JoinedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.LeagueMembers.Add(membership);

        await _context.SaveChangesAsync();

        _logger.LogInformation("Created league {LeagueId} '{LeagueName}' by user {UserId}", 
            league.Id, league.Name, creatorUserId);

        var response = new LeagueResponse(
            league.Id,
            league.Name,
            league.Slug,
            league.Description,
            league.Status,
            league.CurrentSeason,
            league.CurrentPhase,
            league.MaxTeams,
            league.RosterSize,
            league.ActiveRosterSize,
            league.UseDH,
            0,
            1,
            league.CreatedAt
        );

        return CreatedAtAction(nameof(GetLeague), new { id = league.Id }, response);
    }

    /// <summary>
    /// Update a league's settings.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<LeagueResponse>> UpdateLeague(Guid id, [FromBody] UpdateLeagueRequest request)
    {
        var league = await _context.Leagues.FindAsync(id);
        
        if (league == null)
        {
            return NotFound();
        }

        // Update only provided fields
        if (request.Name != null)
        {
            league.Name = request.Name;
        }
        if (request.Description != null)
        {
            league.Description = request.Description;
        }
        if (request.MaxTeams.HasValue)
        {
            league.MaxTeams = request.MaxTeams.Value;
        }
        if (request.RosterSize.HasValue)
        {
            league.RosterSize = request.RosterSize.Value;
        }
        if (request.ActiveRosterSize.HasValue)
        {
            league.ActiveRosterSize = request.ActiveRosterSize.Value;
        }
        if (request.UseDH.HasValue)
        {
            league.UseDH = request.UseDH.Value;
        }
        if (request.Status.HasValue)
        {
            league.Status = request.Status.Value;
        }

        await _context.SaveChangesAsync();

        return await GetLeague(id);
    }

    /// <summary>
    /// Get all members of a league.
    /// </summary>
    [HttpGet("{leagueId:guid}/members")]
    public async Task<ActionResult<List<LeagueMemberResponse>>> GetMembers(Guid leagueId)
    {
        var members = await _context.LeagueMembers
            .Include(m => m.User)
            .Where(m => m.LeagueId == leagueId)
            .Select(m => new LeagueMemberResponse(
                m.Id,
                m.UserId,
                m.User.Username,
                m.User.DisplayName,
                (Shared.DTOs.LeagueRole)m.Role,
                m.JoinedAt,
                m.IsActive
            ))
            .ToListAsync();

        return Ok(members);
    }

    /// <summary>
    /// Add a member to a league.
    /// </summary>
    [HttpPost("{leagueId:guid}/members")]
    public async Task<ActionResult<LeagueMemberResponse>> AddMember(Guid leagueId, [FromBody] AddMemberRequest request)
    {
        var league = await _context.Leagues.FindAsync(leagueId);
        if (league == null)
        {
            return NotFound("League not found");
        }

        var user = await _context.Users.FindAsync(request.UserId);
        if (user == null)
        {
            return NotFound("User not found");
        }

        // Check if already a member
        var existingMembership = await _context.LeagueMembers
            .AnyAsync(m => m.LeagueId == leagueId && m.UserId == request.UserId);
        
        if (existingMembership)
        {
            return Conflict("User is already a member of this league");
        }

        var membership = new LeagueMember
        {
            LeagueId = leagueId,
            UserId = request.UserId,
            Role = (Core.Entities.LeagueRole)request.Role,
            JoinedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.LeagueMembers.Add(membership);
        await _context.SaveChangesAsync();

        var response = new LeagueMemberResponse(
            membership.Id,
            membership.UserId,
            user.Username,
            user.DisplayName,
            (Shared.DTOs.LeagueRole)membership.Role,
            membership.JoinedAt,
            membership.IsActive
        );

        return Created($"/api/leagues/{leagueId}/members/{membership.Id}", response);
    }

    private static string GenerateSlug(string name)
    {
        return name
            .ToLower()
            .Replace(" ", "-")
            .Replace("'", "")
            .Replace("\"", "");
    }
}
