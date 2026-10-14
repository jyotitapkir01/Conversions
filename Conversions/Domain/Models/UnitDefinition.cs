namespace Conversions.Domain.Models;

/// <summary>
/// Represents the definition and metadata of a unit of measurement.
/// </summary>
public record UnitDefinition
{
    /// <summary>
    /// Unique normalized identifier (e.g. "meter", "celsius", "kilogram").
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Display name of the unit (e.g. "Meter", "Celsius", "Kilogram").
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Standard unit symbol (e.g. "m", "°C", "kg").
    /// </summary>
    public required string Symbol { get; init; }

    /// <summary>
    /// Measurement category to which this unit belongs (e.g. "Length", "Temperature", "Weight").
    /// </summary>
    public required string Category { get; init; }

    /// <summary>
    /// Common aliases, plurals, or short codes for flexible resolution (e.g. ["m", "meter", "meters", "metre", "metres"]).
    /// </summary>
    public IReadOnlyCollection<string> Aliases { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Indicates whether this unit is the canonical reference base unit for its category.
    /// </summary>
    public bool IsBaseUnit { get; init; }

    /// <summary>
    /// Multiplier to convert 1 unit of this measurement to the reference base unit.
    /// Used by linear conversion strategies.
    /// </summary>
    public decimal ConversionFactorToBase { get; init; } = 1.0m;
}
