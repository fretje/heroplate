using System.Diagnostics.CodeAnalysis;
using Heroplate.Api.Host.Controllers;
using Heroplate.Api.Infrastructure.BackgroundJobs;
using Heroplate.Api.Infrastructure.Persistence;
using Heroplate.Shared.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Host.Tests;

[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "Disposal is handled by Dependency Injection")]
public sealed class HeroApiAppFactoryFixture : HeroplateApiDbFixture
{
    private readonly HeroApiAppFactory _appFactory;
    public HeroApiAppFactoryFixture()
        : base(forceContainer: false) =>
        _appFactory = new(ApiDb);

    public Task<NotificationsClient> StartNotificationsClientAsync() =>
        NotificationsClient.StartAsync(_appFactory.Server, CreateToken(PermissionProvider.All));

    public override async Task DisposeAsync()
    {
        await _appFactory.DisposeAsync();
        await base.DisposeAsync();
    }

    protected override HttpClient CreateClient() => _appFactory.CreateClient();
}

internal sealed class HeroApiAppFactory : WebApplicationFactory<BaseApiController>
{
    private readonly SqlDbInstance _apiDb;
    public HeroApiAppFactory(SqlDbInstance apiDb) => _apiDb = apiDb;

    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder
            .UseEnvironment("testing")
            .ConfigureTestServices(services =>
            {
                // Replace database connectionstring
                services.Configure<DatabaseSettings>(settings => settings.ConnectionString = _apiDb.ConnectionStringWithTrustServerCertificate);
                services.Configure<HangfireStorageSettings>(settings => settings.ConnectionString = _apiDb.ConnectionString);
            });
}