using FluentAssertions;
using Xunit;

namespace Perlang.Tests.Integration.Typing;

using static EvalHelper;

public class StringTests
{
    [Fact]
    public void string_variable_can_be_printed()
    {
        string source = """
            var s: string = "this is a string";

            print(s);
            """;

        var output = EvalReturningOutputString(source);

        output.Should()
            .Be("this is a string");
    }

    [Fact]
    public void string_variable_can_be_reassigned()
    {
        string source = """
            var s: string = "this is a string";
            s = "this is another string";

            print(s);
            """;

        var output = EvalReturningOutputString(source);

        output.Should()
            .Be("this is another string");
    }

    [Fact]
    public void ascii_string_inferred_variable_can_be_reassigned_with_non_ascii_value()
    {
        string source = """
            var s: string = "this is a string";
            s = "this is a string with non-ASCII characters: åäöÅÄÖéèüÜÿŸïÏすし";

            print(s);
            """;

        var output = EvalReturningOutputString(source);

        output.Should()
            .Be("this is a string with non-ASCII characters: åäöÅÄÖéèüÜÿŸïÏすし");
    }

    [Fact]
    public void non_ascii_string_inferred_variable_can_be_reassigned_with_ascii_value()
    {
        // Same as the ASCIIString to UTF8String above, but the other way around
        string source = """
            var s: string = "this is a string with non-ASCII characters: åäöÅÄÖéèüÜÿŸïÏすし";
            s = "this is a string";

            print(s);
            """;

        var output = EvalReturningOutputString(source);

        output.Should()
            .Be("this is a string");
    }

    [Fact]
    public void ascii_string_and_ascii_string_variable_can_be_concatenated()
    {
        string source = """
            var s1: string = "this is s1";
            var s2: string = " and this is s2";
            var s3: string = s1 + s2;

            print(s3);
            """;

        var output = EvalReturningOutputString(source);

        output.Should()
            .Be("this is s1 and this is s2");
    }

    [Fact]
    public void ascii_string_and_ascii_string_literal_can_be_concatenated()
    {
        string source = """
            var s1: string = "this is s1";
            var s2: string = "and this is s2";
            var s3: string = s1 + " " + s2;

            print(s3);
            """;

        var output = EvalReturningOutputString(source);

        output.Should()
            .Be("this is s1 and this is s2");
    }

    [Fact]
    public void ascii_string_and_utf8_string_variable_can_be_concatenated()
    {
        string source = """
            var s1: string = "this is s1";
            var s2: string = " and this is s2 with non-ASCII characters: åäöÅÄÖéèüÜÿŸïÏすし";
            var s3: string = s1 + s2;

            print(s3);
            """;

        var output = EvalReturningOutputString(source);

        output.Should()
            .Be("this is s1 and this is s2 with non-ASCII characters: åäöÅÄÖéèüÜÿŸïÏすし");
    }

    [Fact]
    public void ascii_string_and_utf8_string_literal_can_be_concatenated()
    {
        string source = """
            var s1: string = "this is s1";
            var s3: string = s1 + " and this is s2 with non-ASCII characters: åäöÅÄÖéèüÜÿŸïÏすし";

            print(s3);
            """;

        var output = EvalReturningOutputString(source);

        output.Should()
            .Be("this is s1 and this is s2 with non-ASCII characters: åäöÅÄÖéèüÜÿŸïÏすし");
    }

    [Fact]
    public void ascii_string_literal_and_string_variable_can_be_concatenated()
    {
        string source = """
            var s2: string = "s2";
            var s3 = "s1 and " + s2;

            print(s3);
            """;

        var output = EvalReturningOutputString(source);

        output.Should()
            .Be("s1 and s2");
    }

    [Fact]
    public void string_variable_and_ascii_string_literal_can_be_concatenated()
    {
        string source = """
            var s1: string = "s1";
            var s3 = s1 + " and s2";

            print(s3);
            """;

        var output = EvalReturningOutputString(source);

        output.Should()
            .Be("s1 and s2");
    }

    [Fact]
    public void utf8_string_literal_and_string_variable_can_be_concatenated()
    {
        string source = """
            var s2: string = "ASCII string";
            var s3 = "åäöÅÄÖéèüÜÿŸïÏすし and " + s2;

            print(s3);
            """;

        var output = EvalReturningOutputString(source);

        output.Should()
            .Be("åäöÅÄÖéèüÜÿŸïÏすし and ASCII string");
    }

    [Fact]
    public void string_variable_and_utf8_string_literal_can_be_concatenated()
    {
        string source = """
            var s1: string = "ASCII string";
            var s3 = s1 + " and åäöÅÄÖéèüÜÿŸïÏすし";

            print(s3);
            """;

        var output = EvalReturningOutputString(source);

        output.Should()
            .Be("ASCII string and åäöÅÄÖéèüÜÿŸïÏすし");
    }

