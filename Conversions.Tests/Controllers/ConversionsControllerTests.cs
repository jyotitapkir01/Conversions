namespace Conversions.Tests.Controllers;

using Conversions.Controllers;
using Conversions.Models;
using Conversions.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;

public class ConversionsControllerTests
{
    private readonly ConversionsController _controller;

    public ConversionsControllerTests()
    {
        var service = new ConversionService();
        _controller = new ConversionsController(service);
    }

    [Fact]
    public void Convert_ValidRequest_ReturnsOkWithResult()
    {
        var request = new ConversionRequest
        {
            Value = 100,
            SourceUnit = "meters",
            TargetUnit = "feet",
            Precision = 2
        };

        var actionResult = _controller.Convert(request);

        actionResult.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)actionResult;
        var response = okResult.Value.Should().BeOfType<ConversionResponse>().Subject;

        response.Value.Should().Be(100m);
        response.ConvertedValue.Should().Be(328.08m);
        response.Category.Should().Be("Length");
    }

    [Fact]
    public void Convert_InvalidModelState_ReturnsBadRequest()
    {
        _controller.ModelState.AddModelError("SourceUnit", "Source unit is required.");

        var request = new ConversionRequest
        {
            Value = 100,
            SourceUnit = "",
            TargetUnit = "feet"
        };

        var actionResult = _controller.Convert(request);

        actionResult.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void Convert_UnknownUnit_ReturnsBadRequestWithError()
    {
        var request = new ConversionRequest
        {
            Value = 10,
            SourceUnit = "unknown_xyz",
            TargetUnit = "meter"
        };

        var actionResult = _controller.Convert(request);

        actionResult.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void Convert_IncompatibleCategories_ReturnsBadRequestWithError()
    {
        var request = new ConversionRequest
        {
            Value = 10,
            SourceUnit = "meter",
            TargetUnit = "kilogram"
        };

        var actionResult = _controller.Convert(request);

        actionResult.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void ConvertViaQuery_ValidParams_ReturnsOkWithResult()
    {
        var actionResult = _controller.ConvertViaQuery(
            value: 0,
            sourceUnit: "celsius",
            targetUnit: "fahrenheit",
            precision: 1);

        actionResult.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)actionResult;
        var response = okResult.Value.Should().BeOfType<ConversionResponse>().Subject;

        response.ConvertedValue.Should().Be(32.0m);
        response.Category.Should().Be("Temperature");
    }

    [Fact]
    public void GetUnits_ReturnsListOfUnits()
    {
        var actionResult = _controller.GetUnits();

        actionResult.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)actionResult;
        var units = okResult.Value.Should().BeAssignableTo<IEnumerable<UnitDto>>().Subject;

        units.Should().NotBeEmpty();
    }

    [Fact]
    public void GetCategories_ReturnsCategoryList()
    {
        var actionResult = _controller.GetCategories();

        actionResult.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)actionResult;
        var categories = okResult.Value.Should().BeAssignableTo<IEnumerable<string>>().Subject;

        categories.Should().Contain(new[] { "Length", "Weight", "Temperature" });
    }
}
