using FluentAssertions;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Arrays;

public class StringArrayTests
{
    [Fact(Skip = "Requires string array support")]
    public void string_array_can_be_indexed()
    {
        string source = """
                var a: string[] = [""a"", ""b"", ""c""];

                print a[0];
                print a[1];
                print a[2];
                """;

        var output = EvalReturningOutput(source);

        output.Should()
            .BeEquivalentTo(
                "a",
                "b",
                "c"
            );
    }
}
