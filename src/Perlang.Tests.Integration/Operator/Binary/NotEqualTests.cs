using FluentAssertions;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Operator.Binary;

public class NotEqualTests
{
    [SkippableTheory]
    [MemberData(nameof(BinaryOperatorData.NotEqual), MemberType = typeof(BinaryOperatorData))]
    void performs_non_equality_comparison(string i, string j, string expectedResult)
    {
        string source = $@"
                var i1 = {i};
                var i2 = {j};

                print i1 != i2;
            ";

        string result = EvalReturningOutputString(source);

        result
            .ToLower()
            .Should()
            .Be(expectedResult);
    }

    [Theory]
    [InlineData("Foo", "Bar", "true")]
    [InlineData("Foo", "foo", "true")] // Comparison is case sensitive
    [InlineData("foo", "foo", "false")]
    void strings_can_be_compared_for_equality(string i, string j, string expectedResult)
    {
        string source = $@"
                var i1 = ""{i}"";
                var i2 = ""{j}"";

                print i1 != i2;
            ";

        string result = EvalReturningOutputString(source);

        result
            .ToLower()
            .Should()
            .Be(expectedResult);
    }

    // TODO: unsupported_types_emits_expected_errors
}
