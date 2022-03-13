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

    object INumericLiteral.Value => Value;

    public IntegerLiteral(T value)
    {
        Value = value;

        BitsUsed = value switch
        {
            int intValue => Math.ILogB(intValue),
            uint uintValue => Math.ILogB(uintValue),
            long longValue => Math.ILogB(longValue),
            ulong ulongValue => Math.ILogB(ulongValue),
            BigInteger bigintValue => bigintValue.GetBitLength(),
            _ => throw new ArgumentException($"Unsupported numeric type encountered: {value.GetType().Name}")
        };
    }

    public override string? ToString()
    {
        return Value.ToString();
    }
}
