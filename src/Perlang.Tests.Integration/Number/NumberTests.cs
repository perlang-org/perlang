using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
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

        [Fact]
        public void literal_integer()
        {
            string source = @"
                123
            ";

            object result = Eval(source);

            Assert.Equal(123, result);
        }

        [Fact]
        public void literal_integer_with_underscores()
        {
            string source = @"
                123_456
            ";

            object result = Eval(source);

            Assert.Equal(123456, result);
        }

        [Fact]
        public void literal_zero()
        {
            string source = @"
                0
            ";

            object result = Eval(source);

            Assert.Equal(0, result);
        }

        [Fact]
        public void literal_negative_zero()
        {
            string source = @"
                -0
            ";

            object result = Eval(source);

            Assert.Equal(-0, result);
        }

        [Fact]
        public void literal_negative_integer()
        {
            string source = @"
                -123
            ";

            object result = Eval(source);

            Assert.Equal(-123, result);
        }

        [Fact]
        public void literal_negative_larger_integer()
        {
            string source = @"
                -2147483648
            ";

            object result = Eval(source);

            Assert.Equal(-2147483648, result);
        }

        [Theory]
        [ClassData(typeof(TestCultures))]
        public async Task literal_float(CultureInfo cultureInfo)
        {
            CultureInfo.CurrentCulture = cultureInfo;

            string source = @"
                123.456
            ";

            object result = Eval(source);

            Assert.Equal(123.456, result);
        }

        [Theory]
        [ClassData(typeof(TestCultures))]
        public async Task literal_negative_float(CultureInfo cultureInfo)
        {
            CultureInfo.CurrentCulture = cultureInfo;

            string source = @"
                -0.001
            ";

            object result = Eval(source);

            Assert.Equal(-0.001, result);
        }

        [Theory]
        [ClassData(typeof(TestCultures))]
        public async Task literal_float_with_underscore_in_integer_part(CultureInfo cultureInfo)
        {
            CultureInfo.CurrentCulture = cultureInfo;

            string source = @"
                123_45.678
            ";

            object result = Eval(source);

            Assert.Equal(12345.678, result);
        }

        [Theory]
        [ClassData(typeof(TestCultures))]
        public async Task literal_float_with_underscore_in_fractional_part(CultureInfo cultureInfo)
        {
            CultureInfo.CurrentCulture = cultureInfo;

            string source = @"
                123.45_678
            ";

            object result = Eval(source);

            Assert.Equal(123.45678, result);
        }

        [Fact]
        public void literal_binary()
        {
            string source = @"
                0b00101010
            ";

            object result = Eval(source);

            Assert.Equal(42, result);
        }

        [Fact]
        public void literal_binary_with_underscores()
        {
            string source = @"
                0b0010_1010
            ";

            object result = Eval(source);

            Assert.Equal(42, result);
        }

        [Fact]
        public void literal_octal()
        {
            string source = @"
                0o755
            ";

            object result = Eval(source);

            Assert.Equal(493, result);
        }

        [Fact]
        public void literal_hexadecimal()
        {
            string source = @"
                0xC0CAC01A
            ";

            object result = Eval(source);

            Assert.Equal(3234512922, result);
        }

        [Fact]
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
