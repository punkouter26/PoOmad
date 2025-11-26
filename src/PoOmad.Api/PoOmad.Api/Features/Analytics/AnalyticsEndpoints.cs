using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace PoOmad.Api.Features.Analytics;

public static class AnalyticsEndpoints
{
    public static RouteGroupBuilder MapAnalyticsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/analytics").RequireAuthorization();

        group.MapGet("/trends", async (
            IMediator mediator,
            ClaimsPrincipal user,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate) =>
        {
            var googleId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(googleId))
                return Results.Unauthorized();

            var end = endDate ?? DateTime.UtcNow.Date;
            var start = startDate ?? end.AddDays(-90); // Default 90 days

            try
            {
                var query = new GetTrendsQuery(googleId, start, end);
                var result = await mediator.Send(query);
                return Results.Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("GetTrends")
        .Produces<Shared.DTOs.TrendsResponseDto>()
        .Produces(400)
        .Produces(401);

        group.MapGet("/correlation", async (
            IMediator mediator,
            ClaimsPrincipal user,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate) =>
        {
            var googleId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(googleId))
                return Results.Unauthorized();

            var end = endDate ?? DateTime.UtcNow.Date;
            var start = startDate ?? end.AddDays(-90); // Default 90 days

            var query = new GetCorrelationQuery(googleId, start, end);
            var result = await mediator.Send(query);
            return Results.Ok(result);
        })
        .WithName("GetCorrelation")
        .Produces<Shared.DTOs.CorrelationDto>()
        .Produces(401);

        return group;
    }
}
