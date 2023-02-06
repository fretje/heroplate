namespace Api.Host.Tests;

// All classes derived from HeroplateApiTestsBase are part of
// the same collection so that means they use the same database.
// But the database will be reset between each class run.
[Collection(nameof(HeroplateApiTestsCollection))]
public abstract class HeroplateApiTestsBase : IClassFixture<ResetHeroplateApiDbFixture<HeroApiAppFactoryFixture>>
{
    protected HeroApiAppFactoryFixture AppFactory { get; }
    protected HeroplateApiTestsBase(HeroApiAppFactoryFixture appFactory) => AppFactory = appFactory;

    protected HttpClient AnonymousClient => AppFactory.AnonymousClient;
    protected HttpClient PermissionlessClient => AppFactory.PermissionlessClient;
    protected HttpClient AdminClient => AppFactory.AdminClient;
}