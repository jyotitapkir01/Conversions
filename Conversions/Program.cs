using Conversions.Application.Services;
using Conversions.Application.Validators;
using Conversions.Domain.Interfaces;
using Conversions.Domain.Strategies;
using Conversions.Infrastructure;
using Conversions.Middleware;
using FluentValidation;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger / OpenAPI documentation
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Unit Conversion API",
        Version = "v1",
        Description = "A RESTful Web API for high-precision unit conversions across Length, Weight, Temperature, Volume, Speed, and Time."
    });

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Register Domain & Application Services
builder.Services.AddSingleton<IUnitRegistry, HardcodedUnitRegistry>();
builder.Services.AddSingleton<IConversionStrategy, LinearConversionStrategy>();
builder.Services.AddSingleton<IConversionStrategy, TemperatureConversionStrategy>();
builder.Services.AddScoped<IConversionService, ConversionService>();

// Register FluentValidation Validators
builder.Services.AddValidatorsFromAssemblyContaining<ConversionRequestValidator>();

var app = builder.Build();

// Global Exception Handling Middleware (RFC 7807 ProblemDetails)
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// Configure Swagger UI (accessible at root '/' and '/swagger')
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Unit Conversion API v1");
    options.RoutePrefix = string.Empty; // Serves Swagger UI at application root
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Partial Program class declaration to support integration testing fixtures if needed
public partial class Program { }
