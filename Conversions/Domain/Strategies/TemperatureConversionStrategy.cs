namespace Conversions.Domain.Strategies;

using Conversions.Domain.Exceptions;
using Conversions.Domain.Models;

/// <summary>
/// Handles non-linear affine temperature conversions (Celsius, Fahrenheit, Kelvin, Rankine).
/// All conversions are normalized via Kelvin as the thermodynamic base unit.
/// </summary>
public class TemperatureConversionStrategy : IConversionStrategy
{
    private const decimal CelsiusOffset = 273.15m;
    private const decimal FahrenheitOffset = 32m;
    private const decimal FiveNinths = 5m / 9m;
    private const decimal NineFifths = 9m / 5m;

    public bool CanHandle(UnitDefinition sourceUnit, UnitDefinition targetUnit)
    {
        return string.Equals(sourceUnit.Category, CategoryConstants.Temperature, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(targetUnit.Category, CategoryConstants.Temperature, StringComparison.OrdinalIgnoreCase);
    }

    public decimal Convert(decimal value, UnitDefinition sourceUnit, UnitDefinition targetUnit)
    {
        if (sourceUnit.Id == targetUnit.Id)
        {
            ValidateNotBelowAbsoluteZero(value, sourceUnit);
            return value;
        }

        // 1. Convert source value to Kelvin
        decimal kelvin = ToKelvin(value, sourceUnit);

        // Validate thermodynamic absolute zero
        if (kelvin < 0m)
        {
            throw new InvalidConversionException(
                $"Temperature value '{value} {sourceUnit.Symbol}' is below absolute zero (0 K / -273.15 °C / -459.67 °F).");
        }

        // 2. Convert Kelvin to target unit
        return FromKelvin(kelvin, targetUnit);
    }

    public string DescribeCalculation(decimal value, decimal result, UnitDefinition sourceUnit, UnitDefinition targetUnit)
    {
        if (sourceUnit.Id == targetUnit.Id)
        {
            return $"{value} {sourceUnit.Symbol} = {result} {targetUnit.Symbol} (identical unit)";
        }

        return sourceUnit.Id switch
        {
            "celsius" when targetUnit.Id == "fahrenheit" =>
                $"({value} °C × 9/5) + 32 = {result:G29} °F",
            "fahrenheit" when targetUnit.Id == "celsius" =>
                $"({value} °F - 32) × 5/9 = {result:G29} °C",
            "celsius" when targetUnit.Id == "kelvin" =>
                $"{value} °C + 273.15 = {result:G29} K",
            "kelvin" when targetUnit.Id == "celsius" =>
                $"{value} K - 273.15 = {result:G29} °C",
            "fahrenheit" when targetUnit.Id == "kelvin" =>
                $"({value} °F - 32) × 5/9 + 273.15 = {result:G29} K",
            "kelvin" when targetUnit.Id == "fahrenheit" =>
                $"({value} K - 273.15) × 9/5 + 32 = {result:G29} °F",
            _ =>
                $"{value} {sourceUnit.Symbol} converted to {result:G29} {targetUnit.Symbol} via Kelvin baseline"
        };
    }

    private static decimal ToKelvin(decimal value, UnitDefinition sourceUnit)
    {
        return sourceUnit.Id switch
        {
            "kelvin" => value,
            "celsius" => value + CelsiusOffset,
            "fahrenheit" => (value - FahrenheitOffset) * FiveNinths + CelsiusOffset,
            "rankine" => value * FiveNinths,
            _ => throw new InvalidConversionException($"Unsupported temperature unit: '{sourceUnit.Id}'.")
        };
    }

    private static decimal FromKelvin(decimal kelvin, UnitDefinition targetUnit)
    {
        return targetUnit.Id switch
        {
            "kelvin" => kelvin,
            "celsius" => kelvin - CelsiusOffset,
            "fahrenheit" => (kelvin - CelsiusOffset) * NineFifths + FahrenheitOffset,
            "rankine" => kelvin * NineFifths,
            _ => throw new InvalidConversionException($"Unsupported temperature unit: '{targetUnit.Id}'.")
        };
    }

    private static void ValidateNotBelowAbsoluteZero(decimal value, UnitDefinition unit)
    {
        decimal kelvin = ToKelvin(value, unit);
        if (kelvin < 0m)
        {
            throw new InvalidConversionException(
                $"Temperature value '{value} {unit.Symbol}' is below absolute zero (0 K / -273.15 °C / -459.67 °F).");
        }
    }
}
