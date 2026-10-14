namespace Conversions.Middleware;

using Conversions.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

/// <summary>
/// Global middleware capturing domain, validation, and unhandled exceptions to return consistent RFC 7807 ProblemDetails.
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "An error occurred handling request: {Path}", context.Request.Path);

        var (statusCode, problemDetails) = exception switch
        {
            ValidationException validationEx => (
                (int)HttpStatusCode.BadRequest,
                CreateValidationProblemDetails(context, validationEx)
            ),
            UnitNotFoundException notFoundEx => (
                (int)HttpStatusCode.NotFound,
                new ProblemDetails
                {
                    Status = (int)HttpStatusCode.NotFound,
                    Title = "Unit Not Found",
                    Detail = notFoundEx.Message,
                    Instance = context.Request.Path
                }
            ),
            IncompatibleUnitsException incompatibleEx => (
                (int)HttpStatusCode.BadRequest,
                new ProblemDetails
                {
                    Status = (int)HttpStatusCode.BadRequest,
                    Title = "Incompatible Units",
                    Detail = incompatibleEx.Message,
                    Instance = context.Request.Path
                }
            ),
            InvalidConversionException invalidEx => (
                (int)HttpStatusCode.UnprocessableEntity,
                new ProblemDetails
                {
                    Status = (int)HttpStatusCode.UnprocessableEntity,
                    Title = "Invalid Conversion",
                    Detail = invalidEx.Message,
                    Instance = context.Request.Path
                }
            ),
            _ => (
                (int)HttpStatusCode.InternalServerError,
                new ProblemDetails
                {
                    Status = (int)HttpStatusCode.InternalServerError,
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred. Please try again later.",
                    Instance = context.Request.Path
                }
            )
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = statusCode;

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, options));
    }

    private static ValidationProblemDetails CreateValidationProblemDetails(HttpContext context, ValidationException ex)
    {
        var errors = ex.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray()
            );

        return new ValidationProblemDetails(errors)
        {
            Status = (int)HttpStatusCode.BadRequest,
            Title = "Validation Error",
            Detail = "One or more validation errors occurred.",
            Instance = context.Request.Path
        };
    }
}
