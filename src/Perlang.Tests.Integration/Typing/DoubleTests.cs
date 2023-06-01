using System.Linq;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Typing;

public class DoubleTests
{
    [Fact]
    public void double_variable_can_be_printed()
    {
        string source = @"
                var d: double = 103.1d;

                print(d);
            ";

        var output = EvalReturningOutputString(source);

        Assert.Equal("103.1", output);
    }

    [Fact]
    public void double_variable_can_be_reassigned()
    {
        string source = @"
                var d: double = 103;
                d = 104;

                print(d);
            ";

        var result = EvalReturningOutputString(source);

        Assert.Equal("104", result);
    }

    [Fact]
    public void double_variable_can_be_initialized_from_long_constant()
    {
        // 64-bit integers cannot be reliably stored in a double, since IEEE 754 double-precision floating point can
        // only represent without data loss integers in the range -2^53 to 2^53. More details:
        // https://en.wikipedia.org/wiki/Double-precision_floating-point_format#Precision_limitations_on_integer_values
        //
        // _However_, since other well-respected languages like Java and C# allow this implicit conversion, we decided
        // to allow it in Perlang alike, to reduce end-user confusion. We could do like CLion/Clang-Tidy and warn about
        // "Implicit conversion from 'unsigned long long' to 'double' changes value from 18446744073709551615 to
        // 18446744073709551616" though.

        string source = @"
                var d: double = 9223372036854775807;

                print(d);
            ";

        var result = EvalReturningOutputString(source);

        // Note how this is less exact than the source value
        Assert.Equal("9.22337203685478E+18", result);
    }

    [Fact]
    public void double_variable_throws_expected_exception_when_initialized_from_bigint_constant()
    {
        string source = @"
                var d: double = 18446744073709551616;
            ";

        var result = EvalWithValidationErrorCatch(source);
        var exception = result.Errors.First();

        Assert.Single(result.Errors);
        Assert.Matches("Cannot assign bigint to double variable", exception.Message);
    }

    [Fact]
    public void double_variable_throws_expected_exception_when_assigned_to_long_variable()
    {
        string source = @"
                var d: double = 8589934592.1;
                var l: long = d;
            ";

        var result = EvalWithValidationErrorCatch(source);
        var exception = result.Errors.First();

        Assert.Single(result.Errors);
        Assert.Matches("Cannot assign double to long variable", exception.Message);
    }

    [SkippableFact]
    public void double_variable_has_expected_type_when_initialized_to_int_value()
    {
        // A 32-bit integer should be converted to `double` when assigned to a variable of that type.
        string source = @"
                var d: double = 103;

                print(d.get_type());
            ";

        var output = EvalReturningOutputString(source);

        Assert.Equal("System.Double", output);
    }

    [SkippableFact]
    public void double_variable_has_expected_type_when_assigned_int_value_from_another_variable()
    {
        // A 32-bit integer should be converted to `double` when assigned to a variable of that type.
        string source = @"
                var d: double = 103;
                var e = d;

                print(e.get_type());
            ";

        var output = EvalReturningOutputString(source);

        Assert.Equal("System.Double", output);
    }

    [SkippableFact]
    public void double_variable_has_expected_type_for_large_value()
    {
        string source = @"
                var d: double = 2147483647;

                print(d.get_type());
            ";

        var output = EvalReturningOutputString(source);

        Assert.Equal("System.Double", output);
    }
}
