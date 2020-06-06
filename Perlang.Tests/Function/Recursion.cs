using System.Linq;
using Xunit;
using static Perlang.Tests.EvalHelper;

namespace Perlang.Tests.Function
{
    public class Recursion
    {
        // Tests based on Lox test suite: https://github.com/munificent/craftinginterpreters/tree/master/test/function

        // Technically not recursion, but still related to nested function calls.
        [Fact]
        void function_can_call_another_function()
        {
            string source = @"
                fun f(i: int): int { return i * 42; }
                fun g(): int { return f(42); }

                print g();
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("1764", output);
        }

        [Fact]
        void recursive_function_returns_expected_result()
        {
            string source = @"
                fun fib(n: int): int {
                  if (n < 2) return n;
                  return fib(n - 1) + fib(n - 2);
                }

                print fib(8);
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("21", output);
        }
    }
}
