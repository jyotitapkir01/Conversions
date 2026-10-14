namespace Conversions.Tests.Domain;

using Conversions.Domain.Exceptions;
using Conversions.Domain.Strategies;
using Conversions.Infrastructure;
using FluentAssertions;
using Xunit;

public class TemperatureConversionStrategyTests
{
    private readonly TemperatureConversionStrategy _strategy = new();
    private readonly HardcodedUnitRegistry _registry = new();

    [Theory]
    [InlineData(0, "celsius", "fahrenheit", 32.0)]
    [InlineData(100, "celsius", "fahrenheit", 212.0)]
    [InlineData(-40, "celsius", "fahrenheit", -40.0)]
    [InlineData(37, "celsius", "fahrenheit", 98.6)]
    public void Convert_CelsiusToFahrenheit_CalculatesAccurately(
        decimal celsius,
        string sourceId,
        string targetId,
        decimal expectedFahrenheit)
    {
        // Arrange
        var sourceUnit = _registry.GetRequiredUnit(sourceId);
        var targetUnit = _registry.GetRequiredUnit(targetId);

        // Act
        decimal result = _strategy.Convert(celsius, sourceUnit, targetUnit);

        // Assert
        result.Should().BeApproximately(expectedFahrenheit, 0.001m);
    }

    [Theory]
    [InlineData(32, "fahrenheit", "celsius", 0.0)]
    [InlineData(212, "fahrenheit", "celsius", 100.0)]
    [InlineData(-40, "fahrenheit", "celsius", -40.0)]
    [InlineData(98.6, "fahrenheit", "celsius", 37.0)]
    public void Convert_FahrenheitToCelsius_CalculatesAccurately(
        decimal fahrenheit,
        string sourceId,
        string targetId,
        decimal expectedCelsius)
    {
        // Arrange
        var sourceUnit = _registry.GetRequiredUnit(sourceId);
        var targetUnit = _registry.GetRequiredUnit(targetId);

        // Act
        decimal result = _strategy.Convert(fahrenheit, sourceUnit, targetUnit);

        // Assert
        result.Should().BeApproximately(expectedCelsius, 0.001m);
    }

    [Theory]
    [InlineData(0, "celsius", "kelvin", 273.15)]
    [InlineData(100, "celsius", "kelvin", 373.15)]
    [InlineData(-273.15, "celsius", "kelvin", 0.0)]
    public void Convert_CelsiusToKelvin_CalculatesAccurately(
        decimal celsius,
        string sourceId,
        string targetId,
        decimal expectedKelvin)
    {
        // Arrange
        var sourceUnit = _registry.GetRequiredUnit(sourceId);
        var targetUnit = _registry.GetRequiredUnit(targetId);

        // Act
        decimal result = _strategy.Convert(celsius, sourceUnit, targetUnit);

        // Assert
        result.Should().Be(expectedKelvin);
    }

    [Theory]
    [InlineData(0, "kelvin", "celsius", -273.15)]
    [InlineData(273.15, "kelvin", "celsius", 0.0)]
    [InlineData(373.15, "kelvin", "celsius", 100.0)]
    public void Convert_KelvinToCelsius_CalculatesAccurately(
        decimal kelvin,
        string sourceId,
        string targetId,
        decimal expectedCelsius)
    {
        // Arrange
        var sourceUnit = _registry.GetRequiredUnit(sourceId);
        var targetUnit = _registry.GetRequiredUnit(targetId);

        // Act
        decimal result = _strategy.Convert(kelvin, sourceUnit, targetUnit);

        // Assert
        result.Should().Be(expectedCelsius);
    }

    [Fact]
    public void Convert_AbsoluteZeroKelvin_SucceedsWithoutError()
    {
        // Arrange
        var kelvin = _registry.GetRequiredUnit("kelvin");
        var celsius = _registry.GetRequiredUnit("celsius");

        // Act
        decimal result = _strategy.Convert(0m, kelvin, celsius);

        // Assert
        result.Should().Be(-273.15m);
    }

    [Theory]
    [InlineData(-273.16, "celsius", "fahrenheit")]
    [InlineData(-459.68, "fahrenheit", "celsius")]
    [InlineData(-1.0, "kelvin", "celsius")]
    public void Convert_TemperatureBelowAbsoluteZero_ThrowsInvalidConversionException(
        decimal invalidValue,
        string sourceId,
        string targetId)
    {
        // Arrange
        var sourceUnit = _registry.GetRequiredUnit(sourceId);
        var targetUnit = _registry.GetRequiredUnit(targetId);

        // Act
        Action act = () => _strategy.Convert(invalidValue, sourceUnit, targetUnit);

        // Assert
        act.Should().Throw<InvalidConversionException>()
            .WithMessage("*absolute zero*");
    }

    [Fact]
    public void DescribeCalculation_ForCelsiusToFahrenheit_ReturnsCorrectFormulaString()
    {
        // Arrange
        var celsius = _registry.GetRequiredUnit("celsius");
        var fahrenheit = _registry.GetRequiredUnit("fahrenheit");

        // Act
        string formula = _strategy.DescribeCalculation(100m, 212m, celsius, fahrenheit);

        // Assert
        formula.Should().Contain("(100 °C × 9/5) + 32 = 212 °F");
    }
}
