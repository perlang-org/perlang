using FluentAssertions;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Typing;

/// <summary>
/// Tests for the `int` data type.
/// </summary>
public class IntTests
{
    [Fact]
    public void int_variable_has_expected_type_when_initialized_to_8bit_value()
    {
        // An 8-bit integer (sbyte) should be expanded to 32-bit when the assignment target is of the 'int' type.
        string source = @"
                var i: int = 103;

                print(i.get_type());
            ";

        string output = EvalReturningOutputString(source);

        output.Should()
            .Be("System.Int32");
    }

    [Fact]
    public void int_variable_has_expected_type_when_initialized_from_int_variable_with_negative_value()
    {
        string source = @"
                var i: int = -12345;
                var j: int = i;

                print(j.get_type());
            ";

        string output = EvalReturningOutputString(source);

        output.Should()
            .Be("System.Int32");
    }

    [Fact]
    public void int_variable_with_negative_value_throws_expected_exception_when_assigned_to_uint_variable()
    {
        string source = @"
                var i: int = -12345;
                var u: uint = i;
            ";

        var result = EvalWithValidationErrorCatch(source);

        result.Errors.Should()
            .ContainSingle().Which
            .Message.Should().Match("Cannot assign int to uint variable");
    }
}
