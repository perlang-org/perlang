using System.Collections.Immutable;
using FluentAssertions;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Stdlib
{
    /// <summary>
    /// Integration test for the <see cref="Perlang.Stdlib.Libc"/> class.
    /// </summary>
    public class LibcTests
    {
        [Fact]
        public void environ_returns_dictionary()
        {
            var result = Eval("Libc.environ()");

            result.Should()
                .BeOfType<ImmutableDictionary<Lang.String, string>>().Which.Should()
                .NotBeEmpty();
        }

        [Fact]
        public void environ_contains_path()
        {
            var result = Eval("Libc.environ()");

            result.Should()
                .BeOfType<ImmutableDictionary<Lang.String, string>>().Which.Should()

                // PATH is typically Path on Windows, hence the need for uppercasing it to make tests pass on Windows.
                .Contain(d => d.Key.ToUpper().ToString() == "PATH");
        }

#if _WINDOWS
        [Fact(Skip = "PATH is named Path on Windows")]
#else
        [Fact]
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
            var result = Eval("Libc.getcwd()");

            result.Should()
                .BeOfType<string>().Which.Should()
                .NotBeNull();
        }

        [Fact]
        public void getenv_path_returns_non_empty_string()
        {
            var result = Eval("Libc.getenv(\"PATH\")");

            result.Should()
                .BeOfType<string>().Which.Should()
                .NotBeEmpty();
        }

        [Fact]
        public void getpid_returns_positive_integer()
        {
            var result = Eval("Libc.getpid()");

            result.Should()
                .BeOfType<int>().Which.Should()
                .BeGreaterThanOrEqualTo(0);
        }
    }
}
