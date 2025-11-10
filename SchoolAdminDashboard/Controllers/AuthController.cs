using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SchoolAdminDashboard.DTOs;
using SchoolAdminDashboard.Models;
using SchoolAdminDashboard.Services;

namespace SchoolAdminDashboard.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        RoleManager<IdentityRole> roleManager,
        ITokenService tokenService,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _tokenService = tokenService;
        _logger = logger;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto registerDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Validate role
        var validRoles = new[] { "Admin", "Teacher", "Student" };
        if (!validRoles.Contains(registerDto.Role))
        {
            return BadRequest(new { message = "Invalid role specified" });
        }

        // Check if user already exists
        var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
        if (existingUser != null)
        {
            return Conflict(new { message = "User with this email already exists" });
        }

        var user = new User
        {
            UserName = registerDto.Email,
            Email = registerDto.Email,
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);

        if (!result.Succeeded)
        {
            return BadRequest(new { message = "User creation failed", errors = result.Errors });
        }

        // Ensure role exists
        if (!await _roleManager.RoleExistsAsync(registerDto.Role))
        {
            await _roleManager.CreateAsync(new IdentityRole(registerDto.Role));
        }

        // Assign role to user
        await _userManager.AddToRoleAsync(user, registerDto.Role);

        var token = _tokenService.GenerateToken(user, registerDto.Role);

        // Log using user ID instead of email to avoid exposing PII in logs
        _logger.LogInformation("User {UserId} registered successfully with role {Role}", user.Id, registerDto.Role);

        return Ok(new AuthResponseDto
        {
            Token = token,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = registerDto.Role,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60)
        });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await _userManager.FindByEmailAsync(loginDto.Email);
        if (user == null)
        {
            return Unauthorized(new { message = "Invalid credentials" });
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
            {
                // Log user ID instead of email for security/privacy
                _logger.LogWarning("User account {UserId} locked out", user.Id);
                return Unauthorized(new { message = "Account locked due to multiple failed login attempts" });
            }

            // Log failed attempt with user ID, not email
            _logger.LogWarning("Failed login attempt for user {UserId}", user.Id);
            return Unauthorized(new { message = "Invalid credentials" });
        }

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Student";

        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        var token = _tokenService.GenerateToken(user, role);

        _logger.LogInformation("User {Email} logged in successfully", user.Email);

        return Ok(new AuthResponseDto
        {
            Token = token,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = role,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60)
        });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult> GetCurrentUser()
    {
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
        {
            return Unauthorized();
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return NotFound();
        }

        var roles = await _userManager.GetRolesAsync(user);

        return Ok(new
        {
            user.Email,
            user.FirstName,
            user.LastName,
            Role = roles.FirstOrDefault(),
            user.LastLoginAt
        });
    }
}
