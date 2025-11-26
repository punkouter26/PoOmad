namespace PoOmad.Api.Infrastructure.Health;

public static class HealthCheckEndpoints
{
    public static void MapHealthChecks(this WebApplication app)
    {
        app.MapGet("/api/health", async (IServiceProvider sp) =>
        {
            var results = new Dictionary<string, object>
            {
                ["status"] = "healthy",
                ["timestamp"] = DateTime.UtcNow
            };

            // TODO: Add Table Storage connectivity check when TableStorageClient is implemented
            // For now, just return basic health status

            return Results.Ok(results);
        })
        .WithName("HealthCheck");
    }
}
