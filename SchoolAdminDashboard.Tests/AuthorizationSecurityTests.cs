using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using SchoolAdminDashboard.DTOs;
using Xunit;

namespace SchoolAdminDashboard.Tests;

public class AuthorizationSecurityTests : IClassFixture<AuthzTestFactory>
{
    private readonly AuthzTestFactory _factory;

    public AuthorizationSecurityTests(AuthzTestFactory factory)
    {
        _factory = factory;
    }

    private async Task<string> GetTokenForRole(string role)
    {
        var client = _factory.CreateClient();
        var registerDto = new RegisterDto
        {
            FirstName = "Test",
            LastName = "User",
            Email = $"{role.ToLower()}{Guid.NewGuid()}@example.com",
            Password = "SecureP@ssw0rd123",
            ConfirmPassword = "SecureP@ssw0rd123",
            Role = role
        };

        var response = await client.PostAsJsonAsync("/api/Auth/register", registerDto);
        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        return result?.Token ?? string.Empty;
    }

    [Fact]
    public async Task GetStudents_WithoutToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/Students");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetStudents_WithAdminToken_ShouldSucceed()
    {
        // Arrange
        var client = _factory.CreateClient();
        var token = await GetTokenForRole("Admin");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync("/api/Students");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetStudents_WithTeacherToken_ShouldSucceed()
    {
        // Arrange
        var client = _factory.CreateClient();
        var token = await GetTokenForRole("Teacher");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync("/api/Students");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetStudents_WithStudentToken_ShouldReturnForbidden()
    {
        // Arrange
        var client = _factory.CreateClient();
        var token = await GetTokenForRole("Student");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync("/api/Students");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateStudent_WithAdminToken_ShouldSucceed()
    {
        // Arrange
        var client = _factory.CreateClient();
        var token = await GetTokenForRole("Admin");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var studentDto = new CreateStudentDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            StudentId = "STU001",
            DateOfBirth = DateTime.Parse("2000-01-01"),
            Grade = 10
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/Students", studentDto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateStudent_WithTeacherToken_ShouldReturnForbidden()
    {
        // Arrange
        var client = _factory.CreateClient();
        var token = await GetTokenForRole("Teacher");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var studentDto = new CreateStudentDto
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@example.com",
            StudentId = "STU002",
            DateOfBirth = DateTime.Parse("2000-01-01"),
            Grade = 10
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/Students", studentDto);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteStudent_WithAdminToken_ShouldSucceed()
    {
        // Arrange
        var client = _factory.CreateClient();
        var token = await GetTokenForRole("Admin");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create a student first
        var studentDto = new CreateStudentDto
        {
            FirstName = "Delete",
            LastName = "Test",
            Email = "delete.test@example.com",
            StudentId = "STU999",
            DateOfBirth = DateTime.Parse("2000-01-01"),
            Grade = 10
        };

        var createResponse = await client.PostAsJsonAsync("/api/Students", studentDto);
        var createdStudent = await createResponse.Content.ReadFromJsonAsync<StudentDto>();

        // Act
        var deleteResponse = await client.DeleteAsync($"/api/Students/{createdStudent!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task GetCurrentUser_WithValidToken_ShouldReturnUserInfo()
    {
        // Arrange
        var client = _factory.CreateClient();
        var token = await GetTokenForRole("Admin");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync("/api/Auth/me");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetCurrentUser_WithoutToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/Auth/me");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
