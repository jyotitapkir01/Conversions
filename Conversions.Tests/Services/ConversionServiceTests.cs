namespace Conversions.Tests.Services;

using Conversions.Models;
using Conversions.Services;
using FluentAssertions;
using Xunit;

public class ConversionServiceTests
{
    private readonly ConversionService _service = new();

    #region Length Conversions

    [Theory]
    [InlineData(1, "meter", "foot", 3.28084)]
    [InlineData(100, "meter", "foot", 328.084)]
    [InlineData(1, "kilometer", "mile", 0.621371)]
    [InlineData(1, "inch", "centimeter", 2.54)]
    [InlineData(12, "inch", "foot", 1.0)]
    [InlineData(3, "foot", "yard", 1.0)]
    public void Convert_LengthUnits_ReturnsExpectedResult(
        decimal inputValue,
        string sourceUnit,
        string targetUnit,
        decimal expectedValue)
    {
        var request = new ConversionRequest
        {
            Value = inputValue,
            SourceUnit = sourceUnit,
            TargetUnit = targetUnit
        };

        var response = _service.Convert(request);

        response.ConvertedValue.Should().BeApproximately(expectedValue, 0.001m);
        response.Category.Should().Be("Length");
    }

    #endregion

    #region Weight Conversions

    [Theory]
    [InlineData(1, "kilogram", "pound", 2.20462)]
    [InlineData(1000, "gram", "kilogram", 1.0)]
    [InlineData(16, "ounce", "pound", 1.0)]
    [InlineData(1, "stone", "pound", 14.0)]
    [InlineData(1, "metricton", "kilogram", 1000.0)]
    public void Convert_WeightUnits_ReturnsExpectedResult(
        decimal inputValue,
        string sourceUnit,
        string targetUnit,
        decimal expectedValue)
    {
        var request = new ConversionRequest
        {
            Value = inputValue,
            SourceUnit = sourceUnit,
            TargetUnit = targetUnit
        };

        var response = _service.Convert(request);

        response.ConvertedValue.Should().BeApproximately(expectedValue, 0.001m);
        response.Category.Should().Be("Weight");
    }

    #endregion

    #region Temperature Conversions

    [Theory]
    [InlineData(0, "celsius", "fahrenheit", 32.0)]
    [InlineData(100, "celsius", "fahrenheit", 212.0)]
    [InlineData(-40, "celsius", "fahrenheit", -40.0)]
    [InlineData(37, "celsius", "fahrenheit", 98.6)]
    [InlineData(32, "fahrenheit", "celsius", 0.0)]
    [InlineData(212, "fahrenheit", "celsius", 100.0)]
    [InlineData(0, "celsius", "kelvin", 273.15)]
    [InlineData(273.15, "kelvin", "celsius", 0.0)]
    public void Convert_TemperatureUnits_ReturnsExpectedResult(
        decimal inputValue,
        string sourceUnit,
        string targetUnit,
        decimal expectedValue)
    {
        var request = new ConversionRequest
        {
            Value = inputValue,
            SourceUnit = sourceUnit,
            TargetUnit = targetUnit
        };

        var response = _service.Convert(request);

        response.ConvertedValue.Should().BeApproximately(expectedValue, 0.01m);
        response.Category.Should().Be("Temperature");
    }

    [Fact]
    public void Convert_TemperatureBelowAbsoluteZero_ThrowsArgumentException()
    {
        var request = new ConversionRequest
        {
            Value = -300m,
            SourceUnit = "celsius",
            TargetUnit = "fahrenheit"
        };

        Action act = () => _service.Convert(request);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*below absolute zero*");
    }

    #endregion

    #region Volume, Speed, Time Conversions

    [Fact]
    public void Convert_VolumeLitersToGallons_ReturnsExpected()
    {
        var request = new ConversionRequest
        {
            Value = 3.78541m,
            SourceUnit = "liter",
            TargetUnit = "gallon"
        };

        var response = _service.Convert(request);
        response.ConvertedValue.Should().BeApproximately(1.0m, 0.01m);
    }

    [Fact]
    public void Convert_SpeedKmhToMph_ReturnsExpected()
    {
        var request = new ConversionRequest
        {
            Value = 100m,
            SourceUnit = "kmh",
            TargetUnit = "mph"
        };

        var response = _service.Convert(request);
        response.ConvertedValue.Should().BeApproximately(62.137m, 0.01m);
    }

    [Fact]
    public void Convert_TimeHoursToMinutes_ReturnsExpected()
    {
        var request = new ConversionRequest
        {
            Value = 2.5m,
            SourceUnit = "hour",
            TargetUnit = "minute"
        };

        var response = _service.Convert(request);
        response.ConvertedValue.Should().Be(150m);
    }

    #endregion

    #region Precision & Aliases

    [Fact]
    public void Convert_WithPrecision_RoundsProperly()
    {
        var request = new ConversionRequest
        {
            Value = 100m,
            SourceUnit = "meter",
            TargetUnit = "foot",
            Precision = 2
        };

        var response = _service.Convert(request);
        response.ConvertedValue.Should().Be(328.08m);
    }

    [Theory]
    [InlineData("m", "ft")]
    [InlineData("meter", "foot")]
    [InlineData("meters", "feet")]
    [InlineData("METRE", "FEET")]
    public void Convert_AliasSupport_ResolvesSameUnit(string source, string target)
    {
        var request = new ConversionRequest
        {
            Value = 10m,
            SourceUnit = source,
            TargetUnit = target,
            Precision = 2
        };

        var response = _service.Convert(request);
        response.ConvertedValue.Should().Be(32.81m);
    }

    #endregion

    #region Error Handling

    [Fact]
    public void Convert_UnknownSourceUnit_ThrowsArgumentException()
    {
        var request = new ConversionRequest
        {
            Value = 10m,
            SourceUnit = "nonexistent_unit",
            TargetUnit = "meter"
        };

        Action act = () => _service.Convert(request);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*Unit 'nonexistent_unit' is not supported*");
    }

    [Fact]
    public void Convert_UnknownTargetUnit_ThrowsArgumentException()
    {
        var request = new ConversionRequest
        {
            Value = 10m,
            SourceUnit = "meter",
            TargetUnit = "nonexistent_unit"
        };

        Action act = () => _service.Convert(request);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*Unit 'nonexistent_unit' is not supported*");
    }

    [Fact]
    public void Convert_IncompatibleCategories_ThrowsArgumentException()
    {
        var request = new ConversionRequest
        {
            Value = 10m,
            SourceUnit = "meter",    // Length
            TargetUnit = "kilogram"  // Weight
        };

        Action act = () => _service.Convert(request);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*incompatible categories*");
    }

    #endregion

    #region Discovery

    [Fact]
    public void GetCategories_ReturnsExpectedCategories()
    {
        var categories = _service.GetCategories();

        categories.Should().Contain(new[] { "Length", "Temperature", "Weight", "Volume", "Speed", "Time" });
    }

    [Fact]
    public void GetUnits_FilteredByCategory_ReturnsOnlyCategoryUnits()
    {
        var units = _service.GetUnits("Length");

        units.Should().NotBeEmpty();
        units.Should().OnlyContain(u => u.Category == "Length");
    }

    #endregion
}
