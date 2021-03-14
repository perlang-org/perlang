using System.Linq;
using Perlang.Parser;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration
{
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

            Assert.Throws<ScanError>(() => EvalReturningOutput(source).SingleOrDefault());
        }
    }
}
