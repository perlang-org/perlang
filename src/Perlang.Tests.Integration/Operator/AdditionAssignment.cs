using System.Linq;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Operator
{
    public class AdditionAssignment
    {
        // "Positive" tests, testing for supported behavior

        [Fact]
        public void addition_assignment_defined_variable()
        {
            string source = @"
                var i = 0;
                i += 1;
                print i;
            ";

            var output = EvalReturningOutputString(source);

            Assert.Equal("1", output);
        }

        [Fact]
        public void addition_assignment_can_be_used_in_for_loops()
        {
            string source = @"
                for (var c = 0; c < 3; c += 1)
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

        [Fact]
        public void addition_assignment_can_be_used_in_assignment_with_inference()
        {
            string source = @"
                var i = 100;
                var j = i += 2;
                print j;
            ";

            var output = EvalReturningOutputString(source);

            Assert.Equal("102", output);
        }

        [Fact]
        public void addition_assignment_can_be_used_in_assignment_with_explicit_types()
        {
            string source = @"
                var i: int = 100;
                var j: int = i += 2;
                print j;
            ";

            var output = EvalReturningOutputString(source);

            Assert.Equal("102", output);
        }

        // "Negative tests", ensuring that unsupported operations fail in the expected way.

        [Fact]
        public void addition_assignment_to_undefined_variable_throws_expected_exception()
        {
            string source = @"
                x += 3;
            ";

            var result = EvalWithValidationErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Matches("Undefined identifier 'x'", exception.Message);
        }

        [Fact]
        public void addition_assignment_to_null_throws_expected_exception()
        {
            string source = @"
                var i = null;
                i += 4;
            ";

            var result = EvalWithValidationErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Equal("Inferred: Perlang.NullObject is not comparable and can therefore not be used with the $PLUS_EQUAL += operator", exception.Message);
        }

        [Fact]
        public void addition_assignment_to_string_throws_expected_exception()
        {
            string source = @"
                var i = ""foo"";
                i += 5;
            ";

            var result = EvalWithValidationErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Equal("Invalid arguments to += operator specified: System.String and System.Int32", exception.Message);
        }
    }
}
