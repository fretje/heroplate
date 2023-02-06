using Heroplate.Api.Domain.Abstractions.Entities;

namespace Heroplate.Api.Application.Common.Auditing;

public class AuditableEntitiesByCreatedOnBetweenSpec<T> : Specification<T>
    where T : IAuditableEntity
{
    public AuditableEntitiesByCreatedOnBetweenSpec(DateTime from, DateTime until) =>
        Query.Where(e => e.CreatedOn >= from && e.CreatedOn <= until);
}