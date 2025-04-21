using FluentAssertions;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Typing;

public class CharTests
{
    [Fact]
    public void char_variable_can_be_printed()
    {
        string source = """
            var c: char = 'x';

            print(c);
            """;

        var output = EvalReturningOutputString(source);

        Assert.Equal("x", output);
    }

    [Fact]
    public void char_variable_can_contain_utf16_character()
    {
        string source = """
            // The UTF16 part in the test name is important; chars do not support UCS-4/UTF-32 characters like
            // emojis.
            var c: char = 'ø';

            print(c);
            """;

        var output = EvalReturningOutputString(source);

        Assert.Equal("ø", output);
    }

    [Fact]
    public void char_variable_cannot_contain_non_bmp_character()
    {
        string source = """
            // This kind of UTF character is too wide for a 16-bit char, and is currently not supported in character
            // literals.
            var c: char = '🎉';

            print(c);
            """;

        var result = EvalWithScanErrorCatch(source);

        result.Errors.Should()
            .ContainSingle().Which
            .Message.Should().Match("Character literal can only contain characters from the Basic Multilingual Plane");
    }

    [Fact]
    public void char_literal_supports_linefeed_escape_sequence()
    {
        string source = """
            var c: char = '\n';

            print(c);
            """;

        var output = EvalReturningOutputString(source);

        Assert.Equal("\n", output);
    }

    [Fact]
    public void char_literal_supports_carriage_return_escape_sequence()
    {
        string source = """
            var c: char = '\r';

            print(c);
            """;

        var output = EvalReturningOutputString(source);

        // This is expected to be empty, since \r does not output anything, it only moves the cursor.
        Assert.Equal("", output);
    }

    [Fact]
    public void char_literal_supports_tab_escape_sequence()
    {
        string source = """
            var c: char = '\t';

            print(c);
            """;

        var output = EvalReturningOutputString(source);

        Assert.Equal("\t", output);
    }

    [Fact]
    public void char_literal_supports_nul_escape_sequence()
    {
        string source = """
            var c: char = '\0';

            print(c);
            """;

        var output = EvalReturningOutputString(source);

        // This is expected to be empty, since \0 is unprintable by printf("%c", c)
        Assert.Equal("", output);
    }

    [Fact]
    public void empty_character_literal_throws_expected_error()
    {
        string source = """
            var c: char = '';
            """;

        var result = EvalWithScanErrorCatch(source);

        result.Errors.Should()
            .ContainSingle().Which
            .Message.Should().Contain("Character literal cannot be empty");
    }

    [Fact]
    public void unsupported_octal_escape_sequence_throws_expected_error()
    {
        // Octal character literals are not supported, just like in C#, apart for the `\0` sequence for convenience.
        // There is little reason for us to add support for it, since we (unlike C++) don't need to be backward
        // compatible with C.
        string source = """
            var c: char = '\033';
            """;

        var result = EvalWithScanErrorCatch(source);

        result.Errors.Should()
            .ContainSingle().Which
            .Message.Should().Be("Unsupported escape sequence: \\033.");
    }

    [Fact]
    public void unsupported_hexadecimal_escape_sequence_throws_expected_error()
    {
        string source = """
            var c: char = '\x1B';
            """;

        var result = EvalWithScanErrorCatch(source);

        result.Errors.Should()
            .ContainSingle().Which
            .Message.Should().Be("Unsupported escape sequence: \\x1B.");
    }
}
