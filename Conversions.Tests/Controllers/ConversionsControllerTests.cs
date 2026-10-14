namespace Conversions.Tests.Controllers;

using Conversions.Application.DTOs;
using Conversions.Application.Services;
using Conversions.Application.Validators;
using Conversions.Controllers;
using Conversions.Domain.Exceptions;
using Conversions.Domain.Interfaces;
using Conversions.Domain.Strategies;
using Conversions.Infrastructure;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

public class ConversionsControllerTests
{
    private readonly ConversionsController _controller;

    public ConversionsControllerTests()
    {
        var registry = new HardcodedUnitRegistry();
        var strategies = new IConversionStrategy[]
        {
            new LinearConversionStrategy(),
            new TemperatureConversionStrategy()
        };
        var logger = NullLogger<ConversionService>.Instance;
        var service = new ConversionService(registry, strategies, logger);
        var validator = new ConversionRequestValidator();

        _controller = new ConversionsController(service, validator);
    }

    [Fact]
    public async Task ConvertAsync_ValidPayload_ReturnsOkWithConvertedResult()
    {
        // Arrange
        var request = new ConversionRequest
        {
            Value = 100,
            SourceUnit = "meters",
            TargetUnit = "feet",
            Precision = 2
        };

        // Act
        var actionResult = await _controller.ConvertAsync(request);

        // Assert
        actionResult.Result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)actionResult.Result!;
        var response = okResult.Value.Should().BeOfType<ConversionResponse>().Subject;

        response.Source.Value.Should().Be(100m);
        response.Target.Value.Should().Be(328.08m);
        response.Category.Should().Be("Length");
    }

    [Fact]
    public async Task ConvertAsync_InvalidPayload_ThrowsValidationException()
    {
        // Arrange
        var request = new ConversionRequest
        {
            Value = 100,
            SourceUnit = "", // Missing source unit
            TargetUnit = "feet"
        };

        // Act
        Func<Task> act = async () => await _controller.ConvertAsync(request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task ConvertViaQueryAsync_ValidParams_ReturnsOkWithConvertedResult()
    {
        // Act
        var actionResult = await _controller.ConvertViaQueryAsync(
            value: 0,
            sourceUnit: "celsius",
            targetUnit: "fahrenheit",
            precision: 1);

        // Assert
        actionResult.Result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)actionResult.Result!;
        var response = okResult.Value.Should().BeOfType<ConversionResponse>().Subject;

        response.Target.Value.Should().Be(32.0m);
        response.Category.Should().Be("Temperature");
    }

    [Fact]
    public void GetUnits_NoFilter_ReturnsAllUnits()
    {
        // Act
        var actionResult = _controller.GetUnits();

        // Assert
        actionResult.Result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)actionResult.Result!;
        var units = okResult.Value.Should().BeAssignableTo<IEnumerable<UnitDto>>().Subject;

        units.Should().NotBeEmpty();
    }

    [Fact]
    public void GetCategories_ReturnsCategoryList()
    {
        // Act
        var actionResult = _controller.GetCategories();

        // Assert
        actionResult.Result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)actionResult.Result!;
        var categories = okResult.Value.Should().BeAssignableTo<IEnumerable<string>>().Subject;

        categories.Should().Contain(new[] { "Length", "Weight", "Temperature" });
    }
}
