using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace Heroplate.Api.Host.Controllers;

#nullable disable
#pragma warning disable RCS1163, IDE0060

public static class BaseApiConventions
{
    [ProducesResponseType(200)]
    [ProducesDefaultResponseType(typeof(ProblemDetails))]
    public static void BaseGet()
    {
    }

    [ProducesResponseType(204)]
    [ProducesResponseType(400, Type = typeof(HttpValidationProblemDetails))]
    [ProducesDefaultResponseType(typeof(ProblemDetails))]
    public static void BasePut()
    {
    }

    [ProducesResponseType(200)]
    [ProducesResponseType(400, Type = typeof(HttpValidationProblemDetails))]
    [ProducesDefaultResponseType(typeof(ProblemDetails))]
    [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
    public static void Search(
        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Any)]
        [ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.Any)]
        object req,
        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Any)]
        [ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.Any)]
        object ct)
    {
    }

    [ProducesResponseType(200)]
    [ProducesDefaultResponseType(typeof(ProblemDetails))]
    [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
    public static void Get()
    {
    }

    [ProducesResponseType(200)]
    [ProducesDefaultResponseType(typeof(ProblemDetails))]
    [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
    public static void Get(
        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Any)]
        [ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.Any)]
        object id)
    {
    }

    [ProducesResponseType(200)]
    [ProducesDefaultResponseType(typeof(ProblemDetails))]
    [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
    public static void Get(
        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Any)]
        [ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.Any)]
        object id,
        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Any)]
        [ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.Any)]
        object ct)
    {
    }

    [ProducesResponseType(200)]
    [ProducesResponseType(400, Type = typeof(HttpValidationProblemDetails))]
    [ProducesDefaultResponseType(typeof(ProblemDetails))]
    [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
    public static void Create(
        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Any)]
        [ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.Any)]
        object req)
    {
    }

    [ProducesResponseType(200)]
    [ProducesResponseType(400, Type = typeof(HttpValidationProblemDetails))]
    [ProducesDefaultResponseType(typeof(ProblemDetails))]
    [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
    public static void Create(
        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Any)]
        [ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.Any)]
        object req,
        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Any)]
        [ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.Any)]
        object ct)
    {
    }

    [ProducesResponseType(200)]
    [ProducesResponseType(400, Type = typeof(HttpValidationProblemDetails))]
    [ProducesDefaultResponseType(typeof(ProblemDetails))]
    [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
    public static void Update(
        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Any)]
        [ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.Any)]
        object req,
        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Any)]
        [ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.Any)]
        object id)
    {
    }

    [ProducesResponseType(200)]
    [ProducesResponseType(400, Type = typeof(HttpValidationProblemDetails))]
    [ProducesDefaultResponseType(typeof(ProblemDetails))]
    [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
    public static void Update(
        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Any)]
        [ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.Any)]
        object req,
        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Any)]
        [ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.Any)]
        object id,
        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Any)]
        [ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.Any)]
        object ct)
    {
    }

    [ProducesResponseType(200)]
    [ProducesDefaultResponseType(typeof(ProblemDetails))]
    [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
    public static void Delete(
        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Any)]
        [ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.Any)]
        object id,
        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Any)]
        [ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.Any)]
        object ct)
    {
    }
}