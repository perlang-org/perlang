using FluentAssertions;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Typing;

public class FloatTests
{
    [Fact]
    public void float_variable_can_be_printed()
    {
        string source = @"
                var f: float = 103.1f;

                print(f);
            ";

        var output = EvalReturningOutputString(source);

        Assert.Equal("103.1", output);
    }

    [Fact]
    public void float_variable_can_be_reassigned()
    {
        string source = @"
                var f: float = 103;
                f = 104;

                print(f);
            ";

        var result = EvalReturningOutputString(source);

        Assert.Equal("104", result);
    }

    [Fact]
    public void float_variable_can_be_initialized_from_int_constant()
    {
        // 32-bit integers cannot be reliably stored in a float, since IEEE 754 single-precision floating point can only
        // represent without data loss integers in the range -2^24+1 to 2^24-1. More details:
        // https://en.wikipedia.org/wiki/Single-precision_floating-point_format#Precision_limitations_on_integer_values
        //
        // _However_, since other well-respected languages like Java and C# allow this implicit conversion, we decided
        // to allow it in Perlang alike, to reduce end-user confusion. We could do like CLion/Clang-Tidy and warn about
        // it, though.

        string source = @"
                var f: float = 2147483647;

                print(f);
            ";

        var result = EvalReturningOutputString(source);

        // Note how this is less exact than the source value
        Assert.Equal("2.147484E+09", result);
    }

    [Fact]
    public void float_variable_throws_expected_exception_when_initialized_from_long_constant()
    {
        string source = @"
                var f: float = 9223372036854775807;
            ";

        var result = EvalWithValidationErrorCatch(source);

        result.Errors.Should()
            .ContainSingle().Which
            .Message.Should().Match("Cannot assign long to float variable");
    }

    [Fact]
    public void float_variable_throws_expected_exception_when_initialized_from_bigint_constant()
    {
        string source = @"
                var f: float = 18446744073709551616;
            ";

        var result = EvalWithValidationErrorCatch(source);

        result.Errors.Should()
            .ContainSingle().Which
            .Message.Should().Match("Cannot assign bigint to float variable");
    }

    [Fact]
    public void float_variable_throws_expected_exception_when_assigned_to_long_variable()
    {
        string source = @"
                var f: float = 8589934592.1f;
                var l: long = f;
            ";

        var result = EvalWithValidationErrorCatch(source);

        result.Errors.Should()
            .ContainSingle().Which
            .Message.Should().Match("Cannot assign float to long variable");
    }

    [SkippableFact]
    public void float_variable_has_expected_type_when_initialized_to_int_value()
    {
        // A 32-bit integer should be converted to `float` when assigned to a variable of that type.
        string source = @"
                var f: float = 103;

                print(f.get_type());
            ";

        var output = EvalReturningOutputString(source);

        Assert.Equal("System.Single", output);
    }

    [SkippableFact]
    public void float_variable_has_expected_type_when_assigned_int_value_from_another_variable()
    {
        // A 32-bit integer should be converted to `float` when assigned to a variable of that type.
        string source = @"
                var f: float = 103;
                var g = f;

                print(g.get_type());
            ";

        var output = EvalReturningOutputString(source);

        Assert.Equal("System.Single", output);
    }

    [SkippableFact]
    public void float_variable_has_expected_type_for_large_value()
    {
        string source = @"
                var f: float = 2147483647;

                print(f.get_type());
            ";

        var output = EvalReturningOutputString(source);

        Assert.Equal("System.Single", output);
    }
}
