using System;
using Perlang.Interpreter;
using Perlang.Parser;
using Xunit;

namespace Perlang.Tests
{
    public class Precedence
    {
        [Fact]
        public void multiply_has_higher_precedence_than_plus()
        {
            Assert.Equal(14.0, Evaluate("2 + 3 * 4"));
        }

        [Fact]
        public void multiply_has_higher_precedence_than_minus()
        {
            Assert.Equal(8.0, Evaluate("20 - 3 * 4"));
        }

        [Fact]
        public void divide_has_higher_precedence_than_plus()
        {
            Assert.Equal(4.0, Evaluate("2 + 6 / 3"));
        }

        [Fact]
        public void divide_has_higher_precedence_than_minus()
        {
            Assert.Equal(0.0, Evaluate("2 - 6 / 3"));
        }

        [Fact]
        public void less_than_has_higher_precedence_than_equals_equals()
        {
            Assert.Equal(true, Evaluate("false == 2 < 1"));
        }

        [Fact]
        public void greater_than_has_higher_precedence_than_equals_equals()
        {
            Assert.Equal(true, Evaluate("false == 1 > 2"));
        }

        [Fact]
        public void less_than_or_equals_has_higher_precedence_than_equals_equals()
        {
            EvaluateEquals(true, "false == 2 <= 1");
        }

        [Fact]
        public void greater_than_or_equals_has_higher_precedence_than_equals_equals()
        {
            EvaluateEquals(true, "false == 1 >= 2");
        }

        [Fact]
        public void one_minus_one_is_not_space_sensitive()
        {
            EvaluateEquals(0.0, "1 - 1");
            EvaluateEquals(0.0, "1 -1");
            EvaluateEquals(0.0, "1- 1");
            EvaluateEquals(0.0, "1-1");
        }

        [Fact]
        public void parentheses_can_be_used_for_grouping()
        {
            EvaluateEquals(4.0, "(2 * (6 - (2 + 2)))");
        }

        private void EvaluateEquals(object expected, string expression)
        {
            Assert.Equal(expected, Evaluate(expression));
        }

        private object Evaluate(string code)
        {
            var interpreter = new PerlangInterpreter(AssertFailRuntimeErrorHandler);
            return interpreter.Eval(code, AssertFailScanErrorHandler, AssertFailParseErrorHandler,
                AssertFailResolveErrorHandler);
        }

        private static void AssertFailScanErrorHandler(ScanError scanError)
        {
            MoreAssert.Fail(scanError.ToString());
        }

        private static void AssertFailParseErrorHandler(Token token, string message, ParseErrorType? parseerrortype)
        {
            throw new NotImplementedException();
        }

        private static void AssertFailResolveErrorHandler(Token token, string message)
        {
            throw new NotImplementedException();
        }

        private static void AssertFailRuntimeErrorHandler(RuntimeError runtimeError)
        {
            MoreAssert.Fail(runtimeError.ToString());
        }
    }
}