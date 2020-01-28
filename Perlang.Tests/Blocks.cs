using System.Linq;
using Xunit;
using static Perlang.Tests.EvalHelper;

namespace Perlang.Tests
{
    public class Blocks
    {
        [Fact]
        public void calling_an_undefined_function_inside_a_block_throws_expected_exception()
        {
            var result = EvalWithRuntimeCatch("if (true) { die_hard(); }");
            var exception = result.RuntimeErrors.First();

            Assert.Single(result.RuntimeErrors);
            Assert.Matches("Undefined variable 'die_hard'", exception.Message);
        }

        [Fact]
        public void referring_to_an_undefined_variable_inside_a_block_throws_expected_exception()
        {
            var result = EvalWithRuntimeCatch("if (true) { var a = die_hard; }");
            var exception = result.RuntimeErrors.First();

            Assert.Single(result.RuntimeErrors);
            Assert.Matches("Undefined variable 'die_hard'", exception.Message);
        }
    }
}
