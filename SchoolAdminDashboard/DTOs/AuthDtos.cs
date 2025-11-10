using System.ComponentModel.DataAnnotations;

namespace SchoolAdminDashboard.DTOs;

public class LoginDto
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public required string Password { get; set; }
}

public class RegisterDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public required string FirstName { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public required string LastName { get; set; }

    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 8)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$", 
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit and one special character")]
    public required string Password { get; set; }

    [Required]
    [Compare("Password")]
    public required string ConfirmPassword { get; set; }

    [Required]
    public required string Role { get; set; }
}

public class AuthResponseDto
{
    public required string Token { get; set; }
    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Role { get; set; }
    public DateTime ExpiresAt { get; set; }
}
