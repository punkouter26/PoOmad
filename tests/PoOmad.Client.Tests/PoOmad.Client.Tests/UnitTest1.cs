using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using PoOmad.Client.Components;
using PoOmad.Client.Services;
using PoOmad.Shared.DTOs;

namespace PoOmad.Client.Tests;

/// <summary>
/// bUnit tests for StreakCounter component
/// </summary>
public class StreakCounterTests : TestContext
{
    private readonly ApiClient _mockApiClient;

    public StreakCounterTests()
    {
        _mockApiClient = Substitute.For<ApiClient>(new HttpClient());
        Services.AddSingleton(_mockApiClient);
    }

    [Fact]
    public void StreakCounter_InitiallyShowsLoading()
    {
        // Arrange - API never returns (simulates loading)
        _mockApiClient.GetStreakAsync().Returns(new TaskCompletionSource<int>().Task);

        // Act
        var cut = RenderComponent<StreakCounter>();

        // Assert
        cut.Markup.Should().Contain("Loading streak...");
    }

    [Fact]
    public async Task StreakCounter_DisplaysZeroStreak()
    {
        // Arrange
        _mockApiClient.GetStreakAsync().Returns(0);

        // Act
        var cut = RenderComponent<StreakCounter>();
        await Task.Delay(100); // Allow async render to complete
        cut.Render();

        // Assert
        cut.Find(".streak-number").TextContent.Should().Be("0");
        cut.Markup.Should().Contain("Days Streak");
    }

    [Fact]
    public async Task StreakCounter_DisplaysMultiDayStreak()
    {
        // Arrange
        _mockApiClient.GetStreakAsync().Returns(5);

        // Act
        var cut = RenderComponent<StreakCounter>();
        await Task.Delay(100);
        cut.Render();

        // Assert
        cut.Find(".streak-number").TextContent.Should().Be("5");
        cut.Markup.Should().Contain("Days Streak");
    }

    [Fact]
    public async Task StreakCounter_DisplaysSingularDayLabel()
    {
        // Arrange
        _mockApiClient.GetStreakAsync().Returns(1);

        // Act
        var cut = RenderComponent<StreakCounter>();
        await Task.Delay(100);
        cut.Render();

        // Assert
        cut.Find(".streak-number").TextContent.Should().Be("1");
        cut.Markup.Should().Contain("Day Streak"); // Singular
        cut.Markup.Should().NotContain("Days Streak");
    }

    [Fact]
    public async Task StreakCounter_DisplaysFireEmoji()
    {
        // Arrange
        _mockApiClient.GetStreakAsync().Returns(10);

        // Act
        var cut = RenderComponent<StreakCounter>();
        await Task.Delay(100);
        cut.Render();

        // Assert
        cut.Find(".streak-icon").TextContent.Should().Contain("🔥");
    }

    [Fact]
    public async Task StreakCounter_HandlesApiError_ShowsZero()
    {
        // Arrange
        _mockApiClient.GetStreakAsync().Returns<int>(x => throw new Exception("API error"));

        // Act
        var cut = RenderComponent<StreakCounter>();
        await Task.Delay(100);
        cut.Render();

        // Assert
        cut.Find(".streak-number").TextContent.Should().Be("0");
    }
}

/// <summary>
/// bUnit tests for CalendarGrid component
/// </summary>
public class CalendarGridTests : TestContext
{
    private readonly ApiClient _mockApiClient;

    public CalendarGridTests()
    {
        _mockApiClient = Substitute.For<ApiClient>(new HttpClient());
        Services.AddSingleton(_mockApiClient);
    }

    [Fact]
    public void CalendarGrid_RendersMonthHeader()
    {
        // Arrange
        _mockApiClient.GetMonthlyLogsAsync(Arg.Any<int>(), Arg.Any<int>())
            .Returns(new List<DailyLogDto>());

        // Act
        var cut = RenderComponent<CalendarGrid>();

        // Assert
        cut.Find(".calendar-header h2").Should().NotBeNull();
    }

    [Fact]
    public void CalendarGrid_RendersDayHeaders()
    {
        // Arrange
        _mockApiClient.GetMonthlyLogsAsync(Arg.Any<int>(), Arg.Any<int>())
            .Returns(new List<DailyLogDto>());

        // Act
        var cut = RenderComponent<CalendarGrid>();

        // Assert
        var dayHeaders = cut.FindAll(".day-header");
        dayHeaders.Count.Should().Be(7);
        dayHeaders[0].TextContent.Should().Be("Sun");
        dayHeaders[6].TextContent.Should().Be("Sat");
    }

    [Fact]
    public void CalendarGrid_HasNavigationButtons()
    {
        // Arrange
        _mockApiClient.GetMonthlyLogsAsync(Arg.Any<int>(), Arg.Any<int>())
            .Returns(new List<DailyLogDto>());

        // Act
        var cut = RenderComponent<CalendarGrid>();

        // Assert
        var navButtons = cut.FindAll(".btn-nav");
        navButtons.Count.Should().Be(2);
    }

    [Fact]
    public async Task CalendarGrid_DisplaysCompliantDay_GreenIndicator()
    {
        // Arrange
        var today = DateTime.Today;
        var logs = new List<DailyLogDto>
        {
            new() { Date = today, OmadCompliant = true, AlcoholConsumed = false, Weight = 175m }
        };
        _mockApiClient.GetMonthlyLogsAsync(today.Year, today.Month).Returns(logs);

        // Act
        var cut = RenderComponent<CalendarGrid>();
        await Task.Delay(100);
        cut.Render();

        // Assert
        cut.Markup.Should().Contain("compliant"); // CSS class for compliant days
    }

    [Fact]
    public async Task CalendarGrid_DisplaysNonCompliantDay_RedIndicator()
    {
        // Arrange
        var today = DateTime.Today;
        var logs = new List<DailyLogDto>
        {
            new() { Date = today, OmadCompliant = false, AlcoholConsumed = false, Weight = 175m }
        };
        _mockApiClient.GetMonthlyLogsAsync(today.Year, today.Month).Returns(logs);

        // Act
        var cut = RenderComponent<CalendarGrid>();
        await Task.Delay(100);
        cut.Render();

        // Assert
        cut.Markup.Should().Contain("non-compliant"); // CSS class for non-compliant days
    }

    [Fact]
    public async Task CalendarGrid_DisplaysWeight_WhenProvided()
    {
        // Arrange
        var today = DateTime.Today;
        var logs = new List<DailyLogDto>
        {
            new() { Date = today, OmadCompliant = true, AlcoholConsumed = false, Weight = 175.5m }
        };
        _mockApiClient.GetMonthlyLogsAsync(today.Year, today.Month).Returns(logs);

        // Act
        var cut = RenderComponent<CalendarGrid>();
        await Task.Delay(100);
        cut.Render();

        // Assert
        cut.Markup.Should().Contain("175.5");
        cut.Markup.Should().Contain("lbs");
    }

    [Fact]
    public async Task CalendarGrid_HighlightsToday()
    {
        // Arrange
        _mockApiClient.GetMonthlyLogsAsync(Arg.Any<int>(), Arg.Any<int>())
            .Returns(new List<DailyLogDto>());

        // Act
        var cut = RenderComponent<CalendarGrid>();
        await Task.Delay(100);
        cut.Render();

        // Assert
        var todayCell = cut.FindAll(".day-cell.today");
        todayCell.Count.Should().BeGreaterThan(0);
    }
}
