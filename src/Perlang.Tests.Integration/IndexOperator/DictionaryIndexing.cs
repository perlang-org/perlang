using FluentAssertions;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.IndexOperator;

public class DictionaryIndexing
{
#if _WINDOWS
    private const string PathKey = "Path";
#else // POSIX
    private const string PathKey = "PATH";
#endif

    [Fact]
    public void dictionary_with_string_key_can_be_indexed_by_string()
    {
        string source = @$"
            var env = Libc.environ();
            print env[""{PathKey}""];
        ";

        var output = EvalReturningOutputString(source);

#if _WINDOWS
        // Not _required_ per se, but will work as long as the CI runner/developer machine has the Windows installation
        // on the C drive. In reality, anything else is unlikely.
        output.Should().Contain("C:\\");
#else // POSIX
        // Not technically a *requirement* on these platforms, but they usually contain /bin, /usr/bin or similar.
        output.Should().Contain("/bin");
#endif
    }

    [Fact]
    public void dictionary_with_string_key_can_be_indexed_multiple_times()
    {
        string source = @$"
            var env = Libc.environ();
            print env[""{PathKey}""][0];
        ";

        var output = EvalReturningOutputString(source);

#if _WINDOWS
        // Again, not _required_ and this is even more risky. As long as it works in CI, I'll be happy for now.
        output.Should().Be("C");
#else // POSIX
        // Again, technically not required but this should work on at least Debian/Ubuntu, macOS and FreeBSD
        output.Should().Be("/");
#endif
    }

    [Fact]
    public void dictionary_with_string_key_throws_expected_error_when_indexed_by_not_present_key()
    {
        string source = @"
            var env = Libc.environ();
            print env[""NOT_PRESENT_DICTIONARY_KEY""];
        ";

        var result = EvalWithRuntimeErrorCatch(source);

        result.Errors.Should()
            .ContainSingle()
            .Which
            .Message.Should().Contain("NOT_PRESENT_DICTIONARY_KEY")
            .And.Contain("not present");
    }

    [Fact]
    public void dictionary_with_string_key_throws_expected_error_when_indexed_by_null()
    {
        string source = @"
            var env = Libc.environ();
            print env[null];
        ";

        var result = EvalWithValidationErrorCatch(source);

        result.Errors.Should()
            .ContainSingle().Which
            .Message.Should().Contain("cannot be indexed by 'null'");
    }

    [Fact]
    public void dictionary_with_string_key_throws_expected_error_when_indexed_by_integer()
    {
        string source = @"
            var env = Libc.environ();
            print env[123];
        ";

        var result = EvalWithValidationErrorCatch(source);

        result.Errors.Should()
            .ContainSingle().Which
            .Message.Should().Contain("cannot be indexed by 'int'");
    }
}
