using Xunit;

namespace Tests.Shared;

public static class HttpResponseMessageExtensions
{
    public static async Task AssertSuccessStatusCodeAsync(this HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            Assert.Fail($"{response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
        }
    }
}