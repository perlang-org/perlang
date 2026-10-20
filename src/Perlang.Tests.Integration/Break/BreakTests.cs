using AwesomeAssertions;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Break;

public class BreakTests
{
    [Fact]
    public void break_at_top_level_emits_expected_error()
    {
        string source = """
            break;
            """;

        var result = EvalWithNameResolutionErrorCatch(source);

        result.Errors.Should()
            .ContainSingle()
            .Which
            .Message.Should().Contain("'break' can only be used inside a 'for', 'switch' or 'while' statement");
    }

    [Fact]
    public void break_in_if_statement_at_top_level_emits_expected_error()
    {
        string source = """
            var b = true;

            if (b) {
                break;
            }
            """;

        var result = EvalWithNameResolutionErrorCatch(source);

        result.Errors.Should()
            .ContainSingle()
            .Which
            .Message.Should().Contain("'break' can only be used inside a 'for', 'switch' or 'while' statement");
    }

    [Fact]
    public void break_after_switch_statement_emits_expected_error()
    {
        string source = """
            var i: int = 123;

            switch (i) {
                case 123:
                    print "one two three";
            }

            break;
            """;

        var result = EvalWithNameResolutionErrorCatch(source);

        result.Errors.Should()
            .ContainSingle()
            .Which
            .Message.Should().Contain("'break' can only be used inside a 'for', 'switch' or 'while' statement");
    }

    [Fact]
    public void break_in_function_emits_expected_error()
    {
        string source = """
            fun f(): void
            {
                break;
            }

            f();
            """;

        var result = EvalWithNameResolutionErrorCatch(source);

        result.Errors.Should()
            .ContainSingle()
            .Which
            .Message.Should().Contain("'break' can only be used inside a 'for', 'switch' or 'while' statement");
    }

    [Fact]
    public void break_in_instance_method_emits_expected_error()
    {
        string source = """
            public class Greeter
            {
                public say_hello(): void
                {
                    break;
                }
            }

            var greeter: Greeter = new Greeter();
            greeter.say_hello();
            """;

        var result = EvalWithNameResolutionErrorCatch(source);

        result.Errors.Should()
            .ContainSingle()
            .Which
            .Message.Should().Contain("'break' can only be used inside a 'for', 'switch' or 'while' statement");
    }

    [Fact]
    public void break_can_be_used_for_early_exit_in_while_loop()
    {
        string source = """
            var i = 0;

            while (true) {
                if (i == 3) {
                    break;
                }

                print i;
                i++;
            }
            """;

        var output = EvalReturningOutput(source);

        output.Should()
            .Equal("0", "1", "2");
    }

    [Fact]
    public void break_can_be_used_for_early_exit_in_for_loop()
    {
        string source = """
            for (var i = 0; i < 10; i++) {
                if (i == 3) {
                    break;
                }

                print i;
            }
            """;

        var output = EvalReturningOutput(source);

        output.Should()
            .Equal("0", "1", "2");
    }

    [Fact]
    public void break_in_string_switch_inside_loop_exits_switch_and_not_loop()
    {
        string source = """
            var s: string = "alpha";

            while (true) {
                switch (s) {
                    case "alpha":
                        break;
                }

                print "after switch";
                break;
            }
            """;

        var output = EvalReturningOutputString(source);

        output.Should()
            .Be("after switch");
    }
}
