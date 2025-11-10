using System.ComponentModel.DataAnnotations;

namespace SchoolAdminDashboard.Models;

public class Teacher
{
    public int Id { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public required string FirstName { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public required string LastName { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(255)]
    public required string Email { get; set; }

    [Required]
    [StringLength(50)]
    public required string EmployeeId { get; set; }

    [Phone]
    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    [Required]
    [StringLength(100)]
    public required string Department { get; set; }

    [StringLength(100)]
    public string? Specialization { get; set; }

    public DateTime HireDate { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Course> Courses { get; set; } = new List<Course>();
}
