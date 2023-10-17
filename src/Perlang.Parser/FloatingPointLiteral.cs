#nullable enable
using System;

namespace Perlang.Parser;

internal readonly struct FloatingPointLiteral<T> : IFloatingPointLiteral, INumericLiteral
    where T : notnull
{
    public object Value { get; }
    public string NumberCharacters { get; }

    /// <inheritdoc cref="INumericLiteral.BitsUsed"/>
    public long BitsUsed { get; }

    public bool IsPositive { get; }

    public FloatingPointLiteral(T value, string numberCharacters)
    {
        Value = value;
        NumberCharacters = numberCharacters;

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
