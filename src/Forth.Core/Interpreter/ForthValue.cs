using System.Runtime.InteropServices;

namespace Forth.Core.Interpreter;

/// <summary>
/// Represents a typed value on the Forth stack, avoiding boxing for primitives.
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public struct ForthValue
{
    /// <summary>
    /// The type of the value.
    /// </summary>
    [FieldOffset(0)]
    public ValueType Type;

    /// <summary>
    /// Long integer value.
    /// </summary>
    [FieldOffset(8)]
    public long LongValue;

    /// <summary>
    /// Double precision floating point value.
    /// </summary>
    [FieldOffset(8)]
    public double DoubleValue;

    /// <summary>
    /// String value.
    /// </summary>
    [FieldOffset(16)]
    public string? StringValue;

    /// <summary>
    /// General object value.
    /// </summary>
    [FieldOffset(16)]
    public object? ObjectValue;

    /// <summary>
    /// Creates a ForthValue from a long.
    /// </summary>
    public static ForthValue FromLong(long value) => new() { Type = ValueType.Long, LongValue = value };

    /// <summary>
    /// Creates a ForthValue from a double.
    /// </summary>
    public static ForthValue FromDouble(double value) => new() { Type = ValueType.Double, DoubleValue = value };

    /// <summary>
    /// Creates a ForthValue from a string.
    /// </summary>
    public static ForthValue FromString(string value) => new() { Type = ValueType.String, StringValue = value };

    /// <summary>
    /// Creates a ForthValue from an object.
    /// </summary>
    public static ForthValue FromObject(object value) => new() { Type = ValueType.Object, ObjectValue = value };

    /// <summary>
    /// Gets the value as a long, throwing if not the correct type.
    /// </summary>
    public long AsLong => Type == ValueType.Long ? LongValue : throw new ForthException(ForthErrorCode.TypeError, "Expected long");

    /// <summary>
    /// Gets the value as a double, throwing if not the correct type.
    /// </summary>
    public double AsDouble => Type == ValueType.Double ? DoubleValue : throw new ForthException(ForthErrorCode.TypeError, "Expected double");

    /// <summary>
    /// Gets the value as a string, throwing if not the correct type.
    /// </summary>
    public string AsString => Type == ValueType.String ? StringValue! : throw new ForthException(ForthErrorCode.TypeError, "Expected string");

    /// <summary>
    /// Gets the value as an object, throwing if not the correct type.
    /// </summary>
    public object AsObject => Type == ValueType.Object ? ObjectValue! : throw new ForthException(ForthErrorCode.TypeError, "Expected object");

    /// <summary>
    /// Converts the value to an object for compatibility.
    /// </summary>
    public object ToObject() => Type switch
    {
        ValueType.Long => LongValue,
        ValueType.Double => DoubleValue,
        ValueType.String => StringValue!,
        ValueType.Object => ObjectValue!,
        _ => throw new ForthException(ForthErrorCode.TypeError, "Unknown type")
    };

    /// <summary>
    /// Tries to convert to long if it's a numeric type.
    /// </summary>
    public bool TryAsLong(out long value)
    {
        if (Type == ValueType.Long)
        {
            value = LongValue;
            return true;
        }
        if (Type == ValueType.Double && DoubleValue == (long)DoubleValue)
        {
            value = (long)DoubleValue;
            return true;
        }
        value = 0;
        return false;
    }

    /// <summary>
    /// String representation for debugging.
    /// </summary>
    public override string ToString() => Type switch
    {
        ValueType.Long => LongValue.ToString(),
        ValueType.Double => DoubleValue.ToString(),
        ValueType.String => $"\"{StringValue}\"",
        ValueType.Object => ObjectValue?.ToString() ?? "null",
        _ => "unknown"
    };
}

/// <summary>
/// Enumeration of value types.
/// </summary>
public enum ValueType : byte
{
    /// <summary>
    /// Represents a long integer value.
    /// </summary>
    Long,
    /// <summary>
    /// Represents a double precision floating point value.
    /// </summary>
    Double,
    /// <summary>
    /// Represents a string value.
    /// </summary>
    String,
    /// <summary>
    /// Represents a general object value.
    /// </summary>
    Object
}