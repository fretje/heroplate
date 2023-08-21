using Ardalis.Specification;
using Heroplate.Api.Application.Common.Persistence;
using Heroplate.Api.Domain.Abstractions.Entities;
using Heroplate.Api.Domain.Abstractions.Events;

namespace Heroplate.Api.Infrastructure.Persistence.Repository;

/// <summary>
/// The repository that implements IRepositoryWithEvents.
/// Implemented as a decorator. It only augments the Add,
/// Update and Delete calls where it adds the respective
/// EntityCreated, EntityUpdated or EntityDeleted event
/// before delegating to the decorated repository.
/// </summary>
/// <typeparam name="T"></typeparam>
public class EventAddingRepositoryDecorator<T> : IRepositoryWithEvents<T>
    where T : class, IAggregateRoot
{
    private readonly IRepository<T> _decorated;

    public EventAddingRepositoryDecorator(IRepository<T> decorated) => _decorated = decorated;

    public Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        entity.DomainEvents.Add(EntityCreatedEvent.WithEntity(entity));
        return _decorated.AddAsync(entity, ct);
    }

    public Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
    {
        foreach (var entity in entities)
        {
            entity.DomainEvents.Add(EntityCreatedEvent.WithEntity(entity));
        }

        return _decorated.AddRangeAsync(entities, ct);
    }

    public Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        entity.DomainEvents.Add(EntityUpdatedEvent.WithEntity(entity));
        return _decorated.UpdateAsync(entity, ct);
    }

    public Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
    {
        foreach (var entity in entities)
        {
            entity.DomainEvents.Add(EntityUpdatedEvent.WithEntity(entity));
        }

        return _decorated.UpdateRangeAsync(entities, ct);
    }

    public Task DeleteAsync(T entity, CancellationToken ct = default)
    {
        entity.DomainEvents.Add(EntityDeletedEvent.WithEntity(entity));
        return _decorated.DeleteAsync(entity, ct);
    }

    public Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
    {
        foreach (var entity in entities)
        {
            entity.DomainEvents.Add(EntityDeletedEvent.WithEntity(entity));
        }

        return _decorated.DeleteRangeAsync(entities, ct);
    }

    // The rest of the methods are simply forwarded.
    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        _decorated.SaveChangesAsync(ct);
    public Task<T?> GetByIdAsync<TId>(TId id, CancellationToken ct = default)
        where TId : notnull =>
        _decorated.GetByIdAsync(id, ct);
    [Obsolete("The method is obsolete in Ardalis.Specification")]
    public Task<T?> GetBySpecAsync(ISpecification<T> specification, CancellationToken ct = default) =>
        throw new NotImplementedException();
    [Obsolete("The method is obsolete in Ardalis.Specification")]
    public Task<TResult?> GetBySpecAsync<TResult>(ISpecification<T, TResult> specification, CancellationToken ct = default) =>
        throw new NotImplementedException();
    public Task<T?> FirstOrDefaultAsync(ISpecification<T> specification, CancellationToken ct = default) =>
        _decorated.FirstOrDefaultAsync(specification, ct);
    public Task<TResult?> FirstOrDefaultAsync<TResult>(ISpecification<T, TResult> specification, CancellationToken ct = default) =>
        _decorated.FirstOrDefaultAsync(specification, ct);
    public Task<T?> SingleOrDefaultAsync(ISingleResultSpecification<T> specification, CancellationToken ct = default) =>
        _decorated.SingleOrDefaultAsync(specification, ct);
    public Task<TResult?> SingleOrDefaultAsync<TResult>(ISingleResultSpecification<T, TResult> specification, CancellationToken ct = default) =>
        _decorated.SingleOrDefaultAsync(specification, ct);
    public Task<List<T>> ListAsync(CancellationToken ct = default) =>
        _decorated.ListAsync(ct);
    public Task<List<T>> ListAsync(ISpecification<T> specification, CancellationToken ct = default) =>
        _decorated.ListAsync(specification, ct);
    public Task<List<TResult>> ListAsync<TResult>(ISpecification<T, TResult> specification, CancellationToken ct = default) =>
        _decorated.ListAsync(specification, ct);
    public Task<bool> AnyAsync(ISpecification<T> specification, CancellationToken ct = default) =>
        _decorated.AnyAsync(specification, ct);
    public Task<bool> AnyAsync(CancellationToken ct = default) =>
        _decorated.AnyAsync(ct);
    public Task<int> CountAsync(ISpecification<T> specification, CancellationToken ct = default) =>
        _decorated.CountAsync(specification, ct);
    public Task<int> CountAsync(CancellationToken ct = default) =>
        _decorated.CountAsync(ct);
    public IAsyncEnumerable<T> AsAsyncEnumerable(ISpecification<T> specification) =>
        _decorated.AsAsyncEnumerable(specification);
}