using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Bredinin.AlloyEditor.Core.Http.Exceptions
{
    public class ErrorHandlingMiddleware(RequestDelegate next, ILogger <ErrorHandlingMiddleware> logger)
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
                logger.LogError(ex,ex.Message);
                await HandleUnexpectedExceptionAsync(context, ex);
            }
        }

        private async Task HandleBusinessExceptionAsync(HttpContext context, BusinessException ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            var response = new
            {
                StatusCode = context.Response.StatusCode,
                Message = ex.Message
            };

            await JsonSerializer.SerializeAsync(context.Response.Body, response);
        }
        private async Task HandleUnexpectedExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            var response = new
            {
                StatusCode = context.Response.StatusCode,
                Message = "An unexpected error occurred"
            };

            await JsonSerializer.SerializeAsync(context.Response.Body, response);
        }
    }
}
