using FluentAssertions;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.IndexOperator;

public class StringIndexing
{
    [Fact]
    public void AsciiString_can_be_indexed_by_integer()
    {
        string source = @"
            print ""foobar""[0];
        ";

        var output = EvalReturningOutputString(source);

        output.Should().Be("f");
    }

    [SkippableFact]
    public void AsciiString_indexed_outside_string_returns_expected_error()
    {
        // The reason why we can't easily support this yet is that -Warray-bounds returns a compile-time error for this in
        // Clang (tested with Clang 14). We would need to be able to have an EvalHelper helper methods for checking ICE
        // errors to be able to deal with it there; EvalWithRuntimeErrorCatch wouldn't help (and is not supported in
        // compiled mode yet)
        Skip.If(PerlangMode.ExperimentalCompilation, "Not yet supported in compiled mode");

        string source = @"
            print ""foobar""[10];
        ";

        var result = EvalWithRuntimeErrorCatch(source);

        result.Errors.Should()
            .ContainSingle()
            .Which
            .Message.Should().Contain("Index was outside the bounds of the array");
    }

    [SkippableFact]
    public void AsciiString_indexed_by_integer_returns_char_object()
    {
        Skip.If(PerlangMode.ExperimentalCompilation, "Not supported in compiled mode");

        string source = @"
            print ""foobar""[0].get_type();
        ";

        var output = EvalReturningOutputString(source);

        // TODO: Ideally this would be a Perlang `char` type, which is printable as a character but is 8-bit wide. It
        // TODO: would only be applicable to `AsciiString` instances though, so its usefulness would be limited...
        output.Should().Be("System.Char");
    }

    [Fact]
    public void AsciiString_indexed_by_integer_assigned_to_other_type_variable_throws_expected_error()
    {
        // This is expected to fail, since an individual element of an AsciiString is `byte`
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
            .Message.Should().Contain("cannot be indexed by 'AsciiString'");
    }
}
