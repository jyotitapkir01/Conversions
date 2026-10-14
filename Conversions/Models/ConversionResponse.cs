namespace Conversions.Models;

/// <summary>
/// Model for a unit conversion result.
/// </summary>
public class ConversionResponse
{
    public decimal Value { get; set; }
    public string SourceUnit { get; set; } = string.Empty;
    public decimal ConvertedValue { get; set; }
    public string TargetUnit { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Formula { get; set; } = string.Empty;
}

/// <summary>
/// Information about a supported unit.
/// </summary>
public class UnitDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string[] Aliases { get; set; } = Array.Empty<string>();
}
