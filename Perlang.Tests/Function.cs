using System.Linq;
using Xunit;
using static Perlang.Tests.EvalHelper;

namespace Perlang.Tests
{
    public class Function
    {
        // Tests based on Lox test suite: https://github.com/munificent/craftinginterpreters/tree/master/test/function

        //
        // "Positive" tests, testing things that are expected to work and work in a certain way.
        //

        [Fact]
        void function_can_receive_0_arguments()
        {
            string source = @"
                fun f0() { return 0; }
                print f0();
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("0", output);
        }

        [Fact]
        void function_can_receive_1_argument()
        {
            string source = @"
                fun f1(a) { return a; }
                print f1(1);
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("1", output);
        }

        [Fact]
        void function_can_receive_2_arguments()
        {
            string source = @"
                fun f2(a, b) { return a + b; }
                print f2(1, 2);
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("3", output);
        }

        [Fact]
        void function_can_receive_3_arguments()
        {
            string source = @"
                fun f3(a, b, c) { return a + b + c; }
                print f3(1, 2, 3);
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("6", output);
        }

        [Fact]
        void function_can_receive_4_arguments()
        {
            string source = @"
                fun f4(a, b, c, d) { return a + b + c + d; }
                print f4(1, 2, 3, 4);
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("10", output);
        }

        [Fact]
        void function_can_receive_5_arguments()
        {
            string source = @"
                fun f5(a, b, c, d, e) { return a + b + c + d + e; }
                print f5(1, 2, 3, 4, 5);
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("15", output);
        }

        [Fact]
        void function_can_receive_6_arguments()
        {
            string source = @"
                fun f6(a, b, c, d, e, f) { return a + b + c + d + e + f; }
                print f6(1, 2, 3, 4, 5, 6);
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("21", output);
        }

        [Fact]
        void function_can_receive_7_arguments()
        {
            string source = @"
                fun f7(a, b, c, d, e, f, g) { return a + b + c + d + e + f + g; }
                print f7(1, 2, 3, 4, 5, 6, 7);
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("28", output);
        }

        [Fact]
        void function_can_receive_8_arguments()
        {
            string source = @"
                fun f8(a, b, c, d, e, f, g, h) { return a + b + c + d + e + f + g + h; }
                print f8(1, 2, 3, 4, 5, 6, 7, 8);
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("36", output);
        }

        [Fact]
        void recursive_function_returns_expected_result()
        {
            string source = @"
                fun fib(n) {
                  if (n < 2) return n;
                  return fib(n - 1) + fib(n - 2);
                }

                print fib(8);
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("21", output);

        }

        // "Negative" tests, asserting that errors are handled in a deterministic way.
        //

        [Fact]
        void extra_arguments_expects_runtime_error()
        {
            string source = @"
                fun f(a, b) {
                    print a;
                    print b;
                }

                f(1, 2, 3, 4);
            ";

            var result = EvalWithRuntimeCatch(source);
            var exception = result.RuntimeErrors.First();

            Assert.Single(result.RuntimeErrors);
            Assert.Matches("Expected 2 argument\\(s\\) but got 4", exception.Message);
        }

        [Fact]
        void missing_arguments_expects_runtime_error()
        {
            string source = @"
                fun f(a, b) {}

                f(1);
            ";

            var result = EvalWithRuntimeCatch(source);
            var exception = result.RuntimeErrors.First();

            Assert.Single(result.RuntimeErrors);
            Assert.Matches("Expected 2 argument\\(s\\) but got 1", exception.Message);
        }

        [Fact]
        void missing_comma_in_parameters_expects_parse_error()
        {
            string source = @"
                fun foo(a, b c, d, e, f) {}
            ";

            var result = EvalWithParseErrorCatch(source);
            var exception = result.ParseErrors.First();

            Assert.Single(result.ParseErrors);
            Assert.Matches("Expect '\\)' after parameters.", exception.Message);
        }
    }
}
