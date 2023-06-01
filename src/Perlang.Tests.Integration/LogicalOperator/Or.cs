using System.Linq;
using FluentAssertions;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.LogicalOperator
{
    // Tests originally based on Lox test suite:
    // https://github.com/munificent/craftinginterpreters/blob/master/test/logical_operator/or.lox
    // https://github.com/munificent/craftinginterpreters/blob/master/test/logical_operator/or_truth.lox
    //
    // ...but eventually most of those tests were removed, as our semantics started to diverge from those of Lox. (no
    // longer return the first truthy argument if true, or the last argument if false, etc. In general, provide a much
    // more C#/Java:esque experience than Lox.)
    public class Or
    {
        [Fact]
        public void short_circuits_at_the_first_true_argument()
        {
            string source = @"
                var a = true;
                var b = false;

                (a = false) ||
                    (b = true) ||
                    (a = true);
                
                print a;
                print b;
            ";

            var output = EvalReturningOutput(source);

            Assert.Equal(new[]
            {
                "False", // The `a = true` assignment should never execute
                "True"
            }, output);
        }

        [Fact]
        public void true_is_truthy()
        {
            // In fact, true is the _only_ truthy value in Perlang. :-)
            string source = @"
                print true || false;
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            output!.Should()
                .Be("True");
        }

        [Fact]
        public void false_is_falsy()
        {
            // Just like with 'true', 'false' is the _only_ falsy value we accept. Anything else is a compile-time
            // error, just like we believe it ought to be.
            string source = @"
                print false || false;
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            output!.Should()
                .Be("False");
        }

        [Fact]
        public void null_is_not_a_valid_or_operand()
        {
            string source = @"
                print null || true;
            ";

            var result = EvalWithValidationErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Matches("'null' is not a valid || operand", exception.Message);
        }

        [Fact]
        public void integer_is_not_a_valid_and_operand()
        {
            string source = @"
                print 0 || false;
            ";

            var result = EvalWithValidationErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Matches("'int' is not a valid || operand", exception.Message);
        }

        [Fact]
        public void string_is_not_a_valid_and_operand()
        {
            string source = @"
                print ""s"" || false;
            ";

            var result = EvalWithValidationErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Matches("'string' is not a valid || operand", exception.Message);
        }

        [Fact]
        public void result_of_or_is_boolean()
        {
            string source = @"
                var v = true || false;

                print(v.get_type());
            ";

            var output = EvalReturningOutputString(source);

            Assert.Equal("System.Boolean", output);
        }
    }
}
