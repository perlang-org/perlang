using System.Linq;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Immutability
{
    public class Function
    {
        // Functions are inherently immutable. Once defined, a function cannot be redefined with another function body.

        [Fact]
        public void function_cannot_be_overwritten_by_another_function_with_the_same_name()
        {
            string source = @"
                fun f(): void {}
                fun f(): void {}
            ";

            var result = EvalWithRuntimeErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Matches("Variable with this name already declared in this scope.", exception.Message);
        }

        [Fact]
        public void function_cannot_be_overwritten_by_a_variable()
        {
            string source = @"
                fun f(): void {}
                var f = 42;
            ";

            var result = EvalWithRuntimeErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Matches("Variable with this name already declared in this scope.", exception.Message);
        }

        [Fact]
        public void function_cannot_be_overwritten_by_assignment()
        {
            string source = @"
                fun f(): void {}
                f = 42;
            ";

            var result = EvalWithValidationErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Matches("Function 'f' is immutable and cannot be modified.", exception.Message);
        }
    }
}
