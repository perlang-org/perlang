using System.Linq;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.ReservedTokens
{
    public class ReservedTokensTests
    {
        [Fact]
        public void ampersand_throws_expected_error()
        {
            // Singe-ampersands will be used for bitwise AND operator.
            string source = @"
                var c = 127 & 63;
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
        public void pipe_throws_expected_error()
        {
            // The single-pipe (vertical bar) character will be used for bitwise OR operator.
            string source = @"
                var c = 127 | 255;
            ";

            var result = EvalWithParseErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Matches("Error at '|': Expect ';' after variable declaration", exception.ToString());
        }
    }
}
