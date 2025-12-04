using Azure;
using Azure.Data.Tables;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PoOmad.Api.Features.DailyLogs;
using PoOmad.Api.Infrastructure.TableStorage;

namespace PoOmad.Api.Tests;

/// <summary>
/// Unit tests for CalculateStreakHandler
/// Tests the streak calculation logic where unlogged days do NOT break streak
/// </summary>
public class CalculateStreakHandlerTests
{
    private readonly TableStorageClient _tableStorageClient;
    private readonly TableClient _tableClient;
    private readonly CalculateStreakHandler _handler;

    public CalculateStreakHandlerTests()
    {
        _tableClient = Substitute.For<TableClient>();
        _tableStorageClient = Substitute.For<TableStorageClient>(Substitute.For<TableServiceClient>());
        _tableStorageClient.GetTableClientAsync("DailyLogs").Returns(_tableClient);
        _handler = new CalculateStreakHandler(_tableStorageClient);
    }

    [Fact]
    public async Task Handle_NoLogs_ReturnsZeroStreak()
    {
        // Arrange
        var query = new CalculateStreakQuery("google-123");
        var emptyLogs = AsyncPageable<DailyLogEntry>.FromPages(Array.Empty<Page<DailyLogEntry>>());
        _tableClient.QueryAsync<DailyLogEntry>(Arg.Any<string>(), cancellationToken: Arg.Any<CancellationToken>())
            .Returns(emptyLogs);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task Handle_AllCompliantDays_ReturnsCorrectStreak()
    {
        // Arrange
        var query = new CalculateStreakQuery("google-123");
        var logs = new List<DailyLogEntry>
        {
            new() { PartitionKey = "google-123", RowKey = "2024-01-03", OmadCompliant = true },
            new() { PartitionKey = "google-123", RowKey = "2024-01-02", OmadCompliant = true },
            new() { PartitionKey = "google-123", RowKey = "2024-01-01", OmadCompliant = true }
        };
        SetupMockQuery(logs);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().Be(3);
    }

    [Fact]
    public async Task Handle_NonCompliantDayBreaksStreak_ReturnsStreakBeforeBreak()
    {
        // Arrange
        var query = new CalculateStreakQuery("google-123");
        var logs = new List<DailyLogEntry>
        {
            new() { PartitionKey = "google-123", RowKey = "2024-01-05", OmadCompliant = true },
            new() { PartitionKey = "google-123", RowKey = "2024-01-04", OmadCompliant = true },
            new() { PartitionKey = "google-123", RowKey = "2024-01-03", OmadCompliant = false }, // Breaks streak
            new() { PartitionKey = "google-123", RowKey = "2024-01-02", OmadCompliant = true },
            new() { PartitionKey = "google-123", RowKey = "2024-01-01", OmadCompliant = true }
        };
        SetupMockQuery(logs);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().Be(2); // Only counts Jan 5 and Jan 4
    }

    [Fact]
    public async Task Handle_MostRecentDayNonCompliant_ReturnsZero()
    {
        // Arrange
        var query = new CalculateStreakQuery("google-123");
        var logs = new List<DailyLogEntry>
        {
            new() { PartitionKey = "google-123", RowKey = "2024-01-03", OmadCompliant = false },
            new() { PartitionKey = "google-123", RowKey = "2024-01-02", OmadCompliant = true },
            new() { PartitionKey = "google-123", RowKey = "2024-01-01", OmadCompliant = true }
        };
        SetupMockQuery(logs);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().Be(0);
    }

    private void SetupMockQuery(List<DailyLogEntry> logs)
    {
        var asyncEnumerable = logs.ToAsyncEnumerable();
        _tableClient.QueryAsync<DailyLogEntry>(Arg.Any<string>(), cancellationToken: Arg.Any<CancellationToken>())
            .Returns(asyncEnumerable);
    }
}

/// <summary>
/// Unit tests for GetDayLogHandler
/// </summary>
public class GetDayLogHandlerTests
{
    private readonly TableStorageClient _tableStorageClient;
    private readonly TableClient _tableClient;
    private readonly GetDayLogHandler _handler;

    public GetDayLogHandlerTests()
    {
        _tableClient = Substitute.For<TableClient>();
        _tableStorageClient = Substitute.For<TableStorageClient>(Substitute.For<TableServiceClient>());
        _tableStorageClient.GetTableClientAsync("DailyLogs").Returns(_tableClient);
        _handler = new GetDayLogHandler(_tableStorageClient);
    }

