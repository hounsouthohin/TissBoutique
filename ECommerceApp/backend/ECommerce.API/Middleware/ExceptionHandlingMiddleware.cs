using System.Net;
using System.Text.Json;

namespace ECommerce.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IWebHostEnvironment _env;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var (statusCode, message) = exception switch
            {
                // Exceptions personnalisées de l'application (si vous en avez)
                // ECommerce.Application.Exceptions.ValidationException validationException => (HttpStatusCode.BadRequest, validationException.Message),
                ECommerce.Application.Exceptions.NotFoundException => (HttpStatusCode.NotFound, exception.Message),
                ECommerce.Application.Exceptions.BadRequestException => (HttpStatusCode.BadRequest, exception.Message),
                
                // Exceptions générales
                UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Unauthorized"),
                KeyNotFoundException => (HttpStatusCode.NotFound, "The requested resource was not found."),
                _ => (HttpStatusCode.InternalServerError, "An internal server error has occurred.")
            };

            context.Response.StatusCode = (int)statusCode;

            object response;
            if (_env.IsDevelopment())
            {
                response = new
                {
                    StatusCode = (int)statusCode,
                    Message = message,
                    Detailed = exception.ToString() // Fournir la trace complète en développement
                };
            }
            else
            {
                response = new
                {
                    StatusCode = (int)statusCode,
                    Message = message
                };
            }

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            return context.Response.WriteAsync(jsonResponse);
        }
    }
}

