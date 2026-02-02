using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using RememberAll;
using RememberAll.src.Data;

namespace RememberAllBackend.Tests.Fixtures;

public class TestWebFactory(string databaseName = "TestDb") : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove SQLite database configuration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if (descriptor != null)
                services.Remove(descriptor);

            // Replace with in-memory database for fast, isolated tests
            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(databaseName));
        });

        builder.ConfigureLogging(logging => logging.ClearProviders());
    }
}
