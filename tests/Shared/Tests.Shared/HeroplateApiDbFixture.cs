using System.Data.Common;
using System.Dynamic;
using System.Net;
using Heroplate.Api.Contracts.Multitenancy;
using Heroplate.Shared.Authorization;
using Microsoft.Data.SqlClient;
using Respawn;
using Xunit;

namespace Tests.Shared;

public abstract class HeroplateApiDbFixture : IAsyncLifetime
{
    private DbConnection? _dbConnection;
    private Respawner? _respawner;

    protected HeroplateApiDbFixture(string? dbName = null, bool forceContainer = false) =>
        ApiDb = new(dbName, forceContainer);

    public SqlDbInstance ApiDb { get; }

    public HttpClient AnonymousClient { get; private set; } = default!;
    public HttpClient PermissionlessClient { get; private set; } = default!;
    public HttpClient AdminClient { get; private set; } = default!;

    public Task ResetDatabaseAsync() =>
        _respawner is null || _dbConnection is null
            ? Task.CompletedTask
            : _respawner.ResetAsync(_dbConnection);

    public virtual async Task InitializeAsync()
    {
        await ApiDb.StartAsync();

        InitializeClients();

        // We have to initialize the respawner after the WebApplicationFactory has been
        // initialized (this happens when CreateClient is called the first time)
        await InitializeRespawnerAsync();
    }

    protected void InitializeClients()
    {
        AnonymousClient = CreateClient();
        PermissionlessClient = CreateClientWithPermissions();
        AdminClient = CreateClientWithPermissions(PermissionProvider.All);
    }

    protected async Task InitializeRespawnerAsync()
    {
        _dbConnection = new SqlConnection(ApiDb.ConnectionStringWithTrustServerCertificate);
        await _dbConnection.OpenAsync();
        _respawner = await Respawner.CreateAsync(_dbConnection, new()
        {
            SchemasToInclude = new[] { "Heroplate" },
            WithReseed = true
        });
    }

    public virtual async Task DisposeAsync()
    {
        AdminClient?.Dispose();
        PermissionlessClient?.Dispose();
        AnonymousClient?.Dispose();
        _dbConnection?.Dispose();
        await ApiDb.DisposeAsync();
    }

    protected abstract HttpClient CreateClient();

    internal HttpClient CreateClientWithPermissions(params string[] permissions) =>
        CreateClient()
            .SetFakeBearerToken(CreateToken(permissions));

    protected static object CreateToken(string[] permissions)
    {
        dynamic data = new ExpandoObject();
        data.tenant = MultitenancyConstants.Root.Id;
        data.sub = MultitenancyConstants.Root.AdminUserId;
        data.email = MultitenancyConstants.Root.EmailAddress;
        data.permission = permissions;
        return data;
    }
}