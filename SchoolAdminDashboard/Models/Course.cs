using System.ComponentModel.DataAnnotations;

namespace SchoolAdminDashboard.Models;

public class Course
{
    public int Id { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public required string Name { get; set; }

    [Required]
    [StringLength(50)]
    [RegularExpression(@"^[A-Z]{2,4}\d{3,4}$", ErrorMessage = "Course code must be 2-4 uppercase letters followed by 3-4 digits")]
    public required string CourseCode { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required]
    [Range(1, 10)]
    public int Credits { get; set; }

    [Required]
    public int TeacherId { get; set; }
    public Teacher? Teacher { get; set; }

    [Required]
    [StringLength(20)]
    public required string Semester { get; set; }

    [Required]
    [Range(2020, 2100)]
    public int Year { get; set; }

    [Range(1, 500)]
    public int MaxCapacity { get; set; } = 30;

    public bool IsActive { get; set; } = true;

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
