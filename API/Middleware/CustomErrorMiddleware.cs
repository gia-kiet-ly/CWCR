using Domain.Base;
using System.Net;
using System.Text.Json;

namespace API.Middleware
{
    public class CustomErrorMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CustomErrorMiddleware> _logger;

        public CustomErrorMiddleware(RequestDelegate next, ILogger<CustomErrorMiddleware> logger)
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
            catch (BaseException.CoreException ex)
            {
                _logger.LogError(ex, "Core exception occurred.");
                await HandleCoreExceptionAsync(context, ex);
            }
            catch (BaseException.ErrorException ex)
            {
                _logger.LogError(ex, "Business error exception occurred.");
                await HandleErrorExceptionAsync(context, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected exception occurred.");
                await HandleUnexpectedExceptionAsync(context, ex);
            }
        }

        private static async Task HandleCoreExceptionAsync(HttpContext context, BaseException.CoreException ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = ex.StatusCode;

            var response = new
            {
                code = ex.Code,
                message = ex.Message,
                statusCode = ex.StatusCode,
                additionalData = ex.AdditionalData
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }

        private static async Task HandleErrorExceptionAsync(HttpContext context, BaseException.ErrorException ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = ex.StatusCode;

            var response = new
            {
                code = ex.ErrorDetail.ErrorCode,
                message = ex.ErrorDetail.ErrorMessage,
                statusCode = ex.StatusCode
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }

        //Production
        private static async Task HandleUnexpectedExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var isDev = context.RequestServices
                .GetRequiredService<IWebHostEnvironment>()
                .IsDevelopment();

            var response = new
            {
                code = "INTERNAL_ERROR",
                message = "An unexpected error occurred.",
                detail = isDev ? ex.Message : null,
                inner = isDev ? ex.InnerException?.Message : null,
                inner2 = isDev ? ex.InnerException?.InnerException?.Message : null
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
