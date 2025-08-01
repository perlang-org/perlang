using System.Linq;
using FluentAssertions;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Operator.Binary;

public class ShiftRightTests
{
    [Theory]
    [MemberData(nameof(BinaryOperatorData.ShiftRight_result), MemberType = typeof(BinaryOperatorData))]
    public void performs_right_shifting(string i, string j, string expectedResult)
    {
        string source = $@"
                    print {i} >> {j};
                ";

        string result = EvalReturningOutputString(source);

        result.Should()
            .Be(expectedResult);
    }

    [SkippableTheory]
    [MemberData(nameof(BinaryOperatorData.ShiftRight_type), MemberType = typeof(BinaryOperatorData))]
    public void with_supported_types_returns_expected_type(string i, string j, string expectedResult)
    {
        Skip.If(PerlangMode.ExperimentalCompilation, "get_type() is not yet supported in compiled mode");

        string source = $@"
                    print ({i} >> {j}).get_type();
                ";

        string result = EvalReturningOutputString(source);

        result.Should()
            .Be(expectedResult);
    }

    [Theory]
    [MemberData(nameof(BinaryOperatorData.ShiftRight_unsupported_types), MemberType = typeof(BinaryOperatorData))]
    public void with_unsupported_types_emits_expected_error(string i, string j, string expectedResult)
    {
        string source = $@"
                    print {i} >> {j};
                ";

        var result = EvalWithValidationErrorCatch(source);

        result.Errors.Should()
            .ContainSingle().Which
            .Message.Should().Match(expectedResult);
    }

    [Fact]
    public void takes_precedence_over_multiplication()
    {
        string source = @"
                    print 65536 >> 6 * 3;
                ";

        string result = EvalReturningOutputString(source);
        Assert.Equal("3072", result);
    }

    [Fact]
    public void takes_precedence_over_division()
    {
        string source = @"
                    print 65536 >> 6 / 3;
                ";

        // Since the operands are integers, the division is expected to be truncated to its integer portion.
        // (1024 / 3 = 341.333...)
        string result = EvalReturningOutputString(source);
        Assert.Equal("341", result);
    }

    [Fact]
    public void takes_precedence_over_addition()
    {
        string source = @"
                    print 65536 >> 6 + 3;
                ";

        string result = EvalReturningOutputString(source);
        Assert.Equal("1027", result);
    }

    [Fact]
    public void takes_precedence_over_subtraction()
    {
        string source = @"
                    print 65536 >> 6 - 3;
                ";

        string result = EvalReturningOutputString(source);
        Assert.Equal("1021", result);
    }

    [Fact]
    public void takes_precedence_over_modulo_operator()
    {
        string source = @"
                    print 65536 >> 6 % 1000;
                ";

        string result = EvalReturningOutputString(source);
        Assert.Equal("24", result);
    }

    [SkippableFact]
    public void takes_precedence_over_power_operator()
    {
        string source = @"
                    print 1024 >> 8 ** 10;
                ";

        string result = EvalReturningOutputString(source);
        Assert.Equal("1048576", result);
    }

    [Fact]
    public void with_integer_and_string_throws_expected_error()
    {
        string source = @"
                    print 1 >> ""foo"";
                ";

        var result = EvalWithValidationErrorCatch(source);
        var exception = result.Errors.FirstOrDefault();

        Assert.Single(result.Errors);
        Assert.Matches("Unsupported >> operand types: 'int' and 'ASCIIString'", exception.Message);
    }
}
