using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Operator.Binary
{
    // Tests based on the following:
    // https://github.com/munificent/craftinginterpreters/blob/c6da0e61e6072271de404464c34b51c2fdc39e59/test/operator/divide.lox
    // https://github.com/munificent/craftinginterpreters/blob/c6da0e61e6072271de404464c34b51c2fdc39e59/test/operator/divide_nonnum_num.lox
    // https://github.com/munificent/craftinginterpreters/blob/c6da0e61e6072271de404464c34b51c2fdc39e59/test/operator/divide_num_nonnum.lox
    public class DivisionTests
    {
        [Theory]
        [MemberData(nameof(BinaryOperatorData.Division_result), MemberType = typeof(BinaryOperatorData))]
        void performs_division(string i, string j, string expectedResult)
        {
            string source = $@"
                var i1 = {i};
                var i2 = {j};

                print i1 / i2;
            ";

            string result = EvalReturningOutputString(source);

            result.Should()
                .Be(expectedResult);
        }

        [SkippableTheory]
        [MemberData(nameof(BinaryOperatorData.Division_type), MemberType = typeof(BinaryOperatorData))]
        public void with_supported_types_returns_expected_type(string i, string j, string expectedResult)
        {
            Skip.If(PerlangMode.ExperimentalCompilation, "Not supported in compiled mode");

            string source = $@"
                    print ({i} / {j}).get_type();
                ";

            string result = EvalReturningOutputString(source);

            result.Should()
                .Be(expectedResult);
        }

        [Theory]
        [MemberData(nameof(BinaryOperatorData.Division_unsupported_types), MemberType = typeof(BinaryOperatorData))]
        public void with_unsupported_types_emits_expected_error(string i, string j, string expectedResult)
        {
            string source = $@"
                    print {i} / {j};
                ";

            var result = EvalWithValidationErrorCatch(source);

            result.Errors.Should()
                .ContainSingle().Which
                .Message.Should().Match(expectedResult);
        }

        [SkippableTheory]
        [ClassData(typeof(TestCultures))]
        public async Task dividing_doubles_works_on_different_cultures(CultureInfo cultureInfo)
        {
            CultureInfo.CurrentCulture = cultureInfo;

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
            Assert.Matches("Unsupported / operand types: 'AsciiString' and 'int'", exception.Message);
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
            Assert.Matches("Unsupported / operand types: 'int' and 'AsciiString'", exception.Message);
        }

        // TODO: This should definitely be a compile-time error (constant division by zero). We should aim for
        // TODO: evaluating constant expressions completely at compile-time, which may have other benefits also.

        [Fact]
        public void division_by_zero_throws_expected_runtime_error()
        {
            string source = @"
                1 / 0
            ";

            var result = EvalWithRuntimeErrorCatch(source);
            var exception = result.Errors.FirstOrDefault();

            Assert.Single(result.Errors);
            Assert.Matches("Attempted to divide by zero.", exception.Message);
        }

        [Fact]
        public void division_by_zero_halts_execution()
        {
            string source = @"
                1 / 0;
                print ""hejhej"";
            ";

            var result = EvalWithRuntimeErrorCatch(source);

            Assert.Empty(result.OutputAsString);
        }
    }
}
