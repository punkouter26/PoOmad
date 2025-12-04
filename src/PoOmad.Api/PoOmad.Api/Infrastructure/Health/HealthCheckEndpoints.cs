using PoOmad.Api.Infrastructure.TableStorage;

namespace PoOmad.Api.Infrastructure.Health;

public static class HealthCheckEndpoints
{
    public static void MapHealthChecks(this WebApplication app)
    {
        app.MapGet("/api/health", async (TableStorageClient tableStorageClient) =>
        {
            var tableStorageHealthy = await tableStorageClient.HealthCheckAsync();
            
            var results = new Dictionary<string, object>
            {
                ["status"] = tableStorageHealthy ? "healthy" : "degraded",
                ["timestamp"] = DateTime.UtcNow,
                ["checks"] = new Dictionary<string, object>
                {
                    ["tableStorage"] = new Dictionary<string, object>
                    {
                        ["status"] = tableStorageHealthy ? "healthy" : "unhealthy",
                        ["description"] = tableStorageHealthy 
                            ? "Azure Table Storage is accessible" 
                            : "Azure Table Storage connectivity failed"
                    }
                }
            };

            return tableStorageHealthy 
                ? Results.Ok(results) 
                : Results.Json(results, statusCode: 503);
        })
        .WithName("HealthCheck")
        .WithTags("Health");
    }
}
