namespace Conversions.Domain.Strategies;

using Conversions.Domain.Models;

/// <summary>
/// Handles linear proportional conversions using category base-unit conversion factors.
/// Suitable for Length, Weight/Mass, Volume, Speed, Time, and any standard ratio-based category.
/// </summary>
public class LinearConversionStrategy : IConversionStrategy
{
    public bool CanHandle(UnitDefinition sourceUnit, UnitDefinition targetUnit)
    {
        // Linear conversion applies when both units are in the same category, except Temperature
        return string.Equals(sourceUnit.Category, targetUnit.Category, StringComparison.OrdinalIgnoreCase) &&
               !string.Equals(sourceUnit.Category, CategoryConstants.Temperature, StringComparison.OrdinalIgnoreCase);
    }

    public decimal Convert(decimal value, UnitDefinition sourceUnit, UnitDefinition targetUnit)
    {
        if (sourceUnit.Id == targetUnit.Id)
        {
            return value;
        }

        // Convert source value to the category's canonical base unit
        decimal baseValue = value * sourceUnit.ConversionFactorToBase;

        // Convert base unit value to target unit
        decimal targetValue = baseValue / targetUnit.ConversionFactorToBase;

        return targetValue;
    }

    public string DescribeCalculation(decimal value, decimal result, UnitDefinition sourceUnit, UnitDefinition targetUnit)
    {
        if (sourceUnit.Id == targetUnit.Id)
        {
            return $"{value} {sourceUnit.Symbol} = {result} {targetUnit.Symbol} (identical unit)";
        }

        decimal effectiveFactor = sourceUnit.ConversionFactorToBase / targetUnit.ConversionFactorToBase;
        return $"{value} {sourceUnit.Symbol} × {effectiveFactor:G29} = {result:G29} {targetUnit.Symbol}";
    }
}
