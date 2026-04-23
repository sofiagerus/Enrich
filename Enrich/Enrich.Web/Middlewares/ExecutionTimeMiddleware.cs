using System.Diagnostics;

namespace Enrich.Web.Middlewares
{
    public class ExecutionTimeMiddleware(RequestDelegate next, ILogger<ExecutionTimeMiddleware> logger)
    {
        private readonly RequestDelegate _next = next;
        private readonly ILogger<ExecutionTimeMiddleware> _logger = logger;

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            await _next(context);

            stopwatch.Stop();
            _logger.LogInformation(
                "Request {Method} {Url} executed in {ElapsedMilliseconds} ms",
                context.Request.Method,
                context.Request.Path,
                stopwatch.ElapsedMilliseconds);
        }
    }
}
