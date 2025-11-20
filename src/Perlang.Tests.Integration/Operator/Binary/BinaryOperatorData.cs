#pragma warning disable SA1515
using System.Collections.Generic;
using System.Linq;
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
            new object[] { "-12", "34.0", "false" },
            new object[] { "2147483647", "4294967295", "false" },
            new object[] { "2147483647", "18446744073709551616", "false" },
            new object[] { "2147483647", "9223372036854775807", "false" },
            new object[] { "2147483646", "2147483647", "false" },
            new object[] { "2147483647", "2147483646", "true" },
            new object[] { "2147483647", "2147483647", "false" },
            new object[] { "2147483647", "340282349999999991754788743781432688640.0f", "false" },
            new object[] { "2147483647", "12.0", "true" },
            new object[] { "4294967296", "9223372036854775807", "false" },
            new object[] { "4294967295", "33", "true" },
            new object[] { "4294967295", "4294967295", "false" },
            new object[] { "4294967295", "9223372036854775807", "false" },
            new object[] { "4294967295", "18446744073709551615", "false" },
            new object[] { "4294967295", "18446744073709551616", "false" },
            new object[] { "4294967295", "12.0", "true" },
            new object[] { "4294967295", "340282349999999991754788743781432688640.0f", "false" },
            new object[] { "9223372036854775807", "2", "true" },
            new object[] { "9223372036854775807", "4294967295", "true" },
            new object[] { "9223372036854775807", "9223372036854775807", "false" },
            new object[] { "9223372036854775807", "18446744073709551616", "false" },
            new object[] { "9223372036854775807", "340282349999999991754788743781432688640.0f", "false" },
            new object[] { "9223372036854775807", "12.0", "true" },
            new object[] { "18446744073709551615", "4294967295", "true" },
            new object[] { "18446744073709551615", "18446744073709551615", "false" },
            new object[] { "18446744073709551615", "18446744073709551616", "false" },
            new object[] { "18446744073709551615", "340282349999999991754788743781432688640.0f", "false" },
            new object[] { "18446744073709551615", "12.0", "true" },
            new object[] { "18446744073709551616", "2", "true" },
            new object[] { "18446744073709551616", "4294967295", "true" },
            new object[] { "18446744073709551616", "9223372036854775807", "true" },
            new object[] { "18446744073709551616", "18446744073709551615", "true" },
            new object[] { "18446744073709551616", "18446744073709551616", "false" },
            new object[] { "340282349999999991754788743781432688640.0f", "2147483647", "true" },
            new object[] { "340282349999999991754788743781432688640.0f", "4294967295", "true" },
            new object[] { "340282349999999991754788743781432688640.0f", "9223372036854775807", "true" },
            new object[] { "340282349999999991754788743781432688640.0f", "18446744073709551615", "true" },
            new object[] { "340282349999999991754788743781432688640.0f", "340282349999999991754788743781432688640.0f", "false" },
            new object[] { "340282349999999991754788743781432688640.0f", "12.0", "true" },
            new object[] { "12.0", "2147483647", "false" },
            new object[] { "12.0", "4294967295", "false" },
            new object[] { "12.0", "9223372036854775807", "false" },
            new object[] { "12.0", "18446744073709551615", "false" },
            new object[] { "12.0", "340282349999999991754788743781432688640.0f", "false" },
            new object[] { "12.0", "34.0", "false" },
            new object[] { "34.0", "33.0", "true" },
        };

    public static IEnumerable<object[]> Greater_unsupported_types =>
        new List<object[]>
        {
            new object[] { "2", "18446744073709551615", "Unsupported > operand types: 'int' and 'ulong'" },
            new object[] { "9223372036854775807", "18446744073709551615", "Unsupported > operand types: 'long' and 'ulong'" },
            new object[] { "18446744073709551615", "2", "Unsupported > operand types: 'ulong' and 'int'" },
            new object[] { "18446744073709551615", "9223372036854775807", "Unsupported > operand types: 'ulong' and 'long'" },
            new object[] { "18446744073709551616", "340282349999999991754788743781432688640.0f", "Unsupported > operand types: 'bigint' and 'float'" },
            new object[] { "18446744073709551616", "12.0", "Unsupported > operand types: 'bigint' and 'double'" },
            new object[] { "340282349999999991754788743781432688640.0f", "18446744073709551616", "Unsupported > operand types: 'float' and 'bigint'" },
            new object[] { "-12.0", "18446744073709551616", "Unsupported > operand types: 'double' and 'bigint'" },
        };

    public static IEnumerable<object[]> GreaterEqual =>
        new List<object[]>
        {
            new object[] { "2147483647", "4294967295", "false" },
            new object[] { "2147483647", "18446744073709551616", "false" },
            new object[] { "2147483647", "9223372036854775807", "false" },
            new object[] { "-2147483648", "34.0", "false" },
            new object[] { "2147483646", "2147483647", "false" },
            new object[] { "2147483647", "2147483647", "true" },
            new object[] { "2147483647", "2147483646", "true" },
            new object[] { "2147483647", "340282349999999991754788743781432688640.0f", "false" },
            new object[] { "2147483647", "33.0", "true" },
            new object[] { "4294967295", "33", "true" },
            new object[] { "4294967295", "4294967295", "true" },
            new object[] { "4294967295", "9223372036854775807", "false" },
            new object[] { "4294967295", "18446744073709551615", "false" },
            new object[] { "4294967295", "18446744073709551616", "false" },
            new object[] { "4294967295", "340282349999999991754788743781432688640.0f", "false" },
            new object[] { "4294967295", "12.0", "true" },
            new object[] { "9223372036854775807", "2", "true" },
            new object[] { "9223372036854775807", "4294967295", "true" },
            new object[] { "9223372036854775807", "9223372036854775807", "true" },
            new object[] { "9223372036854775807", "18446744073709551616", "false" },
            new object[] { "9223372036854775807", "340282349999999991754788743781432688640.0f", "false" },
            new object[] { "9223372036854775807", "12.0", "true" },
            new object[] { "18446744073709551615", "4294967295", "true" },
            new object[] { "18446744073709551615", "18446744073709551615", "true" },
            new object[] { "18446744073709551615", "18446744073709551616", "false" },
            new object[] { "18446744073709551615", "340282349999999991754788743781432688640.0f", "false" },
            new object[] { "18446744073709551615", "12.0", "true" },
            new object[] { "18446744073709551616", "2", "true" },
            new object[] { "18446744073709551616", "4294967295", "true" },
            new object[] { "18446744073709551616", "9223372036854775807", "true" },
            new object[] { "18446744073709551616", "18446744073709551615", "true" },
            new object[] { "18446744073709551616", "18446744073709551616", "true" },
            new object[] { "340282349999999991754788743781432688640.0f", "2147483647", "true" },
            new object[] { "340282349999999991754788743781432688640.0f", "4294967295", "true" },
            new object[] { "340282349999999991754788743781432688640.0f", "9223372036854775807", "true" },
            new object[] { "340282349999999991754788743781432688640.0f", "18446744073709551615", "true" },
            new object[] { "340282349999999991754788743781432688640.0f", "340282349999999991754788743781432688640.0f", "true" },
            new object[] { "340282349999999991754788743781432688640.0f", "12.0", "true" },
            new object[] { "12.0", "34", "false" },
            new object[] { "12.0", "4294967295", "false" },
            new object[] { "12.0", "9223372036854775807", "false" },
            new object[] { "12.0", "18446744073709551615", "false" },
            new object[] { "12.0", "34.0", "false" },
            new object[] { "34.0", "33.0", "true" },
            new object[] { "12.0", "340282349999999991754788743781432688640.0f", "false" },
        };

    public static IEnumerable<object[]> GreaterEqual_unsupported_types =>
        new List<object[]>
        {
            new object[] { "2", "18446744073709551615", "Unsupported >= operand types: 'int' and 'ulong'" },
            new object[] { "9223372036854775807", "18446744073709551615", "Unsupported >= operand types: 'long' and 'ulong'" },
            new object[] { "18446744073709551615", "2", "Unsupported >= operand types: 'ulong' and 'int'" },
            new object[] { "18446744073709551615", "9223372036854775807", "Unsupported >= operand types: 'ulong' and 'long'" },
            new object[] { "18446744073709551616", "340282349999999991754788743781432688640.0f", "Unsupported >= operand types: 'bigint' and 'float'" },
            new object[] { "18446744073709551616", "12.0", "Unsupported >= operand types: 'bigint' and 'double'" },
            new object[] { "340282349999999991754788743781432688640.0f", "18446744073709551616", "Unsupported >= operand types: 'float' and 'bigint'" },
            new object[] { "-12.0", "18446744073709551616", "Unsupported >= operand types: 'double' and 'bigint'" },
        };

    public static IEnumerable<object[]> Less =>
        new List<object[]>
        {
            new object[] { "2", "34", "true" },
            new object[] { "2", "4294967295", "true" },
            new object[] { "2", "9223372036854775807", "true" },
            new object[] { "2", "18446744073709551616", "true" },
            new object[] { "2", "-34", "false" },
            new object[] { "2", "34.0", "true" },
            new object[] { "-12", "34", "true" },
            new object[] { "-12", "-34", "false" },
            new object[] { "-12", "-34.0", "false" },
            new object[] { "2147483647", "33.0", "false" },
            new object[] { "2147483647", "340282349999999991754788743781432688640.0f", "true" },
            new object[] { "4294967295", "33", "false" },
            new object[] { "4294967295", "4294967295", "false" },
            new object[] { "4294967295", "9223372036854775807", "true" },
            new object[] { "4294967295", "18446744073709551615", "true" },
            new object[] { "4294967295", "340282349999999991754788743781432688640.0f", "true" },
            new object[] { "4294967295", "18446744073709551616", "true" },
            new object[] { "4294967295", "12.0", "false" },
            new object[] { "9223372036854775807", "2", "false" },
            new object[] { "9223372036854775807", "4294967295", "false" },
            new object[] { "9223372036854775807", "9223372036854775807", "false" },
            new object[] { "9223372036854775807", "18446744073709551616", "true" },
            new object[] { "9223372036854775807", "340282349999999991754788743781432688640.0f", "true" },
            new object[] { "9223372036854775807", "12.0", "false" },
            new object[] { "18446744073709551615", "4294967295", "false" },
            new object[] { "18446744073709551615", "18446744073709551615", "false" },
            new object[] { "18446744073709551615", "18446744073709551616", "true" },
            new object[] { "18446744073709551615", "340282349999999991754788743781432688640.0f", "true" },
            new object[] { "18446744073709551615", "12.0", "false" },
            new object[] { "18446744073709551616", "2", "false" },
            new object[] { "18446744073709551616", "4294967295", "false" },
            new object[] { "18446744073709551616", "9223372036854775807", "false" },
            new object[] { "18446744073709551616", "18446744073709551615", "false" },
            new object[] { "18446744073709551616", "18446744073709551616", "false" },
            new object[] { "18446744073709551616", "18446744073709551617", "true" },
            new object[] { "12.0", "-34", "false" },
            new object[] { "-12.0", "-34", "false" },
            new object[] { "12.0", "4294967295", "true" },
            new object[] { "12.0", "9223372036854775807", "true" },
            new object[] { "-12.0", "9223372036854775807", "true" },
            new object[] { "12.0", "18446744073709551615", "true" },
            new object[] { "12.0", "340282349999999991754788743781432688640.0f", "true" },
            new object[] { "340282349999999991754788743781432688640.0f", "2147483647", "false" },
            new object[] { "340282349999999991754788743781432688640.0f", "4294967295", "false" },
            new object[] { "340282349999999991754788743781432688640.0f", "9223372036854775807", "false" },
            new object[] { "340282349999999991754788743781432688640.0f", "18446744073709551615", "false" },
            new object[] { "340282349999999991754788743781432688640.0f", "340282349999999991754788743781432688640.0f", "false" },
            new object[] { "340282349999999991754788743781432688640.0f", "12.0", "false" },
            new object[] { "34.0", "33.0", "false" },
            new object[] { "12.0", "34.0", "true" },
        };

    public static IEnumerable<object[]> Less_unsupported_types =>
        new List<object[]>
        {
            new object[] { "2", "18446744073709551615", "Unsupported < operand types: 'int' and 'ulong'" },
            new object[] { "9223372036854775807", "18446744073709551615", "Unsupported < operand types: 'long' and 'ulong'" },
            new object[] { "18446744073709551615", "2", "Unsupported < operand types: 'ulong' and 'int'" },
            new object[] { "18446744073709551615", "9223372036854775807", "Unsupported < operand types: 'ulong' and 'long'" },
            new object[] { "18446744073709551616", "340282349999999991754788743781432688640.0f", "Unsupported < operand types: 'bigint' and 'float'" },
            new object[] { "18446744073709551616", "12.0", "Unsupported < operand types: 'bigint' and 'double'" },
            new object[] { "340282349999999991754788743781432688640.0f", "18446744073709551616", "Unsupported < operand types: 'float' and 'bigint'" },
            new object[] { "12.0", "18446744073709551616", "Unsupported < operand types: 'double' and 'bigint'" },
        };

    public static IEnumerable<object[]> LessEqual =>
        new List<object[]>
        {
            new object[] { "-2147483648", "4294967295", "true" },
            new object[] { "2147483646", "2147483647", "true" },
            new object[] { "2147483647", "4294967295", "true" },
            new object[] { "2147483647", "18446744073709551616", "true" },
            new object[] { "2147483647", "9223372036854775807", "true" },
            new object[] { "2147483647", "2147483647", "true" },
            new object[] { "2147483647", "2147483646", "false" },
            new object[] { "2147483647", "33.0", "false" },
            new object[] { "2147483647", "340282349999999991754788743781432688640.0f", "true" },
            new object[] { "4294967295", "33", "false" },
            new object[] { "4294967295", "4294967295", "true" },
            new object[] { "4294967295", "9223372036854775807", "true" },
            new object[] { "4294967295", "18446744073709551615", "true" },
            new object[] { "4294967295", "18446744073709551616", "true" },
            new object[] { "4294967295", "340282349999999991754788743781432688640.0f", "true" },
            new object[] { "4294967295", "12.0", "false" },
            new object[] { "9223372036854775807", "2", "false" },
            new object[] { "9223372036854775807", "4294967295", "false" },
            new object[] { "9223372036854775807", "9223372036854775807", "true" },
            new object[] { "9223372036854775807", "18446744073709551616", "true" },
            new object[] { "9223372036854775807", "340282349999999991754788743781432688640.0f", "true" },
            new object[] { "9223372036854775807", "12.0", "false" },
            new object[] { "18446744073709551615", "4294967295", "false" },
            new object[] { "18446744073709551615", "18446744073709551615", "true" },
            new object[] { "18446744073709551615", "18446744073709551616", "true" },
            new object[] { "18446744073709551615", "340282349999999991754788743781432688640.0f", "true" },
            new object[] { "18446744073709551615", "12.0", "false" },
            new object[] { "18446744073709551616", "2", "false" },
            new object[] { "18446744073709551616", "4294967295", "false" },
            new object[] { "18446744073709551616", "9223372036854775807", "false" },
            new object[] { "18446744073709551616", "18446744073709551615", "false" },
            new object[] { "18446744073709551616", "18446744073709551616", "true" },
            new object[] { "340282349999999991754788743781432688640.0f", "2147483647", "false" },
            new object[] { "340282349999999991754788743781432688640.0f", "4294967295", "false" },
            new object[] { "340282349999999991754788743781432688640.0f", "9223372036854775807", "false" },
            new object[] { "340282349999999991754788743781432688640.0f", "18446744073709551615", "false" },
            new object[] { "340282349999999991754788743781432688640.0f", "340282349999999991754788743781432688640.0f", "true" },
            new object[] { "340282349999999991754788743781432688640.0f", "34.0", "false" },
            new object[] { "12.0", "34", "true" },
            new object[] { "12.0", "-34", "false" },
            new object[] { "12.0", "4294967295", "true" },
            new object[] { "12.0", "9223372036854775807", "true" },
            new object[] { "12.0", "18446744073709551615", "true" },
            new object[] { "12.0", "34.0", "true" },
            new object[] { "34.0", "33.0", "false" },
            new object[] { "34.0", "340282349999999991754788743781432688640.0f", "true" },
        };

    public static IEnumerable<object[]> LessEqual_unsupported_types =>
        new List<object[]>
        {
            new object[] { "2", "18446744073709551615", "Unsupported <= operand types: 'int' and 'ulong'" },
            new object[] { "9223372036854775807", "18446744073709551615", "Unsupported <= operand types: 'long' and 'ulong'" },
            new object[] { "18446744073709551615", "2", "Unsupported <= operand types: 'ulong' and 'int'" },
            new object[] { "18446744073709551615", "9223372036854775807", "Unsupported <= operand types: 'ulong' and 'long'" },
            new object[] { "18446744073709551616", "340282349999999991754788743781432688640.0f", "Unsupported <= operand types: 'bigint' and 'float'" },
            new object[] { "18446744073709551616", "12.0", "Unsupported <= operand types: 'bigint' and 'double'" },
            new object[] { "340282349999999991754788743781432688640.0f", "18446744073709551616", "Unsupported <= operand types: 'float' and 'bigint'" },
            new object[] { "-12.0", "18446744073709551616", "Unsupported <= operand types: 'double' and 'bigint'" },
        };

    public static IEnumerable<object[]> NotEqual =>
        new List<object[]>
        {
            new object[] { "12", "34", "true" },
            new object[] { "12", "12", "false" },
            new object[] { "12", "12.0", "false" }, // Same value but different types. We follow the precedent of C#, Java and C++ and consider these to be equal.
            new object[] { "12.0", "12", "false" }, // Likewise

            new object[] { "2147483647", "4294967295", "true" },
            new object[] { "2147483647", "9223372036854775807", "true" },
            new object[] { "2147483647", "18446744073709551615", "true" },
            new object[] { "2147483647", "18446744073709551616", "true" },
            new object[] { "2147483647", "340282349999999991754788743781432688640.0f", "true" },
            new object[] { "4294967295", "33", "true" },
            new object[] { "4294967295", "4294967295", "false" },
            new object[] { "4294967295", "9223372036854775807", "true" },
            new object[] { "4294967295", "18446744073709551615", "true" },
            new object[] { "4294967295", "18446744073709551616", "true" },
            new object[] { "4294967295", "340282349999999991754788743781432688640.0f", "true" },
            new object[] { "4294967295", "12.0", "true" },
            new object[] { "9223372036854775807", "2", "true" },
            new object[] { "9223372036854775807", "4294967295", "true" },
            new object[] { "9223372036854775807", "9223372036854775807", "false" },
            new object[] { "9223372036854775807", "18446744073709551615", "true" },
            new object[] { "9223372036854775807", "18446744073709551616", "true" },
            new object[] { "9223372036854775807", "340282349999999991754788743781432688640.0f", "true" },
            new object[] { "9223372036854775807", "12.0", "true" },
            new object[] { "18446744073709551615", "2", "true" },
            new object[] { "18446744073709551615", "4294967295", "true" },
            new object[] { "18446744073709551615", "9223372036854775807", "true" },
            new object[] { "18446744073709551615", "18446744073709551615", "false" },
            new object[] { "18446744073709551615", "18446744073709551616", "true" },
            new object[] { "18446744073709551615", "340282349999999991754788743781432688640.0f", "true" },
            new object[] { "18446744073709551615", "12.0", "true" },
            new object[] { "18446744073709551616", "2", "true" },
            new object[] { "18446744073709551616", "4294967295", "true" },
            new object[] { "18446744073709551616", "9223372036854775807", "true" },
            new object[] { "18446744073709551616", "18446744073709551615", "true" },
            new object[] { "18446744073709551616", "18446744073709551616", "false" },
            new object[] { "18446744073709551616", "340282349999999991754788743781432688640.0f", "true" },
            new object[] { "18446744073709551616", "12.0", "true" },
            new object[] { "340282349999999991754788743781432688640.0f", "2147483647", "true" },
            new object[] { "340282349999999991754788743781432688640.0f", "4294967295", "true" },
            new object[] { "340282349999999991754788743781432688640.0f", "9223372036854775807", "true" },
            new object[] { "340282349999999991754788743781432688640.0f", "18446744073709551615", "true" },
            new object[] { "340282349999999991754788743781432688640.0f", "18446744073709551616", "true" },
            new object[] { "340282349999999991754788743781432688640.0f", "340282349999999991754788743781432688640.0f", "false" },
            new object[] { "340282349999999991754788743781432688640.0f", "12.0", "true" },
            new object[] { "-12.0", "4294967295", "true" },
            new object[] { "-12.0", "9223372036854775807", "true" },
            new object[] { "-12.0", "18446744073709551615", "true" },
            new object[] { "-12.0", "18446744073709551616", "true" },
            new object[] { "-12.0", "340282349999999991754788743781432688640.0f", "true" },
            new object[] { "12.345", "12.345", "false" },
            new object[] { "12.345", "67.890", "true" },
        };

    public static IEnumerable<object[]> Equal =>
        new List<object[]>
        {
            new object[] { "12", "34", "false" },
            new object[] { "12", "12", "true" },
            new object[] { "12", "12.0", "true" }, // Same value but different types. We follow the precedent of C#, Java and C++ and consider these to be equal.
            new object[] { "12.0", "12", "true" }, // Likewise
            new object[] { "12.345", "12.345", "true" },
            new object[] { "12.345", "67.890", "false" },

            new object[] { "2147483647", "4294967295", "false" },
            new object[] { "2147483647", "9223372036854775807", "false" },
            new object[] { "2147483647", "18446744073709551615", "false" },
            new object[] { "2147483647", "18446744073709551616", "false" },
            new object[] { "2147483647", "340282349999999991754788743781432688640.0f", "false" },
            new object[] { "4294967295", "33", "false" },
            new object[] { "4294967295", "4294967295", "true" },
            new object[] { "4294967295", "9223372036854775807", "false" },
            new object[] { "4294967295", "18446744073709551615", "false" },
            new object[] { "4294967295", "18446744073709551616", "false" },
            new object[] { "4294967295", "340282349999999991754788743781432688640.0f", "false" },
            new object[] { "4294967295", "12.0", "false" },
            new object[] { "9223372036854775807", "2147483647", "false" },
            new object[] { "9223372036854775807", "4294967295", "false" },
            new object[] { "9223372036854775807", "9223372036854775807", "true" },
            new object[] { "9223372036854775807", "18446744073709551615", "false" },
            new object[] { "9223372036854775807", "18446744073709551616", "false" },
            new object[] { "9223372036854775807", "340282349999999991754788743781432688640.0f", "false" },
            new object[] { "9223372036854775807", "12.0", "false" },
            new object[] { "9223372036854775807", "9223372036854775807.0", "true" },
            new object[] { "18446744073709551615", "2147483647", "false" },
            new object[] { "18446744073709551615", "4294967295", "false" },
            new object[] { "18446744073709551615", "9223372036854775807", "false" },
            new object[] { "18446744073709551615", "18446744073709551615", "true" },
            new object[] { "18446744073709551615", "18446744073709551616", "false" },
            new object[] { "18446744073709551615", "340282349999999991754788743781432688640.0f", "false" },
            new object[] { "18446744073709551615", "12.0", "false" },
            new object[] { "18446744073709551616", "2147483647", "false" },
            new object[] { "18446744073709551616", "4294967295", "false" },
            new object[] { "18446744073709551616", "9223372036854775807", "false" },
            new object[] { "18446744073709551616", "18446744073709551615", "false" },
            new object[] { "18446744073709551616", "18446744073709551616", "true" },
            new object[] { "18446744073709551616", "340282349999999991754788743781432688640.0f", "false" },
            new object[] { "18446744073709551616", "12.0", "false" },
            new object[] { "340282349999999991754788743781432688640.0f", "2147483647", "false" },
            new object[] { "340282349999999991754788743781432688640.0f", "4294967295", "false" },
            new object[] { "340282349999999991754788743781432688640.0f", "9223372036854775807", "false" },
            new object[] { "340282349999999991754788743781432688640.0f", "18446744073709551615", "false" },
            new object[] { "340282349999999991754788743781432688640.0f", "18446744073709551616", "false" },
            new object[] { "340282349999999991754788743781432688640.0f", "340282349999999991754788743781432688640.0f", "true" },
            new object[] { "340282349999999991754788743781432688640.0f", "12.0", "false" },
            new object[] { "-12.0", "4294967295", "false" },
            new object[] { "-12.0", "9223372036854775807", "false" },
            new object[] { "-12.0", "18446744073709551615", "false" },
            new object[] { "-12.0", "18446744073709551616", "false" },
            new object[] { "-12.0", "340282349999999991754788743781432688640.0f", "false" },
        };

    public static IEnumerable<object[]> Subtraction_result =>
        Subtraction_result_and_type.GetResult();

    public static IEnumerable<object[]> Subtraction_type =>
        Subtraction_result_and_type.GetTypes();

    private static IEnumerable<object[]> Subtraction_result_and_type =>
        new List<object[]>
        {
            new object[] { "2", "34", "-32", "perlang.Int32" },
            new object[] { "2", "-34", "36", "perlang.Int32" },
            new object[] { "2", "4294967295", "-4294967293", "perlang.Int64" },
            new object[] { "2", "9223372036854775807", "-9223372036854775805", "perlang.Int64" },
            new object[] { "2", "18446744073709551616", "-18446744073709551614", "perlang.BigInt" },

            new object[] { "2147483647", "2.0f", "2.147484E+09", "perlang.Float" },
            new object[] { "2147483647", "2.0", "2147483645", "perlang.Double" },
            new object[] { "4294967295", "12", "4294967283", "perlang.Int64" },
            new object[] { "4294967295", "4294967295", "0", "perlang.UInt32" },
            new object[] { "4294967295", "9223372036854775807", "-9223372032559808512", "perlang.Int64" }, // Negative integer overflow
            new object[] { "4294967295", "18446744073709551615", "4294967296", "perlang.UInt64" }, // Positive integer overflow
            new object[] { "4294967295", "18446744073709551616", "-18446744069414584321", "perlang.BigInt" },
            new object[] { "4294967295", "2.0f", "4.294967E+09", "perlang.Float" },
            new object[] { "4294967295", "2.0", "4294967293", "perlang.Double" },
            new object[] { "9223372036854775807", "2", "9223372036854775805", "perlang.Int64" },
            new object[] { "9223372036854775807", "4294967295", "9223372032559808512", "perlang.Int64" },
            new object[] { "9223372036854775807", "9223372036854775807", "0", "perlang.Int64" },
            new object[] { "9223372036854775807", "18446744073709551616", "-9223372036854775809", "perlang.BigInt" },
            new object[] { "9223372036854775807", "2.0f", "9.223372E+18", "perlang.Float" },
            new object[] { "9223372036854775807", "2.0", "9.22337203685478E+18", "perlang.Double" },
            new object[] { "18446744073709551615", "4294967295", "18446744069414584320", "perlang.UInt64" },
            new object[] { "18446744073709551615", "18446744073709551615", "0", "perlang.UInt64" },
            new object[] { "18446744073709551615", "18446744073709551616", "-1", "perlang.BigInt" },
            new object[] { "18446744073709551615", "2.0f", "1.844674E+19", "perlang.Float" },
            new object[] { "18446744073709551615", "2.0", "1.84467440737096E+19", "perlang.Double" },
            new object[] { "18446744073709551616", "2", "18446744073709551614", "perlang.BigInt" },
            new object[] { "18446744073709551616", "4294967295", "18446744069414584321", "perlang.BigInt" },
            new object[] { "18446744073709551616", "9223372036854775807", "9223372036854775809", "perlang.BigInt" },
            new object[] { "18446744073709551616", "18446744073709551615", "1", "perlang.BigInt" },
            new object[] { "18446744073709551616", "18446744073709551617", "-1", "perlang.BigInt" },
            new object[] { "2.0f", "2147483647", "-2.147484E+09", "perlang.Float" },
            new object[] { "2.0f", "4294967295", "-4.294967E+09", "perlang.Float" },
            new object[] { "2.0f", "9223372036854775807", "-9.223372E+18", "perlang.Float" },
            new object[] { "2.0f", "18446744073709551615", "-1.844674E+19", "perlang.Float" },
            new object[] { "2.0f", "2.0f", "0", "perlang.Float" },
            new object[] { "2.0f", "2.0", "0", "perlang.Double" },
            new object[] { "-12.0", "-34", "22", "perlang.Double" },
            new object[] { "-12.0", "4294967295", "-4294967307", "perlang.Double" },
            new object[] { "-12.0", "9223372036854775807", "-9.22337203685478E+18", "perlang.Double" },
            new object[] { "-12.0", "18446744073709551615", "-1.84467440737096E+19", "perlang.Double" },
            new object[] { "2.0", "2.0f", "0", "perlang.Double" },
            new object[] { "12.0", "34.0", "-22", "perlang.Double" }, // Doubles with fraction part zero => fraction part excluded in string representation.
        };

    public static IEnumerable<object[]> Subtraction_unsupported_types =>
        new List<object[]>
        {
            new object[] { "2", "18446744073709551615", "Unsupported - operand types: 'int' and 'ulong'" },
            new object[] { "9223372036854775807", "18446744073709551615", "Unsupported - operand types: 'long' and 'ulong'" },
            new object[] { "18446744073709551615", "2", "Unsupported - operand types: 'ulong' and 'int'" },
            new object[] { "18446744073709551615", "9223372036854775807", "Unsupported - operand types: 'ulong' and 'long'" },
            new object[] { "18446744073709551616", "2.0f", "Unsupported - operand types: 'bigint' and 'float'" },
            new object[] { "18446744073709551616", "2.0", "Unsupported - operand types: 'bigint' and 'double'" },
            new object[] { "2.0f", "18446744073709551616", "Unsupported - operand types: 'float' and 'bigint'" },
            new object[] { "2.0", "18446744073709551616", "Unsupported - operand types: 'double' and 'bigint'" },
        };

    public static IEnumerable<object[]> SubtractionAssignment_result =>
        SubtractionAssignment_result_and_type.GetResult();

    public static IEnumerable<object[]> SubtractionAssignment_type =>
        SubtractionAssignment_result_and_type.GetTypes();

    private static IEnumerable<object[]> SubtractionAssignment_result_and_type =>
        new List<object[]>
        {
            new object[] { "12", "34", "-22", "perlang.Int32" },
            new object[] { "12", "-34", "46", "perlang.Int32" },
            new object[] { "-12", "34", "-46", "perlang.Int32" },
            new object[] { "-12", "-34", "22", "perlang.Int32" },

            new object[] { "2147483647", "2147483647", "0", "perlang.Int32" },
            new object[] { "4294967295", "4294967295", "0", "perlang.UInt32" },
            new object[] { "9223372036854775807", "2", "9223372036854775805", "perlang.Int64" },
            new object[] { "9223372036854775807", "4294967295", "9223372032559808512", "perlang.Int64" },
            new object[] { "9223372036854775807", "9223372036854775807", "0", "perlang.Int64" },
            new object[] { "18446744073709551615", "4294967295", "18446744069414584320", "perlang.UInt64" },
            new object[] { "18446744073709551615", "18446744073709551615", "0", "perlang.UInt64" },
            new object[] { "18446744073709551616", "2", "18446744073709551614", "perlang.BigInt" },
            new object[] { "18446744073709551616", "4294967295", "18446744069414584321", "perlang.BigInt" },
            new object[] { "18446744073709551616", "9223372036854775807", "9223372036854775809", "perlang.BigInt" },
            new object[] { "18446744073709551616", "18446744073709551615", "1", "perlang.BigInt" },
            new object[] { "18446744073709551616", "18446744073709551617", "-1", "perlang.BigInt" },
            new object[] { "2.0f", "2147483647", "-2.147484E+09", "perlang.Float" },
            new object[] { "2.0f", "4294967295", "-4.294967E+09", "perlang.Float" },
            new object[] { "2.0f", "9223372036854775807", "-9.223372E+18", "perlang.Float" },
            new object[] { "2.0f", "18446744073709551615", "-1.844674E+19", "perlang.Float" },
            new object[] { "2.0f", "34.0f", "-32", "perlang.Float" },
            new object[] { "12.0", "34.0f", "-22", "perlang.Double" },
            new object[] { "-12.0", "-34", "22", "perlang.Double" }, // Doubles with fraction part zero => fraction part excluded in string representation.
            new object[] { "-12.0", "4294967295", "-4294967307", "perlang.Double" },
            new object[] { "-12.0", "9223372036854775807", "-9.22337203685478E+18", "perlang.Double" },
            new object[] { "12.0", "18446744073709551615", "-1.84467440737096E+19", "perlang.Double" },
            new object[] { "12.0", "34.0", "-22", "perlang.Double" },
        };

    public static IEnumerable<object[]> SubtractionAssignment_unsupported_types =>
        new List<object[]>
        {
            new object[] { "2147483647", "4294967295", "Cannot assign 'long' to 'int' variable" },
            new object[] { "2147483647", "9223372036854775807", "Cannot assign 'long' to 'int' variable" },
            new object[] { "2147483647", "18446744073709551615", "Cannot assign 'ulong' to 'int' variable" },
            new object[] { "2147483647", "18446744073709551616", "Cannot assign 'bigint' to 'int' variable" },
            new object[] { "2147483647", "2.0f", "Cannot assign 'float' to 'int' variable" },
            new object[] { "2147483647", "2.0", "Cannot assign 'double' to 'int' variable" },
            new object[] { "4294967295", "2147483647", "Cannot assign 'long' to 'uint' variable" }, // TODO: Should work when the rvalue is an integer literal, but requires #352 to be implemented.
            new object[] { "4294967295", "9223372036854775807", "Cannot assign 'long' to 'uint' variable" },
            new object[] { "4294967295", "18446744073709551615", "Cannot assign 'ulong' to 'uint' variable" },
            new object[] { "4294967295", "18446744073709551616", "Cannot assign 'bigint' to 'uint' variable" },
            new object[] { "4294967295", "2.0f", "Cannot assign 'float' to 'uint' variable" },
            new object[] { "4294967295", "2.0", "Cannot assign 'double' to 'uint' variable" },
            new object[] { "9223372036854775807", "18446744073709551615", "Cannot assign 'ulong' to 'long' variable" },
            new object[] { "9223372036854775807", "18446744073709551616", "Cannot assign 'bigint' to 'long' variable" },
            new object[] { "9223372036854775807", "2.0f", "Cannot assign 'float' to 'long' variable" },
            new object[] { "9223372036854775807", "2.0", "Cannot assign 'double' to 'long' variable" },
            new object[] { "18446744073709551615", "2", "Cannot assign 'int' to 'ulong' variable" },
            new object[] { "18446744073709551615", "9223372036854775807", "Cannot assign 'long' to 'ulong' variable" },
            new object[] { "18446744073709551615", "2.0f", "Cannot assign 'float' to 'ulong' variable" },
            new object[] { "18446744073709551615", "2.0", "Cannot assign 'double' to 'ulong' variable" },
            new object[] { "18446744073709551615", "18446744073709551616", "Cannot assign 'bigint' to 'ulong' variable" },
            new object[] { "18446744073709551616", "2.0f", "Cannot assign 'float' to 'bigint' variable" },
            new object[] { "18446744073709551616", "2.0", "Cannot assign 'double' to 'bigint' variable" },
            new object[] { "2.0f", "18446744073709551616", "Cannot assign 'bigint' to 'float' variable" },
            new object[] { "2.0f", "2.0", "Cannot assign 'double' to 'float' variable" },
            new object[] { "2.0", "18446744073709551616", "Cannot assign 'bigint' to 'double' variable" }
        };

    public static IEnumerable<object[]> Addition_result =>
        Addition_result_and_type.GetResult();

    public static IEnumerable<object[]> Addition_type =>
        Addition_result_and_type.GetTypes();

    private static IEnumerable<object[]> Addition_result_and_type =>
        new List<object[]>
        {
            new object[] { "12", "34", "46", "perlang.Int32" },
            new object[] { "12", "-34", "-22", "perlang.Int32" },
            new object[] { "-12", "34", "22", "perlang.Int32" },
            new object[] { "-12", "4294967295", "4294967283", "perlang.Int64" },
            new object[] { "-12", "-34", "-46", "perlang.Int32" },
            new object[] { "4294967295", "340282349999999991754788743781432688640.0f", "3.402823E+38", "perlang.Float" },
            new object[] { "9223372036854775807", "340282349999999991754788743781432688640.0f", "3.402823E+38", "perlang.Float" },
            new object[] { "18446744073709551615", "340282349999999991754788743781432688640.0f", "3.402823E+38", "perlang.Float" },
            new object[] { "-12", "-34.0f", "-46", "perlang.Float" },
            new object[] { "-12", "-34.0", "-46", "perlang.Double" },
            new object[] { "2", "4294967295", "4294967297", "perlang.Int64" }, // `int` + `uint` currently expands to `long` to ensure that all potential return values will fit. This may or may not be a good idea.
            new object[] { "2", "9223372036854775807", "-9223372036854775807", "perlang.Int64" }, // Wraparound because of overflow
            new object[] { "2", "18446744073709551616", "18446744073709551618", "perlang.BigInt" },
            new object[] { "2147483647", "340282349999999991754788743781432688640.0f", "3.402823E+38", "perlang.Float" },
            new object[] { "4294967295", "2", "4294967297", "perlang.Int64" },
            new object[] { "4294967295", "4294967295", "4294967294", "perlang.UInt32" }, // Wraparound because of overflow
            new object[] { "4294967295", "9223372036854775807", "-9223372032559808514", "perlang.Int64" },
            new object[] { "4294967295", "18446744073709551615", "4294967294", "perlang.UInt64" },
            new object[] { "4294967295", "18446744073709551616", "18446744078004518911", "perlang.BigInt" },
            new object[] { "4294967295", "12.0", "4294967307", "perlang.Double" },
            new object[] { "9223372036854775807", "2", "-9223372036854775807", "perlang.Int64" }, // Integer overflow
            new object[] { "9223372036854775807", "4294967295", "-9223372032559808514", "perlang.Int64" },
            new object[] { "9223372036854775807", "9223372036854775807", "-2", "perlang.Int64" }, // Integer overflow
            new object[] { "9223372036854775807", "18446744073709551616", "27670116110564327423", "perlang.BigInt" },
            new object[] { "9223372036854775807", "12.0", "9.22337203685478E+18", "perlang.Double" }, // Loses precision, but supported just like in Java and C#
            new object[] { "18446744073709551615", "4294967295", "4294967294", "perlang.UInt64" }, // Integer overflow
            new object[] { "18446744073709551615", "18446744073709551615", "18446744073709551614", "perlang.UInt64" }, // Integer overflow
            new object[] { "18446744073709551615", "18446744073709551616", "36893488147419103231", "perlang.BigInt" },
            new object[] { "18446744073709551615", "12.0", "1.84467440737096E+19", "perlang.Double" },
            new object[] { "18446744073709551616", "2", "18446744073709551618", "perlang.BigInt" },
            new object[] { "18446744073709551616", "4294967295", "18446744078004518911", "perlang.BigInt" },
            new object[] { "18446744073709551616", "9223372036854775807", "27670116110564327423", "perlang.BigInt" },
            new object[] { "18446744073709551616", "18446744073709551615", "36893488147419103231", "perlang.BigInt" },
            new object[] { "18446744073709551616", "18446744073709551617", "36893488147419103233", "perlang.BigInt" },
            new object[] { "340282349999999991754788743781432688640.0f", "2147483647", "3.402823E+38", "perlang.Float" },
            new object[] { "340282349999999991754788743781432688640.0f", "4294967295", "3.402823E+38", "perlang.Float" },
            new object[] { "340282349999999991754788743781432688640.0f", "9223372036854775807", "3.402823E+38", "perlang.Float" },
            new object[] { "340282349999999991754788743781432688640.0f", "18446744073709551615", "3.402823E+38", "perlang.Float" },
            new object[] { "340282349999999991754788743781432688640.0f", "340282349999999991754788743781432688640.0f", "Infinity", "perlang.Float" },
            new object[] { "340282349999999991754788743781432688640.0f", "12.0", "3.40282346638529E+38", "perlang.Double" },
            new object[] { "12.0", "34", "46", "perlang.Double" },
            new object[] { "12.0", "4294967295", "4294967307", "perlang.Double" },
            new object[] { "12.0", "9223372036854775807", "9.22337203685478E+18", "perlang.Double" },
            new object[] { "12.0", "18446744073709551615", "1.84467440737096E+19", "perlang.Double" },
            new object[] { "12.0", "340282349999999991754788743781432688640.0f", "3.40282346638529E+38", "perlang.Double" },
            new object[] { "12.0", "34.0", "46", "perlang.Double" }, // Doubles with fraction part zero => fraction part excluded in string representation.
            new object[] { "12.1", "34.2", "46.3", "perlang.Double" }
        };

    public static IEnumerable<object[]> Addition_unsupported_types =>
        new List<object[]>
        {
            new object[] { "2", "18446744073709551615", "Unsupported + operand types: 'int' and 'ulong'" },
            new object[] { "340282349999999991754788743781432688640.0f", "18446744073709551616", "Unsupported + operand types: 'float' and 'bigint'" },
            new object[] { "12.0", "18446744073709551616", "Unsupported + operand types: 'double' and 'bigint'" },
            new object[] { "9223372036854775807", "18446744073709551615", "Unsupported + operand types: 'long' and 'ulong'" },
            new object[] { "18446744073709551615", "2", "Unsupported + operand types: 'ulong' and 'int'" },
            new object[] { "18446744073709551615", "9223372036854775807", "Unsupported + operand types: 'ulong' and 'long'" },
            new object[] { "18446744073709551616", "12.0", "Unsupported + operand types: 'bigint' and 'double'" },
            new object[] { "18446744073709551616", "340282349999999991754788743781432688640.0f", "Unsupported + operand types: 'bigint' and 'float'" },
        };

    public static IEnumerable<object[]> AdditionAssignment_result =>
        AdditionAssignment_result_and_type.GetResult();

    public static IEnumerable<object[]> AdditionAssignment_type =>
        AdditionAssignment_result_and_type.GetTypes();

    private static IEnumerable<object[]> AdditionAssignment_result_and_type =>
        new List<object[]>
        {
            new object[] { "12", "34", "46", "perlang.Int32" },
            new object[] { "12", "-34", "-22", "perlang.Int32" },
            new object[] { "-12", "34", "22", "perlang.Int32" },
            new object[] { "-12", "-34", "-46", "perlang.Int32" },
            new object[] { "4294967295", "4294967295", "4294967294", "perlang.UInt32" }, // Integer overflow
            new object[] { "4294967296", "9223372036854775807", "-9223372032559808513", "perlang.Int64" }, // Integer overflow
            new object[] { "9223372036854775807", "4294967295", "-9223372032559808514", "perlang.Int64" }, // Integer overflow
            new object[] { "9223372036854775807", "9223372036854775807", "-2", "perlang.Int64" },
            new object[] { "9223372036854775807", "2", "-9223372036854775807", "perlang.Int64" },
            new object[] { "18446744073709551615", "4294967295", "4294967294", "perlang.UInt64" }, // Integer overflow
            new object[] { "18446744073709551615", "18446744073709551615", "18446744073709551614", "perlang.UInt64" },
            new object[] { "18446744073709551616", "2", "18446744073709551618", "perlang.BigInt" },
            new object[] { "18446744073709551616", "4294967295", "18446744078004518911", "perlang.BigInt" },
            new object[] { "18446744073709551616", "9223372036854775807", "27670116110564327423", "perlang.BigInt" },
            new object[] { "18446744073709551616", "18446744073709551615", "36893488147419103231", "perlang.BigInt" },
            new object[] { "18446744073709551616", "18446744073709551617", "36893488147419103233", "perlang.BigInt" },
            new object[] { "2.0f", "2147483647", "2.147484E+09", "perlang.Float" },
            new object[] { "2.0f", "4294967295", "4.294967E+09", "perlang.Float" },
            new object[] { "2.0f", "9223372036854775807", "9.223372E+18", "perlang.Float" },
            new object[] { "2.0f", "18446744073709551615", "1.844674E+19", "perlang.Float" },
            new object[] { "2.0f", "2.0f", "4", "perlang.Float" },
            new object[] { "-12.0", "-34", "-46", "perlang.Double" },
            new object[] { "-12.0", "4294967295", "4294967283", "perlang.Double" },
            new object[] { "-12.0", "9223372036854775807", "9.22337203685478E+18", "perlang.Double" },
            new object[] { "-12.0", "18446744073709551615", "1.84467440737096E+19", "perlang.Double" },
            new object[] { "12.0", "34.0", "46", "perlang.Double" }, // Doubles with fraction part zero => fraction part excluded in string representation.
            new object[] { "12.1", "34.2", "46.3", "perlang.Double" },
            new object[] { "12.0", "340282349999999991754788743781432688640.0f", "3.40282346638529E+38", "perlang.Double" },
        };

    public static IEnumerable<object[]> AdditionAssignment_unsupported_types =>
        new List<object[]>
        {
            new object[] { "2", "4294967295", "Cannot assign 'long' to 'int' variable" },
            new object[] { "2", "9223372036854775807", "Cannot assign 'long' to 'int' variable" },
            new object[] { "2", "18446744073709551615", "Cannot assign 'ulong' to 'int' variable" },
            new object[] { "2", "18446744073709551616", "Cannot assign 'bigint' to 'int' variable" },
            new object[] { "2", "-34.0", "Cannot assign 'double' to 'int' variable" },

            // This is where it gets interesting: doing `var i = 2147483647; i +=
            // 340282349999999991754788743781432688640.0f` is perfectly valid in Java and will be truncated to
            // Integer.MAX_VALUE, i.e. 2147483647. However, in C# it'll give you a `Cannot convert source type 'float'
            // to target type 'int'` error in Rider; the C# compiler seems to suggest explicit conversion is possible:
            // [CS0266] Cannot implicitly convert type 'float' to 'int'. An explicit conversion exists (are you missing
            // a cast?). We'll go with the C# semantics for now.
            new object[] { "2147483647", "340282349999999991754788743781432688640.0f", "Cannot assign 'float' to 'int' variable" },

            new object[] { "4294967295", "33", "Cannot assign 'long' to 'uint' variable" },
            new object[] { "4294967295", "9223372036854775807", "Cannot assign 'long' to 'uint' variable" },
            new object[] { "4294967295", "18446744073709551615", "Cannot assign 'ulong' to 'uint' variable" },
            new object[] { "4294967295", "18446744073709551616", "Cannot assign 'bigint' to 'uint' variable" },
            new object[] { "4294967295", "340282349999999991754788743781432688640.0f", "Cannot assign 'float' to 'uint' variable" },
            new object[] { "4294967295", "12.0", "Cannot assign 'double' to 'uint' variable" },
            new object[] { "9223372036854775807", "18446744073709551615", "Cannot assign 'ulong' to 'long' variable" },
            new object[] { "9223372036854775807", "18446744073709551616", "Cannot assign 'bigint' to 'long' variable" },
            new object[] { "9223372036854775807", "340282349999999991754788743781432688640.0f", "Cannot assign 'float' to 'long' variable" },
            new object[] { "9223372036854775807", "12.0", "Cannot assign 'double' to 'long' variable" },
            new object[] { "18446744073709551615", "2", "Cannot assign 'int' to 'ulong' variable" },
            new object[] { "18446744073709551615", "12.0", "Cannot assign 'double' to 'ulong' variable" },
            new object[] { "18446744073709551615", "9223372036854775807", "Cannot assign 'long' to 'ulong' variable" },
            new object[] { "18446744073709551615", "18446744073709551616", "Cannot assign 'bigint' to 'ulong' variable" },
            new object[] { "18446744073709551615", "340282349999999991754788743781432688640.0f", "Cannot assign 'float' to 'ulong' variable" },
            new object[] { "18446744073709551616", "12.0", "Cannot assign 'double' to 'bigint' variable" },
            new object[] { "18446744073709551616", "340282349999999991754788743781432688640.0f", "Cannot assign 'float' to 'bigint' variable" },
            new object[] { "2.0f", "18446744073709551616", "Cannot assign 'bigint' to 'float' variable" }, // TODO: could consider supporting this (with precision loss), just like we do for smaller signed/unsigned ints
            new object[] { "2.0f", "12.0", "Cannot assign 'double' to 'float' variable" },
            new object[] { "-12.0", "18446744073709551616", "Cannot assign 'bigint' to 'double' variable" },
        };

    public static IEnumerable<object[]> Division_result =>
        Division_result_and_type.GetResult();

    public static IEnumerable<object[]> Division_type =>
        Division_result_and_type.GetTypes();

    private static IEnumerable<object[]> Division_result_and_type =>
        new List<object[]>
        {
            new object[] { "35", "5", "7", "perlang.Int32" },
            new object[] { "34", "5", "6", "perlang.Int32" }, // `int` division => expecting to be truncated.
            new object[] { "2147483647", "4294967295", "0", "perlang.Int64" },
            new object[] { "2147483647", "9223372036854775807", "0", "perlang.Int64" },
            new object[] { "2147483647", "18446744073709551616", "0", "perlang.BigInt" },
            new object[] { "2147483647", "340282349999999991754788743781432688640.0f", "6.310888E-30", "perlang.Float" },
            new object[] { "4294967295", "2", "2147483647", "perlang.Int64" },
            new object[] { "4294967295", "4294967295", "1", "perlang.UInt32" },
            new object[] { "4294967295", "9223372036854775807", "0", "perlang.Int64" },
            new object[] { "4294967295", "18446744073709551615", "0", "perlang.UInt64" },
            new object[] { "4294967295", "18446744073709551616", "0", "perlang.BigInt" },
            new object[] { "4294967295", "340282349999999991754788743781432688640.0f", "1.262178E-29", "perlang.Float" },
            new object[] { "4294967295", "12.0", "357913941.25", "perlang.Double" },
            new object[] { "9223372036854775807", "2", "4611686018427387903", "perlang.Int64" },
            new object[] { "9223372036854775807", "4294967295", "2147483648", "perlang.Int64" },
            new object[] { "9223372036854775807", "9223372036854775807", "1", "perlang.Int64" },
            new object[] { "9223372036854775807", "18446744073709551616", "0", "perlang.BigInt" },
            new object[] { "9223372036854775807", "340282349999999991754788743781432688640.0f", "2.710506E-20", "perlang.Float" },
            new object[] { "9223372036854775807", "12.0", "7.68614336404565E+17", "perlang.Double" },
            new object[] { "18446744073709551615", "4294967295", "4294967297", "perlang.UInt64" },
            new object[] { "18446744073709551615", "18446744073709551615", "1", "perlang.UInt64" },
            new object[] { "18446744073709551615", "18446744073709551616", "0", "perlang.BigInt" },
            new object[] { "18446744073709551615", "340282349999999991754788743781432688640.0f", "5.421012E-20", "perlang.Float" },
            new object[] { "18446744073709551615", "12.0", "1.53722867280913E+18", "perlang.Double" },
            new object[] { "18446744073709551616", "2", "9223372036854775808", "perlang.BigInt" },
            new object[] { "18446744073709551616", "4294967295", "4294967297", "perlang.BigInt" },
            new object[] { "18446744073709551616", "9223372036854775807", "2", "perlang.BigInt" },
            new object[] { "18446744073709551616", "18446744073709551615", "1", "perlang.BigInt" },
            new object[] { "18446744073709551616", "18446744073709551616", "1", "perlang.BigInt" },
            new object[] { "340282349999999991754788743781432688640.0f", "2147483647", "1.584563E+29", "perlang.Float" },
            new object[] { "340282349999999991754788743781432688640.0f", "4294967295", "7.922816E+28", "perlang.Float" },
            new object[] { "340282349999999991754788743781432688640.0f", "9223372036854775807", "3.689349E+19", "perlang.Float" },
            new object[] { "340282349999999991754788743781432688640.0f", "18446744073709551615", "1.844674E+19", "perlang.Float" },
            new object[] { "340282349999999991754788743781432688640.0f", "340282349999999991754788743781432688640.0f", "1", "perlang.Float" },
            new object[] { "340282349999999991754788743781432688640.0f", "12.0", "2.83568622198774E+37", "perlang.Double" },
            new object[] { "34.0", "5", "6.8", "perlang.Double" },
            new object[] { "12.0", "4294967295", "2.79396772449696E-09", "perlang.Double" },
            new object[] { "12.0", "18446744073709551615", "6.50521303491303E-19", "perlang.Double" },
            new object[] { "34.0", "9223372036854775807", "3.68628738645072E-18", "perlang.Double" },
            new object[] { "34.0", "340282349999999991754788743781432688640.0f", "9.99170257754133E-38", "perlang.Double" },
            new object[] { "34.0", "5.0", "6.8", "perlang.Double" },
            new object[] { "34", "5.0", "6.8", "perlang.Double" }
        };

    public static IEnumerable<object[]> Division_unsupported_types =>
        new List<object[]>
        {
            new object[] { "2", "18446744073709551615", "Unsupported / operand types: 'int' and 'ulong'" },
            new object[] { "9223372036854775807", "18446744073709551615", "Unsupported / operand types: 'long' and 'ulong'" },
            new object[] { "18446744073709551615", "2", "Unsupported / operand types: 'ulong' and 'int'" },
            new object[] { "18446744073709551615", "9223372036854775807", "Unsupported / operand types: 'ulong' and 'long'" },
            new object[] { "18446744073709551616", "340282349999999991754788743781432688640.0f", "Unsupported / operand types: 'bigint' and 'float'" },
            new object[] { "18446744073709551616", "12.0", "Unsupported / operand types: 'bigint' and 'double'" },
            new object[] { "340282349999999991754788743781432688640.0f", "18446744073709551616", "Unsupported / operand types: 'float' and 'bigint'" },
            new object[] { "12.0", "18446744073709551616", "Unsupported / operand types: 'double' and 'bigint'" },
        };

    public static IEnumerable<object[]> Multiplication_result =>
        Multiplication_result_and_type.GetResult();

    public static IEnumerable<object[]> Multiplication_type =>
        Multiplication_result_and_type.GetTypes();

    private static IEnumerable<object[]> Multiplication_result_and_type =>
        new List<object[]>
        {
            new object[] { "5", "3", "15", "perlang.Int32" },
            new object[] { "2", "4294967295", "8589934590", "perlang.Int64" },
            new object[] { "2", "9223372036854775807", "-2", "perlang.Int64" },
            new object[] { "2", "18446744073709551616", "36893488147419103232", "perlang.BigInt" },
            new object[] { "12", "34.0", "408", "perlang.Double" },
            new object[] { "1073741824", "2", "-2147483648", "perlang.Int32" }, // Becomes negative because of signed `int` overflow.
            new object[] { "2147483647", "2.0f", "4.294967E+09", "perlang.Float" },
            new object[] { "4294967295", "2", "8589934590", "perlang.Int64" },
            new object[] { "4294967295", "4294967295", "1", "perlang.UInt32" }, // Unsigned integer overflow
            new object[] { "4294967295", "9223372036854775807", "9223372032559808513", "perlang.Int64" }, // Signed integer overflow
            new object[] { "4294967295", "18446744073709551615", "18446744069414584321", "perlang.UInt64" }, // Unsigned integer overflow
            new object[] { "4294967295", "18446744073709551616", "79228162495817593519834398720", "perlang.BigInt" },
            new object[] { "4294967295", "2.0f", "8.589935E+09", "perlang.Float" },
            new object[] { "4294967295", "12.0", "51539607540", "perlang.Double" },
            new object[] { "9223372036854775807", "2", "-2", "perlang.Int64" },
            new object[] { "9223372036854775807", "4294967295", "9223372032559808513", "perlang.Int64" }, // Overflow
            new object[] { "9223372036854775807", "9223372036854775807", "1", "perlang.Int64" },
            new object[] { "9223372036854775807", "18446744073709551616", "170141183460469231713240559642174554112", "perlang.BigInt" },
            new object[] { "9223372036854775807", "2.0f", "1.844674E+19", "perlang.Float" },
            new object[] { "9223372036854775807", "12.0", "1.10680464442257E+20", "perlang.Double" },
            new object[] { "18446744073709551615", "4294967295", "18446744069414584321", "perlang.UInt64" }, // Unsigned integer overflow
            new object[] { "18446744073709551615", "18446744073709551615", "1", "perlang.UInt64" }, // Unsigned integer overflow
            new object[] { "18446744073709551615", "18446744073709551616", "340282366920938463444927863358058659840", "perlang.BigInt" },
            new object[] { "18446744073709551615", "2.0f", "3.689349E+19", "perlang.Float" },
            new object[] { "18446744073709551615", "12.0", "2.21360928884515E+20", "perlang.Double" },
            new object[] { "18446744073709551616", "2", "36893488147419103232", "perlang.BigInt" },
            new object[] { "18446744073709551616", "4294967295", "79228162495817593519834398720", "perlang.BigInt" },
            new object[] { "18446744073709551616", "9223372036854775807", "170141183460469231713240559642174554112", "perlang.BigInt" },
            new object[] { "18446744073709551616", "18446744073709551615", "340282366920938463444927863358058659840", "perlang.BigInt" },
            new object[] { "18446744073709551616", "18446744073709551616", "340282366920938463463374607431768211456", "perlang.BigInt" },
            new object[] { "2.0f", "2147483647", "4.294967E+09", "perlang.Float" },
            new object[] { "2.0f", "4294967295", "8.589935E+09", "perlang.Float" },
            new object[] { "2.0f", "9223372036854775807", "1.844674E+19", "perlang.Float" },
            new object[] { "2.0f", "18446744073709551615", "3.689349E+19", "perlang.Float" },
            new object[] { "340282349999999991754788743781432688640.0f", "2.0f", "Infinity", "perlang.Float" },
            new object[] { "340282349999999991754788743781432688640.0f", "2.0", "6.80564693277058E+38", "perlang.Double" },
            new object[] { "12.0", "5", "60", "perlang.Double" },
            new object[] { "12.0", "4294967295", "51539607540", "perlang.Double" },
            new object[] { "12.0", "9223372036854775807", "1.10680464442257E+20", "perlang.Double" },
            new object[] { "12.0", "18446744073709551615", "2.21360928884515E+20", "perlang.Double" },
            new object[] { "12.34", "340282349999999991754788743781432688640.0f", "4.19908415751945E+39", "perlang.Double" },
            new object[] { "12.34", "0.3", "3.702", "perlang.Double" }
        };

    public static IEnumerable<object[]> Multiplication_unsupported_types =>
        new List<object[]>
        {
            new object[] { "2", "18446744073709551615", "Unsupported * operand types: 'int' and 'ulong'" },
            new object[] { "9223372036854775807", "18446744073709551615", "Unsupported * operand types: 'long' and 'ulong'" },
            new object[] { "18446744073709551615", "2", "Unsupported * operand types: 'ulong' and 'int'" },
            new object[] { "18446744073709551615", "9223372036854775807", "Unsupported * operand types: 'ulong' and 'long'" },
            new object[] { "18446744073709551616", "2.0f", "Unsupported * operand types: 'bigint' and 'float'" },
            new object[] { "18446744073709551616", "2.0", "Unsupported * operand types: 'bigint' and 'double'" },
            new object[] { "340282349999999991754788743781432688640.0f", "18446744073709551616", "Unsupported * operand types: 'float' and 'bigint'" },
            new object[] { "12.0", "18446744073709551616", "Unsupported * operand types: 'double' and 'bigint'" },
        };

    public static IEnumerable<object[]> Exponential_result =>
        Exponential_result_and_type.GetResult();

    public static IEnumerable<object[]> Exponential_type =>
        Exponential_result_and_type.GetTypes();

    private static IEnumerable<object[]> Exponential_result_and_type =>
        new List<object[]>
        {
            new object[] { "2", "10", "1024", "perlang.BigInt" }, // Currently always expanded to a big integer type, even for cases like this where it would not be required
            new object[] { "2.1", "10", "1667.9880978201", "perlang.Double" },
            new object[] { "2.0", "10.0", "1024", "perlang.Double" },
            new object[] { "2", "9.9", "955.425783333691", "perlang.Double" },
            new object[] { "2147483647", "2.0f", "4.61168601413242E+18", "perlang.Double" },
            new object[] { "2147483647", "2.0", "4.61168601413242E+18", "perlang.Double" },
            new object[] { "4294967295", "2", "18446744065119617025", "perlang.BigInt" },
            new object[] { "4294967295", "340282349999999991754788743781432688640.0f", "Infinity", "perlang.Double" },
            new object[] { "4294967295", "12.0", "3.94020060863065E+115", "perlang.Double" },
            new object[] { "9223372036854775807", "2", "85070591730234615847396907784232501249", "perlang.BigInt" },
            new object[] { "9223372036854775807", "2.0f", "8.50705917302346E+37", "perlang.Double" },
            new object[] { "9223372036854775807", "2.0", "8.50705917302346E+37", "perlang.Double" },
            new object[] { "18446744073709551615", "2", "340282366920938463426481119284349108225", "perlang.BigInt" },
            new object[] { "18446744073709551615", "2.0f", "3.40282366920938E+38", "perlang.Double" },
            new object[] { "18446744073709551615", "2.0", "3.40282366920938E+38", "perlang.Double" },
            new object[] { "18446744073709551616", "2", "340282366920938463463374607431768211456", "perlang.BigInt" },
            new object[] { "340282349999999991754788743781432688640.0f", "2147483647", "Infinity", "perlang.Double" },
            new object[] { "340282349999999991754788743781432688640.0f", "4294967295", "Infinity", "perlang.Double" },
            new object[] { "340282349999999991754788743781432688640.0f", "9223372036854775807", "Infinity", "perlang.Double" },
            new object[] { "340282349999999991754788743781432688640.0f", "18446744073709551615", "Infinity", "perlang.Double" },
            new object[] { "340282349999999991754788743781432688640.0f", "340282349999999991754788743781432688640.0f", "Infinity", "perlang.Double" },
            new object[] { "340282349999999991754788743781432688640.0f", "12.0", "Infinity", "perlang.Double" },
            new object[] { "2.0", "4294967295", "Infinity", "perlang.Double" },
            new object[] { "2.0", "9223372036854775807", "Infinity", "perlang.Double" },
            new object[] { "12.0", "18446744073709551615", "Infinity", "perlang.Double" },
            new object[] { "12.0", "340282349999999991754788743781432688640.0f", "Infinity", "perlang.Double" },
        };

    public static IEnumerable<object[]> Exponential_unsupported_types =>
        new List<object[]>
        {
            new object[] { "2", "4294967295", "Unsupported ** operand types: 'int' and 'uint'" },
            new object[] { "2", "9223372036854775807", "Unsupported ** operand types: 'int' and 'long'" },
            new object[] { "2", "18446744073709551615", "Unsupported ** operand types: 'int' and 'ulong'" },
            new object[] { "2", "18446744073709551616", "Unsupported ** operand types: 'int' and 'bigint'" },
            new object[] { "4294967295", "4294967295", "Unsupported ** operand types: 'uint' and 'uint'" },
            new object[] { "4294967295", "9223372036854775807", "Unsupported ** operand types: 'uint' and 'long'" },
            new object[] { "4294967295", "18446744073709551615", "Unsupported ** operand types: 'uint' and 'ulong'" },
            new object[] { "4294967295", "18446744073709551616", "Unsupported ** operand types: 'uint' and 'bigint'" },
            new object[] { "9223372036854775807", "4294967295", "Unsupported ** operand types: 'long' and 'uint'" },
            new object[] { "9223372036854775807", "9223372036854775807", "Unsupported ** operand types: 'long' and 'long'" },
            new object[] { "9223372036854775807", "18446744073709551615", "Unsupported ** operand types: 'long' and 'ulong'" },
            new object[] { "9223372036854775807", "18446744073709551616", "Unsupported ** operand types: 'long' and 'bigint'" },
            new object[] { "18446744073709551615", "4294967295", "Unsupported ** operand types: 'ulong' and 'uint'" },
            new object[] { "18446744073709551615", "9223372036854775807", "Unsupported ** operand types: 'ulong' and 'long'" },
            new object[] { "18446744073709551615", "18446744073709551615", "Unsupported ** operand types: 'ulong' and 'ulong'" },
            new object[] { "18446744073709551615", "18446744073709551616", "Unsupported ** operand types: 'ulong' and 'bigint'" },
            new object[] { "18446744073709551616", "4294967295", "Unsupported ** operand types: 'bigint' and 'uint'" },
            new object[] { "18446744073709551616", "9223372036854775807", "Unsupported ** operand types: 'bigint' and 'long'" },
            new object[] { "18446744073709551616", "18446744073709551615", "Unsupported ** operand types: 'bigint' and 'ulong'" },
            new object[] { "18446744073709551616", "18446744073709551616", "Unsupported ** operand types: 'bigint' and 'bigint'" },
            new object[] { "18446744073709551616", "2.0f", "Unsupported ** operand types: 'bigint' and 'float'" },
            new object[] { "18446744073709551616", "2.0", "Unsupported ** operand types: 'bigint' and 'double'" },
            new object[] { "2.0", "18446744073709551616", "Unsupported ** operand types: 'double' and 'bigint'" },
            new object[] { "2.0f", "18446744073709551616", "Unsupported ** operand types: 'float' and 'bigint'" },
        };

    public static IEnumerable<object[]> Modulo_result =>
        Modulo_result_and_type.GetResult();

    public static IEnumerable<object[]> Modulo_type =>
        Modulo_result_and_type.GetTypes();

    private static IEnumerable<object[]> Modulo_result_and_type =>
        new List<object[]>
        {
            new object[] { "2", "4294967295", "2", "perlang.Int64" },
            new object[] { "5", "3", "2", "perlang.Int32" },
            new object[] { "2", "9223372036854775807", "2", "perlang.Int64" },
            new object[] { "2", "18446744073709551616", "2", "perlang.BigInt" },
            new object[] { "9", "2.0", "1", "perlang.Double" },
            new object[] { "2147483647", "2", "1", "perlang.Int32" },
            new object[] { "2147483647", "2.0f", "0", "perlang.Float" }, //  This mimics the semantics of C#; 2147483647 implicitly converted to float becomes 2147483648, because of IEEE-754
            new object[] { "2147483647", "2.0", "1", "perlang.Double" },
            new object[] { "-2147483648", "2", "0", "perlang.Int32" },
            new object[] { "4294967295", "2", "1", "perlang.Int64" },
            new object[] { "4294967295", "4294967295", "0", "perlang.UInt32" },
            new object[] { "4294967295", "9223372036854775807", "4294967295", "perlang.Int64" },
            new object[] { "4294967295", "18446744073709551615", "4294967295", "perlang.UInt64" },
            new object[] { "4294967295", "18446744073709551616", "4294967295", "perlang.BigInt" },
            new object[] { "4294967295", "12", "3", "perlang.Int64" }, // TODO: should be uint
            new object[] { "4294967295", "12.0f", "4", "perlang.Float" }, //  This mimics the semantics of C#; 4294967295 implicitly converted to float becomes 4294967296, because of IEEE-754
            new object[] { "4294967295", "12.0", "3", "perlang.Double" },
            new object[] { "9223372036854775807", "2", "1", "perlang.Int64" },
            new object[] { "9223372036854775807", "4294967295", "2147483647", "perlang.Int64" },
            new object[] { "9223372036854775807", "9223372036854775807", "0", "perlang.Int64" },
            new object[] { "9223372036854775807", "18446744073709551616", "9223372036854775807", "perlang.BigInt" },
            new object[] { "9223372036854775807", "12.0f", "8", "perlang.Float" },
            new object[] { "9223372036854775807", "12.0", "8", "perlang.Double" },
            new object[] { "18446744073709551615", "4294967295", "0", "perlang.UInt64" },
            new object[] { "18446744073709551615", "18446744073709551615", "0", "perlang.UInt64" },
            new object[] { "18446744073709551615", "18446744073709551616", "18446744073709551615", "perlang.BigInt" },
            new object[] { "18446744073709551615", "12.0f", "4", "perlang.Float" },
            new object[] { "18446744073709551615", "12.0", "4", "perlang.Double" },
            new object[] { "18446744073709551616", "3", "1", "perlang.BigInt" },
            new object[] { "18446744073709551616", "4294967295", "1", "perlang.BigInt" },
            new object[] { "18446744073709551616", "9223372036854775807", "2", "perlang.BigInt" },
            new object[] { "18446744073709551616", "18446744073709551615", "1", "perlang.BigInt" },
            new object[] { "18446744073709551616", "18446744073709551616", "0", "perlang.BigInt" },
            new object[] { "9.0f", "2147483647", "9", "perlang.Float" },
            new object[] { "9.0f", "4294967295", "9", "perlang.Float" },
            new object[] { "9.0f", "9223372036854775807", "9", "perlang.Float" },
            new object[] { "9.0f", "18446744073709551615", "9", "perlang.Float" },
            new object[] { "9.0f", "2.0f", "1", "perlang.Float" },
            new object[] { "9.0f", "2.0", "1", "perlang.Double" },
            new object[] { "12.0", "2", "0", "perlang.Double" },
            new object[] { "12.0", "4294967295", "12", "perlang.Double" },
            new object[] { "12.0", "9223372036854775807", "12", "perlang.Double" },
            new object[] { "12.0", "18446744073709551615", "12", "perlang.Double" },
            new object[] { "9.0", "2.0f", "1", "perlang.Double" },
            new object[] { "9.0", "2.0", "1", "perlang.Double" },
        };

    public static IEnumerable<object[]> Modulo_unsupported_types =>
        new List<object[]>
        {
            new object[] { "2", "18446744073709551615", "Unsupported * operand types: 'int' and 'ulong'" },
            new object[] { "9223372036854775807", "18446744073709551615", "Unsupported * operand types: 'long' and 'ulong'" },
            new object[] { "18446744073709551615", "2", "Unsupported * operand types: 'ulong' and 'int'" },
            new object[] { "18446744073709551615", "9223372036854775807", "Unsupported * operand types: 'ulong' and 'long'" },
            new object[] { "18446744073709551616", "2.0f", "Unsupported * operand types: 'bigint' and 'float'" },
            new object[] { "18446744073709551616", "2.0", "Unsupported * operand types: 'bigint' and 'double'" },
            new object[] { "2.0f", "18446744073709551616", "Unsupported * operand types: 'float' and 'bigint'" },
            new object[] { "2.0", "18446744073709551616", "Unsupported * operand types: 'double' and 'bigint'" },
        };

    public static IEnumerable<object[]> ShiftLeft_result =>
        ShiftLeft_result_and_type.GetResult();

    public static IEnumerable<object[]> ShiftLeft_type =>
        ShiftLeft_result_and_type.GetTypes();

    private static IEnumerable<object[]> ShiftLeft_result_and_type =>
        new List<object[]>
        {
            new object[] { "1", "10", "1024", "perlang.Int32" },
            new object[] { "1073741824", "1", "-2147483648", "perlang.Int32" }, // Signed wraparound => becomes negative
            new object[] { "4294967295", "2", "4294967292", "perlang.UInt32" },
            new object[] { "9223372036854775807", "2", "-4", "perlang.Int64" }, // Signed wraparound => becomes negative
            new object[] { "18446744073709551615", "2", "18446744073709551612", "perlang.UInt64" }, // Unsigned integer wraparound
            new object[] { "18446744073709551616", "2", "73786976294838206464", "perlang.BigInt" },
        };

    public static IEnumerable<object[]> ShiftLeft_unsupported_types =>
        new List<object[]>
        {
            new object[] { "2147483647", "4294967295", "Unsupported << operand types: 'int' and 'uint'" },
            new object[] { "2147483647", "9223372036854775807", "Unsupported << operand types: 'int' and 'long'" },
            new object[] { "2147483647", "18446744073709551615", "Unsupported << operand types: 'int' and 'ulong'" },
            new object[] { "2147483647", "18446744073709551616", "Unsupported << operand types: 'int' and 'bigint'" },
            new object[] { "2147483647", "2.0f", "Unsupported << operand types: 'int' and 'float'" },
            new object[] { "2147483647", "2.0", "Unsupported << operand types: 'int' and 'double'" },
            new object[] { "4294967295", "4294967295", "Unsupported << operand types: 'uint' and 'uint'" },
            new object[] { "4294967295", "9223372036854775807", "Unsupported << operand types: 'uint' and 'long'" },
            new object[] { "4294967295", "18446744073709551615", "Unsupported << operand types: 'uint' and 'ulong'" },
            new object[] { "4294967295", "18446744073709551616", "Unsupported << operand types: 'uint' and 'bigint'" },
            new object[] { "4294967295", "2.0f", "Unsupported << operand types: 'uint' and 'float'" },
            new object[] { "4294967295", "2.0", "Unsupported << operand types: 'uint' and 'double'" },
            new object[] { "9223372036854775807", "4294967295", "Unsupported << operand types: 'long' and 'uint'" },
            new object[] { "9223372036854775807", "9223372036854775807", "Unsupported << operand types: 'long' and 'long'" },
            new object[] { "9223372036854775807", "18446744073709551615", "Unsupported << operand types: 'long' and 'ulong'" },
            new object[] { "9223372036854775807", "18446744073709551616", "Unsupported << operand types: 'long' and 'bigint'" },
            new object[] { "9223372036854775807", "2.0f", "Unsupported << operand types: 'long' and 'float'" },
            new object[] { "9223372036854775807", "2.0", "Unsupported << operand types: 'long' and 'double'" },
            new object[] { "18446744073709551615", "4294967295", "Unsupported << operand types: 'ulong' and 'uint'" },
            new object[] { "18446744073709551615", "9223372036854775807", "Unsupported << operand types: 'ulong' and 'long'" },
            new object[] { "18446744073709551615", "18446744073709551615", "Unsupported << operand types: 'ulong' and 'ulong'" },
            new object[] { "18446744073709551615", "18446744073709551616", "Unsupported << operand types: 'ulong' and 'bigint'" },
            new object[] { "18446744073709551615", "2.0f", "Unsupported << operand types: 'ulong' and 'float'" },
            new object[] { "18446744073709551615", "2.0", "Unsupported << operand types: 'ulong' and 'double'" },
            new object[] { "18446744073709551616", "4294967295", "Unsupported << operand types: 'bigint' and 'uint'" },
            new object[] { "18446744073709551616", "9223372036854775807", "Unsupported << operand types: 'bigint' and 'long'" },
            new object[] { "18446744073709551616", "18446744073709551615", "Unsupported << operand types: 'bigint' and 'ulong'" },
            new object[] { "18446744073709551616", "18446744073709551616", "Unsupported << operand types: 'bigint' and 'bigint'" },
            new object[] { "18446744073709551616", "2.0f", "Unsupported << operand types: 'bigint' and 'float'" },
            new object[] { "18446744073709551616", "2.0", "Unsupported << operand types: 'bigint' and 'double'" },
            new object[] { "1.0f", "10", "Unsupported << operand types: 'float' and 'int'" },
            new object[] { "1.0f", "4294967295", "Unsupported << operand types: 'float' and 'uint'" },
            new object[] { "1.0f", "9223372036854775807", "Unsupported << operand types: 'float' and 'long'" },
            new object[] { "1.0f", "18446744073709551615", "Unsupported << operand types: 'float' and 'ulong'" },
            new object[] { "1.0f", "18446744073709551616", "Unsupported << operand types: 'float' and 'bigint'" },
            new object[] { "1.0f", "10.0f", "Unsupported << operand types: 'float' and 'float'" },
            new object[] { "1.0f", "10.0", "Unsupported << operand types: 'float' and 'double'" },
            new object[] { "1.0", "10", "Unsupported << operand types: 'double' and 'int'" },
            new object[] { "1.0", "4294967295", "Unsupported << operand types: 'double' and 'uint'" },
            new object[] { "1.0", "9223372036854775807", "Unsupported << operand types: 'double' and 'long'" },
            new object[] { "1.0", "18446744073709551615", "Unsupported << operand types: 'double' and 'ulong'" },
            new object[] { "1.0", "18446744073709551616", "Unsupported << operand types: 'double' and 'bigint'" },
            new object[] { "1.0", "10.0f", "Unsupported << operand types: 'double' and 'float'" },
            new object[] { "1.0", "10.0", "Unsupported << operand types: 'double' and 'double'" },
        };

    public static IEnumerable<object[]> ShiftRight_result =>
        ShiftRight_result_and_type.GetResult();

    public static IEnumerable<object[]> ShiftRight_type =>
        ShiftRight_result_and_type.GetTypes();

    private static IEnumerable<object[]> ShiftRight_result_and_type =>
        new List<object[]>
        {
            new object[] { "2147483647", "32", "2147483647", "perlang.Int32" }, // Wraparound, handled by manual logic in PerlangCompiler
            new object[] { "4294967295", "2", "1073741823", "perlang.UInt32" },
            new object[] { "9223372036854775807", "2", "2305843009213693951", "perlang.Int64" },
            new object[] { "18446744073709551615", "2", "4611686018427387903", "perlang.UInt64" },
            new object[] { "18446744073709551616", "2", "4611686018427387904", "perlang.BigInt" },
        };

    public static IEnumerable<object[]> ShiftRight_unsupported_types =>
        new List<object[]>
        {
            new object[] { "2147483647", "4294967295", "Unsupported >> operand types: 'int' and 'uint'" },
            new object[] { "2147483647", "9223372036854775807", "Unsupported >> operand types: 'int' and 'long'" },
            new object[] { "2147483647", "18446744073709551615", "Unsupported >> operand types: 'int' and 'ulong'" },
            new object[] { "2147483647", "18446744073709551616", "Unsupported >> operand types: 'int' and 'bigint'" },
            new object[] { "2147483647", "2.0f", "Unsupported >> operand types: 'int' and 'float'" },
            new object[] { "2147483647", "2.0", "Unsupported >> operand types: 'int' and 'double'" },
            new object[] { "4294967295", "4294967295", "Unsupported >> operand types: 'uint' and 'uint'" },
            new object[] { "4294967295", "9223372036854775807", "Unsupported >> operand types: 'uint' and 'long'" },
            new object[] { "4294967295", "18446744073709551615", "Unsupported >> operand types: 'uint' and 'ulong'" },
            new object[] { "4294967295", "18446744073709551616", "Unsupported >> operand types: 'uint' and 'bigint'" },
            new object[] { "4294967295", "2.0f", "Unsupported >> operand types: 'uint' and 'float'" },
            new object[] { "4294967295", "2.0", "Unsupported >> operand types: 'uint' and 'double'" },
            new object[] { "9223372036854775807", "4294967295", "Unsupported >> operand types: 'long' and 'uint'" },
            new object[] { "9223372036854775807", "9223372036854775807", "Unsupported >> operand types: 'long' and 'long'" },
            new object[] { "9223372036854775807", "18446744073709551615", "Unsupported >> operand types: 'long' and 'ulong'" },
            new object[] { "9223372036854775807", "18446744073709551616", "Unsupported >> operand types: 'long' and 'bigint'" },
            new object[] { "9223372036854775807", "2.0f", "Unsupported >> operand types: 'long' and 'float'" },
            new object[] { "9223372036854775807", "2.0", "Unsupported >> operand types: 'long' and 'double'" },
            new object[] { "18446744073709551615", "4294967295", "Unsupported >> operand types: 'ulong' and 'uint'" },
            new object[] { "18446744073709551615", "9223372036854775807", "Unsupported >> operand types: 'ulong' and 'long'" },
            new object[] { "18446744073709551615", "18446744073709551615", "Unsupported >> operand types: 'ulong' and 'ulong'" },
            new object[] { "18446744073709551615", "18446744073709551616", "Unsupported >> operand types: 'ulong' and 'bigint'" },
            new object[] { "18446744073709551615", "2.0f", "Unsupported >> operand types: 'ulong' and 'float'" },
            new object[] { "18446744073709551615", "2.0", "Unsupported >> operand types: 'ulong' and 'double'" },
            new object[] { "18446744073709551616", "4294967295", "Unsupported >> operand types: 'bigint' and 'uint'" },
            new object[] { "18446744073709551616", "9223372036854775807", "Unsupported >> operand types: 'bigint' and 'long'" },
            new object[] { "18446744073709551616", "18446744073709551615", "Unsupported >> operand types: 'bigint' and 'ulong'" },
            new object[] { "18446744073709551616", "18446744073709551616", "Unsupported >> operand types: 'bigint' and 'bigint'" },
            new object[] { "18446744073709551616", "2.0f", "Unsupported >> operand types: 'bigint' and 'float'" },
            new object[] { "18446744073709551616", "2.0", "Unsupported >> operand types: 'bigint' and 'double'" },
            new object[] { "2.0f", "2", "Unsupported >> operand types: 'float' and 'int'" },
            new object[] { "2.0f", "4294967295", "Unsupported >> operand types: 'float' and 'uint'" },
            new object[] { "2.0f", "9223372036854775807", "Unsupported >> operand types: 'float' and 'long'" },
            new object[] { "2.0f", "18446744073709551615", "Unsupported >> operand types: 'float' and 'ulong'" },
            new object[] { "2.0f", "18446744073709551616", "Unsupported >> operand types: 'float' and 'bigint'" },
            new object[] { "2.0f", "2.0f", "Unsupported >> operand types: 'float' and 'float'" },
            new object[] { "2.0f", "2.0", "Unsupported >> operand types: 'float' and 'double'" },
            new object[] { "2.0", "2", "Unsupported >> operand types: 'double' and 'int'" },
            new object[] { "2.0", "4294967295", "Unsupported >> operand types: 'double' and 'uint'" },
            new object[] { "2.0", "9223372036854775807", "Unsupported >> operand types: 'double' and 'long'" },
            new object[] { "2.0", "18446744073709551615", "Unsupported >> operand types: 'double' and 'ulong'" },
            new object[] { "2.0", "18446744073709551616", "Unsupported >> operand types: 'double' and 'bigint'" },
            new object[] { "2.0", "2.0f", "Unsupported >> operand types: 'double' and 'float'" },
            new object[] { "2.0", "2.0", "Unsupported >> operand types: 'double' and 'double'" },
        };
}

internal static class IEnumerableObjectExtensionMethods
{
    internal static IEnumerable<object[]> GetResult(this IEnumerable<object[]> resultAndTypes) =>
        resultAndTypes.Select(obj => obj[..3]);

    internal static IEnumerable<object[]> GetTypes(this IEnumerable<object[]> resultAndTypes) =>
        resultAndTypes.Select(obj => new[] { obj[0], obj[1], obj[3] });
}
