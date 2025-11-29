using FluentAssertions;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.In;

public class InTests
{
    [Theory]
    [InlineData("int", "123, 456, 789", "456", "true")]
    [InlineData("long", "123, 456, 789", "456", "true")]
    [InlineData("uint", "123, 456, 789", "456", "true")]
    [InlineData("ulong", "123, 456, 789", "456", "true")]
    [InlineData("float", "123, 456, 789", "456f", "true")]
    [InlineData("double", "123, 456, 789", "456d", "true")]
    [InlineData("bool", "true, false, true", "true", "true")]
    [InlineData("char", "'e', 'n', 'i', 'a', 'c'", "'c'", "true")]
    [InlineData("bigint", "123, 456, 789", "456", "true")]
    public void can_use_in_operator_with_array(string type, string collectionInitializer, string needle, string expectedOutput)
    {
        string source = $"""
            var a: {type}[] = [{collectionInitializer}];

            print {needle} in a;
            """;

        var output = EvalReturningOutput(source);

        output.Should()
            .BeEquivalentTo(expectedOutput);
    }

    [Fact]
    public void can_use_in_operator_with_string_array()
    {
        string source = """
            var a: string[] = ["one", "two", "three"];

            print "two" in a;
            """;

        var output = EvalReturningOutput(source);

        output.Should()
            .BeEquivalentTo(
                "true"
            );
    }

    // TODO: Add test for 'object[]' too, containing a user-defined class

    [Fact]
    public void in_operator_returns_expected_result_when_int_array_is_null()
    {
        string source = """
            var a: int[] = null;

            // A 'null' array does not contain any items => should evaluate to 'false'
            print 456 in a;
            """;

        var result = EvalWithResult(source);

        result.Output.Should()
            .BeEquivalentTo(
                "false"
            );
    }

    [Fact]
    public void in_operator_returns_expected_result_when_string_array_is_null()
    {
        string source = """
            var a: string[] = null;

            // A 'null' array will never contain any items => should evaluate to 'false'
            print "two" in a;
            """;

        var result = EvalWithResult(source);

        result.Output.Should()
            .BeEquivalentTo(
                "false"
            );
    }

    [Fact]
    public void in_operator_returns_expected_result_when_operand_types_do_not_match()
    {
        string source = """
            var a: int[] = [1, 2, 3];

            print "foo" in a;
            """;

        var result = EvalWithValidationErrorCatch(source);

        result.Errors.Should()
            .ContainSingle().Which
            .Message.Should().Match("Cannot search in 'int[]' haystack for needle of type 'ASCIIString'");
    }

    [Fact]
    public void in_operator_returns_expected_result_when_right_hand_operand_is_not_an_array()
    {
        string source = """
            var i: int = 1;

            print 1 in i;
            """;

        var result = EvalWithValidationErrorCatch(source);

        result.Errors.Should()
            .ContainSingle().Which
            .Message.Should().Match("Cannot search in 'int' haystack: type is not an array type");
    }
}
