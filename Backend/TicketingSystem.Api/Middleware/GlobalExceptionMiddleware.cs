using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketingSystem.Domain.Exceptions;

namespace TicketingSystem.Api.Middleware;

public class GlobalExceptionMiddleware
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
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "An error occurred: {Message}", exception.Message);

        context.Response.ContentType = "application/json";

        var isDevelopment =
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

        string? innerMessage = exception.InnerException?.Message;
        string? baseMessage = exception.GetBaseException().Message;

        switch (exception)
        {
            case DbUpdateConcurrencyException:
            case ConflictException:

                context.Response.StatusCode = StatusCodes.Status409Conflict;

                await context.Response.WriteAsJsonAsync(new
                {
                    error = exception is ConflictException
                        ? exception.Message
                        : "Concurrent update conflict",

                    retryAfter = 1000,

                    message = isDevelopment ? exception.Message : null,
                    inner = isDevelopment ? innerMessage : null,
                    detail = isDevelopment ? exception.StackTrace : null
                });
                break;

            case DbUpdateException dbEx:

                context.Response.StatusCode = StatusCodes.Status400BadRequest;

                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Database update failed",
                    message = isDevelopment ? dbEx.Message : null,
                    inner = isDevelopment ? innerMessage : null,
                    baseError = isDevelopment ? baseMessage : null,
                    detail = isDevelopment ? dbEx.StackTrace : null
                });
                break;

            case BusinessException businessEx:

                context.Response.StatusCode = StatusCodes.Status400BadRequest;

                await context.Response.WriteAsJsonAsync(new
                {
                    error = businessEx.Message
                });
                break;

            case KeyNotFoundException notFoundEx:

                context.Response.StatusCode = StatusCodes.Status404NotFound;

                await context.Response.WriteAsJsonAsync(new
                {
                    error = notFoundEx.Message
                });
                break;

            default:

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                await context.Response.WriteAsJsonAsync(new
                {
                    error = "An unexpected error occurred",
                    message = isDevelopment ? exception.Message : null,
                    inner = isDevelopment ? innerMessage : null,
                    baseError = isDevelopment ? baseMessage : null,
                    detail = isDevelopment ? exception.StackTrace : null
                });
                break;
        }
    }
}
