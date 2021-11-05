using System.Linq;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Number
{
    // Based on https://github.com/munificent/craftinginterpreters/blob/master/test/number
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
        public void literal_float()
        {
            string source = @"
                123.456
            ";

            object result = Eval(source);

            Assert.Equal(123.456, result);
        }

        [Fact]
        public void literal_negative_float()
        {
            string source = @"
                -0.001
            ";

            object result = Eval(source);

            Assert.Equal(-0.001, result);
        }

        [Fact]
        public void literal_float_with_underscore_in_integer_part()
        {
            string source = @"
                123_45.678
            ";

            object result = Eval(source);

            Assert.Equal(12345.678, result);
        }

        [Fact]
        public void literal_float_with_underscore_in_fractional_part()
        {
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
