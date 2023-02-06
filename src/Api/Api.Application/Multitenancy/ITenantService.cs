namespace Heroplate.Api.Application.Multitenancy;

public interface ITenantService
{
    Task<List<TenantDto>> GetAllAsync();
    Task<bool> ExistsWithIdAsync(string id);
    Task<bool> ExistsWithNameAsync(string name);
    Task<TenantDto> GetByIdAsync(string id);
    Task<string> CreateAsync(CreateTenantRequest req, CancellationToken ct);
    Task<string> ActivateAsync(string id, CancellationToken ct);
    Task<string> DeactivateAsync(string id, CancellationToken ct);
    Task<string> UpdateSubscriptionAsync(string id, DateTime extendedExpiryDate, CancellationToken ct);
}