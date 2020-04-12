using System.Linq;
using System.Text;
using Perlang.Interpreter;
using Xunit;
using static Perlang.Tests.EvalHelper;

namespace Perlang.Tests.Stdlib
{
    public class Base64DecodeTests
    {
        // [Fact]
        // public void base64_decode_is_a_callable()
        // {
        //     Assert.IsAssignableFrom<ICallable>(Eval("base64_decode"));
        // }

        [Fact]
        public void base64_decode_with_no_arguments_throws_the_expected_exception()
        {
            var result = EvalWithTypeValidationErrorCatch("base64_decode()");
            var exception = result.TypeValidationErrors.First();

            Assert.Single(result.TypeValidationErrors);
            Assert.Contains("Function 'base64_decode' has 1 parameter(s) but was called with 0 argument(s)", exception.Message);
        }

        [Fact]
        public void base64_decode_with_a_string_argument_returns_the_expected_result()
        {
            Assert.Equal("hej hej", Eval("base64_decode(\"aGVqIGhlag==\")"));
        }

        [Fact]
        public void base64_decode_with_a_numeric_argument_throws_the_expected_exception()
        {
            var result = EvalWithTypeValidationErrorCatch("base64_decode(123.45)");
            var runtimeError = result.TypeValidationErrors.First();

            Assert.Single(result.TypeValidationErrors);

            Assert.Equal("Cannot pass System.Double argument as System.String parameter to base64_decode()",
                runtimeError.Message);
        }
    }
}
