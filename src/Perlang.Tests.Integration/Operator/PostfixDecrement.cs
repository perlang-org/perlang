using System.Linq;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Operator
{
    public class PostfixDecrement
    {
        //
        // "Positive" tests, testing for supported behavior
        //

        [Fact]
        public void decrementing_defined_variable()
        {
            string source = @"
                var i = 0;
                i--;
                print i;
            ";

            var output = EvalReturningOutput(source);

            Assert.Equal(new[] { "-1" }, output);
        }

        [Fact]
        public void decrement_can_be_used_in_for_loops()
        {
            string source = @"
                for (var c = 3; c > 0; c--)
                    print c;
            ";

            var output = EvalReturningOutput(source);

            Assert.Equal(new[]
            {
                "3",
                "2",
                "1"
            }, output);
        }

        [Fact]
        public void decrement_can_be_used_in_assignment()
        {
            string source = @"
                var i = 100;
                var j = i--;
                print j;
            ";

            var output = EvalReturningOutput(source).SingleOrDefault();

            // As in languages C# and Java (and unlike C and C++), the operation above has well-defined semantics.
            // The value of i++ is the value of the expression _before_ it gets evaluated, just like in those other
            // languages. If we had a prefix decrement operator, it would differ in this regard.
            Assert.Equal("100", output);
        }

        //
        // "Negative tests", ensuring that unsupported operations fail in the expected way.
        //

        [Fact]
        public void decrementing_undefined_variable_throws_expected_exception()
        {
            string source = @"
                x--;
            ";

            var result = EvalWithValidationErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Matches("Undefined identifier 'x'", exception.Message);
        }

        [Fact]
        public void decrementing_null_variable_throws_expected_exception()
        {
            string source = @"
                var s: string = null;
                s--;
            ";

            var result = EvalWithRuntimeErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Matches("can only be used to decrement numbers, not null", exception.Message);
        }

        [Fact]
        public void decrementing_string_throws_expected_exception()
        {
            string source = @"
                var s = ""foo"";
                s--;
            ";

            var result = EvalWithRuntimeErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Matches("can only be used to decrement numbers, not String", exception.Message);
        }
    }
}
