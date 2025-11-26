using Azure.Data.Tables;

namespace PoOmad.Api.Infrastructure.TableStorage;

/// <summary>
/// Wrapper for Azure Table Storage operations
/// </summary>
public class TableStorageClient
{
    private readonly TableServiceClient _serviceClient;

    public TableStorageClient(TableServiceClient serviceClient)
    {
        _serviceClient = serviceClient;
    }

    public async Task<TableClient> GetTableClientAsync(string tableName)
    {
        var tableClient = _serviceClient.GetTableClient(tableName);
        await tableClient.CreateIfNotExistsAsync();
        return tableClient;
    }

    public async Task<bool> HealthCheckAsync()
    {
        try
        {
            // Try to query account info to verify connectivity
            await _serviceClient.GetPropertiesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
