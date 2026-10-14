using Conversions.Models;

namespace Conversions.Services;

/// <summary>
/// Service that handles unit conversions across multiple measurement categories.
/// Designed for easy scalability: linear units only need a base-unit multiplier,
/// and future data can easily be stored in a database.
/// </summary>
public class ConversionService : IConversionService
{
    private readonly List<UnitDefinition> _units;
    private readonly Dictionary<string, UnitDefinition> _lookup;

    public ConversionService()
    {
        _units = InitializeUnits();
        _lookup = new Dictionary<string, UnitDefinition>(StringComparer.OrdinalIgnoreCase);

        // Build fast lookup by ID, Symbol, and Aliases
        foreach (var unit in _units)
        {
            _lookup[unit.Id] = unit;
            _lookup[unit.Symbol] = unit;
            foreach (var alias in unit.Aliases)
            {
                _lookup[alias] = unit;
            }
        }
    }

    /// <summary>
    /// Converts a value from source unit to target unit.
    /// </summary>
    public ConversionResponse Convert(ConversionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        // 1. Resolve source and target units
        if (!_lookup.TryGetValue(request.SourceUnit.Trim(), out var sourceUnit))
        {
            throw new ArgumentException($"Unit '{request.SourceUnit}' is not supported.");
        }

        if (!_lookup.TryGetValue(request.TargetUnit.Trim(), out var targetUnit))
        {
            throw new ArgumentException($"Unit '{request.TargetUnit}' is not supported.");
        }

        // 2. Ensure both units belong to the same category
        if (!string.Equals(sourceUnit.Category, targetUnit.Category, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                $"Cannot convert between incompatible categories: '{sourceUnit.Name}' ({sourceUnit.Category}) and '{targetUnit.Name}' ({targetUnit.Category}).");
        }

        // 3. Perform conversion
        decimal convertedValue;
        string formula;

        if (string.Equals(sourceUnit.Category, "Temperature", StringComparison.OrdinalIgnoreCase))
        {
            convertedValue = ConvertTemperature(request.Value, sourceUnit, targetUnit, out formula);
        }
        else
        {
            convertedValue = ConvertLinear(request.Value, sourceUnit, targetUnit, out formula);
        }

        // 4. Apply precision rounding if requested
        if (request.Precision.HasValue)
        {
            convertedValue = Math.Round(convertedValue, request.Precision.Value, MidpointRounding.AwayFromZero);
        }

        return new ConversionResponse
        {
            Value = request.Value,
            SourceUnit = sourceUnit.Name,
            ConvertedValue = convertedValue,
            TargetUnit = targetUnit.Name,
            Category = sourceUnit.Category,
            Formula = formula
        };
    }

    /// <summary>
    /// Linear conversion via category base unit (e.g. meter for length, kg for weight).
    /// Formula: (Value * SourceFactor) / TargetFactor
    /// </summary>
    private static decimal ConvertLinear(decimal value, UnitDefinition source, UnitDefinition target, out string formula)
    {
        if (source.Id == target.Id)
        {
            formula = $"{value} {source.Symbol} = {value} {target.Symbol}";
            return value;
        }

        decimal baseValue = value * source.FactorToBase;
        decimal result = baseValue / target.FactorToBase;

        decimal effectiveFactor = source.FactorToBase / target.FactorToBase;
        formula = $"{value} {source.Symbol} × {effectiveFactor:G29} = {result:G29} {target.Symbol}";

        return result;
    }

