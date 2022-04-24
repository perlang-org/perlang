#pragma warning disable SA1515
using System.Collections.Generic;
using Xunit;

// ReSharper disable InconsistentNaming

namespace Perlang.Tests.Integration.Operator.Binary;

/// <summary>
/// <see cref="MemberDataAttribute"/> provider for binary operators.
///
/// This class provides data for binary operators. It is centralized in a single class, to be able to conveniently
/// ensure that all supported combinations of data types are handled by all binary operators. When adding a new data
/// type, ideally only the methods in this class need to be updated; the tests themselves can remain intact.
///
/// See also <see cref="Typing.TypingTests"/>; the test data there might need to ensure that type inference works
/// correctly for the newly added type.
/// </summary>
public static class BinaryOperatorData
{
    public static IEnumerable<object[]> Greater =>
        new List<object[]>
        {
            new object[] { "2147483646", "2147483647", "False" },
            new object[] { "2147483647", "2147483647", "False" },
            new object[] { "2147483647", "2147483646", "True" },
            new object[] { "12", "34.0", "False" },
            new object[] { "2147483647", "33.0", "True" },
            new object[] { "12.0", "34.0", "False" },
            new object[] { "34.0", "33.0", "True" },
        };

    public static IEnumerable<object[]> Greater_unsupported_types =>
        new List<object[]>
        {
            new object[] { "12.0", "34", "Operands must be numbers, not double and int" },
            new object[] { "34.0", "33", "Operands must be numbers, not double and int" },
            new object[] { "4294967295", "33", "Operands must be numbers, not System.UInt32 and int" },
            new object[] { "33", "4294967295", "Operands must be numbers, not int and System.UInt32" },
        };

    public static IEnumerable<object[]> GreaterEqual =>
        new List<object[]>
        {
            new object[] { "2147483646", "2147483647", "False" },
            new object[] { "2147483647", "2147483647", "True" },
            new object[] { "2147483647", "2147483646", "True" },
            new object[] { "12", "34.0", "False" },
            new object[] { "2147483647", "33.0", "True" },
            new object[] { "12.0", "34.0", "False" },
            new object[] { "34.0", "33.0", "True" },
        };

    public static IEnumerable<object[]> GreaterEqual_unsupported_types =>
        new List<object[]>
        {
            new object[] { "12.0", "34", "Operands must be numbers, not double and int" },
            new object[] { "34.0", "33", "Operands must be numbers, not double and int" },
            new object[] { "4294967295", "33", "Operands must be numbers, not System.UInt32 and int" },
            new object[] { "33", "4294967295", "Operands must be numbers, not int and System.UInt32" }
        };

    public static IEnumerable<object[]> Less =>
        new List<object[]>
        {
            new object[] { "12", "34", "True" },
            new object[] { "12", "-34", "False" },
            new object[] { "12", "34.0", "True" },
            new object[] { "-12", "34", "True" },
            new object[] { "-12", "-34", "False" },
            new object[] { "-12", "-34.0", "False" },
            new object[] { "2147483646", "2147483647", "True" },
            new object[] { "2147483647", "2147483647", "False" },
            new object[] { "2147483647", "2147483646", "False" },
            new object[] { "2147483647", "33.0", "False" },

            new object[] { "2", "18446744073709551616", "True" },
            new object[] { "2", "9223372036854775807", "True" },
            new object[] { "4294967296", "9223372036854775807", "True" },
            new object[] { "9223372036854775807", "2", "False" },
            new object[] { "9223372036854775807", "9223372036854775807", "False" },
            new object[] { "9223372036854775807", "18446744073709551616", "True" },
            new object[] { "9223372036854775807", "12.0", "False" },
            new object[] { "18446744073709551616", "2", "False" },
            new object[] { "18446744073709551616", "4294967296", "False" },
            new object[] { "18446744073709551616", "9223372036854775807", "False" },
            new object[] { "18446744073709551616", "18446744073709551617", "True" },
            new object[] { "12.0", "34.0", "True" },
            new object[] { "34.0", "33.0", "False" },
        };

    public static IEnumerable<object[]> Less_unsupported_types =>
        new List<object[]>
        {
            new object[] { "2", "4294967295", "Operands must be numbers, not int and System.UInt32" },
            new object[] { "2", "18446744073709551615", "Operands must be numbers, not int and System.UInt64" },
            new object[] { "4294967295", "33", "Operands must be numbers, not System.UInt32 and int" },
            new object[] { "4294967295", "4294967295", "Operands must be numbers, not System.UInt32 and System.UInt32" },
            new object[] { "4294967295", "9223372036854775807", "Operands must be numbers, not System.UInt32 and long" },
            new object[] { "4294967295", "18446744073709551615", "Operands must be numbers, not System.UInt32 and System.UInt64" },
            new object[] { "4294967295", "18446744073709551616", "Operands must be numbers, not System.UInt32 and bigint" },
            new object[] { "4294967295", "12.0", "Operands must be numbers, not System.UInt32 and double" },
            new object[] { "9223372036854775807", "4294967295", "Operands must be numbers, not long and System.UInt32" },
            new object[] { "9223372036854775807", "18446744073709551615", "Operands must be numbers, not long and System.UInt64" },
            new object[] { "18446744073709551615", "2", "Operands must be numbers, not System.UInt64 and int" },
            new object[] { "18446744073709551615", "4294967295", "Operands must be numbers, not System.UInt64 and System.UInt32" },
            new object[] { "18446744073709551615", "9223372036854775807", "Operands must be numbers, not System.UInt64 and long" },
            new object[] { "18446744073709551615", "18446744073709551615", "Operands must be numbers, not System.UInt64 and System.UInt64" },
            new object[] { "18446744073709551615", "18446744073709551616", "Operands must be numbers, not System.UInt64 and bigint" },
            new object[] { "18446744073709551615", "12.0", "Operands must be numbers, not System.UInt64 and double" },
            new object[] { "18446744073709551616", "4294967295", "Operands must be numbers, not bigint and System.UInt32" },
            new object[] { "18446744073709551616", "18446744073709551615", "Operands must be numbers, not bigint and System.UInt64" },
            new object[] { "18446744073709551616", "12.0", "Operands must be numbers, not bigint and double" },
            new object[] { "-12.0", "-34", "Operands must be numbers, not double and int" },
            new object[] { "-12.0", "4294967295", "Operands must be numbers, not double and System.UInt32" },
            new object[] { "-12.0", "9223372036854775807", "Operands must be numbers, not double and long" },
            new object[] { "-12.0", "18446744073709551615", "Operands must be numbers, not double and System.UInt64" },
            new object[] { "-12.0", "18446744073709551616", "Operands must be numbers, not double and bigint" },
        };

