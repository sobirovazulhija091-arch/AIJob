using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

public class ApiExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiExceptionMiddleware> _logger;

    public ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
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
            _logger.LogError(ex, "Unhandled API exception");
            await WriteErrorAsync(context, ex);
        }
    }

    private static async Task WriteErrorAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message) = exception switch
        {
            InvalidOperationException ex => (HttpStatusCode.BadRequest, ex.Message),
            UnauthorizedAccessException ex => (HttpStatusCode.Unauthorized, ex.Message),
            DbUpdateException ex => (HttpStatusCode.BadRequest, BuildDbUpdateMessage(ex)),
            _ => (HttpStatusCode.InternalServerError, "Something went wrong. Please try again.")
        };

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var payload = JsonSerializer.Serialize(new
        {
            statusCode = (int)statusCode,
            message
        });

        await context.Response.WriteAsync(payload);
    }

    private static string BuildDbUpdateMessage(DbUpdateException ex)
    {
        var message = ex.InnerException?.Message ?? ex.Message;

        if (message.Contains("IX_JobCategories_Name", StringComparison.OrdinalIgnoreCase))
            return "Job category already exists. You cannot add the same category twice.";

        if (message.Contains("IX_Skills_Name", StringComparison.OrdinalIgnoreCase))
            return "Skill already exists. You cannot add the same skill twice.";

        if (message.Contains("IX_Organizations_Name", StringComparison.OrdinalIgnoreCase))
            return "Organization already exists. You cannot add the same organization twice.";

        if (message.Contains("IX_Languages_Name", StringComparison.OrdinalIgnoreCase))
            return "Language already exists. You cannot add the same language twice.";

        if (message.Contains("duplicate", StringComparison.OrdinalIgnoreCase))
            return "This record already exists. Please use a different value.";

        return "The request could not be saved. Please check the data and try again.";
    }
}
