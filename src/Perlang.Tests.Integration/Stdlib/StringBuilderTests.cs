using FluentAssertions;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Stdlib;

public class StringBuilderTests
{
    [Fact]
    public void StringBuilder_append_supports_char_parameter()
    {
        string source = """
            var sb = new StringBuilder();

            sb.append('x');
            print sb.to_string();
            """;

        string output = EvalReturningOutputString(source);

        output.Should()
            .Be("x");
    }
}
