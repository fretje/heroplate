using System.Linq.Expressions;
using Heroplate.Api.Domain.Abstractions.Entities;

namespace Heroplate.Api.Application.Common.Persistence;

// The Repository for the Application Db
// I(Read)RepositoryBase<T> is from Ardalis.Specification

/// <summary>
/// The regular read/write repository for an aggregate root.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IRepository<T> : IRepositoryBase<T>
    where T : class, IAggregateRoot
{
    Task BulkDeleteAsync(Expression<Func<T, bool>> predicate, CancellationToken ct);
}

/// <summary>
/// The read-only repository for an aggregate root.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IReadRepository<T> : IReadRepositoryBase<T>
    where T : class, IAggregateRoot
{
}

/// <summary>
/// A special (read/write) repository for an aggregate root,
/// that also adds EntityCreated, EntityUpdated or EntityDeleted
/// events to the DomainEvents of the entities before adding,
/// updating or deleting them.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IRepositoryWithEvents<T> : IRepositoryBase<T>
    where T : class, IAggregateRoot
{
}