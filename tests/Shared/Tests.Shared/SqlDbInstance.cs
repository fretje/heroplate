using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using MartinCostello.SqlLocalDb;
using Microsoft.Data.SqlClient;

namespace Tests.Shared;

// SqlDbInstance provides a Sql Database Instance.
// It either starts a container with full sql server, or it uses SqlLocalDB where possible (i.e. on Windows).
// You need to call StartAsync before using it.
public sealed class SqlDbInstance : IAsyncDisposable
{
    private readonly bool _forceContainer;

    private readonly MsSqlTestcontainer? _containerDb;
    private readonly SqlLocalDbApi? _localDbApi;
    private TemporarySqlLocalDbInstance? _localDb;

    private bool _hasStarted;

    private bool UseContainers => _forceContainer || !OperatingSystem.IsWindows();

    public string DbName { get; }

    // Set forceContainer to true to use containers, even when the OS is Windows.
    public SqlDbInstance(string? dbName = null, bool forceContainer = false)
    {
        DbName = string.IsNullOrWhiteSpace(dbName) ? $"{Guid.NewGuid():D}" : dbName;
        _forceContainer = forceContainer;

        if (UseContainers)
        {
            using var containerDbConfig = new MsSqlTestcontainerConfiguration("mcr.microsoft.com/mssql/server:2022-latest")
            {
                Database = DbName,
                Password = "$uper$ecretPassword123!"
            };
            _containerDb = new TestcontainersBuilder<MsSqlTestcontainer>()
                .WithDatabase(containerDbConfig)
                .Build();
        }
        else
        {
            _localDbApi = new()
            {
                AutomaticallyDeleteInstanceFiles = true,
                StopOptions = StopInstanceOptions.NoWait,
                StopTimeout = TimeSpan.FromSeconds(1)
            };
        }
    }

    public string ConnectionString =>
        !_hasStarted
            ? throw new InvalidOperationException("The Sql Db Instance hasn't started yet. Call StartAsync first.")
            : UseContainers
                ? _containerDb!.ConnectionString
                : _localDb!.ConnectionString; // SqlDbHelper.GetConnectionString(_localDb!.ConnectionString, DbName);

    public string ConnectionStringWithTrustServerCertificate =>
        new SqlConnectionStringBuilder(ConnectionString) { TrustServerCertificate = true }.ConnectionString;

    public async Task StartAsync()
    {
        if (UseContainers)
        {
            await _containerDb!.StartAsync();
        }
        else
        {
            _localDb = _localDbApi!.CreateTemporaryInstance(deleteFiles: true);
        }

        _hasStarted = true;
    }

    public async ValueTask DisposeAsync()
    {
        if (_localDb is not null)
        {
            // if (await SqlDbHelper.DbExistsAsync(ConnectionString, default))
            // {
            //    await SqlDbHelper.DropAsync(ConnectionString, default);
            // }

            _localDb.Dispose();
            _localDb = null;
        }

        _localDbApi?.Dispose();
        if (_containerDb is not null)
        {
            await _containerDb.DisposeAsync();
        }
    }
}