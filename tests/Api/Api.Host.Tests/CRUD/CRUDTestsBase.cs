using System.Net;
using System.Net.Http.Json;
using Heroplate.Api.Application.Catalog.Brands;
using Heroplate.Api.Application.Common.Entities;
using Heroplate.Api.Application.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace Api.Host.Tests.CRUD;

public abstract class CRUDTestsBase : HeroplateApiTestsBase
{
    protected CRUDTestsBase(HeroApiAppFactoryFixture appFactory)
        : base(appFactory)
    {
    }

    protected abstract string ApiUrl { get; }
    protected abstract string EntityName { get; }

    protected async Task AssertSearchWithoutPermissionFailsWithForbidden()
    {
        var response = await PermissionlessClient.PostAsJsonAsync($"{ApiUrl}/search", new object());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    protected async Task AssertCreateWithoutPermissionFailsWithForbidden()
    {
        var response = await PermissionlessClient.PostAsJsonAsync($"{ApiUrl}", new object());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    protected async Task AssertUpdateWithoutPermissionFailsWithForbidden()
    {
        var response = await PermissionlessClient.PutAsJsonAsync($"{ApiUrl}/1", new object());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    protected async Task AssertDeleteWithoutPermissionFailsWithForbidden()
    {
        var response = await PermissionlessClient.DeleteAsync($"{ApiUrl}/1");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    protected async Task AssertSearchSucceedsWithCount(int count)
    {
        var response = await AdminClient.PostAsJsonAsync($"{ApiUrl}/search", new object());

        await response.AssertSuccessStatusCodeAsync();

        var brands = await response.Content.ReadFromJsonAsync<PaginationResponse<BrandDto>>();

        Assert.NotNull(brands);
        Assert.Equal(count, brands.TotalCount);
        Assert.Equal(count, brands.Data.Count);
    }

    protected Task AssertCreateFailsWithoutName<TCreateRequest>(TCreateRequest createRequest) =>
        AssertCreateFailsWithout("Name", createRequest);

    protected async Task AssertCreateFailsWithout<TCreateRequest>(string propertyName, TCreateRequest createRequest, string? fieldName = null)
    {
        var response = await AdminClient.PostAsJsonAsync(ApiUrl, createRequest);

        await AssertValidationProblemFieldIsRequired(propertyName, fieldName, response);
    }

    protected Task AssertCreateFailsWithEmptyName<TCreateRequest>(TCreateRequest createRequest) =>
        AssertCreateFailsWithEmpty("Name", createRequest);

    protected async Task AssertCreateFailsWithEmpty<TCreateRequest>(string propertyName, TCreateRequest createRequest, string? fieldName = null)
    {
        var response = await AdminClient.PostAsJsonAsync(ApiUrl, createRequest);

        await AssertValidationProblemFieldMustNotBeEmpty(propertyName, fieldName, response);
    }

    protected async Task AssertCreateFailsWithSameName<TCreateRequest>(TCreateRequest createRequest)
        where TCreateRequest : IEntityWithNameRequest
    {
        var response = await AdminClient.PostAsJsonAsync(ApiUrl, createRequest);

        await AssertValidationProblemNameAlreadyExists(createRequest, response);
    }

    protected async Task AssertCreateAndGetSucceeds<TCreateRequest, TDto>(TCreateRequest createRequest, int expectedId)
    {
        var response = await AdminClient.PostAsJsonAsync(ApiUrl, createRequest);

        await response.AssertSuccessStatusCodeAsync();

        int newId = await response.Content.ReadFromJsonAsync<int>();

        Assert.Equal(expectedId, newId);

        response = await AdminClient.GetAsync($"{ApiUrl}/{expectedId}");

        await response.AssertSuccessStatusCodeAsync();

        var dto = await response.Content.ReadFromJsonAsync<TDto>();

        dto.Should().BeEquivalentTo(createRequest, opts => opts.ExcludingMissingMembers());
    }

    protected Task AssertUpdateFailsWithoutName<TUpdateRequest>(TUpdateRequest updateRequest)
       where TUpdateRequest : IUpdateEntityRequest =>
       AssertUpdateFailsWithout("Name", updateRequest);

    protected async Task AssertUpdateFailsWithout<TUpdateRequest>(string propertyName, TUpdateRequest updateRequest, string? fieldName = null)
        where TUpdateRequest : IUpdateEntityRequest
    {
        var response = await AdminClient.PutAsJsonAsync($"{ApiUrl}/{updateRequest.Id}", updateRequest);

        await AssertValidationProblemFieldIsRequired(propertyName, fieldName, response);
    }

    protected Task AssertUpdateFailsWithEmptyName<TUpdateRequest>(TUpdateRequest updateRequest)
        where TUpdateRequest : IUpdateEntityRequest =>
        AssertUpdateFailsWithEmpty("Name", updateRequest);

    protected async Task AssertUpdateFailsWithEmpty<TUpdateRequest>(string propertyName, TUpdateRequest updateRequest, string? fieldName = null)
        where TUpdateRequest : IUpdateEntityRequest
    {
        var response = await AdminClient.PutAsJsonAsync($"{ApiUrl}/{updateRequest.Id}", updateRequest);

        await AssertValidationProblemFieldMustNotBeEmpty(propertyName, fieldName, response);
    }

    protected async Task AssertUpdateFailsWithSameName<TUpdateRequest>(TUpdateRequest updateRequest)
        where TUpdateRequest : IEntityWithNameRequest, IUpdateEntityRequest
    {
        var response = await AdminClient.PutAsJsonAsync($"{ApiUrl}/{updateRequest.Id}", updateRequest);

        await AssertValidationProblemNameAlreadyExists(updateRequest, response);
    }

    protected async Task AssertUpdateAndGetSucceeds<TUpdateRequest, TDto>(TUpdateRequest updateRequest)
        where TUpdateRequest : IUpdateEntityRequest
    {
        var response = await AdminClient.PutAsJsonAsync($"{ApiUrl}/{updateRequest.Id}", updateRequest);

        await response.AssertSuccessStatusCodeAsync();

        int updatedId = await response.Content.ReadFromJsonAsync<int>();

        Assert.Equal(updateRequest.Id, updatedId);

        response = await AdminClient.GetAsync($"{ApiUrl}/{updateRequest.Id}");

        await response.AssertSuccessStatusCodeAsync();

        var dto = await response.Content.ReadFromJsonAsync<TDto>();

        dto.Should().BeEquivalentTo(updateRequest, opts => opts.ExcludingMissingMembers());
    }

    protected async Task AssertDeleteSucceeds(int id)
    {
        var response = await AdminClient.DeleteAsync($"{ApiUrl}/{id}");

        await response.AssertSuccessStatusCodeAsync();

        int deletedId = await response.Content.ReadFromJsonAsync<int>();

        Assert.Equal(id, deletedId);
    }

    protected static async Task AssertValidationProblemFieldIsRequired(string propertyName, string? fieldName, HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        Assert.NotNull(problemDetails);
        Assert.Contains(propertyName, problemDetails.Errors);
        Assert.Equal($"The {fieldName ?? propertyName} field is required.", problemDetails.Errors[propertyName][0]);
    }

    private static async Task AssertValidationProblemFieldMustNotBeEmpty(string propertyName, string? fieldName, HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        Assert.NotNull(problemDetails);
        Assert.Contains(propertyName, problemDetails.Errors);
        Assert.Equal($"'{fieldName ?? propertyName}' must not be empty.", problemDetails.Errors[propertyName][0]);
    }

    protected static async Task AssertValidationProblemFieldMustBeGreaterThan(string propertyName, string value, HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        Assert.NotNull(problemDetails);
        Assert.Contains(propertyName, problemDetails.Errors);
        Assert.Equal($"'{propertyName}' must be greater than or equal to '{value}'.", problemDetails.Errors[propertyName][0]);
    }

    private async Task AssertValidationProblemNameAlreadyExists<TCreateRequest>(TCreateRequest createRequest, HttpResponseMessage response)
        where TCreateRequest : IEntityWithNameRequest
    {
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        Assert.NotNull(problemDetails);
        Assert.Contains("Name", problemDetails.Errors);
        Assert.Equal($"A {EntityName} with the name '{createRequest.Name}' already exists.", problemDetails.Errors["Name"][0]);
    }

    protected async Task AssertValidationProblemNameAlreadyExistsIn<TCreateRequest>(string parentEntityName, TCreateRequest updateRequest, HttpResponseMessage response)
        where TCreateRequest : IEntityWithNameRequest
    {
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        Assert.NotNull(problemDetails);
        Assert.Contains("Name", problemDetails.Errors);
        Assert.Equal($"A {EntityName} with the name '{updateRequest.Name}' already exists in this {parentEntityName}.", problemDetails.Errors["Name"][0]);
    }
}