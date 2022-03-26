#nullable enable
using System;

namespace Perlang.Parser;

internal readonly struct FloatingPointLiteral<T> : INumericLiteral
    where T : notnull
{
    internal T Value { get; }

    /// <inheritdoc cref="INumericLiteral.BitsUsed"/>
    public long BitsUsed { get; }

    object INumericLiteral.Value => Value;

    public FloatingPointLiteral(T value)
    {
        Value = value;

        BitsUsed = value switch
        {
            double doubleValue => Math.ILogB(doubleValue),
            _ => throw new ArgumentException($"Unsupported numeric type encountered: {value.GetType().Name}")
        };
    }

    public override string? ToString()
    {
        return Value.ToString();
    }
}
