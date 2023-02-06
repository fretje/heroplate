using System.Linq.Expressions;
using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Heroplate.Api.Application.Common.Persistence;
using Heroplate.Api.Domain.Abstractions.Entities;
using Heroplate.Api.Infrastructure.Persistence.Context;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Heroplate.Api.Infrastructure.Persistence.Repository;

// Inherited from Ardalis.Specification's RepositoryBase<T>
public class ApplicationDbRepository<T> : RepositoryBase<T>, IReadRepository<T>, IRepository<T>
    where T : class, IAggregateRoot
{
    private readonly ApplicationDbContext _dbContext;
    public ApplicationDbRepository(ApplicationDbContext dbContext)
        : base(dbContext) =>
        _dbContext = dbContext;

    // We override the default behavior when mapping to a dto.
    // We're using Mapster's ProjectToType here to immediately map the result from the database.
    // This is only done when no Selector is defined, so regular specifications with a selector also still work.
    protected override IQueryable<TResult> ApplySpecification<TResult>(ISpecification<T, TResult> specification) =>
        specification.Selector is not null
            ? base.ApplySpecification(specification)
            : ApplySpecification(specification, false)
                .ProjectToType<TResult>();

    /// <summary>
    /// Don't forget to take the tenant into account.
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="ct"></param>
    public Task BulkDeleteAsync(Expression<Func<T, bool>> predicate, CancellationToken ct) =>
        _dbContext.Set<T>().Where(predicate).ExecuteDeleteAsync(ct);
}