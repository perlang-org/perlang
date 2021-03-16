using System.Linq;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.LogicalOperator
{
    // Tests based on Lox test suite:lo https://github.com/munificent/craftinginterpreters/blob/master/test/logical_operator/or.lox
    public class Or
    {
        [Fact]
        public void returns_the_first_true_argument_1_or_true()
        {
            string source = @"
                print 1 or true;
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("1", output);
        }

        [Fact]
        public void returns_the_first_true_argument_false_or_1()
        {
            string source = @"
                print false or 1;
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("1", output);
        }

        [Fact]
        public void returns_the_first_true_argument_false_or_false_or_true()
        {
            string source = @"
                print false or false or true;
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("True", output);
        }

        [Fact]
        public void returns_the_last_argument_if_all_are_false_false_or_false()
        {
            string source = @"
                print false or false;
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("False", output);
        }

        [Fact]
        public void returns_the_last_argument_if_all_are_false_false_or_false_false()
        {
            string source = @"
                print false or false or false;
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("False", output);
        }

        [Fact]
        public void short_circuits_at_the_first_true_argument()
        {
            string source = @"
                var a = ""before"";
                var b = ""before"";
                (a = false) or
                    (b = true) or
                    (a = ""bad"");
                
                print a;
                print b;
            ";

            var output = EvalReturningOutput(source);

            Assert.Equal(new[]
            {
                "False",
                "True" // The a = "bad" assignment should never execute
            }, output);
        }
    }
}
