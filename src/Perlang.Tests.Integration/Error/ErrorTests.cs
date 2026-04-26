using FluentAssertions;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Error;

public class ErrorTests
{
    [Fact]
    public void methods_with_error_union_type_can_return_error()
    {
        string source = """
            private static is_digit(c: char, base: int): bool | error {
                switch (base)
                {
                    case 2:
                        return c == '0' || c == '1';
                    case 8:
                        return c >= '0' && c <= '7';
                    case 10:
                        return c >= '0' && c <= '9';
                   default:
                        // TODO: Change to "new ArgumentError("Base " + base + " is not supported");" once the bug with
                        // NameResolver.VisitNewExpression not calling base.VisitNewExpression has been fixed.
                        return new ArgumentError("Unsupported base");
                };
            }

            // Unsupported base - this is expected to return an error, which must be either handled or propagated
            // through. For now, we don't have a way to explicitly handle errors; only propagating them using the "try"
            // prefix is supported.
            var result = try is_digit('a', 22);

            // Dummy code to avoid "unused variable" warning.
            print(result);
            """;

        var result = EvalWithRuntimeErrorCatch(source);

        result.Errors.Should()
            .ContainSingle()
            .Which
            .Message.Should().Contain("exited with exit code 134");

        result.OutputAsString.Should()
            .Contain("Unsupported base");
    }

    [Fact]
    public void methods_not_returning_error_emits_expected_compiler_error()
    {
        string source = """
            private static is_even(i: int): bool {
                if (i > 0) {
                    return i % 2;
                }
                else {
                    return new ArgumentError("i parameter must be a positive non-zero integer");
                }
            }
            """;

        var result = EvalWithValidationErrorCatch(source);

        result.Errors.Should()
            .ContainSingle()
            .Which
            .Message.Should().Contain("Error types can only be returned from functions with a 'T | error' union return type");
    }
}
