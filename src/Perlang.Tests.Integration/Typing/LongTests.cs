using System.Linq;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Typing
{
    /// <summary>
    /// Tests for the `long` data type.
    /// </summary>
    public class LongTests
    {
        [Fact]
        public void long_variable_can_be_printed()
        {
            string source = @"
                var l: long = 103;

                print(l);
            ";

            var output = EvalReturningOutputString(source);

            Assert.Equal("103", output);
        }

        [Fact]
        public void long_variable_can_be_reassigned()
        {
            string source = @"
                var l: long = 103;
                l = 8589934592;

                print(l);
            ";

            var result = EvalReturningOutputString(source);

            Assert.Equal("8589934592", result);
        }

        [Fact]
        public void long_variable_can_contain_numbers_larger_than_32_bits()
        {
            string source = @"
                var l: long = 8589934592;

                print(l);
            ";

            var output = EvalReturningOutputString(source);

            Assert.Equal("8589934592", output);
        }

        [Fact]
        public void long_variable_throws_expected_exception_on_constant_overflow()
        {
            string source = @"
                var l: long = 1231231230912839019312831232;

                print(l);
            ";

            var result = EvalWithValidationErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Matches("Cannot assign bigint to long variable", exception.Message);
        }

        [Fact]
        public void long_variable_has_expected_type_when_initialized_to_8bit_value()
        {
            // An 8-bit integer (sbyte) should be expanded to 64-bit when the assignment target is of the 'long' type.
            string source = @"
                var l: long = 103;

                print(l.get_type());
            ";

            var output = EvalReturningOutputString(source);

            Assert.Equal("perlang.Int64", output);
        }

        [Fact]
        public void long_variable_has_expected_type_when_assigned_8bit_value_from_another_variable()
        {
            // An 8-bit integer (sbyte) should be expanded to 64-bit when the assignment target is of the 'long' type.
            string source = @"
                var l: long = 103;
                var m = l;

                print(m.get_type());
            ";

            var output = EvalReturningOutputString(source);

            Assert.Equal("perlang.Int64", output);
        }

        [Fact]
        public void long_variable_has_expected_type_for_large_value()
        {
            string source = @"
                var l: long = 8589934592;

                print(l.get_type());
            ";

            var output = EvalReturningOutputString(source);

            Assert.Equal("perlang.Int64", output);
        }

        [Fact]
        public void long_variable_with_32bit_value_emits_expected_error_when_initializer_assigned_to_int_variable()
        {
            // Expansions are fine (103 to `long`), but assignments to a smaller-sized integer, potentially losing data,
            // is not supported with implicit conversions. Attempting to initialize an `int` variable from a `long` is
            // expected to fail.
            string source = @"
                var l: long = 103;
                var i: int = l;
            ";

            var result = EvalWithValidationErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Matches("Cannot assign long to int variable", exception.Message);
        }
    }
}
