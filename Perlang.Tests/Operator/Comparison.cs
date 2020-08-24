using System.Linq;
using Xunit;
using static Perlang.Tests.EvalHelper;

namespace Perlang.Tests.Operator
{
    // Tests based on https://github.com/munificent/craftinginterpreters/blob/master/test/operator/comparison.lox
    public class Comparison
    {
        //
        // Tests for the < (less than) operator
        //

        [Fact]
        public void less_than_greater_is_true()
        {
            string source = @"
                var b = 1 < 2;
                print b;
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("True", output);
        }

        [Fact]
        public void less_than_same_is_false()
        {
            string source = @"
                var b = 2 < 2;
                print b;
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("False", output);
        }

        [Fact]
        public void less_than_smaller_is_false()
        {
            string source = @"
                var b = 2 < 1;
                print b;
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("False", output);
        }

        //
        // Tests for the <= (less than or equals) operator
        //

        [Fact]
        public void less_than_or_equals_greater_is_true()
        {
            string source = @"
                var b = 1 <= 2;
                print b;
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("True", output);
        }

        [Fact]
        public void less_than_or_equals_same_is_true()
        {
            string source = @"
                var b = 2 <= 2;
                print b;
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("True", output);
        }

        [Fact]
        public void less_than_or_equals_smaller_is_false()
        {
            string source = @"
                var b = 2 <= 1;
                print b;
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("False", output);
        }

        //
        // Tests for the > (greater than) operator
        //

        [Fact]
        public void greater_than_smaller_is_false()
        {
            string source = @"
                var b = 1 > 2;
                print b;
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("False", output);
        }

        [Fact]
        public void greater_than_same_is_false()
        {
            string source = @"
                var b = 2 > 2;
                print b;
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("False", output);
        }

        [Fact]
        public void greater_than_smaller_is_true()
        {
            string source = @"
                var b = 2 > 1;
                print b;
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("True", output);
        }

        //
        // Tests for the >= (greater than or equals) operator
        //
        [Fact]
        public void greater_than_or_equals_smaller_is_false()
        {
            string source = @"
                var b = 1 >= 2;
                print b;
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("False", output);
        }

        [Fact]
        public void greater_than_or_equals_same_is_true()
        {
            string source = @"
                var b = 2 >= 2;
                print b;
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("True", output);
        }

        [Fact]
        public void greater_than_or_equals_smaller_is_true()
        {
            string source = @"
                var b = 2 >= 1;
                print b;
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("True", output);
        }

        //
        // Special tests for 0 and -0, ensuring that positive and negative zero compares as the same value.
        //
        [Fact(Skip = "Fails with 'Operand must be a number' error")]
        public void zero_less_than_negative_zero_is_false()
        {
            string source = @"
                var b = 0 < -0;
                print b;
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("False", output);
        }

        [Fact(Skip = "Fails with 'Operand must be a number' error")]
        public void negative_zero_less_than_zero_is_false()
        {
            string source = @"
                var b = -0 < 0;
                print b;
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("False", output);
        }

        [Fact(Skip = "Fails with 'Operand must be a number' error")]
        public void zero_greater_than_negative_zero_is_false()
        {
            string source = @"
                var b = 0 > -0;
                print b;
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("False", output);
        }

        [Fact(Skip = "Fails with 'Operand must be a number' error")]
        public void negative_zero_greater_than_zero_is_false()
        {
            string source = @"
                var b = -0 > 0;
                print b;
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("False", output);
        }

        [Fact(Skip = "Fails with 'Operand must be a number' error")]
        public void zero_less_than_or_equals_negative_zero_is_false()
        {
            string source = @"
                var b = 0 <= -0;
                print b;
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("True", output);
        }

        [Fact(Skip = "Fails with 'Operand must be a number' error")]
        public void negative_zero_less_than_or_equals_zero_is_false()
        {
            string source = @"
                var b = -0 <= 0;
                print b;
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("False", output);
        }

        [Fact(Skip = "Fails with 'Operand must be a number' error")]
        public void zero_greater_than_or_equals_negative_zero_is_false()
        {
            string source = @"
                var b = 0 >= -0;
                print b;
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("False", output);
        }

        [Fact(Skip = "Fails with 'Operand must be a number' error")]
        public void negative_zero_greater_than_or_equals_zero_is_false()
        {
            string source = @"
                var b = -0 >= 0;
                print b;
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("False", output);
        }
    }
}
