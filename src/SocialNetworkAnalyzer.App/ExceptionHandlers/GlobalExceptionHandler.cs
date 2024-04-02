using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using SocialNetworkAnalyzer.App.Abstractions.ExceptionHandlers;

namespace SocialNetworkAnalyzer.App.ExceptionHandlers;

/// <summary>
/// ExceptionHandler for handling all unhandled exceptions
/// </summary>
public class GlobalExceptionHandler : ExceptionHandlerBase, IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        switch (exception)
        {
            case ValidationException:
            {
                return false;
            }

            default:
            {
                var extensions = new Dictionary<string, object?>
                {
                    { "stackTrace", exception.StackTrace },
                    { "innerException", exception.InnerException?.Message },
                    { "source", exception.Source }
                };

                await WriteProblemDetails(httpContext, StatusCodes.Status500InternalServerError, "Unhandled error", exception.Message, extensions);
                return true;
            }
        }
    }
}