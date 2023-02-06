namespace Heroplate.Api.Domain.Identity;

public abstract class ApplicationUserEvent : DomainEvent
{
    public string UserId { get; set; } = default!;

    protected ApplicationUserEvent(string userId) => UserId = userId;
}

public class ApplicationUserCreatedEvent : ApplicationUserEvent
{
    public ApplicationUserCreatedEvent(string userId)
        : base(userId)
    {
    }
}

public class ApplicationUserUpdatedEvent : ApplicationUserEvent
{
    public string? ObjectId { get; set; }
    public bool RolesUpdated { get; set; }

    public ApplicationUserUpdatedEvent(string userId, string? objectId, bool rolesUpdated = false)
        : base(userId) =>
        (ObjectId, RolesUpdated) = (objectId, rolesUpdated);
}