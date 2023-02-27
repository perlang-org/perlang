using FluentAssertions;
using Xunit;

namespace Perlang.Tests.Integration.Typing;

using static Perlang.Tests.Integration.EvalHelper;

public class StringTests
{
    [Fact]
    public void string_variable_can_be_printed()
    {
        string source = @"
                var s: string = ""this is a string"";

                print(s);
            ";

        var output = EvalReturningOutputString(source);

        output.Should()
            .Be("this is a string");
    }

    [Fact]
    public void string_variable_can_be_reassigned()
    {
        string source = @"
                var s: string = ""this is a string"";
                s = ""this is another string"";

                print(s);
            ";

        var output = EvalReturningOutputString(source);

        output.Should()
            .Be("this is another string");
    }

    [Fact]
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
