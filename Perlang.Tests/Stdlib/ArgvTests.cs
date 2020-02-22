using System.Collections.Generic;
using System.Linq;
using Perlang.Exceptions;
using Perlang.Interpreter;
using Xunit;
using static Perlang.Tests.EvalHelper;

namespace Perlang.Tests.Stdlib
{
    public class ArgvTests
    {
        [Fact]
        public void argv_pop_is_a_callable()
        {
            Assert.IsAssignableFrom<ICallable>(Eval("argv_pop"));
        }

        [Fact]
        public void argv_pop_with_no_arguments_throws_the_expected_exception()
        {
            // TODO: Should verify exception message also ("argv_pop: No arguments left")
            Assert.Throws<RuntimeError>(() => Eval("argv_pop()"));
        }

        [Fact]
        public void argv_pop_with_one_argument_returns_the_expected_result()
        {
            Assert.Equal("arg1", EvalWithArguments("argv_pop()", "arg1"));
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
            // TODO: Should verify exception message also ("argv_pop: No arguments left")
            Assert.Throws<RuntimeError>(() => EvalWithArguments("argv_pop(); argv_pop();", "arg1"));
        }
    }
}