using Xunit;
using static Perlang.Tests.EvalHelper;

namespace Perlang.Tests.Stdlib
{
    public class PrintfTests
    {
        [Fact]
        public void printf_is_a_callable()
        {
            Assert.IsAssignableFrom<ICallable>(Eval("printf"));
        }

        [Fact]
        public void printf_without_formatting_outputs_the_expected_value()
        {
            var output = EvalReturningOutput("printf(\"Hello, World\");");

            Assert.Equal(output, new[] {"Hello, World"});
        }

        [Fact]
        public void printf_with_percent_d_at_end_outputs_the_expected_value()
        {
            var output = EvalReturningOutput("printf(\"Hello %d\", 42);");

            Assert.Equal(output, new[] {"Hello 42"});
        }

        [Fact]
        public void printf_with_percent_d_in_the_middle_outputs_the_expected_value()
        {
            var output = EvalReturningOutput("printf(\"Hello %d World\", 42);");

            Assert.Equal(output, new[] {"Hello 42 World"});
        }
    }
}
