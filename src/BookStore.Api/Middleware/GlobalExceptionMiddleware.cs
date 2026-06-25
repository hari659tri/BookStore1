using System.Net;
using System.Text.Json;
using BookStore.Application.Common;
using FluentValidation;

namespace BookStore.Api.Middleware;

public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException exception)
        {
            await WriteErrorAsync(context, HttpStatusCode.BadRequest, "Validation failed.", exception.Errors.Select(x => x.ErrorMessage));
        }
        catch (AppException exception)
        {
            await WriteErrorAsync(context, (HttpStatusCode)exception.StatusCode, exception.Message);
        }
        catch (UnauthorizedAccessException exception)
        {
            await WriteErrorAsync(context, HttpStatusCode.Unauthorized, exception.Message);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception while processing {Method} {Path}", context.Request.Method, context.Request.Path);
            await WriteErrorAsync(context, HttpStatusCode.InternalServerError, "An unexpected error occurred.");
        }
    }

    private static async Task WriteErrorAsync(HttpContext context, HttpStatusCode statusCode, string message, IEnumerable<string>? errors = null)
    {
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var payload = new
        {
            status = (int)statusCode,
            title = message,
            traceId = context.TraceIdentifier,
            errors = errors?.ToArray()
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}
