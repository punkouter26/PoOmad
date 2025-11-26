using MediatR;
using PoOmad.Api.Infrastructure.TableStorage;
using PoOmad.Shared.DTOs;

namespace PoOmad.Api.Features.DailyLogs;

/// <summary>
/// Query to get all logs for a specific month
/// </summary>
public record GetMonthlyLogsQuery(string GoogleId, int Year, int Month) : IRequest<List<DailyLogDto>>;

public class GetMonthlyLogsHandler : IRequestHandler<GetMonthlyLogsQuery, List<DailyLogDto>>
{
    private readonly TableStorageClient _tableStorage;

    public GetMonthlyLogsHandler(TableStorageClient tableStorage)
    {
        _tableStorage = tableStorage;
    }

    public async Task<List<DailyLogDto>> Handle(GetMonthlyLogsQuery request, CancellationToken cancellationToken)
    {
        var tableClient = await _tableStorage.GetTableClientAsync("DailyLogs");

        var startDate = new DateTime(request.Year, request.Month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var startRowKey = startDate.ToString("yyyy-MM-dd");
        var endRowKey = endDate.ToString("yyyy-MM-dd");

        var filter = $"PartitionKey eq '{request.GoogleId}' and RowKey ge '{startRowKey}' and RowKey le '{endRowKey}'";

        var results = new List<DailyLogDto>();

        await foreach (var entity in tableClient.QueryAsync<DailyLogEntry>(filter, cancellationToken: cancellationToken))
        {
            results.Add(new DailyLogDto
            {
                Date = DateTime.Parse(entity.RowKey),
                OmadCompliant = entity.OmadCompliant,
                AlcoholConsumed = entity.AlcoholConsumed,
                Weight = entity.Weight,
                ServerTimestamp = entity.ServerTimestamp
            });
        }

        return results.OrderBy(x => x.Date).ToList();
    }
}
