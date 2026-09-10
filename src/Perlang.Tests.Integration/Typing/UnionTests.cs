using System;
using FluentAssertions;
using Perlang.Compiler;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Typing;

public class UnionTests
{
    [Fact]
    public void is_char_returns_true_for_char_union_variable_with_char_value()
    {
        string source = """
            var c: char | null = 'x';

            if (c is char v) {
                print(v);
            }
            """;

        var output = EvalReturningOutputString(source);

        output.Should()
            .Be("x");
    }

    [Fact]
    public void is_char_returns_false_for_null_value()
    {
        string source = """
            var c: char | null = null;

            if (c is char v) {
                print(v);
            }
            """;

        var output = EvalReturningOutputString(source);

        // Nothing is expected to be printed because the 'is' comparison is expected to evaluate to false
        output.Should()
            .BeEmpty();
    }

    [Fact]
    public void is_char_can_specify_variable_with_expected_value_for_char_union_variable_with_char_value()
    {
        string source = """
            var c: char | null = 'x';

            if (c is char v) {
                print("not null");
            }
            """;

        var output = EvalReturningOutputString(source);

        output.Should()
            .Be("not null");
    }

    [Fact]
    public void is_char_can_specify_variable_for_false_scenario()
    {
        string source = """
            var c: char | null = null;

            if (c is char) {
                print("null");
            }
            """;

        var output = EvalReturningOutputString(source);

        // Nothing is expected to be printed because the 'is' comparison is expected to evaluate to false
        output.Should()
            .BeEmpty();
    }

    [Fact]
    public void is_char_can_not_use_specified_variable_in_else_branch()
    {
        string source = """
            var c: char | null = 'x';

            if (c is char v) {
                print(v);
            }
            else {
                // Expected to be a compilation error, since 'v' is not in scope here
                print(v);
            }
            """;

        var result = EvalWithValidationErrorCatch(source);

        result.Errors.Should()
            .ContainSingle()
            .Which
            .Message.Should()
            .Match("Undefined identifier 'v'");
    }

    [Fact]
    public void null_comparison_returns_false_for_char_union_variable_with_char_value()
    {
        string source = """
            var c: char | null = 'x';

            if (c == null) {
                print("null");
            }
            """;

        var output = EvalReturningOutputString(source);

        // Nothing is expected to be printed because c is non-null
        output.Should()
            .BeEmpty();
    }

    [Fact]
    public void null_comparison_returns_true_for_null_value()
    {
        string source = """
            var c: char | null = null;

            if (c == null) {
                print("null");
            }
            """;

        var output = EvalReturningOutputString(source);

        output.Should()
            .Be("null");
    }

    [Fact]
    public void is_char_returns_false_for_char_union_variable_assigned_to_null_after_initialization()
    {
        string source = """
            var c: char | null = 'x';

            c = null;

            if (c is char v) {
                print(v);
            }
            """;

        var output = EvalReturningOutputString(source);

        // Expected to be empty since the 'is' check is expected to have evaluated to false in this case.
        output.Should()
            .BeEmpty();
    }

    [Fact]
    public void function_with_nullable_union_return_type_can_return_non_null()
    {
        string source = """
            fun f(): char | null
            {
                return 'x';
            }

            var c = f();

            if (c is char v) {
                print(v);
            }
            """;

        var output = EvalReturningOutputString(source);

        output.Should()
            .Be("x");
    }

    [Fact]
    public void function_with_nullable_union_return_type_can_return_null()
    {
        string source = """
            fun f(): char | null
            {
                return null;
            }

            var c = f();

            if (c is char v) {
                print(v);
            }
            """;

        var output = EvalReturningOutputString(source);

        // Expected to be empty since the method returned 'null'
        output.Should()
            .BeEmpty();
    }

    [Fact]
    public void function_with_nullable_union_parameter_can_receive_non_null_parameter()
    {
        string source = """
            fun f(c: char | null): void
            {
                if (c is char d) {
                    print(d);
                }
            }

            f('x');
            """;

        var output = EvalReturningOutputString(source);

        output.Should()
            .Be("x");
    }

    [Fact]
    public void setting_non_union_variable_to_null_emits_expected_error()
    {
        string source = """
            // Expected to fail since 'char' is not nullable
            var c: char = null;
            """;

        Action action = () => EvalReturningOutput(source);

        // Relying on the raw C++ compilation error is admittedly rough, but better than nothing for now.
        action.Should().Throw<PerlangCompilerException>()
            .WithMessage("*cannot initialize a variable of type 'char16_t' with an rvalue of type 'std::nullptr_t'*");
    }
}
