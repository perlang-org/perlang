using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Operator
{
    // Tests based on the following:
    // https://github.com/munificent/craftinginterpreters/blob/adfb64d00d04e389ddc086d16cc29ac49fc6c21e/test/operator/multiply.lox
    // https://github.com/munificent/craftinginterpreters/blob/adfb64d00d04e389ddc086d16cc29ac49fc6c21e/test/operator/multiply_nonnum_num.lox
    // https://github.com/munificent/craftinginterpreters/blob/adfb64d00d04e389ddc086d16cc29ac49fc6c21e/test/operator/multiply_num_nonnum.lox
    public class Multiplication
    {
        //
        // Tests for the * (multiplication) operator
        //
        [Fact]
        public void multiplying_integers_returns_integer()
        {
            string source = @"
                5 * 3
            ";

            object result = Eval(source);

            Assert.Equal(15, result);
        }

        [Theory]
        [ClassData(typeof(TestCultures))]
        public async Task multiplying_doubles_returns_double(CultureInfo cultureInfo)
        {
            CultureInfo.CurrentCulture = cultureInfo;

            string source = @"
                12.34 * 0.3
            ";

            object result = Eval(source);

            Assert.Equal(3.702, result);
        }

        [Fact]
        public void multiplying_non_number_with_number_throws_expected_error()
        {
            string source = @"
                ""1"" * 1;
            ";

            var result = EvalWithValidationErrorCatch(source);
            var exception = result.Errors.FirstOrDefault();

            Assert.Single(result.Errors);
            Assert.Equal("Unsupported * operands specified: string and int", exception.Message);
        }

        [Fact]
        public void multiplying_number_with_non_number_throws_expected_error()
        {
            string source = @"
                1 * ""1"";
            ";

            var result = EvalWithValidationErrorCatch(source);
            var exception = result.Errors.FirstOrDefault();

            Assert.Single(result.Errors);
            Assert.Equal("Unsupported * operands specified: int and string", exception.Message);
        }
    }
}
