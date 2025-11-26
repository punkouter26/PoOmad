namespace PoOmad.Shared.DTOs;

/// <summary>
/// Daily log entry capturing OMAD compliance, alcohol consumption, and weight for a specific date.
/// Each user can have one log entry per day.
/// </summary>
public class DailyLogDto
{
    /// <summary>
    /// Date of the log entry (cannot be in the future).
    /// </summary>
    public DateTime Date { get; set; }
    
    /// <summary>
    /// Whether the user successfully completed One Meal A Day on this date.
    /// True = Success (green indicator), False = Missed (red indicator).
    /// </summary>
    public bool OmadCompliant { get; set; }
    
    /// <summary>
    /// Whether the user consumed alcohol on this date.
    /// Used for analytics and correlation with weight trends.
    /// </summary>
    public bool AlcoholConsumed { get; set; }
    
    /// <summary>
    /// User's weight in pounds on this date. Optional to allow logging without weight.
    /// Must be between 50 and 500 lbs. Triggers confirmation if change exceeds 5 lbs from previous day.
    /// </summary>
    public decimal? Weight { get; set; }
    
    /// <summary>
    /// Server-side timestamp when this log was created or last updated.
    /// Used for conflict resolution in offline sync (last-write-wins).
    /// </summary>
    public DateTime ServerTimestamp { get; set; }
}
