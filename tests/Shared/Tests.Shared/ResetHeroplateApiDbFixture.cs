using Xunit;

namespace Tests.Shared;

// Adding this as a classfixture makes sure the database is only reset after all tests in a class have run.
// Otherwise deriving the test class from this will make sure the database is reset after each test.
public class ResetHeroplateApiDbFixture<TDbFixture> : IAsyncLifetime
    where TDbFixture : HeroplateApiDbFixture
{
    private readonly TDbFixture _dbFixture;
    public ResetHeroplateApiDbFixture(TDbFixture factory) => _dbFixture = factory;

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _dbFixture.ResetDatabaseAsync();
}