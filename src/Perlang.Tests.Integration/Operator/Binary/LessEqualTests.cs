using FluentAssertions;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Operator.Binary;

public class LessEqualTests
{
    [SkippableTheory]
    [MemberData(nameof(BinaryOperatorData.LessEqual), MemberType = typeof(BinaryOperatorData))]
    void performs_less_equal_comparison(string i, string j, string expectedResult)
    {
        string source = $@"
                var i1 = {i};
                var i2 = {j};

                print i1 <= i2;
            ";

        string result = EvalReturningOutputString(source)
            .ToLower();

        result.Should()
            .Be(expectedResult);
    }

    [Theory]
    [MemberData(nameof(BinaryOperatorData.LessEqual_unsupported_types), MemberType = typeof(BinaryOperatorData))]
    public void with_unsupported_types_emits_expected_error(string i, string j, string expectedError)
    {
        string source = $@"
                var i1 = {i};
                var i2 = {j};

                print i1 <= i2;
            ";

        var result = EvalWithValidationErrorCatch(source);

        result.Errors.Should()
            .ContainSingle().Which
            .Message.Should().Match(expectedError);
    }
}
