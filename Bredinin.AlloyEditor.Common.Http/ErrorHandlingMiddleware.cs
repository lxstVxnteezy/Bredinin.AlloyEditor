using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Bredinin.AlloyEditor.Common.Http;

public class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (BusinessException ex)
        {
            await HandleBusinessExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error: {Message}", ex.Message);
            await HandleUnexpectedExceptionAsync(context, ex);
        }
    }

    private async Task HandleBusinessExceptionAsync(HttpContext context, BusinessException ex)
    {
        context.Response.ContentType = "application/json";
        
        var statusCode = (int)ex.StatusCode;
        context.Response.StatusCode = statusCode;

        var response = new ErrorResponse(
            StatusCode: statusCode,
            Message: GetDefaultMessageForStatusCode(statusCode),
            Detail: ex.Message
        );
        
        logger.LogWarning("Business exception: {Message}, StatusCode: {StatusCode}", ex.Message, statusCode);

        await JsonSerializer.SerializeAsync(context.Response.Body, response);
    }

    private async Task HandleUnexpectedExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var response = new ErrorResponse(
            StatusCode: StatusCodes.Status500InternalServerError,
            Message: "An unexpected error occurred",
            Detail: ex.Message // В production лучше скрыть детали
        );
        
        await JsonSerializer.SerializeAsync(context.Response.Body, response);
    }
    
    private static string GetDefaultMessageForStatusCode(int statusCode)
    {
        return statusCode switch
        {
            StatusCodes.Status400BadRequest => "Bad request",
            StatusCodes.Status404NotFound => "Resource not found",
            StatusCodes.Status409Conflict => "Resource conflict",
            StatusCodes.Status422UnprocessableEntity => "Validation error",
            _ => "An error occurred"
        };
    }
}