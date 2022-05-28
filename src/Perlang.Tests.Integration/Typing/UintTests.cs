using System.Linq;
using FluentAssertions;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Typing;

/// <summary>
/// Tests for the `uint` data type.
/// </summary>
public class UintTests
{
    [Theory]
    [InlineData("103", "103")]
    [InlineData("2147483647", "2147483647")] // Int32.MaxValue
    [InlineData("2147483648", "2147483648")] // Int32.MaxValue + 1
    [InlineData("4294967295", "4294967295")] // UInt32.MaxValue
    public void uint_variable_can_be_printed(string value, string expectedOutput)
    {
        string source = $@"
            var u: uint = {value};

            print(u);
        ";

        var output = EvalReturningOutputString(source);

        Assert.Equal(expectedOutput, output);
    }

    [Theory]
    [InlineData("103", "System.UInt32")]
    [InlineData("2147483647", "System.UInt32")] // Int32.MaxValue
    [InlineData("2147483648", "System.UInt32")] // Int32.MaxValue + 1
    [InlineData("4294967295", "System.UInt32")] // UInt32.MaxValue
    public void uint_variable_has_expected_type(string value, string expectedType)
    {
        string source = $@"
            var u: uint = {value};

            print(u.get_type());
        ";

        var output = EvalReturningOutputString(source);

        Assert.Equal(expectedType, output);
    }

    [Fact]
    public void uint_variable_can_be_reassigned()
    {
        string source = @"
            var u: uint = 103;
            u = 4294967295;

            print(u);
        ";

        var result = EvalReturningOutputString(source);

        Assert.Equal("4294967295", result);
    }

    [Fact]
    public void uint_variable_throws_expected_exception_on_constant_overflow()
    {
        string source = @"
            var u: uint = 1231231230912839019312831232;

            print(u);
        ";

        var result = EvalWithValidationErrorCatch(source);
        var exception = result.Errors.First();

        Assert.Single(result.Errors);
        Assert.Matches("Cannot assign bigint to uint variable", exception.Message);
    }

    [Fact]
    public void uint_variable_has_expected_type_when_initialized_to_8bit_value()
    {
        // An 8-bit integer (sbyte) should be expanded to 32-bit when the assignment target is of the `uint` type.
        string source = @"
            var u: uint = 103;

            print(u.get_type());
        ";

        var output = EvalReturningOutputString(source);

        Assert.Equal("System.UInt32", output);
    }

    [Fact]
    public void uint_variable_has_expected_type_when_assigned_8bit_value_from_another_variable()
    {
        // An 8-bit integer (sbyte) should be expanded to 32-bit when the assignment target is of the `uint` type.
        string source = @"
            var u: uint = 103;
            var v = u;

            print(u.get_type());
        ";

        var output = EvalReturningOutputString(source);

        Assert.Equal("System.UInt32", output);
    }

    [Fact]
    public void uint_variable_emits_expected_error_when_initializer_assigned_to_int_variable()
    {
        // Expansions are fine (103 to `uint`), but assignment to a smaller-sized variable is not supported with
        // implicit conversions. Attempting to initialize an `int` variable with this value is expected to fail, since
        // not all `uint` values can be stored in a 32-bit signed integer without data loss.
        string source = @"
            var u: uint = 103;
            var i: int = u;
        ";

        var result = EvalWithValidationErrorCatch(source);

        result.Errors.Should()
            .ContainSingle().Which
            .Message.Should().Match("Cannot assign uint to int variable");
    }

    [Fact]
    public void int_variable_emits_expected_error_when_initializer_assigned_to_uint_variable()
    {
        string source = @"
                var i: int = 12345;
                var u: uint = i;

                print(u.get_type());
            ";

        var result = EvalWithValidationErrorCatch(source);

        result.Errors.Should()
            .ContainSingle().Which
            .Message.Should().Match("Cannot assign int to uint variable");
    }
}
