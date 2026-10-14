namespace Conversions.Domain.Exceptions;

/// <summary>
/// Base exception for domain conversion errors.
/// </summary>
public abstract class ConversionDomainException : Exception
{
    protected ConversionDomainException(string message) : base(message) { }
}

/// <summary>
/// Thrown when a requested unit name or alias is not recognized.
/// </summary>
public class UnitNotFoundException : ConversionDomainException
{
    public string UnitIdentifier { get; }

    public UnitNotFoundException(string unitIdentifier)
        : base($"Unit '{unitIdentifier}' was not recognized.")
    {
        UnitIdentifier = unitIdentifier;
    }
}

/// <summary>
/// Thrown when attempting to convert between units of incompatible measurement categories (e.g. Length to Weight).
/// </summary>
public class IncompatibleUnitsException : ConversionDomainException
{
    public string SourceUnit { get; }
    public string SourceCategory { get; }
    public string TargetUnit { get; }
    public string TargetCategory { get; }

    public IncompatibleUnitsException(
        string sourceUnit,
        string sourceCategory,
        string targetUnit,
        string targetCategory)
        : base($"Cannot convert between incompatible categories: '{sourceUnit}' ({sourceCategory}) and '{targetUnit}' ({targetCategory}).")
    {
        SourceUnit = sourceUnit;
        SourceCategory = sourceCategory;
        TargetUnit = targetUnit;
        TargetCategory = targetCategory;
    }
}

/// <summary>
/// Thrown when a conversion value is physically or mathematically invalid (e.g. below absolute zero).
/// </summary>
public class InvalidConversionException : ConversionDomainException
{
    public InvalidConversionException(string message) : base(message) { }
}
