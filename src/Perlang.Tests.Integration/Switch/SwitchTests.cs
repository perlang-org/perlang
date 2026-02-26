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
        // TODO: When we support ranges for conditions, add a copy of this test which uses ranges instead
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
