using System.Linq;
using FluentAssertions;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Operator.Binary;

public class SubtractionAssignmentTests
{
    [Theory]
    [MemberData(nameof(BinaryOperatorData.SubtractionAssignment_result), MemberType = typeof(BinaryOperatorData))]
    public void performs_subtraction_assignment(string i, string j, string expectedResult)
    {
        string source = $@"
            var i = {i};
            i -= {j};
            print i;
        ";

        var output = EvalReturningOutputString(source);

        output.Should()
            .Be(expectedResult);
    }

    [Theory]
    [MemberData(nameof(BinaryOperatorData.SubtractionAssignment_type), MemberType = typeof(BinaryOperatorData))]
    void with_supported_types_returns_expected_type(string i, string j, string expectedType)
    {
        string source = $@"
            var i = {i};

            print (i -= {j}).get_type();
        ";

        string result = EvalReturningOutputString(source);

        result.Should()
            .Be(expectedType);
    }

    [Theory]
    [MemberData(nameof(BinaryOperatorData.SubtractionAssignment_unsupported_types), MemberType = typeof(BinaryOperatorData))]
    public void with_unsupported_types_emits_expected_error(string i, string j, string expectedError)
    {
        string source = $@"
            var i = {i};
            print i -= {j};
        ";

        var result = EvalWithValidationErrorCatch(source);

        result.Errors.Should()
            .ContainSingle().Which
            .Message.Should().Match(expectedError);
    }

    [Fact]
    public void subtraction_assignment_can_be_used_in_for_loops()
    {
        string source = @"
            for (var c = 3; c > 0; c -= 1)
                print c;
        ";

        var output = EvalReturningOutput(source);

        output.Should()
            .Equal("3", "2", "1");
    }

    [Fact]
    public void subtraction_assignment_can_be_used_in_assignment_with_inference()
    {
        string source = @"
            var i = 100;
            var j = i -= 2;
            print j;
        ";

        var output = EvalReturningOutputString(source);

        output.Should()
            .Be("98");
    }

    [Fact]
    public void subtraction_assignment_can_be_used_in_assignment_with_explicit_types()
    {
        string source = @"
            var i: int = 100;
            var j: int = i -= 2;
            print j;
        ";

        var output = EvalReturningOutputString(source);

        output.Should()
            .Be("98");
    }

    [Fact]
    public void subtraction_assignment_to_undefined_variable_throws_expected_exception()
    {
        string source = @"
            x +- 3;
        ";

        var result = EvalWithValidationErrorCatch(source);
        var exception = result.Errors.First();

        Assert.Single(result.Errors);
        Assert.Matches("Undefined identifier 'x'", exception.Message);
    }

    [Fact]
    public void subtraction_assignment_to_null_throws_expected_exception()
    {
        string source = @"
            var i = null;
            i -= 4;
        ";

        var result = EvalWithValidationErrorCatch(source);
        var exception = result.Errors.First();

        Assert.Single(result.Errors);
        Assert.Equal("Inferred: Perlang.NullObject is not comparable and can therefore not be used with the $MINUS_EQUAL -= operator", exception.Message);
    }

    [Fact]
    public void subtraction_assignment_to_string_throws_expected_exception()
    {
        string source = @"
            var i = ""foo"";
            i -= 5;
        ";

        var result = EvalWithValidationErrorCatch(source);
        var exception = result.Errors.First();

        Assert.Single(result.Errors);
        Assert.Equal("Cannot assign int to string variable", exception.Message);
    }
}
