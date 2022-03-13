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
                var d: double = 103.1;

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
    public void double_variable_throws_expected_exception_on_constant_overflow()
    {
        // 64-bit integers cannot be reliably stored in a double, since IEEE 754 double-precision floating point only
        // represent exactly the range -2^53 to 2^53. More details:
        // https://en.wikipedia.org/wiki/Double-precision_floating-point_format#Precision_limitations_on_integer_values
        string source = @"
                var d: double = 1231231230912839019312831232;
            ";

        var result = EvalWithValidationErrorCatch(source);
        var exception = result.Errors.First();

        Assert.Single(result.Errors);
        Assert.Matches("Cannot assign bigint to double variable", exception.Message);
    }

    [Fact]
    public void double_variable_has_expected_type_when_initialized_to_8bit_value()
    {
        // An 8-bit integer (sbyte) should be expanded to 64-bit when the assignment target is of the 'long' type.
        string source = @"
                var d: double = 103;

                print(d.get_type());
            ";

        var output = EvalReturningOutputString(source);

        Assert.Equal("System.Double", output);
    }

    [Fact]
    public void double_variable_has_expected_type_when_assigned_8bit_value_from_another_variable()
    {
        // An 8-bit integer (sbyte) should be expanded to 64-bit when the assignment target is of the 'long' type.
        string source = @"
                var d: double = 103;
                var e = d;

                print(e.get_type());
            ";

        var output = EvalReturningOutputString(source);

        Assert.Equal("System.Double", output);
    }

    // The value becomes a uint in this case, but uints are not fully supported in the language yet.
    [Fact(Skip = "Pending https://github.com/perlang-org/perlang/issues/70")]
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
