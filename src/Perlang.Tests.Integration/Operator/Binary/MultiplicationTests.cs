using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Operator.Binary;

// Tests based on the following:
// https://github.com/munificent/craftinginterpreters/blob/adfb64d00d04e389ddc086d16cc29ac49fc6c21e/test/operator/multiply.lox
// https://github.com/munificent/craftinginterpreters/blob/adfb64d00d04e389ddc086d16cc29ac49fc6c21e/test/operator/multiply_nonnum_num.lox
// https://github.com/munificent/craftinginterpreters/blob/adfb64d00d04e389ddc086d16cc29ac49fc6c21e/test/operator/multiply_num_nonnum.lox
public class MultiplicationTests
{
    //
    // Tests for the * (multiplication) operator
    //
    [Theory]
    [MemberData(nameof(BinaryOperatorData.Multiplication_result), MemberType = typeof(BinaryOperatorData))]
    public void performs_multiplication(string i, string j, string expectedResult)
    {
        string source = $@"
                print {i} * {j};
            ";

        string result = EvalReturningOutputString(source);

        result.Should()
            .Be(expectedResult);
    }

    [SkippableTheory]
    [MemberData(nameof(BinaryOperatorData.Multiplication_type), MemberType = typeof(BinaryOperatorData))]
    public void with_supported_types_returns_expected_type(string i, string j, string expectedResult)
    {
        Skip.If(PerlangMode.ExperimentalCompilation, "get_type() is not yet supported in compiled mode");

        string source = $@"
                print ({i} * {j}).get_type();
            ";

        string result = EvalReturningOutputString(source);

        result.Should()
            .Be(expectedResult);
    }

    [Theory]
    [MemberData(nameof(BinaryOperatorData.Multiplication_unsupported_types), MemberType = typeof(BinaryOperatorData))]
    public void with_unsupported_types_emits_expected_error(string i, string j, string expectedResult)
    {
        string source = $@"
                    print {i} * {j};
                ";

        var result = EvalWithValidationErrorCatch(source);

        result.Errors.Should()
            .ContainSingle().Which
            .Message.Should().Match(expectedResult);
    }

    [SkippableTheory]
    [ClassData(typeof(TestCultures))]
    public async Task multiplying_doubles_works_on_different_cultures(CultureInfo cultureInfo)
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
        Assert.Equal("Unsupported * operand types: 'ASCIIString' and 'int'", exception.Message);
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
        Assert.Equal("Unsupported * operand types: 'int' and 'ASCIIString'", exception.Message);
    }
}