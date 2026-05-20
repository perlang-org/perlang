using FluentAssertions;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Stdlib;

public class CharClassTests
{
    [Fact]
    public void Char_to_upper_returns_expected_value_for_alphabetic_ASCII_character()
    {
        string source = """
            var c: char = 'x';

            print(Char.to_upper(c));
            """;

        var output = EvalReturningOutputString(source);

        output.Should()
            .Be("X");
    }

    [Fact]
    public void Char_to_upper_returns_expected_value_for_non_alphabetic_ASCII_character()
    {
        string source = """
            var c: char = '!';

            print(Char.to_upper(c));
            """;

        var output = EvalReturningOutputString(source);

        output.Should()
            .Be("!");
    }

    [Fact]
    public void Char_to_upper_returns_expected_value_for_UTF16_character()
    {
        string source = """
            var c: char = 'ø';

            print(Char.to_upper(c));
            """;

        var output = EvalReturningOutputString(source);

        // Note: this assertion depends on the current locale supporting Unicode. LANG=en_US.UTF-8 works; LANG=en_US
        // does not. It will be interesting to test how programs like this will behave on e.g. FreeBSD.
        output.Should()
            .Be("Ø");
    }

    [Fact]
    public void Char_to_lower_returns_expected_value_for_alphabetic_ASCII_character()
    {
        string source = """
            var c: char = 'Z';

            print(Char.to_lower(c));
            """;

        var output = EvalReturningOutputString(source);

        output.Should()
            .Be("z");
    }

    [Fact]
    public void Char_to_lower_returns_expected_value_for_non_alphabetic_ASCII_character()
    {
        string source = """
            var c: char = '?';

            print(Char.to_lower(c));
            """;

        var output = EvalReturningOutputString(source);

        output.Should()
            .Be("?");
    }

    [Fact]
    public void Char_to_lower_returns_expected_value_for_UTF16_character()
    {
        string source = """
            var c: char = 'Ä';

            print(Char.to_lower(c));
            """;

        var output = EvalReturningOutputString(source);

        // Note: this assertion depends on the current locale supporting Unicode. LANG=en_US.UTF-8 works; LANG=en_US
        // does not. It will be interesting to test how programs like this will behave on e.g. FreeBSD.
        output.Should()
            .Be("ä");
    }
}