    public static IEnumerable<object[]> LessEqual =>
        new List<object[]>
        {
            new object[] { "2147483646", "2147483647", "True" },
            new object[] { "2147483647", "2147483647", "True" },
            new object[] { "2147483647", "2147483646", "False" },
            new object[] { "12", "34.0", "True" },
            new object[] { "2147483647", "33.0", "False" },
            new object[] { "12.0", "34.0", "True" },
            new object[] { "34.0", "33.0", "False" },

            new object[] { "2", "18446744073709551616", "True" },
            new object[] { "2", "9223372036854775807", "True" },
            new object[] { "4294967296", "9223372036854775807", "True" },
            new object[] { "9223372036854775807", "2", "False" },
            new object[] { "9223372036854775807", "9223372036854775807", "True" },
            new object[] { "9223372036854775807", "18446744073709551616", "True" },
            new object[] { "9223372036854775807", "12.0", "False" },
            new object[] { "18446744073709551616", "2", "False" },
            new object[] { "18446744073709551616", "4294967296", "False" },
            new object[] { "18446744073709551616", "9223372036854775807", "False" },
            new object[] { "18446744073709551616", "18446744073709551617", "True" },
            new object[] { "12.0", "34.0", "True" },
            new object[] { "34.0", "33.0", "False" },
        };

    public static IEnumerable<object[]> LessEqual_unsupported_types =>
        new List<object[]>
        {
            new object[] { "2", "4294967295", "Operands must be numbers, not int and System.UInt32" },
            new object[] { "2", "18446744073709551615", "Operands must be numbers, not int and System.UInt64" },
            new object[] { "4294967295", "33", "Operands must be numbers, not System.UInt32 and int" },
            new object[] { "4294967295", "4294967295", "Operands must be numbers, not System.UInt32 and System.UInt32" },
            new object[] { "4294967295", "9223372036854775807", "Operands must be numbers, not System.UInt32 and long" },
            new object[] { "4294967295", "18446744073709551615", "Operands must be numbers, not System.UInt32 and System.UInt64" },
            new object[] { "4294967295", "18446744073709551616", "Operands must be numbers, not System.UInt32 and bigint" },
            new object[] { "4294967295", "12.0", "Operands must be numbers, not System.UInt32 and double" },
            new object[] { "9223372036854775807", "4294967295", "Operands must be numbers, not long and System.UInt32" },
            new object[] { "9223372036854775807", "18446744073709551615", "Operands must be numbers, not long and System.UInt64" },
            new object[] { "18446744073709551615", "2", "Operands must be numbers, not System.UInt64 and int" },
            new object[] { "18446744073709551615", "4294967295", "Operands must be numbers, not System.UInt64 and System.UInt32" },
            new object[] { "18446744073709551615", "9223372036854775807", "Operands must be numbers, not System.UInt64 and long" },
            new object[] { "18446744073709551615", "18446744073709551615", "Operands must be numbers, not System.UInt64 and System.UInt64" },
            new object[] { "18446744073709551615", "18446744073709551616", "Operands must be numbers, not System.UInt64 and bigint" },
            new object[] { "18446744073709551615", "12.0", "Operands must be numbers, not System.UInt64 and double" },
            new object[] { "18446744073709551616", "4294967295", "Operands must be numbers, not bigint and System.UInt32" },
            new object[] { "18446744073709551616", "18446744073709551615", "Operands must be numbers, not bigint and System.UInt64" },
            new object[] { "18446744073709551616", "12.0", "Operands must be numbers, not bigint and double" },
            new object[] { "-12.0", "-34", "Operands must be numbers, not double and int" },
            new object[] { "-12.0", "4294967295", "Operands must be numbers, not double and System.UInt32" },
            new object[] { "-12.0", "9223372036854775807", "Operands must be numbers, not double and long" },
            new object[] { "-12.0", "18446744073709551615", "Operands must be numbers, not double and System.UInt64" },
            new object[] { "-12.0", "18446744073709551616", "Operands must be numbers, not double and bigint" },
        };

    public static IEnumerable<object[]> NotEqual =>
        new List<object[]>
        {
            new object[] { "12", "34", "True" },
            new object[] { "12", "12", "False" },
            new object[] { "12", "12.0", "True" }, // Same value but different types. Note: this is truthy in C# AND Java.
            new object[] { "12.0", "12", "True" }, // Same value but different types. Note: this is truthy in C# AND Java.
            new object[] { "12.345", "12.345", "False" },
            new object[] { "12.345", "67.890", "True" },

            new object[] { "2", "4294967295", "True" },
            new object[] { "2", "9223372036854775807", "True" },
            new object[] { "2", "18446744073709551615", "True" },
            new object[] { "2", "18446744073709551616", "True" },
            new object[] { "4294967295", "33", "True" },
            new object[] { "4294967295", "4294967295", "False" },
            new object[] { "4294967295", "9223372036854775807", "True" },
            new object[] { "4294967295", "18446744073709551615", "True" },
            new object[] { "4294967295", "18446744073709551616", "True" },
            new object[] { "4294967295", "12.0", "True" },
            new object[] { "9223372036854775807", "2", "True" },
            new object[] { "9223372036854775807", "4294967295", "True" },
            new object[] { "9223372036854775807", "9223372036854775807", "False" },
            new object[] { "9223372036854775807", "18446744073709551615", "True" },
            new object[] { "9223372036854775807", "18446744073709551616", "True" },
            new object[] { "9223372036854775807", "12.0", "True" },
            new object[] { "18446744073709551615", "2", "True" },
            new object[] { "18446744073709551615", "4294967295", "True" },
            new object[] { "18446744073709551615", "9223372036854775807", "True" },
            new object[] { "18446744073709551615", "18446744073709551615", "False" },
            new object[] { "18446744073709551615", "18446744073709551616", "True" },
            new object[] { "18446744073709551615", "12.0", "True" },
            new object[] { "18446744073709551616", "2", "True" },
            new object[] { "18446744073709551616", "4294967295", "True" },
            new object[] { "18446744073709551616", "9223372036854775807", "True" },
            new object[] { "18446744073709551616", "18446744073709551615", "True" },
            new object[] { "18446744073709551616", "18446744073709551616", "False" },
            new object[] { "18446744073709551616", "12.0", "True" },
            new object[] { "-12.0", "4294967295", "True" },
            new object[] { "-12.0", "9223372036854775807", "True" },
            new object[] { "-12.0", "18446744073709551615", "True" },
            new object[] { "-12.0", "18446744073709551616", "True" },
        };

