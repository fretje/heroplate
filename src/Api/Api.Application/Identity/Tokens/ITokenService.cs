namespace Heroplate.Api.Application.Identity.Tokens;

public interface ITokenService : ITransientService
{
    Task<TokenResponse> GetTokenAsync(TokenRequest request, string ipAddress, CancellationToken ct);

    Task<TokenResponse> RefreshTokenAsync(RefreshTokenRequest request, string ipAddress);
}