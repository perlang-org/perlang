using FluentAssertions;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Operator;

/// <summary>
/// Tests for the unary prefix - (negation) operator.
/// </summary>
public class PrefixNegation
{
    [Fact]
    public void negation_of_negative_int()
    {
        string source = @"
                - -100
            ";

        object result = Eval(source);

        result.Should()
            .Be(100);
    }

    [Fact]
    public void negation_of_positive_int()
    {
        // Note: there is a certain logic in the parsing of the unary prefix operator, converting "- 123" into a single
        // literal expression. To ensure that this logic is circumvented, we add grouping parentheses.
        string source = @"
                -(123)
            ";

        object result = Eval(source);

        result.Should()
            .Be(-123);
    }

    [Fact]
    public void negation_of_negative_int_without_space_throws_expected_error()
    {
        string source = @"
                --100
            ";

        var result = EvalWithParseErrorCatch(source);

        result.Errors.Should()
            .ContainSingle().Which
            .Message.Should().Contain("Expect expression");
    }
}
