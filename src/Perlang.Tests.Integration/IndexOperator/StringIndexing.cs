using FluentAssertions;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.IndexOperator;

public class StringIndexing
{
    [Fact]
    public void string_variable_indexing_emits_validation_error()
    {
        // As for UTF-8 strings, this cannot work because we don't *know* whether a `string` variable can be indexed or
        // not; it could be a UTF-8 string. However, this uglifies the language _significantly_ to the point where I'm
        // even considering changing `UTF8String` to not be a `string`... It is perhaps best to treat it as its own
        // animal, and just make it easy to convert to/from `UTF16String` when needed. The really sad part about this
        // though is that it hampers the usage of UTF-8 strings in Perlang greatly...
        string source = """
            var s: string;
            s = "foobar";
            print s[1];
            """;

        var result = EvalWithValidationErrorCatch(source);

        result.Errors.Should()
            .ContainSingle()
            .Which
            .Message.Should().Contain("operation not supported");
    }

    [Fact]
    public void ASCIIString_literal_can_be_indexed_by_integer()
    {
        string source = """
            print "foobar"[0];
            """;

        var output = EvalReturningOutputString(source);

        output.Should().Be("f");
    }

    [Fact]
    public void ASCIIString_literal_indexed_outside_string_returns_expected_error()
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
            .Contain("Index 10 is out-of-bounds for a string with length 6 (valid range: 0..5)");
    }

    [Fact]
    public void UTF8String_indexing_emits_validation_error()
    {
        // This doesn't work, because strings with non-ASCII characters becomes UTF8Strings. And think about it. What
        // would indexing a UTF8String mean? It doesn't really "mean" anything. One interesting thing to work around this
        // would be to convert the string to UTF-16 internally, the first time it is attempted to be indexed. But we need
        // a length() method which returns the number of UTF-16 code units, in that case. See
        // https://gitlab.perlang.org/perlang/perlang/-/issues/370 for some discussion around this.
        string source = """
            print "åäöÅÄÖéèüÜÿŸïÏすし"[0];
            """;

        var result = EvalWithValidationErrorCatch(source);

        result.Errors.Should()
            .ContainSingle()
            .Which
            .Message.Should().Contain("operation not supported");
    }

    [Fact]
    public void UTF16String_variable_can_be_indexed_by_integer()
    {
        string source = """
            var s: UTF16String = "åäöÅÄÖéèüÜÿŸïÏすし".as_utf16();
            print s[14];
            """;

        var output = EvalReturningOutputString(source);

        output.Should().Be("す");
    }

    [Fact]
    public void UTF16String_literal_can_be_indexed_by_integer()
    {
        string source = """
            print "åäöÅÄÖéèüÜÿŸïÏすし".as_utf16()[14];
            """;

        var output = EvalReturningOutputString(source);

        output.Should().Be("す");
    }

    [Fact]
    public void UTF16String_literal_indexed_outside_string_returns_expected_error()
    {
        string source = """
            print "åäöÅÄÖéèüÜÿŸïÏすし".as_utf16()[100];
            """;

        var result = EvalWithRuntimeErrorCatch(source);

        result.Errors.Should()
            .ContainSingle()
            .Which
            .Message.Should().Contain("exited with exit code 134");

        result.OutputAsString.Should()
            .Contain("Index 100 is out-of-bounds for a string with length 16 (valid range: 0..15)");
    }

    [SkippableFact]
    public void ASCIIString_indexed_by_integer_returns_char_object()
    {
        Skip.If(PerlangMode.ExperimentalCompilation, "get_type() for 'char' types is not yet supported in compiled mode");

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
            .Message.Should().Match("Cannot assign char to string variable");
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
            .Message.Should().Contain("cannot be indexed by 'ASCIIString'");
    }
}
