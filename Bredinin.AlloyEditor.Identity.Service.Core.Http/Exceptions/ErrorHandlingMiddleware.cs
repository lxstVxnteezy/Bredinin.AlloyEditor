using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Bredinin.AlloyEditor.Identity.Service.Core.Http.Exceptions
{
    public class ErrorHandlingMiddleware(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context, ILogger<ErrorHandlingMiddleware> logger)
        {
            try
            {
                await next(context);
            }
            catch (BusinessException ex)
            {
                await HandleBusinessExceptionAsync(context, ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                await HandleAuthExceptionAsync(context);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);    
                await HandleUnexpectedExceptionAsync(context);
            }
        }

        private async Task HandleAuthExceptionAsync(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;

            var response = new
            {
                StatusCode = context.Response.StatusCode,
                Message = "Unauthorized"
            };

            await JsonSerializer.SerializeAsync(context.Response.Body, response);
        }

        private async Task HandleBusinessExceptionAsync(HttpContext context, Exception ex)
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
        private async Task HandleUnexpectedExceptionAsync(HttpContext context)
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
