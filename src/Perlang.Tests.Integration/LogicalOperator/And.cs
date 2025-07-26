using System.Linq;
using FluentAssertions;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.LogicalOperator
{
    // Tests originally based on Lox test suite:
    // https://github.com/munificent/craftinginterpreters/blob/master/test/logical_operator/and.lox
    // https://github.com/munificent/craftinginterpreters/blob/master/test/logical_operator/and_truth.lox
    //
    // ...but eventually most of those tests were removed, as our semantics started to diverge from those of Lox. (no
    // longer return the first falsy argument if false, the last truthy argument if true, etc. In general, provide a
    // much more C#/Java:esque experience than Lox.)
    public class And
    {
        [Fact]
        public void short_circuits_at_the_first_falsy_argument()
        {
            string source = @"
                var a = false;
                var b = true;

                (a = true) &&
                    (b = false) &&
                    (a = false);

                print a;
                print b;
            ";

            // "True" and "False" in interpreted mode, "true" and "false" in compiled mode
            var output = EvalReturningOutput(source)
                .Select(s => s.ToLower());

            Assert.Equal(new[]
            {
                "true",
                "false" // The `a = "bad" assignment should never execute
            }, output);
        }

        [Fact]
        public void true_is_truthy()
        {
            // In fact, true is the _only_ truthy value in Perlang. :-)
            string source = @"
                print true && true;
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            // "True" in interpreted mode and "true" in compiled mode
            output!.ToLower().Should()
                .Be("true");
        }

        [Fact]
        public void false_is_falsy()
        {
            // Just like with 'true', 'false' is the _only_ falsy value we accept. Anything else is a compile-time
            // error, just like we believe it ought to be.
            string source = @"
                print false && false;
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            // "False" in interpreted mode and "false" in compiled mode
            output!.ToLower().Should()
                .Be("false");
        }

        [Fact]
        public void null_is_not_a_valid_and_operand()
        {
            string source = @"
                print null && true;
            ";

            var result = EvalWithValidationErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Matches("'null' is not a valid && operand", exception.Message);
        }

        [Fact]
        public void integer_is_not_a_valid_and_operand()
        {
            string source = @"
                print 0 && false;
            ";

            var result = EvalWithValidationErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Matches("'int' is not a valid && operand", exception.Message);
        }

        [Fact]
        public void string_is_not_a_valid_and_operand()
        {
            string source = @"
                print """" && false;
            ";

            var result = EvalWithValidationErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Matches("'ASCIIString' is not a valid && operand", exception.Message);
        }

        [SkippableFact]
        public void result_of_and_is_boolean()
        {
            string source = @"
                var v = true && false;

                print(v.get_type());
            ";

            var output = EvalReturningOutputString(source);

            Assert.Equal("perlang.Bool", output);
        }
    }
}
