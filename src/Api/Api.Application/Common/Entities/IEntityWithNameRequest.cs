namespace Heroplate.Api.Application.Common.Entities;

public interface IEntityWithNameRequest : IRequest<int>
{
    string Name { get; }
    string? Description { get; }
}