namespace Conversions.Application.DTOs;

/// <summary>
/// Value and metadata for a specific unit measurement.
/// </summary>
public record UnitValueDto
{
    /// <summary>
    /// Numerical value.
    /// </summary>
    public required decimal Value { get; init; }

    /// <summary>
    /// unit.
    /// </summary>
    public required string Unit { get; init; }

    /// <summary>
    /// symbol.
    /// </summary>
    public required string Symbol { get; init; }
}

/// <summary>
/// Response payload for a completed unit conversion.
/// </summary>
public record ConversionResponse
{
    /// <summary>
    /// Input value and unit details.
    /// </summary>
    public required UnitValueDto Source { get; init; }

    /// <summary>
    /// Converted target value and unit details.
    /// </summary>
    public required UnitValueDto Target { get; init; }

    /// <summary>
    /// Measurement category of the conversion (e.g. Length, Temperature, Weight).
    /// </summary>
    public required string Category { get; init; }

    /// <summary>
    /// Human-readable explanation of the applied mathematical calculation.
    /// </summary>
    public required string Formula { get; init; }
}

/// <summary>
/// Metadata for a supported unit of measurement.
/// </summary>
public record UnitDto
{
    /// <summary>
    /// Unique identifier of the unit.
    /// </summary>
    /// <example>meter</example>
    public required string Id { get; init; }

    /// <summary>
    /// Display name of the unit.
    /// </summary>
    /// <example>Meter</example>
    public required string Name { get; init; }

    /// <summary>
    /// Unit symbol.
    /// </summary>
    /// <example>m</example>
    public required string Symbol { get; init; }

    /// <summary>
    /// Category to which the unit belongs.
    /// </summary>
    /// <example>Length</example>
    public required string Category { get; init; }

    /// <summary>
    /// Indicates whether this is the canonical reference unit for the category.
    /// </summary>
    public bool IsBaseUnit { get; init; }

    /// <summary>
    /// Alternate identifiers and aliases accepted for this unit.
    /// </summary>
    public IReadOnlyCollection<string> Aliases { get; init; } = Array.Empty<string>();
}
