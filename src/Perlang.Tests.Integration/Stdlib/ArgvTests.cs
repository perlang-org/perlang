using System.Linq;
using Perlang.Interpreter;
using Perlang.Stdlib;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Stdlib
{
    public class ArgvTests
    {
        [Fact]
        public void ARGV_is_defined()
        {
            Assert.IsAssignableFrom<Argv>(Eval("ARGV"));
        }

        [Fact]
        public void ARGV_pop_is_defined()
        {
            Assert.IsAssignableFrom<TargetAndMethodContainer>(Eval("ARGV.pop"));
        }

        [Fact]
        public void ARGV_pop_with_no_arguments_throws_the_expected_exception()
        {
            var result = EvalWithRuntimeErrorCatch("ARGV.pop()");
            var exception = result.Errors.FirstOrDefault();

            Assert.Single(result.Errors);
            Assert.Matches("No arguments left", exception.Message);
        }

        [Fact]
        public void ARGV_pop_with_one_argument_expr_returns_the_expected_result()
        {
            Assert.Equal("arg1", EvalWithArguments("ARGV.pop()", "arg1"));
        }

        [Fact]
        public void ARGV_pop_with_one_argument_stmt_returns_the_expected_result()
        {
            var result = EvalReturningOutput("print ARGV.pop();", "arg1").SingleOrDefault();

            Assert.Equal("arg1", result);
        }

        [Fact]
        public void ARGV_pop_with_multiple_arguments_returns_the_expected_result()
        {
            var result = EvalReturningOutput("ARGV.pop(); print ARGV.pop();", "arg1", "arg2").Single();

            Assert.Equal("arg2", result);
        }

        [Fact]
        public void ARGV_pop_too_many_times_throws_the_expected_exception()
        {
            var result = EvalWithRuntimeErrorCatch("ARGV.pop(); ARGV.pop();", "arg1");
            var exception = result.Errors.FirstOrDefault();

            Assert.Single(result.Errors);
            Assert.Matches("No arguments left", exception.Message);
        }
    }
}
