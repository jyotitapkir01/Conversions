namespace Conversions.Application.Validators;

using Conversions.Application.DTOs;
using FluentValidation;

/// <summary>
/// Validator for incoming conversion requests.
/// </summary>
public class ConversionRequestValidator : AbstractValidator<ConversionRequest>
{
    public ConversionRequestValidator()
    {
        RuleFor(x => x.SourceUnit)
            .NotEmpty()
            .WithMessage("Source unit is required.")
            .MaximumLength(50)
            .WithMessage("Source unit must not exceed 50 characters.");

        RuleFor(x => x.TargetUnit)
            .NotEmpty()
            .WithMessage("Target unit is required.")
            .MaximumLength(50)
            .WithMessage("Target unit must not exceed 50 characters.");

        RuleFor(x => x.Precision)
            .InclusiveBetween(0, 15)
            .When(x => x.Precision.HasValue)
            .WithMessage("Precision must be between 0 and 15 decimal places.");
    }
}
