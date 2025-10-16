using FluentAssertions;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Typing;

/// <summary>
/// Tests for the `object` data type. `object` is equivalent to `perlang::Object` and represents the "root" type for all
/// objects in the unified Perlang type system (much like how C#/.NET works). Value types like `int` and `long` can be
/// implicitly converted to this type, with implicit boxing taking place to make it function like a normal reference
/// type.
/// </summary>
public class ObjectTests
{
    [Fact]
    public void object_variable_has_expected_type_when_initialized_with_integer()
    {
        string source = """
            var o: object = 103;

            print(o.get_type());
            """;

        string output = EvalReturningOutputString(source);

        output.Should()
            .Be("perlang.Integer");
    }

    [Fact]
    public void object_variable_with_integer_content_can_be_printed()
    {
        string source = """
            var o: object = 103;

            print(o);
            """;

        string output = EvalReturningOutputString(source);

        output.Should()
            .Be("103");
    }

    [Fact]
    public void object_variable_with_string_content_can_be_printed()
    {
        string source = """
            var o: object = "this is a string";

            print(o);
            """;

        string output = EvalReturningOutputString(source);

        output.Should()
            .Be("this is a string");
    }

    [Fact]
    public void function_can_take_object_parameter()
    {
        string source = """
            fun say(o: object): void
            {
                print(o);
            }

            var o: object = "Hello, World";

            say(o);
            """;

        string output = EvalReturningOutputString(source);

        output.Should()
            .Be("Hello, World");
    }
}
