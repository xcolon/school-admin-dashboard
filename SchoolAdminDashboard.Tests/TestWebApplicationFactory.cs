using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace SchoolAdminDashboard.Tests;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("TestDbName", "TestDb_" + Guid.NewGuid().ToString());
    }
}

public class AuthTestFactory : TestWebApplicationFactory { }
public class AuthzTestFactory : TestWebApplicationFactory { }
public class ValidationTestFactory : TestWebApplicationFactory { }
public class SqlInjectionTestFactory : TestWebApplicationFactory { }
