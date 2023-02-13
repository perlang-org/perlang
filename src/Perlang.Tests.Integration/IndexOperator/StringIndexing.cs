using FluentAssertions;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.IndexOperator;

public class StringIndexing
{
    [Fact]
    public void string_can_be_indexed_by_integer()
    {
        string source = @"
            print ""foobar""[0];
        ";

        var output = EvalReturningOutputString(source);

        output.Should().Be("f");
    }

    [Fact]
    public void string_indexed_by_integer_returns_char_object()
    {
        string source = @"
            print ""foobar""[0].get_type();
        ";

        var output = EvalReturningOutputString(source);

        output.Should().Be("System.Char");
    }

    [Fact]
    public void string_indexed_by_integer_assigned_to_other_type_variable_throws_expected_error()
    {
        // This is expected to fail, since an individual element of a string is `char`
        string source = @"
            var s: string = ""foobar""[0];
        ";

        var result = EvalWithValidationErrorCatch(source);

        result.Errors.Should()
            .ContainSingle().Which
            .Message.Should().Match("Cannot assign System.Char to string variable");
    }

    [Fact]
    public void string_indexed_by_string_throws_expected_error()
    {
        string source = @"
            print ""foobar""[""baz""];
        ";

        var result = EvalWithValidationErrorCatch(source);

        result.Errors.Should()
            .ContainSingle()
            .Which
            .Message.Should().Contain("cannot be indexed by 'string'");
    }
}
