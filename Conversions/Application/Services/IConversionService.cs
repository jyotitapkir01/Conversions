namespace Conversions.Application.Services;

using Conversions.Application.DTOs;

/// <summary>
/// Service contract for executing conversions and retrieving supported units metadata.
/// </summary>
public interface IConversionService
{
    /// <summary>
    /// Executes a conversion based on the specified request.
    /// </summary>
    ConversionResponse Convert(ConversionRequest request);

    /// <summary>
    /// Returns supported units, optionally filtered by category.
    /// </summary>
    IReadOnlyCollection<UnitDto> GetUnits(string? category = null);

    /// <summary>
    /// Returns all supported measurement categories.
    /// </summary>
    IReadOnlyCollection<string> GetCategories();
}
