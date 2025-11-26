using Azure;
using Azure.Data.Tables;
using MediatR;
using PoOmad.Api.Features.DailyLogs;
using PoOmad.Api.Infrastructure.TableStorage;
using PoOmad.Shared.DTOs;

namespace PoOmad.Api.Features.Analytics;

public record GetTrendsQuery(string GoogleId, DateTime StartDate, DateTime EndDate) : IRequest<TrendsResponseDto>;

public class GetTrendsHandler : IRequestHandler<GetTrendsQuery, TrendsResponseDto>
{
    private readonly TableStorageClient _tableStorage;

    public GetTrendsHandler(TableStorageClient tableStorage)
    {
        _tableStorage = tableStorage;
    }

    public async Task<TrendsResponseDto> Handle(GetTrendsQuery request, CancellationToken cancellationToken)
    {
        var tableClient = await _tableStorage.GetTableClientAsync("DailyLogs");
        
        // Query all logs in the date range
        var filter = $"PartitionKey eq '{request.GoogleId}' and RowKey ge '{request.StartDate:yyyy-MM-dd}' and RowKey le '{request.EndDate:yyyy-MM-dd}'";
        var logs = new List<DailyLogEntry>();

        await foreach (var entry in tableClient.QueryAsync<DailyLogEntry>(filter, cancellationToken: cancellationToken))
        {
            logs.Add(entry);
        }

        // Sort by date
        logs = logs.OrderBy(l => l.RowKey).ToList();

        // Validate minimum data requirement
        if (logs.Count < 3)
        {
            throw new InvalidOperationException("At least 3 days of logged data are required to generate trends.");
        }

        // Validate first logged day has weight
        if (!logs.First().Weight.HasValue)
        {
            throw new InvalidOperationException("Weight is required on the first logged day to generate trends.");
        }

        // Generate data points with gap filling
        var dataPoints = new List<TrendDataPointDto>();
        decimal? lastKnownWeight = null;

        for (var date = request.StartDate; date <= request.EndDate; date = date.AddDays(1))
        {
            var log = logs.FirstOrDefault(l => l.RowKey == date.ToString("yyyy-MM-dd"));

            if (log != null)
            {
                // Actual logged data
                var weight = log.Weight ?? lastKnownWeight; // Use logged weight or carry forward
                dataPoints.Add(new TrendDataPointDto(
                    date,
                    weight,
                    log.AlcoholConsumed,
                    IsCarryForward: !log.Weight.HasValue && lastKnownWeight.HasValue
                ));

                if (log.Weight.HasValue)
                {
                    lastKnownWeight = log.Weight.Value;
                }
            }
            else if (lastKnownWeight.HasValue)
            {
                // No log for this day - carry forward weight
                dataPoints.Add(new TrendDataPointDto(
                    date,
                    lastKnownWeight,
                    AlcoholConsumed: false,
                    IsCarryForward: true
                ));
            }
        }

        // Calculate weight change
        var firstWeight = logs.FirstOrDefault(l => l.Weight.HasValue)?.Weight;
        var lastWeight = logs.LastOrDefault(l => l.Weight.HasValue)?.Weight;
        var weightChange = (firstWeight.HasValue && lastWeight.HasValue)
            ? (decimal?)(lastWeight.Value - firstWeight.Value)
            : null;

        return new TrendsResponseDto(
            DataPoints: dataPoints,
            TotalDaysLogged: logs.Count,
            WeightChange: weightChange
        );
    }
}