    /// <summary>
    /// Temperature conversion normalized through Kelvin.
    /// </summary>
    private static decimal ConvertTemperature(decimal value, UnitDefinition source, UnitDefinition target, out string formula)
    {
        // 1. Normalize source temperature to Kelvin
        decimal kelvin = source.Id.ToLowerInvariant() switch
        {
            "celsius" => value + 273.15m,
            "fahrenheit" => (value - 32m) * (5m / 9m) + 273.15m,
            "kelvin" => value,
            "rankine" => value * (5m / 9m),
            _ => throw new ArgumentException($"Unsupported temperature unit: '{source.Id}'.")
        };

        // Validate physical boundary (cannot be below absolute zero)
        if (kelvin < 0m)
        {
            throw new ArgumentException($"Temperature '{value} {source.Symbol}' is below absolute zero (0 K / -273.15 °C / -459.67 °F).");
        }

        // 2. Convert from Kelvin to target unit
        decimal result = target.Id.ToLowerInvariant() switch
        {
            "celsius" => kelvin - 273.15m,
            "fahrenheit" => (kelvin - 273.15m) * (9m / 5m) + 32m,
            "kelvin" => kelvin,
            "rankine" => kelvin * (9m / 5m),
            _ => throw new ArgumentException($"Unsupported temperature unit: '{target.Id}'.")
        };

        // Descriptive formula
        formula = source.Id switch
        {
            "celsius" when target.Id == "fahrenheit" => $"({value} °C × 9/5) + 32 = {result:G29} °F",
            "fahrenheit" when target.Id == "celsius" => $"({value} °F - 32) × 5/9 = {result:G29} °C",
            "celsius" when target.Id == "kelvin" => $"{value} °C + 273.15 = {result:G29} K",
            "kelvin" when target.Id == "celsius" => $"{value} K - 273.15 = {result:G29} °C",
            _ => $"{value} {source.Symbol} = {result:G29} {target.Symbol}"
        };

        return result;
    }

    public List<UnitDto> GetUnits(string? category = null)
    {
        var query = string.IsNullOrWhiteSpace(category)
            ? _units
            : _units.Where(u => string.Equals(u.Category, category, StringComparison.OrdinalIgnoreCase));

        return query.Select(u => new UnitDto
        {
            Id = u.Id,
            Name = u.Name,
            Symbol = u.Symbol,
            Category = u.Category,
            Aliases = u.Aliases
        }).ToList();
    }

    public List<string> GetCategories()
    {
        return _units.Select(u => u.Category)
                     .Distinct(StringComparer.OrdinalIgnoreCase)
                     .OrderBy(c => c)
                     .ToList();
    }

