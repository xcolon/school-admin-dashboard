using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace SchoolAdminDashboard.Models;

public class User : IdentityUser
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public required string FirstName { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public required string LastName { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? LastLoginAt { get; set; }
}
