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
public class TeachersController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TeachersController> _logger;

    public TeachersController(ApplicationDbContext context, ILogger<TeachersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult<IEnumerable<object>>> GetTeachers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;

        var query = _context.Teachers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(t => 
                EF.Functions.Like(t.FirstName, $"%{search}%") ||
                EF.Functions.Like(t.LastName, $"%{search}%") ||
                EF.Functions.Like(t.Email, $"%{search}%") ||
                EF.Functions.Like(t.EmployeeId, $"%{search}%"));
        }

        var teachers = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new
            {
                t.Id,
                t.FirstName,
                t.LastName,
                t.Email,
                t.EmployeeId,
                t.PhoneNumber,
                t.Department,
                t.Specialization,
                t.HireDate,
                t.IsActive
            })
            .ToListAsync();

        return Ok(teachers);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult> GetTeacher(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Invalid teacher ID" });
        }

        var teacher = await _context.Teachers.FindAsync(id);
        if (teacher == null)
        {
            return NotFound(new { message = "Teacher not found" });
        }

        return Ok(new
        {
            teacher.Id,
            teacher.FirstName,
            teacher.LastName,
            teacher.Email,
            teacher.EmployeeId,
            teacher.PhoneNumber,
            teacher.Department,
            teacher.Specialization,
            teacher.HireDate,
            teacher.IsActive
        });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> CreateTeacher([FromBody] CreateTeacherDto createDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var existingTeacher = await _context.Teachers
            .Where(t => t.EmployeeId == createDto.EmployeeId || t.Email == createDto.Email)
            .FirstOrDefaultAsync();

        if (existingTeacher != null)
        {
            return Conflict(new { message = "Teacher with this ID or email already exists" });
        }

        var teacher = new Teacher
        {
            FirstName = createDto.FirstName,
            LastName = createDto.LastName,
            Email = createDto.Email,
            EmployeeId = createDto.EmployeeId,
            PhoneNumber = createDto.PhoneNumber,
            Department = createDto.Department,
            Specialization = createDto.Specialization,
            HireDate = createDto.HireDate
        };

        _context.Teachers.Add(teacher);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Teacher created: {EmployeeId}", teacher.EmployeeId);

        return CreatedAtAction(nameof(GetTeacher), new { id = teacher.Id }, new
        {
            teacher.Id,
            teacher.FirstName,
            teacher.LastName,
            teacher.Email,
            teacher.EmployeeId,
            teacher.PhoneNumber,
            teacher.Department,
            teacher.Specialization,
            teacher.HireDate,
            teacher.IsActive
        });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteTeacher(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Invalid teacher ID" });
        }

        var teacher = await _context.Teachers.FindAsync(id);
        if (teacher == null)
        {
            return NotFound(new { message = "Teacher not found" });
        }

        _context.Teachers.Remove(teacher);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Teacher deleted: {EmployeeId}", teacher.EmployeeId);

        return NoContent();
    }
}

public class CreateTeacherDto
{
    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.StringLength(100, MinimumLength = 2)]
    public required string FirstName { get; set; }

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.StringLength(100, MinimumLength = 2)]
    public required string LastName { get; set; }

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.EmailAddress]
    public required string Email { get; set; }

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.StringLength(50)]
    public required string EmployeeId { get; set; }

    [System.ComponentModel.DataAnnotations.Phone]
    [System.ComponentModel.DataAnnotations.StringLength(20)]
    public string? PhoneNumber { get; set; }

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.StringLength(100)]
    public required string Department { get; set; }

    [System.ComponentModel.DataAnnotations.StringLength(100)]
    public string? Specialization { get; set; }

    [System.ComponentModel.DataAnnotations.Required]
    public DateTime HireDate { get; set; }
}
