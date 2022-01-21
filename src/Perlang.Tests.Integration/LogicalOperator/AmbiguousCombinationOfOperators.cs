using System.Linq;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.LogicalOperator
{
    public class AmbiguousCombinationOfOperators
    {
        [Fact]
        public void combined_and_and_or_operators_emits_expected_warning_for_statement()
        {
            // The following program is valid in many other languages. In Perlang, however, we want to prevent users
            // from writing code like this since it is potentially very ambiguous to the reader. The example below is
            // a seemingly trivial and harmless example; however, real-world scenarios can be more complex and delicate.
            //
            // Given that this design decision is potentially a source of controversy, it felt most reasonable to make
            // this a compilation warning (and warnings are errors by default, as they ought to be) that could
            // potentially be disabled for potential users which strongly oppose this decision.
            string source = @"
                var a = false && false || true;
            ";

            var result = EvalWithResult(source);
            var compilerWarning = result.CompilerWarnings.FirstOrDefault();

            Assert.Single(result.CompilerWarnings);
            Assert.Matches("Invalid combination of boolean operators: && and ||", compilerWarning.Message);
        }

        [Fact]
        public void combined_and_and_or_operators_emits_expected_warning_for_expression()
        {
            // Like the above, but for programs which consist of a single expression. (We should really unify these code
            // paths at some point, to make tests like this irrelevant.)
            string source = @"
                false && false || true
            ";

            var result = EvalWithResult(source);
            var compilerWarning = result.CompilerWarnings.FirstOrDefault();

            Assert.Single(result.CompilerWarnings);
            Assert.Matches("Invalid combination of boolean operators: && and ||", compilerWarning.Message);
        }

        [Fact]
        public void grouped_and_and_or_operators_does_not_emit_warnings()
        {
            // Adding some parentheses () for grouping makes the above example perfectly legitimate.
            //
            // Note that the example has deliberately been selected to have a different value than the example above. &&
            // has higher precedence than || in most languages, meaning that the first example will evaluate to `true`.
            // It is precisely this distinction between `(false && false) || true` and `false && (false || true)` that we
            // want to highlight and force the user to pay extra attention to, especially since boolean operators with
            // implicit precedence are hard for some people to use correctly; they can easily be a source of subtle
            // bugs.
            string source = @"
                var a = false && (false || true);
                print a;
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("False", output);
        }
    }
}
