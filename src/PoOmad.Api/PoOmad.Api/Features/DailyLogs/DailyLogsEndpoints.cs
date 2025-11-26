using MediatR;
using PoOmad.Shared.DTOs;
using System.Security.Claims;

namespace PoOmad.Api.Features.DailyLogs;

public static class DailyLogsEndpoints
{
    public static void MapDailyLogsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/daily-logs")
            .RequireAuthorization();

        // POST /api/daily-logs - Create or update a daily log
        group.MapPost("/", async (DailyLogDto dto, IMediator mediator, ClaimsPrincipal user, bool? confirm) =>
        {
            var googleId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(googleId))
                return Results.Unauthorized();

            try
            {
                var command = new LogDayCommand(
                    googleId,
                    dto.Date,
                    dto.OmadCompliant,
                    dto.AlcoholConsumed,
                    dto.Weight,
                    confirm ?? false);

                var result = await mediator.Send(command);
                return Results.Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message, requiresConfirmation = true });
            }
        })
        .WithName("LogDay");

        // GET /api/daily-logs/{date} - Get specific day's log
        group.MapGet("/{date}", async (DateTime date, IMediator mediator, ClaimsPrincipal user) =>
        {
            var googleId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(googleId))
                return Results.Unauthorized();

            var query = new GetDayLogQuery(googleId, date);
            var result = await mediator.Send(query);
            return result != null ? Results.Ok(result) : Results.NotFound();
        })
        .WithName("GetDayLog");

        // GET /api/daily-logs/month/{year}/{month} - Get monthly logs
        group.MapGet("/month/{year}/{month}", async (int year, int month, IMediator mediator, ClaimsPrincipal user, HttpContext context) =>
        {
            var googleId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(googleId))
                return Results.Unauthorized();

            // Add response caching headers (5 minutes)
            context.Response.Headers["Cache-Control"] = "private, max-age=300";
            context.Response.Headers["Vary"] = "Cookie";

            var query = new GetMonthlyLogsQuery(googleId, year, month);
            var result = await mediator.Send(query);
            return Results.Ok(result);
        })
        .WithName("GetMonthlyLogs")
        .CacheOutput(policy => policy.Expire(TimeSpan.FromMinutes(5)));

        // GET /api/daily-logs/streak - Get current streak
        group.MapGet("/streak", async (IMediator mediator, ClaimsPrincipal user) =>
        {
            var googleId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(googleId))
                return Results.Unauthorized();

            var query = new CalculateStreakQuery(googleId);
            var result = await mediator.Send(query);
            return Results.Ok(new { streak = result });
        })
        .WithName("GetStreak");

        // DELETE /api/daily-logs/{date} - Delete specific day's log
        group.MapDelete("/{date}", async (DateTime date, IMediator mediator, ClaimsPrincipal user) =>
        {
            var googleId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(googleId))
                return Results.Unauthorized();

            var command = new DeleteDayLogCommand(googleId, date);
            var result = await mediator.Send(command);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteDayLog");
    }
}
