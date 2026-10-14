namespace Conversions.Tests.Application;

using Conversions.Application.DTOs;
using Conversions.Application.Validators;
using FluentAssertions;
using NUnit.Framework;

[TestFixture]
public class ConversionRequestValidatorTests
{
    private readonly ConversionRequestValidator _validator = new();

    [Test]
    public void Validate_ValidRequest_IsValid()
    {
        // Arrange
        var request = new ConversionRequest
        {
            Value = 100m,
            SourceUnit = "meter",
            TargetUnit = "foot",
            Precision = 4
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [TestCase("")]
    [TestCase("   ")]
    [TestCase(null)]
    public void Validate_MissingSourceUnit_FailsValidation(string? invalidSource)
    {
        // Arrange
        var request = new ConversionRequest
        {
            Value = 10m,
            SourceUnit = invalidSource!,
            TargetUnit = "foot"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ConversionRequest.SourceUnit));
    }

    [TestCase("")]
    [TestCase("   ")]
    [TestCase(null)]
    public void Validate_MissingTargetUnit_FailsValidation(string? invalidTarget)
    {
        // Arrange
        var request = new ConversionRequest
        {
            Value = 10m,
            SourceUnit = "meter",
            TargetUnit = invalidTarget!
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ConversionRequest.TargetUnit));
    }

    [TestCase(-1)]
    [TestCase(16)]
    public void Validate_InvalidPrecision_FailsValidation(int invalidPrecision)
    {
        // Arrange
        var request = new ConversionRequest
        {
            Value = 10m,
            SourceUnit = "meter",
            TargetUnit = "foot",
            Precision = invalidPrecision
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ConversionRequest.Precision));
    }

    [TestCase(0)]
    [TestCase(6)]
    [TestCase(15)]
    public void Validate_ValidPrecision_PassesValidation(int validPrecision)
    {
        // Arrange
        var request = new ConversionRequest
        {
            Value = 10m,
            SourceUnit = "meter",
            TargetUnit = "foot",
            Precision = validPrecision
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