    public static IEnumerable<object[]> Equal =>
        new List<object[]>
        {
            new object[] { "12", "34", "False" },
            new object[] { "12", "12", "True" },
            new object[] { "12", "12.0", "False" }, // Same value but different types. Note: this is truthy in C# AND Java.
            new object[] { "12.0", "12", "False" }, // Same value but different types. Note: this is truthy in C# AND Java.
            new object[] { "12.345", "12.345", "True" },
            new object[] { "12.345", "67.890", "False" },

            new object[] { "2", "4294967295", "False" },
            new object[] { "2", "9223372036854775807", "False" },
            new object[] { "2", "18446744073709551615", "False" },
            new object[] { "2", "18446744073709551616", "False" },
            new object[] { "4294967295", "33", "False" },
            new object[] { "4294967295", "4294967295", "True" },
            new object[] { "4294967295", "9223372036854775807", "False" },
            new object[] { "4294967295", "18446744073709551615", "False" },
            new object[] { "4294967295", "18446744073709551616", "False" },
            new object[] { "4294967295", "12.0", "False" },
            new object[] { "9223372036854775807", "2", "False" },
            new object[] { "9223372036854775807", "4294967295", "False" },
            new object[] { "9223372036854775807", "9223372036854775807", "True" },
            new object[] { "9223372036854775807", "18446744073709551615", "False" },
            new object[] { "9223372036854775807", "18446744073709551616", "False" },
            new object[] { "9223372036854775807", "12.0", "False" },
            new object[] { "18446744073709551615", "2", "False" },
            new object[] { "18446744073709551615", "4294967295", "False" },
            new object[] { "18446744073709551615", "9223372036854775807", "False" },
            new object[] { "18446744073709551615", "18446744073709551615", "True" },
            new object[] { "18446744073709551615", "18446744073709551616", "False" },
            new object[] { "18446744073709551615", "12.0", "False" },
            new object[] { "18446744073709551616", "2", "False" },
            new object[] { "18446744073709551616", "4294967295", "False" },
            new object[] { "18446744073709551616", "9223372036854775807", "False" },
            new object[] { "18446744073709551616", "18446744073709551615", "False" },
            new object[] { "18446744073709551616", "18446744073709551616", "True" },
            new object[] { "18446744073709551616", "12.0", "False" },
            new object[] { "-12.0", "4294967295", "False" },
            new object[] { "-12.0", "9223372036854775807", "False" },
            new object[] { "-12.0", "18446744073709551615", "False" },
            new object[] { "-12.0", "18446744073709551616", "False" },
        };

    public static IEnumerable<object[]> Subtraction_result =>
        new List<object[]>
        {
            new object[] { "12", "34", "-22" },
            new object[] { "12", "-34", "46" },
            new object[] { "-12", "34", "-46" },
            new object[] { "-12", "-34", "22" },
            new object[] { "2", "18446744073709551616", "-18446744073709551614" },
            new object[] { "-12", "-34.0", "22" },
            new object[] { "4294967296", "9223372036854775807", "-9223372032559808511" },
            new object[] { "9223372036854775807", "9223372036854775807", "0" },
            new object[] { "9223372036854775807", "18446744073709551616", "-9223372036854775809" },
            new object[] { "9223372036854775807", "12.0", "9.223372036854776E+18" }, // TODO: We should make this unsupported. As can be seen in the exponential expression, the operation loses precision.
            new object[] { "18446744073709551616", "2", "18446744073709551614" },
            new object[] { "18446744073709551616", "4294967296", "18446744069414584320" },
            new object[] { "18446744073709551616", "9223372036854775807", "9223372036854775809" },
            new object[] { "18446744073709551616", "18446744073709551617", "-1" },
            new object[] { "12.0", "34.0", "-22" }, // Doubles with fraction part zero => fraction part excluded in string representation.
        };

    public static IEnumerable<object[]> Subtraction_type =>
        new List<object[]>
        {
            new object[] { "12", "34", "System.Int32" },
            new object[] { "12", "-34", "System.Int32" },
            new object[] { "-12", "34", "System.Int32" },
            new object[] { "-12", "-34", "System.Int32" },
            new object[] { "-12", "-34.0", "System.Double" },
            new object[] { "4294967296", "9223372036854775807", "System.Int64" },
            new object[] { "12.0", "34.0", "System.Double" },
            new object[] { "9223372036854775807", "9223372036854775807", "System.Int64" },
            new object[] { "9223372036854775807", "12.0", "System.Double" },
            new object[] { "18446744073709551616", "2", "System.Numerics.BigInteger" },
            new object[] { "18446744073709551616", "4294967296", "System.Numerics.BigInteger" },
            new object[] { "18446744073709551616", "9223372036854775807", "System.Numerics.BigInteger" },
            new object[] { "18446744073709551616", "18446744073709551617", "System.Numerics.BigInteger" }
        };

