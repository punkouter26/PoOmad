using Azure.Data.Tables;
using MediatR;
using PoOmad.Api.Features.DailyLogs;
using PoOmad.Api.Infrastructure.TableStorage;
using PoOmad.Shared.DTOs;

namespace PoOmad.Api.Features.Analytics;

public record GetCorrelationQuery(string GoogleId, DateTime StartDate, DateTime EndDate) : IRequest<CorrelationDto>;

public class GetCorrelationHandler : IRequestHandler<GetCorrelationQuery, CorrelationDto>
{
    private readonly TableStorageClient _tableStorage;

    public GetCorrelationHandler(TableStorageClient tableStorage)
    {
        _tableStorage = tableStorage;
    }

    public async Task<CorrelationDto> Handle(GetCorrelationQuery request, CancellationToken cancellationToken)
    {
        var tableClient = await _tableStorage.GetTableClientAsync("DailyLogs");
        
        // Query all logs with weight in the date range
        var filter = $"PartitionKey eq '{request.GoogleId}' and RowKey ge '{request.StartDate:yyyy-MM-dd}' and RowKey le '{request.EndDate:yyyy-MM-dd}'";
        var logsWithWeight = new List<DailyLogEntry>();

        await foreach (var entry in tableClient.QueryAsync<DailyLogEntry>(filter, cancellationToken: cancellationToken))
        {
            if (entry.Weight.HasValue)
            {
                logsWithWeight.Add(entry);
            }
        }

        if (logsWithWeight.Count < 2)
        {
            // Not enough data for correlation
            return new CorrelationDto(
                Correlation: null,
                DaysWithAlcohol: 0,
                TotalDays: logsWithWeight.Count,
                AverageWeightWithAlcohol: null,
                AverageWeightWithoutAlcohol: null
            );
        }

        var daysWithAlcohol = logsWithWeight.Where(l => l.AlcoholConsumed).ToList();
        var daysWithoutAlcohol = logsWithWeight.Where(l => !l.AlcoholConsumed).ToList();

        var avgWeightWithAlcohol = daysWithAlcohol.Any()
            ? daysWithAlcohol.Average(l => l.Weight!.Value)
            : (decimal?)null;

        var avgWeightWithoutAlcohol = daysWithoutAlcohol.Any()
            ? daysWithoutAlcohol.Average(l => l.Weight!.Value)
            : (decimal?)null;

        // Calculate Pearson correlation coefficient
        double? correlation = null;
        if (logsWithWeight.Count >= 2)
        {
            var n = logsWithWeight.Count;
            var alcoholValues = logsWithWeight.Select(l => l.AlcoholConsumed ? 1.0 : 0.0).ToList();
            var weightValues = logsWithWeight.Select(l => (double)l.Weight!.Value).ToList();

            var meanAlcohol = alcoholValues.Average();
            var meanWeight = weightValues.Average();

            var sumProduct = 0.0;
            var sumAlcoholSq = 0.0;
            var sumWeightSq = 0.0;

            for (int i = 0; i < n; i++)
            {
                var alcoholDiff = alcoholValues[i] - meanAlcohol;
                var weightDiff = weightValues[i] - meanWeight;

                sumProduct += alcoholDiff * weightDiff;
                sumAlcoholSq += alcoholDiff * alcoholDiff;
                sumWeightSq += weightDiff * weightDiff;
            }

            var denominator = Math.Sqrt(sumAlcoholSq * sumWeightSq);
            correlation = denominator > 0 ? sumProduct / denominator : 0;
        }

        return new CorrelationDto(
            Correlation: correlation,
            DaysWithAlcohol: daysWithAlcohol.Count,
            TotalDays: logsWithWeight.Count,
            AverageWeightWithAlcohol: avgWeightWithAlcohol,
            AverageWeightWithoutAlcohol: avgWeightWithoutAlcohol
        );
    }
}
