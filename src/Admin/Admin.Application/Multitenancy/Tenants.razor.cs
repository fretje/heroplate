using Mapster;

namespace Heroplate.Admin.Application.Multitenancy;

public partial class Tenants
{
    protected EntityClientTableContext<TenantDetail, Guid, CreateTenantRequest> Context { get; set; } = default!;
    private EntityTable<TenantDetail, Guid, CreateTenantRequest>? EntityTable { get; set; }

    private List<TenantDetail> _tenants = new();
    private string? _searchString;

    protected override void OnInitialized() =>
        Context = new(
            fields: new()
            {
                new(tenant => tenant.Id, L["Id"]),
                new(tenant => tenant.Name, L["Name"]),
                new(tenant => tenant.AdminEmail, L["Admin Email"]),
                new(tenant => tenant.ValidUpto.ToString("MMM dd, yyyy", CultureInfo.CurrentCulture), L["Valid Upto"]),
                new(tenant => tenant.IsActive, L["Active"], Type: typeof(bool))
            },
            loadDataFunc: async () => _tenants = (await TenantsClient.GetListAsync()).Adapt<List<TenantDetail>>(),
            searchFunc: (searchString, tenantDto) =>
                string.IsNullOrWhiteSpace(searchString)
                    || tenantDto.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase),
            createFunc: tenant => TenantsClient.CreateAsync(tenant.Adapt<CreateTenantRequest>()),
            entityTypeName: L["Tenant"],
            entityTypeNamePlural: L["Tenants"],
            createPermission: Permissions.Tenants.Create,
            hasExtraActionsFunc: () => true);

    private void ViewTenantDetails(string id)
    {
        var tenant = _tenants.First(f => f.Id == id);
        tenant.ShowDetails = !tenant.ShowDetails;
        foreach (var otherTenants in _tenants.Except(new[] { tenant }))
        {
            otherTenants.ShowDetails = false;
        }
    }

    private async Task ViewUpgradeSubscriptionModalAsync(string id)
    {
        var tenant = _tenants.First(f => f.Id == id);
        var parameters = new DialogParameters
        {
            {
                nameof(UpgradeSubscriptionModal.Request),
                new UpgradeSubscriptionRequest
                {
                    TenantId = tenant.Id,
                    ExtendedExpiryDate = tenant.ValidUpto
                }
            }
        };
        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true, DisableBackdropClick = true };
        var dialog = await DialogService.ShowAsync<UpgradeSubscriptionModal>(L["Upgrade Subscription"], parameters, options);
        var result = await dialog.Result;
        if (!result.Canceled && EntityTable is not null)
        {
            await EntityTable.RefreshDataAsync();
        }
    }

    private async Task DeactivateTenantAsync(string id)
    {
        if (await ApiHelper.ExecuteCallGuardedAsync(
            () => TenantsClient.DeactivateAsync(id),
            Snackbar,
            null,
            L["Tenant Deactivated."]) is not null && EntityTable is not null)
        {
            await EntityTable.RefreshDataAsync();
        }
    }

    private async Task ActivateTenantAsync(string id)
    {
        if (await ApiHelper.ExecuteCallGuardedAsync(
            () => TenantsClient.ActivateAsync(id),
            Snackbar,
            null,
            L["Tenant Activated."]) is not null && EntityTable is not null)
        {
            await EntityTable.RefreshDataAsync();
        }
    }

    public class TenantDetail : TenantDto
    {
        public bool ShowDetails { get; set; }
    }
}