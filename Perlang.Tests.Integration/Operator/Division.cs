using System.Linq;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Operator
{
    // Tests based on the following:
    // https://github.com/munificent/craftinginterpreters/blob/c6da0e61e6072271de404464c34b51c2fdc39e59/test/operator/divide.lox
    // https://github.com/munificent/craftinginterpreters/blob/c6da0e61e6072271de404464c34b51c2fdc39e59/test/operator/divide_nonnum_num.lox
    // https://github.com/munificent/craftinginterpreters/blob/c6da0e61e6072271de404464c34b51c2fdc39e59/test/operator/divide_num_nonnum.lox
    public class Division
    {
        //
        // Tests for the / (division) operator
        //
        [Fact]
        public void dividing_integers_returns_integer()
        {
            string source = @"
                8 / 2
            ";

            object result = Eval(source);

            Assert.Equal(4, result);
        }

        [Fact]
        public void dividing_decimals_returns_decimal()
        {
            string source = @"
                24.68 / 12.34
            ";

            object result = Eval(source);

            Assert.Equal(2.0, result);
        }

        [Fact]
        public void dividing_non_number_with_number_throws_expected_error()
        {
            string source = @"
                ""1"" / 1
            ";

            var result = EvalWithValidationErrorCatch(source);
            var exception = result.Errors.FirstOrDefault();

            Assert.Single(result.Errors);
            Assert.Matches("Invalid arguments to operator SLASH specified", exception.Message);
        }

        [Fact]
        public void dividing_number_with_non_number_throws_expected_error()
        {
            string source = @"
                1 / ""1""
            ";

            var result = EvalWithValidationErrorCatch(source);
            var exception = result.Errors.FirstOrDefault();

            Assert.Single(result.Errors);
            Assert.Matches("Invalid arguments to operator SLASH specified", exception.Message);
        }
    }
}
