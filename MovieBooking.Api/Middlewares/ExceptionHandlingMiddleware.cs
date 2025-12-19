
using System.Net;
using System.Text.Json;

namespace MovieBooking.Api.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        { _next = next; _logger = logger; }
        public async Task InvokeAsync(HttpContext context)
        {
            try { await _next(context); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";
                var problem = new { title = "An unexpected error occurred", status = context.Response.StatusCode, detail = ex.Message, traceId = context.TraceIdentifier };
                await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
            }
        }
    }
}
