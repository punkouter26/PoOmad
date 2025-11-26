using Azure;
using Azure.Data.Tables;

namespace PoOmad.Api.Features.Profile;

/// <summary>
/// User profile entity for Azure Table Storage
/// PartitionKey: GoogleId, RowKey: "profile"
/// </summary>
public class UserProfile : ITableEntity
{
    public string PartitionKey { get; set; } = string.Empty; // GoogleId
    public string RowKey { get; set; } = "profile";
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    // Profile properties
    public string Email { get; set; } = string.Empty;
    public string Height { get; set; } = string.Empty;
    public decimal StartingWeight { get; set; }
    public DateTime StartDate { get; set; }
}
