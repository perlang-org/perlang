using System.Linq;
using Perlang.Interpreter;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Stdlib
{
    public class Base64DecodeTests
    {
        [SkippableFact]
        public void Base64_decode_is_defined()
        {
            Assert.IsAssignableFrom<TargetAndMethodContainer>(Eval("Base64.decode"));
        }

        [Fact]
        public void Base64_decode_with_no_arguments_throws_the_expected_exception()
        {
            var result = EvalWithValidationErrorCatch("Base64.decode()");
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Contains("Method 'decode' has 1 parameter(s) but was called with 0 argument(s)", exception.Message);
        }

        [SkippableFact]
        public void Base64_decode_with_a_padded_string_argument_returns_the_expected_result()
        {
            Assert.Equal("hej hej", Eval("Base64.decode(\"aGVqIGhlag==\")"));
        }

        [SkippableFact]
        public void Base64_decode_with_an_non_padded_string_argument_returns_the_expected_result()
        {
            // This used to fail at one point, which is why we added a test for it.
            Assert.Equal("hej hej", Eval("Base64.decode(\"aGVqIGhlag\")"));
        }

        [Fact]
        public void Base64_decode_with_a_numeric_argument_throws_the_expected_exception()
        {
            var result = EvalWithValidationErrorCatch("Base64.decode(123)");
            var runtimeError = result.Errors.First();

            Assert.Single(result.Errors);

            Assert.Equal("Cannot pass int argument as string parameter to decode()", runtimeError.Message);
        }
    }
}
