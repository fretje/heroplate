using Heroplate.Shared.Authorization;

namespace Shared.Authorization.Tests;

public class PermissionsAuthorizationPolicyNameTests
{
    [Theory]
    [InlineData(new[] { "test", "test2" }, false, $"{PermissionsAuthorizationPolicyName.Prefix}^test|test2^False")]
    [InlineData(new string[0], true, $"{PermissionsAuthorizationPolicyName.Prefix}^^True")]
    [InlineData(new[] { "test" }, true, $"{PermissionsAuthorizationPolicyName.Prefix}^test^True")]
    [InlineData(new[] { "test", "test2", "test 5" }, false, $"{PermissionsAuthorizationPolicyName.Prefix}^test|test2|test 5^False")]
    [InlineData(null, false, null)]
    public void ForTests(string[] permissions, bool allRequired, string expected)
    {
        string? policyName = PermissionsAuthorizationPolicyName.For(permissions, allRequired);

        Assert.Equal(expected, policyName);
    }

    [Theory]
    [InlineData($"{PermissionsAuthorizationPolicyName.Prefix}^test|test2^False", true)]
    [InlineData($"{PermissionsAuthorizationPolicyName.Prefix}^^True", true)]
    [InlineData($"{PermissionsAuthorizationPolicyName.Prefix}^test^True", true)]
    [InlineData($"{PermissionsAuthorizationPolicyName.Prefix}^test|test2|test 5^False", true)]
    [InlineData($"{PermissionsAuthorizationPolicyName.Prefix}^test|test2|test 5", false)]
    [InlineData("", false)]
    [InlineData("^", false)]
    [InlineData("|", false)]
    [InlineData(null, false)]
    public void TryParseTests(string policyName, bool expected)
    {
        bool isValidName = PermissionsAuthorizationPolicyName.TryParse(policyName, out string[]? permissions, out bool allRequired);

        Assert.Equal(expected, isValidName);
    }

    [Theory]
    [InlineData($"{PermissionsAuthorizationPolicyName.Prefix}^test|test2^False", new[] { "test", "test2" })]
    [InlineData($"{PermissionsAuthorizationPolicyName.Prefix}^^True", new string[0])]
    [InlineData($"{PermissionsAuthorizationPolicyName.Prefix}^test^True", new[] { "test" })]
    [InlineData($"{PermissionsAuthorizationPolicyName.Prefix}^test|test2|test 5^False", new[] { "test", "test2", "test 5" })]
    [InlineData("", new string[0])]
    [InlineData(null, new string[0])]
    public void PermissionsFromTests(string policyName, string[] expected)
    {
        string[] permissions = PermissionsAuthorizationPolicyName.PermissionsFrom(policyName);

        Assert.Equal(expected, permissions);
    }

    [Theory]
    [InlineData($"{PermissionsAuthorizationPolicyName.Prefix}^test|test2^False", false)]
    [InlineData($"{PermissionsAuthorizationPolicyName.Prefix}^^True", true)]
    [InlineData($"{PermissionsAuthorizationPolicyName.Prefix}^test^True", true)]
    [InlineData($"{PermissionsAuthorizationPolicyName.Prefix}^test|test2|test 5^False", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void AllRequiredFromTests(string policyName, bool expected)
    {
        bool allRequired = PermissionsAuthorizationPolicyName.AllRequiredFrom(policyName);

        Assert.Equal(expected, allRequired);
    }
}