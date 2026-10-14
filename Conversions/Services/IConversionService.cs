namespace Conversions.Services;

using Conversions.Models;

public interface IConversionService
{
    ConversionResponse Convert(ConversionRequest request);
    List<UnitDto> GetUnits(string? category = null);
    List<string> GetCategories();
}
