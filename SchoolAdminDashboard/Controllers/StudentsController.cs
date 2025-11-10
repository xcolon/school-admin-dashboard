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
public class StudentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<StudentsController> _logger;

    public StudentsController(ApplicationDbContext context, ILogger<StudentsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult<IEnumerable<StudentDto>>> GetStudents(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        // Input validation
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;

        var query = _context.Students.AsQueryable();

        // Secure search with parameterization (prevents SQL injection)
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(s => 
                EF.Functions.Like(s.FirstName, $"%{search}%") ||
                EF.Functions.Like(s.LastName, $"%{search}%") ||
                EF.Functions.Like(s.Email, $"%{search}%") ||
                EF.Functions.Like(s.StudentId, $"%{search}%"));
        }

        var students = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new StudentDto
            {
                Id = s.Id,
                FirstName = s.FirstName,
                LastName = s.LastName,
                Email = s.Email,
                StudentId = s.StudentId,
                DateOfBirth = s.DateOfBirth,
                Grade = s.Grade,
                PhoneNumber = s.PhoneNumber,
                Address = s.Address,
                EnrollmentDate = s.EnrollmentDate,
                IsActive = s.IsActive
            })
            .ToListAsync();

        return Ok(students);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Teacher,Student")]
    public async Task<ActionResult<StudentDto>> GetStudent(int id)
    {
        // Input validation
        if (id <= 0)
        {
            return BadRequest(new { message = "Invalid student ID" });
        }

        var student = await _context.Students
            .Where(s => s.Id == id)
            .Select(s => new StudentDto
            {
                Id = s.Id,
                FirstName = s.FirstName,
                LastName = s.LastName,
                Email = s.Email,
                StudentId = s.StudentId,
                DateOfBirth = s.DateOfBirth,
                Grade = s.Grade,
                PhoneNumber = s.PhoneNumber,
                Address = s.Address,
                EnrollmentDate = s.EnrollmentDate,
                IsActive = s.IsActive
            })
            .FirstOrDefaultAsync();

        if (student == null)
        {
            return NotFound(new { message = "Student not found" });
        }

        return Ok(student);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<StudentDto>> CreateStudent([FromBody] CreateStudentDto createDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Check for duplicate student ID or email (using parameterized queries)
        var existingStudent = await _context.Students
            .Where(s => s.StudentId == createDto.StudentId || s.Email == createDto.Email)
            .FirstOrDefaultAsync();

        if (existingStudent != null)
        {
            return Conflict(new { message = "Student with this ID or email already exists" });
        }

        var student = new Student
        {
            FirstName = createDto.FirstName,
            LastName = createDto.LastName,
            Email = createDto.Email,
            StudentId = createDto.StudentId,
            DateOfBirth = createDto.DateOfBirth,
            Grade = createDto.Grade,
            PhoneNumber = createDto.PhoneNumber,
            Address = createDto.Address
        };

        _context.Students.Add(student);
        await _context.SaveChangesAsync();

        var sanitizedStudentId = student.StudentId.Replace('\n', ' ').Replace('\r', ' ');
        _logger.LogInformation("Student created: {StudentId}", sanitizedStudentId);

        var studentDto = new StudentDto
        {
            Id = student.Id,
            FirstName = student.FirstName,
            LastName = student.LastName,
            Email = student.Email,
            StudentId = student.StudentId,
            DateOfBirth = student.DateOfBirth,
            Grade = student.Grade,
            PhoneNumber = student.PhoneNumber,
            Address = student.Address,
            EnrollmentDate = student.EnrollmentDate,
            IsActive = student.IsActive
        };

        return CreatedAtAction(nameof(GetStudent), new { id = student.Id }, studentDto);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> UpdateStudent(int id, [FromBody] UpdateStudentDto updateDto)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Invalid student ID" });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var student = await _context.Students.FindAsync(id);
        if (student == null)
        {
            return NotFound(new { message = "Student not found" });
        }

        // Update only provided fields
        if (!string.IsNullOrWhiteSpace(updateDto.FirstName))
            student.FirstName = updateDto.FirstName;

        if (!string.IsNullOrWhiteSpace(updateDto.LastName))
            student.LastName = updateDto.LastName;

        if (!string.IsNullOrWhiteSpace(updateDto.Email))
        {
            // Check for duplicate email using parameterized query
            var emailExists = await _context.Students
                .AnyAsync(s => s.Email == updateDto.Email && s.Id != id);
            if (emailExists)
            {
                return Conflict(new { message = "Email already in use" });
            }
            student.Email = updateDto.Email;
        }

        if (updateDto.Grade.HasValue)
            student.Grade = updateDto.Grade.Value;

        if (updateDto.PhoneNumber != null)
            student.PhoneNumber = updateDto.PhoneNumber;

        if (updateDto.Address != null)
            student.Address = updateDto.Address;

        if (updateDto.IsActive.HasValue)
            student.IsActive = updateDto.IsActive.Value;

        await _context.SaveChangesAsync();

        var sanitizedStudentId = student.StudentId.Replace('\n', ' ').Replace('\r', ' ');
        _logger.LogInformation("Student updated: {StudentId}", sanitizedStudentId);

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteStudent(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Invalid student ID" });
        }

        var student = await _context.Students.FindAsync(id);
        if (student == null)
        {
            return NotFound(new { message = "Student not found" });
        }

        _context.Students.Remove(student);
        await _context.SaveChangesAsync();

        var sanitizedStudentId = student.StudentId.Replace('\n', ' ').Replace('\r', ' ');
        _logger.LogInformation("Student deleted: {StudentId}", sanitizedStudentId);

        return NoContent();
    }
}
