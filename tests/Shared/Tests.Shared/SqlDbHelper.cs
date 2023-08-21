using Microsoft.Data.SqlClient;

namespace Tests.Shared;

public static class SqlDbHelper
{
    private static readonly SqlConstants _sqlConstants = new();

    public static string GetConnectionString(string masterConnectionString, string dbName) =>
        new SqlConnectionStringBuilder(masterConnectionString)
        {
            InitialCatalog = dbName
        }.ConnectionString;

    private static string GetMasterConnectionString(string connectionString) =>
        GetConnectionString(connectionString, "master");

    private static string GetCatalog(string connectionString) =>
        new SqlConnectionStringBuilder(connectionString).InitialCatalog;

    public static string AddTrustServerCertificate(string connectionString) =>
        new SqlConnectionStringBuilder(connectionString)
        {
            TrustServerCertificate = true
        }.ConnectionString;

    public static Task<bool> DbExistsAsync(string connectionString, CancellationToken ct) =>
        DbExistsAsync(GetMasterConnectionString(connectionString), GetCatalog(connectionString), ct);

    private static async Task<bool> DbExistsAsync(string masterConnectionString, string dbName, CancellationToken ct)
    {
        try
        {
            int count = await ExecuteScalarAsync<int>(
                masterConnectionString,
                "SELECT COUNT(*) FROM sys.databases WHERE Name = @name",
                new[] { new SqlParameter("name", dbName) },
                ct).ConfigureAwait(false);
            return count > 0;
        }
        catch (SqlException)
        {
            return false;
        }
    }

    private static string Quoted(string value) => "'" + value.Replace("'", "''") + "'";

    public static async Task<string> CreateAsync(string masterConnectionString, string dbName, bool caseSensitive, CancellationToken ct)
    {
        string sql = $"CREATE DATABASE {_sqlConstants.EscapedName(dbName)}";
        if (caseSensitive)
        {
            sql += " Collate Latin1_General_CS_AS";
        }

        if (!await IsAzureAsync(masterConnectionString, ct).ConfigureAwait(false))
        {
            sql += $"; ALTER DATABASE {_sqlConstants.EscapedName(dbName)} SET RECOVERY SIMPLE";
        }

        // on azure creating a db can take a while ==> timeout of 3 minutes
        await ExecuteNonQueryAsync(masterConnectionString, sql, 60 * 3, ct).ConfigureAwait(false);

        return GetConnectionString(masterConnectionString, dbName);
    }

    public static Task CreateAsync(string connectionString, CancellationToken ct) =>
        CreateAsync(connectionString, false, ct);

    public static Task CreateAsync(string connectionString, bool caseSensitive, CancellationToken ct)
    {
        string masterConnectionString = GetMasterConnectionString(connectionString);
        string dbName = GetCatalog(connectionString);
        return string.IsNullOrWhiteSpace(dbName)
            ? throw new ApplicationException("No DbName provided.")
            : CreateAsync(masterConnectionString, dbName, caseSensitive, ct);
    }

    public static Task DropAsync(string connectionString, CancellationToken ct) =>
        DropAsync(GetMasterConnectionString(connectionString), GetCatalog(connectionString), ct);

    public static async Task DropAsync(string masterConnectionString, string dbName, CancellationToken ct)
    {
        _ = dbName ?? throw new ArgumentNullException(nameof(dbName));

        string escapedDbName = _sqlConstants.EscapedName(dbName);
        string? sql = default;
        if (!await IsAzureAsync(masterConnectionString, ct).ConfigureAwait(false))
        {
            sql += $"EXEC msdb.dbo.sp_delete_database_backuphistory @database_name = N{Quoted(dbName)};" +
                   $"ALTER DATABASE {escapedDbName} SET SINGLE_USER WITH ROLLBACK IMMEDIATE;";
        }

        sql += $"DROP DATABASE {escapedDbName}";
        await ExecuteNonQueryAsync(masterConnectionString, sql, ct).ConfigureAwait(false);
    }

    private static async Task<bool> IsAzureAsync(string connectionString, CancellationToken ct) =>
        await ExecuteScalarAsync<string>(connectionString, "SELECT SERVERPROPERTY('Edition')", ct).ConfigureAwait(false)
            == "SQL Azure";

    public static Task<int> ExecuteNonQueryAsync(string connectionString, string sql, CancellationToken ct) =>
        ExecuteNonQueryAsync(connectionString, sql, null, ct);

    public static async Task<int> ExecuteNonQueryAsync(string connectionString, string sql, int? commandTimeout, CancellationToken ct)
    {
        _ = sql ?? throw new ArgumentNullException(nameof(sql));

        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct).ConfigureAwait(false);
        int returnValue = 0;
        foreach (string query in sql.Split(new string[] { "\r\nGO\r\n", "\nGO\n" }, StringSplitOptions.RemoveEmptyEntries))
        {
            if (query is not "\r\n" and not "\n")
            {
                using var command = connection.CreateCommand();
                command.CommandText = query;
                if (commandTimeout.HasValue)
                {
                    command.CommandTimeout = commandTimeout.Value;
                }

                returnValue += await command.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            }
        }

        return returnValue;
    }

    public static Task<T?> ExecuteScalarAsync<T>(string connectionString, string sql, CancellationToken ct) =>
        ExecuteScalarAsync<T>(connectionString, sql, null, ct);

    public static async Task<T?> ExecuteScalarAsync<T>(string connectionString, string sql, SqlParameter[]? parameters, CancellationToken ct) =>
        await ExecuteScalarAsync(connectionString, sql, parameters, ct).ConfigureAwait(false)
            is T scalar ? scalar : default;

    public static async Task<object?> ExecuteScalarAsync(string connectionString, string sql, SqlParameter[]? parameters, CancellationToken ct)
    {
        using var connection = new SqlConnection(connectionString);
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        if (parameters is not null)
        {
            command.Parameters.AddRange(parameters);
        }

        await connection.OpenAsync(ct).ConfigureAwait(false);
        object scalar = await command.ExecuteScalarAsync(ct).ConfigureAwait(false);
        return scalar is DBNull ? null : scalar;
    }
}