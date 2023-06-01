using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Function
{
    public class Return
    {
        [Fact]
        public void function_can_return_values_in_if_statement()
        {
            string source = @"
                fun f(i: int): int {
                    if (i < 10) return i;
                    if (i >= 10) return i / 2;

                    // Will never be reached
                    return -1;
                }

                print f(5);
                print f(15);
            ";

            var output = EvalReturningOutput(source);

            Assert.Equal(new[]
            {
                "5",
                "7"
            }, output);
        }
    }
}
