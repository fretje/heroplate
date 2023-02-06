using Heroplate.Api.Domain.Abstractions.Entities;

namespace Heroplate.Api.Application.Common.Entities;

public class EntityWithNameRequestValidator<TEntity> : AbstractValidator<IEntityWithNameRequest>
    where TEntity : class, IEntityWithName
{
    public EntityWithNameRequestValidator(IReadRepository<TEntity> entityRepo, IStringLocalizer localizer) =>
        RuleFor(s => s.Name)
            .NotEmpty()
            .MaximumLength(256)
            .MustAsync(async (entityRequest, name, ct) =>
                    await entityRepo.FirstOrDefaultAsync(new EntityByNameSpec<TEntity>(name), ct) is not { } existingEntity
                        || (entityRequest is IUpdateEntityRequest entityToUpdate && existingEntity.Id == entityToUpdate.Id))
                .WithMessage((_, name) => localizer["A {0} with the name '{1}' already exists.", typeof(TEntity).Name, name]);
}