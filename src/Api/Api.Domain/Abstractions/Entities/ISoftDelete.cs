namespace Heroplate.Api.Domain.Abstractions.Entities;

public interface ISoftDelete
{
    DateTime? DeletedOn { get; set; }
    Guid? DeletedBy { get; set; }
}