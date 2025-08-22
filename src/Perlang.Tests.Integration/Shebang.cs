using System.Linq;
using FluentAssertions;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration;

public class Shebang
{
    [Fact]
    public void initial_shebang_is_silently_ignored()
    {
        string source = @"
                #!/usr/bin/env perlang
                print 24219;
            ".Trim();

        string output = EvalReturningOutput(source).SingleOrDefault();

        Assert.Equal("24219", output);
    }

    [Fact]
    public void shebang_in_the_middle_of_program_throws_expected_error()
    {
        string source = @"
                var a = 10;
                #!/usr/bin/env perlang
                var b = 10;
            ".Trim();

        var result = EvalWithScanErrorCatch(source);

        // This doesn't get treated as a shebang, but as preprocessor directive, which is why the error message
        // doesn't contain the hash sign.
        result.Errors.Should()
            .ContainSingle().Which
            .Message.Should().Match("Unknown preprocessor directive !/usr/bin/env perlang.");
    }
}