namespace Conversions.Models;

/// <summary>
/// Defines a unit of measurement and its relationship to its category's base unit.
/// </summary>
public class UnitDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Multiplier to convert this unit into its category's base unit.
    /// Example for Length (Base = Meter):
    /// 1 km = 1000 m  -> FactorToBase = 1000
    /// 1 cm = 0.01 m  -> FactorToBase = 0.01
    /// 1 ft = 0.3048 m -> FactorToBase = 0.3048
    /// </summary>
    public decimal FactorToBase { get; set; } = 1.0m;

    /// <summary>
    /// Common names, abbreviations, and plurals for this unit.
    /// </summary>
    public string[] Aliases { get; set; } = Array.Empty<string>();
}
