using System;
using FluentAssertions;
using Perlang.Compiler;
using Perlang.Interpreter;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.InlineCpp;

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

    // Note about using the extern keyword: the compiler will perform absolutely NO VALIDATION WHATSOEVER that the
    // class definition matches the C++ implementation. However, a C++ class definition will be generated from this
    // and added to the header file, so in case the C++ code is implemented in the same file (as in this example),
    // all will be fine. If it's defined in another compilation unit though, you could very well run into ANY KIND
    // OF WEIRD CRASHES, BUGS ETC when using this functionality. It's an extremely powerful feature and can be very
    // useful but should be handled with care.

    [Fact(DisplayName = "C++ method in 'extern' class can be called from Perlang code outside the class")]
    public void cpp_method_in_extern_class_can_be_called_from_perlang_code_outside_the_class()
    {
        string source = """
            #c++-prototypes
            #include <iostream>
            #/c++-prototypes

            public extern class CppClass
            {
                public cpp_method(): void;
            }

            // TODO: Would be nice to be able to put this in an explicit 'main' method, but it's not currently
            // possible since it causes a conflict with top-level statements. This forces us to write the code in a
            // slightly messy way.
            var cpp = new CppClass();
            cpp.cpp_method();

            #c++-methods
            void CppClass::cpp_method()
            {
                std::cout << "Hello World from cpp_method" << std::endl;
            }
            #/c++-methods
            """;

        var output = EvalReturningOutput(source);

        output.Should()
            .Equal("Hello World from cpp_method");
    }

    [Fact(DisplayName = "C++ method marked as 'extern' can be called from Perlang code inside the same class")]
    public void cpp_method_marked_as_extern_can_be_called_from_perlang_code_inside_the_same_class()
    {
        string source = """
            #c++-prototypes
            #include <iostream>
            #/c++-prototypes

            public class Greeter
            {
                public extern cpp_method(): void;
            
                public greet(): void
                {
                    cpp_method();
                }
            }

            var greeter = new Greeter();
            greeter.greet();

            #c++-methods
            void Greeter::cpp_method()
            {
                std::cout << "Hello World from cpp_method" << std::endl;
            }
            #/c++-methods
            """;

        var output = EvalReturningOutput(source);

        output.Should()
            .Equal("Hello World from cpp_method");
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