using System.Diagnostics;
using System.Net;
using System.Text.Json;

namespace HospitalApi.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
                _logger.LogError(ex, "Unhandled Exception");

                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";

                var response = new
                {
                    success = false,
                    message = "Something went wrong."
                };

                await context.Response.WriteAsJsonAsync(response);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            //Admin@123
            context.Response.ContentType = "application/json";
            int statusCodes = ex switch
            {
                ArgumentException => StatusCodes.Status400BadRequest,
                KeyNotFoundException => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status500InternalServerError
            };

            context.Response.StatusCode = statusCodes;

            var response = new Models.Common.ApiError
            {
                StatusCode = statusCodes,
                Message = statusCodes == 500 
                    ?"An unexpected error occurred."
                    : ex.Message,
            #if Debug
                Details = ex.StackTrace
            #endif
            };

            var json = JsonSerializer.Serialize(response);

            return context.Response.WriteAsync(json);
        }
    }
}
