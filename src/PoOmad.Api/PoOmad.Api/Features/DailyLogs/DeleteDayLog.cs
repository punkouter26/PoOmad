using Azure;
using MediatR;
using PoOmad.Api.Infrastructure.TableStorage;

namespace PoOmad.Api.Features.DailyLogs;

/// <summary>
/// Command to delete a specific day's log entry
/// </summary>
public record DeleteDayLogCommand(string GoogleId, DateTime Date) : IRequest<bool>;

public class DeleteDayLogHandler : IRequestHandler<DeleteDayLogCommand, bool>
{
    private readonly TableStorageClient _tableStorage;
    private readonly ILogger<DeleteDayLogHandler> _logger;

    public DeleteDayLogHandler(TableStorageClient tableStorage, ILogger<DeleteDayLogHandler> logger)
    {
        _tableStorage = tableStorage;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteDayLogCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var tableClient = await _tableStorage.GetTableClientAsync("DailyLogs");
            var rowKey = request.Date.ToString("yyyy-MM-dd");

            await tableClient.DeleteEntityAsync(request.GoogleId, rowKey, cancellationToken: cancellationToken);

            _logger.LogInformation("Deleted log for {GoogleId} on {Date}", request.GoogleId, request.Date);
            return true;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogWarning("Log not found for {GoogleId} on {Date}", request.GoogleId, request.Date);
            return false;
        }
    }
}
