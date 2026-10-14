namespace Conversions.Domain.Interfaces;

using Conversions.Domain.Models;

/// <summary>
/// Registry abstraction for retrieving and resolving units of measurement.
/// Supports decoupling conversion logic from data storage (e.g. in-memory, database, external config).
/// </summary>
public interface IUnitRegistry
{
    /// <summary>
    /// Finds a unit by its unique identifier, symbol, or one of its known aliases (case-insensitive).
    /// </summary>
    UnitDefinition? FindUnit(string unitIdentifierOrAlias);

    /// <summary>
    /// Retrieves a unit by identifier or alias, throwing <see cref="Exceptions.UnitNotFoundException"/> if not found.
    /// </summary>
    UnitDefinition GetRequiredUnit(string unitIdentifierOrAlias);

    /// <summary>
    /// Returns all registered units.
    /// </summary>
    IReadOnlyCollection<UnitDefinition> GetAllUnits();

    /// <summary>
    /// Returns all registered units belonging to a specific category.
    /// </summary>
    IReadOnlyCollection<UnitDefinition> GetUnitsByCategory(string category);

    /// <summary>
    /// Returns all distinct category names currently registered.
    /// </summary>
    IReadOnlyCollection<string> GetCategories();

    /// <summary>
    /// Checks if two units belong to the same category and can be converted to one another.
    /// </summary>
    bool AreCompatible(UnitDefinition sourceUnit, UnitDefinition targetUnit);
}
