using System.Security.Claims;
using Heroplate.Api.Application.Common.Interfaces;

namespace Heroplate.Api.Infrastructure.Identity;

internal interface ICreateUserService : ITransientService
{
    Task<string> GetOrCreateFromPrincipalAsync(ClaimsPrincipal principal, CancellationToken ct = default);
}