using System.Net.Http.Json;
using Heroplate.Api.Application.Catalog.Brands;
using Heroplate.Api.Application.Catalog.Products;

namespace Tests.Shared;

public static class HttpClientExtensions
{
    public static Task<int> CreateBrandAsync(this HttpClient client) =>
        client.PostAsJsonAndGetIdAsync("/api/v1/brands", new CreateBrandRequest { Name = $"Brand {Guid.NewGuid():D}" });

    public static async Task<int> CreateProductAsync(this HttpClient client, int? brandId = null)
    {
        brandId ??= await client.CreateBrandAsync();

        return await client.PostAsJsonAndGetIdAsync("/api/v1/products", new CreateProductRequest { Name = $"Product {Guid.NewGuid():D}", Rate = 1, BrandId = brandId.Value });
    }

    public static async Task<int> PostAsJsonAndGetIdAsync<T>(this HttpClient client, string requestUri, T value)
    {
        var result = await client.PostAsJsonAsync(requestUri, value);
        await result.AssertSuccessStatusCodeAsync();
        return await result.Content.ReadFromJsonAsync<int>();
    }
}