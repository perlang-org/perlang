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

        var output = EvalReturningOutputString(source);

        Assert.Equal("System.Int32", output);
    }
}