    public static IEnumerable<object[]> Subtraction_unsupported_types =>
        new List<object[]>
        {
            new object[] { "2", "4294967295", "Operands must be numbers, not int and System.UInt32" },
            new object[] { "2", "9223372036854775807", "Operands must be numbers, not int and long" },
            new object[] { "2", "18446744073709551615", "Operands must be numbers, not int and System.UInt64" },
            new object[] { "4294967295", "33", "Operands must be numbers, not System.UInt32 and int" },
            new object[] { "4294967295", "4294967295", "Operands must be numbers, not System.UInt32 and System.UInt32" },
            new object[] { "4294967295", "9223372036854775807", "Operands must be numbers, not System.UInt32 and long" },
            new object[] { "4294967295", "18446744073709551615", "Operands must be numbers, not System.UInt32 and System.UInt64" },
            new object[] { "4294967295", "18446744073709551616", "Operands must be numbers, not System.UInt32 and bigint" },
            new object[] { "4294967295", "12.0", "Operands must be numbers, not System.UInt32 and double" },
            new object[] { "9223372036854775807", "2", "Operands must be numbers, not long and int" },
            new object[] { "9223372036854775807", "4294967295", "Operands must be numbers, not long and System.UInt32" },
            new object[] { "9223372036854775807", "18446744073709551615", "Operands must be numbers, not long and System.UInt64" },
            new object[] { "18446744073709551615", "2", "Operands must be numbers, not System.UInt64 and int" },
            new object[] { "18446744073709551615", "4294967295", "Operands must be numbers, not System.UInt64 and System.UInt32" },
            new object[] { "18446744073709551615", "9223372036854775807", "Operands must be numbers, not System.UInt64 and long" },
            new object[] { "18446744073709551615", "18446744073709551615", "Operands must be numbers, not System.UInt64 and System.UInt64" },
            new object[] { "18446744073709551615", "18446744073709551616", "Operands must be numbers, not System.UInt64 and bigint" },
            new object[] { "18446744073709551615", "12.0", "Operands must be numbers, not System.UInt64 and double" },
            new object[] { "18446744073709551616", "4294967295", "Operands must be numbers, not bigint and System.UInt32" },
            new object[] { "18446744073709551616", "18446744073709551615", "Operands must be numbers, not bigint and System.UInt64" },
            new object[] { "18446744073709551616", "12.0", "Operands must be numbers, not bigint and double" },
            new object[] { "-12.0", "-34", "Operands must be numbers, not double and int" },
            new object[] { "-12.0", "4294967295", "Operands must be numbers, not double and System.UInt32" },
            new object[] { "-12.0", "9223372036854775807", "Operands must be numbers, not double and long" },
            new object[] { "-12.0", "18446744073709551615", "Operands must be numbers, not double and System.UInt64" },
            new object[] { "-12.0", "18446744073709551616", "Operands must be numbers, not double and bigint" },
        };

    public static IEnumerable<object[]> SubtractionAssignment_result =>
        new List<object[]>
        {
            new object[] { "12", "34", "-22" },
            new object[] { "12", "-34", "46" },
            new object[] { "-12", "34", "-46" },
            new object[] { "-12", "-34", "22" },
            new object[] { "4294967296", "9223372036854775807", "-9223372032559808511" },
            new object[] { "9223372036854775807", "9223372036854775807", "0" },
            new object[] { "18446744073709551616", "2", "18446744073709551614" },
            new object[] { "18446744073709551616", "4294967296", "18446744069414584320" },
            new object[] { "18446744073709551616", "9223372036854775807", "9223372036854775809" },
            new object[] { "18446744073709551616", "18446744073709551617", "-1" },
            new object[] { "12.0", "34.0", "-22" }, // Doubles with fraction part zero => fraction part excluded in string representation.
        };

    public static IEnumerable<object[]> SubtractionAssignment_type =>
        new List<object[]>
        {
            new object[] { "12", "34", "System.Int32" },
            new object[] { "12", "-34", "System.Int32" },
            new object[] { "-12", "34", "System.Int32" },
            new object[] { "-12", "-34", "System.Int32" },
            new object[] { "4294967296", "9223372036854775807", "System.Int64" },
            new object[] { "12.0", "34.0", "System.Double" },
            new object[] { "9223372036854775807", "9223372036854775807", "System.Int64" },
            new object[] { "18446744073709551616", "2", "System.Numerics.BigInteger" },
            new object[] { "18446744073709551616", "4294967296", "System.Numerics.BigInteger" },
            new object[] { "18446744073709551616", "9223372036854775807", "System.Numerics.BigInteger" },
            new object[] { "18446744073709551616", "18446744073709551617", "System.Numerics.BigInteger" }
        };

    public static IEnumerable<object[]> SubtractionAssignment_unsupported_types_runtime =>
        new List<object[]>
        {
            new object[] { "4294967295", "33", "Operands must be numbers, not System.UInt32 and int" },
            new object[] { "4294967295", "4294967295", "Operands must be numbers, not System.UInt32 and System.UInt32" },
            new object[] { "9223372036854775807", "2", "Operands must be numbers, not long and int" },
            new object[] { "9223372036854775807", "4294967295", "Operands must be numbers, not long and System.UInt32" },
            new object[] { "18446744073709551615", "2", "Operands must be numbers, not System.UInt64 and int" },
            new object[] { "18446744073709551615", "4294967295", "Operands must be numbers, not System.UInt64 and System.UInt32" },
            new object[] { "18446744073709551615", "9223372036854775807", "Operands must be numbers, not System.UInt64 and long" },
            new object[] { "18446744073709551615", "18446744073709551615", "Operands must be numbers, not System.UInt64 and System.UInt64" },
            new object[] { "18446744073709551616", "4294967295", "Operands must be numbers, not bigint and System.UInt32" },
            new object[] { "18446744073709551616", "18446744073709551615", "Operands must be numbers, not bigint and System.UInt64" },
            new object[] { "-12.0", "-34", "Operands must be numbers, not double and int" },
            new object[] { "-12.0", "4294967295", "Operands must be numbers, not double and System.UInt32" },
            new object[] { "-12.0", "9223372036854775807", "Operands must be numbers, not double and long" },
            new object[] { "-12.0", "18446744073709551615", "Operands must be numbers, not double and System.UInt64" },
            new object[] { "-12.0", "18446744073709551616", "Operands must be numbers, not double and bigint" },
        };

