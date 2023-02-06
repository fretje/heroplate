using Heroplate.Api.Application.Catalog.Brands;
using Heroplate.Api.Domain.Catalog;

namespace Api.Host.Tests.CRUD;

public class BrandTests : CRUDTestsBase
{
    protected override string ApiUrl => "/api/v1/brands";
    protected override string EntityName => nameof(Brand);

    public BrandTests(HeroApiAppFactoryFixture appFactory)
        : base(appFactory)
    {
    }

    [Fact]
    public Task SearchWithoutPermissionShouldReturnForbidden() =>
        AssertSearchWithoutPermissionFailsWithForbidden();

    [Fact]
    public Task CreateWithoutPermissionShouldReturnForbidden() =>
        AssertCreateWithoutPermissionFailsWithForbidden();

    [Fact]
    public Task UpdateWithoutPermissionShouldReturnForbidden() =>
        AssertUpdateWithoutPermissionFailsWithForbidden();

    [Fact]
    public Task DeleteWithoutPermissionShouldReturnForbidden() =>
        AssertDeleteWithoutPermissionFailsWithForbidden();

    [Fact]
    public Task SearchShouldReturn0() =>
        AssertSearchSucceedsWithCount(0);

    [Fact]
    public Task CreateWithoutNameShouldFail() =>
        AssertCreateFailsWithoutName(new CreateBrandRequest());

    [Fact]
    public Task CreateWithEmptyNameShouldFail() =>
        AssertCreateFailsWithEmptyName(new CreateBrandRequest { Name = "" });

    [Fact]
    [Order(1)]
    public Task CreateShouldSucceed() =>
        AssertCreateAndGetSucceeds<CreateBrandRequest, BrandDto>(
            new() { Name = "Test Brand", Description = "Test Brand Description" },
            1);

    [Fact]
    [Order(2)]
    public Task UpdateWithoutNameShouldFail() =>
        AssertUpdateFailsWithoutName(new UpdateBrandRequest());

    [Fact]
    [Order(2)]
    public Task UpdateWithEmptyNameShouldFail() =>
        AssertUpdateFailsWithEmptyName(new UpdateBrandRequest { Name = "" });

    [Fact]
    [Order(2)]
    public Task UpdateWithoutChangesShouldSucceed() =>
        AssertUpdateAndGetSucceeds<UpdateBrandRequest, BrandDto>(
            new() { Id = 1, Name = "Test Brand", Description = "Test Brand Description" });

    [Fact]
    [Order(3)]
    public Task UpdateNameShouldSucceed() =>
        AssertUpdateAndGetSucceeds<UpdateBrandRequest, BrandDto>(
            new() { Id = 1, Name = "Test Brand Updated", Description = "Test Brand Description" });

    [Fact]
    [Order(4)]
    public Task UpdateDescriptionShouldSucceed() =>
        AssertUpdateAndGetSucceeds<UpdateBrandRequest, BrandDto>(
            new() { Id = 1, Name = "Test Brand", Description = "Test Brand Description Updated" });

    [Fact]
    [Order(5)]
    public Task Create2ndWithSameNameShouldFail() =>
        AssertCreateFailsWithSameName(new CreateBrandRequest { Name = "Test Brand" });

    [Fact]
    [Order(5)]
    public Task Create2ndShouldSucceed() =>
        AssertCreateAndGetSucceeds<CreateBrandRequest, BrandDto>(
            new() { Name = "Test Brand 2", Description = "Test Brand 2 Description" },
            2);

    [Fact]
    [Order(6)]
    public Task Update2ndWithSameNameAsFirstShouldFail() =>
        AssertUpdateFailsWithSameName(new UpdateBrandRequest { Id = 2, Name = "Test Brand" });

    [Fact]
    [Order(6)]
    public Task SearchShouldReturn2() =>
        AssertSearchSucceedsWithCount(2);

    [Fact]
    [Order(7)]
    public Task DeleteShouldSucceed() =>
        AssertDeleteSucceeds(1);

    [Fact]
    [Order(8)]
    public Task SearchShouldReturn1() =>
        AssertSearchSucceedsWithCount(1);
}