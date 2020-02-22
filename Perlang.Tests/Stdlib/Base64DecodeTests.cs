using System.Linq;
using System.Text;
using Perlang.Interpreter;
using Xunit;
using static Perlang.Tests.EvalHelper;

namespace Perlang.Tests.Stdlib
{
    public class Base64DecodeTests
    {
        [Fact]
        public void base64_decode_is_a_callable()
        {
            Assert.IsAssignableFrom<ICallable>(Eval("base64_decode"));
        }

        [Fact]
        public void base64_decode_with_no_arguments_throws_the_expected_exception()
        {
            var result = EvalWithRuntimeCatch("base64_decode()");
            var exception = result.RuntimeErrors.First();

            Assert.Single(result.RuntimeErrors);
            Assert.IsType<RuntimeError>(exception);
            Assert.Contains("Expected 1 argument(s)", exception.Message);
        }

        [Fact]
        public void base64_decode_with_a_string_argument_returns_the_expected_result()
        {
            Assert.Equal("hej hej", Eval("base64_decode(\"aGVqIGhlag==\")"));
        }

        [Fact]
        public void base64_decode_with_a_numeric_argument_throws_the_expected_exception()
        {
            var result = EvalWithRuntimeCatch("base64_decode(123.45)");
            var runtimeError = result.RuntimeErrors.First();

            Assert.Single(result.RuntimeErrors);

            Assert.Equal("base64_decode: Unable to cast object of type 'System.Double' to type 'System.String'.",
                runtimeError.Message);
        }
    }
}
