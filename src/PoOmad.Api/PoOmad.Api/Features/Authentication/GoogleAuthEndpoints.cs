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
        group.MapGet("/google", async (HttpContext context, IConfiguration configuration) =>
        {
            // Check if Google auth is configured
            var clientId = configuration["Authentication:Google:ClientId"];
            var clientSecret = configuration["Authentication:Google:ClientSecret"];
            
            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                return Results.Problem(
                    title: "Google OAuth Not Configured",
                    detail: "Google OAuth credentials are not configured. Please set Authentication:Google:ClientId and Authentication:Google:ClientSecret in your configuration.",
                    statusCode: 503);
            }

            // Redirect URI is where the user goes AFTER successful auth
            // The OAuth callback (/signin-google) is handled automatically by ASP.NET
            var properties = new AuthenticationProperties
            {
                RedirectUri = "/", // Go to home page after successful login
                Items = { { "scheme", GoogleDefaults.AuthenticationScheme } }
            };

            await context.ChallengeAsync(GoogleDefaults.AuthenticationScheme, properties);
            return Results.Empty;
        })
        .WithName("InitiateGoogleAuth")
        .AllowAnonymous();

        // DEV ONLY: Login as test user without Google OAuth
        // This endpoint only works in Development environment
        group.MapGet("/dev-login", async (HttpContext context, IHostEnvironment env, IMediator mediator) =>
        {
            if (!env.IsDevelopment())
            {
                return Results.NotFound();
            }

            const string devUserId = "dev-user-12345";
            const string devUserEmail = "dev@localhost.test";

            // Create claims for a fake dev user
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, devUserId),
                new(ClaimTypes.Email, devUserEmail),
                new(ClaimTypes.Name, "Dev User")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            // Ensure dev user has a profile so they don't get redirected to /setup
            var existingProfile = await mediator.Send(new GetProfileQuery(devUserId));
            if (existingProfile == null)
            {
                await mediator.Send(new CreateProfileCommand(devUserId, devUserEmail, "5'10\"", 180.0m));
            }

            return Results.Redirect("/");
        })
        .WithName("DevLogin")
        .AllowAnonymous()
        .DisableRateLimiting(); // Disable rate limiting for dev-login to allow E2E testing

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
