using FluentValidation;
using PoOmad.Shared.DTOs;

namespace PoOmad.Shared.Validators;

/// <summary>
/// Validator for DailyLogDto
/// </summary>
public class DailyLogValidator : AbstractValidator<DailyLogDto>
{
    public DailyLogValidator()
    {
        RuleFor(x => x.Date)
            .LessThanOrEqualTo(DateTime.UtcNow.Date)
            .WithMessage("Cannot log future dates");

        RuleFor(x => x.Weight)
            .InclusiveBetween(50m, 500m)
            .When(x => x.Weight.HasValue)
            .WithMessage("Weight must be between 50 and 500 lbs");
    }
}
