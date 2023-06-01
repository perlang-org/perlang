using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Perlang.Tests.Integration.Typing;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Number
{
    /// <summary>
    /// Based on https://github.com/munificent/craftinginterpreters/blob/master/test/number
    ///
    /// Note that not all number-related tests are located in this class. See the other related classes for more
    /// specific tests for particular data types:
    ///
    /// <list type="bullet">
    /// <item><description><see cref="BigintTests"/></description></item>
    /// <item><description><see cref="DoubleTests"/></description></item>
    /// <item><description><see cref="IntTests"/></description></item>
    /// <item><description><see cref="LongTests"/></description></item>
    /// </list>
    ///
    /// The rationale is basically this: Tests which specify explicit variable tests belong in the "specific" test
    /// classes for that data type. Tests which exercise the "find the smallest suitable integer/floating-point type"
    /// fit better in <see cref="NumberTests"/>.
    /// </summary>
    public class NumberTests
    {
        [Fact]
        public void decimal_point_at_eof()
        {
            string source = @"
                123.
            ";

            var result = EvalWithParseErrorCatch(source);
            var exception = result.Errors.FirstOrDefault();

            Assert.Single(result.Errors);
            Assert.Matches("Expect identifier after '.'", exception.Message);
        }

        [Fact]
        public void leading_dot()
        {
            string source = @"
                .123
            ";

            var result = EvalWithParseErrorCatch(source);
            var exception = result.Errors.FirstOrDefault();

            Assert.Single(result.Errors);
            Assert.Matches("Expect expression", exception.Message);
        }

        [SkippableFact]
        public void literal_integer()
        {
            string source = @"
                123
            ";

            object result = Eval(source);

            Assert.Equal(123, result);
        }

        [SkippableFact]
        public void literal_integer_with_underscores()
        {
            string source = @"
                123_456
            ";

            object result = Eval(source);

            Assert.Equal(123456, result);
        }

        [SkippableFact]
        public void literal_zero()
        {
            string source = @"
                0
            ";

            object result = Eval(source);

            Assert.Equal(0, result);
        }

        [SkippableFact]
        public void literal_negative_zero()
        {
            string source = @"
                -0
            ";

            object result = Eval(source);

            Assert.Equal(-0, result);
        }

        [SkippableFact]
        public void literal_negative_integer()
        {
            string source = @"
                -123
            ";

            object result = Eval(source);

            Assert.Equal(-123, result);
        }

        [SkippableFact]
        public void literal_negative_larger_integer()
        {
            string source = @"
                -2147483648
            ";

            object result = Eval(source);

            Assert.Equal(-2147483648, result);
        }

        [SkippableTheory]
        [ClassData(typeof(TestCultures))]
        public async Task literal_float(CultureInfo cultureInfo)
        {
            CultureInfo.CurrentCulture = cultureInfo;

            string source = @"
                123.456f
            ";

            object result = Eval(source);

            result.Should()
                .Be(123.456f);
        }

        [SkippableTheory]
        [ClassData(typeof(TestCultures))]
        public async Task literal_float_has_expected_type(CultureInfo cultureInfo)
        {
            CultureInfo.CurrentCulture = cultureInfo;

            string source = @"
                123.456f
            ";

            object result = Eval(source);

            result.Should()
                .BeOfType<float>();
        }

        [SkippableTheory]
        [ClassData(typeof(TestCultures))]
        public async Task literal_negative_float(CultureInfo cultureInfo)
        {
            CultureInfo.CurrentCulture = cultureInfo;

            string source = @"
                -0.001f
            ";

            object result = Eval(source);

            result.Should()
                .Be(-0.001f);
        }

        [SkippableTheory]
        [ClassData(typeof(TestCultures))]
        public async Task literal_float_with_underscore_in_integer_part(CultureInfo cultureInfo)
        {
            CultureInfo.CurrentCulture = cultureInfo;

            string source = @"
                123_45.678f
            ";

            object result = Eval(source);

            result.Should()
                .Be(12345.678f);
        }

        [SkippableTheory]
        [ClassData(typeof(TestCultures))]
        public async Task literal_float_with_underscore_in_fractional_part(CultureInfo cultureInfo)
        {
            CultureInfo.CurrentCulture = cultureInfo;

            string source = @"
                123.45_678f
            ";

            object result = Eval(source);

            result.Should()
                .Be(123.45678f);
        }

        [SkippableTheory]
        [ClassData(typeof(TestCultures))]
        public async Task literal_double_with_suffix(CultureInfo cultureInfo)
        {
            CultureInfo.CurrentCulture = cultureInfo;

            string source = @"
                123.456d
            ";

            object result = Eval(source);

            result.Should()
                .Be(123.456d);
        }

        [SkippableTheory]
        [ClassData(typeof(TestCultures))]
        public async Task literal_double_with_suffix_has_expected_type(CultureInfo cultureInfo)
        {
            CultureInfo.CurrentCulture = cultureInfo;

            string source = @"
                123.456d
            ";

            object result = Eval(source);

            result.Should()
                .BeOfType<double>();
        }

        [SkippableTheory]
        [ClassData(typeof(TestCultures))]
        public async Task literal_double_with_implicit_suffix(CultureInfo cultureInfo)
        {
            CultureInfo.CurrentCulture = cultureInfo;

            string source = @"
                123.456
            ";

            object result = Eval(source);

            result.Should()
                .Be(123.456d);
        }

        [SkippableTheory]
        [ClassData(typeof(TestCultures))]
        public async Task literal_double_with_implicit_suffix_has_expected_type(CultureInfo cultureInfo)
        {
            CultureInfo.CurrentCulture = cultureInfo;

            string source = @"
                123.456
            ";

            object result = Eval(source);

            result.Should()
                .BeOfType<double>();
        }

        [SkippableFact]
        public void literal_binary()
        {
            string source = @"
                0b00101010
            ";

            object result = Eval(source);

            Assert.Equal(42, result);
        }

        [SkippableFact]
        public void literal_binary_with_underscores()
        {
            string source = @"
                0b0010_1010
            ";

            object result = Eval(source);

            Assert.Equal(42, result);
        }

        [SkippableFact]
        public void literal_octal()
        {
            string source = @"
                0o755
            ";

            object result = Eval(source);

            Assert.Equal(493, result);
        }

        [SkippableFact]
        public void literal_hexadecimal()
        {
            string source = @"
                0xC0CAC01A
            ";

            object result = Eval(source);

            Assert.Equal(3234512922, result);
        }

        [SkippableFact]
        public void literal_hexadecimal_with_underscores()
        {
            string source = @"
                0xC0_CA_C0_1A
            ";

            object result = Eval(source);

            Assert.Equal(3234512922, result);
        }
    }
}
