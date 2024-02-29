using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Operator.Binary
{
    // TODO: There is a certain overlap with these tests and the Less/LessEqual/Greater/GreaterEqual ones. We can
    // TODO: probably do away with these tests at some point, and let the unique tests in this be absorbed into the
    // TODO: specific tests.

    // Tests based on https://github.com/munificent/craftinginterpreters/blob/master/test/operator/comparison.lox
    public class Comparison
    {
        public static readonly List<object[]> ComparisonTypes = new()
        {
            new object[] { "int", "int" },
            new object[] { "int", "long" },
            new object[] { "int", "bigint" },
            new object[] { "long", "int" },
            new object[] { "long", "long" },
            new object[] { "long", "bigint" },

            // TODO: Make these work. They are dependent on constant expression values being available at compile-time,
            // TODO: since we do `int`-to-`double` assignments in these tests. This is perfectly _fine_ for small,
            // TODO: supported values but unsafe for non-constant values (since a loss of precision can occur).
            //new object[] { "double", "double" },
            //new object[] { "bigint", "double" },  // See #229
            //new object[] { "double", "bigint" }   // See #229
        };

        //
        // Tests for the < (less than) operator
        //

        [Theory]
        [MemberData(nameof(ComparisonTypes))]
        public void less_than_greater_is_true(string left, string right)
        {
            string source = $@"
                var left: {left} = 1;
                var right: {right} = 2;
                var b = left < right;
                print b;
            ";

            // "True" vs "true" in interpreted and compiled mode
            string output = EvalReturningOutputString(source)
                .ToLower();

            output.Should()
                .Be("true");
        }

        [Fact]
        public void less_than_bigint_and_double_throws_expected_error()
        {
            string source = $@"
                var b = (2 ** 64) < 18446744073709551616.1;
                print b;
            ";

            var result = EvalWithValidationErrorCatch(source);

            result.Errors.Should()
                .ContainSingle().Which
                .Message.Should().Be("Unsupported < operand types: 'bigint' and 'double'");
        }

        [Theory]
        [MemberData(nameof(ComparisonTypes))]
        public void less_than_same_is_false(string left, string right)
        {
            string source = $@"
                var left: {left} = 2;
                var right: {right} = 2;
                var b = left < right;
                print b;
            ";

            // "False" vs "false" in interpreted and compiled mode
            string output = EvalReturningOutputString(source)
                .ToLower();

            output.Should()
                .Be("false");
        }

        [Theory]
        [MemberData(nameof(ComparisonTypes))]
        public void less_than_smaller_is_false(string left, string right)
        {
            string source = $@"
                var left: {left} = 2;
                var right: {right} = 1;
                var b = left < right;
                print b;
            ";

            // "False" vs "false" in interpreted and compiled mode
            string output = EvalReturningOutputString(source)
                .ToLower();

            output.Should()
                .Be("false");
        }

        //
        // Tests for the <= (less than or equals) operator
        //

        [Theory]
        [MemberData(nameof(ComparisonTypes))]
        public void less_than_or_equals_greater_is_true(string left, string right)
        {
            string source = $@"
                var left: {left} = 1;
                var right: {right} = 2;
                var b = left <= right;
                print b;
            ";

            // "True" vs "true" in interpreted and compiled mode
            string output = EvalReturningOutputString(source)
                .ToLower();

            output.Should()
                .Be("true");
        }

        [Theory]
        [MemberData(nameof(ComparisonTypes))]
        public void less_than_or_equals_same_is_true(string left, string right)
        {
            string source = $@"
                var left: {left} = 2;
                var right: {right} = 2;
                var b = left <= right;
                print b;
            ";

            // "True" vs "true" in interpreted and compiled mode
            string output = EvalReturningOutputString(source)
                .ToLower();

            output.Should()
                .Be("true");
        }

        [Theory]
        [MemberData(nameof(ComparisonTypes))]
        public void less_than_or_equals_smaller_is_false(string left, string right)
        {
            string source = $@"
                var left: {left} = 2;
                var right: {right} = 1;
                var b = left <= right;
                print b;
            ";

            // "False" vs "false" in interpreted and compiled mode
            string output = EvalReturningOutputString(source)
                .ToLower();

            output.Should()
                .Be("false");
        }

        //
        // Tests for the > (greater than) operator
        //

        [Theory]
        [MemberData(nameof(ComparisonTypes))]
        public void greater_than_smaller_is_false(string left, string right)
        {
            string source = $@"
                var left: {left} = 1;
                var right: {right} = 2;
                var b = left > right;
                print b;
            ";

            // "False" vs "false" in interpreted and compiled mode
            string output = EvalReturningOutputString(source)
                .ToLower();

            output.Should()
                .Be("false");
        }

        [Theory]
        [MemberData(nameof(ComparisonTypes))]
        public void greater_than_same_is_false(string left, string right)
        {
            string source = $@"
                var left: {left} = 2;
                var right: {right} = 2;
                var b = left > right;
                print b;
            ";

            // "False" vs "false" in interpreted and compiled mode
            string output = EvalReturningOutputString(source)
                .ToLower();

            output.Should()
                .Be("false");
        }

        [Theory]
        [MemberData(nameof(ComparisonTypes))]
        public void greater_than_smaller_is_true(string left, string right)
        {
            string source = $@"
                var left: {left} = 2;
                var right: {right} = 1;
                var b = left > right;
                print b;
            ";

            // "True" vs "true" in interpreted and compiled mode
            string output = EvalReturningOutputString(source)
                .ToLower();

            output.Should()
                .Be("true");
        }

        //
        // Tests for the >= (greater than or equals) operator
        //
        [Theory]
        [MemberData(nameof(ComparisonTypes))]
        public void greater_than_or_equals_larger_is_false(string left, string right)
        {
            string source = $@"
                var left: {left} = 1;
                var right: {right} = 2;
                var b = left >= right;
                print b;
            ";

            // "False" vs "false" in interpreted and compiled mode
            string output = EvalReturningOutputString(source)
                .ToLower();

            output.Should()
                .Be("false");
        }

        [Theory]
        [MemberData(nameof(ComparisonTypes))]
        public void greater_than_or_equals_same_is_true(string left, string right)
        {
            string source = $@"
                var left: {left} = 2;
                var right: {right} = 2;
                var b = left >= right;
                print b;
            ";

            // "True" vs "true" in interpreted and compiled mode
            string output = EvalReturningOutputString(source)
                .ToLower();

            output.Should()
                .Be("true");
        }

        [Theory]
        [MemberData(nameof(ComparisonTypes))]
        public void greater_than_or_equals_smaller_is_true(string left, string right)
        {
            string source = $@"
                var left: {left} = 2;
                var right: {right} = 1;
                var b = left >= right;
                print b;
            ";

            // "True" vs "true" in interpreted and compiled mode
            string output = EvalReturningOutputString(source)
                .ToLower();

            output.Should()
                .Be("true");
        }

        //
        // Special tests for 0.0 and -0.0, ensuring that positive and negative zero compares as the same value.
        //
        // Note that this is specifically only relevant for floats; its (IEEE 754) binary representation supports the
        // distinction between these numbers. For integers, this is not true; the binary representation of two's
        // complement (used to store negative integers on the majority of today's computer architectures) does not allow
        // the concept of negative zero.
        //
        // More on the subject:
        //
        // - Signed zero: https://en.wikipedia.org/wiki/Signed_zero
        // - Two's complement: https://en.wikipedia.org/wiki/Two%27s_complement
        //
        // Given the above, the actual semantics are pretty much the same for both integer and floats in this aspect. We
        // add a trivial integer test as well first, just for the sake of it.
        [Fact]
        public void zero_and_negative_zero_integers_are_identical()
        {
            // We deliberately do not check for equality, since even negative and positive zero floats are equal.
            // Instead, we check if one is smaller than the other.
            string source = @"
                print 0 < -0;
            ";

            // "False" vs "false" in interpreted and compiled mode
            string output = EvalReturningOutputString(source)
                .ToLower();

            output.Should()
                .Be("false");
        }

        [Theory]
        [ClassData(typeof(TestCultures))]
        public async Task zero_less_than_negative_zero_is_false(CultureInfo cultureInfo)
        {
            CultureInfo.CurrentCulture = cultureInfo;

            string source = @"
                print 0.0 < -0.0;
            ";

            // "False" vs "false" in interpreted and compiled mode
            string output = EvalReturningOutputString(source)
                .ToLower();

            output.Should()
                .Be("false");
        }

        [Theory]
        [ClassData(typeof(TestCultures))]
        public async Task negative_zero_less_than_zero_is_false(CultureInfo cultureInfo)
        {
            CultureInfo.CurrentCulture = cultureInfo;

            string source = @"
                print -0.0 < 0.0;
            ";

            // "False" vs "false" in interpreted and compiled mode
            string output = EvalReturningOutputString(source)
                .ToLower();

            output.Should()
                .Be("false");
        }

        [Theory]
        [ClassData(typeof(TestCultures))]
        public async Task zero_greater_than_negative_zero_is_false(CultureInfo cultureInfo)
        {
            CultureInfo.CurrentCulture = cultureInfo;

            string source = @"
                print 0.0 > -0.0;
            ";

            // "False" vs "false" in interpreted and compiled mode
            string output = EvalReturningOutputString(source)
                .ToLower();

            output.Should()
                .Be("false");
        }

        [Theory]
        [ClassData(typeof(TestCultures))]
        public async Task negative_zero_greater_than_zero_is_false(CultureInfo cultureInfo)
        {
            CultureInfo.CurrentCulture = cultureInfo;

            string source = @"
                print -0.0 > 0.0;
            ";

            // "False" vs "false" in interpreted and compiled mode
            string output = EvalReturningOutputString(source)
                .ToLower();

            output.Should()
                .Be("false");
        }

        [Theory]
        [ClassData(typeof(TestCultures))]
        public async Task zero_less_than_or_equals_negative_zero_is_false(CultureInfo cultureInfo)
        {
            CultureInfo.CurrentCulture = cultureInfo;

            string source = @"
                print 0.0 <= -0.0;
            ";

            // "True" vs "true" in interpreted and compiled mode
            string output = EvalReturningOutputString(source)
                .ToLower();

            output.Should()
                .Be("true");
        }

        [Theory]
        [ClassData(typeof(TestCultures))]
        public async Task negative_zero_less_than_or_equals_zero_is_false(CultureInfo cultureInfo)
        {
            CultureInfo.CurrentCulture = cultureInfo;

            string source = @"
                print -0.0 <= 0.0;
            ";

            // "True" vs "true" in interpreted and compiled mode
            string output = EvalReturningOutputString(source)
                .ToLower();

            output.Should()
                .Be("true");
        }

        [Theory]
        [ClassData(typeof(TestCultures))]
        public async Task zero_greater_than_or_equals_negative_zero_is_true(CultureInfo cultureInfo)
        {
            CultureInfo.CurrentCulture = cultureInfo;

            string source = @"
                print 0.0 >= -0.0;
            ";

            // "True" vs "true" in interpreted and compiled mode
            string output = EvalReturningOutputString(source)
                .ToLower();

            output.Should()
                .Be("true");
        }

        [Theory]
        [ClassData(typeof(TestCultures))]
        public async Task negative_zero_greater_than_or_equals_zero_is_false(CultureInfo cultureInfo)
        {
            CultureInfo.CurrentCulture = cultureInfo;

            string source = @"
                print -0.0 >= 0.0;
            ";

            // "True" vs "true" in interpreted and compiled mode
            string output = EvalReturningOutputString(source)
                .ToLower();

            output.Should()
                .Be("true");
        }
    }
}
