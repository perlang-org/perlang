using System.Linq;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.ReservedTokens
{
    public class ReservedTokensTests
    {
        [Fact]
        public void single_quote_throws_expected_error()
        {
            // Single quotes are currently unused, but will likely be used to support C/C#/Java-style single characters.
            string source = @"
                var c = 'x';
            ";

            var result = EvalWithParseErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Matches("Error at ''': Expect expression", exception.ToString());
        }

        [Fact]
        public void ampersand_throws_expected_error()
        {
            // Ampersands will be used for bitwise and boolean AND operators.
            string source = @"
                var c = true && false;
            ";

            var result = EvalWithParseErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Matches("Error at '&': Expect ';' after variable declaration", exception.ToString());
        }

        [Fact]
        public void question_mark_throws_expected_error()
        {
            // We currently do not support ternary expressions, but we might end up supporting it. Another use for the ?
            // character could be ?? (null coalesce, C#-style) or `foo?.bar?.baz` (null-safe navigation, also C#-style)
            // - other languages like JavaScript and Kotlin also implement similar things.
            string source = @"
                var c = true ? 1 : 2;
            ";

            var result = EvalWithParseErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Matches("Error at '\\?': Expect ';' after variable declaration", exception.ToString());
        }

        [Fact]
        public void vertical_bar_throws_expected_error()
        {
            // The vertical bar (pipe) character wil be used for bitwise and boolean AND operators.
            string source = @"
                var c = 123 | 456;
            ";

            var result = EvalWithParseErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Matches("Error at '|': Expect ';' after variable declaration", exception.ToString());
        }
    }
}
