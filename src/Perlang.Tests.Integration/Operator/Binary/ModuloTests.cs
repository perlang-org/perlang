using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Operator.Binary
{
    /// <summary>
    /// Tests for the % (modulo) operator.
    /// </summary>
    public class ModuloTests
    {
        [Theory]
        [MemberData(nameof(BinaryOperatorData.Modulo_result), MemberType = typeof(BinaryOperatorData))]
        public void returns_remainder_of_division(string i, string j, string expectedResult)
        {
            string source = $@"
                print {i} % {j};
            ";

            string result = EvalReturningOutputString(source);

            result.Should()
                .Be(expectedResult);
        }

        [Theory]
        [MemberData(nameof(BinaryOperatorData.Modulo_type), MemberType = typeof(BinaryOperatorData))]
        public void with_supported_types_returns_expected_type(string i, string j, string expectedResult)
        {
            string source = $@"
                    print ({i} % {j}).get_type();
                ";

            string result = EvalReturningOutputString(source);

            result.Should()
                .Be(expectedResult);
        }

        [Theory]
        [MemberData(nameof(BinaryOperatorData.Modulo_unsupported_types), MemberType = typeof(BinaryOperatorData))]
        public void with_unsupported_types_emits_expected_error(string i, string j, string expectedResult)
        {
            string source = $@"
                    print {i} % {j};
                ";

            // TODO: Should be validation errors, not runtime errors.
            var result = EvalWithRuntimeErrorCatch(source);

            result.Errors.Should()
                .ContainSingle().Which
                .Message.Should().Match(expectedResult);
        }

        [Theory]
        [ClassData(typeof(TestCultures))]
        public async Task modulo_operation_works_on_different_cultures(CultureInfo cultureInfo)
        {
            CultureInfo.CurrentCulture = cultureInfo;

            string source = @"
                12.34 % 0.3
            ";

            object result = Eval(source);

            // IEEE 754... :-)
            Assert.Equal(0.04000000000000031, result);
        }

        [Theory]
        [ClassData(typeof(TestCultures))]
        public async Task modulo_operation_combined_with_others_without_grouping(CultureInfo cultureInfo)
        {
            CultureInfo.CurrentCulture = cultureInfo;

            string source = @"
                2 * 5 / 10 * 4 % 2.1
            ";

            object result = Eval(source);

            Assert.Equal(1.9, result);
        }

        [Theory]
        [ClassData(typeof(TestCultures))]
        public async Task modulo_operation_combined_with_others_with_grouping_first_operators(CultureInfo cultureInfo)
        {
            CultureInfo.CurrentCulture = cultureInfo;

            string source = @"
                (2 * 5 / 10 * 4) % 2.1
            ";

            object result = Eval(source);

            Assert.Equal(1.9, result);
        }

        [Theory]
        [ClassData(typeof(TestCultures))]
        public async Task modulo_operation_combined_with_others_with_grouping_last_operators(CultureInfo cultureInfo)
        {
            CultureInfo.CurrentCulture = cultureInfo;

            string source = @"
                2 * 5 / 10 * (4 % 2.1)
            ";

            object result = Eval(source);

            Assert.Equal(1.9, result);
        }

        [Fact]
        public void modulo_operation_on_non_number_with_number_throws_expected_error()
        {
            string source = @"
                ""1"" % 1;
            ";

            var result = EvalWithValidationErrorCatch(source);
            var exception = result.Errors.FirstOrDefault();

            Assert.Single(result.Errors);
            Assert.Equal("Unsupported % operands specified: string and int", exception.Message);
        }

        [Fact]
        public void modulo_operation_on_number_with_non_number_throws_expected_error()
        {
            string source = @"
                1 % ""1"";
            ";

            var result = EvalWithValidationErrorCatch(source);
            var exception = result.Errors.FirstOrDefault();

            Assert.Single(result.Errors);
            Assert.Equal("Unsupported % operands specified: int and string", exception.Message);
        }
    }
}
