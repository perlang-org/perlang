using FluentAssertions;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Function;

// TODO: Incorporate the tests from Arguments, Recursion and Return into this class, or rename them to FunctionArguments etc.
public class FunctionTests
{
    [Fact]
    public void using_implement_modifier_without_public_should_emit_expected_error()
    {
        string source = """
            implement fun foo(): void {
            }
            """;

        var result = EvalWithParseErrorCatch(source);

        // This error message is a bit illogical; methods don't really have a visibility to begin with right now.
        // Accepting this deficiency for the time being. We might do away with the 'fun' keyword soon anyway, since our
        // current approach of defining free functions and methods belonging to classes is a bit inconsistent.
        result.Errors.Should()
            .ContainSingle()
            .Which
            .Message.Should().Contain("'implement' keyword must come after visibility");
    }

    [Fact]
    public void using_implement_modifier_for_functions_should_emit_expected_error()
    {
        string source = """
            public implement fun foo(): void {
            }
            """;

        var result = EvalWithParseErrorCatch(source);

        result.Errors.Should()
            .ContainSingle()
            .Which
            .Message.Should().Contain("implement' modifier is not valid for functions");
    }
}
