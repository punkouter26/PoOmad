using Azure;
using Azure.Data.Tables;

namespace PoOmad.Api.Features.DailyLogs;

/// <summary>
/// Daily log entry entity for Azure Table Storage
/// PartitionKey: GoogleId, RowKey: yyyy-MM-dd
/// </summary>
public class DailyLogEntry : ITableEntity
{
    public string PartitionKey { get; set; } = string.Empty; // GoogleId
    public string RowKey { get; set; } = string.Empty; // yyyy-MM-dd
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    // Log properties
    public bool OmadCompliant { get; set; }
    public bool AlcoholConsumed { get; set; }
    public double? Weight { get; set; }
    public DateTime ServerTimestamp { get; set; }
}
