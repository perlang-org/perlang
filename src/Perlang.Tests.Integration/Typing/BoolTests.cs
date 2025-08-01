using FluentAssertions;
using Xunit;

namespace Perlang.Tests.Integration.Typing;

public class BoolTests
{
    [Fact]
    public void bool_variable_can_be_explicitly_typed()
    {
        string source = """
            var b: bool = true;

            print(b);
            """;

        var output = EvalHelper.EvalReturningOutputString(source);

        output.Should()
            .Be("true");
    }

    [Fact]
    public void bool_variable_can_be_implicitly_typed()
    {
        string source = """
            var b = true;

            print(b);
            """;

        var output = EvalHelper.EvalReturningOutputString(source);

        output.Should()
            .Be("true");
    }
}
