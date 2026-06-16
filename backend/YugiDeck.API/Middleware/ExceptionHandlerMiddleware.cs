using System.Net;
using System.Text.Json;

namespace YugiDeck.API.Middleware;

public class ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);
            await WriteErrorResponse(context, ex);
        }
    }

    private static async Task WriteErrorResponse(HttpContext context, Exception ex)
    {
        var (statusCode, message) = ex switch
        {
            ArgumentException or InvalidOperationException => (HttpStatusCode.BadRequest, ex.Message),
            UnauthorizedAccessException                    => (HttpStatusCode.Unauthorized, "Unauthorized"),
            KeyNotFoundException                           => (HttpStatusCode.NotFound, ex.Message),
            _                                              => (HttpStatusCode.InternalServerError, "An unexpected error occurred")
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var body = JsonSerializer.Serialize(new
        {
            status = (int)statusCode,
            error  = message
        });

        await context.Response.WriteAsync(body);
    }
}
