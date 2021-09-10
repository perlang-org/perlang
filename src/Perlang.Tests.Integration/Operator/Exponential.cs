using System.Linq;
using System.Numerics;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Operator
{
    public class Exponential
    {
        //
        // Tests for the ** (exponential) operator. The operator works pretty much like in Ruby. An interesting detail
        // about it is that the returned value is a BigInteger, regardless of the size of the input operands. This
        // may sometimes be impractical, but for simple/REPL scenarios it is still basically useful.
        //
        [Fact]
        public void exponential_integer_literals()
        {
            string source = @"
                2 ** 10
            ";

            object result = Eval(source);

            Assert.Equal(new BigInteger(1024), result);
        }

        [Fact]
        public void exponential_positive_and_negative_integer_literals()
        {
            // This is clearly a bit of a weird case, and I'm not sure this is semantics we want to keep in the language
            // in the long run. BigInteger.Pow() only supports positive exponents, which is reasonable since negative
            // exponents are likely to lead to fractional values.
            //
            // We avoid this by using the double-based Math.Pow() method under the hood instead. This does mean that the
            // return value is _not_ an integer of any sort any more though; it silently becomes a double. I'm not sure
            // whether this is a good idea or not, but let's try it and see how it feels.

            string source = @"
                10 ** -3
            ";

            object result = Eval(source);

            Assert.Equal(0.001, result);
        }

        [Fact]
        public void exponential_negative_and_positive_integer_literals()
        {
            string source = @"
                -10 ** 3
            ";

            object result = Eval(source);

            Assert.Equal(new BigInteger(-1000), result);
        }

        [Fact]
        public void exponential_negative_and_positive_integer_literals_again()
        {
            // This tests an important edge case where we differ from MRI Ruby. Try it in 'irb' and you'll see what I
            // mean: MRI returns -59049 in this case.
            //
            // 2.6.3 :005 > -3 ** 10
            // => -59049
            //
            // I reckon this is because (surprise, surprise!) the ** operator seems to have _lower_ precedence in Ruby
            // than - (which seems to be an operator and not just a modifier to the integer). These examples illustrate
            // this further:
            //
            // 2.6.3 :007 > -(3 ** 10)
            // => -59049
            // 2.6.3 :008 > (-3) ** 10
            // => 59049
            //
            // I believe the semantics we have right now makes most sense, but feel free to challenge this if you think
            // this is a fallacy on my behalf.

            string source = @"
                -3 ** 10
            ";

            object result = Eval(source);

            Assert.Equal(new BigInteger(59049), result);
        }

        [Fact]
        public void exponential_integer_and_float_literals()
        {
            string source = @"
                10 ** 3.5
            ";

            object result = Eval(source);

            Assert.Equal(3162.2776601683795, result);
        }

        [Fact]
        public void exponential_integer_and_negative_float_literals()
        {
            string source = @"
                10 ** -3.5
            ";

            object result = Eval(source);

            Assert.Equal(0.00031622776601683794, result);
        }

        [Fact]
        public void exponential_integer_literals_and_multiplication()
        {
            string source = @"
                2 ** 10 * 2
            ";

            object result = Eval(source);

            Assert.Equal(new BigInteger(2048), result);
        }

        [Fact]
        public void exponential_integer_literal_and_function_return_value()
        {
            string source = @"
                fun foo(): int { return 16; }

                print 2 ** foo();
            ";

            string result = EvalReturningOutputString(source);

            Assert.Equal("65536", result);
        }

        [Fact]
        public void exponential_integer_literals_as_variable_initializer()
        {
            string source = @"
                var x = 2 ** 12;
                print x;
            ";

            string result = EvalReturningOutputString(source);

            Assert.Equal("4096", result);
        }

        [Fact]
        public void exponential_function_return_value_and_integer_literal()
        {
            string source = @"
                fun foo(): int { return 4; }

                print foo() ** 8;
            ";

            string result = EvalReturningOutputString(source);

            Assert.Equal("65536", result);
        }

        [Fact]
        public void exponential_function_return_values()
        {
            string source = @"
                fun base(): int { return 2; }
                fun exponent(): int { return 8; }

                print base() ** exponent();
            ";

            string result = EvalReturningOutputString(source);

            Assert.Equal("256", result);
        }

        [Fact]
        public void exponential_string_and_integer_throws_expected_error()
        {
            string source = @"
                ""string"" ** 10
            ";

            var result = EvalWithValidationErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Equal("Invalid arguments to ** operator specified: System.String and System.Int32", exception.Message);
        }

        [Fact]
        public void exponential_integer_and_string_throws_expected_error()
        {
            string source = @"
                10 ** ""string""
            ";

            var result = EvalWithValidationErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Equal("Invalid arguments to ** operator specified: System.Int32 and System.String", exception.Message);
        }

        [Fact]
        public void exponential_function_reference_and_literal_throws_expected_error()
        {
            string source = @"
                fun base(): int { return 2; }

                print base ** 8;
            ";

            var result = EvalWithRuntimeErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Matches("Operands must be numbers, not PerlangFunction and Int32", exception.Message);
        }
    }
}
