using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;

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
            // Continue the middleware pipeline
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Set default response code
        HttpStatusCode code = HttpStatusCode.InternalServerError;

        if (exception is UnauthorizedAccessException)
            code = HttpStatusCode.Unauthorized; // 401
        else if (exception is ArgumentException || exception is FormatException)
            code = HttpStatusCode.BadRequest; // 400
        else if (exception is TimeoutException)
            code = HttpStatusCode.RequestTimeout; // 408


        context.Response.StatusCode = (int)code;
        context.Response.ContentType = "application/json";

        var response = new
        {
            StatusCode = context.Response.StatusCode,
            Message = exception.Message,
            Details = exception.StackTrace // Optional in production
        };

        return context.Response.WriteAsJsonAsync(response);
    }
}
