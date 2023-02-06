namespace Heroplate.Admin.Application.Identity.Users;

public partial class UserRoles
{
    [Parameter]
    public string? Id { get; set; }
    [CascadingParameter]
    protected Task<AuthenticationState> AuthState { get; set; } = default!;

    private List<UserRoleDto> _userRolesList = default!;

    private string _title = "";
    private string _description = "";

    private string _searchString = "";

    private bool _canEditUsers;
    private bool _canSearchRoles;
    private bool _loaded;

    protected override async Task OnInitializedAsync()
    {
        var state = await AuthState;
        _canEditUsers = await Authorizer.HasPermissionAsync(state.User, Permissions.Users.Update);
        _canSearchRoles = await Authorizer.HasPermissionAsync(state.User, Permissions.UserRoles.View);

        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        _loaded = false;

        if (await ApiHelper.ExecuteCallGuardedAsync(
                () => UsersClient.GetByIdAsync(Id), Snackbar) is { } user)
        {
            _title = $"{user.FirstName} {user.LastName}";
            _description = L["Manage {0} {1}'s Roles", user.FirstName ?? "", user.LastName ?? ""];

            if (await ApiHelper.ExecuteCallGuardedAsync(
                    () => UsersClient.GetRolesAsync(user.Id.ToString()), Snackbar)
                is ICollection<UserRoleDto> response)
            {
                _userRolesList = response.ToList();
            }
        }

        _loaded = true;
    }

    private async Task SaveAsync()
    {
        var request = new UserRolesRequest()
        {
            UserRoles = _userRolesList
        };

        if (await ApiHelper.ExecuteCallGuardedAsync(
                () => UsersClient.AssignRolesAsync(Id, request),
                Snackbar,
                successMessage: L["Updated User Roles."])
            is not null)
        {
            Navigation.NavigateTo("/users");
        }
    }

    private bool Search(UserRoleDto userRole) =>
        string.IsNullOrWhiteSpace(_searchString)
            || userRole.RoleName?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) is true;
}