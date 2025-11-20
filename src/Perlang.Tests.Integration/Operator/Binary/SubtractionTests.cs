using FluentAssertions;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Operator.Binary;

public class SubtractionTests
{
    [Theory]
    [MemberData(nameof(BinaryOperatorData.Subtraction_result), MemberType = typeof(BinaryOperatorData))]
    private void performs_subtraction(string i, string j, string expectedResult)
    {
        string source = $@"
            var i1 = {i};
            var i2 = {j};

            print i1 - i2;
        ";

        string result = EvalReturningOutputString(source);

        result.Should()
            .Be(expectedResult);
    }

    [Theory]
    [MemberData(nameof(BinaryOperatorData.Subtraction_type), MemberType = typeof(BinaryOperatorData))]
    private void with_supported_types_returns_expected_type(string i, string j, string expectedType)
    {
        string source = $@"
            var i1 = {i};
            var i2 = {j};

            print (i1 - i2).get_type();
        ";

        string result = EvalReturningOutputString(source);

        result.Should()
            .Be(expectedType);
    }

    [Theory]
    [MemberData(nameof(BinaryOperatorData.Subtraction_unsupported_types), MemberType = typeof(BinaryOperatorData))]
    public void with_unsupported_types_emits_expected_error(string i, string j, string expectedError)
    {
        string source = $@"
            print {i} - {j};
        ";

        var result = EvalWithValidationErrorCatch(source);

        result.Errors.Should()
            .ContainSingle().Which
            .Message.Should().Match(expectedError);
    }

    [Fact]
    private void subtraction_of_strings_throws_expected_error()
    {
        string source = @"
            var s1 = ""foo"";
            var s2 = ""bar"";

            print s1 - s2;
        ";

        var result = EvalWithValidationErrorCatch(source);

        result.Errors.Should()
            .ContainSingle().Which
            .Message.Should().Match("Unsupported - operand types: 'ASCIIString' and 'ASCIIString'");
    }
}
