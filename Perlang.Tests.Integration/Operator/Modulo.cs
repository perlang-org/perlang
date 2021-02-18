using System.Linq;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Operator
{
    /// <summary>
    /// Tests for the % (modulo) operator.
    /// </summary>
    public class Modulo
    {
        //
        // "Positive" tests, testing for supported behavior
        //

        [Fact]
        public void modulo_operation_on_integers_returns_integer()
        {
            string source = @"
                5 % 3
            ";

            object result = Eval(source);

            Assert.Equal(2, result);
        }

        [Fact]
        public void modulo_operation_on_doubles_returns_double()
        {
            string source = @"
                12.34 % 0.3
            ";

            object result = Eval(source);

            // IEEE 754... :-)
            Assert.Equal(0.04000000000000031, result);
        }

        [Fact]
        public void modulo_operation_combined_with_others_without_grouping()
        {
            string source = @"
                2 * 5 / 10 * 4 % 2.1
            ";

            object result = Eval(source);

            Assert.Equal(1.9, result);
        }

        [Fact]
        public void modulo_operation_combined_with_others_with_grouping_first_operators()
        {
            string source = @"
                (2 * 5 / 10 * 4) % 2.1
            ";

            object result = Eval(source);

            Assert.Equal(1.9, result);
        }

        [Fact]
        public void modulo_operation_combined_with_others_with_grouping_last_operators()
        {
            string source = @"
                2 * 5 / 10 * (4 % 2.1)
            ";

            object result = Eval(source);

            Assert.Equal(1.9, result);
        }

        //
        // "Negative tests", ensuring that unsupported operations fail in the expected way.
        //

        [Fact]
        public void modulo_operation_on_non_number_with_number_throws_expected_error()
        {
            string source = @"
                ""1"" % 1;
            ";

            var result = EvalWithValidationErrorCatch(source);
            var exception = result.Errors.FirstOrDefault();

            Assert.Single(result.Errors);
            Assert.Equal("Invalid arguments to % operator specified", exception.Message);
        }

        [Fact]
        public void modulo_operation_on_number_with_non_number_throws_expected_error()
        {
            string source = @"
                1 % ""1"";
            ";

            var result = EvalWithValidationErrorCatch(source);
            var exception = result.Errors.FirstOrDefault();

            Assert.Single(result.Errors);
            Assert.Equal("Invalid arguments to % operator specified", exception.Message);
        }
    }
}
