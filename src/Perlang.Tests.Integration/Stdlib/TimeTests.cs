using System;
using FluentAssertions;
using Perlang.Interpreter;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Stdlib
{
    public class TimeTests
    {
        [SkippableFact]
        public void Time_now_is_defined()
        {
            Assert.IsAssignableFrom<TargetAndMethodContainer>(Eval("Time.now"));
        }

        [SkippableFact]
        public void Time_now_returns_a_DateTime_value()
        {
            Assert.IsType<DateTime>(Eval("Time.now()"));
        }

        // TODO: Simplify this to Time.now.ticks once we have property getter support in place
        // TODO: https://gitlab.perlang.org/perlang/perlang/-/issues/114
        [SkippableFact]
        public void Time_now_get_Ticks_returns_a_value_greater_than_zero()
        {
            Skip.If(PerlangMode.ExperimentalCompilation, "Not supported in compiled mode");

            string source = @"
               print Time.now().get_Ticks() > 0;
            ";

            string output = EvalReturningOutputString(source);

            // "True" in interpreted mode and "true" in compiled mode
            output.ToLower().Should()
                .Be("true");
        }

        [SkippableFact]
        public void Time_now_ticks_returns_a_value_greater_than_zero()
        {
            Skip.If(PerlangMode.ExperimentalCompilation, "Not supported in compiled mode");

            string source = @"
               print Time.now().ticks() > 0;
            ";

            string output = EvalReturningOutputString(source);

            // "True" in interpreted mode and "true" in compiled mode
            output.ToLower().Should()
                .Be("true");
        }
    }
}
