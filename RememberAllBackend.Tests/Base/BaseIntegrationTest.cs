using RememberAllBackend.Tests.Fixtures;

namespace RememberAllBackend.Tests.Base;

public abstract class BaseIntegrationTest : IAsyncLifetime, IClassFixture<IndividualDatabaseFixture>
{
    protected readonly IndividualDatabaseFixture Fixture;
    protected HttpClient Client { get; private set; } = null!;

    protected BaseIntegrationTest(IndividualDatabaseFixture fixture)
    {
        Fixture = fixture;
    }

    public virtual async Task InitializeAsync()
    {
        // Reset database before each test method
        await Fixture.ResetDatabaseAsync();
        Client = Fixture.Factory.CreateClient();
    }

    public virtual async Task DisposeAsync()
    {
        // Cleanup after each test method (can be extended by derived classes)
        await Task.CompletedTask;
    }
}
