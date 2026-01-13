using System.Collections.Concurrent;
using System.Net;

namespace MySecrets.API.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly ConcurrentDictionary<string, RateLimitInfo> _requestCounts = new();
    
    // Rate limit configuration
    private const int MaxRequestsPerWindow = 100;  // General requests
    private const int MaxAuthRequestsPerWindow = 5; // Auth requests (stricter)
    private const int WindowSizeSeconds = 60;

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientIp = GetClientIpAddress(context);
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";
        
        // Stricter limits for auth endpoints
        var isAuthEndpoint = path.Contains("/api/auth/login") || 
                            path.Contains("/api/auth/register");
        
        var maxRequests = isAuthEndpoint ? MaxAuthRequestsPerWindow : MaxRequestsPerWindow;
        var key = isAuthEndpoint ? $"auth:{clientIp}" : $"general:{clientIp}";

        var rateLimitInfo = _requestCounts.GetOrAdd(key, _ => new RateLimitInfo());

        lock (rateLimitInfo)
        {
            var now = DateTime.UtcNow;
            
            // Reset window if expired
            if (now - rateLimitInfo.WindowStart > TimeSpan.FromSeconds(WindowSizeSeconds))
            {
                rateLimitInfo.WindowStart = now;
                rateLimitInfo.RequestCount = 0;
            }

            rateLimitInfo.RequestCount++;

            // Add rate limit headers
            var remaining = Math.Max(0, maxRequests - rateLimitInfo.RequestCount);
            var resetTime = rateLimitInfo.WindowStart.AddSeconds(WindowSizeSeconds);
            
            context.Response.Headers["X-RateLimit-Limit"] = maxRequests.ToString();
            context.Response.Headers["X-RateLimit-Remaining"] = remaining.ToString();
            context.Response.Headers["X-RateLimit-Reset"] = ((DateTimeOffset)resetTime).ToUnixTimeSeconds().ToString();

            if (rateLimitInfo.RequestCount > maxRequests)
            {
                _logger.LogWarning(
                    "Rate limit exceeded for {ClientIp} on {Path}. Requests: {Count}/{Max}",
                    clientIp, path, rateLimitInfo.RequestCount, maxRequests);

                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                context.Response.Headers["Retry-After"] = 
                    ((int)(resetTime - now).TotalSeconds).ToString();
                
                return;
            }
        }

        await _next(context);
        
        // Periodic cleanup of old entries
        if (DateTime.UtcNow.Second == 0) // Once per minute
        {
            CleanupOldEntries();
        }
    }

    private void CleanupOldEntries()
    {
        var cutoff = DateTime.UtcNow.AddSeconds(-WindowSizeSeconds * 2);
        var keysToRemove = _requestCounts
            .Where(kvp => kvp.Value.WindowStart < cutoff)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in keysToRemove)
        {
            _requestCounts.TryRemove(key, out _);
        }
    }

    private static string GetClientIpAddress(HttpContext context)
    {
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',').First().Trim();
        }
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private class RateLimitInfo
    {
        public DateTime WindowStart { get; set; } = DateTime.UtcNow;
        public int RequestCount { get; set; }
    }
}

public static class RateLimitingMiddlewareExtensions
{
    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RateLimitingMiddleware>();
    }
}
