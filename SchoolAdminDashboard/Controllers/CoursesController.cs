using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolAdminDashboard.Data;
using SchoolAdminDashboard.DTOs;
using SchoolAdminDashboard.Models;

namespace SchoolAdminDashboard.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CoursesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CoursesController> _logger;

    public CoursesController(ApplicationDbContext context, ILogger<CoursesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Teacher,Student")]
    public async Task<ActionResult<IEnumerable<object>>> GetCourses(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;

        var query = _context.Courses.Include(c => c.Teacher).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c => 
                EF.Functions.Like(c.Name, $"%{search}%") ||
                EF.Functions.Like(c.CourseCode, $"%{search}%") ||
                EF.Functions.Like(c.Teacher!.FirstName, $"%{search}%") ||
                EF.Functions.Like(c.Teacher!.LastName, $"%{search}%"));
        }

        var courses = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.CourseCode,
                c.Description,
                c.Credits,
                c.TeacherId,
                TeacherName = c.Teacher != null ? $"{c.Teacher.FirstName} {c.Teacher.LastName}" : "N/A",
                c.Semester,
                c.Year,
                c.MaxCapacity,
                c.IsActive
            })
            .ToListAsync();

        return Ok(courses);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Teacher,Student")]
    public async Task<ActionResult> GetCourse(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Invalid course ID" });
        }

        var course = await _context.Courses
            .Include(c => c.Teacher)
            .Where(c => c.Id == id)
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.CourseCode,
                c.Description,
                c.Credits,
                c.TeacherId,
                TeacherName = c.Teacher != null ? $"{c.Teacher.FirstName} {c.Teacher.LastName}" : "N/A",
                c.Semester,
                c.Year,
                c.MaxCapacity,
                c.IsActive
            })
            .FirstOrDefaultAsync();

        if (course == null)
        {
            return NotFound(new { message = "Course not found" });
        }

        return Ok(course);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> CreateCourse([FromBody] CreateCourseDto createDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var existingCourse = await _context.Courses
            .Where(c => c.CourseCode == createDto.CourseCode)
            .FirstOrDefaultAsync();

        if (existingCourse != null)
        {
            return Conflict(new { message = "Course with this code already exists" });
        }

        var teacher = await _context.Teachers.FindAsync(createDto.TeacherId);
        if (teacher == null)
        {
            return BadRequest(new { message = "Teacher not found" });
        }

        var course = new Course
        {
            Name = createDto.Name,
            CourseCode = createDto.CourseCode,
            Description = createDto.Description,
            Credits = createDto.Credits,
            TeacherId = createDto.TeacherId,
            Semester = createDto.Semester,
            Year = createDto.Year,
            MaxCapacity = createDto.MaxCapacity
        };

        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        var sanitizedCourseCode = course.CourseCode.Replace('\n', ' ').Replace('\r', ' ');
        _logger.LogInformation("Course created: {CourseCode}", sanitizedCourseCode);

        return CreatedAtAction(nameof(GetCourse), new { id = course.Id }, new
        {
            course.Id,
            course.Name,
            course.CourseCode,
            course.Description,
            course.Credits,
            course.TeacherId,
            TeacherName = $"{teacher.FirstName} {teacher.LastName}",
            course.Semester,
            course.Year,
            course.MaxCapacity,
            course.IsActive
        });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteCourse(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Invalid course ID" });
        }

        var course = await _context.Courses.FindAsync(id);
        if (course == null)
        {
            return NotFound(new { message = "Course not found" });
        }

        _context.Courses.Remove(course);
        await _context.SaveChangesAsync();

        var sanitizedCourseCode = course.CourseCode.Replace('\n', ' ').Replace('\r', ' ');
        _logger.LogInformation("Course deleted: {CourseCode}", sanitizedCourseCode);

        return NoContent();
    }
}

public class CreateCourseDto
{
    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.StringLength(100, MinimumLength = 2)]
    public required string Name { get; set; }

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.StringLength(50)]
    [System.ComponentModel.DataAnnotations.RegularExpression(@"^[A-Z]{2,4}\d{3,4}$", 
        ErrorMessage = "Course code must be 2-4 uppercase letters followed by 3-4 digits")]
    public required string CourseCode { get; set; }

    [System.ComponentModel.DataAnnotations.StringLength(1000)]
    public string? Description { get; set; }

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.Range(1, 10)]
    public int Credits { get; set; }

    [System.ComponentModel.DataAnnotations.Required]
    public int TeacherId { get; set; }

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.StringLength(20)]
    public required string Semester { get; set; }

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.Range(2020, 2100)]
    public int Year { get; set; }

    [System.ComponentModel.DataAnnotations.Range(1, 500)]
    public int MaxCapacity { get; set; } = 30;
}
