using System;
using System.Globalization;
using System.Numerics;
using Perlang.Extensions;

#nullable enable
namespace Perlang.Parser;

internal static class NumberParser
{
    public static object Parse(NumericToken numericToken)
    {
        string numberCharacters = (string)numericToken.Literal!;

        if (numericToken.IsFractional)
        {
            // TODO: This is a mess. We currently treat all floating point values as _double_, which is insane. We
            // TODO: should probably have a "use smallest possible type" logic as below for integers, for floating point
            // TODO: values as well. We could also consider supporting `decimal` while we're at it.

            // The explicit IFormatProvider is required to ensure we use 123.45 format, regardless of host OS
            // language/region settings. See #263 for more details.
            return Double.Parse(numberCharacters, CultureInfo.InvariantCulture);
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
                return (int)value;
            }
            else if (value <= UInt32.MaxValue)
            {
                return (uint)value;
            }
            else if (value <= Int64.MaxValue)
            {
                return (long)value;
            }
            else if (value <= UInt64.MaxValue)
            {
                return (ulong)value;
            }
            else // Anything else remains a BigInteger
            {
                return value;
            }
        }
    }

    public static object MakeNegative(object value)
    {
        if (value is double doubleValue)
        {
            return -doubleValue;
        }
        else if (value is int doubleInt)
        {
            return -doubleInt;
        }
        else if (value is uint doubleUint)
        {
            long negativeValue = -doubleUint;

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
                return (int)negativeValue;
            }
            else
            {
                return negativeValue;
            }
        }
        else if (value is long longValue)
        {
            return -longValue;
        }
        else if (value is ulong ulongValue)
        {
            // Again, this needs to be handled specially to ensure that numbers that fit in a `long` doesn't use
            // BigInteger unnecessarily.
            BigInteger negativeValue = -new BigInteger(ulongValue);

            if (negativeValue >= Int64.MinValue)
            {
                return (long)negativeValue;
            }
            else
            {
                // All negative numbers that are too big to fit in any of the smaller signed integer types will go
                // through this code path.
                return negativeValue;
            }
        }
        else
        {
            throw new ArgumentException($"Type {value.GetType().ToTypeKeyword()} not supported");
        }
    }
}
