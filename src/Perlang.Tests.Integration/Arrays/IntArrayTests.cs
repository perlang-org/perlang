using FluentAssertions;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Arrays;

public class IntArrayTests
{
    [Fact(Skip = "Explicitly typed arrays are not yet supported")]
    public void explicitly_typed_int_array_can_be_indexed()
    {
        string source = """
                var a: int[] = [1, 2, 3];

                print a[0];
                print a[1];
                print a[2];
                """;

        var output = EvalReturningOutput(source);

        output.Should()
            .BeEquivalentTo(
                "1",
                "2",
                "3"
            );
    }

    [Fact]
    public void implicitly_typed_int_array_can_be_indexed()
    {
        string source = """
                var a = [1, 2, 3];

                print a[0];
                print a[1];
                print a[2];
                """;

        var output = EvalReturningOutput(source);

        output.Should()
            .BeEquivalentTo(
                "1",
                "2",
                "3"
            );
    }

    // TODO: Add test for indexing before and after array size, ensuring that we get the expected exceptions
}
