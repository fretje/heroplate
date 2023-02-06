using System.Net;
using Hellang.Middleware.ProblemDetails;
using Heroplate.Api.Application.Common.Exceptions;

namespace Heroplate.Api.Infrastructure.Common.ProblemDetails;

public class HeroProblemDetails : StatusCodeProblemDetails
{
    protected HeroProblemDetails(int statusCode, Exception ex)
        : base(statusCode)
    {
        Detail = ex.Message;
        if (ex.TargetSite?.DeclaringType?.FullName is { } source)
        {
            Extensions["Source"] = source;
        }
    }

    public static Microsoft.AspNetCore.Mvc.ProblemDetails Create(Exception exception)
    {
        var statusCode = exception is CustomException customException
            ? customException.StatusCode
            : HttpStatusCode.InternalServerError;

        return new HeroProblemDetails((int)statusCode, exception);
    }
}