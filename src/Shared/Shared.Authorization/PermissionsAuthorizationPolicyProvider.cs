using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Heroplate.Shared.Authorization;

public class PermissionsAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
{
    private readonly AuthorizationOptions _options;
    public PermissionsAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
        : base(options) =>
        _options = options.Value;

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        var policy = await base.GetPolicyAsync(policyName);

        if (policy == null && PermissionsAuthorizationPolicyName
            .TryParse(policyName, out string[]? permissions, out bool allRequired))
        {
            policy = new AuthorizationPolicyBuilder()
                .AddRequirements(new PermissionsAuthorizationRequirement(permissions, allRequired))
                .Build();

            _options.AddPolicy(policyName, policy);
        }

        return policy;
    }
}