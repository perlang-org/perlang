using FluentAssertions;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Operator.Binary;

public class EqualTests
{
    [Theory]
    [MemberData(nameof(BinaryOperatorData.Equal), MemberType = typeof(BinaryOperatorData))]
    void performs_equality_comparison(string i, string j, string expectedResult)
    {
        string source = $@"
                var i1 = {i};
                var i2 = {j};

                print i1 == i2;
            ";

        string result = EvalReturningOutputString(source);

        result.Should()
            .Be(expectedResult);
    }

    [Theory]
    [InlineData("Foo", "Bar", "False")]
    [InlineData("Foo", "foo", "False")] // Comparison is case sensitive
    [InlineData("foo", "foo", "True")]
    void strings_can_be_compared_for_equality(string i, string j, string expectedResult)
    {
        string source = $@"
                var i1 = ""{i}"";
                var i2 = ""{j}"";

                print i1 == i2;
            ";

        string result = EvalReturningOutputString(source);

        result.Should()
            .Be(expectedResult);
    }
}
