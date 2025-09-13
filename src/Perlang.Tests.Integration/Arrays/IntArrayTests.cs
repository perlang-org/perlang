using FluentAssertions;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Arrays;

public class IntArrayTests
{
    [Fact]
    public void explicitly_typed_int_array_with_initializer_can_be_indexed()
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
    public void explicitly_typed_int_array_with_fixed_size_can_be_indexed()
    {
        string source = """
            var a: int[] = new int[3];

            a[0] = 111;
            a[1] = 222;
            a[2] = 333;

            print a[0];
            print a[1];
            print a[2];
            """;

        var output = EvalReturningOutput(source);

        output.Should()
            .BeEquivalentTo(
                "111",
                "222",
                "333"
            );
    }

    [Fact]
    public void implicitly_typed_int_array_with_initializer_can_be_indexed()
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
    public void implicitly_typed_int_array_with_fixed_size_can_be_indexed()
    {
        string source = """
            var a = new int[3];

            a[0] = 111;
            a[1] = 222;
            a[2] = 333;

            print a[0];
            print a[1];
            print a[2];
            """;

        var output = EvalReturningOutput(source);

        output.Should()
            .BeEquivalentTo(
                "111",
                "222",
                "333"
            );
    }

    [Fact]
    public void implicitly_typed_int_array_with_fixed_size_has_expected_initial_content()
    {
        string source = """
            var a = new int[3];

            // No assignment to the individual elements takes place, but the language guarantees that they are all
            // initialized to the default value (0).

            print a[0];
            print a[1];
            print a[2];
            """;

        var output = EvalReturningOutput(source);

        output.Should()
            .BeEquivalentTo(
                "0",
                "0",
                "0"
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
    public void indexing_int_array_with_initializer_outside_of_boundaries_produces_expected_error()
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

    [Fact]
    public void indexing_int_array_with_fixed_size_outside_of_boundaries_produces_expected_error()
    {
        string source = """
            var a = new int[3];

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

    [Fact]
    public void indexing_uninitialized_int_array_with_fixed_size_produces_expected_error()
    {
        string source = """
            var a: int[];

            print a[0];
            """;

        var result = EvalWithRuntimeErrorCatch(source);

        result.Errors.Should()
            .ContainSingle()
            .Which
            .Message.Should().Contain("exited with exit code 134");

        // Ideally, this would be detected by the compiler, but the problem is that the 'var a: int[]' part above gets
        // translated to 'std::shared_ptr<perlang::IntArray> a'. In other words, a local stack-allocated std::shared_ptr
        // which is initialized with a null pointer. Regretfully, this is not an "uninitialized" variable per se but a
        // variable initialized to null, so we don't get any help by the C++ compiler in this case.
        result.OutputAsString.Should()
            .Contain("terminate called after throwing an instance of 'perlang::NullPointerException");
    }
}
