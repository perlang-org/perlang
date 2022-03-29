#pragma warning disable SA1515
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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
            new object[] { "-12", "34.0", "False" },
            new object[] { "2147483647", "33.0", "True" },
            new object[] { "12.0", "34", "False" },
            new object[] { "12.0", "9223372036854775807", "False" },
            new object[] { "12.0", "34.0", "False" },
            new object[] { "34.0", "33.0", "True" },

            new object[] { "2", "18446744073709551616", "False" },
            new object[] { "2", "9223372036854775807", "False" },
            new object[] { "4294967296", "9223372036854775807", "False" },
            new object[] { "9223372036854775807", "2", "True" },
            new object[] { "9223372036854775807", "9223372036854775807", "False" },
            new object[] { "9223372036854775807", "18446744073709551616", "False" },
            new object[] { "9223372036854775807", "12.0", "True" },
            new object[] { "18446744073709551616", "2", "True" },
            new object[] { "18446744073709551616", "4294967296", "True" },
            new object[] { "18446744073709551616", "9223372036854775807", "True" },
            new object[] { "18446744073709551616", "18446744073709551616", "False" },
        };

    public static IEnumerable<object[]> Greater_unsupported_types =>
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
            new object[] { "-12.0", "4294967295", "Operands must be numbers, not double and System.UInt32" },
            new object[] { "-12.0", "18446744073709551615", "Operands must be numbers, not double and System.UInt64" },
            new object[] { "-12.0", "18446744073709551616", "Operands must be numbers, not double and bigint" },
        };

    public static IEnumerable<object[]> GreaterEqual =>
        new List<object[]>
        {
            new object[] { "2147483646", "2147483647", "False" },
            new object[] { "2147483647", "2147483647", "True" },
            new object[] { "2147483647", "2147483646", "True" },
            new object[] { "-12", "34.0", "False" },
            new object[] { "2147483647", "33.0", "True" },
            new object[] { "12.0", "34", "False" },
            new object[] { "12.0", "9223372036854775807", "False" },
            new object[] { "12.0", "34.0", "False" },
            new object[] { "34.0", "33.0", "True" },

            new object[] { "2", "18446744073709551616", "False" },
            new object[] { "2", "9223372036854775807", "False" },
            new object[] { "4294967296", "9223372036854775807", "False" },
            new object[] { "9223372036854775807", "2", "True" },
            new object[] { "9223372036854775807", "9223372036854775807", "True" },
            new object[] { "9223372036854775807", "18446744073709551616", "False" },
            new object[] { "9223372036854775807", "12.0", "True" },
            new object[] { "18446744073709551616", "2", "True" },
            new object[] { "18446744073709551616", "4294967296", "True" },
            new object[] { "18446744073709551616", "9223372036854775807", "True" },
            new object[] { "18446744073709551616", "18446744073709551616", "True" },
            new object[] { "12.0", "34.0", "False" },
            new object[] { "34.0", "33.0", "True" },
        };

    public static IEnumerable<object[]> GreaterEqual_unsupported_types =>
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
            new object[] { "-12.0", "4294967295", "Operands must be numbers, not double and System.UInt32" },
            new object[] { "-12.0", "18446744073709551615", "Operands must be numbers, not double and System.UInt64" },
            new object[] { "-12.0", "18446744073709551616", "Operands must be numbers, not double and bigint" },
        };

    public static IEnumerable<object[]> Less =>
        new List<object[]>
        {
            new object[] { "12", "34", "True" },
            new object[] { "12", "-34", "False" },
            new object[] { "12", "34.0", "True" },
            new object[] { "-12", "34", "True" },
            new object[] { "-12.0", "-34", "False" },
            new object[] { "-12.0", "9223372036854775807", "True" },
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
            new object[] { "-12.0", "-34", "False" },
            new object[] { "-12.0", "9223372036854775807", "True" },
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
            new object[] { "-12.0", "4294967295", "Operands must be numbers, not double and System.UInt32" },
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
            new object[] { "-12.0", "34.0", "True" },
            new object[] { "-12.0", "-34", "False" },
            new object[] { "-12.0", "9223372036854775807", "True" },
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
            new object[] { "-12.0", "4294967295", "Operands must be numbers, not double and System.UInt32" },
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
        Subtraction_result_and_type.GetResult();

    public static IEnumerable<object[]> Subtraction_type =>
        Subtraction_result_and_type.GetTypes();

    private static IEnumerable<object[]> Subtraction_result_and_type =>
        new List<object[]>
        {
            new object[] { "12", "34", "-22", typeof(int) },
            new object[] { "12", "-34", "46", typeof(int) },
            new object[] { "-12", "34", "-46", typeof(int) },
            new object[] { "-12", "-34", "22", typeof(int) },
            new object[] { "2", "18446744073709551616", "-18446744073709551614", typeof(BigInteger) },
            new object[] { "-12", "-34.0", "22", typeof(double) }, // TODO: Should we really support this? What does other languages do?
            new object[] { "4294967296", "9223372036854775807", "-9223372032559808511", typeof(long) },
            new object[] { "9223372036854775807", "9223372036854775807", "0", typeof(long) },
            new object[] { "9223372036854775807", "18446744073709551616", "-9223372036854775809", typeof(BigInteger) },
            new object[] { "9223372036854775807", "12.0", "9.223372036854776E+18", typeof(double) }, // TODO: We should make this unsupported. As can be seen in the exponential expression, the operation loses precision.
            new object[] { "18446744073709551616", "2", "18446744073709551614", typeof(BigInteger) },
            new object[] { "18446744073709551616", "4294967296", "18446744069414584320", typeof(BigInteger) },
            new object[] { "18446744073709551616", "9223372036854775807", "9223372036854775809", typeof(BigInteger) },
            new object[] { "18446744073709551616", "18446744073709551617", "-1", typeof(BigInteger) },
            new object[] { "-12.0", "-34", "22", typeof(double) },
            new object[] { "-12.0", "9223372036854775807", "-9.223372036854776E+18", typeof(double) }, // TODO: Support this or not?
            new object[] { "12.0", "34.0", "-22", typeof(double) }, // Doubles with fraction part zero => fraction part excluded in string representation.
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
            new object[] { "-12.0", "4294967295", "Operands must be numbers, not double and System.UInt32" },
            new object[] { "-12.0", "18446744073709551615", "Operands must be numbers, not double and System.UInt64" },
            new object[] { "-12.0", "18446744073709551616", "Operands must be numbers, not double and bigint" },
        };

    public static IEnumerable<object[]> SubtractionAssignment_result =>
        SubtractionAssignment_result_and_type.GetResult();

    public static IEnumerable<object[]> SubtractionAssignment_type =>
        SubtractionAssignment_result_and_type.GetTypes();

    private static IEnumerable<object[]> SubtractionAssignment_result_and_type =>
        new List<object[]>
        {
            new object[] { "12", "34", "-22", typeof(int) },
            new object[] { "12", "-34", "46", typeof(int) },
            new object[] { "-12", "34", "-46", typeof(int) },
            new object[] { "-12", "-34", "22", typeof(int) },
            new object[] { "4294967296", "9223372036854775807", "-9223372032559808511", typeof(long) },
            new object[] { "9223372036854775807", "9223372036854775807", "0", typeof(long) },
            new object[] { "18446744073709551616", "2", "18446744073709551614", typeof(BigInteger) },
            new object[] { "18446744073709551616", "4294967296", "18446744069414584320", typeof(BigInteger) },
            new object[] { "18446744073709551616", "9223372036854775807", "9223372036854775809", typeof(BigInteger) },
            new object[] { "18446744073709551616", "18446744073709551617", "-1", typeof(BigInteger) },
            new object[] { "-12.0", "-34", "22", typeof(double) }, // Doubles with fraction part zero => fraction part excluded in string representation.
            new object[] { "-12.0", "9223372036854775807", "-9.223372036854776E+18", typeof(double) }, // TODO: Support this or not? Can cause precision loss
            new object[] { "12.0", "34.0", "-22", typeof(double) },
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
            new object[] { "-12.0", "4294967295", "Operands must be numbers, not double and System.UInt32" },
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
        Addition_result_and_type.GetResult();

    public static IEnumerable<object[]> Addition_type =>
        Addition_result_and_type.GetTypes();

    private static IEnumerable<object[]> Addition_result_and_type =>
        new List<object[]>
        {
            new object[] { "12", "34", "46", typeof(int) },
            new object[] { "12", "-34", "-22", typeof(int) },
            new object[] { "-12", "34", "22", typeof(int) },
            new object[] { "-12", "-34", "-46", typeof(int) },
            new object[] { "-12", "-34.0", "-46", typeof(double) },
            new object[] { "2", "18446744073709551616", "18446744073709551618", typeof(BigInteger) },
            new object[] { "4294967296", "9223372036854775807", "-9223372032559808513", typeof(long) }, // Probably becomes negative because of integer overflow.
            new object[] { "4294967296", "18446744073709551616", "18446744078004518912", typeof(BigInteger) },
            new object[] { "9223372036854775807", "9223372036854775807", "-2", typeof(long) },
            new object[] { "9223372036854775807", "12.0", "9.223372036854776E+18", typeof(double) }, // TODO: We should make this unsupported. As can be seen in the exponential expression, the operation loses precision.
            new object[] { "18446744073709551616", "2", "18446744073709551618", typeof(BigInteger) },
            new object[] { "18446744073709551616", "4294967296", "18446744078004518912", typeof(BigInteger) },
            new object[] { "18446744073709551616", "9223372036854775807", "27670116110564327423", typeof(BigInteger) },
            new object[] { "18446744073709551616", "18446744073709551617", "36893488147419103233", typeof(BigInteger) },
            new object[] { "12.0", "34", "46", typeof(double) },
            new object[] { "12.0", "9223372036854775807", "9.223372036854776E+18", typeof(double) },
            new object[] { "12.0", "34.0", "46", typeof(double) }, // Doubles with fraction part zero => fraction part excluded in string representation.
            new object[] { "12.1", "34.2", "46.300000000000004", typeof(double) }, // IEEE-754... :-)
        };

    public static IEnumerable<object[]> Addition_unsupported_types =>
        new List<object[]>
        {
            new object[] { "2", "4294967295", "Operands must be numbers, not int and System.UInt32" },
            new object[] { "2", "4294967296", "Operands must be numbers, not int and long" },
            new object[] { "2", "18446744073709551615", "Operands must be numbers, not int and System.UInt64" },
            new object[] { "12.0", "4294967295", "Operands must be numbers, not double and System.UInt32" },
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
        AdditionAssignment_result_and_type.GetResult();

    public static IEnumerable<object[]> AdditionAssignment_type =>
        AdditionAssignment_result_and_type.GetTypes();

    private static IEnumerable<object[]> AdditionAssignment_result_and_type =>
        new List<object[]>
        {
            new object[] { "12", "34", "46", typeof(int) },
            new object[] { "12", "-34", "-22", typeof(int) },
            new object[] { "-12", "34", "22", typeof(int) },
            new object[] { "-12", "-34", "-46", typeof(int) },
            new object[] { "4294967296", "9223372036854775807", "-9223372032559808513", typeof(long) }, // Probably becomes negative because of integer overflow.
            new object[] { "9223372036854775807", "9223372036854775807", "-2", typeof(long) },
            new object[] { "18446744073709551616", "2", "18446744073709551618", typeof(BigInteger) },
            new object[] { "18446744073709551616", "4294967296", "18446744078004518912", typeof(BigInteger) },
            new object[] { "18446744073709551616", "9223372036854775807", "27670116110564327423", typeof(BigInteger) },
            new object[] { "18446744073709551616", "18446744073709551617", "36893488147419103233", typeof(BigInteger) },
            new object[] { "-12.0", "-34", "-46", typeof(double) },
            new object[] { "-12.0", "9223372036854775807", "9.223372036854776E+18", typeof(double) },
            new object[] { "12.0", "34.0", "46", typeof(double) }, // Doubles with fraction part zero => fraction part excluded in string representation.
            new object[] { "12.1", "34.2", "46.300000000000004", typeof(double) }, // IEEE-754... :-)
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
            new object[] { "-12.0", "4294967295", "Operands must be numbers, not double and System.UInt32" },
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
        Division_result_and_type.GetResult();

    public static IEnumerable<object[]> Division_type =>
        Division_result_and_type.GetTypes();

    private static IEnumerable<object[]> Division_result_and_type =>
        new List<object[]>
        {
            new object[] { "35", "5", "7", typeof(int) },
            new object[] { "34", "5", "6", typeof(int) }, // `int` division => expecting to be truncated.
            new object[] { "2", "18446744073709551616", "0", typeof(BigInteger) },
            new object[] { "9223372036854775807", "9223372036854775807", "1", typeof(long) },
            new object[] { "9223372036854775807", "18446744073709551616", "0", typeof(BigInteger) },
            new object[] { "9223372036854775807", "12.0", "7.686143364045646E+17", typeof(double) }, // TODO: We should make this unsupported
            new object[] { "18446744073709551616", "2", "9223372036854775808", typeof(BigInteger) },
            new object[] { "18446744073709551616", "9223372036854775807", "2", typeof(BigInteger) },
            new object[] { "18446744073709551616", "18446744073709551616", "1", typeof(BigInteger) },
            new object[] { "34.0", "5", "6.8", typeof(double) },
            new object[] { "34.0", "9223372036854775807", "3.686287386450715E-18", typeof(double) },
            new object[] { "34.0", "5.0", "6.8", typeof(double) },
            new object[] { "34", "5.0", "6.8", typeof(double) }
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
            new object[] { "12.0", "4294967295", "Operands must be numbers, not double and System.UInt32" },
            new object[] { "12.0", "18446744073709551615", "Operands must be numbers, not double and System.UInt64" },
            new object[] { "12.0", "18446744073709551616", "Operands must be numbers, not double and bigint" },
        };

    public static IEnumerable<object[]> Multiplication_result =>
        Multiplication_result_and_type.GetResult();

    public static IEnumerable<object[]> Multiplication_type =>
        Multiplication_result_and_type.GetTypes();

    private static IEnumerable<object[]> Multiplication_result_and_type =>
        new List<object[]>
        {
            new object[] { "5", "3", "15", typeof(int) },
            new object[] { "2", "18446744073709551616", "36893488147419103232", typeof(BigInteger) },
            new object[] { "12", "34.0", "408", typeof(double) },
            new object[] { "1073741824", "2", "-2147483648", typeof(int) }, // Becomes negative because of signed `int` overflow.
            new object[] { "9223372036854775807", "9223372036854775807", "1", typeof(long) },
            new object[] { "9223372036854775807", "18446744073709551616", "170141183460469231713240559642174554112", typeof(BigInteger) },
            new object[] { "9223372036854775807", "12.0", "1.1068046444225731E+20", typeof(double) }, // TODO: We should make this unsupported, since it's likely to lose precision
            new object[] { "18446744073709551616", "2", "36893488147419103232", typeof(BigInteger) },
            new object[] { "18446744073709551616", "9223372036854775807", "170141183460469231713240559642174554112", typeof(BigInteger) },
            new object[] { "18446744073709551616", "18446744073709551616", "340282366920938463463374607431768211456", typeof(BigInteger) },
            new object[] { "12.0", "5", "60", typeof(double) },
            new object[] { "12.0", "9223372036854775807", "1.1068046444225731E+20", typeof(double) }, // TODO: Should we support this or not? Can cause precision loss.
            new object[] { "12.34", "0.3", "3.702", typeof(double) }
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
            new object[] { "12.0", "4294967295", "Operands must be numbers, not double and System.UInt32" },
            new object[] { "12.0", "18446744073709551615", "Operands must be numbers, not double and System.UInt64" },
            new object[] { "12.0", "18446744073709551616", "Operands must be numbers, not double and bigint" },
        };

    public static IEnumerable<object[]> Exponential_result =>
        Exponential_result_and_type.GetResult();

    public static IEnumerable<object[]> Exponential_type =>
        Exponential_result_and_type.GetTypes();

    private static IEnumerable<object[]> Exponential_result_and_type =>
        new List<object[]>
        {
            new object[] { "2", "10", "1024", typeof(BigInteger) },
            new object[] { "2.1", "10", "1667.9880978201006", typeof(double) },
            new object[] { "2.0", "10.0", "1024", typeof(double) },
            new object[] { "2", "9.9", "955.425783333691", typeof(double) },
            new object[] { "4294967296", "2", "18446744073709551616", typeof(BigInteger) },
            new object[] { "18446744073709551616", "2", "340282366920938463463374607431768211456", typeof(BigInteger) }
        };

    public static IEnumerable<object[]> Exponential_unsupported_types =>
        new List<object[]>
        {
            new object[] { "4294967296", "4294967296", "Unsupported ** operands specified: long and long" },
            new object[] { "10.0", "4294967296", "Unsupported ** operands specified: double and long" },
            new object[] { "18446744073709551616", "18446744073709551616", "Unsupported ** operands specified: bigint and bigint" },

            new object[] { "2", "4294967295", "Unsupported ** operands specified: int and System.UInt32" },
            new object[] { "2", "9223372036854775807", "Unsupported ** operands specified: int and long" },
            new object[] { "2", "18446744073709551615", "Unsupported ** operands specified: int and System.UInt64" },
            new object[] { "2", "18446744073709551616", "Unsupported ** operands specified: int and bigint" },
            new object[] { "4294967295", "33", "Unsupported ** operands specified: System.UInt32 and int" },
            new object[] { "4294967295", "4294967295", "Unsupported ** operands specified: System.UInt32 and System.UInt32" },
            new object[] { "4294967295", "9223372036854775807", "Unsupported ** operands specified: System.UInt32 and long" },
            new object[] { "4294967295", "18446744073709551615", "Unsupported ** operands specified: System.UInt32 and System.UInt64" },
            new object[] { "4294967295", "18446744073709551616", "Unsupported ** operands specified: System.UInt32 and bigint" },
            new object[] { "4294967295", "12.0", "Unsupported ** operands specified: System.UInt32 and double" },
            new object[] { "9223372036854775807", "4294967295", "Unsupported ** operands specified: long and System.UInt32" },
            new object[] { "9223372036854775807", "18446744073709551615", "Unsupported ** operands specified: long and System.UInt64" },
            new object[] { "9223372036854775807", "18446744073709551616", "Unsupported ** operands specified: long and bigint" },
            new object[] { "9223372036854775807", "10.0", "Unsupported ** operands specified: long and double" },
            new object[] { "18446744073709551615", "2", "Unsupported ** operands specified: System.UInt64 and int" },
            new object[] { "18446744073709551615", "4294967295", "Unsupported ** operands specified: System.UInt64 and System.UInt32" },
            new object[] { "18446744073709551615", "9223372036854775807", "Unsupported ** operands specified: System.UInt64 and long" },
            new object[] { "18446744073709551615", "18446744073709551615", "Unsupported ** operands specified: System.UInt64 and System.UInt64" },
            new object[] { "18446744073709551615", "18446744073709551616", "Unsupported ** operands specified: System.UInt64 and bigint" },
            new object[] { "18446744073709551615", "12.0", "Unsupported ** operands specified: System.UInt64 and double" },
            new object[] { "18446744073709551616", "4294967295", "Unsupported ** operands specified: bigint and System.UInt32" },
            new object[] { "18446744073709551616", "9223372036854775807", "Unsupported ** operands specified: bigint and long" },
            new object[] { "18446744073709551616", "18446744073709551615", "Unsupported ** operands specified: bigint and System.UInt64" },
            new object[] { "18446744073709551616", "12.0", "Unsupported ** operands specified: bigint and double" },
            new object[] { "-12.0", "4294967295", "Unsupported ** operands specified: double and System.UInt32" },
            new object[] { "-12.0", "9223372036854775807", "Unsupported ** operands specified: double and long" },
            new object[] { "-12.0", "18446744073709551615", "Unsupported ** operands specified: double and System.UInt64" },
            new object[] { "-12.0", "18446744073709551616", "Unsupported ** operands specified: double and bigint" },
        };

    public static IEnumerable<object[]> Modulo_result =>
        Modulo_result_and_type.GetResult();

    public static IEnumerable<object[]> Modulo_type =>
        Modulo_result_and_type.GetTypes();

    private static IEnumerable<object[]> Modulo_result_and_type =>
        new List<object[]>
        {
            new object[] { "5", "3", "2", typeof(int) },
            new object[] { "2", "18446744073709551616", "2", typeof(BigInteger) },
            new object[] { "9", "2.0", "1", typeof(double) },
            new object[] { "2147483647", "2", "1", typeof(int) },
            new object[] { "-2147483648", "2", "0", typeof(int) },
            new object[] { "9223372036854775807", "9223372036854775807", "0", typeof(long) },
            new object[] { "9223372036854775807", "18446744073709551616", "9223372036854775807", typeof(BigInteger) },
            new object[] { "9223372036854775807", "12.0", "8", typeof(double) }, // TODO: We should consider making this unsupported, since it may cause loss of precision.
            new object[] { "18446744073709551616", "3", "1", typeof(BigInteger) },
            new object[] { "18446744073709551616", "9223372036854775807", "2", typeof(BigInteger) },
            new object[] { "18446744073709551616", "18446744073709551616", "0", typeof(BigInteger) },
            new object[] { "9.0", "2.0", "1", typeof(double) },
            new object[] { "-12.0", "2", "-0", typeof(double) },
            new object[] { "-12.0", "9223372036854775807", "-12", typeof(double) },
        };

    public static IEnumerable<object[]> Modulo_unsupported_types =>
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
            new object[] { "18446744073709551616", "4294967295", "Operands must be numbers, not bigint and System.UInt32" },
            new object[] { "18446744073709551616", "18446744073709551615", "Operands must be numbers, not bigint and System.UInt64" },
            new object[] { "18446744073709551616", "12.0", "Operands must be numbers, not bigint and double" },
            new object[] { "-12.0", "4294967295", "Operands must be numbers, not double and System.UInt32" },
            new object[] { "-12.0", "18446744073709551615", "Operands must be numbers, not double and System.UInt64" },
            new object[] { "-12.0", "18446744073709551616", "Operands must be numbers, not double and bigint" },
        };

    public static IEnumerable<object[]> ShiftLeft_result =>
        ShiftLeft_result_and_type.GetResult();

    public static IEnumerable<object[]> ShiftLeft_type =>
        ShiftLeft_result_and_type.GetTypes();

    private static IEnumerable<object[]> ShiftLeft_result_and_type =>
        new List<object[]>
        {
            new object[] { "1", "10", "1024", typeof(int) },
            new object[] { "1073741824", "1", "-2147483648", typeof(int) }, // Wraps around => becomes negative
            new object[] { "9223372036854775807", "2", "-4", typeof(long) }, // Wraps around => becomes negative
            new object[] { "18446744073709551616", "2", "73786976294838206464", typeof(BigInteger) },
        };

    public static IEnumerable<object[]> ShiftLeft_unsupported_types =>
        new List<object[]>
        {
            new object[] { "2", "4294967295", "Operands must be numbers, not int and System.UInt32" },
            new object[] { "2", "9223372036854775807", "Operands must be numbers, not int and long" },
            new object[] { "2", "18446744073709551615", "Operands must be numbers, not int and System.UInt64" },
            new object[] { "2", "18446744073709551616", "Operands must be numbers, not int and bigint" },
            new object[] { "2", "12.0", "Operands must be numbers, not int and double" },
            new object[] { "4294967295", "2", "Operands must be numbers, not System.UInt32 and int" },
            new object[] { "4294967295", "4294967295", "Operands must be numbers, not System.UInt32 and System.UInt32" },
            new object[] { "4294967295", "9223372036854775807", "Operands must be numbers, not System.UInt32 and long" },
            new object[] { "4294967295", "18446744073709551615", "Operands must be numbers, not System.UInt32 and System.UInt64" },
            new object[] { "4294967295", "18446744073709551616", "Operands must be numbers, not System.UInt32 and bigint" },
            new object[] { "4294967295", "12.0", "Operands must be numbers, not System.UInt32 and double" },
            new object[] { "9223372036854775807", "4294967295", "Operands must be numbers, not long and System.UInt32" },
            new object[] { "9223372036854775807", "9223372036854775807", "Operands must be numbers, not long and long" },
            new object[] { "9223372036854775807", "18446744073709551615", "Operands must be numbers, not long and System.UInt64" },
            new object[] { "9223372036854775807", "18446744073709551616", "Operands must be numbers, not long and bigint" },
            new object[] { "9223372036854775807", "12.0", "Operands must be numbers, not long and double" },
            new object[] { "18446744073709551615", "2", "Operands must be numbers, not System.UInt64 and int" },
            new object[] { "18446744073709551615", "4294967295", "Operands must be numbers, not System.UInt64 and System.UInt32" },
            new object[] { "18446744073709551615", "9223372036854775807", "Operands must be numbers, not System.UInt64 and long" },
            new object[] { "18446744073709551615", "18446744073709551615", "Operands must be numbers, not System.UInt64 and System.UInt64" },
            new object[] { "18446744073709551615", "18446744073709551616", "Operands must be numbers, not System.UInt64 and bigint" },
            new object[] { "18446744073709551615", "12.0", "Operands must be numbers, not System.UInt64 and double" },
            new object[] { "18446744073709551616", "4294967295", "Operands must be numbers, not bigint and System.UInt32" },
            new object[] { "18446744073709551616", "9223372036854775807", "Operands must be numbers, not bigint and long" },
            new object[] { "18446744073709551616", "18446744073709551615", "Operands must be numbers, not bigint and System.UInt64" },
            new object[] { "18446744073709551616", "18446744073709551616", "Operands must be numbers, not bigint and bigint" },
            new object[] { "18446744073709551616", "12.0", "Operands must be numbers, not bigint and double" },
            new object[] { "12.0", "2", "Operands must be numbers, not double and int" },
            new object[] { "12.0", "4294967295", "Operands must be numbers, not double and System.UInt32" },
            new object[] { "12.0", "9223372036854775807", "Operands must be numbers, not double and long" },
            new object[] { "12.0", "18446744073709551615", "Operands must be numbers, not double and System.UInt64" },
            new object[] { "12.0", "18446744073709551616", "Operands must be numbers, not double and bigint" },
            new object[] { "12.0", "12.0", "Operands must be numbers, not double and double" },
        };

    public static IEnumerable<object[]> ShiftRight_result =>
        ShiftRight_result_and_type.GetResult();

    public static IEnumerable<object[]> ShiftRight_type =>
        ShiftRight_result_and_type.GetTypes();

    private static IEnumerable<object[]> ShiftRight_result_and_type =>
        new List<object[]>
        {
            new object[] { "2147483647", "32", "2147483647", typeof(int) },
            new object[] { "9223372036854775807", "2", "2305843009213693951", typeof(long) }, // Integer overflow => becomes negative
            new object[] { "18446744073709551616", "2", "4611686018427387904", typeof(BigInteger) },
        };

    public static IEnumerable<object[]> ShiftRight_unsupported_types =>
        new List<object[]>
        {
            new object[] { "2", "4294967295", "Operands must be numbers, not int and System.UInt32" },
            new object[] { "2", "9223372036854775807", "Operands must be numbers, not int and long" },
            new object[] { "2", "18446744073709551615", "Operands must be numbers, not int and System.UInt64" },
            new object[] { "2", "18446744073709551616", "Operands must be numbers, not int and bigint" },
            new object[] { "2", "12.0", "Operands must be numbers, not int and double" },
            new object[] { "4294967295", "2", "Operands must be numbers, not System.UInt32 and int" },
            new object[] { "4294967295", "4294967295", "Operands must be numbers, not System.UInt32 and System.UInt32" },
            new object[] { "4294967295", "9223372036854775807", "Operands must be numbers, not System.UInt32 and long" },
            new object[] { "4294967295", "18446744073709551615", "Operands must be numbers, not System.UInt32 and System.UInt64" },
            new object[] { "4294967295", "18446744073709551616", "Operands must be numbers, not System.UInt32 and bigint" },
            new object[] { "4294967295", "12.0", "Operands must be numbers, not System.UInt32 and double" },
            new object[] { "9223372036854775807", "4294967295", "Operands must be numbers, not long and System.UInt32" },
            new object[] { "9223372036854775807", "9223372036854775807", "Operands must be numbers, not long and long" },
            new object[] { "9223372036854775807", "18446744073709551615", "Operands must be numbers, not long and System.UInt64" },
            new object[] { "9223372036854775807", "18446744073709551616", "Operands must be numbers, not long and bigint" },
            new object[] { "9223372036854775807", "12.0", "Operands must be numbers, not long and double" },
            new object[] { "18446744073709551615", "2", "Operands must be numbers, not System.UInt64 and int" },
            new object[] { "18446744073709551615", "4294967295", "Operands must be numbers, not System.UInt64 and System.UInt32" },
            new object[] { "18446744073709551615", "9223372036854775807", "Operands must be numbers, not System.UInt64 and long" },
            new object[] { "18446744073709551615", "18446744073709551615", "Operands must be numbers, not System.UInt64 and System.UInt64" },
            new object[] { "18446744073709551615", "18446744073709551616", "Operands must be numbers, not System.UInt64 and bigint" },
            new object[] { "18446744073709551615", "12.0", "Operands must be numbers, not System.UInt64 and double" },
            new object[] { "18446744073709551616", "4294967295", "Operands must be numbers, not bigint and System.UInt32" },
            new object[] { "18446744073709551616", "9223372036854775807", "Operands must be numbers, not bigint and long" },
            new object[] { "18446744073709551616", "18446744073709551615", "Operands must be numbers, not bigint and System.UInt64" },
            new object[] { "18446744073709551616", "18446744073709551616", "Operands must be numbers, not bigint and bigint" },
            new object[] { "18446744073709551616", "12.0", "Operands must be numbers, not bigint and double" },
            new object[] { "12.0", "2", "Operands must be numbers, not double and int" },
            new object[] { "12.0", "4294967295", "Operands must be numbers, not double and System.UInt32" },
            new object[] { "12.0", "9223372036854775807", "Operands must be numbers, not double and long" },
            new object[] { "12.0", "18446744073709551615", "Operands must be numbers, not double and System.UInt64" },
            new object[] { "12.0", "18446744073709551616", "Operands must be numbers, not double and bigint" },
            new object[] { "12.0", "12.0", "Operands must be numbers, not double and double" },
        };
}

internal static class IEnumerableObjectExtensionMethods
{
    internal static IEnumerable<object[]> GetResult(this IEnumerable<object[]> resultAndTypes) =>
        resultAndTypes.Select(obj => obj[..3]);

    internal static IEnumerable<object[]> GetTypes(this IEnumerable<object[]> resultAndTypes) =>
        resultAndTypes.Select(obj => new[] { obj[0], obj[1], obj[3].ToString() });
}
