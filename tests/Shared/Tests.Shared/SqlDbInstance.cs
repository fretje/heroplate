using MartinCostello.SqlLocalDb;
using Testcontainers.MsSql;

namespace Tests.Shared;

// SqlDbInstance provides a Sql Database Instance.
// It either starts a container with full sql server, or it uses SqlLocalDB where possible (i.e. on Windows).
// You need to call StartAsync before using it.
public sealed class SqlDbInstance : IAsyncDisposable
{
    private readonly bool _forceContainer;

    private readonly MsSqlContainer? _containerDb;
    private readonly SqlLocalDbApi? _localDbApi;
    private TemporarySqlLocalDbInstance? _localDb;
    private string? _connectionString;

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
            _containerDb = new MsSqlBuilder()
                .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
                .WithPassword("$uper$ecretPassword123")
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
            : _connectionString!;

    public string ConnectionStringWithTrustServerCertificate =>
        SqlDbHelper.AddTrustServerCertificate(ConnectionString);

    public async Task StartAsync()
    {
        if (UseContainers)
        {
            await _containerDb!.StartAsync();

            // Create a new db with the actual DbName
            _connectionString = SqlDbHelper.GetConnectionString(_containerDb.GetConnectionString(), DbName);
            await SqlDbHelper.CreateAsync(_connectionString, default);
        }
        else
        {
            _localDb = _localDbApi!.CreateTemporaryInstance(deleteFiles: true);
            _connectionString = SqlDbHelper.GetConnectionString(_localDb.ConnectionString, DbName);
        }

        _hasStarted = true;
    }

    public async ValueTask DisposeAsync()
    {
        if (_localDb is not null)
        {
            if (await SqlDbHelper.DbExistsAsync(ConnectionString, default))
            {
                await SqlDbHelper.DropAsync(ConnectionString, default);
            }

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