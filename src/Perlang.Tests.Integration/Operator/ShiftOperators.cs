using System.Linq;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Operator
{
    public class ShiftOperators
    {
        public class ShiftLeftOperator
        {
            [Fact]
            public void with_integers_performs_left_shifting()
            {
                string source = @"
                    print 1 << 10;
                ";

                string result = EvalReturningOutputString(source);
                Assert.Equal("1024", result);
            }

            [Fact]
            public void takes_precedence_over_multiplication()
            {
                string source = @"
                    print 1 << 10 * 3;
                ";

                string result = EvalReturningOutputString(source);
                Assert.Equal("3072", result);
            }

            [Fact]
            public void takes_precedence_over_division()
            {
                string source = @"
                    print 1 << 10 / 3;
                ";

                // Since the operands are integers, the division is expected to be truncated to its integer portion.
                // (1024 / 3 = 341.333...)
                string result = EvalReturningOutputString(source);
                Assert.Equal("341", result);
            }

            [Fact]
            public void takes_precedence_over_addition()
            {
                string source = @"
                    print 1 << 10 + 3;
                ";

                string result = EvalReturningOutputString(source);
                Assert.Equal("1027", result);
            }

            [Fact]
            public void takes_precedence_over_subtraction()
            {
                string source = @"
                    print 1 << 10 - 3;
                ";

                string result = EvalReturningOutputString(source);
                Assert.Equal("1021", result);
            }

            [Fact]
            public void takes_precedence_over_modulo_operator()
            {
                string source = @"
                    print 1 << 10 % 1000;
                ";

                string result = EvalReturningOutputString(source);
                Assert.Equal("24", result);
            }

            [Fact]
            public void takes_precedence_over_power_operator()
            {
                string source = @"
                    print 1 << 2 ** 10;
                ";

                string result = EvalReturningOutputString(source);
                Assert.Equal("1048576", result);
            }

            [Fact]
            public void with_integer_and_string_throws_expected_error()
            {
                string source = @"
                    print 1 << ""foo"";
                ";

                var result = EvalWithValidationErrorCatch(source);
                var exception = result.Errors.FirstOrDefault();

                Assert.Single(result.Errors);
                Assert.Matches("Unsupported << operands specified: int and string", exception.Message);
            }
        }

        public class ShiftRightOperator
        {
            [Fact]
            public void with_integers_performs_right_shifting()
            {
                string source = @"
                    print 65536 >> 6;
                ";

                string result = EvalReturningOutputString(source);
                Assert.Equal("1024", result);
            }

            [Fact]
            public void takes_precedence_over_multiplication()
            {
                string source = @"
                    print 65536 >> 6 * 3;
                ";

                string result = EvalReturningOutputString(source);
                Assert.Equal("3072", result);
            }

            [Fact]
            public void takes_precedence_over_division()
            {
                string source = @"
                    print 65536 >> 6 / 3;
                ";

                // Since the operands are integers, the division is expected to be truncated to its integer portion.
                // (1024 / 3 = 341.333...)
                string result = EvalReturningOutputString(source);
                Assert.Equal("341", result);
            }

            [Fact]
            public void takes_precedence_over_addition()
            {
                string source = @"
                    print 65536 >> 6 + 3;
                ";

                string result = EvalReturningOutputString(source);
                Assert.Equal("1027", result);
            }

            [Fact]
            public void takes_precedence_over_subtraction()
            {
                string source = @"
                    print 65536 >> 6 - 3;
                ";

                string result = EvalReturningOutputString(source);
                Assert.Equal("1021", result);
            }

            [Fact]
            public void takes_precedence_over_modulo_operator()
            {
                string source = @"
                    print 65536 >> 6 % 1000;
                ";

                string result = EvalReturningOutputString(source);
                Assert.Equal("24", result);
            }

            [Fact]
            public void takes_precedence_over_power_operator()
            {
                string source = @"
                    print 1024 >> 8 ** 10;
                ";

                string result = EvalReturningOutputString(source);
                Assert.Equal("1048576", result);
            }

            [Fact]
            public void with_integer_and_string_throws_expected_error()
            {
                string source = @"
                    print 1 >> ""foo"";
                ";

                var result = EvalWithValidationErrorCatch(source);
                var exception = result.Errors.FirstOrDefault();

                Assert.Single(result.Errors);
                Assert.Matches("Unsupported >> operands specified: int and string", exception.Message);
            }
        }
    }
}