    public static IEnumerable<object[]> SubtractionAssignment_unsupported_types_validation =>
        new List<object[]>
        {
            new object[] { "2", "4294967295", "Cannot assign System.UInt32 to int variable" },
            new object[] { "2", "9223372036854775807", "Cannot assign long to int variable" },
            new object[] { "2", "18446744073709551615", "Cannot assign System.UInt64 to int variable" },
            new object[] { "2", "18446744073709551616", "Cannot assign bigint to int variable" },
            new object[] { "-12", "-34.0", "Cannot assign double to int variable" },
            new object[] { "4294967295", "12.0", "Cannot assign double to System.UInt32 variable" },
            new object[] { "4294967295", "9223372036854775807", "Cannot assign long to System.UInt32 variable" },
            new object[] { "4294967295", "18446744073709551615", "Cannot assign System.UInt64 to System.UInt32 variable" },
            new object[] { "4294967295", "18446744073709551616", "Cannot assign bigint to System.UInt32 variable" },
            new object[] { "9223372036854775807", "18446744073709551615", "Cannot assign System.UInt64 to long variable" },
            new object[] { "9223372036854775807", "18446744073709551616", "Cannot assign bigint to long variable" },
            new object[] { "9223372036854775807", "12.0", "Cannot assign double to long variable" },
            new object[] { "18446744073709551615", "12.0", "Cannot assign double to System.UInt64 variable" },
            new object[] { "18446744073709551615", "18446744073709551616", "Cannot assign bigint to System.UInt64 variable" },
            new object[] { "18446744073709551616", "12.0", "Cannot assign double to bigint variable" },
        };

    public static IEnumerable<object[]> Addition_result =>
        new List<object[]>
        {
            new object[] { "12", "34", "46" },
            new object[] { "12", "-34", "-22" },
            new object[] { "-12", "34", "22" },
            new object[] { "-12", "-34", "-46" },
            new object[] { "-12", "-34.0", "-46" },
            new object[] { "2", "18446744073709551616", "18446744073709551618" },
            new object[] { "4294967296", "9223372036854775807", "-9223372032559808513" }, // Probably becomes negative because of integer overflow.
            new object[] { "4294967296", "18446744073709551616", "18446744078004518912" },
            new object[] { "9223372036854775807", "9223372036854775807", "-2" },
            new object[] { "9223372036854775807", "12.0", "9.223372036854776E+18" }, // TODO: We should make this unsupported. As can be seen in the exponential expression, the operation loses precision.
            new object[] { "18446744073709551616", "2", "18446744073709551618" },
            new object[] { "18446744073709551616", "4294967296", "18446744078004518912" },
            new object[] { "18446744073709551616", "9223372036854775807", "27670116110564327423" },
            new object[] { "18446744073709551616", "18446744073709551617", "36893488147419103233" },
            new object[] { "12.0", "34.0", "46" }, // Doubles with fraction part zero => fraction part excluded in string representation.
            new object[] { "12.1", "34.2", "46.300000000000004" }, // IEEE-754... :-)
        };

    public static IEnumerable<object[]> Addition_type =>
        new List<object[]>
        {
            new object[] { "12", "34", "System.Int32" },
            new object[] { "12", "-34", "System.Int32" },
            new object[] { "-12", "34", "System.Int32" },
            new object[] { "-12", "-34", "System.Int32" },
            new object[] { "-12", "-34.0", "System.Double" },
            new object[] { "4294967296", "9223372036854775807", "System.Int64" }, // Probably becomes negative because of integer overflow.
            new object[] { "9223372036854775807", "9223372036854775807", "System.Int64" },
            new object[] { "9223372036854775807", "12.0", "System.Double" },
            new object[] { "18446744073709551616", "2", "System.Numerics.BigInteger" },
            new object[] { "18446744073709551616", "4294967296", "System.Numerics.BigInteger" },
            new object[] { "18446744073709551616", "9223372036854775807", "System.Numerics.BigInteger" },
            new object[] { "18446744073709551616", "18446744073709551617", "System.Numerics.BigInteger" },
            new object[] { "12.0", "34.0", "System.Double" },
            new object[] { "12.1", "34.2", "System.Double" },
        };

    public static IEnumerable<object[]> Addition_unsupported_types =>
        new List<object[]>
        {
            new object[] { "2", "4294967295", "Operands must be numbers, not int and System.UInt32" },
            new object[] { "2", "4294967296", "Operands must be numbers, not int and long" },
            new object[] { "2", "18446744073709551615", "Operands must be numbers, not int and System.UInt64" },
            new object[] { "12.0", "34", "Operands must be numbers, not double and int" },
            new object[] { "12.0", "4294967295", "Operands must be numbers, not double and System.UInt32" },
            new object[] { "12.0", "9223372036854775807", "Operands must be numbers, not double and long" },
            new object[] { "12.0", "18446744073709551615", "Operands must be numbers, not double and System.UInt64" },
            new object[] { "12.0", "18446744073709551616", "Operands must be numbers, not double and bigint" },
            new object[] { "4294967295", "2", "Operands must be numbers, not System.UInt32 and int" },
            new object[] { "4294967295", "4294967295", "Operands must be numbers, not System.UInt32 and System.UInt32" },
            new object[] { "4294967295", "9223372036854775807", "Operands must be numbers, not System.UInt32 and long" },
            new object[] { "4294967295", "18446744073709551615", "Operands must be numbers, not System.UInt32 and System.UInt64" },
            new object[] { "4294967295", "18446744073709551616", "Operands must be numbers, not System.UInt32 and bigint" },
            new object[] { "4294967295", "12.0", "Operands must be numbers, not System.UInt32 and double" },
            new object[] { "4294967296", "4294967295", "Operands must be numbers, not long and System.UInt32" },
            new object[] { "4294967296", "18446744073709551615", "Operands must be numbers, not long and System.UInt64" },
            new object[] { "9223372036854775807", "2", "Operands must be numbers, not long and int" },
            new object[] { "18446744073709551615", "2", "Operands must be numbers, not System.UInt64 and int" },
            new object[] { "18446744073709551615", "4294967295", "Operands must be numbers, not System.UInt64 and System.UInt32" },
            new object[] { "18446744073709551615", "9223372036854775807", "Operands must be numbers, not System.UInt64 and long" },
            new object[] { "18446744073709551615", "18446744073709551615", "Operands must be numbers, not System.UInt64 and System.UInt64" },
            new object[] { "18446744073709551615", "18446744073709551616", "Operands must be numbers, not System.UInt64 and bigint" },
            new object[] { "18446744073709551615", "12.0", "Operands must be numbers, not System.UInt64 and double" },
            new object[] { "18446744073709551616", "4294967295", "Operands must be numbers, not bigint and System.UInt32" },
            new object[] { "18446744073709551616", "18446744073709551615", "Operands must be numbers, not bigint and System.UInt64" },
            new object[] { "18446744073709551616", "12.0", "Operands must be numbers, not bigint and double" },
        };

