using System.Linq;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.ReservedKeywords
{
    public class ReservedKeywordsTests
    {
        [Fact]
        public void reserved_keyword_class_throws_expected_error()
        {
            string source = @"
                class Foo {}
            ";

            var result = EvalWithParseErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Matches("Error at 'class': Expect expression", exception.ToString());
        }

        [Fact]
        public void function_return_type_detects_reserved_keywords()
        {
            // 'float' is not a valid type name yet, so this test expects a particular error to be thrown.
            // For more details on reserved words, see #178. (Technically, this test does not exercise the "reserved
            // words" code paths at all; since float isn't defined, it fails as it would with any other type.)
            string source = @"
                fun foo(): float {
                    return 123.45;
                }
            ";

            var result = EvalWithValidationErrorCatch(source);
            var exception = result.Errors.FirstOrDefault();

            Assert.Single(result.Errors);
            Assert.Matches("Type not found: float", exception.ToString());
        }
    }
}
