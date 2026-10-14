namespace Conversions.Domain.Strategies;

using Conversions.Domain.Models;

/// <summary>
/// Defines the strategy contract for performing unit conversion between two units.
/// </summary>
public interface IConversionStrategy
{
    /// <summary>
    /// Determines whether this strategy can handle conversion between the given source and target units.
    /// </summary>
    bool CanHandle(UnitDefinition sourceUnit, UnitDefinition targetUnit);

    /// <summary>
    /// Performs the numerical conversion.
    /// </summary>
    decimal Convert(decimal value, UnitDefinition sourceUnit, UnitDefinition targetUnit);

    /// <summary>
    /// Generates a human-readable explanation of the conversion formula applied.
    /// </summary>
    string DescribeCalculation(decimal value, decimal result, UnitDefinition sourceUnit, UnitDefinition targetUnit);
}
