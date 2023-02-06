namespace Heroplate.Admin.Application.Identity.Roles;

public partial class Roles
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthState { get; set; } = default!;

    protected EntityClientTableContext<RoleDto, string?, CreateOrUpdateRoleRequest> Context { get; set; } = default!;

    private bool _canViewRoleClaims;

    protected override async Task OnInitializedAsync()
    {
        var state = await AuthState;
        _canViewRoleClaims = await Authorizer.HasPermissionAsync(state.User, Permissions.RoleClaims.View);

        Context = new(
            fields: new()
            {
                new(role => role.Id, L["Id"]),
                new(role => role.Name, L["Name"]),
                new(role => role.Description, L["Description"])
            },
            loadDataFunc: async () => (await RolesClient.GetListAsync()).ToList(),
            searchFunc: (searchString, role) =>
                string.IsNullOrWhiteSpace(searchString)
                    || role.Name?.Contains(searchString, StringComparison.OrdinalIgnoreCase) == true
                    || role.Description?.Contains(searchString, StringComparison.OrdinalIgnoreCase) == true,
            idFunc: role => role.Id,
            createFunc: RolesClient.RegisterRoleAsync,
            updateFunc: async (_, role) => await RolesClient.RegisterRoleAsync(role),
            deleteFunc: RolesClient.DeleteAsync,
            entityTypeName: L["Role"],
            entityTypeNamePlural: L["Roles"],
            createPermission: Permissions.Roles.Create,
            updatePermission: Permissions.Roles.Update,
            deletePermission: Permissions.Roles.Delete,
            hasExtraActionsFunc: () => _canViewRoleClaims,
            canUpdateEntityFunc: e => !ApplicationRoles.IsDefault(e.Name),
            canDeleteEntityFunc: e => !ApplicationRoles.IsDefault(e.Name));
    }

    private void ManagePermissions(string roleId) =>
        Navigation.NavigateTo($"/roles/{roleId}/permissions");
}