    public static IEnumerable<object[]> AdditionAssignment_result =>
        new List<object[]>
        {
            new object[] { "12", "34", "46" },
            new object[] { "12", "-34", "-22" },
            new object[] { "-12", "34", "22" },
            new object[] { "-12", "-34", "-46" },
            new object[] { "4294967296", "9223372036854775807", "-9223372032559808513" }, // Probably becomes negative because of integer overflow.
            new object[] { "9223372036854775807", "9223372036854775807", "-2" },
            new object[] { "18446744073709551616", "2", "18446744073709551618" },
            new object[] { "18446744073709551616", "4294967296", "18446744078004518912" },
            new object[] { "18446744073709551616", "9223372036854775807", "27670116110564327423" },
            new object[] { "18446744073709551616", "18446744073709551617", "36893488147419103233" },
            new object[] { "12.0", "34.0", "46" }, // Doubles with fraction part zero => fraction part excluded in string representation.
            new object[] { "12.1", "34.2", "46.300000000000004" }, // IEEE-754... :-)
        };

    public static IEnumerable<object[]> AdditionAssignment_type =>
        new List<object[]>
        {
            new object[] { "12", "34", "System.Int32" },
            new object[] { "12", "-34", "System.Int32" },
            new object[] { "-12", "34", "System.Int32" },
            new object[] { "-12", "-34", "System.Int32" },
            new object[] { "4294967296", "9223372036854775807", "System.Int64" }, // Probably becomes negative because of integer overflow.
            new object[] { "9223372036854775807", "9223372036854775807", "System.Int64" },
            new object[] { "18446744073709551616", "2", "System.Numerics.BigInteger" },
            new object[] { "18446744073709551616", "4294967296", "System.Numerics.BigInteger" },
            new object[] { "18446744073709551616", "9223372036854775807", "System.Numerics.BigInteger" },
            new object[] { "18446744073709551616", "18446744073709551617", "System.Numerics.BigInteger" },
            new object[] { "12.0", "34.0", "System.Double" },
            new object[] { "12.1", "34.2", "System.Double" },
        };

    public static IEnumerable<object[]> AdditionAssignment_unsupported_types_runtime =>
        new List<object[]>
        {
            new object[] { "4294967295", "33", "Operands must be numbers, not System.UInt32 and int" },
            new object[] { "4294967295", "4294967295", "Operands must be numbers, not System.UInt32 and System.UInt32" },
            new object[] { "9223372036854775807", "2", "Operands must be numbers, not long and int" },
            new object[] { "9223372036854775807", "4294967295", "Operands must be numbers, not long and System.UInt32" },
            new object[] { "18446744073709551615", "2", "Operands must be numbers, not System.UInt64 and int" },
            new object[] { "18446744073709551615", "4294967295", "Operands must be numbers, not System.UInt64 and System.UInt32" },
            new object[] { "18446744073709551615", "9223372036854775807", "Operands must be numbers, not System.UInt64 and long" },
            new object[] { "18446744073709551615", "18446744073709551615", "Operands must be numbers, not System.UInt64 and System.UInt64" },
            new object[] { "18446744073709551616", "4294967295", "Operands must be numbers, not bigint and System.UInt32" },
            new object[] { "18446744073709551616", "18446744073709551615", "Operands must be numbers, not bigint and System.UInt64" },
            new object[] { "-12.0", "-34", "Operands must be numbers, not double and int" },
            new object[] { "-12.0", "4294967295", "Operands must be numbers, not double and System.UInt32" },
            new object[] { "-12.0", "9223372036854775807", "Operands must be numbers, not double and long" },
            new object[] { "-12.0", "18446744073709551615", "Operands must be numbers, not double and System.UInt64" },
            new object[] { "-12.0", "18446744073709551616", "Operands must be numbers, not double and bigint" },
        };

    public static IEnumerable<object[]> AdditionAssignment_unsupported_types_validation =>
        new List<object[]>
        {
            new object[] { "2", "4294967295", "Cannot assign System.UInt32 to int variable" },
            new object[] { "2", "9223372036854775807", "Cannot assign long to int variable" },
            new object[] { "2", "18446744073709551615", "Cannot assign System.UInt64 to int variable" },
            new object[] { "2", "18446744073709551616", "Cannot assign bigint to int variable" },
            new object[] { "-12", "-34.0", "Cannot assign double to int variable" },
            new object[] { "4294967295", "12.0", "Cannot assign double to System.UInt32 variable" },
            new object[] { "4294967295", "9223372036854775807", "Cannot assign long to System.UInt32 variable" },
            new object[] { "4294967295", "18446744073709551615", "Cannot assign System.UInt64 to System.UInt32 variable" },
            new object[] { "4294967295", "18446744073709551616", "Cannot assign bigint to System.UInt32 variable" },
            new object[] { "9223372036854775807", "18446744073709551615", "Cannot assign System.UInt64 to long variable" },
            new object[] { "9223372036854775807", "18446744073709551616", "Cannot assign bigint to long variable" },
            new object[] { "9223372036854775807", "12.0", "Cannot assign double to long variable" },
            new object[] { "18446744073709551615", "12.0", "Cannot assign double to System.UInt64 variable" },
            new object[] { "18446744073709551615", "18446744073709551616", "Cannot assign bigint to System.UInt64 variable" },
            new object[] { "18446744073709551616", "12.0", "Cannot assign double to bigint variable" },
        };

    public static IEnumerable<object[]> Division_result =>
        new List<object[]>
        {
            new object[] { "35", "5", "7" },
            new object[] { "34", "5", "6" }, // `int` division => expecting to be truncated.
            new object[] { "2", "18446744073709551616", "0" },
            new object[] { "9223372036854775807", "9223372036854775807", "1" },
            new object[] { "9223372036854775807", "18446744073709551616", "0" },
            new object[] { "9223372036854775807", "12.0", "7.686143364045646E+17" }, // TODO: We should make this unsupported
            new object[] { "18446744073709551616", "2", "9223372036854775808" },
            new object[] { "18446744073709551616", "9223372036854775807", "2" },
            new object[] { "18446744073709551616", "18446744073709551616", "1" },
            new object[] { "34.0", "5.0", "6.8" },
            new object[] { "34", "5.0", "6.8" }
        };

