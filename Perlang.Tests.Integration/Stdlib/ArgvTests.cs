using System;
using System.Collections.Generic;
using System.Linq;
using Perlang.Interpreter;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Stdlib
{
    public class ArgvTests
    {
        [Fact]
        public void Argv_pop_is_defined()
        {
            Assert.IsAssignableFrom<TargetAndMethodContainer>(Eval("Argv.pop"));
        }

        [Fact]
        public void argv_pop_with_no_arguments_throws_the_expected_exception()
        {
            var result = EvalWithRuntimeCatch("Argv.pop()");
            var exception = result.RuntimeErrors.FirstOrDefault();

            Assert.Single(result.RuntimeErrors);
            Assert.Matches("No arguments left", exception.Message);
        }

        [Fact]
        public void argv_pop_with_one_argument_expr_returns_the_expected_result()
        {
            Assert.Equal("arg1", EvalWithArguments("Argv.pop()", "arg1"));
        }

        [Fact]
        public void argv_pop_with_one_argument_stmt_returns_the_expected_result()
        {
            var result = EvalReturningOutput("print Argv.pop();", "arg1").SingleOrDefault();

            Assert.Equal("arg1", result);
        }

        [Fact]
        public void argv_pop_with_multiple_arguments_returns_the_expected_result()
        {
            var output = new List<string>();

            EvalWithArguments("Argv.pop(); print Argv.pop();", s => output.Add(s), "arg1", "arg2");

            Assert.Equal("arg2", output.Single());
        }

        [Fact]
        public void argv_pop_too_many_times_throws_the_expected_exception()
        {
            var result = EvalWithRuntimeCatch("Argv.pop(); Argv.pop();", "arg1");
            var exception = result.RuntimeErrors.FirstOrDefault();

            Assert.Single(result.RuntimeErrors);
            Assert.Matches("No arguments left", exception.Message);
        }
    }
}
