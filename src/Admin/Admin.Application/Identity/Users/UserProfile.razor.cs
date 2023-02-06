namespace Heroplate.Admin.Application.Identity.Users;

public partial class UserProfile
{
    [Parameter]
    public string? Id { get; set; }
    [Parameter]
    public string? Title { get; set; }
    [Parameter]
    public string? Description { get; set; }

    private bool _active;
    private bool _emailConfirmed;
    private char _firstLetterOfName;
    private string? _firstName;
    private string? _lastName;
    private string? _phoneNumber;
    private string? _email;
    private string? _imageUrl;
    private bool _loaded;

    private async Task ToggleUserStatusAsync()
    {
        var request = new ToggleUserStatusRequest { ActivateUser = _active, UserId = Id };
        await ApiHelper.ExecuteCallGuardedAsync(() => UsersClient.ToggleStatusAsync(Id, request), Snackbar);
        Navigation.NavigateTo("/users");
    }

    [Parameter]
    public string? ImageUrl { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (await ApiHelper.ExecuteCallGuardedAsync(
                () => UsersClient.GetByIdAsync(Id), Snackbar)
            is UserDetailsDto user)
        {
            _firstName = user.FirstName;
            _lastName = user.LastName;
            _email = user.Email;
            _phoneNumber = user.PhoneNumber;
            _active = user.IsActive;
            _emailConfirmed = user.EmailConfirmed;
            _imageUrl = string.IsNullOrEmpty(user.ImageUrl) ? "" : Config[ConfigNames.ApiBaseUrl] + user.ImageUrl;
            Title = $"{_firstName} {_lastName}'s {_localizer["Profile"]}";
            Description = _email;
            if (_firstName?.Length > 0)
            {
                _firstLetterOfName = _firstName.ToUpper(CultureInfo.CurrentCulture).FirstOrDefault();
            }
        }

        _loaded = true;
    }
}