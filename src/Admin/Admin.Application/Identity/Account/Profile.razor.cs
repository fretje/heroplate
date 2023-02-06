namespace Heroplate.Admin.Application.Identity.Account;

public partial class Profile
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthState { get; set; } = default!;

    private readonly UpdateUserRequest _profileModel = new();

    private string? _imageUrl;
    private string? _userId;
    private char _firstLetterOfName;

    private CustomValidation? _customValidation;

    protected override async Task OnInitializedAsync()
    {
        if ((await AuthState).User is { } user)
        {
            _userId = user.GetUserId();
            _profileModel.Email = user.GetEmail() ?? "";
            _profileModel.FirstName = user.GetFirstName() ?? "";
            _profileModel.LastName = user.GetSurname() ?? "";
            _profileModel.PhoneNumber = user.GetPhoneNumber();
            _imageUrl = string.IsNullOrEmpty(user?.GetImageUrl()) ? "" : Config[ConfigNames.ApiBaseUrl] + user?.GetImageUrl();
            if (_userId is not null)
            {
                _profileModel.Id = _userId;
            }
        }

        if (_profileModel.FirstName?.Length > 0)
        {
            _firstLetterOfName = _profileModel.FirstName.ToUpper(CultureInfo.CurrentCulture).FirstOrDefault();
        }
    }

    private async Task UpdateProfileAsync()
    {
        if (await ApiHelper.ExecuteCallGuardedAsync(
            () => PersonalClient.UpdateProfileAsync(_profileModel), Snackbar, _customValidation))
        {
            Snackbar.Add(L["Your Profile has been updated. Please Login again to Continue."], Severity.Success);
            Authenticator.NavigateToLogin(Navigation.Uri);
        }
    }

    private async Task UploadFilesAsync(InputFileChangeEventArgs e)
    {
        var file = e.File;
        if (file is not null)
        {
            string? extension = Path.GetExtension(file.Name);
            if (!AdminConstants.SupportedImageFormats.Contains(extension.ToLowerInvariant()))
            {
                Snackbar.Add("Image Format Not Supported.", Severity.Error);
                return;
            }

            string? fileName = $"{_userId}-{Guid.NewGuid():N}";
            fileName = fileName[..Math.Min(fileName.Length, 90)];
            var imageFile = await file.RequestImageFileAsync(AdminConstants.StandardImageFormat, AdminConstants.MaxImageWidth, AdminConstants.MaxImageHeight);
            byte[]? buffer = new byte[imageFile.Size];
            await imageFile.OpenReadStream(AdminConstants.MaxAllowedSize).ReadAsync(buffer);
            string? base64String = $"data:{AdminConstants.StandardImageFormat};base64,{Convert.ToBase64String(buffer)}";
            _profileModel.Image = new FileUploadRequest() { Name = fileName, Data = base64String, Extension = extension };

            await UpdateProfileAsync();
        }
    }

    public async Task RemoveImageAsync()
    {
        if (await DialogService.ShowDeleteConfirmationAsync(
            L["You're sure you want to delete your Profile Image?"]))
        {
            _profileModel.DeleteCurrentImage = true;
            await UpdateProfileAsync();
        }
    }
}