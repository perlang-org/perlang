using System.Linq;
using Xunit;
using static Perlang.Tests.EvalHelper;

namespace Perlang.Tests.Operator
{
    public class PostfixDecrement
    {
        [Fact]
        public void decrementing_defined_variable()
        {
            string source = @"
                var i = 0;
                i--;
                print i;
            ";

            var output = EvalReturningOutput(source);

            Assert.Equal(new[] {"-1"}, output);
        }

        [Fact]
        public void decrementing_undefined_variable_throws_expected_exception()
        {
            string source = @"
                x--;
            ";

            var result = EvalWithTypeValidationErrorCatch(source);
            var exception = result.TypeValidationErrors.First();

            Assert.Single(result.TypeValidationErrors);
            Assert.Matches("Undefined variable 'x'", exception.Message);
        }

        [Fact]
        public void decrementing_nil_throws_expected_exception()
        {
            string source = @"
                var i = nil;
                i--;
            ";

            var result = EvalWithRuntimeCatch(source);
            var exception = result.RuntimeErrors.First();

            Assert.Single(result.RuntimeErrors);
            Assert.Matches("can only be used to decrement numbers, not nil", exception.Message);
        }

        [Fact]
        public void decrementing_string_throws_expected_exception()
        {
            string source = @"
                var i = ""foo"";
                i--;
            ";

            var result = EvalWithRuntimeCatch(source);
            var exception = result.RuntimeErrors.First();

            Assert.Single(result.RuntimeErrors);
            Assert.Matches("can only be used to decrement numbers, not String", exception.Message);
        }

        [Fact]
        public void decrement_can_be_used_in_for_loops()
        {
            string source = @"
                for (var c = 3; c > 0; c--)
                    print c;
            ";

            var output = EvalReturningOutput(source);

            Assert.Equal(new[]
            {
                "3",
                "2",
                "1"
            }, output);
        }

        [Fact]
        public void decrement_can_be_used_in_assignment()
        {
            string source = @"
                var i = 100;
                var j = i--;
                print j;
            ";

            var output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("99", output);
        }
    }
}
