using FluentAssertions;
using Xunit;

namespace Perlang.Tests.Integration.Typing;

using static EvalHelper;

public class StringTests
{
    [SkippableFact]
    public void string_variable_can_be_printed()
    {
        Skip.If(PerlangMode.ExperimentalCompilation, "Not supported in compiled mode");

        string source = @"
                var s: string = ""this is a string"";

                print(s);
            ";

        var output = EvalReturningOutputString(source);

        output.Should()
            .Be("this is a string");
    }

    [SkippableFact]
    public void string_variable_can_be_reassigned()
    {
        Skip.If(PerlangMode.ExperimentalCompilation, "Not supported in compiled mode");

        string source = @"
                var s: string = ""this is a string"";
                s = ""this is another string"";

                print(s);
            ";

        var output = EvalReturningOutputString(source);

        output.Should()
            .Be("this is another string");
    }

    [SkippableFact]
    public void ascii_string_inferred_variable_can_be_reassigned_with_non_ascii_value()
    {
        // The code below is incredibly hard to support in compiled mode, because: an AsciiString cannot be assigned to a
        // String variable in C++ (because the latter is an abstract class; I believe the C++ compiler will try to make a
        // copy of it). `const perlang::string& s = ...` works, but then the problem is that the variable can obviously
        // not be reassigned on the second line... because it is constant. We'll have to think through how to solve this
        // properly.
        Skip.If(PerlangMode.ExperimentalCompilation, "Not yet supported in compiled mode");

        string source = @"
                var s: string = ""this is a string"";
                s = ""this is a string with non-ASCII characters: åäöÅÄÖéèüÜÿŸïÏ"";

                print(s);
            ";

        var output = EvalReturningOutputString(source);

        output.Should()
            .Be("this is a string with non-ASCII characters: åäöÅÄÖéèüÜÿŸïÏ");
    }

    [SkippableFact]
    public void ascii_string_variable_has_expected_type()
    {
        string source = @"
                var s: string = ""this is an ASCII string"";

                print(s.get_type());
            ";

        var output = EvalReturningOutputString(source);

        output.Should()
            .Be("Perlang.Lang.AsciiString");
    }
}
