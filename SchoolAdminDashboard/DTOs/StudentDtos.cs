using System.ComponentModel.DataAnnotations;

namespace SchoolAdminDashboard.DTOs;

public class StudentDto
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
    public required string Email { get; set; }

    [Required]
    [StringLength(50)]
    public required string StudentId { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime DateOfBirth { get; set; }

    [Required]
    [Range(1, 12)]
    public int Grade { get; set; }

    [Phone]
    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    [StringLength(500)]
    public string? Address { get; set; }

    public DateTime EnrollmentDate { get; set; }
    public bool IsActive { get; set; }
}

public class CreateStudentDto
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
    [StringLength(50)]
    [RegularExpression(@"^[A-Z0-9]+$", ErrorMessage = "Student ID must contain only uppercase letters and numbers")]
    public required string StudentId { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime DateOfBirth { get; set; }

    [Required]
    [Range(1, 12)]
    public int Grade { get; set; }

    [Phone]
    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    [StringLength(500)]
    public string? Address { get; set; }
}

public class UpdateStudentDto
{
    [StringLength(100, MinimumLength = 2)]
    public string? FirstName { get; set; }

    [StringLength(100, MinimumLength = 2)]
    public string? LastName { get; set; }

    [EmailAddress]
    public string? Email { get; set; }

    [Range(1, 12)]
    public int? Grade { get; set; }

    [Phone]
    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    [StringLength(500)]
    public string? Address { get; set; }

    public bool? IsActive { get; set; }
}
