using System.Linq;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Typing
{
    /// <summary>
    /// Tests for the `long` data type.
    /// </summary>
    public class BigintTests
    {
        [Fact]
        public void bigint_variable_can_be_printed()
        {
            string source = @"
                var v: bigint = 103;

                print(v);
            ";

            var output = EvalReturningOutputString(source);

            Assert.Equal("103", output);
        }

        [Fact]
        public void bigint_variable_can_be_reassigned()
        {
            string source = @"
                var v: bigint = 103;
                v = 8589934592;

                print(v);
            ";

            var result = EvalReturningOutputString(source);

            Assert.Equal("8589934592", result);
        }

        [Fact]
        public void bigint_variable_can_contain_numbers_larger_than_32_bits()
        {
            string source = @"
                var v: bigint = 8589934592;

                print(v);
            ";

            var output = EvalReturningOutputString(source);

            Assert.Equal("8589934592", output);
        }

        [Fact]
        public void bigint_variable_can_contain_numbers_larger_than_64_bits()
        {
            string source = @"
                var v: bigint = 1231231230912839019312831232;

                print(v);
            ";

            var output = EvalReturningOutputString(source);

            Assert.Equal("1231231230912839019312831232", output);
        }

        [SkippableFact]
        public void bigint_variable_has_expected_type_when_initialized_to_8bit_value()
        {
            // An 8-bit integer (sbyte) should be expanded to bigint when the assignment target is of the 'bigint' type.
            string source = @"
                var v: bigint = 103;

                print(v.get_type());
            ";

            var output = EvalReturningOutputString(source);

            Assert.Equal("System.Numerics.BigInteger", output);
        }

        [SkippableFact]
        public void bigint_variable_has_expected_type_when_assigned_8bit_value_from_another_variable()
        {
            // An 8-bit integer (sbyte) should be expanded to bigint when the assignment target is of the 'bigint' type.
            string source = @"
                var v: bigint = 103;
                var x = v;

                print(x.get_type());
            ";

            var output = EvalReturningOutputString(source);

            Assert.Equal("System.Numerics.BigInteger", output);
        }

        [SkippableFact]
        public void bigint_variable_has_expected_type_for_large_value()
        {
            string source = @"
                var v: bigint = 8589934592;

                print(v.get_type());
            ";

            var output = EvalReturningOutputString(source);

            Assert.Equal("System.Numerics.BigInteger", output);
        }

        [Fact]
        public void bigint_variable_with_32bit_value_emits_expected_error_when_initializer_assigned_to_int_variable()
        {
            // Expansions are fine (103 to bigint), but the other way around is not supported with implicit conversions.
            // Attempting to initialize an int variable with this value is expected to fail.
            string source = @"
                var v: bigint = 103;
                var i: int = v;
            ";

            var result = EvalWithValidationErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Matches("Cannot assign bigint to int variable", exception.Message);
        }
    }
}
