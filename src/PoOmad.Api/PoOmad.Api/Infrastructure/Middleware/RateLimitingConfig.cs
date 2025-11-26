using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using PoOmad.Shared.DTOs;

namespace PoOmad.Api.Infrastructure.Middleware;

/// <summary>
/// Rate limiting configuration for API endpoints
/// </summary>
public static class RateLimitingConfig
{
    public const string AuthPolicy = "auth";
    public const string ApiPolicy = "api";

    /// <summary>
    /// Adds rate limiting services to the DI container
    /// </summary>
    public static IServiceCollection AddRateLimitingPolicies(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            // Global rejection handler with RFC 7807 response
            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/problem+json";

                var retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfterValue)
                    ? retryAfterValue.TotalSeconds.ToString("F0")
                    : "60";

                context.HttpContext.Response.Headers["Retry-After"] = retryAfter;

                var problemDetails = new ProblemDetailsDto
                {
                    Status = StatusCodes.Status429TooManyRequests,
                    Title = "Too Many Requests",
                    Type = "https://tools.ietf.org/html/rfc6585#section-4",
                    Detail = $"Rate limit exceeded. Please retry after {retryAfter} seconds.",
                    Instance = context.HttpContext.Request.Path
                };

                await context.HttpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            };

            // Authentication endpoints: Stricter limit (5 requests per minute per IP)
            // Prevents brute force attacks on OAuth flow
            options.AddPolicy(AuthPolicy, context =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 2,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0 // No queuing - immediate rejection
                    }));

            // General API endpoints: More permissive (100 requests per minute per user)
            options.AddPolicy(ApiPolicy, context =>
            {
                // Use authenticated user ID if available, otherwise fall back to IP
                var userId = context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var partitionKey = userId ?? context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                return RateLimitPartition.GetTokenBucketLimiter(
                    partitionKey: partitionKey,
                    factory: _ => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = 100,
                        ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                        TokensPerPeriod = 100,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 10
                    });
            });
        });

        return services;
    }

    /// <summary>
    /// Adds rate limiting middleware to the application pipeline
    /// </summary>
    public static IApplicationBuilder UseRateLimitingPolicies(this IApplicationBuilder app)
    {
        return app.UseRateLimiter();
    }
}
