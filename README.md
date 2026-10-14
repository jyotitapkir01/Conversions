# Unit Conversion API

A robust, extensible RESTful Web API built with **ASP.NET Core (.NET 9)** to convert numerical values between various units of measurement (Length, Temperature, Weight/Mass, Volume, Speed, and Time).

Designed following clean, modular software engineering practices suitable for a real-world team environment, with support for future scalability to hundreds of units and non-linear conversion formulas.

---

## Table of Contents

- [Features](#features)
- [Architecture & Design](#architecture--design)
  - [Design Decisions & Trade-Offs](#design-decisions--trade-offs)
  - [How the System Scales to Hundreds of Units](#how-the-system-scales-to-hundreds-of-units)
- [Supported Measurement Categories & Units](#supported-measurement-categories--units)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Run with .NET CLI](#run-with-net-cli)
  - [Run with Visual Studio / VS Code](#run-with-visual-studio--vs-code)
  - [Interactive Swagger UI](#interactive-swagger-ui)
- [API Reference & Examples](#api-reference--examples)
  - [1. Convert Units (POST)](#1-convert-units-post)
  - [2. Convert Units via Query Parameters (GET)](#2-convert-units-via-query-parameters-get)
  - [3. List Supported Categories (GET)](#3-list-supported-categories-get)
  - [4. List Supported Units (GET)](#4-list-supported-units-get)
  - [5. Error Responses](#5-error-responses)
- [Running Automated Tests](#running-automated-tests)
- [Project Structure](#project-structure)

---

## Features

- **Multi-Category Conversions**: Supports Length, Temperature, Weight/Mass, Volume, Speed, and Time.
- **Flexible Unit Resolution**: Recognizes unit IDs, symbols, and common aliases/plurals (e.g. `m`, `meter`, `meters`, `metre`).
- **High-Precision Calculations**: Uses `decimal` (128-bit) arithmetic to eliminate floating-point precision errors, with optional user-specified decimal rounding.
- **Strategy Pattern Architecture**: Separates linear ratio conversions from affine/non-linear conversions (e.g. Temperature with offset baselines).
- **Extensible Unit Registry**: Decoupled behind `IUnitRegistry` interface for seamless migration to a database or configuration file.
- **Input Validation**: Validates payloads using FluentValidation (field presence, lengths, precision ranges).
- **RFC 7807 Standard Error Handling**: Centralized global exception middleware returns structured `ProblemDetails` for errors (400 Bad Request, 404 Not Found, 422 Unprocessable Entity, 500 Internal Error).
- **Interactive OpenAPI/Swagger**: Self-documenting API with XML summaries, served at the application root (`/`).
- **Thorough Test Coverage**: 80+ unit and integration tests covering domain strategies, services, validation, controllers, and edge cases.

---

## Architecture & Design

The solution follows a clean, layered structure with clear separation of concerns:

```
[ HTTP Request ]
       │
       ▼
[ ConversionsController ] ── (Validates input DTO)
       │
       ▼
[ ConversionService ] ─────── (Coordinates workflow)
    ├── [ IUnitRegistry ] ─── (Resolves units & checks category compatibility)
    └── [ IConversionStrategy ]
            ├── LinearConversionStrategy      (Length, Weight, Volume, Speed, Time)
            └── TemperatureConversionStrategy (Celsius, Fahrenheit, Kelvin, Rankine)
       │
       ▼
[ HTTP Response / ProblemDetails ]
```

### Design Decisions & Trade-Offs

| Decision | Rationale | Trade-Off |
| :--- | :--- | :--- |
| **`decimal` vs `double`** | Financial and scientific conversions require exact decimal representations without binary floating-point rounding quirks (e.g. `0.1 + 0.2 != 0.3` in `double`). | Slightly slower computational speed than native CPU IEEE 754 floats, but negligible for REST API loads. |
| **Canonical Base Unit (Pivot)** | Every linear unit defines a `ConversionFactorToBase` (e.g., meters for Length, kilograms for Weight). Conversion is simply `(Value * SourceFactor) / TargetFactor`. Adding $N$ units requires $N$ conversion factors instead of $N \times (N - 1)$ manual pair combinations. | Requires picking a canonical base unit for each category. |
| **Strategy Pattern for Conversions** | Linear conversions have a constant multiplier; temperature conversions have temperature offsets ($T_K = T_C + 273.15$, $(T_F - 32) \times \frac{5}{9}$). The `IConversionStrategy` interface allows handling both transparently without polluting the codebase with `if/else` checks. | Adds an interface and multiple strategy implementations, but follows Open/Closed Principle (OCP). |
| **In-Memory Registry Behind Interface** | For this challenge, units are hardcoded in `HardcodedUnitRegistry` implementing `IUnitRegistry`. Because `ConversionService` only depends on `IUnitRegistry`, moving to an EF Core database implementation in the future requires zero changes to application logic. | Data is compiled into the binary rather than dynamic at runtime, but perfectly fits current requirements. |
| **Global Exception Middleware** | Domain exceptions (`UnitNotFoundException`, `IncompatibleUnitsException`, `InvalidConversionException`) are mapped to HTTP status codes (`404`, `400`, `422`) via standard RFC 7807 `ProblemDetails`. | Requires keeping exception mappings synchronized if new domain exceptions are introduced. |

### How the System Scales to Hundreds of Units

1. **Adding New Linear Units**:
   - Simply add a new `UnitDefinition` to the registry with its `ConversionFactorToBase` and aliases.
   - No code changes are required in `ConversionService` or `LinearConversionStrategy`.
2. **Adding New Categories**:
   - If the new category is linear (e.g., Energy: Joules, Calories, kWh; or Pressure: Pascal, Bar, PSI), add the units with their base factor. The `LinearConversionStrategy` automatically handles them.
   - If the category requires custom formulas (e.g., logarithmic units like Decibels), implement a new `IConversionStrategy` and register it in `Program.cs`.
3. **Database Migration**:
   - Implement `EfCoreUnitRegistry : IUnitRegistry` backed by SQLite/PostgreSQL/SQL Server.
   - Swap `builder.Services.AddSingleton<IUnitRegistry, HardcodedUnitRegistry>()` to `builder.Services.AddScoped<IUnitRegistry, EfCoreUnitRegistry>()`.

---

## Supported Measurement Categories & Units

| Category | Canonical Base Unit | Supported Units & Common Aliases |
| :--- | :--- | :--- |
| **Length** | Meter (`m`) | Meter (`m`), Kilometer (`km`), Centimeter (`cm`), Millimeter (`mm`), Mile (`mi`), Yard (`yd`), Foot (`ft`, `feet`), Inch (`in`), Nautical Mile (`nmi`) |
| **Weight / Mass** | Kilogram (`kg`) | Kilogram (`kg`, `kilo`), Gram (`g`), Milligram (`mg`), Metric Ton (`t`, `tonne`), Pound (`lb`, `lbs`), Ounce (`oz`), Stone (`st`) |
| **Temperature** | Kelvin (`K`) | Celsius (`°C`, `c`), Fahrenheit (`°F`, `f`), Kelvin (`K`), Rankine (`°R`, `r`) |
| **Volume** | Liter (`L`) | Liter (`l`, `litre`), Milliliter (`ml`, `mL`), Cubic Meter (`m³`, `m3`), US Gallon (`gal`), US Pint (`pt`) |
| **Speed** | Meter per second (`m/s`) | Meter/sec (`m/s`), Kilometer/hour (`km/h`, `kmh`), Mile/hour (`mph`), Knot (`kn`, `kt`) |
| **Time** | Second (`s`) | Second (`s`, `sec`), Millisecond (`ms`), Minute (`min`), Hour (`h`, `hr`), Day (`d`) |

*All unit lookups are case-insensitive and support standard names, symbols, and plural forms.*

---

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) (or latest stable .NET SDK)

### Run with .NET CLI

1. Clone or open the repository folder:
   ```bash
   cd Conversions
   ```

2. Restore dependencies and build the solution:
   ```bash
   dotnet build
   ```

3. Run the Web API project:
   ```bash
   dotnet run --project Conversions
   ```

4. The API will start at:
   - `http://localhost:5022`
   - `https://localhost:7011`

### Run with Visual Studio / VS Code

- **Visual Studio 2022**: Open `Conversions.sln`, select `Conversions` as the startup project, and press `F5` or `Ctrl+F5`.
- **VS Code**: Open the folder, press `Ctrl+Shift+B` to build, or run `dotnet run --project Conversions` in the integrated terminal.

### Interactive Swagger UI

Once running, navigate to the root URL in your browser:

👉 **[http://localhost:5022](http://localhost:5022)** (or `http://localhost:5022/swagger`)

From here, you can explore the OpenAPI schema and execute test requests directly in your browser.

---

## API Reference & Examples

### 1. Convert Units (POST)

**Endpoint:** `POST /api/v1/conversions`  
**Headers:** `Content-Type: application/json`

#### Request Body
```json
{
  "value": 100,
  "sourceUnit": "meter",
  "targetUnit": "foot",
  "precision": 2
}
```

#### Response (`200 OK`)
```json
{
  "source": {
    "value": 100,
    "unit": "Meter",
    "symbol": "m"
  },
  "target": {
    "value": 328.08,
    "unit": "Foot",
    "symbol": "ft"
  },
  "category": "Length",
  "formula": "100 m × 3.2808398950131233595800524934 = 328.08 ft"
}
```

---

### 2. Convert Units via Query Parameters (GET)

Convenient for browser queries or simple scripts:

**Endpoint:** `GET /api/v1/conversions?value=25&sourceUnit=celsius&targetUnit=fahrenheit&precision=1`

#### Response (`200 OK`)
```json
{
  "source": {
    "value": 25,
    "unit": "Celsius",
    "symbol": "°C"
  },
  "target": {
    "value": 77.0,
    "unit": "Fahrenheit",
    "symbol": "°F"
  },
  "category": "Temperature",
  "formula": "(25 °C × 9/5) + 32 = 77 °F"
}
```

---

### 3. List Supported Categories (GET)

**Endpoint:** `GET /api/v1/conversions/categories`

#### Response (`200 OK`)
```json
[
  "Length",
  "Speed",
  "Temperature",
  "Time",
  "Volume",
  "Weight"
]
```

---

### 4. List Supported Units (GET)

**Endpoint:** `GET /api/v1/conversions/units?category=Length`

#### Response (`200 OK`)
```json
[
  {
    "id": "meter",
    "name": "Meter",
    "symbol": "m",
    "category": "Length",
    "isBaseUnit": true,
    "aliases": ["meter", "meters", "metre", "metres"]
  },
  {
    "id": "kilometer",
    "name": "Kilometer",
    "symbol": "km",
    "category": "Length",
    "isBaseUnit": false,
    "aliases": ["kilometer", "kilometers", "kilometre", "kilometres"]
  }
]
```

---

### 5. Error Responses

All error responses strictly follow the **RFC 7807 ProblemDetails** format:

#### Incompatible Categories (`400 Bad Request`)
Attempting to convert meters to kilograms:
```json
{
  "type": "about:blank",
  "title": "Incompatible Units",
  "status": 400,
  "detail": "Cannot convert between incompatible categories: 'Meter' (Length) and 'Kilogram' (Weight).",
  "instance": "/api/v1/conversions"
}
```

#### Unknown Unit (`404 Not Found`)
```json
{
  "type": "about:blank",
  "title": "Unit Not Found",
  "status": 404,
  "detail": "Unit 'lightyear' was not recognized.",
  "instance": "/api/v1/conversions"
}
```

#### Physical Domain Violation (`422 Unprocessable Entity`)
Attempting to convert temperature below absolute zero ($< 0\text{ K}$ / $-273.15\text{ }^\circ\text{C}$):
```json
{
  "type": "about:blank",
  "title": "Invalid Conversion",
  "status": 422,
  "detail": "Temperature value '-300 °C' is below absolute zero (0 K / -273.15 °C / -459.67 °F).",
  "instance": "/api/v1/conversions"
}
```

---

## Running Automated Tests

The solution includes an extensive suite of automated tests using **xUnit** and **FluentAssertions**.

Run all tests from the repository root:

```bash
dotnet test
```

Expected output:
```text
Passed!  - Failed: 0, Passed: 83, Skipped: 0, Total: 83
```

Test coverage includes:
- **Unit Registry Tests**: Name resolution, case-insensitivity, alias mapping, unknown unit handling.
- **Linear Strategy Tests**: Metric and imperial conversions across length, weight, volume, speed, and time.
- **Temperature Strategy Tests**: Celsius $\leftrightarrow$ Fahrenheit $\leftrightarrow$ Kelvin $\leftrightarrow$ Rankine, absolute zero boundaries.
- **Validator Tests**: Required fields, maximum lengths, precision limits.
- **Service Tests**: Strategy orchestration, precision rounding, formula output generation.
- **Controller Tests**: HTTP responses, status codes, query-string endpoints.

---

## Project Structure

```
Conversions/
├── Conversions.sln                    # Solution entry point
├── .editorconfig                      # Shared team code styling rules
├── .gitignore                         # Standard .NET git ignore rules
├── README.md                          # Project documentation
│
├── Conversions/                       # ASP.NET Core Web API Project
│   ├── Conversions.csproj             # Dependencies & targets (net9.0)
│   ├── Conversions.http               # Ready-to-run HTTP requests for VS / VS Code
│   ├── Program.cs                     # Application setup, DI, Swagger, & middleware
│   ├── appsettings.json               # Environment configuration
│   ├── Controllers/
│   │   └── ConversionsController.cs   # REST API endpoints (POST, GET, units catalog)
│   ├── Application/
│   │   ├── DTOs/                      # Request and Response transfer models
│   │   ├── Services/                  # Conversion orchestration service & contract
│   │   └── Validators/                # FluentValidation rules
│   ├── Domain/
│   │   ├── Models/                    # UnitDefinition, CategoryConstants
│   │   ├── Interfaces/                # IUnitRegistry contract
│   │   ├── Strategies/                # IConversionStrategy, Linear & Temperature strategies
│   │   └── Exceptions/                # Domain-specific exception types
│   ├── Infrastructure/
│   │   └── HardcodedUnitRegistry.cs   # In-memory unit dictionary & alias index
│   └── Middleware/
│       └── GlobalExceptionHandlerMiddleware.cs  # RFC 7807 ProblemDetails error handler
│
└── Conversions.Tests/                 # xUnit Test Project
    ├── Conversions.Tests.csproj       # Test dependencies (xUnit, FluentAssertions)
    ├── Domain/                        # Strategy calculation tests
    ├── Infrastructure/                # Registry lookup & alias tests
    ├── Application/                   # Service & validator unit tests
    └── Controllers/                   # Controller action & status code tests
```
