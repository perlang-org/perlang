using FluentAssertions;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.IndexOperator;

public class InvalidIndexing
{
    [Fact]
    public void null_throws_expected_error_when_indexed()
    {
        string source = @"
            print null[123];
        ";

        var result = EvalWithValidationErrorCatch(source);

        result.Errors.Should()
            .ContainSingle()
            .Which
            .Message.Should().Contain("'null' reference cannot be indexed");
    }
}
