using Xunit;
using static Perlang.Tests.EvalHelper;

namespace Perlang.Tests.Stdlib
{
    public class ClockTests
    {
        [Fact]
        public void clock_is_a_callable()
        {
            Assert.IsAssignableFrom<ICallable>(Eval("clock"));
        }

        [Fact]
        public void clock_returns_a_double_value()
        {
            Assert.IsType<double>(Eval("clock()"));
        }

        [Fact]
        public void clock_returns_a_value_greater_than_zero()
        {
            Assert.True((double) Eval("clock()") > 0);
        }
    }
}
