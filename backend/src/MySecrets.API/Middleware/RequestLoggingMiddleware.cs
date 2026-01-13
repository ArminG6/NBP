using System.Diagnostics;

namespace MySecrets.API.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var requestId = Guid.NewGuid().ToString("N")[..8];
        var stopwatch = Stopwatch.StartNew();

        // Add request ID to response headers for debugging
        context.Response.Headers["X-Request-ID"] = requestId;

        var method = context.Request.Method;
        var path = context.Request.Path;
        var ipAddress = GetClientIpAddress(context);

        _logger.LogInformation(
            "[{RequestId}] {Method} {Path} started from {IpAddress}",
            requestId, method, path, ipAddress);

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            var statusCode = context.Response.StatusCode;
            var elapsed = stopwatch.ElapsedMilliseconds;

            if (statusCode >= 500)
            {
                _logger.LogError(
                    "[{RequestId}] {Method} {Path} completed with {StatusCode} in {Elapsed}ms",
                    requestId, method, path, statusCode, elapsed);
            }
            else if (statusCode >= 400)
            {
                _logger.LogWarning(
                    "[{RequestId}] {Method} {Path} completed with {StatusCode} in {Elapsed}ms",
                    requestId, method, path, statusCode, elapsed);
            }
            else
            {
                _logger.LogInformation(
                    "[{RequestId}] {Method} {Path} completed with {StatusCode} in {Elapsed}ms",
                    requestId, method, path, statusCode, elapsed);
            }
        }
    }

    private static string GetClientIpAddress(HttpContext context)
    {
        // Check for forwarded headers (when behind a proxy/load balancer)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',').First().Trim();
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}

public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestLoggingMiddleware>();
    }
}
