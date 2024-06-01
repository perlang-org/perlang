using FluentAssertions;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Arrays;

public class IntArrayTests
{
    [Fact]
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

    [Fact]
    public void indexing_int_array_with_negative_index_produces_expected_error()
    {
        string source = """
                var a = [1, 2, 3];

                print a[-1];
                """;

        var result = EvalWithRuntimeErrorCatch(source);

        result.Errors.Should()
            .ContainSingle()
            .Which
            .Message.Should().Contain("exited with exit code 134");

        result.OutputAsString.Should()
            .Contain("index out of range (18446744073709551615 > 2)");
    }

    [Fact]
    public void indexing_int_array_outside_of_boundaries_produces_expected_error()
    {
        string source = """
                var a = [1, 2, 3];

                // a[2] is the last element of the array
                print a[3];
                """;

        var result = EvalWithRuntimeErrorCatch(source);

        result.Errors.Should()
            .ContainSingle()
            .Which
            .Message.Should().Contain("exited with exit code 134");

        result.OutputAsString.Should()
            .Contain("index out of range (3 > 2)");
    }
}
