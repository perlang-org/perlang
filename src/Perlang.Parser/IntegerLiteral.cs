#nullable enable
using System;
using System.Numerics;

namespace Perlang.Parser;

internal class IntegerLiteral<T> : INumericLiteral
    where T : notnull
{
    internal T Value { get; }

    /// <inheritdoc cref="INumericLiteral.BitsUsed"/>
    public long BitsUsed { get; }

    public bool IsPositive { get; }

    object INumericLiteral.Value => Value;

    public IntegerLiteral(T value)
    {
        Value = value;

        BitsUsed = value switch
        {
            int intValue => (int)Math.Ceiling(Math.Log2(intValue)),
            uint uintValue => (int)Math.Ceiling(Math.Log2(uintValue)),
            long longValue => (int)Math.Ceiling(Math.Log2(longValue)),
            ulong ulongValue => (int)Math.Ceiling(Math.Log2(ulongValue)),
            BigInteger bigintValue => bigintValue.GetBitLength(),
            _ => throw new ArgumentException($"Unsupported numeric type encountered: {value.GetType().Name}")
        };

        IsPositive = value switch
        {
            int intValue => intValue >= 0,
            long longValue => longValue >= 0,
            BigInteger bigintValue => bigintValue >= 0,
            uint => true,
            ulong => true,
            _ => throw new ArgumentException($"Unsupported numeric type encountered: {value.GetType().Name}")
        };
    }

    public override string? ToString()
    {
        return Value.ToString();
    }
}
