namespace PoOmad.Shared.DTOs;

/// <summary>
/// RFC 7807 Problem Details for HTTP error responses.
/// Provides machine-readable format for API errors with consistent structure.
/// </summary>
public class ProblemDetailsDto
{
    /// <summary>
    /// URI reference identifying the problem type. Default is "about:blank" for generic errors.
    /// Example: "https://api.poomad.com/errors/validation-failed"
    /// </summary>
    public string Type { get; set; } = "about:blank";
    
    /// <summary>
    /// Short, human-readable summary of the problem type.
    /// Example: "Validation Failed", "Unauthorized"
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// HTTP status code for this occurrence of the problem.
    /// Example: 400 (Bad Request), 401 (Unauthorized), 404 (Not Found)
    /// </summary>
    public int Status { get; set; }
    
    /// <summary>
    /// Human-readable explanation specific to this occurrence of the problem.
    /// Example: "Weight must be between 50 and 500 lbs"
    /// </summary>
    public string? Detail { get; set; }
    
    /// <summary>
    /// URI reference identifying the specific occurrence of the problem.
    /// Example: "/api/daily-logs/2025-11-23"
    /// </summary>
    public string? Instance { get; set; }
    
    /// <summary>
    /// Additional problem-specific data. Used for validation errors, field-level details, etc.
    /// Example: { "errors": { "Weight": ["Must be between 50 and 500"] } }
    /// </summary>
    public Dictionary<string, object>? Extensions { get; set; }
}
