using PoOmad.Shared.DTOs;
using System.Net.Http.Json;

namespace PoOmad.Client.Services;

public class ApiClient
{
    private readonly HttpClient _httpClient;

    public ApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // Authentication
    public async Task<UserInfoDto?> GetCurrentUserAsync()
    {
        return await _httpClient.GetFromJsonAsync<UserInfoDto>("/api/auth/me");
    }

    // Profile
    public async Task<UserProfileDto?> GetProfileAsync()
    {
        var response = await _httpClient.GetAsync("/api/profile");
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<UserProfileDto>()
            : null;
    }

    public async Task<UserProfileDto?> CreateProfileAsync(UserProfileDto profile)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/profile", profile);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<UserProfileDto>();
    }

    public async Task<UserProfileDto?> UpdateProfileAsync(UserProfileDto profile)
    {
        var response = await _httpClient.PutAsJsonAsync("/api/profile", profile);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<UserProfileDto>();
    }

    public async Task SignOutAsync()
    {
        await _httpClient.PostAsync("/api/auth/signout", null);
    }

    // Daily Logs
    public async Task<DailyLogDto?> GetDayLogAsync(DateTime date)
    {
        var response = await _httpClient.GetAsync($"/api/daily-logs/{date:yyyy-MM-dd}");
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<DailyLogDto>()
            : null;
    }

    public async Task<List<DailyLogDto>> GetMonthlyLogsAsync(int year, int month)
    {
        return await _httpClient.GetFromJsonAsync<List<DailyLogDto>>($"/api/daily-logs/month/{year}/{month}")
            ?? new List<DailyLogDto>();
    }

    public async Task<int> GetStreakAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<dynamic>("/api/daily-logs/streak");
        return response?.streak ?? 0;
    }

    public async Task<(bool success, string? error)> LogDayAsync(DailyLogDto log, bool confirm = false)
    {
        var url = confirm ? "/api/daily-logs?confirm=true" : "/api/daily-logs";
        var response = await _httpClient.PostAsJsonAsync(url, log);

        if (response.IsSuccessStatusCode)
            return (true, null);

        if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            var error = await response.Content.ReadFromJsonAsync<dynamic>();
            return (false, error?.error?.ToString());
        }

        return (false, "Failed to log day");
    }

    public async Task<bool> DeleteDayLogAsync(DateTime date)
    {
        var response = await _httpClient.DeleteAsync($"/api/daily-logs/{date:yyyy-MM-dd}");
        return response.IsSuccessStatusCode;
    }

    // Analytics methods
    public async Task<TrendsResponseDto?> GetTrendsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var url = "/api/analytics/trends";
        var queryParams = new List<string>();

        if (startDate.HasValue)
            queryParams.Add($"startDate={startDate.Value:yyyy-MM-dd}");
        if (endDate.HasValue)
            queryParams.Add($"endDate={endDate.Value:yyyy-MM-dd}");

        if (queryParams.Any())
            url += "?" + string.Join("&", queryParams);

        var response = await _httpClient.GetAsync(url);

        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<TrendsResponseDto>();

        return null;
    }

    public async Task<CorrelationDto?> GetCorrelationAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var url = "/api/analytics/correlation";
        var queryParams = new List<string>();

        if (startDate.HasValue)
            queryParams.Add($"startDate={startDate.Value:yyyy-MM-dd}");
        if (endDate.HasValue)
            queryParams.Add($"endDate={endDate.Value:yyyy-MM-dd}");

        if (queryParams.Any())
            url += "?" + string.Join("&", queryParams);

        var response = await _httpClient.GetAsync(url);

        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<CorrelationDto>();

        return null;
    }
}
