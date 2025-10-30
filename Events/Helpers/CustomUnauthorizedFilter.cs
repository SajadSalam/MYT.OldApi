using System.Net;
using System.Text.Json;
using Events.DATA.DTOs;
using Serilog;

public class CustomUnauthorizedMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHostEnvironment _hostEnvironment;

    public CustomUnauthorizedMiddleware(RequestDelegate next, IHostEnvironment hostEnvironment)
    {
        _next = next;
        _hostEnvironment = hostEnvironment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            // Log and handle any exception
            await HandleExceptionAsync(context, ex);
        }

        if (context.Response.StatusCode == 401 && !context.Response.HasStarted)
        {
            // Check if response has started before modifying it
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";

            var apiException = new ApiException(context.Response.StatusCode, "You are not authorized", null);
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(apiException, options);

            await context.Response.WriteAsync(json);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        if (!context.Response.HasStarted)
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            var apiException = _hostEnvironment.IsDevelopment()
                ? new ApiException(context.Response.StatusCode, ex.Message, ex.StackTrace)
                : new ApiException(context.Response.StatusCode, "Internal Server Error", null);

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(apiException, options);

            await context.Response.WriteAsync(json);
        }

        Log.Logger.Error(ex, "Error in {Endpoint} {Method} {ErrorMessage} {ErrorStackTrace}", context.Request.Path, context.Request.Method, ex.Message, ex.StackTrace);
        Log.CloseAndFlush();
    }
}
