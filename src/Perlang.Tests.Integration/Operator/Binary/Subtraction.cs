using FluentAssertions;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Operator.Binary;

public class Subtraction
{
    [Theory]
    [MemberData(nameof(BinaryOperatorData.Minus_result), MemberType = typeof(BinaryOperatorData))]
    void performs_subtraction(string i, string j, string expectedResult)
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
    [MemberData(nameof(BinaryOperatorData.Minus_type), MemberType = typeof(BinaryOperatorData))]
    void with_supported_types_returns_expected_type(string i, string j, string expectedType)
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
    [MemberData(nameof(BinaryOperatorData.Minus_unsupported_types), MemberType = typeof(BinaryOperatorData))]
    public void with_unsupported_types_emits_expected_error(string i, string j, string expectedError)
    {
        string source = $@"
            print {i} - {j};
        ";

        // TODO: Should definitely not be a runtime-error, but rather caught in the validation phase.
        var result = EvalWithRuntimeErrorCatch(source);

        result.Errors.Should()
            .ContainSingle().Which
            .Message.Should().Match(expectedError);
    }

    [Fact]
    void subtraction_of_strings_throws_expected_error()
    {
        string source = @"
            var s1 = ""foo"";
            var s2 = ""bar"";

            print s1 - s2;
        ";

        var result = EvalWithValidationErrorCatch(source);

        result.Errors.Should()
            .ContainSingle().Which
            .Message.Should().Match("Unsupported - operands specified: string and string");
    }
}
