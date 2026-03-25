using Microsoft.AspNetCore.Diagnostics;

namespace Enrich.Web.Handlers
{
    public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            logger.LogError(exception, "Необроблена помилка під час обробки запиту {Method} {Path}",
                httpContext.Request.Method, httpContext.Request.Path);

            httpContext.Response.StatusCode = 500;
            httpContext.Response.Redirect("/Home/Error");

            return true;
        }
    }
}
