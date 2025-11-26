using Azure;
using MediatR;
using PoOmad.Api.Infrastructure.TableStorage;
using PoOmad.Shared.DTOs;

namespace PoOmad.Api.Features.DailyLogs;

/// <summary>
/// Query to get a specific day's log
/// </summary>
public record GetDayLogQuery(string GoogleId, DateTime Date) : IRequest<DailyLogDto?>;

public class GetDayLogHandler : IRequestHandler<GetDayLogQuery, DailyLogDto?>
{
    private readonly TableStorageClient _tableStorage;

    public GetDayLogHandler(TableStorageClient tableStorage)
    {
        _tableStorage = tableStorage;
    }

    public async Task<DailyLogDto?> Handle(GetDayLogQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var tableClient = await _tableStorage.GetTableClientAsync("DailyLogs");
            var rowKey = request.Date.ToString("yyyy-MM-dd");

            var response = await tableClient.GetEntityAsync<DailyLogEntry>(
                request.GoogleId,
                rowKey,
                cancellationToken: cancellationToken);

            var entity = response.Value;

            return new DailyLogDto
            {
                Date = DateTime.Parse(entity.RowKey),
                OmadCompliant = entity.OmadCompliant,
                AlcoholConsumed = entity.AlcoholConsumed,
                Weight = entity.Weight,
                ServerTimestamp = entity.ServerTimestamp
            };
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }
}
