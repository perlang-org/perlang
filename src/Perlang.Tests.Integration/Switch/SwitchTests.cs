using System.Linq;
using FluentAssertions;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Switch;

public class SwitchTests
{
    [Fact]
    public void switch_statement_can_switch_on_char_literals()
    {
        string source = """
            var c: char = 'c';

            switch (c) {
                case 'a':
                    print "first";
                case 'b':
                    print "second";
                case 'c':
                    print "third";
                case 'd':
                    print "fourth";
                default:
                    print "other";
            }
            """;

        var output = EvalReturningOutputString(source);

        output.Should()
            .Be("third");
    }

    [Fact]
    public void switch_statement_can_switch_on_char_ranges()
    {
        string source = """
            fun print_char_group(c: char): void
            {
                switch (c) {
                    case 'a'..'c':
                        print "first";
                    case 'x'..'z':
                        print "second";
                    default:
                        print "other";
                }
            }

            print_char_group('b');
            print_char_group('x');
            print_char_group('j');
            """;

        var output = EvalReturningOutput(source);

        output.Should()
            .BeEquivalentTo(
                "first",
                "second",
                "other"
            );
    }

    [Fact]
    public void switch_statement_can_switch_on_integer()
    {
        string source = """
            var i: int = 10;

            switch (i) {
                case 1:
                    print "one";
                case 10:
                    print "ten";
                default:
                    print "default";
            }
            """;

        var output = EvalReturningOutputString(source);

        output.Should()
            .Be("ten");
    }

    [Fact]
    public void switch_statement_does_what_when_mixing_multiple_types_as_condition()
    {
        string source = """
            var i: int = 10;

            switch (i) {
                case 1:
                    print "one";
                case 2:
                    print "two";
                case 'z':
                    print "zed";
                default:
                    print "default";
            }
            """;

        var result = EvalWithValidationErrorCatch(source);
        var exception = result.Errors.FirstOrDefault();

        result.Errors.Should().ContainSingle();
        exception?.Message.Should()
            .Be("Invalid case value: 'z' (char) does not match switch expression 'i' (int).");
    }

    [Fact]
    public void switch_statement_can_have_same_branch_for_multiple_conditions()
    {
        string source = """
            fun print_number_group(i: int): void
            {
                switch (i) {
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                        print "one to five";
                    case 6:
                    case 7:
                    case 8:
                    case 9:
                    case 10:
                        print "six to ten";
                    default:
                        print "other";
                }
            }

            print_number_group(3);
            print_number_group(7);
            print_number_group(42);
            """;

        var output = EvalReturningOutput(source);

        output.Should()
            .BeEquivalentTo(
                "one to five",
                "six to ten",
                "other"
            );
    }

    [Fact]
    public void switch_statement_can_switch_on_integer_ranges()
    {
        string source = """
            fun print_number_group(i: int): void
            {
                switch (i) {
                    case 1..5:
                        print "one to five";
                    case 6..10:
                        print "six to ten";
                    default:
                        print "other";
                }
            }

            print_number_group(3);
            print_number_group(7);
            print_number_group(42);
            """;

        var output = EvalReturningOutput(source);

        output.Should()
            .BeEquivalentTo(
                "one to five",
                "six to ten",
                "other"
            );
    }

    [Fact]
    public void switch_statement_emits_expected_error_for_too_large_range_condition()
    {
        string source = """
            var i: int = 129;

            switch (i) {
                case 1..129:
                    print "small number";
                default:
                    print "other";
            }
            """;

        var result = EvalWithCppCompilationErrorCatch(source);

        result.Errors.Should()
            .ContainSingle()
            .Which
            .Message.Should().Contain("exceeds the maximum of 128");

        result.Errors.Should()
            .ContainSingle()
            .Which
            .Message.Should().Contain("Use an 'if' statement in the 'default' branch instead.");
    }

    [Fact]
    public void switch_statement_emits_expected_error_for_descending_range_condition()
    {
        string source = """
            var i: int = 10;

            switch (i) {
                case 10..1:
                    print "number";
                default:
                    print "other";
            }
            """;

        var result = EvalWithCppCompilationErrorCatch(source);

        result.Errors.Should()
            .ContainSingle()
            .Which
            .Message.Should().Contain("only ascending ranges are supported");
    }

    [Fact]
    public void switch_statement_can_switch_on_enum()
    {
        string source = """
            enum Color {
                RED,
                GREEN,
                BLUE
            }

            var c: Color = Color.GREEN;

            switch (c) {
                case Color.RED:
                    print "red";
                case Color.GREEN:
                    print "green";
                case Color.BLUE:
                    print "blue";
            }
            """;

        var output = EvalReturningOutputString(source);

        output.Should()
            .Be("green");
    }

    [SkippableFact(Skip = "switch condition type 'std::shared_ptr<perlang::String>' requires explicit conversion to 'bool'")]
    public void switch_statement_can_switch_on_strings()
    {
        string source = """
            var s: string = "brown";

            switch (s) {
                case "alpha":
                    print "A";
                case "brown":
                    print "B";
                case "charlie":
                    print "C";
                default:
                    // no-op
            }
            """;

        var output = EvalReturningOutputString(source);

        output.Should()
            .Be("B");
    }
}
