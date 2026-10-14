# Unit Conversion Web API

MVC Web API for converting numerical values between units of measurement (Length, Temperature, Weight/Mass, Volume, Speed, and Time).

Built with standard ASP.NET Core conventions (Controllers, Models, Services), minimal exception handling, and an intuitive design that easily scales to support hundreds of units and external databases.

---

## Table of Contents

- [Features](#features)
- [Architecture & Simplicity](#architecture--simplicity)
  - [Design Decisions & Trade-Offs](#design-decisions--trade-offs)
  - [How the System Scales to Hundreds of Units](#how-the-system-scales-to-hundreds-of-units)
- [Supported Categories & Units](#supported-categories--units)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Run with .NET CLI](#run-with-net-cli)
  - [Run with Visual Studio / VS Code](#run-with-visual-studio--vs-code)
  - [Interactive Swagger UI](#interactive-swagger-ui)
- [API Reference & Examples](#api-reference--examples)
  - [1. Convert Units (POST)](#1-convert-units-post)
  - [2. Convert Units via Query Parameters (GET)](#2-convert-units-via-query-parameters-get)
  - [3. List Categories (GET)](#3-list-categories-get)
  - [4. List Units (GET)](#4-list-units-get)
  - [5. Error Handling](#5-error-handling)
- [Running Automated Tests](#running-automated-tests)
- [Project Structure](#project-structure)

---

## Features

- **Standard MVC Web API Structure**: Straightforward `Controllers`, `Models`, and `Services` layers without over-complicated abstractions.
- **Multiple Categories**: Length, Temperature, Weight/Mass, Volume, Speed, and Time.
- **Flexible Unit Resolution**: Case-insensitive resolution supporting IDs, symbols, and plurals/aliases (e.g., `m`, `meter`, `meters`, `metre`).
- **Minimal, Clean Exception Handling**: Handled via standard ASP.NET Core `ModelState` validation and clean `ArgumentException` responses (`400 Bad Request`).
- **High-Precision Calculations**: Uses `decimal` (128-bit) arithmetic to avoid floating-point inaccuracies, with optional precision rounding.
- **Interactive Swagger UI**: Explore and test all endpoints directly in the browser at the application root (`/`).
- **Complete Test Suite**: Comprehensive xUnit tests covering conversions, boundary cases, error conditions, and controllers.

---

## Architecture & Simplicity

The application uses standard ASP.NET Core dependency injection and a 3-layer MVC architecture:

```
[ HTTP Request ]
       │
       ▼
[ ConversionsController ] ── Validates request via DataAnnotations ([Required], [Range])
       │
       ▼
[ ConversionService ] ───── Resolves units, performs conversion & formatting
       │
       ▼
[ HTTP Response ]
```

### Design Decisions & Trade-Offs

| Decision | Rationale | Trade-Off |
| :--- | :--- | :--- |
| **Standard MVC Layers (Controllers, Models, Services)** | Keeps the codebase accessible, intuitive, and easy to explain. Avoids over-engineering for a focused Web API. | Less rigid than full enterprise Clean Architecture, but significantly easier to maintain and navigate. |
| **Minimal Exception Handling** | The controller uses standard `ModelState` validation and catches `ArgumentException` for domain errors (unknown units, incompatible categories, negative absolute zero), returning simple `400 Bad Request` JSON. | No need for heavy custom middleware or multiple custom exception classes. |
| **Canonical Base-Unit Normalization** | For linear categories (Length, Weight, Volume, Speed, Time), each unit defines a `FactorToBase` relative to a reference unit (e.g. meter, kg). Conversion is `(value * sourceFactor) / targetFactor`. | Requires choosing a base unit per category, but eliminates the need for $N \times (N - 1)$ manual pair combinations. |
| **Kelvin Pivot for Temperature** | Temperature is non-linear due to offsets. Normalizing input to Kelvin ($T_K$) first and then converting Kelvin to the target unit makes adding new temperature scales straightforward. | Handled via a dedicated helper method rather than complex external strategy classes. |
| **`decimal` Precision** | Floating-point types (`double`, `float`) suffer from binary representation quirks (e.g. `0.1 + 0.2 != 0.3`). `decimal` ensures financial and mathematical accuracy. | Slower than native CPU floating-point operations, but negligible for REST APIs. |

### How the System Scales to Hundreds of Units

1. **Adding New Linear Units**:
   - Adding a unit only requires 1 entry with its `FactorToBase` (e.g., `new() { Id = "yard", FactorToBase = 0.9144m }`).
   - The conversion formula `(Value * SourceFactor) / TargetFactor` remains identical for all linear units.
2. **Moving to a Database (Future Scale)**:
   - The `IConversionService` contract decouples callers from data storage.
   - To load hundreds of units from SQLite, PostgreSQL, or SQL Server, simply replace the in-memory list with an EF Core `DbContext.Units` query.

---

## Supported Categories & Units

| Category | Reference Base Unit | Supported Units & Common Aliases |
| :--- | :--- | :--- |
| **Length** | Meter (`m`) | Meter (`m`, `meters`), Kilometer (`km`), Centimeter (`cm`), Millimeter (`mm`), Mile (`mi`), Yard (`yd`), Foot (`ft`, `feet`), Inch (`in`), Nautical Mile (`nmi`) |
| **Weight / Mass** | Kilogram (`kg`) | Kilogram (`kg`, `kilo`), Gram (`g`), Milligram (`mg`), Metric Ton (`t`, `tonne`), Pound (`lb`, `lbs`), Ounce (`oz`), Stone (`st`) |
| **Temperature** | Kelvin (`K`) | Celsius (`°C`, `c`), Fahrenheit (`°F`, `f`), Kelvin (`K`), Rankine (`°R`, `r`) |
| **Volume** | Liter (`L`) | Liter (`l`), Milliliter (`ml`), Cubic Meter (`m³`), US Gallon (`gal`), US Pint (`pt`) |
| **Speed** | Meter per second (`m/s`) | Meter/sec (`m/s`), Kilometer/hour (`km/h`), Mile/hour (`mph`), Knot (`kn`) |
| **Time** | Second (`s`) | Second (`s`, `sec`), Millisecond (`ms`), Minute (`min`), Hour (`h`, `hr`), Day (`d`) |

---

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

### Run with .NET CLI

1. Open your terminal in the solution root directory:
   ```bash
   dotnet build
   ```

2. Run the Web API:
   ```bash
   dotnet run --project Conversions
   ```

3. The API will start on:
   - `http://localhost:5022`

### Interactive Swagger UI

Open your browser to:

👉 **[http://localhost:5022](http://localhost:5022)**

You will see the Swagger UI where you can test each endpoint interactively.

---

## API Reference & Examples

### 1. Convert Units (POST)

**Endpoint:** `POST /api/conversions`  
**Headers:** `Content-Type: application/json`

#### Request
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
  "value": 100,
  "sourceUnit": "Meter",
  "convertedValue": 328.08,
  "targetUnit": "Foot",
  "category": "Length",
  "formula": "100 m × 3.2808398950131233595800524934 = 328.08 ft"
}
```

---

### 2. Convert Units via Query Parameters (GET)

**Endpoint:** `GET /api/conversions?value=25&sourceUnit=celsius&targetUnit=fahrenheit&precision=1`

#### Response (`200 OK`)
```json
{
  "value": 25,
  "sourceUnit": "Celsius",
  "convertedValue": 77.0,
  "targetUnit": "Fahrenheit",
  "category": "Temperature",
  "formula": "(25 °C × 9/5) + 32 = 77 °F"
}
```

---

### 3. List Categories (GET)

**Endpoint:** `GET /api/conversions/categories`

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

### 4. List Units (GET)

**Endpoint:** `GET /api/conversions/units?category=Length`

#### Response (`200 OK`)
```json
[
  {
    "id": "meter",
    "name": "Meter",
    "symbol": "m",
    "category": "Length",
    "aliases": ["meter", "meters", "metre", "metres"]
  },
  {
    "id": "kilometer",
    "name": "Kilometer",
    "symbol": "km",
    "category": "Length",
    "aliases": ["kilometer", "kilometers", "kilometre", "kilometres"]
  }
]
```

---

### 5. Error Handling

#### Incompatible Categories (`400 Bad Request`)
Attempting to convert meters to kilograms:
```json
{
  "error": "Cannot convert between incompatible categories: 'Meter' (Length) and 'Kilogram' (Weight)."
}
```

#### Unknown Unit (`400 Bad Request`)
```json
{
  "error": "Unit 'unknown_unit' is not supported."
}
```

#### Validation Error (`400 Bad Request`)
When required fields are missing:
```json
{
  "errors": {
    "SourceUnit": ["Source unit is required."]
  }
}
```

---

## Running Automated Tests

Run the test suite with:

```bash
dotnet test
```

Expected output:
```text
Passed!  - Failed: 0, Passed: 40, Skipped: 0, Total: 40
```

## Project Structure

```
Conversions/
├── Conversions.sln                    # Solution entry point
├── .editorconfig                      # Formatting rules
├── .gitignore                         # Git ignore file
├── README.md                          # Documentation
│
├── Conversions/                       # ASP.NET Core Web API
│   ├── Conversions.csproj             # Project configuration (.NET 9)
│   ├── Conversions.http               # Sample HTTP requests
│   ├── Program.cs                     # Startup and DI configuration
│   ├── appsettings.json               # Application configuration
│   ├── Controllers/
│   │   └── ConversionsController.cs   # HTTP endpoints (POST, GET, metadata)
│   ├── Models/
│   │   ├── ConversionRequest.cs       # Input DTO with validation attributes
│   │   ├── ConversionResponse.cs      # Output DTO
│   │   └── UnitDefinition.cs          # Unit definition and factor
│   └── Services/
│       ├── IConversionService.cs      # Service interface
│       └── ConversionService.cs       # Conversion logic & unit lookup
│
└── Conversions.Tests/                 # xUnit Test Project
    ├── Conversions.Tests.csproj
    ├── Controllers/
    │   └── ConversionsControllerTests.cs # Controller tests
    └── Services/
        └── ConversionServiceTests.cs    # Conversion logic tests
```
