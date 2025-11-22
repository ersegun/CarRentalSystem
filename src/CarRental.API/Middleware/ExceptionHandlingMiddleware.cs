using CarRental.Application.Exceptions;
using System.Net;
using System.Text.Json;

namespace CarRental.API.Middleware;

// Global exception handling middleware
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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

        var response = context.Response;
        response.ContentType = "application/json";

        var (statusCode, message) = exception switch
        {
            RentalNotFoundException ex => (HttpStatusCode.NotFound, ex.Message),
            RentalValidationException ex => (HttpStatusCode.BadRequest, ex.Message),
            DuplicateBookingException ex => (HttpStatusCode.Conflict, ex.Message),
            _ => (HttpStatusCode.InternalServerError, "An internal server error occurred. Please try again later.")
        };

        response.StatusCode = (int)statusCode;

        var errorResponse = new
        {
            error = message,
            statusCode = (int)statusCode,
            timestamp = DateTime.UtcNow
        };

        var jsonResponse = JsonSerializer.Serialize(errorResponse);
        await response.WriteAsync(jsonResponse);
    }
}
