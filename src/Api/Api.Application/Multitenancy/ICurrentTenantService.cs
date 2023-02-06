namespace Heroplate.Api.Application.Multitenancy;

public interface ICurrentTenantService
{
    string Id { get; }
    string Identifier { get; }
}