using MediatR;
using Microsoft.AspNetCore.Authorization;
using PoOmad.Shared.DTOs;
using System.Security.Claims;

namespace PoOmad.Api.Features.Profile;

public static class ProfileEndpoints
{
    public static void MapProfileEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/profile")
            .RequireAuthorization();

        // POST /api/profile - Create new profile
        group.MapPost("/", async (UserProfileDto dto, IMediator mediator, ClaimsPrincipal user) =>
        {
            var googleId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(googleId))
                return Results.Unauthorized();

            var command = new CreateProfileCommand(googleId, dto.Email, dto.Height, dto.StartingWeight);
            var result = await mediator.Send(command);
            return Results.Created($"/api/profile", result);
        })
        .WithName("CreateProfile");

        // GET /api/profile - Get current user's profile
        group.MapGet("/", async (IMediator mediator, ClaimsPrincipal user) =>
        {
            var googleId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(googleId))
                return Results.Unauthorized();

            var query = new GetProfileQuery(googleId);
            var result = await mediator.Send(query);
            return result != null ? Results.Ok(result) : Results.NotFound();
        })
        .WithName("GetProfile");

        // PUT /api/profile - Update existing profile
        group.MapPut("/", async (UserProfileDto dto, IMediator mediator, ClaimsPrincipal user) =>
        {
            var googleId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(googleId))
                return Results.Unauthorized();

            var command = new UpdateProfileCommand(googleId, dto.Height, dto.StartingWeight);
            var result = await mediator.Send(command);
            return Results.Ok(result);
        })
        .WithName("UpdateProfile");
    }
}
