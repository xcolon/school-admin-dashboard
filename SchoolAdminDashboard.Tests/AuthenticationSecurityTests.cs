using System.Net;
using System.Net.Http.Json;
using SchoolAdminDashboard.DTOs;
using Xunit;

namespace SchoolAdminDashboard.Tests;

public class AuthenticationSecurityTests : IClassFixture<AuthTestFactory>
{
    private readonly AuthTestFactory _factory;

    public AuthenticationSecurityTests(AuthTestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Register_WithWeakPassword_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var registerDto = new RegisterDto
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Password = "weak",  // Weak password
            ConfirmPassword = "weak",
            Role = "Student"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/Auth/register", registerDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithStrongPassword_ShouldSucceed()
    {
        // Arrange
        var client = _factory.CreateClient();
        var registerDto = new RegisterDto
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Password = "SecureP@ssw0rd123",
            ConfirmPassword = "SecureP@ssw0rd123",
            Role = "Student"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/Auth/register", registerDto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(result);
        Assert.NotEmpty(result.Token);
    }

    [Fact]
    public async Task Register_WithInvalidRole_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var registerDto = new RegisterDto
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test2@example.com",
            Password = "SecureP@ssw0rd123",
            ConfirmPassword = "SecureP@ssw0rd123",
            Role = "InvalidRole"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/Auth/register", registerDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();
        var loginDto = new LoginDto
        {
            Email = "nonexistent@example.com",
            Password = "WrongPassword123!"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/Auth/login", loginDto);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_AfterRegistration_ShouldSucceed()
    {
        // Arrange
        var client = _factory.CreateClient();
        var registerDto = new RegisterDto
        {
            FirstName = "Test",
            LastName = "User",
            Email = "logintest@example.com",
            Password = "SecureP@ssw0rd123",
            ConfirmPassword = "SecureP@ssw0rd123",
            Role = "Student"
        };

        await client.PostAsJsonAsync("/api/Auth/register", registerDto);

        var loginDto = new LoginDto
        {
            Email = "logintest@example.com",
            Password = "SecureP@ssw0rd123"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/Auth/login", loginDto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(result);
        Assert.NotEmpty(result.Token);
    }

    [Fact]
    public async Task Register_WithMismatchedPasswords_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var registerDto = new RegisterDto
        {
            FirstName = "Test",
            LastName = "User",
            Email = "mismatch@example.com",
            Password = "SecureP@ssw0rd123",
            ConfirmPassword = "DifferentP@ssw0rd123",
            Role = "Student"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/Auth/register", registerDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithInvalidEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var registerDto = new RegisterDto
        {
            FirstName = "Test",
            LastName = "User",
            Email = "invalid-email",
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
