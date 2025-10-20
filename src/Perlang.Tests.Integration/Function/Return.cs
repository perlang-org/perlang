using FluentAssertions;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Function;

public class Return
{
    [Fact]
    public void function_can_return_values_in_if_statement()
    {
        string source = """
            fun f(i: int): int {
                if (i < 10) return i;
                if (i >= 10) return i / 2;

                // Will never be reached
                return -1;
            }

            print f(5);
            print f(15);
            """;

        var output = EvalReturningOutput(source);

        output.Should()
            .BeEquivalentTo(
                "5",
                "7"
            );
    }

    [Fact]
    public void function_can_return_value_from_binary_expression()
    {
        string source = """
            fun is_alpha(c: char): bool
            {
                return (c >= 'a' && c <= 'z') ||
                       (c >= 'A' && c <= 'Z');
            }

            print is_alpha('j');
            print is_alpha('0');
            print is_alpha('し'); // Expected to be false, since our method above only supports ASCII characters
            """;

        var output = EvalReturningOutput(source);

        output.Should()
            .BeEquivalentTo(
                "true",
                "false",
                "false"
            );
    }
}
