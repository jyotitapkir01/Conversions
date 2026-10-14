namespace Conversions.Application.Services;

using Conversions.Application.DTOs;
using Conversions.Domain.Exceptions;
using Conversions.Domain.Interfaces;
using Conversions.Domain.Models;
using Conversions.Domain.Strategies;
using Microsoft.Extensions.Logging;

/// <summary>
/// Core application service orchestrating unit resolution, strategy dispatch, and response formatting.
/// </summary>
public class ConversionService : IConversionService
{
    private readonly IUnitRegistry _registry;
    private readonly IEnumerable<IConversionStrategy> _strategies;
    private readonly ILogger<ConversionService> _logger;

    public ConversionService(
        IUnitRegistry registry,
        IEnumerable<IConversionStrategy> strategies,
        ILogger<ConversionService> logger)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _strategies = strategies ?? throw new ArgumentNullException(nameof(strategies));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public ConversionResponse Convert(ConversionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogInformation(
            "Processing conversion: {Value} from '{SourceUnit}' to '{TargetUnit}' (Precision: {Precision})",
            request.Value, request.SourceUnit, request.TargetUnit, request.Precision);

        // 1. Resolve source and target units
        UnitDefinition sourceUnit = _registry.GetRequiredUnit(request.SourceUnit);
        UnitDefinition targetUnit = _registry.GetRequiredUnit(request.TargetUnit);

        // 2. Validate category compatibility
        if (!_registry.AreCompatible(sourceUnit, targetUnit))
        {
            _logger.LogWarning(
                "Incompatible units conversion rejected: '{SourceUnit}' ({SourceCat}) to '{TargetUnit}' ({TargetCat})",
                sourceUnit.Name, sourceUnit.Category, targetUnit.Name, targetUnit.Category);

            throw new IncompatibleUnitsException(
                sourceUnit.Name, sourceUnit.Category,
                targetUnit.Name, targetUnit.Category);
        }

        // 3. Locate appropriate conversion strategy
        IConversionStrategy strategy = _strategies.FirstOrDefault(s => s.CanHandle(sourceUnit, targetUnit))
            ?? throw new InvalidConversionException(
                $"No conversion strategy is registered for category '{sourceUnit.Category}'.");

        // 4. Calculate converted value
        decimal rawResult = strategy.Convert(request.Value, sourceUnit, targetUnit);

        // 5. Apply rounding if precision is specified
        decimal finalResult = request.Precision.HasValue
            ? Math.Round(rawResult, request.Precision.Value, MidpointRounding.AwayFromZero)
            : rawResult;

        // 6. Describe calculation formula
        string formula = strategy.DescribeCalculation(request.Value, finalResult, sourceUnit, targetUnit);

        _logger.LogInformation(
            "Conversion succeeded: {Value} {SourceSymbol} = {Result} {TargetSymbol}",
            request.Value, sourceUnit.Symbol, finalResult, targetUnit.Symbol);

        return new ConversionResponse
        {
            Source = new UnitValueDto
            {
                Value = request.Value,
                Unit = sourceUnit.Name,
                Symbol = sourceUnit.Symbol
            },
            Target = new UnitValueDto
            {
                Value = finalResult,
                Unit = targetUnit.Name,
                Symbol = targetUnit.Symbol
            },
            Category = sourceUnit.Category,
            Formula = formula
        };
    }

    public IReadOnlyCollection<UnitDto> GetUnits(string? category = null)
    {
        var units = string.IsNullOrWhiteSpace(category)
            ? _registry.GetAllUnits()
            : _registry.GetUnitsByCategory(category);

        return units.Select(MapToDto).ToList().AsReadOnly();
    }

    public IReadOnlyCollection<string> GetCategories()
    {
        return _registry.GetCategories();
    }

    private static UnitDto MapToDto(UnitDefinition unit) => new()
    {
        Id = unit.Id,
        Name = unit.Name,
        Symbol = unit.Symbol,
        Category = unit.Category,
        IsBaseUnit = unit.IsBaseUnit,
        Aliases = unit.Aliases
    };
}
