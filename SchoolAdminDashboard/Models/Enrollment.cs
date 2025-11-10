using System.ComponentModel.DataAnnotations;

namespace SchoolAdminDashboard.Models;

public class Enrollment
{
    public int Id { get; set; }

    [Required]
    public int StudentId { get; set; }
    public Student? Student { get; set; }

    [Required]
    public int CourseId { get; set; }
    public Course? Course { get; set; }

    public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;

    [Range(0, 100)]
    public decimal? Grade { get; set; }

    [StringLength(20)]
    public string Status { get; set; } = "Active";
}
