using FluentAssertions;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Operator.Binary;

public class GreaterEqualTests
{
    [Theory]
    [MemberData(nameof(BinaryOperatorData.GreaterEqual), MemberType = typeof(BinaryOperatorData))]
    void performs_greater_than_or_equal_comparison(string i, string j, string expectedResult)
    {
        string source = $@"
                var i1 = {i};
                var i2 = {j};

                print i1 >= i2;
            ";

        string result = EvalReturningOutputString(source);

        result.Should()
            .Be(expectedResult);
    }

    [Theory]
    [MemberData(nameof(BinaryOperatorData.GreaterEqual_unsupported_types), MemberType = typeof(BinaryOperatorData))]
    void with_unsupported_types_emits_expected_error(string i, string j, string expectedError)
    {
        string source = $@"
                var i1 = {i};
                var i2 = {j};

                print i1 >= i2;
            ";

        var result = EvalWithValidationErrorCatch(source);

        result.Errors.Should()
            .ContainSingle().Which
            .Message.Should().Match(expectedError);
    }
}
