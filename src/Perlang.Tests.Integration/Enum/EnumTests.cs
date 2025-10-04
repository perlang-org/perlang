#pragma warning disable SA1025
using FluentAssertions;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Enum;

/// <summary>
/// Tests for the `enum` keyword.
/// </summary>
public class EnumTests
{
    [Fact]
    public void enum_declaration()
    {
        const string source = """
            enum Color {
                RED,
                GREEN,
                BLUE
            }

            print Color.RED;
            print Color.GREEN;
            print Color.BLUE;
            """;

        var output = EvalReturningOutput(source);

        output.Should()
            .Equal(
                "0",
                "1",
                "2"
        );
    }

    [Fact]
    public void enum_declaration_with_initializers()
    {
        string source = """
            enum Color {
                RED = 1,
                GREEN = 2,
                BLUE = 4
            }

            print Color.RED;
            print Color.GREEN;
            print Color.BLUE;
            """;

        var output = EvalReturningOutput(source);

        output.Should()
            .Equal(
                "1",
                "2",
                "4"
            );
    }

    [Fact]
    public void error_enum_declaration_with_initializer_referring_to_undefined_symbol()
    {
        string source = """
            enum Color {
                RED,
                GREEN = PURPLE,
                BLUE
            }
            """;

        var result = EvalWithValidationErrorCatch(source);

        result.Errors.Should()
            .ContainSingle()
            .Which
            .Message.Should().Contain("Undefined identifier 'PURPLE'");
    }

    [Fact]
    public void enum_declaration_with_initializer_consisting_of_arithmetic_expression()
    {
        string source = """
            enum Color {
                RED,
                GREEN = 1 + 2,
                BLUE
            }

            print Color.RED;
            print Color.GREEN;
            print Color.BLUE;
            """;

        var output = EvalReturningOutput(source);

        output.Should()
            .Equal(
                "0",
                "3",
                "4"
            );
    }

    [Fact]
    public void enum_declaration_with_last_element_initializer_overflowing_a_32_bit_integer()
    {
        string source = """
            enum Color {
                RED,
                GREEN,
                BLUE = 4294967295
            }

            print Color.RED;
            print Color.GREEN;
            print Color.BLUE;
            """;

        var output = EvalReturningOutput(source);

        output.Should()
            .Equal(
                "0",
                "1",
                "4294967295" // UInt32.MaxValue
            );
    }

    [Fact]
    public void enum_declaration_with_second_last_element_initializer_overflowing_a_32_bit_integer()
    {
        string source = """
            enum Color {
                RED,
                GREEN = 4294967295,
                BLUE
            }

            print Color.RED;
            print Color.GREEN;
            print Color.BLUE;
            """;

        var output = EvalReturningOutput(source);

        output.Should()
            .Equal(
                "0",
                "4294967295", // UInt32.MaxValue
                "4294967296"  // UInt32.MaxValue + 1
            );
    }

    [Fact]
    public void error_enum_declaration_with_second_last_element_initializer_overflowing_a_64_bit_integer()
    {
        string source = """
            enum Color {
                RED,
                GREEN = 18446744073709551615,
                BLUE
            }

            print Color.RED;
            print Color.GREEN;
            print Color.BLUE;
            """;

        var result = EvalWithCppCompilationErrorCatch(source);

        result.Errors.Should()
            .ContainSingle()
            .Which
            .Message.Should().Contain("incremented enumerator value 18446744073709551616 is not representable in the largest integer type");
    }

    [Fact]
    public void enum_declaration_with_initializer_with_negative_value()
    {
        string source = """
            enum Color {
                RED = 1,
                GREEN = -1,
                BLUE
            }

            print Color.RED;
            print Color.GREEN;
            print Color.BLUE;
            """;

        var output = EvalReturningOutput(source);

        output.Should()
            .Equal(
                "1",
                "-1",
                "0"
            );
    }

