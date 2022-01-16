using System.Linq;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.LogicalOperator
{
    // Tests based on Lox test suite:
    // https://github.com/munificent/craftinginterpreters/blob/master/test/logical_operator/or.lox
    // https://github.com/munificent/craftinginterpreters/blob/master/test/logical_operator/or_truth.lox
    public class Or
    {
        [Fact]
        public void returns_the_first_true_argument_1_or_true()
        {
            string source = @"
                print 1 || true;
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("1", output);
        }

        [Fact]
        public void returns_the_first_true_argument_false_or_1()
        {
            string source = @"
                print false || 1;
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("1", output);
        }

        [Fact]
        public void returns_the_first_true_argument_false_or_false_or_true()
        {
            string source = @"
                print false || false || true;
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("True", output);
        }

        [Fact]
        public void returns_the_last_argument_if_all_are_false_false_or_false()
        {
            string source = @"
                print false || false;
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("False", output);
        }

        [Fact]
        public void returns_the_last_argument_if_all_are_false_false_or_false_false()
        {
            string source = @"
                print false || false || false;
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("False", output);
        }

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

        // Note: we might want to tighten the semantics here so that 'bool or non-bool' throws a compile-time error.
        // These semantics were inherited from Lox, which is dynamically typed with no compile-time typechecking
        // whatsoever.
        [Fact]
        public void false_is_falsy()
        {
            string source = @"
                print false || ""ok"";
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("ok", output);
        }

        [Fact]
        public void null_is_falsy()
        {
            string source = @"
                print null || ""ok"";
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("ok", output);
        }

        [Fact]
        public void true_is_truthy()
        {
            string source = @"
                print true || ""ok"";
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("True", output);
        }

        [Fact]
        public void zero_is_truthy()
        {
            string source = @"
                print 0 || ""ok"";
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("0", output);
        }

        [Fact]
        public void strings_are_truthy()
        {
            string source = @"
                print ""s"" || ""ok"";
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("s", output);
        }
    }
}
