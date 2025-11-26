using Azure;
using Azure.Data.Tables;
using MediatR;
using PoOmad.Api.Infrastructure.TableStorage;
using PoOmad.Shared.DTOs;

namespace PoOmad.Api.Features.DailyLogs;

/// <summary>
/// Command to log or update a day's entry
/// </summary>
public record LogDayCommand(
    string GoogleId,
    DateTime Date,
    bool OmadCompliant,
    bool AlcoholConsumed,
    decimal? Weight,
    bool ConfirmWeightChange = false) : IRequest<DailyLogDto>;

public class LogDayHandler : IRequestHandler<LogDayCommand, DailyLogDto>
{
    private readonly TableStorageClient _tableStorage;
    private readonly ILogger<LogDayHandler> _logger;

    public LogDayHandler(TableStorageClient tableStorage, ILogger<LogDayHandler> logger)
    {
        _tableStorage = tableStorage;
        _logger = logger;
    }

    public async Task<DailyLogDto> Handle(LogDayCommand request, CancellationToken cancellationToken)
    {
        var tableClient = await _tableStorage.GetTableClientAsync("DailyLogs");

        // Validate 5 lb weight threshold if weight is provided
        if (request.Weight.HasValue && !request.ConfirmWeightChange)
        {
            await ValidateWeightThreshold(tableClient, request.GoogleId, request.Date, request.Weight.Value, cancellationToken);
        }

        var rowKey = request.Date.ToString("yyyy-MM-dd");
        var entity = new DailyLogEntry
        {
            PartitionKey = request.GoogleId,
            RowKey = rowKey,
            OmadCompliant = request.OmadCompliant,
            AlcoholConsumed = request.AlcoholConsumed,
            Weight = request.Weight,
            ServerTimestamp = DateTime.UtcNow
        };

        try
        {
            // Try to get existing entry to determine if update or insert
            var existing = await tableClient.GetEntityAsync<DailyLogEntry>(
                request.GoogleId,
                rowKey,
                cancellationToken: cancellationToken);

            entity.ETag = existing.Value.ETag;
            await tableClient.UpdateEntityAsync(entity, entity.ETag, cancellationToken: cancellationToken);
            _logger.LogInformation("Updated log for {GoogleId} on {Date}", request.GoogleId, request.Date);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            await tableClient.AddEntityAsync(entity, cancellationToken);
            _logger.LogInformation("Created log for {GoogleId} on {Date}", request.GoogleId, request.Date);
        }

        return new DailyLogDto
        {
            Date = request.Date,
            OmadCompliant = entity.OmadCompliant,
            AlcoholConsumed = entity.AlcoholConsumed,
            Weight = entity.Weight,
            ServerTimestamp = entity.ServerTimestamp
        };
    }

    private async Task ValidateWeightThreshold(
        TableClient tableClient,
        string googleId,
        DateTime currentDate,
        decimal currentWeight,
        CancellationToken cancellationToken)
    {
        // Get previous day's log to check weight difference
        var previousDate = currentDate.AddDays(-1);
        var previousRowKey = previousDate.ToString("yyyy-MM-dd");

        try
        {
            var previousEntry = await tableClient.GetEntityAsync<DailyLogEntry>(
                googleId,
                previousRowKey,
                cancellationToken: cancellationToken);

            if (previousEntry.Value.Weight.HasValue)
            {
                var weightDifference = Math.Abs(currentWeight - previousEntry.Value.Weight.Value);
                if (weightDifference > 5)
                {
                    throw new InvalidOperationException(
                        $"Weight change of {weightDifference:F1} lbs exceeds 5 lb threshold. Please confirm this is correct.");
                }
            }
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            // No previous entry found, allow any weight
            _logger.LogDebug("No previous weight entry found for validation");
        }
    }
}
