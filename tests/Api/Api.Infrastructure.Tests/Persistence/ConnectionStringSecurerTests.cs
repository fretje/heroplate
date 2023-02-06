using Heroplate.Api.Application.Common.Persistence;
using Xunit;

namespace Api.Infrastructure.Tests.Persistence;

public class ConnectionStringSecurerTests
{
    private const string Mssql = "Data Source=(localdb)\\mssqllocaldb;Initial Catalog=fullStackHeroDb;MultipleActiveResultSets=True;";

    private readonly IConnectionStringSecurer _connectionStringSecurer;

    public ConnectionStringSecurerTests(IConnectionStringSecurer connectionStringSecurer) => _connectionStringSecurer = connectionStringSecurer;

    [Theory]
    [InlineData(Mssql + ";Integrated Security=True;", "mssql", false, "MSSQL: CASE 1 - Integrated Security")]
    [InlineData(Mssql + ";user id=root;password=12345;", "mssql", true, "MSSQL: CASE 2 - Credentials")]
    public void MakeSecureTest(string connectionString, string dbProvider, bool containsCredentials, string name)
    {
        string? res1 = _connectionStringSecurer.MakeSecure(connectionString, dbProvider);
        string? check1 = _connectionStringSecurer.MakeSecure(res1, dbProvider);

        Assert.True(check1?.Equals(res1, StringComparison.OrdinalIgnoreCase), name); // don't know what this is actually testing?

        Assert.DoesNotContain("12345", check1);
        Assert.DoesNotContain("root", check1);

        if (containsCredentials)
        {
            Assert.Contains("*******", check1);
        }
    }
}