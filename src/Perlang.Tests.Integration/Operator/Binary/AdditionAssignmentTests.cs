using FluentAssertions;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Operator.Binary
{
    public class AdditionAssignmentTests
    {
        [Theory]
        [MemberData(nameof(BinaryOperatorData.AdditionAssignment_result), MemberType = typeof(BinaryOperatorData))]
        public void performs_addition_assignment(string i, string j, string expectedResult)
        {
            string source = $@"
                var i = {i};
                i += {j};
                print i;
            ";

            string output = EvalReturningOutputString(source);

            output.Should()
                .Be(expectedResult);
        }

        [Theory]
        [MemberData(nameof(BinaryOperatorData.AdditionAssignment_type), MemberType = typeof(BinaryOperatorData))]
        public void with_supported_types_returns_expected_type(string i, string j, string expectedType)
        {
            string source = $@"
                var i = {i};
                print (i += {j}).get_type();
            ";

            string output = EvalReturningOutputString(source);

            output.Should()
                .Be(expectedType);
        }

        [Theory]
        [MemberData(nameof(BinaryOperatorData.AdditionAssignment_unsupported_types_runtime), MemberType = typeof(BinaryOperatorData))]
        public void with_unsupported_types_emits_expected_runtime_error(string i, string j, string expectedResult)
        {
            string source = $@"
                var i = {i};
                print (i += {j});
            ";

            // TODO: Should be validation errors, not runtime errors. The shift-left operator does it right, use the same
            // TODO: approach here.
            var result = EvalWithRuntimeErrorCatch(source);

            result.Errors.Should()
                .ContainSingle().Which
                .Message.Should().Match(expectedResult);
        }

        [Theory]
        [MemberData(nameof(BinaryOperatorData.AdditionAssignment_unsupported_types_validation), MemberType = typeof(BinaryOperatorData))]
        public void with_unsupported_types_emits_expected_validation_error(string i, string j, string expectedResult)
        {
            string source = $@"
                var i = {i};
                print (i += {j});
            ";

            var result = EvalWithValidationErrorCatch(source);

            result.Errors.Should()
                .ContainSingle().Which
                .Message.Should().Match(expectedResult);
        }

        [Fact]
        public void addition_assignment_can_be_used_in_for_loops()
        {
            string source = @"
                for (var c = 0; c < 3; c += 1)
                    print c;
            ";

            var output = EvalReturningOutput(source);

            output.Should()
                .Equal("0", "1", "2");
        }

        [Fact]
        public void addition_assignment_can_be_used_in_assignment_with_inference()
        {
            string source = @"
                var i = 100;
                var j = i += 2;
                print j;
            ";

            string output = EvalReturningOutputString(source);

            output.Should()
                .Be("102");
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

            output.Should()
                .Be("102");
        }

        [Fact]
        public void addition_assignment_to_undefined_variable_throws_expected_exception()
        {
            string source = @"
                x += 3;
            ";

            var result = EvalWithValidationErrorCatch(source);

            result.Errors.Should()
                .ContainSingle().Which
                .Message.Should().Match("Undefined identifier 'x'");
        }

        [Fact]
        public void addition_assignment_to_null_throws_expected_exception()
        {
            string source = @"
                var i = null;
                i += 4;
            ";

            var result = EvalWithValidationErrorCatch(source);

            result.Errors.Should()
                .ContainSingle().Which
                .Message.Should().Match("Inferred: Perlang.NullObject is not comparable and can therefore not be used with the $PLUS_EQUAL += operator");
        }

        [Fact]
        public void addition_assignment_to_string_throws_expected_exception()
        {
            string source = @"
                var i = ""foo"";
                i += 5;
            ";

            var result = EvalWithValidationErrorCatch(source);

            result.Errors.Should()
                .ContainSingle().Which
                .Message.Should().Match("Unsupported += operands specified: string and int");
        }
    }
}