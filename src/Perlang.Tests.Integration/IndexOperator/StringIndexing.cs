using FluentAssertions;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.IndexOperator;

public class StringIndexing
{
    [Fact]
    public void ASCIIString_can_be_indexed_by_integer()
    {
        string source = """
            print "foobar"[0];
            """;

        var output = EvalReturningOutputString(source);

        output.Should().Be("f");
    }

    [Fact]
    public void ASCIIString_indexed_outside_string_returns_expected_error()
    {
        string source = """
            print "foobar"[10];
            """;

        var result = EvalWithRuntimeErrorCatch(source);

        result.Errors.Should()
            .ContainSingle()
            .Which
            .Message.Should().Contain("exited with exit code 134");

        result.OutputAsString.Should()
            .Contain("Index 10 is out-of-bounds for a string with length 6");
    }

    [Fact(Skip = "Not yet supported for UTF8String")]
    public void UTF8String_indexed_outside_string_returns_expected_error()
    {
        string source = """
            print "åäöÅÄÖéèüÜÿŸïÏすし"[10];
            """;

        var result = EvalWithRuntimeErrorCatch(source);

        result.Errors.Should()
            .ContainSingle()
            .Which
            .Message.Should().Contain("exited with exit code 134");

        result.OutputAsString.Should()
            .Contain("Index 10 is out-of-bounds for a string with length 6");
    }

    [SkippableFact]
    public void ASCIIString_indexed_by_integer_returns_char_object()
    {
        Skip.If(PerlangMode.ExperimentalCompilation, "get_type() is not yet supported in compiled mode");

        string source = """
            print "foobar"[0].get_type();
            """;

        var output = EvalReturningOutputString(source);

        // TODO: Ideally this would be a Perlang `char` type, which is printable as a character but is 8-bit wide. It
        // TODO: would only be applicable to `AsciiString` instances though, so its usefulness would be limited...
        output.Should().Be("System.Char");
    }

    [Fact]
    public void ASCIIString_indexed_by_integer_assigned_to_other_type_variable_throws_expected_error()
    {
        // This is expected to fail, since an individual element of an AsciiString is `char`
        string source = """
            var s: string = "foobar"[0];
            """;

        var result = EvalWithValidationErrorCatch(source);

        result.Errors.Should()
            .ContainSingle().Which
            .Message.Should().Match("Cannot assign System.Char to string variable");
    }

    [Fact]
    public void ASCIIString_indexed_by_string_throws_expected_error()
    {
        string source = """
            print "foobar"["baz"];
            """;

        var result = EvalWithValidationErrorCatch(source);

        result.Errors.Should()
            .ContainSingle()
            .Which
            .Message.Should().Contain("cannot be indexed by 'AsciiString'");
    }
}
