#nullable enable
using System;

namespace Perlang.Parser;

internal readonly struct FloatingPointLiteral<T> : INumericLiteral
    where T : notnull
{
    internal T Value { get; }

    /// <inheritdoc cref="INumericLiteral.BitsUsed"/>
    public long BitsUsed { get; }

    public bool IsPositive { get; }

    object INumericLiteral.Value => Value;

    public FloatingPointLiteral(T value)
    {
        Value = value;

        BitsUsed = value switch
        {
            float floatValue => (int)Math.Ceiling(Math.Log2(floatValue)),
            double doubleValue => (int)Math.Ceiling(Math.Log2(doubleValue)),
            _ => throw new ArgumentException($"Unsupported numeric type encountered: {value.GetType().Name}")
        };

        IsPositive = value switch
        {
            float floatValue => floatValue >= 0,
            double doubleValue => doubleValue >= 0,
            _ => throw new ArgumentException($"Unsupported numeric type encountered: {value.GetType().Name}")
        };
    }

    public override string? ToString()
    {
        return Value.ToString();
    }
}
