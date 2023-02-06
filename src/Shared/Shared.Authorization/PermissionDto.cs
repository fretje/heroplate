namespace Heroplate.Shared.Authorization;

public record PermissionDto(string Name, string Description, bool IsBasic = false, bool IsRoot = false)
{
    public string Resource => Permissions.ResourceFor(Name);
}