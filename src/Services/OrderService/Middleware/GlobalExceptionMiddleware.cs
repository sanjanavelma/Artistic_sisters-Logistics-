using Artistic_Sisters.Shared.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace OrderService.Middleware;

/// <summary>
/// Global exception handling middleware wired into the OrderService pipeline.
///
/// Why this exists:
///   The Shared library already defines DomainException for business rule
///   violations (e.g. "an order must have at least one item"). Without this
///   middleware, that exception would bubble up as an unformatted 500 even
///   though it is a 400-level caller error.
///
/// What it does:
///   - DomainException  → HTTP 400  + JSON { error, statusCode }
///   - Anything else    → HTTP 500  + generic JSON (hides internal detail)
///
/// Every error, regardless of type, always returns the same JSON shape:
///   { "statusCode": 400, "error": "An order must contain at least one item." }
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
    {
        _next   = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DomainException ex)
        {
            // Known business-rule violation — the caller did something wrong.
            // Log at Warning (not Error) because this is expected behaviour.
            _logger.LogWarning("Domain rule violated on {Method} {Path}: {Message}",
                context.Request.Method, context.Request.Path, ex.Message);

            await WriteJsonResponse(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (Exception ex)
        {
            // Something unexpected happened — log the full stack for diagnosis.
            _logger.LogError(ex, "Unhandled exception on {Method} {Path}",
                context.Request.Method, context.Request.Path);

            await WriteJsonResponse(context, HttpStatusCode.InternalServerError,
                "An unexpected error occurred. Please try again later.");
        }
    }

    // Shared helper — keeps the catch blocks DRY.
    private static Task WriteJsonResponse(HttpContext context, HttpStatusCode code, string message)
    {
        context.Response.StatusCode  = (int)code;
        context.Response.ContentType = "application/json";

        var payload = JsonSerializer.Serialize(new
        {
            statusCode = (int)code,
            error      = message
        });

        return context.Response.WriteAsync(payload);
    }
}
