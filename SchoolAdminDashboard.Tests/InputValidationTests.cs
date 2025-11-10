using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using SchoolAdminDashboard.DTOs;
using Xunit;

namespace SchoolAdminDashboard.Tests;

public class InputValidationTests : IClassFixture<ValidationTestFactory>
{
    private readonly ValidationTestFactory _factory;

    public InputValidationTests(ValidationTestFactory factory)
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

    [Fact]
    public async Task CreateStudent_WithInvalidEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var token = await GetAdminToken();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var studentDto = new CreateStudentDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "invalid-email",  // Invalid email
            StudentId = "STU001",
            DateOfBirth = DateTime.Parse("2000-01-01"),
            Grade = 10
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/Students", studentDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateStudent_WithInvalidStudentId_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var token = await GetAdminToken();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var studentDto = new CreateStudentDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            StudentId = "stu-001",  // Invalid format (lowercase)
            DateOfBirth = DateTime.Parse("2000-01-01"),
            Grade = 10
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/Students", studentDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateStudent_WithInvalidGrade_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var token = await GetAdminToken();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var studentDto = new CreateStudentDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john2@example.com",
            StudentId = "STU002",
            DateOfBirth = DateTime.Parse("2000-01-01"),
            Grade = 15  // Invalid grade (out of range)
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/Students", studentDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateStudent_WithExcessivelyLongName_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var token = await GetAdminToken();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var studentDto = new CreateStudentDto
        {
            FirstName = new string('A', 101),  // Exceeds max length
            LastName = "Doe",
            Email = "long@example.com",
            StudentId = "STU003",
            DateOfBirth = DateTime.Parse("2000-01-01"),
            Grade = 10
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/Students", studentDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateStudent_WithValidData_ShouldSucceed()
    {
        // Arrange
        var client = _factory.CreateClient();
        var token = await GetAdminToken();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var studentDto = new CreateStudentDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "valid@example.com",
            StudentId = "STU004",
            DateOfBirth = DateTime.Parse("2000-01-01"),
            Grade = 10,
            PhoneNumber = "123-456-7890",
            Address = "123 Main St"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/Students", studentDto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task UpdateStudent_WithInvalidId_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var token = await GetAdminToken();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var updateDto = new UpdateStudentDto
        {
            FirstName = "Updated"
        };

        // Act
        var response = await client.PutAsJsonAsync("/api/Students/-1", updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateStudent_WithMissingRequiredFields_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var token = await GetAdminToken();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var invalidDto = new { FirstName = "John" }; // Missing required fields

        // Act
        var response = await client.PostAsJsonAsync("/api/Students", invalidDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithShortFirstName_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var registerDto = new RegisterDto
        {
            FirstName = "A",  // Too short
            LastName = "User",
            Email = "shortname@example.com",
            Password = "SecureP@ssw0rd123",
            ConfirmPassword = "SecureP@ssw0rd123",
            Role = "Student"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/Auth/register", registerDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
