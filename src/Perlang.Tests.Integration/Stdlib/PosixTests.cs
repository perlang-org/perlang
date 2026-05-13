using FluentAssertions;
using Perlang.Stdlib;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Stdlib;

/// <summary>
/// Integration test for the <see cref="PosixStdlibClass"/> class.
/// </summary>
public class PosixTests
{
#if _WINDOWS
    [Fact(Skip = "Only supported on POSIX platforms")]
#else
    [Fact]
#endif
    public void getegid_returns_positive_integer()
    {
        string output = EvalReturningOutputString("print Posix.getegid();");

        output.Should()
            .MatchRegex(@"^\d+$");
    }

#if _WINDOWS
    [Fact(Skip = "Only supported on POSIX platforms")]
#else
    [Fact]
#endif
    public void geteuid_returns_positive_integer()
    {
        string output = EvalReturningOutputString("print Posix.geteuid();");

        output.Should()
            .MatchRegex(@"^\d+$");
    }

#if _WINDOWS
    [Fact(Skip = "Only supported on POSIX platforms")]
#else
    [Fact]
#endif
    public void getgid_returns_positive_integer()
    {
        string output = EvalReturningOutputString("print Posix.getgid();");

        output.Should()
            .MatchRegex(@"^\d+$");
    }

#if _WINDOWS
    [Fact(Skip = "Only supported on POSIX platforms")]
#else
    [Fact]
#endif
    public void getppid_returns_positive_integer()
    {
        string output = EvalReturningOutputString("print Posix.getppid();");

        output.Should()
            .MatchRegex(@"^\d+$");
    }

#if _WINDOWS
    [Fact(Skip = "Only supported on POSIX platforms")]
#else
    [Fact]
#endif
    public void getuid_returns_positive_integer()
    {
        string output = EvalReturningOutputString("print Posix.getuid();");

        output.Should()
            .MatchRegex(@"^\d+$");
    }
}
