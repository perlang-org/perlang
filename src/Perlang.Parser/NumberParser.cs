using System;
using System.Globalization;
using System.Numerics;
using Perlang.Internal.Extensions;

#nullable enable
namespace Perlang.Parser;

internal static class NumberParser
{
    public static INumericLiteral Parse(NumericToken numericToken)
    {
        string numberCharacters = (string)numericToken.Literal!;

        if (numericToken.IsFractional)
        {
            if (numericToken.HasSuffix)
            {
                switch (numericToken.Suffix)
                {
                    case 'f':
                    {
                        // The explicit IFormatProvider is required to ensure we use 123.45 format, regardless of host OS
                        // language/region settings. See #263 for more details.
                        float value = Single.Parse(numberCharacters, CultureInfo.InvariantCulture);
                        return new FloatingPointLiteral<float>(value);
                    }

                    case 'd':
                    {
                        // The explicit IFormatProvider is required to ensure we use 123.45 format, regardless of host OS
                        // language/region settings. See #263 for more details.
                        double value = Double.Parse(numberCharacters, CultureInfo.InvariantCulture);
                        return new FloatingPointLiteral<double>(value);
                    }

                    default:
                        throw new InvalidOperationException($"Numeric literal suffix {numericToken.Suffix} is not supported");
                }
            }
            else
            {
                // No suffix provided => use `double` precision by default, just like C#
                double value = Double.Parse(numberCharacters, CultureInfo.InvariantCulture);
                return new FloatingPointLiteral<double>(value);
            }
        }
        else
        {
            // Any potential preceding '-' character has already been taken care of at this stage => we can treat
            // the number as an unsigned value. However, we still try to coerce it to the smallest signed or
            // unsigned integer type in which it will fit (but never smaller than 32-bit). This coincidentally
            // follows the same semantics as how C# does it, for simplicity.

            BigInteger value = numericToken.NumberBase switch
            {
                NumericToken.Base.DECIMAL =>
                    BigInteger.Parse(numberCharacters, numericToken.NumberStyles),

                NumericToken.Base.BINARY =>
                    Convert.ToUInt64(numberCharacters, 2),

                NumericToken.Base.OCTAL =>
                    Convert.ToUInt64(numberCharacters, 8),

                NumericToken.Base.HEXADECIMAL =>

                    // Quoting from
                    // https://docs.microsoft.com/en-us/dotnet/api/system.numerics.biginteger.parse?view=net-5.0#System_Numerics_BigInteger_Parse_System_ReadOnlySpan_System_Char__System_Globalization_NumberStyles_System_IFormatProvider_
                    //
                    // If value is a hexadecimal string, the Parse(String, NumberStyles) method interprets value as a
                    // negative number stored by using two's complement representation if its first two hexadecimal
                    // digits are greater than or equal to 0x80. In other words, the method interprets the highest-order
                    // bit of the first byte in value as the sign bit. To make sure that a hexadecimal string is
                    // correctly interpreted as a positive number, the first digit in value must have a value of zero.
                    //
                    // We presume that all hexadecimals should be treated as positive numbers for now.
                    BigInteger.Parse('0' + numberCharacters, numericToken.NumberStyles),

                _ =>
                    throw new InvalidOperationException($"Base {(int)numericToken.NumberBase} not supported")
            };

            if (value <= Int32.MaxValue)
            {
                return new IntegerLiteral<int>((int)value);
            }
            else if (value <= UInt32.MaxValue)
            {
                return new IntegerLiteral<uint>((uint)value);
            }
            else if (value <= Int64.MaxValue)
            {
                return new IntegerLiteral<long>((long)value);
            }
            else if (value <= UInt64.MaxValue)
            {
                return new IntegerLiteral<ulong>((ulong)value);
            }
            else // Anything else remains a BigInteger
            {
                return new IntegerLiteral<BigInteger>(value);
            }
        }
    }

    public static object MakeNegative(object value)
    {
        if (value is INumericLiteral numericLiteral)
        {
            if (numericLiteral.Value is float floatValue)
            {
                return new FloatingPointLiteral<float>(-floatValue);
            }
            else if (numericLiteral.Value is double doubleValue)
            {
                return new FloatingPointLiteral<double>(-doubleValue);
            }
            else if (numericLiteral.Value is int intValue)
            {
                return new IntegerLiteral<int>(-intValue);
            }
            else if (numericLiteral.Value is uint uintValue)
            {
                long negativeValue = -uintValue;

                // This is a special hack to ensure that the value -2147483648 gets returned as an `int` and not a `long`.
                // Some details available in #302, summarized here in brief:
                //
                // The value 2147483648 is too large for an `int` => gets parsed into a `ulong` where it will fit. Once it
                // has been made negative, the value -2147483648 is again small enough to fit in an `int` => the code below
                // will narrow it down to comply with the "smallest type possible" design principle.
                //
                // Rationale: Two's complement: https://en.wikipedia.org/wiki/Two%27s_complement
                if (negativeValue >= Int32.MinValue)
                {
                    return new IntegerLiteral<int>((int)negativeValue);
                }
                else
                {
                    return new IntegerLiteral<long>(negativeValue);
                }
            }
            else if (numericLiteral.Value is long longValue)
            {
                return new IntegerLiteral<long>(-longValue);
            }
            else if (numericLiteral.Value is ulong ulongValue)
            {
                // Again, this needs to be handled specially to ensure that numbers that fit in a `long` doesn't use
                // BigInteger unnecessarily.
                BigInteger negativeValue = -new BigInteger(ulongValue);

                if (negativeValue >= Int64.MinValue)
                {
                    return new IntegerLiteral<long>((long)negativeValue);
                }
                else
                {
                    // All negative numbers that are too big to fit in any of the smaller signed integer types will go
                    // through this code path.
                    return new IntegerLiteral<BigInteger>(negativeValue);
                }
            }
            else
            {
                throw new ArgumentException($"Type {numericLiteral.Value.GetType().ToTypeKeyword()} not supported");
            }
        }
        else
        {
            throw new ArgumentException($"Type {value.GetType().ToTypeKeyword()} not supported");
        }
    }
}
