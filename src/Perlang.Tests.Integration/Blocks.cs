using System.Linq;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration;

public class Blocks
{
    [Fact]
    public void calling_an_undefined_function_inside_a_block_throws_expected_exception()
    {
        var result = EvalWithValidationErrorCatch("if (true) { die_hard(); }");
        var exception = result.Errors.First();

        Assert.Single(result.Errors);
        Assert.Matches("Attempting to call undefined function 'die_hard'", exception.Message);
    }

    [Fact]
    public void referring_to_an_undefined_variable_inside_a_block_throws_expected_exception()
    {
        var result = EvalWithValidationErrorCatch("if (true) { var a = die_hard; }");
        var exception = result.Errors.First();

        Assert.Single(result.Errors);
        Assert.Matches("Undefined identifier 'die_hard'", exception.Message);
    }
}