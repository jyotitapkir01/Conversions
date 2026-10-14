namespace Conversions.Tests.Infrastructure;

using Conversions.Domain.Exceptions;
using Conversions.Domain.Models;
using Conversions.Infrastructure;
using FluentAssertions;
using Xunit;

public class HardcodedUnitRegistryTests
{
    private readonly HardcodedUnitRegistry _registry = new();

    [Theory]
    [InlineData("meter", "meter")]
    [InlineData("m", "meter")]
    [InlineData("METERS", "meter")]
    [InlineData("  feet  ", "foot")]
    [InlineData("kg", "kilogram")]
    [InlineData("celsius", "celsius")]
    [InlineData("°C", "celsius")]
    [InlineData("fahrenheit", "fahrenheit")]
    [InlineData("°F", "fahrenheit")]
    public void FindUnit_ValidIdentifierOrAlias_ReturnsUnit(string query, string expectedId)
    {
        // Act
        var unit = _registry.FindUnit(query);

        // Assert
        unit.Should().NotBeNull();
        unit!.Id.Should().Be(expectedId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("unknown_unit_xyz")]
    public void FindUnit_InvalidOrUnknown_ReturnsNull(string query)
    {
        // Act
        var unit = _registry.FindUnit(query);

        // Assert
        unit.Should().BeNull();
    }

    [Fact]
    public void GetRequiredUnit_Unknown_ThrowsUnitNotFoundException()
    {
        // Act
        Action act = () => _registry.GetRequiredUnit("unknown_fake_unit");

        // Assert
        act.Should().Throw<UnitNotFoundException>()
            .WithMessage("*unknown_fake_unit*");
    }

    [Fact]
    public void GetAllUnits_ContainsCoreRequiredCategories()
    {
        // Act
        var units = _registry.GetAllUnits();

        // Assert
        units.Should().NotBeEmpty();
        units.Should().Contain(u => u.Category == CategoryConstants.Length);
        units.Should().Contain(u => u.Category == CategoryConstants.Weight);
        units.Should().Contain(u => u.Category == CategoryConstants.Temperature);
    }

    [Fact]
    public void GetUnitsByCategory_Length_ReturnsOnlyLengthUnits()
    {
        // Act
        var lengthUnits = _registry.GetUnitsByCategory(CategoryConstants.Length);

        // Assert
        lengthUnits.Should().NotBeEmpty();
        lengthUnits.Should().OnlyContain(u => u.Category == CategoryConstants.Length);
    }

    [Fact]
    public void AreCompatible_SameCategory_ReturnsTrue()
    {
        // Arrange
        var meter = _registry.GetRequiredUnit("meter");
        var foot = _registry.GetRequiredUnit("foot");

        // Act & Assert
        _registry.AreCompatible(meter, foot).Should().BeTrue();
    }

    [Fact]
    public void AreCompatible_DifferentCategories_ReturnsFalse()
    {
        // Arrange
        var meter = _registry.GetRequiredUnit("meter");
        var kilogram = _registry.GetRequiredUnit("kilogram");

        // Act & Assert
        _registry.AreCompatible(meter, kilogram).Should().BeFalse();
    }
}
