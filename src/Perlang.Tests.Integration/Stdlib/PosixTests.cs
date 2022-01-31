using FluentAssertions;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Stdlib
{
    public class PosixTests
    {
#if _WINDOWS
        [Fact(Skip = "Only supported on POSIX platforms")]
#else
        [Fact]
#endif
        public void getegid_returns_positive_integer()
        {
            var result = Eval("Posix.getegid()");

            result.Should()
                .BeOfType<int>().Which.Should()
                .BeGreaterThanOrEqualTo(0);
        }

#if _WINDOWS
        [Fact(Skip = "Only supported on POSIX platforms")]
#else
        [Fact]
#endif
        public void geteuid_returns_positive_integer()
        {
                var result = Eval("Posix.geteuid()");

                result.Should()
                        .BeOfType<int>().Which.Should()
                        .BeGreaterThanOrEqualTo(0);
        }

#if _WINDOWS
        [Fact(Skip = "Only supported on POSIX platforms")]
#else
        [Fact]
#endif
        public void getgid_returns_positive_integer()
        {
            var result = Eval("Posix.getgid()");

            result.Should()
                .BeOfType<int>().Which.Should()
                .BeGreaterThanOrEqualTo(0);
        }

#if _WINDOWS
        [Fact(Skip = "Only supported on POSIX platforms")]
#else
        [Fact]
#endif
        public void getpid_returns_positive_integer()
        {
            var result = Eval("Posix.getpid()");

            result.Should()
                .BeOfType<int>().Which.Should()
                .BeGreaterThanOrEqualTo(0);
        }

#if _WINDOWS
        [Fact(Skip = "Only supported on POSIX platforms")]
#else
        [Fact]
#endif
        public void getppid_returns_positive_integer()
        {
            var result = Eval("Posix.getppid()");

            result.Should()
                .BeOfType<int>().Which.Should()
                .BeGreaterThanOrEqualTo(0);
        }

#if _WINDOWS
        [Fact(Skip = "Only supported on POSIX platforms")]
#else
        [Fact]
#endif
        public void getuid_returns_positive_integer()
        {
            var result = Eval("Posix.getuid()");

            result.Should()
                .BeOfType<int>().Which.Should()
                .BeGreaterThanOrEqualTo(0);
        }
    }
}
