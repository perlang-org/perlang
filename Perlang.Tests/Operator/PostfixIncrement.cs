using System.Linq;
using Xunit;
using static Perlang.Tests.EvalHelper;

namespace Perlang.Tests.Operator
{
    public class PostfixIncrement
    {
        [Fact]
        public void incrementing_defined_variable()
        {
            string source = @"
                var i = 0;
                i++;
                print i;
            ";

            var output = EvalReturningOutput(source);

            Assert.Equal(new[] { "1" }, output);
        }

        [Fact]
        public void incrementing_undefined_variable_throws_expected_exception()
        {
            string source = @"
                x++;
            ";

            var result = EvalWithRuntimeCatch(source);
            var exception = result.RuntimeErrors.First();

            Assert.Single(result.RuntimeErrors);
            Assert.Matches("Undefined variable 'x'", exception.Message);
        }

        [Fact]
        public void incrementing_nil_throws_expected_exception()
        {
            string source = @"
                var i = nil;
                i++;
            ";

            var result = EvalWithRuntimeCatch(source);
            var exception = result.RuntimeErrors.First();

            Assert.Single(result.RuntimeErrors);
            Assert.Matches("can only be used to increment numbers, not nil", exception.Message);
        }

        [Fact]
        public void incrementing_string_throws_expected_exception()
        {
            string source = @"
                var i = ""foo"";
                i++;
            ";

            var result = EvalWithRuntimeCatch(source);
            var exception = result.RuntimeErrors.First();

            Assert.Single(result.RuntimeErrors);
            Assert.Matches("can only be used to increment numbers, not String", exception.Message);
        }

        [Fact]
        public void increment_can_be_used_in_for_loops()
        {
            string source = @"
                for (var c = 0; c < 3; c++)
                    print c;
            ";

            var output = EvalReturningOutput(source);

            Assert.Equal(new[]
            {
                "0",
                "1",
                "2"
            }, output);
        }
    }
}
