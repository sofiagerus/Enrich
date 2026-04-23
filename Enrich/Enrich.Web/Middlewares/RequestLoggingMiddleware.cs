using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace Enrich.Web.Middlewares
{
    public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        private static readonly HashSet<string> SensitiveHeaders = new(StringComparer.OrdinalIgnoreCase)
        {
            "Authorization",
            "Cookie",
            "Set-Cookie",
            "X-Api-Key",
            "X-Csrf-Token"
        };

        private static readonly string[] SensitivePaths = { "/login", "/register", "/password", "/auth", "/account" };

        private readonly RequestDelegate _next = next;
        private readonly ILogger<RequestLoggingMiddleware> _logger = logger;

        private static string MaskSensitiveJsonFields(string body)
        {
            if (string.IsNullOrWhiteSpace(body))
            {
                return body;
            }

            var regex = new Regex("(\"(password|token|secret|newPassword|confirmPassword)\"\\s*:\\s*\")[^\"]+(\")", RegexOptions.IgnoreCase);
            var formRegex = new Regex("((password|token|secret|newPassword|confirmPassword)=)[^&]+", RegexOptions.IgnoreCase);

            var maskedBody = regex.Replace(body, "$1***REDACTED***$3");
            maskedBody = formRegex.Replace(maskedBody, "$1***REDACTED***");

            return maskedBody;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            context.Request.EnableBuffering();

            var method = context.Request.Method;
            var path = context.Request.Path.ToString().ToLower();
            var url = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}";
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown IP";

            var headers = string.Join(", ", context.Request.Headers.Select(h =>
                $"{h.Key}: {(SensitiveHeaders.Contains(h.Key) ? "***REDACTED***" : h.Value.ToString())}"));

            string bodyAsText;

            if (SensitivePaths.Any(path.Contains))
            {
                bodyAsText = "*** REDACTED SENSITIVE DATA ***";
            }
            else
            {
                bodyAsText = await ReadRequestBodyAsync(context.Request);
                bodyAsText = MaskSensitiveJsonFields(bodyAsText);
            }

            var userId = "Anonymous";
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "UnknownId";
            }

            _logger.LogInformation(
                "Request Info: Method: {Method}, URL: {Url}, IP: {Ip}, User ID: {UserId}, Headers: [{Headers}], Body: {Body}",
                method, url, ip, userId, headers, bodyAsText);

            await _next(context);
        }

        private async Task<string> ReadRequestBodyAsync(HttpRequest request)
        {
            if (!request.Body.CanSeek)
            {
                return "Cannot seek body";
            }

            request.Body.Position = 0;
            using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync();

            request.Body.Position = 0;

            return string.IsNullOrWhiteSpace(body) ? "Empty" : body;
        }
    }
}
