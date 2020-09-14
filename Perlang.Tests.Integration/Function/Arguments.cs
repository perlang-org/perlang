using System.Linq;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Function
{
    public class Arguments
    {
        // Tests based on Lox test suite: https://github.com/munificent/craftinginterpreters/tree/master/test/function

        //
        // "Positive" tests, testing things that are expected to work and work in a certain way.
        //

        [Fact]
        void function_can_receive_0_arguments()
        {
            string source = @"
                fun f0(): int { return 0; }
                print f0();
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("0", output);
        }

        [Fact]
        void function_can_receive_1_argument()
        {
            string source = @"
                fun f1(a: int): int { return a; }
                print f1(1);
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("1", output);
        }

        [Fact]
        void function_can_receive_2_arguments()
        {
            string source = @"
                fun f2(a: int, b: int): int { return a + b; }
                print f2(1, 2);
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("3", output);
        }

        [Fact]
        void function_can_receive_3_arguments()
        {
            string source = @"
                fun f3(a: int, b: int, c: int): int { return a + b + c; }
                print f3(1, 2, 3);
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("6", output);
        }

        [Fact]
        void function_can_receive_4_arguments()
        {
            string source = @"
                fun f4(a: int, b: int, c: int, d: int): int { return a + b + c + d; }
                print f4(1, 2, 3, 4);
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("10", output);
        }

        [Fact]
        void function_can_receive_5_arguments()
        {
            string source = @"
                fun f5(a: int, b: int, c: int, d: int, e: int): int { return a + b + c + d + e; }
                print f5(1, 2, 3, 4, 5);
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("15", output);
        }

        [Fact]
        void function_can_receive_6_arguments()
        {
            string source = @"
                fun f6(a: int, b: int, c: int, d: int, e: int, f: int): int { return a + b + c + d + e + f; }
                print f6(1, 2, 3, 4, 5, 6);
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("21", output);
        }

        [Fact]
        void function_can_receive_7_arguments()
        {
            string source = @"
                fun f7(a: int, b: int, c: int, d: int, e: int, f: int, g: int): int { return a + b + c + d + e + f + g; }
                print f7(1, 2, 3, 4, 5, 6, 7);
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("28", output);
        }

        [Fact]
        void function_can_receive_8_arguments()
        {
            string source = @"
                fun f8(a: int, b: int, c: int, d: int, e: int, f: int, g: int, h: int): int { return a + b + c + d + e + f + g + h; }
                print f8(1, 2, 3, 4, 5, 6, 7, 8);
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("36", output);
        }

        [Fact]
        void function_can_receive_binary_expression_as_argument()
        {
            string source = @"
                var a = 1;

                fun f(i: int): int { return (i / 1024); }
                print f(a * 1024);
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("1", output);
        }

        [Fact]
        void function_can_receive_function_call_result_as_argument()
        {
            string source = @"
                fun f(i: int): int { return i * 42; }
                fun g(): int { return 42; }

                print f(g());
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("1764", output);
        }

        // "Negative" tests, asserting that errors are handled in a deterministic way.
        //

        [Fact]
        void extra_arguments_expects_runtime_error()
        {
            string source = @"
                fun f(a: int, b: int): void {
                    print a;
                    print b;
                }

                f(1, 2, 3, 4);
            ";

            var result = EvalWithTypeValidationErrorCatch(source);
            var exception = result.TypeValidationErrors.First();

            Assert.Single(result.TypeValidationErrors);
            Assert.Matches("Function 'f' has 2 parameter\\(s\\) but was called with 4 argument\\(s\\)",
                exception.Message);
        }

        [Fact]
        void missing_arguments_expects_runtime_error()
        {
            string source = @"
                fun f(a: int, b: int): void {}

                f(1);
            ";

            var result = EvalWithTypeValidationErrorCatch(source);
            var exception = result.TypeValidationErrors.First();

            Assert.Single(result.TypeValidationErrors);
            Assert.Matches("Function 'f' has 2 parameter\\(s\\) but was called with 1 argument\\(s\\)",
                exception.Message);
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
