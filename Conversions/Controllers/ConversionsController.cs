using Conversions.Application.DTOs;
using Conversions.Application.Services;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Conversions.Controllers;

/// <summary>
/// Provides endpoints for unit conversions, supported units catalog, and category discovery.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class ConversionsController : ControllerBase
{
    private readonly IConversionService _conversionService;
    private readonly IValidator<ConversionRequest> _validator;

    public ConversionsController(
        IConversionService conversionService,
        IValidator<ConversionRequest> validator)
    {
        _conversionService = conversionService ?? throw new ArgumentNullException(nameof(conversionService));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    }

    /// <summary>
    /// Converts a numerical value from one unit of measurement to another.
    /// </summary>
    /// <param name="request">The conversion parameters (value, source unit, target unit, optional precision).</param>
    /// <returns>The conversion result including converted value, units, category, and applied calculation formula.</returns>
    /// <response code="200">Conversion completed successfully.</response>
    /// <response code="400">Invalid input parameters, missing fields, or incompatible unit categories.</response>
    /// <response code="404">Source or target unit was not recognized.</response>
    /// <response code="422">Value violates physical/mathematical domain constraints (e.g. below absolute zero).</response>
    [HttpPost]
    [ProducesResponseType(typeof(ConversionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ConversionResponse>> ConvertAsync([FromBody] ConversionRequest request)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var response = _conversionService.Convert(request);
        return Ok(response);
    }

    /// <summary>
    /// Converts a numerical value using query parameters (convenient for GET/browser requests).
    /// </summary>
    /// <param name="value">Numerical value to convert.</param>
    /// <param name="sourceUnit">Source unit identifier or alias (e.g. "m", "meter", "celsius", "kg").</param>
    /// <param name="targetUnit">Target unit identifier or alias (e.g. "ft", "foot", "fahrenheit", "lb").</param>
    /// <param name="precision">Optional decimal rounding precision (0-15).</param>
    /// <returns>The conversion result.</returns>
    /// <response code="200">Conversion completed successfully.</response>
    /// <response code="400">Invalid input parameters or incompatible unit categories.</response>
    /// <response code="404">Source or target unit was not recognized.</response>
    /// <response code="422">Value violates physical domain constraints.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ConversionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ConversionResponse>> ConvertViaQueryAsync(
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

        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var response = _conversionService.Convert(request);
        return Ok(response);
    }

    /// <summary>
    /// Retrieves all supported units of measurement, optionally filtered by category.
    /// </summary>
    /// <param name="category">Optional filter (e.g. "Length", "Temperature", "Weight", "Volume", "Speed", "Time").</param>
    /// <returns>List of supported units and their metadata.</returns>
    [HttpGet("units")]
    [ProducesResponseType(typeof(IEnumerable<UnitDto>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<UnitDto>> GetUnits([FromQuery] string? category = null)
    {
        var units = _conversionService.GetUnits(category);
        return Ok(units);
    }

    /// <summary>
    /// Retrieves all supported measurement categories.
    /// </summary>
    /// <returns>List of category names.</returns>
    [HttpGet("categories")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<string>> GetCategories()
    {
        var categories = _conversionService.GetCategories();
        return Ok(categories);
    }
}
