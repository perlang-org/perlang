using AwesomeAssertions;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Print;

public class PrintTests
{
    [Fact]
    public void print_without_parameters_outputs_empty_line()
    {
        string source = """
            print;
            """;

        var output = EvalReturningOutput(source);

        output.Should()
            .ContainSingle()
            .Which.Should().BeEmpty();
    }
}
