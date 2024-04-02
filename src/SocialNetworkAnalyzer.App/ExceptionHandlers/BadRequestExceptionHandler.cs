using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using SocialNetworkAnalyzer.App.Abstractions.ExceptionHandlers;

namespace SocialNetworkAnalyzer.App.ExceptionHandlers;

/// <summary>
/// ExceptionHandler for handling <see cref="ValidationException"/>
/// </summary>
public sealed class BadRequestExceptionHandler : ExceptionHandlerBase, IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        switch (exception)
        {
            case ValidationException validationException:
            {
                var extensions = validationException.Errors.ToDictionary<ValidationFailure, string, object?>(error => JsonNamingPolicy.CamelCase.ConvertName(error.PropertyName),
                    error =>
                        new
                        {
                            message = error.ErrorMessage,
                            severity = error.Severity.ToString(),
                            errorCode = error.ErrorCode,
                            attemptedValue = error.AttemptedValue
                        });

                await WriteProblemDetails(httpContext, StatusCodes.Status400BadRequest, "Validation error", validationException.Message, extensions);
                return true;
            }

            default:
            {
                return false;
            }
        }
    }
}