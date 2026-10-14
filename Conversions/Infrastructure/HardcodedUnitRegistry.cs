namespace Conversions.Infrastructure;

using Conversions.Domain.Exceptions;
using Conversions.Domain.Interfaces;
using Conversions.Domain.Models;

/// <summary>
/// In-memory hardcoded unit registry.
/// Provides out-of-the-box definitions for Length, Weight, Temperature, Volume, Speed, and Time.
/// Designed behind <see cref="IUnitRegistry"/> so it can be swapped with a database or external configuration source in the future.
/// </summary>
public class HardcodedUnitRegistry : IUnitRegistry
{
    private readonly List<UnitDefinition> _units;
    private readonly Dictionary<string, UnitDefinition> _lookup;

    public HardcodedUnitRegistry()
    {
        _units = BuildInitialUnits();
        _lookup = new Dictionary<string, UnitDefinition>(StringComparer.OrdinalIgnoreCase);

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

    public UnitDefinition? FindUnit(string unitIdentifierOrAlias)
    {
        if (string.IsNullOrWhiteSpace(unitIdentifierOrAlias))
        {
            return null;
        }

        string key = unitIdentifierOrAlias.Trim();
        return _lookup.GetValueOrDefault(key);
    }

    public UnitDefinition GetRequiredUnit(string unitIdentifierOrAlias)
    {
        return FindUnit(unitIdentifierOrAlias)
            ?? throw new UnitNotFoundException(unitIdentifierOrAlias);
    }

    public IReadOnlyCollection<UnitDefinition> GetAllUnits() => _units.AsReadOnly();

    public IReadOnlyCollection<UnitDefinition> GetUnitsByCategory(string category)
    {
        return _units
            .Where(u => string.Equals(u.Category, category, StringComparison.OrdinalIgnoreCase))
            .ToList()
            .AsReadOnly();
    }

    public IReadOnlyCollection<string> GetCategories()
    {
        return _units
            .Select(u => u.Category)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(c => c)
            .ToList()
            .AsReadOnly();
    }

    public bool AreCompatible(UnitDefinition sourceUnit, UnitDefinition targetUnit)
    {
        return string.Equals(sourceUnit.Category, targetUnit.Category, StringComparison.OrdinalIgnoreCase);
    }

    private static List<UnitDefinition> BuildInitialUnits()
    {
        return new List<UnitDefinition>
        {
            // ==========================================
            // LENGTH (Base: Meter)
            // ==========================================
            new()
            {
                Id = "meter",
                Name = "Meter",
                Symbol = "m",
                Category = CategoryConstants.Length,
                IsBaseUnit = true,
                ConversionFactorToBase = 1.0m,
                Aliases = new[] { "meter", "meters", "metre", "metres" }
            },
            new()
            {
                Id = "kilometer",
                Name = "Kilometer",
                Symbol = "km",
                Category = CategoryConstants.Length,
                ConversionFactorToBase = 1000m,
                Aliases = new[] { "kilometer", "kilometers", "kilometre", "kilometres" }
            },
            new()
            {
                Id = "centimeter",
                Name = "Centimeter",
                Symbol = "cm",
                Category = CategoryConstants.Length,
                ConversionFactorToBase = 0.01m,
                Aliases = new[] { "centimeter", "centimeters", "centimetre", "centimetres" }
            },
            new()
            {
                Id = "millimeter",
                Name = "Millimeter",
                Symbol = "mm",
                Category = CategoryConstants.Length,
                ConversionFactorToBase = 0.001m,
                Aliases = new[] { "millimeter", "millimeters", "millimetre", "millimetres" }
            },
            new()
            {
                Id = "mile",
                Name = "Mile",
                Symbol = "mi",
                Category = CategoryConstants.Length,
                ConversionFactorToBase = 1609.344m,
                Aliases = new[] { "mile", "miles" }
            },
            new()
            {
                Id = "yard",
                Name = "Yard",
                Symbol = "yd",
                Category = CategoryConstants.Length,
                ConversionFactorToBase = 0.9144m,
                Aliases = new[] { "yard", "yards" }
            },
            new()
            {
                Id = "foot",
                Name = "Foot",
                Symbol = "ft",
                Category = CategoryConstants.Length,
                ConversionFactorToBase = 0.3048m,
                Aliases = new[] { "foot", "feet" }
            },
            new()
            {
                Id = "inch",
                Name = "Inch",
                Symbol = "in",
                Category = CategoryConstants.Length,
                ConversionFactorToBase = 0.0254m,
                Aliases = new[] { "inch", "inches" }
            },
            new()
            {
                Id = "nauticalmile",
                Name = "Nautical Mile",
                Symbol = "nmi",
                Category = CategoryConstants.Length,
                ConversionFactorToBase = 1852m,
                Aliases = new[] { "nauticalmile", "nauticalmiles", "nm" }
            },

            // ==========================================
            // WEIGHT / MASS (Base: Kilogram)
            // ==========================================
            new()
            {
                Id = "kilogram",
                Name = "Kilogram",
                Symbol = "kg",
                Category = CategoryConstants.Weight,
                IsBaseUnit = true,
                ConversionFactorToBase = 1.0m,
                Aliases = new[] { "kilogram", "kilograms", "kilo", "kilos" }
            },
            new()
            {
                Id = "gram",
                Name = "Gram",
                Symbol = "g",
                Category = CategoryConstants.Weight,
                ConversionFactorToBase = 0.001m,
                Aliases = new[] { "gram", "grams" }
            },
            new()
            {
                Id = "milligram",
                Name = "Milligram",
                Symbol = "mg",
                Category = CategoryConstants.Weight,
                ConversionFactorToBase = 0.000001m,
                Aliases = new[] { "milligram", "milligrams" }
            },
            new()
            {
                Id = "metricton",
                Name = "Metric Ton",
                Symbol = "t",
                Category = CategoryConstants.Weight,
                ConversionFactorToBase = 1000m,
                Aliases = new[] { "tonne", "tonnes", "metricton", "metrictons" }
            },
            new()
            {
                Id = "pound",
                Name = "Pound",
                Symbol = "lb",
                Category = CategoryConstants.Weight,
                ConversionFactorToBase = 0.45359237m,
                Aliases = new[] { "pound", "pounds", "lbs" }
            },
            new()
            {
                Id = "ounce",
                Name = "Ounce",
                Symbol = "oz",
                Category = CategoryConstants.Weight,
                ConversionFactorToBase = 0.028349523125m,
                Aliases = new[] { "ounce", "ounces" }
            },
            new()
            {
                Id = "stone",
                Name = "Stone",
                Symbol = "st",
                Category = CategoryConstants.Weight,
                ConversionFactorToBase = 6.35029318m,
                Aliases = new[] { "stone", "stones" }
            },

            // ==========================================
            // TEMPERATURE (Base: Kelvin)
            // ==========================================
            new()
            {
                Id = "kelvin",
                Name = "Kelvin",
                Symbol = "K",
                Category = CategoryConstants.Temperature,
                IsBaseUnit = true,
                Aliases = new[] { "kelvin" }
            },
            new()
            {
                Id = "celsius",
                Name = "Celsius",
                Symbol = "°C",
                Category = CategoryConstants.Temperature,
                Aliases = new[] { "celsius", "centigrade", "c", "degc" }
            },
            new()
            {
                Id = "fahrenheit",
                Name = "Fahrenheit",
                Symbol = "°F",
                Category = CategoryConstants.Temperature,
                Aliases = new[] { "fahrenheit", "f", "degf" }
            },
            new()
            {
                Id = "rankine",
                Name = "Rankine",
                Symbol = "°R",
                Category = CategoryConstants.Temperature,
                Aliases = new[] { "rankine", "r", "degr" }
            },

            // ==========================================
            // VOLUME (Base: Liter)
            // ==========================================
            new()
            {
                Id = "liter",
                Name = "Liter",
                Symbol = "L",
                Category = CategoryConstants.Volume,
                IsBaseUnit = true,
                ConversionFactorToBase = 1.0m,
                Aliases = new[] { "liter", "liters", "litre", "litres", "l" }
            },
            new()
            {
                Id = "milliliter",
                Name = "Milliliter",
                Symbol = "mL",
                Category = CategoryConstants.Volume,
                ConversionFactorToBase = 0.001m,
                Aliases = new[] { "milliliter", "milliliters", "millilitre", "millilitres", "ml" }
            },
            new()
            {
                Id = "cubicmeter",
                Name = "Cubic Meter",
                Symbol = "m³",
                Category = CategoryConstants.Volume,
                ConversionFactorToBase = 1000m,
                Aliases = new[] { "cubicmeter", "cubicmeters", "m3" }
            },
            new()
            {
                Id = "gallon",
                Name = "US Gallon",
                Symbol = "gal",
                Category = CategoryConstants.Volume,
                ConversionFactorToBase = 3.785411784m,
                Aliases = new[] { "gallon", "gallons", "usgallon", "usgallons" }
            },
            new()
            {
                Id = "pint",
                Name = "US Pint",
                Symbol = "pt",
                Category = CategoryConstants.Volume,
                ConversionFactorToBase = 0.473176473m,
                Aliases = new[] { "pint", "pints", "uspint" }
            },

            // ==========================================
            // SPEED (Base: Meter per second)
            // ==========================================
            new()
            {
                Id = "mps",
                Name = "Meter per second",
                Symbol = "m/s",
                Category = CategoryConstants.Speed,
                IsBaseUnit = true,
                ConversionFactorToBase = 1.0m,
                Aliases = new[] { "mps", "meterpersecond", "meterspersecond" }
            },
            new()
            {
                Id = "kmh",
                Name = "Kilometer per hour",
                Symbol = "km/h",
                Category = CategoryConstants.Speed,
                ConversionFactorToBase = 1.0m / 3.6m,
                Aliases = new[] { "kmh", "kph", "kilometerperhour", "kilometersperhour" }
            },
            new()
            {
                Id = "mph",
                Name = "Mile per hour",
                Symbol = "mph",
                Category = CategoryConstants.Speed,
                ConversionFactorToBase = 0.44704m,
                Aliases = new[] { "mph", "mileperhour", "milesperhour" }
            },
            new()
            {
                Id = "knot",
                Name = "Knot",
                Symbol = "kn",
                Category = CategoryConstants.Speed,
                ConversionFactorToBase = 0.5144444444444444444444444444m,
                Aliases = new[] { "knot", "knots", "kt" }
            },

            // ==========================================
            // TIME (Base: Second)
            // ==========================================
            new()
            {
                Id = "second",
                Name = "Second",
                Symbol = "s",
                Category = CategoryConstants.Time,
                IsBaseUnit = true,
                ConversionFactorToBase = 1.0m,
                Aliases = new[] { "second", "seconds", "sec", "secs" }
            },
            new()
            {
                Id = "millisecond",
                Name = "Millisecond",
                Symbol = "ms",
                Category = CategoryConstants.Time,
                ConversionFactorToBase = 0.001m,
                Aliases = new[] { "millisecond", "milliseconds" }
            },
            new()
            {
                Id = "minute",
                Name = "Minute",
                Symbol = "min",
                Category = CategoryConstants.Time,
                ConversionFactorToBase = 60m,
                Aliases = new[] { "minute", "minutes" }
            },
            new()
            {
                Id = "hour",
                Name = "Hour",
                Symbol = "h",
                Category = CategoryConstants.Time,
                ConversionFactorToBase = 3600m,
                Aliases = new[] { "hour", "hours", "hr", "hrs" }
            },
            new()
            {
                Id = "day",
                Name = "Day",
                Symbol = "d",
                Category = CategoryConstants.Time,
                ConversionFactorToBase = 86400m,
                Aliases = new[] { "day", "days" }
            }
        };
    }
}
