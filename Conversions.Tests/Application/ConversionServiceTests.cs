namespace Conversions.Tests.Application;

using Conversions.Application.DTOs;
using Conversions.Application.Services;
using Conversions.Domain.Exceptions;
using Conversions.Domain.Interfaces;
using Conversions.Domain.Strategies;
using Conversions.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

public class ConversionServiceTests
{
    private readonly ConversionService _service;

    public ConversionServiceTests()
    {
        var registry = new HardcodedUnitRegistry();
        var strategies = new IConversionStrategy[]
        {
            new LinearConversionStrategy(),
            new TemperatureConversionStrategy()
        };
        var logger = NullLogger<ConversionService>.Instance;

        _service = new ConversionService(registry, strategies, logger);
    }

    [Fact]
    public void Convert_ValidMetersToFeet_ReturnsExpectedResponse()
    {
        // Arrange
        var request = new ConversionRequest
        {
            Value = 100m,
            SourceUnit = "meter",
            TargetUnit = "foot",
            Precision = 2
        };

        // Act
        var response = _service.Convert(request);

        // Assert
        response.Should().NotBeNull();
        response.Category.Should().Be("Length");
        response.Source.Value.Should().Be(100m);
        response.Source.Unit.Should().Be("Meter");
        response.Source.Symbol.Should().Be("m");
        response.Target.Value.Should().Be(328.08m);
        response.Target.Unit.Should().Be("Foot");
        response.Target.Symbol.Should().Be("ft");
        response.Formula.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Convert_WithPrecision_RoundsAccurately()
    {
        // Arrange
        var request = new ConversionRequest
        {
            Value = 1m,
            SourceUnit = "meter",
            TargetUnit = "foot",
            Precision = 4
        };

        // Act
        var response = _service.Convert(request);

        // Assert
        response.Target.Value.Should().Be(3.2808m);
    }

    [Fact]
    public void Convert_WithoutPrecision_ReturnsRawDecimal()
    {
        // Arrange
        var request = new ConversionRequest
        {
            Value = 1m,
            SourceUnit = "meter",
            TargetUnit = "foot",
            Precision = null
        };

        // Act
        var response = _service.Convert(request);

        // Assert
        // 1 meter = 1 / 0.3048 feet = 3.2808398950131233595800524934 feet
        response.Target.Value.ToString().Should().StartWith("3.280839895");
    }

    [Fact]
    public void Convert_IncompatibleCategories_ThrowsIncompatibleUnitsException()
    {
        // Arrange
        var request = new ConversionRequest
        {
            Value = 50m,
            SourceUnit = "meter",
            TargetUnit = "kilogram"
        };

        // Act
        Action act = () => _service.Convert(request);

        // Assert
        act.Should().Throw<IncompatibleUnitsException>()
            .WithMessage("*incompatible categories*Length*Weight*");
    }

    [Fact]
    public void Convert_UnknownSourceUnit_ThrowsUnitNotFoundException()
    {
        // Arrange
        var request = new ConversionRequest
        {
            Value = 10m,
            SourceUnit = "lightyears",
            TargetUnit = "meter"
        };

        // Act
        Action act = () => _service.Convert(request);

        // Assert
        act.Should().Throw<UnitNotFoundException>()
            .WithMessage("*lightyears*");
    }

    [Fact]
    public void Convert_TemperatureBelowAbsoluteZero_ThrowsInvalidConversionException()
    {
        // Arrange
        var request = new ConversionRequest
        {
            Value = -300m,
            SourceUnit = "celsius",
            TargetUnit = "fahrenheit"
        };

        // Act
        Action act = () => _service.Convert(request);

        // Assert
        act.Should().Throw<InvalidConversionException>()
            .WithMessage("*absolute zero*");
    }

    [Fact]
    public void GetUnits_FilteredByCategory_ReturnsOnlyMatchingUnits()
    {
        // Act
        var units = _service.GetUnits("Weight");

        // Assert
        units.Should().NotBeEmpty();
        units.Should().OnlyContain(u => u.Category == "Weight");
    }

    [Fact]
    public void GetCategories_ReturnsExpectedCategoryList()
    {
        // Act
        var categories = _service.GetCategories();

        // Assert
        categories.Should().Contain("Length");
        categories.Should().Contain("Weight");
        categories.Should().Contain("Temperature");
    }
}
