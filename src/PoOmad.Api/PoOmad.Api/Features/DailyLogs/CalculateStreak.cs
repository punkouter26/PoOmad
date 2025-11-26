using MediatR;
using PoOmad.Api.Infrastructure.TableStorage;

namespace PoOmad.Api.Features.DailyLogs;

/// <summary>
/// Query to calculate current OMAD streak
/// Unlogged days do NOT break streak - only logged OmadCompliant=false breaks streak
/// </summary>
public record CalculateStreakQuery(string GoogleId) : IRequest<int>;

public class CalculateStreakHandler : IRequestHandler<CalculateStreakQuery, int>
{
    private readonly TableStorageClient _tableStorage;

    public CalculateStreakHandler(TableStorageClient tableStorage)
    {
        _tableStorage = tableStorage;
    }

    public async Task<int> Handle(CalculateStreakQuery request, CancellationToken cancellationToken)
    {
        var tableClient = await _tableStorage.GetTableClientAsync("DailyLogs");

        // Get all logs for this user, ordered by date descending
        var filter = $"PartitionKey eq '{request.GoogleId}'";
        var logs = new List<DailyLogEntry>();

        await foreach (var entity in tableClient.QueryAsync<DailyLogEntry>(filter, cancellationToken: cancellationToken))
        {
            logs.Add(entity);
        }

        if (logs.Count == 0)
            return 0;

        // Sort by date descending (most recent first)
        var sortedLogs = logs.OrderByDescending(x => DateTime.Parse(x.RowKey)).ToList();

        int streak = 0;

        // Count consecutive compliant days from most recent
        foreach (var log in sortedLogs)
        {
            if (log.OmadCompliant)
            {
                streak++;
            }
            else
            {
                // Logged as non-compliant - streak breaks
                break;
            }
        }

        return streak;
    }
}
