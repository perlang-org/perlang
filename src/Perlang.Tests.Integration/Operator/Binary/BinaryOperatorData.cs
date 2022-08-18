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
            new object[] { "-12", "34.0", "False" },
            new object[] { "2147483647", "4294967295", "False" },
            new object[] { "2147483647", "18446744073709551616", "False" },
            new object[] { "2147483647", "9223372036854775807", "False" },
            new object[] { "2147483646", "2147483647", "False" },
            new object[] { "2147483647", "2147483646", "True" },
            new object[] { "2147483647", "2147483647", "False" },
            new object[] { "2147483647", "340282349999999991754788743781432688640.0f", "False" },
            new object[] { "2147483647", "12.0", "True" },
            new object[] { "4294967296", "9223372036854775807", "False" },
            new object[] { "4294967295", "33", "True" },
            new object[] { "4294967295", "4294967295", "False" },
            new object[] { "4294967295", "9223372036854775807", "False" },
            new object[] { "4294967295", "18446744073709551615", "False" },
            new object[] { "4294967295", "18446744073709551616", "False" },
            new object[] { "4294967295", "12.0", "True" },
            new object[] { "4294967295", "340282349999999991754788743781432688640.0f", "False" },
            new object[] { "9223372036854775807", "2", "True" },
            new object[] { "9223372036854775807", "4294967295", "True" },
            new object[] { "9223372036854775807", "9223372036854775807", "False" },
            new object[] { "9223372036854775807", "18446744073709551616", "False" },
            new object[] { "9223372036854775807", "340282349999999991754788743781432688640.0f", "False" },
            new object[] { "9223372036854775807", "12.0", "True" },
            new object[] { "18446744073709551615", "4294967295", "True" },
            new object[] { "18446744073709551615", "18446744073709551615", "False" },
            new object[] { "18446744073709551615", "18446744073709551616", "False" },
            new object[] { "18446744073709551615", "340282349999999991754788743781432688640.0f", "False" },
            new object[] { "18446744073709551615", "12.0", "True" },
            new object[] { "18446744073709551616", "2", "True" },
            new object[] { "18446744073709551616", "4294967295", "True" },
            new object[] { "18446744073709551616", "9223372036854775807", "True" },
            new object[] { "18446744073709551616", "18446744073709551615", "True" },
            new object[] { "18446744073709551616", "18446744073709551616", "False" },
            new object[] { "340282349999999991754788743781432688640.0f", "2147483647", "True" },
            new object[] { "340282349999999991754788743781432688640.0f", "4294967295", "True" },
            new object[] { "340282349999999991754788743781432688640.0f", "9223372036854775807", "True" },
            new object[] { "340282349999999991754788743781432688640.0f", "18446744073709551615", "True" },
            new object[] { "340282349999999991754788743781432688640.0f", "340282349999999991754788743781432688640.0f", "False" },
            new object[] { "340282349999999991754788743781432688640.0f", "12.0", "True" },
            new object[] { "12.0", "2147483647", "False" },
            new object[] { "12.0", "4294967295", "False" },
            new object[] { "12.0", "9223372036854775807", "False" },
            new object[] { "12.0", "18446744073709551615", "False" },
            new object[] { "12.0", "340282349999999991754788743781432688640.0f", "False" },
            new object[] { "12.0", "34.0", "False" },
            new object[] { "34.0", "33.0", "True" },
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
            new object[] { "2147483647", "4294967295", "False" },
            new object[] { "2147483647", "18446744073709551616", "False" },
            new object[] { "2147483647", "9223372036854775807", "False" },
            new object[] { "-2147483648", "34.0", "False" },
            new object[] { "2147483646", "2147483647", "False" },
            new object[] { "2147483647", "2147483647", "True" },
            new object[] { "2147483647", "2147483646", "True" },
            new object[] { "2147483647", "340282349999999991754788743781432688640.0f", "False" },
            new object[] { "2147483647", "33.0", "True" },
            new object[] { "4294967295", "33", "True" },
            new object[] { "4294967295", "4294967295", "True" },
            new object[] { "4294967295", "9223372036854775807", "False" },
            new object[] { "4294967295", "18446744073709551615", "False" },
            new object[] { "4294967295", "18446744073709551616", "False" },
            new object[] { "4294967295", "340282349999999991754788743781432688640.0f", "False" },
            new object[] { "4294967295", "12.0", "True" },
            new object[] { "9223372036854775807", "2", "True" },
            new object[] { "9223372036854775807", "4294967295", "True" },
            new object[] { "9223372036854775807", "9223372036854775807", "True" },
            new object[] { "9223372036854775807", "18446744073709551616", "False" },
            new object[] { "9223372036854775807", "340282349999999991754788743781432688640.0f", "False" },
            new object[] { "9223372036854775807", "12.0", "True" },
            new object[] { "18446744073709551615", "4294967295", "True" },
            new object[] { "18446744073709551615", "18446744073709551615", "True" },
            new object[] { "18446744073709551615", "18446744073709551616", "False" },
            new object[] { "18446744073709551615", "340282349999999991754788743781432688640.0f", "False" },
            new object[] { "18446744073709551615", "12.0", "True" },
            new object[] { "18446744073709551616", "2", "True" },
            new object[] { "18446744073709551616", "4294967295", "True" },
            new object[] { "18446744073709551616", "9223372036854775807", "True" },
            new object[] { "18446744073709551616", "18446744073709551615", "True" },
            new object[] { "18446744073709551616", "18446744073709551616", "True" },
            new object[] { "340282349999999991754788743781432688640.0f", "2147483647", "True" },
            new object[] { "340282349999999991754788743781432688640.0f", "4294967295", "True" },
            new object[] { "340282349999999991754788743781432688640.0f", "9223372036854775807", "True" },
            new object[] { "340282349999999991754788743781432688640.0f", "18446744073709551615", "True" },
            new object[] { "340282349999999991754788743781432688640.0f", "340282349999999991754788743781432688640.0f", "True" },
            new object[] { "340282349999999991754788743781432688640.0f", "12.0", "True" },
            new object[] { "12.0", "34", "False" },
            new object[] { "12.0", "4294967295", "False" },
            new object[] { "12.0", "9223372036854775807", "False" },
            new object[] { "12.0", "18446744073709551615", "False" },
            new object[] { "12.0", "34.0", "False" },
            new object[] { "34.0", "33.0", "True" },
            new object[] { "12.0", "340282349999999991754788743781432688640.0f", "False" },
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
            new object[] { "2", "34", "True" },
            new object[] { "2", "4294967295", "True" },
            new object[] { "2", "9223372036854775807", "True" },
            new object[] { "2", "18446744073709551616", "True" },
            new object[] { "2", "-34", "False" },
            new object[] { "2", "34.0", "True" },
            new object[] { "-12", "34", "True" },
            new object[] { "-12", "-34", "False" },
            new object[] { "-12", "-34.0", "False" },
            new object[] { "2147483647", "33.0", "False" },
            new object[] { "2147483647", "340282349999999991754788743781432688640.0f", "True" },
            new object[] { "4294967295", "33", "False" },
            new object[] { "4294967295", "4294967295", "False" },
            new object[] { "4294967295", "9223372036854775807", "True" },
            new object[] { "4294967295", "18446744073709551615", "True" },
            new object[] { "4294967295", "340282349999999991754788743781432688640.0f", "True" },
            new object[] { "4294967295", "18446744073709551616", "True" },
            new object[] { "4294967295", "12.0", "False" },
            new object[] { "9223372036854775807", "2", "False" },
            new object[] { "9223372036854775807", "4294967295", "False" },
            new object[] { "9223372036854775807", "9223372036854775807", "False" },
            new object[] { "9223372036854775807", "18446744073709551616", "True" },
            new object[] { "9223372036854775807", "340282349999999991754788743781432688640.0f", "True" },
            new object[] { "9223372036854775807", "12.0", "False" },
            new object[] { "18446744073709551615", "4294967295", "False" },
            new object[] { "18446744073709551615", "18446744073709551615", "False" },
            new object[] { "18446744073709551615", "18446744073709551616", "True" },
            new object[] { "18446744073709551615", "340282349999999991754788743781432688640.0f", "True" },
            new object[] { "18446744073709551615", "12.0", "False" },
            new object[] { "18446744073709551616", "2", "False" },
            new object[] { "18446744073709551616", "4294967295", "False" },
            new object[] { "18446744073709551616", "9223372036854775807", "False" },
            new object[] { "18446744073709551616", "18446744073709551615", "False" },
            new object[] { "18446744073709551616", "18446744073709551616", "False" },
            new object[] { "18446744073709551616", "18446744073709551617", "True" },
            new object[] { "12.0", "-34", "False" },
            new object[] { "-12.0", "-34", "False" },
            new object[] { "12.0", "4294967295", "True" },
            new object[] { "12.0", "9223372036854775807", "True" },
            new object[] { "-12.0", "9223372036854775807", "True" },
            new object[] { "12.0", "18446744073709551615", "True" },
            new object[] { "12.0", "340282349999999991754788743781432688640.0f", "True" },
            new object[] { "340282349999999991754788743781432688640.0f", "2147483647", "False" },
            new object[] { "340282349999999991754788743781432688640.0f", "4294967295", "False" },
            new object[] { "340282349999999991754788743781432688640.0f", "9223372036854775807", "False" },
            new object[] { "340282349999999991754788743781432688640.0f", "18446744073709551615", "False" },
            new object[] { "340282349999999991754788743781432688640.0f", "340282349999999991754788743781432688640.0f", "False" },
            new object[] { "340282349999999991754788743781432688640.0f", "12.0", "False" },
            new object[] { "34.0", "33.0", "False" },
            new object[] { "12.0", "34.0", "True" },
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
            new object[] { "-2147483648", "4294967295", "True" },
            new object[] { "2147483646", "2147483647", "True" },
            new object[] { "2147483647", "4294967295", "True" },
            new object[] { "2147483647", "18446744073709551616", "True" },
            new object[] { "2147483647", "9223372036854775807", "True" },
            new object[] { "2147483647", "2147483647", "True" },
            new object[] { "2147483647", "2147483646", "False" },
            new object[] { "2147483647", "33.0", "False" },
            new object[] { "2147483647", "340282349999999991754788743781432688640.0f", "True" },
            new object[] { "4294967295", "33", "False" },
            new object[] { "4294967295", "4294967295", "True" },
            new object[] { "4294967295", "9223372036854775807", "True" },
            new object[] { "4294967295", "18446744073709551615", "True" },
            new object[] { "4294967295", "18446744073709551616", "True" },
            new object[] { "4294967295", "340282349999999991754788743781432688640.0f", "True" },
            new object[] { "4294967295", "12.0", "False" },
            new object[] { "9223372036854775807", "2", "False" },
            new object[] { "9223372036854775807", "4294967295", "False" },
            new object[] { "9223372036854775807", "9223372036854775807", "True" },
            new object[] { "9223372036854775807", "18446744073709551616", "True" },
            new object[] { "9223372036854775807", "340282349999999991754788743781432688640.0f", "True" },
            new object[] { "9223372036854775807", "12.0", "False" },
            new object[] { "18446744073709551615", "4294967295", "False" },
            new object[] { "18446744073709551615", "18446744073709551615", "True" },
            new object[] { "18446744073709551615", "18446744073709551616", "True" },
            new object[] { "18446744073709551615", "340282349999999991754788743781432688640.0f", "True" },
            new object[] { "18446744073709551615", "12.0", "False" },
            new object[] { "18446744073709551616", "2", "False" },
            new object[] { "18446744073709551616", "4294967295", "False" },
            new object[] { "18446744073709551616", "9223372036854775807", "False" },
            new object[] { "18446744073709551616", "18446744073709551615", "False" },
            new object[] { "18446744073709551616", "18446744073709551616", "True" },
            new object[] { "340282349999999991754788743781432688640.0f", "2147483647", "False" },
            new object[] { "340282349999999991754788743781432688640.0f", "4294967295", "False" },
            new object[] { "340282349999999991754788743781432688640.0f", "9223372036854775807", "False" },
            new object[] { "340282349999999991754788743781432688640.0f", "18446744073709551615", "False" },
            new object[] { "340282349999999991754788743781432688640.0f", "340282349999999991754788743781432688640.0f", "True" },
            new object[] { "340282349999999991754788743781432688640.0f", "34.0", "False" },
            new object[] { "12.0", "34.0", "True" },
            new object[] { "12.0", "-34", "False" },
            new object[] { "12.0", "4294967295", "True" },
            new object[] { "12.0", "9223372036854775807", "True" },
            new object[] { "12.0", "18446744073709551615", "True" },
            new object[] { "12.0", "34.0", "True" },
            new object[] { "34.0", "33.0", "False" },
            new object[] { "34.0", "340282349999999991754788743781432688640.0f", "True" },
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
            new object[] { "12", "34", "True" },
            new object[] { "12", "12", "False" },
            new object[] { "12", "12.0", "True" }, // Same value but different types. Note: this is truthy in C# AND Java.
            new object[] { "12.0", "12", "True" }, // Same value but different types. Note: this is truthy in C# AND Java.

            new object[] { "2147483647", "4294967295", "True" },
            new object[] { "2147483647", "9223372036854775807", "True" },
            new object[] { "2147483647", "18446744073709551615", "True" },
            new object[] { "2147483647", "18446744073709551616", "True" },
            new object[] { "2147483647", "340282349999999991754788743781432688640.0f", "True" },
            new object[] { "4294967295", "33", "True" },
            new object[] { "4294967295", "4294967295", "False" },
            new object[] { "4294967295", "9223372036854775807", "True" },
            new object[] { "4294967295", "18446744073709551615", "True" },
            new object[] { "4294967295", "18446744073709551616", "True" },
            new object[] { "4294967295", "340282349999999991754788743781432688640.0f", "True" },
            new object[] { "4294967295", "12.0", "True" },
            new object[] { "9223372036854775807", "2", "True" },
            new object[] { "9223372036854775807", "4294967295", "True" },
            new object[] { "9223372036854775807", "9223372036854775807", "False" },
            new object[] { "9223372036854775807", "18446744073709551615", "True" },
            new object[] { "9223372036854775807", "18446744073709551616", "True" },
            new object[] { "9223372036854775807", "340282349999999991754788743781432688640.0f", "True" },
            new object[] { "9223372036854775807", "12.0", "True" },
            new object[] { "18446744073709551615", "2", "True" },
            new object[] { "18446744073709551615", "4294967295", "True" },
            new object[] { "18446744073709551615", "9223372036854775807", "True" },
            new object[] { "18446744073709551615", "18446744073709551615", "False" },
            new object[] { "18446744073709551615", "18446744073709551616", "True" },
            new object[] { "18446744073709551615", "340282349999999991754788743781432688640.0f", "True" },
            new object[] { "18446744073709551615", "12.0", "True" },
            new object[] { "18446744073709551616", "2", "True" },
            new object[] { "18446744073709551616", "4294967295", "True" },
            new object[] { "18446744073709551616", "9223372036854775807", "True" },
            new object[] { "18446744073709551616", "18446744073709551615", "True" },
            new object[] { "18446744073709551616", "18446744073709551616", "False" },
            new object[] { "18446744073709551616", "340282349999999991754788743781432688640.0f", "True" },
            new object[] { "18446744073709551616", "12.0", "True" },
            new object[] { "340282349999999991754788743781432688640.0f", "2147483647", "True" },
            new object[] { "340282349999999991754788743781432688640.0f", "4294967295", "True" },
            new object[] { "340282349999999991754788743781432688640.0f", "9223372036854775807", "True" },
            new object[] { "340282349999999991754788743781432688640.0f", "18446744073709551615", "True" },
            new object[] { "340282349999999991754788743781432688640.0f", "18446744073709551616", "True" },
            new object[] { "340282349999999991754788743781432688640.0f", "340282349999999991754788743781432688640.0f", "False" },
            new object[] { "340282349999999991754788743781432688640.0f", "12.0", "True" },
            new object[] { "-12.0", "4294967295", "True" },
            new object[] { "-12.0", "9223372036854775807", "True" },
            new object[] { "-12.0", "18446744073709551615", "True" },
            new object[] { "-12.0", "18446744073709551616", "True" },
            new object[] { "-12.0", "340282349999999991754788743781432688640.0f", "True" },
            new object[] { "12.345", "12.345", "False" },
            new object[] { "12.345", "67.890", "True" },
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

            new object[] { "2147483647", "4294967295", "False" },
            new object[] { "2147483647", "9223372036854775807", "False" },
            new object[] { "2147483647", "18446744073709551615", "False" },
            new object[] { "2147483647", "18446744073709551616", "False" },
            new object[] { "2147483647", "340282349999999991754788743781432688640.0f", "False" },
            new object[] { "4294967295", "33", "False" },
            new object[] { "4294967295", "4294967295", "True" },
            new object[] { "4294967295", "9223372036854775807", "False" },
            new object[] { "4294967295", "18446744073709551615", "False" },
            new object[] { "4294967295", "18446744073709551616", "False" },
            new object[] { "4294967295", "340282349999999991754788743781432688640.0f", "False" },
            new object[] { "4294967295", "12.0", "False" },
            new object[] { "9223372036854775807", "2147483647", "False" },
            new object[] { "9223372036854775807", "4294967295", "False" },
            new object[] { "9223372036854775807", "9223372036854775807", "True" },
            new object[] { "9223372036854775807", "18446744073709551615", "False" },
            new object[] { "9223372036854775807", "18446744073709551616", "False" },
            new object[] { "9223372036854775807", "340282349999999991754788743781432688640.0f", "False" },
            new object[] { "9223372036854775807", "12.0", "False" },
            new object[] { "18446744073709551615", "2147483647", "False" },
            new object[] { "18446744073709551615", "4294967295", "False" },
            new object[] { "18446744073709551615", "9223372036854775807", "False" },
            new object[] { "18446744073709551615", "18446744073709551615", "True" },
            new object[] { "18446744073709551615", "18446744073709551616", "False" },
            new object[] { "18446744073709551615", "340282349999999991754788743781432688640.0f", "False" },
            new object[] { "18446744073709551615", "12.0", "False" },
            new object[] { "18446744073709551616", "2147483647", "False" },
            new object[] { "18446744073709551616", "4294967295", "False" },
            new object[] { "18446744073709551616", "9223372036854775807", "False" },
            new object[] { "18446744073709551616", "18446744073709551615", "False" },
            new object[] { "18446744073709551616", "18446744073709551616", "True" },
            new object[] { "18446744073709551616", "340282349999999991754788743781432688640.0f", "False" },
            new object[] { "18446744073709551616", "12.0", "False" },
            new object[] { "340282349999999991754788743781432688640.0f", "2147483647", "False" },
            new object[] { "340282349999999991754788743781432688640.0f", "4294967295", "False" },
            new object[] { "340282349999999991754788743781432688640.0f", "9223372036854775807", "False" },
            new object[] { "340282349999999991754788743781432688640.0f", "18446744073709551615", "False" },
            new object[] { "340282349999999991754788743781432688640.0f", "18446744073709551616", "False" },
            new object[] { "340282349999999991754788743781432688640.0f", "340282349999999991754788743781432688640.0f", "True" },
            new object[] { "340282349999999991754788743781432688640.0f", "12.0", "False" },
            new object[] { "-12.0", "4294967295", "False" },
            new object[] { "-12.0", "9223372036854775807", "False" },
            new object[] { "-12.0", "18446744073709551615", "False" },
            new object[] { "-12.0", "18446744073709551616", "False" },
            new object[] { "-12.0", "340282349999999991754788743781432688640.0f", "False" },
        };

    public static IEnumerable<object[]> Subtraction_result =>
        Subtraction_result_and_type.GetResult();

    public static IEnumerable<object[]> Subtraction_type =>
        Subtraction_result_and_type.GetTypes();

    private static IEnumerable<object[]> Subtraction_result_and_type =>
        new List<object[]>
        {
            new object[] { "2", "34", "-32", typeof(int) },
            new object[] { "2", "-34", "36", typeof(int) },
            new object[] { "2", "34", "-32", typeof(int) },
            new object[] { "2", "4294967295", "-4294967293", typeof(long) },
            new object[] { "2", "9223372036854775807", "-9223372036854775805", typeof(long) },
            new object[] { "2", "18446744073709551616", "-18446744073709551614", typeof(BigInteger) },

            new object[] { "2147483647", "2.0f", "2.1474836E+09", typeof(float) },
            new object[] { "2147483647", "2.0", "2147483645", typeof(double) },
            new object[] { "4294967295", "12", "4294967283", typeof(long) },
            new object[] { "4294967295", "4294967295", "0", typeof(uint) },
            new object[] { "4294967295", "9223372036854775807", "-9223372032559808512", typeof(long) }, // Negative integer overflow
            new object[] { "4294967295", "18446744073709551615", "4294967296", typeof(ulong) }, // Positive integer overflow
            new object[] { "4294967295", "18446744073709551616", "-18446744069414584321", typeof(BigInteger) },
            new object[] { "4294967295", "2.0f", "4.2949673E+09", typeof(float) },
            new object[] { "4294967295", "2.0", "4294967293", typeof(double) },
            new object[] { "9223372036854775807", "2", "9223372036854775805", typeof(long) },
            new object[] { "9223372036854775807", "4294967295", "9223372032559808512", typeof(long) },
            new object[] { "9223372036854775807", "9223372036854775807", "0", typeof(long) },
            new object[] { "9223372036854775807", "18446744073709551616", "-9223372036854775809", typeof(BigInteger) },
            new object[] { "9223372036854775807", "2.0f", "9.223372E+18", typeof(float) },
            new object[] { "9223372036854775807", "2.0", "9.223372036854776E+18", typeof(double) },
            new object[] { "18446744073709551615", "4294967295", "18446744069414584320", typeof(ulong) },
            new object[] { "18446744073709551615", "18446744073709551615", "0", typeof(ulong) },
            new object[] { "18446744073709551615", "18446744073709551616", "-1", typeof(BigInteger) },
            new object[] { "18446744073709551615", "2.0f", "1.8446744E+19", typeof(float) },
            new object[] { "18446744073709551615", "2.0", "1.8446744073709552E+19", typeof(double) },
            new object[] { "18446744073709551616", "2", "18446744073709551614", typeof(BigInteger) },
            new object[] { "18446744073709551616", "4294967295", "18446744069414584321", typeof(BigInteger) },
            new object[] { "18446744073709551616", "9223372036854775807", "9223372036854775809", typeof(BigInteger) },
            new object[] { "18446744073709551616", "18446744073709551615", "1", typeof(BigInteger) },
            new object[] { "18446744073709551616", "18446744073709551617", "-1", typeof(BigInteger) },
            new object[] { "2.0f", "2147483647", "-2.1474836E+09", typeof(float) },
            new object[] { "2.0f", "4294967295", "-4.2949673E+09", typeof(float) },
            new object[] { "2.0f", "9223372036854775807", "-9.223372E+18", typeof(float) },
            new object[] { "2.0f", "18446744073709551615", "-1.8446744E+19", typeof(float) },
            new object[] { "2.0f", "2.0f", "0", typeof(float) },
            new object[] { "2.0f", "2.0", "0", typeof(double) },
            new object[] { "-12.0", "-34", "22", typeof(double) },
            new object[] { "-12.0", "4294967295", "-4294967307", typeof(double) },
            new object[] { "-12.0", "9223372036854775807", "-9.223372036854776E+18", typeof(double) },
            new object[] { "-12.0", "18446744073709551615", "-1.8446744073709552E+19", typeof(double) },
            new object[] { "2.0", "2.0f", "0", typeof(double) },
            new object[] { "12.0", "34.0", "-22", typeof(double) }, // Doubles with fraction part zero => fraction part excluded in string representation.
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
            new object[] { "12", "34", "-22", typeof(int) },
            new object[] { "12", "-34", "46", typeof(int) },
            new object[] { "-12", "34", "-46", typeof(int) },
            new object[] { "-12", "-34", "22", typeof(int) },

            new object[] { "2147483647", "2147483647", "0", typeof(int) },
            new object[] { "4294967295", "4294967295", "0", typeof(uint) },
            new object[] { "9223372036854775807", "4294967295", "9223372032559808512", typeof(long) },
            new object[] { "9223372036854775807", "9223372036854775807", "0", typeof(long) },
            new object[] { "9223372036854775807", "2", "9223372036854775805", typeof(long) },
            new object[] { "9223372036854775807", "9223372036854775807", "0", typeof(long) },
            new object[] { "18446744073709551615", "4294967295", "18446744069414584320", typeof(ulong) },
            new object[] { "18446744073709551615", "18446744073709551615", "0", typeof(ulong) },
            new object[] { "18446744073709551616", "2", "18446744073709551614", typeof(BigInteger) },
            new object[] { "18446744073709551616", "4294967295", "18446744069414584321", typeof(BigInteger) },
            new object[] { "18446744073709551616", "9223372036854775807", "9223372036854775809", typeof(BigInteger) },
            new object[] { "18446744073709551616", "18446744073709551615", "1", typeof(BigInteger) },
            new object[] { "18446744073709551616", "18446744073709551617", "-1", typeof(BigInteger) },
            new object[] { "2.0f", "2147483647", "-2.1474836E+09", typeof(float) },
            new object[] { "2.0f", "4294967295", "-4.2949673E+09", typeof(float) },
            new object[] { "2.0f", "9223372036854775807", "-9.223372E+18", typeof(float) },
            new object[] { "2.0f", "18446744073709551615", "-1.8446744E+19", typeof(float) },
            new object[] { "2.0f", "34.0f", "-32", typeof(float) },
            new object[] { "12.0", "34.0f", "-22", typeof(double) },
            new object[] { "-12.0", "-34", "22", typeof(double) }, // Doubles with fraction part zero => fraction part excluded in string representation.
            new object[] { "-12.0", "4294967295", "-4294967307", typeof(double) },
            new object[] { "-12.0", "9223372036854775807", "-9.223372036854776E+18", typeof(double) },
            new object[] { "12.0", "18446744073709551615", "-1.8446744073709552E+19", typeof(double) },
            new object[] { "12.0", "34.0", "-22", typeof(double) },
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
            new object[] { "12", "34", "46", typeof(int) },
            new object[] { "12", "-34", "-22", typeof(int) },
            new object[] { "-12", "34", "22", typeof(int) },
            new object[] { "-12", "4294967295", "4294967283", typeof(long) },
            new object[] { "-12", "-34", "-46", typeof(int) },
            new object[] { "4294967295", "340282349999999991754788743781432688640.0f", "3.4028235E+38", typeof(float) },
            new object[] { "9223372036854775807", "340282349999999991754788743781432688640.0f", "3.4028235E+38", typeof(float) },
            new object[] { "18446744073709551615", "340282349999999991754788743781432688640.0f", "3.4028235E+38", typeof(float) },
            new object[] { "-12", "-34.0", "-46", typeof(double) },
            new object[] { "-12", "-34.0", "-46", typeof(double) },
            new object[] { "2", "4294967295", "4294967297", typeof(long) }, // `int` + `uint` currently expands to `long` to ensure that all potential return values will fit. This may or may nto be a good idea.
            new object[] { "2", "9223372036854775807", "-9223372036854775807", typeof(long) }, // Wraparound because of overflow
            new object[] { "2", "18446744073709551616", "18446744073709551618", typeof(BigInteger) },
            new object[] { "2147483647", "340282349999999991754788743781432688640.0f", "3.4028235E+38", typeof(float) },
            new object[] { "4294967295", "2", "4294967297", typeof(long) },
            new object[] { "4294967295", "4294967295", "4294967294", typeof(uint) }, // Wraparound because of overflow
            new object[] { "4294967295", "9223372036854775807", "-9223372032559808514", typeof(long) },
            new object[] { "4294967295", "18446744073709551615", "4294967294", typeof(ulong) },
            new object[] { "4294967295", "18446744073709551616", "18446744078004518911", typeof(BigInteger) },
            new object[] { "4294967295", "12.0", "4294967307", typeof(double) },
            new object[] { "9223372036854775807", "2", "-9223372036854775807", typeof(long) }, // Integer overflow
            new object[] { "9223372036854775807", "4294967295", "-9223372032559808514", typeof(long) },
            new object[] { "9223372036854775807", "9223372036854775807", "-2", typeof(long) }, // Integer overflow
            new object[] { "9223372036854775807", "18446744073709551616", "27670116110564327423", typeof(BigInteger) },
            new object[] { "9223372036854775807", "12.0", "9.223372036854776E+18", typeof(double) }, // Loses precision, but supported just like in Java and C#
            new object[] { "18446744073709551615", "4294967295", "4294967294", typeof(ulong) }, // Integer overflow
            new object[] { "18446744073709551615", "18446744073709551615", "18446744073709551614", typeof(ulong) }, // Integer overflow
            new object[] { "18446744073709551615", "18446744073709551616", "36893488147419103231", typeof(BigInteger) },
            new object[] { "18446744073709551615", "12.0", "1.8446744073709552E+19", typeof(double) },
            new object[] { "18446744073709551616", "2", "18446744073709551618", typeof(BigInteger) },
            new object[] { "18446744073709551616", "4294967295", "18446744078004518911", typeof(BigInteger) },
            new object[] { "18446744073709551616", "9223372036854775807", "27670116110564327423", typeof(BigInteger) },
            new object[] { "18446744073709551616", "18446744073709551615", "36893488147419103231", typeof(BigInteger) },
            new object[] { "18446744073709551616", "18446744073709551617", "36893488147419103233", typeof(BigInteger) },
            new object[] { "340282349999999991754788743781432688640.0f", "2147483647", "3.4028235E+38", typeof(float) },
            new object[] { "340282349999999991754788743781432688640.0f", "4294967295", "3.4028235E+38", typeof(float) },
            new object[] { "340282349999999991754788743781432688640.0f", "9223372036854775807", "3.4028235E+38", typeof(float) },
            new object[] { "340282349999999991754788743781432688640.0f", "18446744073709551615", "3.4028235E+38", typeof(float) },
            new object[] { "340282349999999991754788743781432688640.0f", "340282349999999991754788743781432688640.0f", "Infinity", typeof(float) },
            new object[] { "340282349999999991754788743781432688640.0f", "12.0", "3.4028234663852886E+38", typeof(double) },
            new object[] { "12.0", "34", "46", typeof(double) },
            new object[] { "12.0", "4294967295", "4294967307", typeof(double) },
            new object[] { "12.0", "9223372036854775807", "9.223372036854776E+18", typeof(double) },
            new object[] { "12.0", "18446744073709551615", "1.8446744073709552E+19", typeof(double) },
            new object[] { "12.0", "340282349999999991754788743781432688640.0f", "3.4028234663852886E+38", typeof(double) },
            new object[] { "12.0", "34.0", "46", typeof(double) }, // Doubles with fraction part zero => fraction part excluded in string representation.
            new object[] { "12.1", "34.2", "46.300000000000004", typeof(double) }, // IEEE-754... :-)
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
            new object[] { "12", "34", "46", typeof(int) },
            new object[] { "12", "-34", "-22", typeof(int) },
            new object[] { "-12", "34", "22", typeof(int) },
            new object[] { "-12", "-34", "-46", typeof(int) },
            new object[] { "4294967295", "4294967295", "4294967294", typeof(uint) },
            new object[] { "4294967296", "9223372036854775807", "-9223372032559808513", typeof(long) }, // Integer overflow
            new object[] { "9223372036854775807", "4294967295", "-9223372032559808514", typeof(long) }, // Integer overflow
            new object[] { "9223372036854775807", "9223372036854775807", "-2", typeof(long) },
            new object[] { "9223372036854775807", "2", "-9223372036854775807", typeof(long) },
            new object[] { "18446744073709551615", "4294967295", "4294967294", typeof(ulong) }, // Integer overflow
            new object[] { "18446744073709551615", "18446744073709551615", "18446744073709551614", typeof(ulong) },
            new object[] { "18446744073709551616", "2", "18446744073709551618", typeof(BigInteger) },
            new object[] { "18446744073709551616", "4294967295", "18446744078004518911", typeof(BigInteger) },
            new object[] { "18446744073709551616", "9223372036854775807", "27670116110564327423", typeof(BigInteger) },
            new object[] { "18446744073709551616", "18446744073709551615", "36893488147419103231", typeof(BigInteger) },
            new object[] { "18446744073709551616", "18446744073709551617", "36893488147419103233", typeof(BigInteger) },
            new object[] { "2.0f", "2147483647", "2.1474836E+09", typeof(float) },
            new object[] { "2.0f", "4294967295", "4.2949673E+09", typeof(float) },
            new object[] { "2.0f", "9223372036854775807", "9.223372E+18", typeof(float) },
            new object[] { "2.0f", "18446744073709551615", "1.8446744E+19", typeof(float) },
            new object[] { "2.0f", "2.0f", "4", typeof(float) },
            new object[] { "-12.0", "-34", "-46", typeof(double) },
            new object[] { "-12.0", "4294967295", "4294967283", typeof(double) },
            new object[] { "-12.0", "9223372036854775807", "9.223372036854776E+18", typeof(double) },
            new object[] { "-12.0", "18446744073709551615", "1.8446744073709552E+19", typeof(double) },
            new object[] { "12.0", "34.0", "46", typeof(double) }, // Doubles with fraction part zero => fraction part excluded in string representation.
            new object[] { "12.1", "34.2", "46.300000000000004", typeof(double) }, // IEEE-754... :-)
            new object[] { "12.0", "340282349999999991754788743781432688640.0f", "3.4028234663852886E+38", typeof(double) },
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
            new object[] { "35", "5", "7", typeof(int) },
            new object[] { "34", "5", "6", typeof(int) }, // `int` division => expecting to be truncated.
            new object[] { "2147483647", "4294967295", "0", typeof(long) },
            new object[] { "2147483647", "9223372036854775807", "0", typeof(long) },
            new object[] { "2147483647", "18446744073709551616", "0", typeof(BigInteger) },
            new object[] { "2147483647", "340282349999999991754788743781432688640.0f", "6.310888E-30", typeof(float) },
            new object[] { "4294967295", "2", "2147483647", typeof(long) },
            new object[] { "4294967295", "4294967295", "1", typeof(uint) },
            new object[] { "4294967295", "9223372036854775807", "0", typeof(long) },
            new object[] { "4294967295", "18446744073709551615", "0", typeof(ulong) },
            new object[] { "4294967295", "18446744073709551616", "0", typeof(BigInteger) },
            new object[] { "4294967295", "340282349999999991754788743781432688640.0f", "1.2621776E-29", typeof(float) },
            new object[] { "4294967295", "12.0", "357913941.25", typeof(double) },
            new object[] { "9223372036854775807", "2", "4611686018427387903", typeof(long) },
            new object[] { "9223372036854775807", "4294967295", "2147483648", typeof(long) },
            new object[] { "9223372036854775807", "9223372036854775807", "1", typeof(long) },
            new object[] { "9223372036854775807", "18446744073709551616", "0", typeof(BigInteger) },
            new object[] { "9223372036854775807", "340282349999999991754788743781432688640.0f", "2.7105058E-20", typeof(float) },
            new object[] { "9223372036854775807", "12.0", "7.686143364045646E+17", typeof(double) },
            new object[] { "18446744073709551615", "4294967295", "4294967297", typeof(ulong) },
            new object[] { "18446744073709551615", "18446744073709551615", "1", typeof(ulong) },
            new object[] { "18446744073709551615", "18446744073709551616", "0", typeof(BigInteger) },
            new object[] { "18446744073709551615", "340282349999999991754788743781432688640.0f", "5.4210115E-20", typeof(float) },
            new object[] { "18446744073709551615", "12.0", "1.5372286728091292E+18", typeof(double) },
            new object[] { "18446744073709551616", "2", "9223372036854775808", typeof(BigInteger) },
            new object[] { "18446744073709551616", "4294967295", "4294967297", typeof(BigInteger) },
            new object[] { "18446744073709551616", "9223372036854775807", "2", typeof(BigInteger) },
            new object[] { "18446744073709551616", "18446744073709551615", "1", typeof(BigInteger) },
            new object[] { "18446744073709551616", "18446744073709551616", "1", typeof(BigInteger) },
            new object[] { "340282349999999991754788743781432688640.0f", "2147483647", "1.5845632E+29", typeof(float) },
            new object[] { "340282349999999991754788743781432688640.0f", "4294967295", "7.922816E+28", typeof(float) },
            new object[] { "340282349999999991754788743781432688640.0f", "9223372036854775807", "3.6893486E+19", typeof(float) },
            new object[] { "340282349999999991754788743781432688640.0f", "18446744073709551615", "1.8446743E+19", typeof(float) },
            new object[] { "340282349999999991754788743781432688640.0f", "340282349999999991754788743781432688640.0f", "1", typeof(float) },
            new object[] { "340282349999999991754788743781432688640.0f", "12.0", "2.8356862219877405E+37", typeof(double) },
            new object[] { "34.0", "5", "6.8", typeof(double) },
            new object[] { "12.0", "4294967295", "2.793967724496957E-09", typeof(double) },
            new object[] { "12.0", "18446744073709551615", "6.505213034913027E-19", typeof(double) },
            new object[] { "34.0", "9223372036854775807", "3.686287386450715E-18", typeof(double) },
            new object[] { "34.0", "340282349999999991754788743781432688640.0f", "9.991702577541327E-38", typeof(double) },
            new object[] { "34.0", "5.0", "6.8", typeof(double) },
            new object[] { "34", "5.0", "6.8", typeof(double) }
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
            new object[] { "5", "3", "15", typeof(int) },
            new object[] { "2", "4294967295", "8589934590", typeof(long) },
            new object[] { "2", "9223372036854775807", "-2", typeof(long) },
            new object[] { "2", "18446744073709551616", "36893488147419103232", typeof(BigInteger) },
            new object[] { "12", "34.0", "408", typeof(double) },
            new object[] { "1073741824", "2", "-2147483648", typeof(int) }, // Becomes negative because of signed `int` overflow.
            new object[] { "2147483647", "2.0f", "4.2949673E+09", typeof(float) },
            new object[] { "4294967295", "2", "8589934590", typeof(long) },
            new object[] { "4294967295", "4294967295", "1", typeof(uint) }, // Unsigned integer overflow
            new object[] { "4294967295", "9223372036854775807", "9223372032559808513", typeof(long) }, // Signed integer overflow
            new object[] { "4294967295", "18446744073709551615", "18446744069414584321", typeof(ulong) }, // Unsigned integer overflow
            new object[] { "4294967295", "18446744073709551616", "79228162495817593519834398720", typeof(BigInteger) },
            new object[] { "4294967295", "2.0f", "8.589935E+09", typeof(float) },
            new object[] { "4294967295", "12.0", "51539607540", typeof(double) },
            new object[] { "9223372036854775807", "2", "-2", typeof(long) },
            new object[] { "9223372036854775807", "4294967295", "9223372032559808513", typeof(long) }, // Overflow
            new object[] { "9223372036854775807", "9223372036854775807", "1", typeof(long) },
            new object[] { "9223372036854775807", "18446744073709551616", "170141183460469231713240559642174554112", typeof(BigInteger) },
            new object[] { "9223372036854775807", "2.0f", "1.8446744E+19", typeof(float) },
            new object[] { "9223372036854775807", "12.0", "1.1068046444225731E+20", typeof(double) },
            new object[] { "18446744073709551615", "4294967295", "18446744069414584321", typeof(ulong) }, // Unsigned integer overflow
            new object[] { "18446744073709551615", "18446744073709551615", "1", typeof(ulong) }, // Unsigned integer overflow
            new object[] { "18446744073709551615", "18446744073709551616", "340282366920938463444927863358058659840", typeof(BigInteger) },
            new object[] { "18446744073709551615", "2.0f", "3.689349E+19", typeof(float) },
            new object[] { "18446744073709551615", "12.0", "2.2136092888451462E+20", typeof(double) },
            new object[] { "18446744073709551616", "2", "36893488147419103232", typeof(BigInteger) },
            new object[] { "18446744073709551616", "4294967295", "79228162495817593519834398720", typeof(BigInteger) },
            new object[] { "18446744073709551616", "9223372036854775807", "170141183460469231713240559642174554112", typeof(BigInteger) },
            new object[] { "18446744073709551616", "18446744073709551615", "340282366920938463444927863358058659840", typeof(BigInteger) },
            new object[] { "18446744073709551616", "18446744073709551616", "340282366920938463463374607431768211456", typeof(BigInteger) },
            new object[] { "2.0f", "2147483647", "4.2949673E+09", typeof(float) },
            new object[] { "2.0f", "4294967295", "8.589935E+09", typeof(float) },
            new object[] { "2.0f", "9223372036854775807", "1.8446744E+19", typeof(float) },
            new object[] { "2.0f", "18446744073709551615", "3.689349E+19", typeof(float) },
            new object[] { "340282349999999991754788743781432688640.0f", "2.0f", "Infinity", typeof(float) },
            new object[] { "340282349999999991754788743781432688640.0f", "2.0", "6.805646932770577E+38", typeof(double) },
            new object[] { "12.0", "5", "60", typeof(double) },
            new object[] { "12.0", "4294967295", "51539607540", typeof(double) },
            new object[] { "12.0", "9223372036854775807", "1.1068046444225731E+20", typeof(double) },
            new object[] { "12.0", "18446744073709551615", "2.2136092888451462E+20", typeof(double) },
            new object[] { "12.34", "340282349999999991754788743781432688640.0f", "4.199084157519446E+39", typeof(double) },
            new object[] { "12.34", "0.3", "3.702", typeof(double) }
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
            new object[] { "2", "10", "1024", typeof(BigInteger) }, // Currently always expanded to a big integer type, even for cases like this where it would not be required
            new object[] { "2.1", "10", "1667.9880978201006", typeof(double) },
            new object[] { "2.0", "10.0", "1024", typeof(double) },
            new object[] { "2", "9.9", "955.425783333691", typeof(double) },
            new object[] { "2147483647", "2.0f", "4.6116860141324206E+18", typeof(double) },
            new object[] { "2147483647", "2.0", "4.6116860141324206E+18", typeof(double) },
            new object[] { "4294967295", "2", "18446744065119617025", typeof(BigInteger) },
            new object[] { "4294967295", "340282349999999991754788743781432688640.0f", "Infinity", typeof(double) },
            new object[] { "4294967295", "12.0", "3.9402006086306546E+115", typeof(double) },
            new object[] { "9223372036854775807", "2", "85070591730234615847396907784232501249", typeof(BigInteger) },
            new object[] { "9223372036854775807", "2.0f", "8.507059173023462E+37", typeof(double) },
            new object[] { "9223372036854775807", "2.0", "8.507059173023462E+37", typeof(double) },
            new object[] { "18446744073709551615", "2", "340282366920938463426481119284349108225", typeof(BigInteger) },
            new object[] { "18446744073709551615", "2.0f", "3.402823669209385E+38", typeof(double) },
            new object[] { "18446744073709551615", "2.0", "3.402823669209385E+38", typeof(double) },
            new object[] { "18446744073709551616", "2", "340282366920938463463374607431768211456", typeof(BigInteger) },
            new object[] { "340282349999999991754788743781432688640.0f", "2147483647", "Infinity", typeof(double) },
            new object[] { "340282349999999991754788743781432688640.0f", "4294967295", "Infinity", typeof(double) },
            new object[] { "340282349999999991754788743781432688640.0f", "9223372036854775807", "Infinity", typeof(double) },
            new object[] { "340282349999999991754788743781432688640.0f", "18446744073709551615", "Infinity", typeof(double) },
            new object[] { "340282349999999991754788743781432688640.0f", "340282349999999991754788743781432688640.0f", "Infinity", typeof(double) },
            new object[] { "340282349999999991754788743781432688640.0f", "12.0", "Infinity", typeof(double) },
            new object[] { "2.0", "4294967295", "Infinity", typeof(double) },
            new object[] { "2.0", "9223372036854775807", "Infinity", typeof(double) },
            new object[] { "12.0", "18446744073709551615", "Infinity", typeof(double) },
            new object[] { "12.0", "340282349999999991754788743781432688640.0f", "Infinity", typeof(double) },
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
            new object[] { "2", "4294967295", "2", typeof(long) },
            new object[] { "5", "3", "2", typeof(int) },
            new object[] { "2", "9223372036854775807", "2", typeof(long) },
            new object[] { "2", "18446744073709551616", "2", typeof(BigInteger) },
            new object[] { "9", "2.0", "1", typeof(double) },
            new object[] { "2147483647", "2", "1", typeof(int) },
            new object[] { "2147483647", "2.0f", "0", typeof(float) }, // IEEE-754 :-)
            new object[] { "2147483647", "2.0", "1", typeof(double) },
            new object[] { "-2147483648", "2", "0", typeof(int) },
            new object[] { "4294967295", "2", "1", typeof(long) },
            new object[] { "4294967295", "4294967295", "0", typeof(uint) },
            new object[] { "4294967295", "9223372036854775807", "4294967295", typeof(long) },
            new object[] { "4294967295", "18446744073709551615", "4294967295", typeof(ulong) },
            new object[] { "4294967295", "18446744073709551616", "4294967295", typeof(BigInteger) },
            new object[] { "4294967295", "12", "3", typeof(long) }, // TODO: should be uint
            new object[] { "4294967295", "12.0f", "4", typeof(float) }, // IEEE-754 :-)
            new object[] { "4294967295", "12.0", "3", typeof(double) },
            new object[] { "9223372036854775807", "2", "1", typeof(long) },
            new object[] { "9223372036854775807", "4294967295", "2147483647", typeof(long) },
            new object[] { "9223372036854775807", "9223372036854775807", "0", typeof(long) },
            new object[] { "9223372036854775807", "18446744073709551616", "9223372036854775807", typeof(BigInteger) },
            new object[] { "9223372036854775807", "12.0f", "8", typeof(float) },
            new object[] { "9223372036854775807", "12.0", "8", typeof(double) },
            new object[] { "18446744073709551615", "4294967295", "0", typeof(ulong) },
            new object[] { "18446744073709551615", "18446744073709551615", "0", typeof(ulong) },
            new object[] { "18446744073709551615", "18446744073709551616", "18446744073709551615", typeof(BigInteger) },
            new object[] { "18446744073709551615", "12.0f", "4", typeof(float) },
            new object[] { "18446744073709551615", "12.0", "4", typeof(double) },
            new object[] { "18446744073709551616", "3", "1", typeof(BigInteger) },
            new object[] { "18446744073709551616", "4294967295", "1", typeof(BigInteger) },
            new object[] { "18446744073709551616", "9223372036854775807", "2", typeof(BigInteger) },
            new object[] { "18446744073709551616", "18446744073709551615", "1", typeof(BigInteger) },
            new object[] { "18446744073709551616", "18446744073709551616", "0", typeof(BigInteger) },
            new object[] { "9.0f", "2147483647", "9", typeof(float) },
            new object[] { "9.0f", "4294967295", "9", typeof(float) },
            new object[] { "9.0f", "9223372036854775807", "9", typeof(float) },
            new object[] { "9.0f", "18446744073709551615", "9", typeof(float) },
            new object[] { "9.0f", "2.0f", "1", typeof(float) },
            new object[] { "9.0f", "2.0", "1", typeof(double) },
            new object[] { "12.0", "2", "0", typeof(double) },
            new object[] { "12.0", "4294967295", "12", typeof(double) },
            new object[] { "12.0", "9223372036854775807", "12", typeof(double) },
            new object[] { "12.0", "18446744073709551615", "12", typeof(double) },
            new object[] { "9.0", "2.0f", "1", typeof(double) },
            new object[] { "9.0", "2.0", "1", typeof(double) },
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
            new object[] { "1", "10", "1024", typeof(int) },
            new object[] { "1073741824", "1", "-2147483648", typeof(int) }, // Signed wraparound => becomes negative
            new object[] { "4294967295", "2", "4294967292", typeof(uint) },
            new object[] { "9223372036854775807", "2", "-4", typeof(long) }, // Signed wraparound => becomes negative
            new object[] { "18446744073709551615", "2", "18446744073709551612", typeof(ulong) }, // Unsigned integer wraparound
            new object[] { "18446744073709551616", "2", "73786976294838206464", typeof(BigInteger) },
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
            new object[] { "2147483647", "32", "2147483647", typeof(int) },
            new object[] { "4294967295", "2", "1073741823", typeof(uint) },
            new object[] { "9223372036854775807", "2", "2305843009213693951", typeof(long) },
            new object[] { "18446744073709551615", "2", "4611686018427387903", typeof(ulong) },
            new object[] { "18446744073709551616", "2", "4611686018427387904", typeof(BigInteger) },
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
        resultAndTypes.Select(obj => new[] { obj[0], obj[1], obj[3].ToString() });
}
