using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.RateLimiting;
using PoOmad.Shared.DTOs;
using PoOmad.Api.Infrastructure.Middleware;
using System.Security.Claims;
using MediatR;
using PoOmad.Api.Features.Profile;

namespace PoOmad.Api.Features.Authentication;

public static class GoogleAuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth")
            .RequireRateLimiting(RateLimitingConfig.AuthPolicy);

        // GET /api/auth/google - Initiate Google OAuth flow
        group.MapGet("/google", async (HttpContext context) =>
        {
            // Redirect URI is where the user goes AFTER successful auth
            // The OAuth callback (/signin-google) is handled automatically by ASP.NET
            var properties = new AuthenticationProperties
            {
                RedirectUri = "/", // Go to home page after successful login
                Items = { { "scheme", GoogleDefaults.AuthenticationScheme } }
            };

            await context.ChallengeAsync(GoogleDefaults.AuthenticationScheme, properties);
        })
        .WithName("InitiateGoogleAuth")
        .AllowAnonymous();

        // Note: OAuth callback is handled automatically by ASP.NET Core at /signin-google
        // After successful auth, user is redirected to RedirectUri specified above

        // GET /api/auth/me - Get current authenticated user info
        group.MapGet("/me", async (HttpContext context, IMediator mediator) =>
        {
            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                return Results.Ok(new UserInfoDto
                {
                    IsAuthenticated = false,
                    HasProfile = false
                });
            }

            var googleId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            var email = context.User.FindFirst(ClaimTypes.Email)?.Value ?? "";

            // Check if user has a profile
            var profile = await mediator.Send(new GetProfileQuery(googleId));

            return Results.Ok(new UserInfoDto
            {
                GoogleId = googleId,
                Email = email,
                IsAuthenticated = true,
                HasProfile = profile != null
            });
        })
        .WithName("GetCurrentUser");

        // POST /api/auth/signout - Sign out and clear cookies
        group.MapPost("/signout", async (HttpContext context) =>
        {
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.Redirect("/auth");
        })
        .WithName("SignOut")
        .RequireAuthorization();
    }
}