    public static IEnumerable<object[]> Division_type =>
        new List<object[]>
        {
            new object[] { "35", "5", "System.Int32" },
            new object[] { "34", "5", "System.Int32" },
            new object[] { "34.0", "5.0", "System.Double" },
            new object[] { "34", "5.0", "System.Double" }
        };

    public static IEnumerable<object[]> Division_unsupported_types =>
        new List<object[]>
        {
            new object[] { "2", "4294967295", "Operands must be numbers, not int and System.UInt32" },
            new object[] { "2", "9223372036854775807", "Operands must be numbers, not int and long" },
            new object[] { "2", "18446744073709551615", "Operands must be numbers, not int and System.UInt64" },
            new object[] { "4294967295", "2", "Operands must be numbers, not System.UInt32 and int" },
            new object[] { "4294967295", "4294967295", "Operands must be numbers, not System.UInt32 and System.UInt32" },
            new object[] { "4294967295", "9223372036854775807", "Operands must be numbers, not System.UInt32 and long" },
            new object[] { "4294967295", "18446744073709551615", "Operands must be numbers, not System.UInt32 and System.UInt64" },
            new object[] { "4294967295", "18446744073709551616", "Operands must be numbers, not System.UInt32 and bigint" },
            new object[] { "4294967295", "12.0", "Operands must be numbers, not System.UInt32 and double" },
            new object[] { "9223372036854775807", "2", "Operands must be numbers, not long and int" },
            new object[] { "9223372036854775807", "4294967295", "Operands must be numbers, not long and System.UInt32" },
            new object[] { "9223372036854775807", "18446744073709551615", "Operands must be numbers, not long and System.UInt64" },
            new object[] { "18446744073709551615", "2", "Operands must be numbers, not System.UInt64 and int" },
            new object[] { "18446744073709551615", "4294967295", "Operands must be numbers, not System.UInt64 and System.UInt32" },
            new object[] { "18446744073709551615", "9223372036854775807", "Operands must be numbers, not System.UInt64 and long" },
            new object[] { "18446744073709551615", "18446744073709551615", "Operands must be numbers, not System.UInt64 and System.UInt64" },
            new object[] { "18446744073709551615", "18446744073709551616", "Operands must be numbers, not System.UInt64 and bigint" },
            new object[] { "18446744073709551615", "12.0", "Operands must be numbers, not System.UInt64 and double" },
            new object[] { "18446744073709551616", "18446744073709551615", "Operands must be numbers, not bigint and System.UInt64" },
            new object[] { "18446744073709551616", "4294967295", "Operands must be numbers, not bigint and System.UInt32" },
            new object[] { "18446744073709551616", "12.0", "Operands must be numbers, not bigint and double" },
            new object[] { "12.0", "5", "Operands must be numbers, not double and int" },
            new object[] { "12.0", "4294967295", "Operands must be numbers, not double and System.UInt32" },
            new object[] { "12.0", "9223372036854775807", "Operands must be numbers, not double and long" },
            new object[] { "12.0", "18446744073709551615", "Operands must be numbers, not double and System.UInt64" },
            new object[] { "12.0", "18446744073709551616", "Operands must be numbers, not double and bigint" },
        };

    public static IEnumerable<object[]> Multiplication_result =>
        new List<object[]>
        {
            new object[] { "5", "3", "15" },
            new object[] { "2", "18446744073709551616", "36893488147419103232" },
            new object[] { "12", "34.0", "408" },
            new object[] { "1073741824", "2", "-2147483648" }, // Becomes negative because of signed `int` overflow.
            new object[] { "9223372036854775807", "9223372036854775807", "1" },
            new object[] { "9223372036854775807", "18446744073709551616", "170141183460469231713240559642174554112" },
            new object[] { "9223372036854775807", "12.0", "1.1068046444225731E+20" }, // TODO: We should make this unsupported, since it's likely to lose precision
            new object[] { "18446744073709551616", "2", "36893488147419103232" },
            new object[] { "18446744073709551616", "9223372036854775807", "170141183460469231713240559642174554112" },
            new object[] { "18446744073709551616", "18446744073709551616", "340282366920938463463374607431768211456" },
            new object[] { "12.34", "0.3", "3.702" }
        };

    public static IEnumerable<object[]> Multiplication_type =>
        new List<object[]>
        {
            new object[] { "5", "3", "System.Int32" },
            new object[] { "12", "34.0", "System.Double" },
            new object[] { "12.34", "0.3", "System.Double" },
            new object[] { "1073741824", "2", "System.Int32" }
        };

