using AwesomeAssertions;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Typing;

public class UnionTests
{
    [Fact]
    public void method_returning_char_and_string_union_can_return_char()
    {
        string source = """
            fun f(): char | string
            {
                return 'a';
            }

            if (f() is char c) {
                print c;
            }
            """;

        var output = EvalReturningOutputString(source);

        output.Should()
            .Be("a");
    }

    [Fact]
    public void method_returning_char_and_string_union_can_return_string()
    {
        string source = """
            fun f(): char | string
            {
                return "this is a string";
            }

            if (f() is string s) {
                print s;
            }
            """;

        var output = EvalReturningOutputString(source);

        output.Should()
            .Be("this is a string");
    }

    [Fact]
    public void method_returning_char_and_string_union_cannot_return_null()
    {
        string source = """
            fun f(): char | string
            {
                // This is expected to generate a compilation error
                return null;
            }
            """;

        var result = EvalWithValidationErrorCatch(source);

        result.Errors.Should()
            .ContainSingle()
            .Which
            .Message.Should().Be("Function returning 'char | string' cannot return null");
    }

    [Fact]
    public void method_with_char_and_string_union_parameter_can_be_called_with_char_argument()
    {
        string source = """
            fun f(p: char | string): void
            {
                if (p is char c) {
                    print c;
                }
            }

            f('a');
            """;

        var output = EvalReturningOutputString(source);

        output.Should()
            .Be("a");
    }

    [Fact]
    public void method_with_char_and_string_union_parameter_can_be_called_with_string_argument()
    {
        string source = """"
            fun f(p: char | string): void
            {
                if (p is string s) {
                    print s;
                }
            }

            f("this is a string");
            """";

        var output = EvalReturningOutputString(source);

        output.Should()
            .Be("this is a string");
    }

    [Fact]
    public void method_with_char_and_string_union_parameter_cannot_be_called_with_null_argument()
    {
        string source = """
            fun f(p: char | string): void
            {
            }

            // This is expected to generate a compilation error
            f(null);
            """;

        var result = EvalWithValidationErrorCatch(source);

        result.Errors.Should()
            .ContainSingle()
            .Which
            .Message.Should().Be("Cannot pass null argument as parameter 'p: char | string' to f()");
    }

    [Fact]
    public void char_and_string_union_field_can_be_assigned_char_literal()
    {
        string source = """
            var v: char | string = 'a';

            if (v is char c) {
                print c;
            }
            """;

        var output = EvalReturningOutputString(source);

        output.Should()
            .Be("a");
    }

    [Fact]
    public void char_and_string_union_field_can_be_assigned_string()
    {
        string source = """
            var v: char | string = "this is a string";

            if (v is string s) {
                print s;
            }
            """;

        var output = EvalReturningOutputString(source);

        output.Should()
            .Be("this is a string");
    }

    [Fact]
    public void char_and_string_union_cannot_be_assigned_null()
    {
        string source = """
            // This is expected to generate a compilation error
            var v: char | string = null;
            """;

        var result = EvalWithValidationErrorCatch(source);

        result.Errors.Should()
            .ContainSingle()
            .Which
            .Message.Should().Be("Cannot assign null to char | string variable");
    }
}
