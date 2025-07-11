#pragma warning disable S4144 // Methods should not have identical implementations
using FluentAssertions;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Operator.Binary;

public class EqualTests
{
    [Theory]
    [MemberData(nameof(BinaryOperatorData.Equal), MemberType = typeof(BinaryOperatorData))]
    private void performs_equality_comparison(string i, string j, string expectedResult)
    {
        string source = $"""
            var i1 = {i};
            var i2 = {j};

            print i1 == i2;
            """;

        string result = EvalReturningOutputString(source)
            .ToLower();

        result.Should()
            .Be(expectedResult);
    }

    [Theory]
    [InlineData("\"Foo\"", "\"Bar\"", "false")]
    [InlineData("\"Foo\"", "\"foo\"", "false")] // Comparison is case sensitive
    [InlineData("\"foo\"", "\"foo\"", "true")]
    [InlineData("\"fo\" + \"o\"", "\"foo\"", "true")] // Force non-equal string literals to ensure dynamic strings can also be equality-compared
    [InlineData("\"fö\" + \"ö\"", "\"föö\"", "true")] // Force non-equal string literals to ensure dynamic strings can also be equality-compared
    private void strings_can_be_compared_for_equality(string i, string j, string expectedResult)
    {
        string source = $"""
            var i1 = {i};
            var i2 = {j};

            print i1 == i2;
            """;

        // "True" in interpreted mode vs "true" in compiled mode
        string result = EvalReturningOutputString(source)
            .ToLower();

        result.Should()
            .Be(expectedResult);
    }
}