    public static IEnumerable<object[]> Multiplication_unsupported_types =>
        new List<object[]>
        {
            new object[] { "2", "4294967295", "Operands must be numbers, not int and System.UInt32" },
            new object[] { "2", "9223372036854775807", "Operands must be numbers, not int and long" },
            new object[] { "2", "18446744073709551615", "Operands must be numbers, not int and System.UInt64" },
            new object[] { "4294967295", "2", "Operands must be numbers, not System.UInt32 and int" },
            new object[] { "4294967295", "4294967295", "Operands must be numbers, not System.UInt32 and System.UInt32" },
            new object[] { "4294967295", "9223372036854775807", "Operands must be numbers, not System.UInt32 and long" },
            new object[] { "4294967295", "18446744073709551615", "Operands must be numbers, not System.UInt32 and System.UInt64" },
            new object[] { "4294967295", "18446744073709551616", "Operands must be numbers, not System.UInt32 and bigint" },
            new object[] { "4294967295", "12.0", "Operands must be numbers, not System.UInt32 and double" },
            new object[] { "9223372036854775807", "2", "Operands must be numbers, not long and int" },
            new object[] { "9223372036854775807", "4294967295", "Operands must be numbers, not long and System.UInt32" },
            new object[] { "9223372036854775807", "18446744073709551615", "Operands must be numbers, not long and System.UInt64" },
            new object[] { "18446744073709551615", "2", "Operands must be numbers, not System.UInt64 and int" },
            new object[] { "18446744073709551615", "4294967295", "Operands must be numbers, not System.UInt64 and System.UInt32" },
            new object[] { "18446744073709551615", "9223372036854775807", "Operands must be numbers, not System.UInt64 and long" },
            new object[] { "18446744073709551615", "18446744073709551615", "Operands must be numbers, not System.UInt64 and System.UInt64" },
            new object[] { "18446744073709551615", "18446744073709551616", "Operands must be numbers, not System.UInt64 and bigint" },
            new object[] { "18446744073709551615", "12.0", "Operands must be numbers, not System.UInt64 and double" },
            new object[] { "18446744073709551616", "18446744073709551615", "Operands must be numbers, not bigint and System.UInt64" },
            new object[] { "18446744073709551616", "4294967295", "Operands must be numbers, not bigint and System.UInt32" },
            new object[] { "18446744073709551616", "12.0", "Operands must be numbers, not bigint and double" },
            new object[] { "12.0", "5", "Operands must be numbers, not double and int" },
            new object[] { "12.0", "4294967295", "Operands must be numbers, not double and System.UInt32" },
            new object[] { "12.0", "9223372036854775807", "Operands must be numbers, not double and long" },
            new object[] { "12.0", "18446744073709551615", "Operands must be numbers, not double and System.UInt64" },
            new object[] { "12.0", "18446744073709551616", "Operands must be numbers, not double and bigint" },
        };

    public static IEnumerable<object[]> Exponential_result =>
        new List<object[]>
        {
            new object[] { "2", "10", "1024" },
            new object[] { "2.0", "10.0", "1024" },
            new object[] { "2", "9.9", "955.425783333691" },
            new object[] { "4294967296", "2", "18446744073709551616" },
            new object[] { "4294967296", "10.0", "2.13598703592091E+96" },
            new object[] { "18446744073709551616", "2", "340282366920938463463374607431768211456" }
        };

    public static IEnumerable<object[]> Exponential_type =>
        new List<object[]>
        {
            new object[] { "2", "10", "System.Numerics.BigInteger" },
            new object[] { "2.0", "10.0", "System.Double" },
            new object[] { "2", "9.9", "System.Double" },
            new object[] { "4294967296", "2", "System.Numerics.BigInteger" },
            new object[] { "4294967296", "10.0", "System.Double" },
            new object[] { "18446744073709551616", "2", "System.Numerics.BigInteger" }
        };

    public static IEnumerable<object[]> Exponential_unsupported_types =>
        new List<object[]>
        {
            new object[] { "2.1", "10", "Unsupported ** operands specified: double and int" },
            new object[] { "4294967296", "4294967296", "Unsupported ** operands specified: long and long" },
            new object[] { "10.0", "4294967296", "Unsupported ** operands specified: double and long" },
            new object[] { "18446744073709551616", "18446744073709551616", "Unsupported ** operands specified: bigint and bigint" }
        };

    public static IEnumerable<object[]> Modulo_result =>
        new List<object[]>
        {
            new object[] { "5", "3", "2" },
            new object[] { "9", "2.0", "1" },
            new object[] { "9.0", "2.0", "1" },
            new object[] { "2147483647", "2", "1" },
            new object[] { "-2147483648", "2", "0" },
            new object[] { "18446744073709551616", "3", "1" }
        };

    public static IEnumerable<object[]> Percent_type =>
        new List<object[]>
        {
            new object[] { "5", "3", "System.Int32" },
            new object[] { "9", "2.0", "System.Double" },
            new object[] { "9.0", "2.0", "System.Double" },
            new object[] { "2147483647", "2", "System.Int32" },
            new object[] { "-2147483648", "2", "System.Int32" },
            new object[] { "18446744073709551616", "3", "System.Numerics.BigInteger" }
        };

    public static IEnumerable<object[]> Modulo_unsupported_types =>
        new List<object[]>
        {
            new object[] { "9.0", "2", "Operands must be numbers, not double and int" },
            new object[] { "4294967295", "2", "Operands must be numbers, not System.UInt32 and int" },
            new object[] { "9223372036854775807", "2", "Operands must be numbers, not long and int" },
            new object[] { "18446744073709551615", "2", "Operands must be numbers, not System.UInt64 and int" },
        };

    public static IEnumerable<object[]> ShiftLeft_result =>
        new List<object[]>
        {
            new object[] { "1", "10", "1024" },
            new object[] { "1073741824", "1", "-2147483648" }, // Integer overflow => becomes negative
            new object[] { "4611686018427387904", "1", "-9223372036854775808" }, // Integer overflow => becomes negative
        };

    public static IEnumerable<object[]> LessLess_type =>
        new List<object[]>
        {
            new object[] { "1", "10", "System.Int32" },
            new object[] { "1073741824", "1", "System.Int32" },
            new object[] { "4294967296", "1", "System.Int64" },
            new object[] { "18446744073709551616", "1", "System.Numerics.BigInteger" }
        };

    public static IEnumerable<object[]> ShiftLeft_unsupported_types =>
        new List<object[]>
        {
            new object[] { "2147483648", "1", "Operands must be numbers, not System.UInt32 and int" },
            new object[] { "9223372036854775808", "1", "Operands must be numbers, not System.UInt64 and int" }
        };

    public static IEnumerable<object[]> ShiftRight_result =>
        new List<object[]>
        {
            new object[] { "65536", "6", "1024" },
            new object[] { "1073741824", "1", "536870912" },
            new object[] { "4611686018427387904", "1", "2305843009213693952" },
        };

    public static IEnumerable<object[]> GreaterGreater_type =>
        new List<object[]>
        {
            new object[] { "65536", "6", "System.Int32" },
            new object[] { "1073741824", "1", "System.Int32" },
            new object[] { "4611686018427387904", "1", "System.Int64" },
        };

    public static IEnumerable<object[]> ShiftRight_unsupported_types =>
        new List<object[]>
        {
            new object[] { "2147483648", "1", "Operands must be numbers, not System.UInt32 and int" },
            new object[] { "9223372036854775808", "1", "Operands must be numbers, not System.UInt64 and int" }
        };
}
