using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StratSphere.Core.Entities;
using StratSphere.Infrastructure.Data;
using StratSphere.Infrastructure.Services;
using StratSphere.Shared.DTOs;

namespace StratSphere.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly StratSphereDbContext _context;
    private readonly ILogger<AuthController> _logger;
    private readonly IPasswordService _passwordService;

    public AuthController(
        StratSphereDbContext context,
        IAuthService authService,
        IPasswordService passwordService,
        ILogger<AuthController> logger)
    {
        _context = context;
        _authService = authService;
        _passwordService = passwordService;
        _logger = logger;
    }

    /// <summary>
    /// Get current user info from token.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserResponse>> GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        return Ok(new UserResponse(user.Id, user.Email, user.Username, user.DisplayName, user.CreatedAt));
    }

    /// <summary>
    /// Get current user's league memberships.
    /// </summary>
    [HttpGet("me/leagues")]
    [Authorize]
    public async Task<ActionResult<List<LeagueSummaryResponse>>> GetMyLeagues()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        // Admins see all leagues
        if (user.IsAdmin)
        {
            var allLeagues = await _context.Leagues
                .Select(l => new LeagueSummaryResponse(
                    l.Id,
                    l.Name,
                    l.Slug,
                    l.Status,
                    l.Teams.Count
                ))
                .ToListAsync();

            return Ok(allLeagues);
        }

        // Regular users see only their leagues
        var myLeagues = await _context.LeagueMembers
            .Include(m => m.League)
            .ThenInclude(l => l.Teams)
            .Where(m => m.UserId == userId && m.IsActive)
            .Select(m => new LeagueSummaryResponse(
                m.League.Id,
                m.League.Name,
                m.League.Slug,
                m.League.Status,
                m.League.Teams.Count
            ))
            .ToListAsync();

        return Ok(myLeagues);
    }

    /// <summary>
    /// Login with email/username and password.
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u =>
                u.Email.ToLower() == request.EmailOrUsername.ToLower() ||
                u.Username.ToLower() == request.EmailOrUsername.ToLower());

        if (user == null)
        {
            return Unauthorized(new { message = "Invalid credentials" });
        }

        if (!user.IsActive)
        {
            return Unauthorized(new { message = "Account is disabled" });
        }

        if (!_passwordService.VerifyPassword(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Invalid credentials" });
        }

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("User logged in: {Username}", user.Username);

        var roles = user.IsAdmin ? new[] { "Admin" } : Array.Empty<string>();
        var token = _authService.GenerateToken(user, roles);

        return Ok(new AuthResponse(
            token,
            DateTime.UtcNow.AddDays(1),
            new UserResponse(user.Id, user.Email, user.Username, user.DisplayName, user.CreatedAt)
        ));
    }

    /// <summary>
    /// Register a new user.
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterUserRequest request)
    {
        // Check if email already exists
        var existingEmail = await _context.Users.AnyAsync(u => u.Email.ToLower() == request.Email.ToLower());
        if (existingEmail)
        {
            return Conflict(new { message = "Email already registered" });
        }

        // Check if username already exists
        var existingUsername = await _context.Users.AnyAsync(u => u.Username.ToLower() == request.Username.ToLower());
        if (existingUsername)
        {
            return Conflict(new { message = "Username already taken" });
        }

        var user = new User
        {
            Email = request.Email.ToLower(),
            Username = request.Username,
            DisplayName = request.DisplayName,
            PasswordHash = _passwordService.HashPassword(request.Password),
            IsActive = true,
            IsAdmin = false
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User registered: {Username} ({Email})", user.Username, user.Email);

        var roles = user.IsAdmin ? new[] { "Admin" } : Array.Empty<string>();
        var token = _authService.GenerateToken(user, roles);

        return Ok(new AuthResponse(
            token,
            DateTime.UtcNow.AddDays(1),
            new UserResponse(user.Id, user.Email, user.Username, user.DisplayName, user.CreatedAt)
        ));
    }
}
