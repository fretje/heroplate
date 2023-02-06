using System.Data;
using Dapper;
using Finbuckle.MultiTenant.EntityFrameworkCore;
using Heroplate.Api.Application.Common.Exceptions;
using Heroplate.Api.Application.Common.Persistence;
using Heroplate.Api.Domain.Abstractions.Entities;
using Heroplate.Api.Infrastructure.Persistence.Context;

namespace Heroplate.Api.Infrastructure.Persistence.Repository;

public class DapperRepository : IDapperRepository
{
    private readonly ApplicationDbContext _dbContext;

    public DapperRepository(ApplicationDbContext dbContext) => _dbContext = dbContext;

    public async Task<IReadOnlyList<T>> QueryAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, CancellationToken ct = default)
    where T : class, IEntity =>
        (await _dbContext.Connection.QueryAsync<T>(sql, param, transaction))
            .AsList();

    public async Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, CancellationToken ct = default)
    where T : class, IEntity
    {
        if (!_dbContext.Model.GetMultiTenantEntityTypes().Any(t => t.ClrType == typeof(T)))
        {
            sql = sql.Replace("@tenant", _dbContext.TenantInfo.Id);
        }

        var entity = await _dbContext.Connection.QueryFirstOrDefaultAsync<T>(sql, param, transaction);

        return entity ?? throw new NotFoundException();
    }

    public Task<T> QuerySingleAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, CancellationToken ct = default)
    where T : class, IEntity
    {
        if (!_dbContext.Model.GetMultiTenantEntityTypes().Any(t => t.ClrType == typeof(T)))
        {
            sql = sql.Replace("@tenant", _dbContext.TenantInfo.Id);
        }

        return _dbContext.Connection.QuerySingleAsync<T>(sql, param, transaction);
    }
}