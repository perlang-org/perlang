using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration
{
    // Based on https://github.com/munificent/craftinginterpreters/blob/master/test/precedence.lox
    public class Precedence
    {
        [Fact]
        public void multiply_has_higher_precedence_than_plus()
        {
            Assert.Equal(14, Eval("2 + 3 * 4"));
        }

        [Fact]
        public void multiply_has_higher_precedence_than_minus()
        {
            Assert.Equal(8, Eval("20 - 3 * 4"));
        }

        [Fact]
        public void divide_has_higher_precedence_than_plus()
        {
            Assert.Equal(4, Eval("2 + 6 / 3"));
        }

        [Fact]
        public void divide_has_higher_precedence_than_minus()
        {
            Assert.Equal(0, Eval("2 - 6 / 3"));
        }

        [Fact]
        public void less_than_has_higher_precedence_than_equals_equals()
        {
            Assert.Equal(true, Eval("false == 2 < 1"));
        }

        [Fact]
        public void greater_than_has_higher_precedence_than_equals_equals()
        {
            Assert.Equal(true, Eval("false == 1 > 2"));
        }

        [Fact]
        public void less_than_or_equals_has_higher_precedence_than_equals_equals()
        {
            Assert.Equal(true, Eval("false == 2 <= 1"));
        }

        [Fact]
        public void greater_than_or_equals_has_higher_precedence_than_equals_equals()
        {
            Assert.Equal(true, Eval("false == 1 >= 2"));
        }

        [Fact]
        public void one_minus_one_is_not_space_sensitive()
        {
            Assert.Equal(0, Eval("1 - 1"));
            Assert.Equal(0, Eval("1 -1"));
            Assert.Equal(0, Eval("1- 1"));
            Assert.Equal(0, Eval("1-1"));
        }

        [Fact]
        public void parentheses_can_be_used_for_grouping()
        {
            Assert.Equal(4, Eval("(2 * (6 - (2 + 2)))"));
        }
    }
}
