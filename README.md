# School Admin Dashboard

A secure .NET 9.0 Web API for school administration with comprehensive security features including JWT authentication, role-based access control (RBAC), input validation, and SQL injection prevention.

## Features

### Security Features
- **JWT Authentication**: Secure token-based authentication with configurable expiration
- **Role-Based Access Control (RBAC)**: Three roles (Admin, Teacher, Student) with granular permissions
- **Password Security**: Strong password requirements with hashing via ASP.NET Core Identity
- **Input Validation**: Comprehensive validation using Data Annotations and model binding
- **SQL Injection Prevention**: Parameterized queries with Entity Framework Core
- **Security Headers**: X-Content-Type-Options, X-Frame-Options, X-XSS-Protection, CSP
- **CORS Configuration**: Configurable allowed origins for API access
- **Account Lockout**: Automatic lockout after 5 failed login attempts (15 minutes)

### API Endpoints

#### Authentication
- `POST /api/Auth/register` - Register new user (public)
- `POST /api/Auth/login` - User login (public)
- `GET /api/Auth/me` - Get current user info (authenticated)

#### Students
- `GET /api/Students` - List students with pagination and search (Admin, Teacher)
- `GET /api/Students/{id}` - Get student by ID (Admin, Teacher, Student)
- `POST /api/Students` - Create student (Admin only)
- `PUT /api/Students/{id}` - Update student (Admin only)
- `DELETE /api/Students/{id}` - Delete student (Admin only)

#### Teachers
- `GET /api/Teachers` - List teachers with pagination and search (Admin, Teacher)
- `GET /api/Teachers/{id}` - Get teacher by ID (Admin, Teacher)
- `POST /api/Teachers` - Create teacher (Admin only)
- `DELETE /api/Teachers/{id}` - Delete teacher (Admin only)

#### Courses
- `GET /api/Courses` - List courses with pagination and search (All authenticated)
- `GET /api/Courses/{id}` - Get course by ID (All authenticated)
- `POST /api/Courses` - Create course (Admin only)
- `DELETE /api/Courses/{id}` - Delete course (Admin only)

## Technology Stack

- **.NET 9.0** - Modern cross-platform framework
- **ASP.NET Core Identity** - User authentication and authorization
- **Entity Framework Core** - ORM with SQL Server and In-Memory providers
- **JWT Bearer** - Token-based authentication
- **Swagger/OpenAPI** - API documentation and testing
- **xUnit** - Unit and integration testing framework
- **BCrypt.Net** - Password hashing
- **FluentValidation** - Advanced input validation

## Getting Started

### Prerequisites
- .NET 9.0 SDK or later
- SQL Server (or SQL Server LocalDB)
- Visual Studio 2022 / VS Code / JetBrains Rider (optional)

### Installation

1. Clone the repository:
```bash
git clone https://github.com/xcolon/school-admin-dashboard.git
cd school-admin-dashboard
```

2. Restore dependencies:
```bash
dotnet restore
```

3. Update connection string in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SchoolAdminDb;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true"
  }
}
```

4. Apply database migrations:
```bash
cd SchoolAdminDashboard
dotnet ef migrations add InitialCreate
dotnet ef database update
```

5. Run the application:
```bash
dotnet run
```

The API will be available at `https://localhost:5001` (or `http://localhost:5000`)

### Using Swagger UI

Navigate to `https://localhost:5001/swagger` to access the interactive API documentation.

## Configuration

### JWT Settings

Configure JWT settings in `appsettings.json`:
```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong",
    "Issuer": "SchoolAdminDashboard",
    "Audience": "SchoolAdminDashboardUsers",
    "ExpirationMinutes": "60"
  }
}
```

**Important**: Change the `SecretKey` in production to a strong, unique value.

### CORS Settings

Configure allowed origins in `appsettings.json`:
```json
{
  "AllowedOrigins": [
    "http://localhost:3000",
    "http://localhost:4200"
  ]
}
```

## Testing

The project includes 32 comprehensive security tests covering:
- Authentication security
- Authorization and RBAC
- Input validation
- SQL injection prevention

### Run Tests
```bash
dotnet test
```

### Run Tests with Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## API Usage Examples

### Register a New User
```bash
curl -X POST https://localhost:5001/api/Auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.doe@example.com",
    "password": "SecureP@ssw0rd123",
    "confirmPassword": "SecureP@ssw0rd123",
    "role": "Admin"
  }'
```

### Login
```bash
curl -X POST https://localhost:5001/api/Auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john.doe@example.com",
    "password": "SecureP@ssw0rd123"
  }'
```

Response:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "email": "john.doe@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "role": "Admin",
  "expiresAt": "2024-01-01T12:00:00Z"
}
```

### Create a Student (Requires Admin Role)
```bash
curl -X POST https://localhost:5001/api/Students \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "firstName": "Jane",
    "lastName": "Smith",
    "email": "jane.smith@example.com",
    "studentId": "STU001",
    "dateOfBirth": "2005-01-15",
    "grade": 10,
    "phoneNumber": "555-1234",
    "address": "123 Main St"
  }'
```

## Security Best Practices

1. **Strong Passwords**: Passwords must be at least 8 characters with uppercase, lowercase, digits, and special characters
2. **Token Security**: Store JWT tokens securely (HttpOnly cookies or secure storage)
3. **HTTPS Only**: Always use HTTPS in production
4. **Secret Management**: Use environment variables or Azure Key Vault for secrets
5. **Regular Updates**: Keep dependencies up to date
6. **Rate Limiting**: Consider adding rate limiting middleware in production

## Database Models

### User
- Identity-based user with roles
- Tracks last login timestamp
- Lockout support for security

### Student
- Student information with enrollment tracking
- Unique student ID and email
- Grade level and contact information

### Teacher
- Teacher information with department
- Employee ID and specialization
- Course assignments

### Course
- Course details with teacher assignment
- Semester and year tracking
- Enrollment capacity management

### Enrollment
- Many-to-many relationship between Students and Courses
- Grade tracking
- Status management

## Project Structure

```
SchoolAdminDashboard/
├── Controllers/          # API controllers
│   ├── AuthController.cs
│   ├── StudentsController.cs
│   ├── TeachersController.cs
│   └── CoursesController.cs
├── Models/              # Domain models
│   ├── User.cs
│   ├── Student.cs
│   ├── Teacher.cs
│   ├── Course.cs
│   └── Enrollment.cs
├── DTOs/                # Data Transfer Objects
│   ├── AuthDtos.cs
│   └── StudentDtos.cs
├── Data/                # Database context
│   └── ApplicationDbContext.cs
├── Services/            # Business services
│   └── TokenService.cs
└── Program.cs           # Application entry point

SchoolAdminDashboard.Tests/
├── AuthenticationSecurityTests.cs
├── AuthorizationSecurityTests.cs
├── InputValidationTests.cs
└── SqlInjectionPreventionTests.cs
```

## License

This project is for educational purposes.

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## Support

For issues and questions, please open an issue on GitHub.
