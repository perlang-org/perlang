using System.Collections.Generic;
using System.Linq;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Stdlib
{
    public class ArgvTests
    {
        // [Fact]
        // public void argv_pop_is_a_callable()
        // {
        //     Assert.IsAssignableFrom<ICallable>(Eval("argv_pop"));
        // }

        [Fact]
        public void argv_pop_with_no_arguments_throws_the_expected_exception()
        {
            var result = EvalWithRuntimeCatch("argv_pop()");
            var exception = result.RuntimeErrors.FirstOrDefault();

            Assert.Single(result.RuntimeErrors);
            Assert.Matches("No arguments left", exception.Message);
        }

        [Fact]
        public void argv_pop_with_one_argument_expr_returns_the_expected_result()
        {
            Assert.Equal("arg1", EvalWithArguments("argv_pop()", "arg1"));
        }

        [Fact]
        public void argv_pop_with_one_argument_stmt_returns_the_expected_result()
        {
            var result = EvalReturningOutput("print argv_pop();", "arg1").SingleOrDefault();

            Assert.Equal("arg1", result);
        }

        [Fact]
        public void argv_pop_with_multiple_arguments_returns_the_expected_result()
        {
            var output = new List<string>();

            EvalWithArguments("argv_pop(); print argv_pop();", s => output.Add(s), "arg1", "arg2");

            Assert.Equal("arg2", output.Single());
        }

        [Fact]
        public void argv_pop_too_many_times_throws_the_expected_exception()
        {
            var result = EvalWithRuntimeCatch("argv_pop(); argv_pop();", "arg1");
            var exception = result.RuntimeErrors.FirstOrDefault();

            Assert.Single(result.RuntimeErrors);
            Assert.Matches("No arguments left", exception.Message);
        }
    }
}
