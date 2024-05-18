using System;
using FluentAssertions;
using Perlang.Compiler;
using Perlang.Interpreter;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.InlineCpp
{
    /// <summary>
    /// Inline C++ tests.
    /// </summary>
    public class InlineCppTests
    {
        [Fact(DisplayName = "C++ can be used to implement the `main` method")]
        public void cpp_can_be_used_to_implement_the_main_method()
        {
            string source = """
                #c++-prototypes
                int main();
                #/c++-prototypes

                #c++-methods
                int main()
                {
                    puts("main method implemented in C++");
                    return 0;
                }
                #/c++-methods
                """;

            if (PerlangMode.ExperimentalCompilation) {
                var output = EvalReturningOutput(source, CompilerFlags.RemoveEmptyMainMethod);

                output.Should()
                    .Equal("main method implemented in C++");
            }
            else {
                Action action = () => EvalWithRuntimeErrorCatch(source);

                action.Should()
                    .Throw<PerlangInterpreterException>()
                    .WithMessage("C++ code is not supported in interpreted mode");
            }
        }

        [Fact(DisplayName = "C++ method can be called from Perlang code", Skip = "Needs `extern` keyword")]
        public void cpp_method_can_be_called_from_perlang_code()
        {
            // TODO: Even if/when we can get the parser to consume the "raw" C++ code and handle it correctly, there's
            // TODO: still a challenge here. The Perlang code doesn't know anything about this method. How could we change
            // TODO: this? We would need something like an `extern` keyword or so, so you can write
            // TODO: `extern fun native_method(): void;`. This is probably the "easiest" way out of this. As a workaround
            // TODO: for now, we could skip this test and add another test method which just has a C++ `main` function
            // TODO: which prints a message as step 1. We can then fix `extern` in a separate PR.
            string source = """
                #c++-prototypes
                static void cpp_method();
                #/c++prototypes

                extern fun cpp_method(): void;

                fun main(): void
                {
                    cpp_method();
                }

                #c++-methods
                void cpp_method()
                {
                    puts(""cpp_method output"");
                }
                #/c++methods
                """;

            var output = EvalReturningOutput(source);

            output.Should()
                .Equal("cpp_method output");
        }

        [Fact(DisplayName = "C++ method can contain C++ comment")]
        public void cpp_method_can_contain_cpp_comment()
        {
            string source = """
                #c++-methods
                void native_main(int argc, const char **argv)
                {
                    // TODO: range check
                    const char* first_argument = argv[0];
                }
                #/c++-methods
                """;

            // The above code used to give a 'Expected '/c++-methods' but got '// TODO: range check'.' error.
            var result = EvalWithParseErrorCatch(source);

            result.Errors.Should()
                .BeEmpty();
        }
    }
}
