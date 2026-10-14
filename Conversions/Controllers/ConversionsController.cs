using Conversions.Models;
using Conversions.Services;
using Microsoft.AspNetCore.Mvc;

namespace Conversions.Controllers;

[ApiController]
[Route("api/[controller]")]
[Route("api/v1/[controller]")]
public class ConversionsController : ControllerBase
{
    private readonly IConversionService _conversionService;

    public ConversionsController(IConversionService conversionService)
    {
        _conversionService = conversionService;
    }

    /// <summary>
    /// Converts a numerical value from one unit to another.
    /// </summary>
    [HttpPost]
    public IActionResult Convert([FromBody] ConversionRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = _conversionService.Convert(request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Converts a numerical value via query parameters.
    /// </summary>
    [HttpGet]
    public IActionResult ConvertViaQuery(
        [FromQuery] decimal value,
        [FromQuery] string sourceUnit,
        [FromQuery] string targetUnit,
        [FromQuery] int? precision = null)
    {
        var request = new ConversionRequest
        {
            Value = value,
            SourceUnit = sourceUnit,
            TargetUnit = targetUnit,
            Precision = precision
        };

        return Convert(request);
    }

    /// <summary>
    /// Returns supported units, optionally filtered by category.
    /// </summary>
    [HttpGet("units")]
    public IActionResult GetUnits([FromQuery] string? category = null)
    {
        var units = _conversionService.GetUnits(category);
        return Ok(units);
    }

    /// <summary>
    /// Returns all supported measurement categories.
    /// </summary>
    [HttpGet("categories")]
    public IActionResult GetCategories()
    {
        var categories = _conversionService.GetCategories();
        return Ok(categories);
    }
}
