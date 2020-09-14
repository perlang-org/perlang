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
            var exception = result.ParseErrors.FirstOrDefault();

            Assert.Single(result.ParseErrors);
            Assert.Matches("Expect identifier after '.'", exception.Message);
        }

        [Fact]
        public void leading_dot()
        {
            string source = @"
                .123
            ";

            var result = EvalWithParseErrorCatch(source);
            var exception = result.ParseErrors.FirstOrDefault();

            Assert.Single(result.ParseErrors);
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
    }
}
