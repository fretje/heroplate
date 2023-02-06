using System.Net;

namespace Heroplate.Api.Application.Common.Exceptions;

public class NotFoundException : CustomException
{
    public NotFoundException(string message = "Not Found")
        : base(message, null, HttpStatusCode.NotFound)
    {
    }
}