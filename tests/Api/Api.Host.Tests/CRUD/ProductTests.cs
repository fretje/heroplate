using System.Net.Http.Json;
using Heroplate.Api.Application.Catalog.Products;
using Heroplate.Api.Domain.Catalog;

namespace Api.Host.Tests.CRUD;

public class ProductTests : CRUDTestsBase
{
    protected override string ApiUrl => "/api/v1/products";
    protected override string EntityName => nameof(Product);

    public ProductTests(HeroApiAppFactoryFixture appFactory)
        : base(appFactory)
    {
    }

    [Fact]
    public Task SearchShouldReturn0() =>
        AssertSearchSucceedsWithCount(0);

    [Fact]
    public Task CreateWithoutNameShouldFail() =>
        AssertCreateFailsWithoutName(new CreateProductRequest());

    [Fact]
    public Task CreateWithEmptyNameShouldFail() =>
        AssertCreateFailsWithEmptyName(new CreateProductRequest { Name = "" });

    [Fact]
    public Task CreateWithEmptyBrandShouldFail() =>
        AssertCreateFailsWithEmpty("BrandId", new CreateProductRequest { Name = "" }, "Brand Id");

    [Fact]
    public async Task CreateWithRateLowerThanOneShouldFail()
    {
        var response = await AdminClient.PostAsJsonAsync(ApiUrl, new CreateProductRequest { Name = "" });

        await AssertValidationProblemFieldMustBeGreaterThan("Rate", "1", response);
    }

    [Fact]
    [Order(1)]
    public async Task CreateShouldSucceed()
    {
        // We first need to create a brand
        int brandId = await AdminClient.CreateBrandAsync();

        await AssertCreateAndGetSucceeds<CreateProductRequest, ProductDto>(
            new() { Name = "Test Product", Description = "Test Product Description", Rate = 1, BrandId = brandId },
            1);
    }

    [Fact]
    [Order(2)]
    public Task UpdateWithoutNameShouldFail() =>
        AssertUpdateFailsWithoutName(new UpdateProductRequest());

    [Fact]
    [Order(2)]
    public Task UpdateWithEmptyNameShouldFail() =>
        AssertUpdateFailsWithEmptyName(new UpdateProductRequest { Name = "" });

    [Fact]
    [Order(2)]
    public Task UpdateWithEmptyBrandShouldFail() =>
        AssertUpdateFailsWithEmpty("BrandId", new UpdateProductRequest { Name = "" }, "Brand Id");

    [Fact]
    public async Task UpdateWithRateLowerThanOneShouldFail()
    {
        var response = await AdminClient.PutAsJsonAsync($"{ApiUrl}/0", new UpdateProductRequest { Name = "" });

        await AssertValidationProblemFieldMustBeGreaterThan("Rate", "1", response);
    }

    [Fact]
    [Order(2)]
    public Task UpdateWithoutChangesShouldSucceed() =>
        AssertUpdateAndGetSucceeds<UpdateProductRequest, ProductDto>(
            new() { Id = 1, Name = "Test Product", Description = "Test Product Description", Rate = 1, BrandId = 1 });

    [Fact]
    [Order(3)]
    public Task UpdateNameShouldSucceed() =>
        AssertUpdateAndGetSucceeds<UpdateProductRequest, ProductDto>(
            new() { Id = 1, Name = "Test Product Updated", Description = "Test Product Description", Rate = 1, BrandId = 1 });

    [Fact]
    [Order(4)]
    public Task UpdateDescriptionShouldSucceed() =>
        AssertUpdateAndGetSucceeds<UpdateProductRequest, ProductDto>(
            new() { Id = 1, Name = "Test Product", Description = "Test Product Description Updated", Rate = 1, BrandId = 1 });

    [Fact]
    [Order(4)]
    public Task UpdateRateShouldSucceed() =>
        AssertUpdateAndGetSucceeds<UpdateProductRequest, ProductDto>(
            new() { Id = 1, Name = "Test Product", Description = "Test Product Description", Rate = 2, BrandId = 1 });

    [Fact]
    [Order(5)]
    public async Task Create2ndWithSameNameShouldFail()
    {
        var request = new CreateProductRequest { Name = "Test Product", Rate = 1, BrandId = 1 };

        var response = await AppFactory.AdminClient.PostAsJsonAsync(ApiUrl, request);

        await AssertValidationProblemNameAlreadyExistsIn(nameof(Brand), request, response);
    }

    [Fact]
    [Order(5)]
    public Task Create2ndShouldSucceed() =>
        AssertCreateAndGetSucceeds<CreateProductRequest, ProductDto>(
            new() { Name = "Test Product 2", Description = "Test Product 2 Description", Rate = 1, BrandId = 1 },
            2);

    [Fact]
    [Order(6)]
    public async Task Create3rdWithSameNameInOtherBrandShouldSucceed()
    {
        // We first need to create a 2nd brand
        int brandId = await AdminClient.CreateBrandAsync();

        await AssertCreateAndGetSucceeds<CreateProductRequest, ProductDto>(new() { Name = "Test Product 2", Rate = 1, BrandId = brandId }, 3);
    }

    [Fact]
    [Order(6)]
    public async Task Update2ndWithSameNameAsFirstShouldFail()
    {
        var updateRequest = new UpdateProductRequest { Id = 2, Name = "Test Product", Rate = 1, BrandId = 1 };

        var response = await AdminClient.PutAsJsonAsync($"{ApiUrl}/{updateRequest.Id}", updateRequest);

        await AssertValidationProblemNameAlreadyExistsIn(nameof(Brand), updateRequest, response);
    }

    [Fact]
    [Order(6)]
    public async Task Update3rdWithSameNameInOtherBrandShouldSucceed() =>
         await AssertUpdateAndGetSucceeds<UpdateProductRequest, ProductDto>(
             new() { Id = 3, Name = "Test Product", Rate = 1, BrandId = 2 });

    [Fact]
    [Order(6)]
    public Task SearchShouldReturn3() =>
        AssertSearchSucceedsWithCount(3);

    [Fact]
    [Order(7)]
    public Task DeleteShouldSucceed() =>
        AssertDeleteSucceeds(1);

    [Fact]
    [Order(8)]
    public Task SearchShouldReturn2() =>
        AssertSearchSucceedsWithCount(2);
}