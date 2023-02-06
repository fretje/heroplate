using Heroplate.Api.Application.Identity.Users.Password;

namespace Heroplate.Api.Application.Identity.Users;

public interface IUserService : ITransientService
{
    Task<bool> ExistsWithNameAsync(string name);
    Task<bool> ExistsWithEmailAsync(string email, string? exceptId = null);
    Task<bool> ExistsWithPhoneNumberAsync(string phoneNumber, string? exceptId = null);

    Task<List<UserDetailsDto>> GetListAsync(CancellationToken ct);

    Task<int> GetCountAsync(CancellationToken ct);

    Task<UserDetailsDto> GetAsync(string userId, CancellationToken ct);

    Task<List<UserRoleDto>> GetRolesAsync(string userId, CancellationToken ct);
    Task<string> AssignRolesAsync(string userId, UserRolesRequest req, CancellationToken ct);

    Task<string[]> GetPermissionsAsync(string userId, CancellationToken ct = default);

    Task ToggleStatusAsync(ToggleUserStatusRequest req, CancellationToken ct);

    Task<string> CreateAsync(CreateUserRequest request, string origin, CancellationToken ct);
    Task UpdateAsync(UpdateUserRequest req, CancellationToken ct);

    Task<string> ConfirmEmailAsync(string userId, string code, CancellationToken ct);

    Task<string> ForgotPasswordAsync(ForgotPasswordRequest request, string origin);
    Task<string> ResetPasswordAsync(ResetPasswordRequest request);
    Task ChangePasswordAsync(ChangePasswordRequest request, string userId);
}