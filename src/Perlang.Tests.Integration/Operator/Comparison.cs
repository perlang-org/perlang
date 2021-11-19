using System.Collections.Generic;
using System.Linq;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Operator
{
    // Tests based on https://github.com/munificent/craftinginterpreters/blob/master/test/operator/comparison.lox
    public class Comparison
    {
        public static readonly List<object[]> ComparisonTypes = new()
        {
            new object[] { "int", "int" },
            new object[] { "int", "long" },
            new object[] { "long", "int" },
            new object[] { "long", "long" }
        };

        //
        // Tests for the < (less than) operator
        //

        [Theory]
        [MemberData(nameof(ComparisonTypes))]
        public void less_than_greater_is_true(string left, string right)
        {
            string source = $@"
                var left: {left} = 1;
                var right: {right} = 2;
                var b = left < right;
                print b;
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("True", output);
        }

        [Theory]
        [MemberData(nameof(ComparisonTypes))]
        public void less_than_same_is_false(string left, string right)
        {
            string source = $@"
                var left: {left} = 2;
                var right: {right} = 2;
                var b = left < right;
                print b;
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("False", output);
        }

        [Theory]
        [MemberData(nameof(ComparisonTypes))]
        public void less_than_smaller_is_false(string left, string right)
        {
            string source = $@"
                var left: {left} = 2;
                var right: {right} = 1;
                var b = left < right;
                print b;
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("False", output);
        }

        //
        // Tests for the <= (less than or equals) operator
        //

        [Theory]
        [MemberData(nameof(ComparisonTypes))]
        public void less_than_or_equals_greater_is_true(string left, string right)
        {
            string source = $@"
                var left: {left} = 1;
                var right: {right} = 2;
                var b = left <= right;
                print b;
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("True", output);
        }

        [Theory]
        [MemberData(nameof(ComparisonTypes))]
        public void less_than_or_equals_same_is_true(string left, string right)
        {
            string source = $@"
                var left: {left} = 2;
                var right: {right} = 2;
                var b = left <= right;
                print b;
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("True", output);
        }

        [Theory]
        [MemberData(nameof(ComparisonTypes))]
        public void less_than_or_equals_smaller_is_false(string left, string right)
        {
            string source = $@"
                var left: {left} = 2;
                var right: {right} = 1;
                var b = left <= right;
                print b;
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("False", output);
        }

        //
        // Tests for the > (greater than) operator
        //

        [Theory]
        [MemberData(nameof(ComparisonTypes))]
        public void greater_than_smaller_is_false(string left, string right)
        {
            string source = $@"
                var left: {left} = 1;
                var right: {right} = 2;
                var b = left > right;
                print b;
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("False", output);
        }

        [Theory]
        [MemberData(nameof(ComparisonTypes))]
        public void greater_than_same_is_false(string left, string right)
        {
            string source = $@"
                var left: {left} = 2;
                var right: {right} = 2;
                var b = left > right;
                print b;
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("False", output);
        }

        [Theory]
        [MemberData(nameof(ComparisonTypes))]
        public void greater_than_smaller_is_true(string left, string right)
        {
            string source = $@"
                var left: {left} = 2;
                var right: {right} = 1;
                var b = left > right;
                print b;
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("True", output);
        }

        //
        // Tests for the >= (greater than or equals) operator
        //
        [Theory]
        [MemberData(nameof(ComparisonTypes))]
        public void greater_than_or_equals_larger_is_false(string left, string right)
        {
            string source = $@"
                var left: {left} = 1;
                var right: {right} = 2;
                var b = left >= right;
                print b;
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("False", output);
        }

        [Theory]
        [MemberData(nameof(ComparisonTypes))]
        public void greater_than_or_equals_same_is_true(string left, string right)
        {
            string source = $@"
                var left: {left} = 2;
                var right: {right} = 2;
                var b = left >= right;
                print b;
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("True", output);
        }

        [Theory]
        [MemberData(nameof(ComparisonTypes))]
        public void greater_than_or_equals_smaller_is_true(string left, string right)
        {
            string source = $@"
                var left: {left} = 2;
                var right: {right} = 1;
                var b = left >= right;
                print b;
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("True", output);
        }

        //
        // Special tests for 0.0 and -0.0, ensuring that positive and negative zero compares as the same value.
        //
        // Note that this is specifically only relevant for floats; its (IEEE 754) binary representation supports the
        // distinction between these numbers. For integers, this is not true; the binary representation of two's
        // complement (used to store negative integers on the majority of today's computer architectures) does not allow
        // the concept of negative zero.
        //
        // More on the subject:
        //
        // - Signed zero: https://en.wikipedia.org/wiki/Signed_zero
        // - Two's complement: https://en.wikipedia.org/wiki/Two%27s_complement
        //
        // Given the above, the actual semantics are pretty much the same for both integer and floats in this aspect. We
        // add a trivial integer test as well first, just for the sake of it.
        [Fact]
        public void zero_and_negative_zero_integers_are_identical()
        {
            // We deliberately do not check for equality, since even negative and positive zero floats are equal.
            // Instead, we check if one is smaller than the other.
            string source = @"
                0 < -0
            ";

            object output = Eval(source);

            Assert.Equal(false, output);
        }

        [Fact]
        public void zero_less_than_negative_zero_is_false()
        {
            string source = @"
                0.0 < -0.0
            ";

            object output = Eval(source);

            Assert.Equal(false, output);
        }

        [Fact]
        public void negative_zero_less_than_zero_is_false()
        {
            string source = @"
                -0.0 < 0.0
            ";

            object output = Eval(source);

            Assert.Equal(false, output);
        }

        [Fact]
        public void zero_greater_than_negative_zero_is_false()
        {
            string source = @"
                0.0 > -0.0
            ";

            object output = Eval(source);

            Assert.Equal(false, output);
        }

        [Fact]
        public void negative_zero_greater_than_zero_is_false()
        {
            string source = @"
                -0.0 > 0.0
            ";

            object output = Eval(source);

            Assert.Equal(false, output);
        }

        [Fact]
        public void zero_less_than_or_equals_negative_zero_is_false()
        {
            string source = @"
                0.0 <= -0.0
            ";

            object output = Eval(source);

            Assert.Equal(true, output);
        }

        [Fact]
        public void negative_zero_less_than_or_equals_zero_is_false()
        {
            string source = @"
                -0.0 <= 0.0
            ";

            object output = Eval(source);

            Assert.Equal(true, output);
        }

        [Fact]
        public void zero_greater_than_or_equals_negative_zero_is_false()
        {
            string source = @"
                0.0 >= -0.0
            ";

            object output = Eval(source);

            Assert.Equal(true, output);
        }

        [Fact]
        public void negative_zero_greater_than_or_equals_zero_is_false()
        {
            string source = @"
                -0.0 >= 0.0
            ";

            object output = Eval(source);

            Assert.Equal(true, output);
        }
    }
}
