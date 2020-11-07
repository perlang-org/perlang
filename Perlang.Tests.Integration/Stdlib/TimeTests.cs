using System;
using Perlang.Interpreter;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Stdlib
{
    public class TimeTests
    {
        [Fact]
        public void Time_now_is_defined()
        {
            Assert.IsAssignableFrom<TargetAndMethodContainer>(Eval("Time.now"));
        }

        [Fact]
        public void Time_now_returns_a_DateTime_value()
        {
            Assert.IsType<DateTime>(Eval("Time.now()"));
        }

        // TODO: Simplify this to Time.now.ticks once we have property getter support in place
        // TODO: https://github.com/perlang-org/perlang/issues/114
        [Fact]
        public void Time_now_get_Ticks_returns_a_value_greater_than_zero()
        {
            Assert.True((long) Eval("Time.now().get_Ticks()") > 0);
        }

        [Fact]
        public void Time_now_ticks_returns_a_value_greater_than_zero()
        {
            Assert.True((long) Eval("Time.now().ticks()") > 0);
        }
    }
}
