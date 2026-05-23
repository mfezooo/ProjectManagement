using System.Net;
using System.Text.Json;
using ProjectManagement.Application.Common.Exceptions;
using ProjectManagement.Application.Common.Models;
using ValidationException = ProjectManagement.Application.Common.Exceptions.ValidationException;

namespace ProjectManagement.API.Middleware;

public class GlobalExceptionMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

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
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        var statusCode = HttpStatusCode.InternalServerError;
        var message = "An unexpected error occurred.";
        List<string>? errors = null;

        switch (exception)
        {
            case ValidationException ve:
                statusCode = HttpStatusCode.BadRequest;
                message = "One or more validation errors occurred.";
                errors = ve.Errors
                    .SelectMany(kvp => kvp.Value.Select(v => $"{kvp.Key}: {v}"))
                    .ToList();
                _logger.LogInformation("Validation failure: {Errors}", string.Join("; ", errors));
                break;

            case NotFoundException nfe:
                statusCode = HttpStatusCode.NotFound;
                message = nfe.Message;
                _logger.LogInformation("Not found: {Message}", nfe.Message);
                break;

            case UnauthorizedException ue:
                statusCode = HttpStatusCode.Unauthorized;
                message = ue.Message;
                _logger.LogInformation("Unauthorized: {Message}", ue.Message);
                break;

            case ForbiddenException fe:
                statusCode = HttpStatusCode.Forbidden;
                message = fe.Message;
                _logger.LogInformation("Forbidden: {Message}", fe.Message);
                break;

            case ConflictException ce:
                statusCode = HttpStatusCode.Conflict;
                message = ce.Message;
                _logger.LogInformation("Conflict: {Message}", ce.Message);
                break;

            default:
                _logger.LogError(exception, "Unhandled exception while processing {Path}", context.Request.Path);
                break;
        }

        var response = ApiResponse<object>.Fail(message, errors);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }
}

public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app) =>
        app.UseMiddleware<GlobalExceptionMiddleware>();
}
