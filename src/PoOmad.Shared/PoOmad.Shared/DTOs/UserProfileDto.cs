namespace PoOmad.Shared.DTOs;

/// <summary>
/// User profile data transfer object containing basic user information and OMAD tracking settings.
/// </summary>
public class UserProfileDto
{
    /// <summary>
    /// Google account email address (unique identifier for user).
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Google OAuth subject identifier (unique per user).
    /// </summary>
    public string GoogleId { get; set; } = string.Empty;
    
    /// <summary>
    /// User's height in format "5'10\"" or "178cm". Validated to be between 4'0" and 7'0" (122-213 cm).
    /// </summary>
    public string Height { get; set; } = string.Empty;
    
    /// <summary>
    /// User's starting weight in pounds. Must be between 50 and 500 lbs.
    /// </summary>
    public decimal StartingWeight { get; set; }
    
    /// <summary>
    /// Date when user started OMAD tracking. Used as baseline for analytics.
    /// </summary>
    public DateTime StartDate { get; set; }
}
