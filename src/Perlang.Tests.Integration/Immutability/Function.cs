using System;
using System.Linq;
using FluentAssertions;
using Perlang.Compiler;
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

            if (PerlangMode.ExperimentalCompilation) {
                Action action = () => EvalReturningOutput(source);

                action.Should().Throw<PerlangCompilerException>()
                    .WithMessage("*Function 'f' is already defined*");
            }
            else {
                var result = EvalWithRuntimeErrorCatch(source);
                var exception = result.Errors.First();

                Assert.Single(result.Errors);
                Assert.Matches("Variable with this name already declared in this scope.", exception.Message);
            }
        }

        [SkippableFact]
        public void function_cannot_be_overwritten_by_a_variable()
        {
            Skip.If(PerlangMode.ExperimentalCompilation, "In compiled mode, functions are shadowed by variables.");

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
                f = 123;
            ";

            var result = EvalWithValidationErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Matches("Function 'f' is immutable and cannot be modified.", exception.Message);
        }
    }
}