    private static List<UnitDefinition> InitializeUnits()
    {
        return new List<UnitDefinition>
        {
            // Length (Base unit: meter)
            new() { Id = "meter", Name = "Meter", Symbol = "m", Category = "Length", FactorToBase = 1.0m, Aliases = ["meter", "meters", "metre", "metres"] },
            new() { Id = "kilometer", Name = "Kilometer", Symbol = "km", Category = "Length", FactorToBase = 1000m, Aliases = ["kilometer", "kilometers", "kilometre", "kilometres"] },
            new() { Id = "centimeter", Name = "Centimeter", Symbol = "cm", Category = "Length", FactorToBase = 0.01m, Aliases = ["centimeter", "centimeters", "centimetre", "centimetres"] },
            new() { Id = "millimeter", Name = "Millimeter", Symbol = "mm", Category = "Length", FactorToBase = 0.001m, Aliases = ["millimeter", "millimeters", "millimetre", "millimetres"] },
            new() { Id = "mile", Name = "Mile", Symbol = "mi", Category = "Length", FactorToBase = 1609.344m, Aliases = ["mile", "miles"] },
            new() { Id = "yard", Name = "Yard", Symbol = "yd", Category = "Length", FactorToBase = 0.9144m, Aliases = ["yard", "yards"] },
            new() { Id = "foot", Name = "Foot", Symbol = "ft", Category = "Length", FactorToBase = 0.3048m, Aliases = ["foot", "feet"] },
            new() { Id = "inch", Name = "Inch", Symbol = "in", Category = "Length", FactorToBase = 0.0254m, Aliases = ["inch", "inches"] },
            new() { Id = "nauticalmile", Name = "Nautical Mile", Symbol = "nmi", Category = "Length", FactorToBase = 1852m, Aliases = ["nauticalmile", "nauticalmiles", "nm"] },

            // Weight / Mass (Base unit: kilogram)
            new() { Id = "kilogram", Name = "Kilogram", Symbol = "kg", Category = "Weight", FactorToBase = 1.0m, Aliases = ["kilogram", "kilograms", "kilo", "kilos"] },
            new() { Id = "gram", Name = "Gram", Symbol = "g", Category = "Weight", FactorToBase = 0.001m, Aliases = ["gram", "grams"] },
            new() { Id = "milligram", Name = "Milligram", Symbol = "mg", Category = "Weight", FactorToBase = 0.000001m, Aliases = ["milligram", "milligrams"] },
            new() { Id = "metricton", Name = "Metric Ton", Symbol = "t", Category = "Weight", FactorToBase = 1000m, Aliases = ["tonne", "tonnes", "metricton", "metrictons"] },
            new() { Id = "pound", Name = "Pound", Symbol = "lb", Category = "Weight", FactorToBase = 0.45359237m, Aliases = ["pound", "pounds", "lbs"] },
            new() { Id = "ounce", Name = "Ounce", Symbol = "oz", Category = "Weight", FactorToBase = 0.028349523125m, Aliases = ["ounce", "ounces"] },
            new() { Id = "stone", Name = "Stone", Symbol = "st", Category = "Weight", FactorToBase = 6.35029318m, Aliases = ["stone", "stones"] },

            // Temperature (Base unit: kelvin)
            new() { Id = "celsius", Name = "Celsius", Symbol = "°C", Category = "Temperature", Aliases = ["celsius", "centigrade", "c", "degc"] },
            new() { Id = "fahrenheit", Name = "Fahrenheit", Symbol = "°F", Category = "Temperature", Aliases = ["fahrenheit", "f", "degf"] },
            new() { Id = "kelvin", Name = "Kelvin", Symbol = "K", Category = "Temperature", Aliases = ["kelvin", "k"] },
            new() { Id = "rankine", Name = "Rankine", Symbol = "°R", Category = "Temperature", Aliases = ["rankine", "r"] },

            // Volume (Base unit: liter)
            new() { Id = "liter", Name = "Liter", Symbol = "L", Category = "Volume", FactorToBase = 1.0m, Aliases = ["liter", "liters", "litre", "litres", "l"] },
            new() { Id = "milliliter", Name = "Milliliter", Symbol = "mL", Category = "Volume", FactorToBase = 0.001m, Aliases = ["milliliter", "milliliters", "ml"] },
            new() { Id = "cubicmeter", Name = "Cubic Meter", Symbol = "m³", Category = "Volume", FactorToBase = 1000m, Aliases = ["cubicmeter", "cubicmeters", "m3"] },
            new() { Id = "gallon", Name = "US Gallon", Symbol = "gal", Category = "Volume", FactorToBase = 3.785411784m, Aliases = ["gallon", "gallons", "usgallon"] },
            new() { Id = "pint", Name = "US Pint", Symbol = "pt", Category = "Volume", FactorToBase = 0.473176473m, Aliases = ["pint", "pints", "uspint"] },

            // Speed (Base unit: m/s)
            new() { Id = "mps", Name = "Meter per second", Symbol = "m/s", Category = "Speed", FactorToBase = 1.0m, Aliases = ["mps", "meterpersecond"] },
            new() { Id = "kmh", Name = "Kilometer per hour", Symbol = "km/h", Category = "Speed", FactorToBase = 1.0m / 3.6m, Aliases = ["kmh", "kph", "kilometerperhour"] },
            new() { Id = "mph", Name = "Mile per hour", Symbol = "mph", Category = "Speed", FactorToBase = 0.44704m, Aliases = ["mph", "mileperhour"] },
            new() { Id = "knot", Name = "Knot", Symbol = "kn", Category = "Speed", FactorToBase = 0.514444444444m, Aliases = ["knot", "knots", "kt"] },

            // Time (Base unit: second)
            new() { Id = "second", Name = "Second", Symbol = "s", Category = "Time", FactorToBase = 1.0m, Aliases = ["second", "seconds", "sec"] },
            new() { Id = "millisecond", Name = "Millisecond", Symbol = "ms", Category = "Time", FactorToBase = 0.001m, Aliases = ["millisecond", "milliseconds"] },
            new() { Id = "minute", Name = "Minute", Symbol = "min", Category = "Time", FactorToBase = 60m, Aliases = ["minute", "minutes"] },
            new() { Id = "hour", Name = "Hour", Symbol = "h", Category = "Time", FactorToBase = 3600m, Aliases = ["hour", "hours", "hr"] },
            new() { Id = "day", Name = "Day", Symbol = "d", Category = "Time", FactorToBase = 86400m, Aliases = ["day", "days"] }
        };
    }
}
