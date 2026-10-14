namespace Conversions.Tests.Domain;

using Conversions.Domain.Models;
using Conversions.Domain.Strategies;
using Conversions.Infrastructure;
using FluentAssertions;
using Xunit;

public class LinearConversionStrategyTests
{
    private readonly LinearConversionStrategy _strategy = new();
    private readonly HardcodedUnitRegistry _registry = new();

    [Theory]
    [InlineData(1, "meter", "foot", 3.28084)]
    [InlineData(100, "meter", "foot", 328.084)]
    [InlineData(1, "kilometer", "mile", 0.621371)]
    [InlineData(1, "inch", "centimeter", 2.54)]
    [InlineData(12, "inch", "foot", 1.0)]
    [InlineData(3, "foot", "yard", 1.0)]
    public void Convert_LengthUnits_CalculatesExpectedValues(
        decimal inputValue,
        string sourceId,
        string targetId,
        decimal expectedApprox)
    {
        // Arrange
        var sourceUnit = _registry.GetRequiredUnit(sourceId);
        var targetUnit = _registry.GetRequiredUnit(targetId);

        // Act
        decimal result = _strategy.Convert(inputValue, sourceUnit, targetUnit);

        // Assert
        result.Should().BeApproximately(expectedApprox, 0.001m);
    }

    [Theory]
    [InlineData(1, "kilogram", "pound", 2.20462)]
    [InlineData(1000, "gram", "kilogram", 1.0)]
    [InlineData(16, "ounce", "pound", 1.0)]
    [InlineData(1, "stone", "pound", 14.0)]
    [InlineData(1, "metricton", "kilogram", 1000.0)]
    public void Convert_WeightUnits_CalculatesExpectedValues(
        decimal inputValue,
        string sourceId,
        string targetId,
        decimal expectedApprox)
    {
        // Arrange
        var sourceUnit = _registry.GetRequiredUnit(sourceId);
        var targetUnit = _registry.GetRequiredUnit(targetId);

        // Act
        decimal result = _strategy.Convert(inputValue, sourceUnit, targetUnit);

        // Assert
        result.Should().BeApproximately(expectedApprox, 0.001m);
    }

    [Theory]
    [InlineData(1, "liter", "milliliter", 1000.0)]
    [InlineData(1, "gallon", "liter", 3.78541)]
    [InlineData(1, "cubicmeter", "liter", 1000.0)]
    public void Convert_VolumeUnits_CalculatesExpectedValues(
        decimal inputValue,
        string sourceId,
        string targetId,
        decimal expectedApprox)
    {
        // Arrange
        var sourceUnit = _registry.GetRequiredUnit(sourceId);
        var targetUnit = _registry.GetRequiredUnit(targetId);

        // Act
        decimal result = _strategy.Convert(inputValue, sourceUnit, targetUnit);

        // Assert
        result.Should().BeApproximately(expectedApprox, 0.001m);
    }

    [Theory]
    [InlineData(1, "hour", "minute", 60.0)]
    [InlineData(1, "hour", "second", 3600.0)]
    [InlineData(1, "day", "hour", 24.0)]
    [InlineData(1000, "millisecond", "second", 1.0)]
    public void Convert_TimeUnits_CalculatesExpectedValues(
        decimal inputValue,
        string sourceId,
        string targetId,
        decimal expectedApprox)
    {
        // Arrange
        var sourceUnit = _registry.GetRequiredUnit(sourceId);
        var targetUnit = _registry.GetRequiredUnit(targetId);

        // Act
        decimal result = _strategy.Convert(inputValue, sourceUnit, targetUnit);

        // Assert
        result.Should().BeApproximately(expectedApprox, 0.001m);
    }

    [Fact]
    public void Convert_SameUnit_ReturnsIdenticalValue()
    {
        // Arrange
        var unit = _registry.GetRequiredUnit("meter");

        // Act
        decimal result = _strategy.Convert(42.5m, unit, unit);

        // Assert
        result.Should().Be(42.5m);
    }

    [Fact]
    public void CanHandle_LinearCategories_ReturnsTrue()
    {
        // Arrange
        var meter = _registry.GetRequiredUnit("meter");
        var foot = _registry.GetRequiredUnit("foot");

        // Act & Assert
        _strategy.CanHandle(meter, foot).Should().BeTrue();
    }

    [Fact]
    public void CanHandle_TemperatureCategory_ReturnsFalse()
    {
        // Arrange
        var celsius = _registry.GetRequiredUnit("celsius");
        var fahrenheit = _registry.GetRequiredUnit("fahrenheit");

        // Act & Assert
        _strategy.CanHandle(celsius, fahrenheit).Should().BeFalse();
    }

    [Fact]
    public void DescribeCalculation_GeneratesHumanReadableFormula()
    {
        // Arrange
        var meter = _registry.GetRequiredUnit("meter");
        var foot = _registry.GetRequiredUnit("foot");

        // Act
        string formula = _strategy.DescribeCalculation(10m, 32.8084m, meter, foot);

        // Assert
        formula.Should().Contain("10 m");
        formula.Should().Contain("ft");
    }
}
