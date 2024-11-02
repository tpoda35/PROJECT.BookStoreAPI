using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace BookStoreAPI.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            _logger.LogInformation("The incoming request is processed by Middleware ({requestpath}).",
                context.Request.Path);
            try
            {
                await _next(context); // Catches errors
            }
            catch (Exception ex)
            {
                _logger.LogError("An error ocurred during a request ({requestpath}). Details: {details}.",
                    context.Request.Path, ex.Message);
                await ExceptionHandlerAsync(context, ex);
            }
        }

        private async Task ExceptionHandlerAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = new ProblemDetails
            {
                Status = context.Response.StatusCode,
                Title = "An error ocurred while processing your request.",
                Detail = exception.Message,
                Instance = context.Request.Path
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
