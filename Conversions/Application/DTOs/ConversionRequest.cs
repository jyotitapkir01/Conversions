namespace Conversions.Application.DTOs;

/// <summary>
/// Request payload to convert a numerical value between two units.
/// </summary>
public record ConversionRequest
{
    /// <summary>
    /// Value  to be converted.
    /// </summary>
    public required decimal Value { get; init; }

    /// <summary>
    /// Units 
    /// </summary>
    public required string SourceUnit { get; init; }

    /// <summary>
    /// Target Units 
    /// </summary>
    public required string TargetUnit { get; init; }

    /// <summary>
    /// decimal places to round the target value.
    /// </summary>
    public int? Precision { get; init; }
}
