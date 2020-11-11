using System.Linq;
using System.Text;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Stdlib
{
    public class Base64EncodeTests
    {
        [Fact]
        public void Base64_encode_with_no_arguments_throws_the_expected_exception()
        {
            var result = EvalWithValidationErrorCatch("Base64.encode()");
            var exception = result.ValidationErrors.First();

            Assert.Single(result.ValidationErrors);
            Assert.Contains("Method 'encode' has 1 parameter(s) but was called with 0 argument(s)", exception.Message);
        }

        [Fact]
        public void Base64_encode_with_a_string_argument_returns_the_expected_result()
        {
            Assert.Equal("aGVqIGhlag==", Eval("Base64.encode(\"hej hej\")"));
        }

        [Fact]
        public void Base64_encode_with_a_long_string_argument_returns_the_expected_result()
        {
            var sb = new StringBuilder();

            for (int i = 0; i < 4; i++)
            {
                sb.Append("hej hej, hemskt mycket hej");
            }

            // At the moment, all lines are wrapped at every 76 characters. We could consider to make this configurable,
            // but it's awkward until we support method overloading.
            Assert.Equal(
                "aGVqIGhlaiwgaGVtc2t0IG15Y2tldCBoZWpoZWogaGVqLCBoZW1za3QgbXlja2V0IGhlamhlaiBo\r\n" +
                "ZWosIGhlbXNrdCBteWNrZXQgaGVqaGVqIGhlaiwgaGVtc2t0IG15Y2tldCBoZWou",
                Eval($"Base64.encode(\"{sb}.\")")
            );
        }

        [Fact]
        public void Base64_encode_with_a_numeric_argument_throws_the_expected_exception()
        {
            var result = EvalWithValidationErrorCatch("Base64.encode(123.45)");
            var runtimeError = result.ValidationErrors.First();

            Assert.Single(result.ValidationErrors);

            Assert.Equal("Cannot pass System.Double argument as System.String parameter to encode()", runtimeError.Message);
        }
    }
}
