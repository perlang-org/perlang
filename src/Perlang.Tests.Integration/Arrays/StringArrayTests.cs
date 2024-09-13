using FluentAssertions;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Arrays;

public class StringArrayTests
{
    [Fact]
    public void string_array_with_ascii_content_can_be_indexed()
    {
        string source = """
                var a: string[] = ["a", "b", "c"];

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

    [Fact]
    public void string_array_with_utf8_content_can_be_indexed()
    {
        string source = """
                var a: string[] = ["å", "ä", "ö", "ü", "ÿ", "Ÿ", "す", "し"];

                print a[0];
                print a[1];
                print a[2];
                print a[7];
                """;

        var output = EvalReturningOutput(source);

        output.Should()
            .BeEquivalentTo(
                "å",
                "ä",
                "ö",
                "し"
            );
    }

    [Fact]
    public void implicitly_typed_string_array_with_ascii_content_can_be_indexed()
    {
        string source = """
                var a = ["a", "b", "c"];

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

    [Fact]
    public void implicitly_typed_string_array_with_utf8_content_can_be_indexed()
    {
        string source = """
                var a = ["å", "ä", "ö", "ü", "ÿ", "Ÿ", "す", "し"];

                print a[0];
                print a[1];
                print a[2];
                print a[7];
                """;

        var output = EvalReturningOutput(source);

        output.Should()
            .BeEquivalentTo(
                "å",
                "ä",
                "ö",
                "し"
            );
    }

    [Fact]
    public void string_array_length_property_returns_expected_value()
    {
        string source = """
                var a: string[] = ["a", "b", "c"];

                print a.length;
                """;

        var output = EvalReturningOutput(source);

        output.Should()
            .BeEquivalentTo(
                "3"
            );
    }

    [Fact]
    public void string_array_nonexistent_property_raises_expected_error()
    {
        string source = """
                var a: string[] = ["a", "b", "c"];

                print a.non_existent_property;
                """;

        var result = EvalWithValidationErrorCatch(source);

        // Worth pointing out here is that the error comes from TypeResolver, which relies on some CLR reflection at the
        // moment. Once we have migrated away from this, we should be able to get a slightly different error message. :)
        result.Errors.Should()
            .ContainSingle()
            .Which
            .Message.Should().Contain("Failed to locate method 'non_existent_property'");
    }
}
