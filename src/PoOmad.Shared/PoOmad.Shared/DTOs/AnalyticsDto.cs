namespace PoOmad.Shared.DTOs;

/// <summary>
/// Single data point in weight and alcohol trend chart.
/// </summary>
/// <param name="Date">Date of the data point.</param>
/// <param name="Weight">Weight in pounds. May be null if not logged, or carried forward from previous day.</param>
/// <param name="AlcoholConsumed">Whether alcohol was consumed on this date.</param>
/// <param name="IsCarryForward">True if weight is carried forward from a previous day (gap-filling), false if actually logged.</param>
public record TrendDataPointDto(
    DateTime Date,
    decimal? Weight,
    bool AlcoholConsumed,
    bool IsCarryForward
);

/// <summary>
/// Response containing weight and alcohol trends over a date range.
/// Requires minimum 3 days of logged data.
/// </summary>
/// <param name="DataPoints">List of trend data points with gap-filling applied.</param>
/// <param name="TotalDaysLogged">Number of days with actual logged data (excludes carried-forward gaps).</param>
/// <param name="WeightChange">Total weight change in pounds from first to last data point. Null if insufficient data.</param>
public record TrendsResponseDto(
    List<TrendDataPointDto> DataPoints,
    int TotalDaysLogged,
    decimal? WeightChange
);

/// <summary>
/// Statistical correlation between alcohol consumption and weight changes.
/// </summary>
/// <param name="Correlation">Pearson correlation coefficient between alcohol consumption and weight (-1 to 1). Null if insufficient data.</param>
/// <param name="DaysWithAlcohol">Number of days where alcohol was consumed.</param>
/// <param name="TotalDays">Total number of days with logged data.</param>
/// <param name="AverageWeightWithAlcohol">Average weight on days with alcohol consumption. Null if no alcohol days.</param>
/// <param name="AverageWeightWithoutAlcohol">Average weight on days without alcohol consumption. Null if all days had alcohol.</param>
public record CorrelationDto(
    double? Correlation,
    int DaysWithAlcohol,
    int TotalDays,
    decimal? AverageWeightWithAlcohol,
    decimal? AverageWeightWithoutAlcohol
);