    [Fact]
    public async Task Handle_ExistingLog_ReturnsDto()
    {
        // Arrange
        var date = new DateTime(2024, 1, 15);
        var query = new GetDayLogQuery("google-123", date);
        
        var entity = new DailyLogEntry
        {
            PartitionKey = "google-123",
            RowKey = "2024-01-15",
            OmadCompliant = true,
            AlcoholConsumed = false,
            Weight = 175.5,
            ServerTimestamp = DateTime.UtcNow
        };

        _tableClient.GetEntityAsync<DailyLogEntry>("google-123", "2024-01-15", cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Response.FromValue(entity, Substitute.For<Response>()));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.OmadCompliant.Should().BeTrue();
        result.AlcoholConsumed.Should().BeFalse();
        result.Weight.Should().Be(175.5m);
        result.Date.Should().Be(date);
    }

    [Fact]
    public async Task Handle_NonExistingLog_ReturnsNull()
    {
        // Arrange
        var date = new DateTime(2024, 1, 15);
        var query = new GetDayLogQuery("google-123", date);

        _tableClient.GetEntityAsync<DailyLogEntry>("google-123", "2024-01-15", cancellationToken: Arg.Any<CancellationToken>())
            .Returns<Response<DailyLogEntry>>(x => throw new RequestFailedException(404, "Not found"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}

/// <summary>
/// Unit tests for LogDayHandler
/// </summary>
public class LogDayHandlerTests
{
    private readonly TableStorageClient _tableStorageClient;
    private readonly TableClient _tableClient;
    private readonly ILogger<LogDayHandler> _logger;
    private readonly LogDayHandler _handler;

    public LogDayHandlerTests()
    {
        _tableClient = Substitute.For<TableClient>();
        _tableStorageClient = Substitute.For<TableStorageClient>(Substitute.For<TableServiceClient>());
        _tableStorageClient.GetTableClientAsync("DailyLogs").Returns(_tableClient);
        _logger = Substitute.For<ILogger<LogDayHandler>>();
        _handler = new LogDayHandler(_tableStorageClient, _logger);
    }

    [Fact]
    public async Task Handle_NewLog_CreatesEntry()
    {
        // Arrange
        var date = new DateTime(2024, 1, 15);
        var command = new LogDayCommand("google-123", date, true, false, 175.5m, ConfirmWeightChange: true);

        _tableClient.GetEntityAsync<DailyLogEntry>("google-123", "2024-01-15", cancellationToken: Arg.Any<CancellationToken>())
            .Returns<Response<DailyLogEntry>>(x => throw new RequestFailedException(404, "Not found"));

        _tableClient.AddEntityAsync(Arg.Any<DailyLogEntry>(), Arg.Any<CancellationToken>())
            .Returns(Substitute.For<Response>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.OmadCompliant.Should().BeTrue();
        result.AlcoholConsumed.Should().BeFalse();
        result.Weight.Should().Be(175.5m);
        result.Date.Should().Be(date);

        await _tableClient.Received(1).AddEntityAsync(Arg.Any<DailyLogEntry>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ExistingLog_UpdatesEntry()
    {
        // Arrange
        var date = new DateTime(2024, 1, 15);
        var command = new LogDayCommand("google-123", date, true, true, 176.0m, ConfirmWeightChange: true);

        var existingEntity = new DailyLogEntry
        {
            PartitionKey = "google-123",
            RowKey = "2024-01-15",
            OmadCompliant = false,
            AlcoholConsumed = false,
            Weight = 175.5,
            ETag = new ETag("etag-value")
        };

        _tableClient.GetEntityAsync<DailyLogEntry>("google-123", "2024-01-15", cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Response.FromValue(existingEntity, Substitute.For<Response>()));

        _tableClient.UpdateEntityAsync(Arg.Any<DailyLogEntry>(), Arg.Any<ETag>(), cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Substitute.For<Response>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.OmadCompliant.Should().BeTrue();
        result.AlcoholConsumed.Should().BeTrue();
        result.Weight.Should().Be(176.0m);

        await _tableClient.Received(1).UpdateEntityAsync(Arg.Any<DailyLogEntry>(), Arg.Any<ETag>(), cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_LogWithoutWeight_Succeeds()
    {
        // Arrange
        var date = new DateTime(2024, 1, 15);
        var command = new LogDayCommand("google-123", date, true, false, null);

        _tableClient.GetEntityAsync<DailyLogEntry>("google-123", "2024-01-15", cancellationToken: Arg.Any<CancellationToken>())
            .Returns<Response<DailyLogEntry>>(x => throw new RequestFailedException(404, "Not found"));

        _tableClient.AddEntityAsync(Arg.Any<DailyLogEntry>(), Arg.Any<CancellationToken>())
            .Returns(Substitute.For<Response>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Weight.Should().BeNull();
    }
}

/// <summary>
/// Extension to convert IEnumerable to IAsyncEnumerable for testing
/// </summary>
internal static class AsyncEnumerableExtensions
{
    public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IEnumerable<T> source)
    {
        foreach (var item in source)
        {
            yield return item;
        }
        await Task.CompletedTask;
    }
}
