using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;

namespace Enrich.Web.Filters
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RateLimitAttribute : ActionFilterAttribute
    {
        private readonly int _limit;
        private readonly int _periodInSeconds;

        public RateLimitAttribute(int limit, int periodInSeconds = 60)
        {
            _limit = limit;
            _periodInSeconds = periodInSeconds;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var cache = context.HttpContext.RequestServices.GetRequiredService<IMemoryCache>();
            var ipAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown-ip";

            var cacheKey = $"RateLimit_{ipAddress}_{context.ActionDescriptor.DisplayName}";

            if (cache.TryGetValue(cacheKey, out int requestCount))
            {
                if (requestCount >= _limit)
                {
                    context.Result = new RedirectToActionResult("RateLimitExceeded", "Home", null);
                    return;
                }

                cache.Set(cacheKey, requestCount + 1, TimeSpan.FromSeconds(_periodInSeconds));
            }
            else
            {
                cache.Set(cacheKey, 1, TimeSpan.FromSeconds(_periodInSeconds));
            }

            await next();
        }
    }
}