    [Fact]
    public void utf8_string_and_ascii_string_variable_can_be_concatenated()
    {
        string source = """
            var s1: string = "this is s1 with non-ASCII characters: åäöÅÄÖéèüÜÿŸïÏすし";
            var s2: string = " and this is s2";
            var s3: string = s1 + s2;

            print(s3);
            """;

        var output = EvalReturningOutputString(source);

        output.Should()
            .Be("this is s1 with non-ASCII characters: åäöÅÄÖéèüÜÿŸïÏすし and this is s2");
    }

    [Fact]
    public void utf8_string_and_ascii_string_literal_can_be_concatenated()
    {
        string source = """
            var s1: string = "this is s1 with non-ASCII characters: åäöÅÄÖéèüÜÿŸïÏすし";
            var s3: string = s1 + " and this is s2";

            print(s3);
            """;

        var output = EvalReturningOutputString(source);

        output.Should()
            .Be("this is s1 with non-ASCII characters: åäöÅÄÖéèüÜÿŸïÏすし and this is s2");
    }

    [Fact]
    public void ascii_string_can_be_concatenated_with_int_and_ascii_string()
    {
        string source = """
            var s1: string = "temperature is ";
            var i: int = 85;
            var s2: string = " degrees fahrenheit";
            var s3: string = s1 + i + s2;

            print(s3);
            """;

        var output = EvalReturningOutputString(source);

        output.Should()
            .Be("temperature is 85 degrees fahrenheit");
    }

    [Fact]
    public void ascii_string_can_be_concatenated_with_double_and_string()
    {
        string source = """
            var s1: string = "temperature is ";
            var i: double = 85.2;
            var s2: string = " degrees fahrenheit";
            var s3: string = s1 + i + s2;

            print(s3);
            """;

        var output = EvalReturningOutputString(source);

        output.Should()
            .Be("temperature is 85.2 degrees fahrenheit");
    }

    [Fact]
    public void utf8_string_can_be_concatenated_with_double_and_ascii_string()
    {
        string source = """
            var s1: string = "Den årliga medeltemperaturen i Vasa är ";
            var d: double = 3.4;
            var s2: string = " grader Celsius";
            var s3: string = s1 + d + s2;

            print(s3);
            """;

        var output = EvalReturningOutputString(source);

        output.Should()
            .Be("Den årliga medeltemperaturen i Vasa är 3.4 grader Celsius");
    }

    [Fact]
    public void utf8_string_can_be_concatenated_with_int()
    {
        string source = """
            var s1: string = "Årets varmaste temperatur (Celsius): ";
            var i: int = 32;
            var s2: string = s1 + i;

            print(s2);
            """;

        var output = EvalReturningOutputString(source);

        output.Should()
            .Be("Årets varmaste temperatur (Celsius): 32");
    }

    [Fact]
    public void ascii_strings_can_be_compared()
    {
        string source = """
            var s1: string = "this is a string";
            var s2: string = "this is a string";

            print(s1 == s2);
            """;

        var output = EvalReturningOutputString(source);

        output.Should()
            .Be("true");
    }

    [Fact]
    public void utf8_strings_can_be_compared()
    {
        string source = """
            var s1: string = "this is a string with non-ASCII characters: åäöÅÄÖéèüÜÿŸïÏすし";
            var s2: string = "this is a string with non-ASCII characters: åäöÅÄÖéèüÜÿŸïÏすし";

            print(s1 == s2);
            """;

        var output = EvalReturningOutputString(source);

        output.Should()
            .Be("true");
    }

    [Fact]
    public void ascii_string_can_be_compared_with_utf8_string()
    {
        string source = """
            var s1: string = "this is a string";
            var s2: string = "this is a string with non-ASCII characters: åäöÅÄÖéèüÜÿŸïÏすし";

            print(s1 == s2);
            """;

        var output = EvalReturningOutputString(source);

        output.Should()
            .Be("false");
    }

    [Fact]
    public void utf8_string_can_be_compared_with_ascii_string()
    {
        string source = """
            var s1: string = "this is a string with non-ASCII characters: åäöÅÄÖéèüÜÿŸïÏすし";
            var s2: string = "this is a string";

            print(s1 == s2);
            """;

        var output = EvalReturningOutputString(source);

        output.Should()
            .Be("false");
    }

    [Fact]
    public void implicitly_typed_ascii_string_variable_has_expected_type()
    {
        string source = """
            var s: string = "this is an ASCII string";

            print(s.get_type());
            """;

        var output = EvalReturningOutputString(source);

        output.Should()
            .Be("perlang::ASCIIString");
    }

    [Fact]
    public void explicitly_typed_ASCII_string_can_be_printed()
    {
        string source = """
            var s: ASCIIString = "this is an ASCII string";

            print(s);
            """;

        var output = EvalReturningOutputString(source);

        output.Should()
            .Be("this is an ASCII string");
    }

    [Fact]
    public void explicitly_typed_UTF8_string_can_be_printed()
    {
        string source = """
            var s: UTF8String = "this is a string with non-ASCII characters: åäöÅÄÖéèüÜÿŸïÏすし";

            print(s);
            """;

        var output = EvalReturningOutputString(source);

        output.Should()
            .Be("this is a string with non-ASCII characters: åäöÅÄÖéèüÜÿŸïÏすし");
    }
}
