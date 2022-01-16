using System.Linq;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.LogicalOperator
{
    // Tests based on Lox test suite:
    // https://github.com/munificent/craftinginterpreters/blob/master/test/logical_operator/and.lox
    // https://github.com/munificent/craftinginterpreters/blob/master/test/logical_operator/and_truth.lox
    public class And
    {
        [Fact]
        public void returns_the_first_falsy_argument_false_and_1()
        {
            string source = @"
                print false and 1;
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("False", output);
        }

        [Fact]
        public void returns_the_first_falsy_argument_true_and_1()
        {
            string source = @"
                print true and 1;
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("1", output);
        }

        [Fact]
        public void returns_the_first_falsy_argument_1_and_2_and_false()
        {
            string source = @"
                print 1 and 2 and false;
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("False", output);
        }

        [Fact]
        public void returns_the_last_argument_if_all_are_truthy_1_and_true()
        {
            string source = @"
                print 1 and true;
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("True", output);
        }

        [Fact]
        public void returns_the_last_argument_if_all_are_truthy_1_and_2_and_3()
        {
            string source = @"
                print 1 and 2 and 3;
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("3", output);
        }

        [Fact]
        public void short_circuits_at_the_first_falsy_argument()
        {
            string source = @"
                var a = false;
                var b = true;

                (a = true) and
                    (b = false) and
                    (a = false);

                print a;
                print b;
            ";

            var output = EvalReturningOutput(source);

            Assert.Equal(new[]
            {
                "True",
                "False" // The `a = "bad" assignment should never execute
            }, output);
        }

        // Note: we might want to tighten the semantics here so that 'bool or non-bool' throws a compile-time error.
        // These semantics were inherited from Lox, which is dynamically typed with no compile-time typechecking
        // whatsoever.
        [Fact]
        public void false_is_falsy()
        {
            string source = @"
                print false and ""bad"";
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("False", output);
        }

        [Fact]
        public void null_is_falsy()
        {
            string source = @"
                print null and ""bad"";
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("null", output);
        }

        [Fact]
        public void true_is_truthy()
        {
            string source = @"
                print true and ""ok"";
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("ok", output);
        }

        [Fact]
        public void zero_is_truthy()
        {
            string source = @"
                print 0 and ""ok"";
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("ok", output);
        }

        [Fact]
        public void empty_string_is_truthy()
        {
            string source = @"
                print """" and ""ok"";
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("ok", output);
        }
    }
}
