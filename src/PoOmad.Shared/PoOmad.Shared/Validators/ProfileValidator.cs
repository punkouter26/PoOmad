using FluentValidation;
using PoOmad.Shared.DTOs;
using System.Text.RegularExpressions;

namespace PoOmad.Shared.Validators;

/// <summary>
/// Validator for UserProfileDto with height format support (4'0"-7'0" or 122-213cm)
/// </summary>
public partial class ProfileValidator : AbstractValidator<UserProfileDto>
{
    public ProfileValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("Valid email is required");

        RuleFor(x => x.Height)
            .NotEmpty()
            .Must(BeValidHeight)
            .WithMessage("Height must be in format 4'0\"-7'0\" or 122-213cm");

        RuleFor(x => x.StartingWeight)
            .InclusiveBetween(50, 500)
            .WithMessage("Weight must be between 50 and 500 lbs");

        RuleFor(x => x.StartDate)
            .LessThanOrEqualTo(DateTime.UtcNow.Date)
            .WithMessage("Start date cannot be in the future");
    }

    private static bool BeValidHeight(string height)
    {
        if (string.IsNullOrWhiteSpace(height))
            return false;

        // Imperial format: 4'0" to 7'0" (4 feet to 7 feet with inches)
        var imperialRegex = ImperialHeightRegex();
        if (imperialRegex.IsMatch(height))
        {
            var match = imperialRegex.Match(height);
            var feet = int.Parse(match.Groups[1].Value);
            var inches = int.Parse(match.Groups[2].Value);

            var totalInches = (feet * 12) + inches;
            return totalInches >= 48 && totalInches <= 84; // 4'0" (48") to 7'0" (84")
        }

        // Metric format: 122-213cm
        var metricRegex = MetricHeightRegex();
        if (metricRegex.IsMatch(height))
        {
            var match = metricRegex.Match(height);
            var cm = int.Parse(match.Groups[1].Value);
            return cm >= 122 && cm <= 213;
        }

        return false;
    }

    [GeneratedRegex(@"^(\d)'(\d{1,2})""$")]
    private static partial Regex ImperialHeightRegex();

    [GeneratedRegex(@"^(\d{3})cm$")]
    private static partial Regex MetricHeightRegex();
}
