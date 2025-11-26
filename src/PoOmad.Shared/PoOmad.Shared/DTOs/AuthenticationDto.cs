namespace PoOmad.Shared.DTOs;

/// <summary>
/// OAuth callback parameters from Google authentication flow.
/// Used server-side to exchange authorization code for user information.
/// </summary>
public class GoogleAuthCallbackDto
{
    /// <summary>
    /// Authorization code returned by Google OAuth.
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// State parameter for CSRF protection (must match original request).
    /// </summary>
    public string State { get; set; } = string.Empty;
}

/// <summary>
/// Current authenticated user information and profile status.
/// Returned by GET /api/auth/me endpoint.
/// </summary>
public class UserInfoDto
{
    /// <summary>
    /// User's Google account email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Google OAuth subject identifier (unique per user).
    /// </summary>
    public string GoogleId { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether the user is currently authenticated (has valid session cookie).
    /// </summary>
    public bool IsAuthenticated { get; set; }
    
    /// <summary>
    /// Whether the user has completed profile setup (height and starting weight).
    /// False indicates user should be redirected to setup wizard.
    /// </summary>
    public bool HasProfile { get; set; }
}