    [Fact]
    public void enum_declaration_with_initializers_and_implicit_values()
    {
        string source = """
            enum Color {
                RED = 1,
                GREEN,
                BLUE
            }

            print Color.RED;
            print Color.GREEN;
            print Color.BLUE;
            """;

        var output = EvalReturningOutput(source);

        output.Should()
            .Equal(
                "1",
                "2",
                "3"
            );
    }

    [Fact]
    public void error_enum_declaration_with_string_initializer()
    {
        string source = """
            enum Color {
                RED = "red",
                GREEN,
                BLUE
            }
            """;

        var result = EvalWithCppCompilationErrorCatch(source);

        result.Errors.Should()
            .ContainSingle()
            .Which
            .Message.Should().Contain("integral constant expression requires explicit conversion");
    }

    [Fact]
    public void enum_declaration_with_initializers_and_implicit_values_and_explicit_values()
    {
        string source = """
            enum Color {
                RED = 1,
                GREEN,
                BLUE = 4,
                YELLOW
            }

            print Color.RED;
            print Color.GREEN;
            print Color.BLUE;
            print Color.YELLOW;
            """;

        var output = EvalReturningOutput(source);

        output.Should()
            .Equal(
                "1",
                "2",
                "4",
                "5"
            );
    }

    [Fact]
    public void enum_declaration_with_initializers_and_implicit_values_and_explicit_values_and_repeated_values()
    {
        string source = """
            enum Color {
                RED = 1,
                GREEN,
                BLUE = 4,
                YELLOW = 4
            }

            print Color.RED;
            print Color.GREEN;
            print Color.BLUE;
            print Color.YELLOW;
            """;

        var output = EvalReturningOutput(source);

        output.Should()
            .Equal(
                "1",
                "2",
                "4",
                "4"
            );
    }

    [Fact]
    public void enum_declaration_with_initializers_and_implicit_values_and_explicit_values_and_repeated_values_and_repeated_values_in_different_enums()
    {
        string source = """
            enum Color {
                RED = 1,
                GREEN,
                BLUE = 4,
                YELLOW = 4
            }

            enum Shape {
                SQUARE = 1,
                CIRCLE,
                TRIANGLE = 4,
                HEXAGON = 4
            }

            print Color.RED;
            print Color.GREEN;
            print Color.BLUE;
            print Color.YELLOW;

            print Shape.SQUARE;
            print Shape.CIRCLE;
            print Shape.TRIANGLE;
            print Shape.HEXAGON;
            """;

        var output = EvalReturningOutput(source);

        output.Should()
            .Equal(
                "1",
                "2",
                "4",
                "4",
                "1",
                "2",
                "4",
                "4"
            );
    }

    [Fact]
    public void enum_reference_to_nonexistent_value_throws_expected_error()
    {
        string source = """
            enum Color {
                RED,
                GREEN,
                BLUE
            }

            print Color.PURPLE;
            """;

        var result = EvalWithValidationErrorCatch(source);

        result.Errors.Should()
            .ContainSingle()
            .Which
            .Message.Should().Contain("Enum member 'PURPLE' not found in enum 'Color'");
    }

    [Fact]
    public void enum_can_be_used_as_explicit_type_for_local_variable()
    {
        const string source = """
            enum Color {
                RED,
                GREEN,
                BLUE
            }

            var a: Color = Color.BLUE;
            var b: Color = Color.GREEN;
            var c: Color = Color.RED;

            print a;
            print b;
            print c;
            """;

        var output = EvalReturningOutput(source);

        output.Should()
            .Equal(
                "2",
                "1",
                "0"
            );
    }

    [Fact]
    public void enum_can_be_used_as_implicit_type_for_local_variable()
    {
        const string source = """
            enum Color {
                RED,
                GREEN,
                BLUE
            }

            var a = Color.BLUE;
            var b = Color.GREEN;
            var c = Color.RED;

            print a;
            print b;
            print c;
            """;

        var output = EvalReturningOutput(source);

        output.Should()
            .Equal(
                "2",
                "1",
                "0"
            );
    }
}
