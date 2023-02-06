namespace Heroplate.Api.Application.Identity.Users.Password;

public record ResetPasswordRequest(string Email, string Password, string Token);