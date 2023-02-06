using System.Diagnostics.CodeAnalysis;
using Heroplate.Api.Contracts.Multitenancy;
using Mapster;

namespace Heroplate.Admin.Application.Identity.Roles;

public partial class RolePermissions
{
    [Parameter]
    public string Id { get; set; } = default!; // from route
    [CascadingParameter]
    protected Task<AuthenticationState> AuthState { get; set; } = default!;

    private Dictionary<string, List<PermissionViewModel>> _groupedRoleClaims = default!;

    private string _title = "";
    private string _description = "";

    private string _searchString = "";

    private bool _canSearchRoleClaims;
    private bool _canEditRoleClaims;
    private bool _isRootTenant;
    private bool _loaded;

    static RolePermissions() => TypeAdapterConfig<PermissionDto, PermissionViewModel>.NewConfig().MapToConstructor(true);

    protected override async Task OnInitializedAsync()
    {
        var state = await AuthState;
        _canSearchRoleClaims = await Authorizer.HasPermissionAsync(state.User, Permissions.RoleClaims.View);
        _canEditRoleClaims = await Authorizer.HasPermissionAsync(state.User, Permissions.RoleClaims.Update);
        _isRootTenant = state.User.GetTenant() == MultitenancyConstants.Root.Id;

        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        _loaded = false;

        if (await ApiHelper.ExecuteCallGuardedAsync(
                () => RolesClient.GetByIdWithPermissionsAsync(Id), Snackbar)
            is RoleDto role && role.Permissions is not null)
        {
            _title = L["{0} Permissions", role.Name];
            _description = L["Manage {0} Role Permissions", role.Name];

            var permissions = _isRootTenant
                ? PermissionProvider.AllPermissions
                : PermissionProvider.AdminPermissions;

            _groupedRoleClaims = permissions
                .GroupBy(p => p.Resource)
                .ToDictionary(g => g.Key, g => g.Select(p =>
                {
                    var permission = p.Adapt<PermissionViewModel>();
                    permission.Enabled = role.Permissions.Contains(permission.Name);
                    return permission;
                }).ToList());
        }

        _loaded = true;
    }

    private static Color GetGroupBadgeColor(int selected, int all) =>
        selected == 0 ? Color.Error : selected == all ? Color.Success : Color.Info;

    private async Task SaveAsync()
    {
        var allPermissions = _groupedRoleClaims.Values.SelectMany(a => a);
        var selectedPermissions = allPermissions.Where(a => a.Enabled);
        var request = new UpdateRolePermissionsRequest()
        {
            RoleId = Id,
            Permissions = selectedPermissions.Where(x => x.Enabled).Select(x => x.Name).ToList(),
        };

        if (await ApiHelper.ExecuteCallGuardedAsync(
                () => RolesClient.UpdatePermissionsAsync(request.RoleId, request),
                Snackbar,
                successMessage: L["Updated Permissions."])
            is not null)
        {
            Navigation.NavigateTo("/roles");
        }
    }

    private bool Search(PermissionViewModel permission) =>
        string.IsNullOrWhiteSpace(_searchString)
            || permission.Name.Contains(_searchString, StringComparison.OrdinalIgnoreCase)
            || permission.Description.Contains(_searchString, StringComparison.OrdinalIgnoreCase);
}

public sealed record PermissionViewModel : PermissionDto
{
    public bool Enabled { get; set; }

    public PermissionViewModel(string name, string description, bool isBasic = false, bool isRoot = false)
        : base(name, description, isBasic, isRoot)
    {
    }

    // Don't take "enabled" into account when comparing PermissionViewModels
    public bool Equals(PermissionViewModel? other) => base.Equals(other);

    [SuppressMessage("Roslynator", "RCS1132:Remove redundant overriding member.", Justification = "GetHashCode is here to prevent CS8851 warning on the line above.")]
    [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "The suppression is actually necessary.")]
    public override int GetHashCode() => base.GetHashCode();
}