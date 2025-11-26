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
            var properties = new AuthenticationProperties
            {
                RedirectUri = "/api/auth/google/callback",
                Items = { { "scheme", GoogleDefaults.AuthenticationScheme } }
            };

            await context.ChallengeAsync(GoogleDefaults.AuthenticationScheme, properties);
        })
        .WithName("InitiateGoogleAuth")
        .AllowAnonymous();

        // GET /api/auth/google/callback - Handle Google OAuth callback
        group.MapGet("/google/callback", async (HttpContext context) =>
        {
            var result = await context.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

            if (!result.Succeeded)
                return Results.Redirect("/auth?error=authentication_failed");

            var claims = result.Principal?.Claims.ToList() ?? new List<Claim>();
            var googleId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(googleId) || string.IsNullOrEmpty(email))
                return Results.Redirect("/auth?error=missing_claims");

            // Create authentication cookie with claims
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await context.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30)
                });

            // Redirect to client app (same origin since WASM is hosted in API)
            return Results.Redirect("/");
        })
        .WithName("GoogleAuthCallback")
        .AllowAnonymous();

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
