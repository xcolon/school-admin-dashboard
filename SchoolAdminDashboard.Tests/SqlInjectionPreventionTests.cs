using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using SchoolAdminDashboard.DTOs;
using Xunit;

namespace SchoolAdminDashboard.Tests;

public class SqlInjectionPreventionTests : IClassFixture<SqlInjectionTestFactory>
{
    private readonly SqlInjectionTestFactory _factory;

    public SqlInjectionPreventionTests(SqlInjectionTestFactory factory)
    {
        _factory = factory;
    }

    private async Task<string> GetAdminToken()
    {
        var client = _factory.CreateClient();
        var registerDto = new RegisterDto
        {
            FirstName = "Admin",
            LastName = "User",
            Email = $"admin{Guid.NewGuid()}@example.com",
            Password = "SecureP@ssw0rd123",
            ConfirmPassword = "SecureP@ssw0rd123",
            Role = "Admin"
        };

        var response = await client.PostAsJsonAsync("/api/Auth/register", registerDto);
        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        return result?.Token ?? string.Empty;
    }

    private async Task CreateTestStudent(HttpClient client, string studentId)
    {
        var studentDto = new CreateStudentDto
        {
            FirstName = "Test",
            LastName = "Student",
            Email = $"test{studentId}@example.com",
            StudentId = studentId,
            DateOfBirth = DateTime.Parse("2000-01-01"),
            Grade = 10
        };

        await client.PostAsJsonAsync("/api/Students", studentDto);
    }

    [Fact]
    public async Task SearchStudents_WithSqlInjectionAttempt_ShouldBeSafe()
    {
        // Arrange
        var client = _factory.CreateClient();
        var token = await GetAdminToken();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        await CreateTestStudent(client, "STU001");

        // Act - Try SQL injection in search parameter
        var response = await client.GetAsync("/api/Students?search='; DROP TABLE Students; --");

        // Assert - Should return OK without executing malicious SQL
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var students = await response.Content.ReadFromJsonAsync<List<StudentDto>>();
        Assert.NotNull(students);
    }

    [Fact]
    public async Task SearchStudents_WithUnionInjectionAttempt_ShouldBeSafe()
    {
        // Arrange
        var client = _factory.CreateClient();
        var token = await GetAdminToken();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        await CreateTestStudent(client, "STU002");

        // Act - Try UNION-based SQL injection
        var response = await client.GetAsync("/api/Students?search=' UNION SELECT * FROM Users --");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var students = await response.Content.ReadFromJsonAsync<List<StudentDto>>();
        Assert.NotNull(students);
    }

    [Fact]
    public async Task CreateStudent_WithSqlInjectionInStudentId_ShouldBeSafe()
    {
        // Arrange
        var client = _factory.CreateClient();
        var token = await GetAdminToken();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var studentDto = new CreateStudentDto
        {
            FirstName = "Test",
            LastName = "Student",
            Email = "injection@example.com",
            StudentId = "'; DROP TABLE Students; --",  // SQL injection attempt
            DateOfBirth = DateTime.Parse("2000-01-01"),
            Grade = 10
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/Students", studentDto);

        // Assert - Should fail validation (contains special characters) but not execute SQL
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateStudent_WithXssInName_ShouldStoreSafely()
    {
        // Arrange
        var client = _factory.CreateClient();
        var token = await GetAdminToken();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var studentDto = new CreateStudentDto
        {
            FirstName = "<script>alert('XSS')</script>",  // XSS attempt
            LastName = "Student",
            Email = "xss@example.com",
            StudentId = "STU003",
            DateOfBirth = DateTime.Parse("2000-01-01"),
            Grade = 10
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/Students", studentDto);

        // Assert - Should store the data safely (not execute script)
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var created = await response.Content.ReadFromJsonAsync<StudentDto>();
        Assert.NotNull(created);
        
        // Verify data is stored as-is (will be escaped on output by framework)
        var getResponse = await client.GetAsync($"/api/Students/{created.Id}");
        var retrieved = await getResponse.Content.ReadFromJsonAsync<StudentDto>();
        Assert.NotNull(retrieved);
        Assert.Equal("<script>alert('XSS')</script>", retrieved.FirstName);
    }

    [Fact]
    public async Task Login_WithSqlInjectionInEmail_ShouldBeSafe()
    {
        // Arrange
        var client = _factory.CreateClient();
        var loginDto = new LoginDto
        {
            Email = "admin@example.com' OR '1'='1",  // SQL injection attempt
            Password = "anything"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/Auth/login", loginDto);

        // Assert - Should return Unauthorized, not bypass authentication
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SearchStudents_WithWildcardCharacters_ShouldBeSafe()
    {
        // Arrange
        var client = _factory.CreateClient();
        var token = await GetAdminToken();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        await CreateTestStudent(client, "STU004");

        // Act - Try search with SQL wildcards
        var response = await client.GetAsync("/api/Students?search=%");

        // Assert - Should handle wildcards safely
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateStudent_WithSqlInjectionInEmail_ShouldBeSafe()
    {
        // Arrange
        var client = _factory.CreateClient();
        var token = await GetAdminToken();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        await CreateTestStudent(client, "STU005");
        var studentsResponse = await client.GetAsync("/api/Students?search=STU005");
        var students = await studentsResponse.Content.ReadFromJsonAsync<List<StudentDto>>();
        var student = students?.FirstOrDefault();
        Assert.NotNull(student);

        var updateDto = new UpdateStudentDto
        {
            Email = "test@example.com'; DROP TABLE Students; --"  // SQL injection attempt
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/Students/{student.Id}", updateDto);

        // Assert - Entity Framework uses parameterized queries, so this is safely stored
        // The response may be OK or Conflict (if email exists), but data won't be modified maliciously
        Assert.True(response.StatusCode == HttpStatusCode.NoContent || 
                    response.StatusCode == HttpStatusCode.Conflict ||
                    response.StatusCode == HttpStatusCode.BadRequest);
        
        // Verify student still exists (table not dropped)
        var verifyResponse = await client.GetAsync($"/api/Students/{student.Id}");
        Assert.Equal(HttpStatusCode.OK, verifyResponse.StatusCode);
    }

    [Fact]
    public async Task GetStudent_WithInvalidIdType_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var token = await GetAdminToken();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act - Try to pass SQL injection as ID
        var response = await client.GetAsync("/api/Students/1' OR '1'='1");

        // Assert - Should return 400 Bad Request (type mismatch)
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
