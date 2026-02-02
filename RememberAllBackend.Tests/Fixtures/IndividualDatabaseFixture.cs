using RememberAll.src.Data;

namespace RememberAllBackend.Tests.Fixtures;

public class IndividualDatabaseFixture : IAsyncLifetime
{
    private readonly string databaseId = Guid.NewGuid().ToString();
    public TestWebFactory Factory { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        Factory = new TestWebFactory($"TestDb_{databaseId}");

        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await dbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        if (Factory == null)
            return;

        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await dbContext.Database.EnsureDeletedAsync();
        await Factory.DisposeAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Delete all data from all tables
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
    }
}
