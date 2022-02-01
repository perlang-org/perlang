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
        public void getcwd_returns_non_null_string()
        {
            var result = Eval("Libc.getcwd()");

            result.Should()
                .BeOfType<string>().Which.Should()
                .NotBeNull();
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
