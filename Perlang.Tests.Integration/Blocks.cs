using System.Linq;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration
{
    public class Blocks
    {
        [Fact]
        void calling_an_undefined_function_inside_a_block_throws_expected_exception()
        {
            var result = EvalWithTypeValidationErrorCatch("if (true) { die_hard(); }");
            var exception = result.TypeValidationErrors.First();

            Assert.Single(result.TypeValidationErrors);
            Assert.Matches("Attempting to call undefined function 'die_hard'", exception.Message);
        }

        [Fact]
        void referring_to_an_undefined_variable_inside_a_block_throws_expected_exception()
        {
            var result = EvalWithTypeValidationErrorCatch("if (true) { var a = die_hard; }");
            var exception = result.TypeValidationErrors.First();

            Assert.Single(result.TypeValidationErrors);
            Assert.Matches("Undefined variable 'die_hard'", exception.Message);
        }
    }
}
