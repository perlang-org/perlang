using System.Collections.Immutable;
using FluentAssertions;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Stdlib;

/// <summary>
/// Integration test for the <see cref="Perlang.Stdlib.Libc"/> class.
/// </summary>
public class LibcTests
{
    [SkippableFact]
    public void environ_returns_dictionary()
    {
        Skip.If(PerlangMode.ExperimentalCompilation, "Not yet supported in compiled mode");

        var result = Eval("Libc.environ()");

        result.Should()
            .BeOfType<ImmutableDictionary<Lang.String, string>>().Which.Should()
            .NotBeEmpty();
    }

    [SkippableFact]
    public void environ_contains_path()
    {
        Skip.If(PerlangMode.ExperimentalCompilation, "Not yet supported in compiled mode");

        var result = Eval("Libc.environ()");

        result.Should()
            .BeOfType<ImmutableDictionary<Lang.String, string>>().Which.Should()

            // PATH is typically Path on Windows, hence the need for uppercasing it to make tests pass on Windows.
            .Contain(d => d.Key.to_upper().ToString() == "PATH");
    }

#if _WINDOWS
        [Fact(Skip = "PATH is named Path on Windows")]
#else
    [SkippableFact]
#endif
    public void environ_item_supports_get_Item()
    {
        // TODO: This can be improved when #270 is implemented.
        var result = Eval("Libc.environ().get_Item(\"PATH\")");

        result.Should()
            .BeOfType<string>().Which.Should()
            .NotBeEmpty();
    }

    [Fact]
    public void getcwd_returns_non_null_string()
    {
        string output = EvalReturningOutputString("print Libc.getcwd();");

        output.Should()
            .NotBeEmpty();

        // This will be true on all Unix-based systems, but will have to be adjusted for Windows
        output.Should()
            .Contain("/");
    }

    [Fact]
    public void getenv_path_returns_non_empty_string()
    {
        // TODO: Will this work on Windows, despite the path variable being named 'Path'?
        string output = EvalReturningOutputString("print Libc.getenv(\"PATH\");");

        output.Should()
            .NotBeEmpty();
    }

    [Fact]
    public void getpid_returns_positive_integer()
    {
        string output = EvalReturningOutputString("print Libc.getpid();");

        output.Should()
            .MatchRegex(@"^\d+$");
    }
}
