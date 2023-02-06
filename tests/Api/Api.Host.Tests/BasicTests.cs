using System.Net;
using System.Net.Http.Json;

namespace Api.Host.Tests;

public class BasicTests : HeroplateApiTestsBase
{
    public BasicTests(HeroApiAppFactoryFixture appFactory)
        : base(appFactory)
    {
    }

    [Fact]
    public async Task GetHomeEndpointReturnsSuccess() =>
        await (await AnonymousClient.GetAsync("/api"))
            .AssertSuccessStatusCodeAsync();

    [Theory]
    [InlineData("/api/v1/brands/search")]
    [InlineData("/api/v1/products/search")]
    public async Task SearchEndpointWithAnonymousUserReturnsUnAuthorized(string endpoint)
    {
        var response = await AnonymousClient.PostAsJsonAsync(endpoint, new object());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/v1/brands/search")]
    [InlineData("/api/v1/products/search")]
    public async Task SearchEndpointWithAdminUserReturnsSuccess(string endpoint) =>
        await (await AdminClient.PostAsJsonAsync(endpoint, new object()))
            .AssertSuccessStatusCodeAsync();

    [Fact]
    public async Task NotificationsClientCanConnect()
    {
        await using var notifications = await AppFactory.StartNotificationsClientAsync();
    }
}