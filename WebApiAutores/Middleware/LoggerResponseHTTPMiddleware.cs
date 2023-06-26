using Microsoft.Extensions.Logging;

namespace WebApiAutores.Middleware
{
    public static class LoggerResponseHTTPMiddlewareExtensions
    {
        public static IApplicationBuilder UseLoggerResponseHTTP(this IApplicationBuilder app)
        {
            return app.UseMiddleware<LoggerResponseHTTPMiddleware>();
        }
    }
    public class LoggerResponseHTTPMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<LoggerResponseHTTPMiddleware> logger;

        public LoggerResponseHTTPMiddleware(RequestDelegate next,
            ILogger<LoggerResponseHTTPMiddleware> logger)
        {
            this.next = next;
            this.logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            using (var ms = new MemoryStream())
            {
                var originalBodyResponse = context.Response.Body;
                context.Response.Body = ms;

                await next(context);

                ms.Seek(0, SeekOrigin.Begin);
                string response = new StreamReader(ms).ReadToEnd();
                ms.Seek(0, SeekOrigin.Begin);

                await ms.CopyToAsync(originalBodyResponse);
                context.Response.Body = originalBodyResponse;

                logger.LogInformation(response);
            }
        }
    }
}
