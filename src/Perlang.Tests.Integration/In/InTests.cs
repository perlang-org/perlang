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

    // TODO: Add array test for 'object[]' too, containing a user-defined class

    [Theory]
    [InlineData("int", "1", "100", "50", "true")]
    [InlineData("int", "1", "100", "-100", "false")]
    [InlineData("int", "1", "100", "0", "false")]
    [InlineData("int", "1", "100", "123", "false")]
    [InlineData("long", "1", "100", "50", "true")]
    [InlineData("long", "1", "100", "-100", "false")]
    [InlineData("long", "1", "100", "0", "false")]
    [InlineData("long", "1", "100", "123", "false")]
    [InlineData("uint", "1", "100", "50", "true")]
    [InlineData("uint", "1", "100", "0", "false")]
    [InlineData("uint", "1", "100", "123", "false")]
    [InlineData("ulong", "1", "100", "50", "true")]
    [InlineData("ulong", "1", "100", "0", "false")]
    [InlineData("ulong", "1", "100", "123", "false")]
    [InlineData("char", "'a'", "'z'", "'j'", "true")]
    [InlineData("char", "'a'", "'c'", "'d'", "false")]
    [InlineData("bigint", "18446744073709551616", "19446744073709551616", "18446744073709551617", "true")]
    [InlineData("bigint", "18446744073709551616", "19446744073709551616", "18446744073709551616", "true")]
    [InlineData("bigint", "18446744073709551616", "19446744073709551616", "19446744073709551616", "true")]
    [InlineData("bigint", "18446744073709551616", "19446744073709551616", "-100", "false")]
    [InlineData("bigint", "18446744073709551616", "19446744073709551616", "1", "false")]
    public void can_use_in_operator_with_range(string type, string begin, string end, string needle, string expectedResult)
    {
        string source = $"""
            // Workaround for the fact that e.g. the integer literal 50
            // cannot be implicitly used when searching in a "uint" range.
            // When we have casting and/or support for "50.to_uint()"-style
            // conversions, we can get rid of it.
            var needle: {type} = {needle};
            var begin: {type} = {begin};
            var end: {type} = {end};

            print needle in begin..end;
            """;

        var output = EvalReturningOutput(source);

        output.Should()
            .BeEquivalentTo(
                expectedResult
            );
    }

    [Fact]
    public void in_operator_emits_expected_error_when_range_begin_and_end_types_are_not_compatible()
    {
        string source = """
            print 10 in 'a'..65;
            """;

        var result = EvalWithValidationErrorCatch(source);

        result.Errors.Should()
            .ContainSingle().Which
            .Message.Should().Match("Both sides of the range must be of equal types (not 'char' and 'int')");
    }

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
    public void in_operator_emits_expected_error_when_operand_types_do_not_match()
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
    public void in_operator_emits_expected_error_when_right_hand_operand_is_not_an_array()
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
