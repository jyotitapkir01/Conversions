using System.ComponentModel.DataAnnotations;

namespace Conversions.Models;

/// <summary>
/// Model for a unit conversion request.
/// </summary>
public class ConversionRequest
{
    /// <summary>
    /// Numerical value to convert.
    /// </summary>
    [Required(ErrorMessage = "Value is required.")]
    public decimal Value { get; set; }

    /// <summary>
    /// The unit to convert from (e.g. "meter", "m", "celsius", "kg").
    /// </summary>
    [Required(ErrorMessage = "Source unit is required.")]
    public string SourceUnit { get; set; } = string.Empty;

    /// <summary>
    /// The unit to convert to (e.g. "foot", "ft", "fahrenheit", "lb").
    /// </summary>
    [Required(ErrorMessage = "Target unit is required.")]
    public string TargetUnit { get; set; } = string.Empty;

    /// <summary>
    /// Optional number of decimal places to round the result.
    /// </summary>
    [Range(0, 15, ErrorMessage = "Precision must be between 0 and 15 decimal places.")]
    public int? Precision { get; set; }
}
