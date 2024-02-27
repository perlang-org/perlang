using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Operator.Binary
{
    /// <summary>
    /// Tests for the ** (exponential) operator. The operator works pretty much like in Ruby.
    ///
    /// The type of the returned value is varies depending on the input types.
    /// </summary>
    public class ExponentialTests
    {
        [Theory]
        [MemberData(nameof(BinaryOperatorData.Exponential_result), MemberType = typeof(BinaryOperatorData))]
        public void performs_exponential_calculation(string i, string j, string expectedResult)
        {
            string source = $@"
                    print {i} ** {j};
                ";

            string result = EvalReturningOutputString(source);

            result.Should()
                .Be(expectedResult);
        }

        [SkippableTheory]
        [MemberData(nameof(BinaryOperatorData.Exponential_type), MemberType = typeof(BinaryOperatorData))]
        public void with_supported_types_returns_expected_type(string i, string j, string expectedResult)
        {
            Skip.If(PerlangMode.ExperimentalCompilation, "Not supported in compiled mode");

            string source = $@"
                    print ({i} ** {j}).get_type();
                ";

            string result = EvalReturningOutputString(source);

            result.Should()
                .Be(expectedResult);
        }

        [Theory]
        [MemberData(nameof(BinaryOperatorData.Exponential_unsupported_types), MemberType = typeof(BinaryOperatorData))]
        public void with_unsupported_types_emits_expected_error(string i, string j, string expectedResult)
        {
            string source = $@"
                    print {i} ** {j};
                ";

            var result = EvalWithValidationErrorCatch(source);

            result.Errors.Should()
                .ContainSingle().Which
                .Message.Should().Match(expectedResult);
        }

        [Fact]
        public void exponential_positive_and_negative_integer_literals_throws_expected_error()
        {
            // We thought about supporting this, returning a `double` instead of a `bigint` in this case, but returning
            // _different types depending on exponent sign_ (positive or negative) seems far too dynamic for us. Perlang
            // is a strongly typed language where we want our users to be able to expect few "surprises" in this regard.
            // Hence, perhaps even a runtime exception is better in this case. If we want to make it even better at some
            // point, we could make this a compile-time check for negative integer literals at least.

            string source = @"
                10 ** -3
            ";

            var result = EvalWithRuntimeErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Equal("The number must be greater than or equal to zero. (Parameter 'exponent')", exception.Message);
        }

        [SkippableFact]
        public void exponential_negative_and_positive_integer_literals()
        {
            string source = @"
                -10 ** 3
            ";

            object result = Eval(source);

            Assert.Equal(new BigInteger(-1000), result);
        }

        [SkippableFact]
        public void exponential_negative_and_positive_integer_literals_again()
        {
            // This tests an important edge case where we differ from MRI Ruby. Try it in 'irb' and you'll see what I
            // mean: MRI returns -59049 in this case.
            //
            // 2.6.3 :005 > -3 ** 10
            // => -59049
            //
            // I reckon this is because (surprise, surprise!) the ** operator seems to have _lower_ precedence in Ruby
            // than - (which seems to be an operator and not just a modifier to the integer). These examples illustrate
            // this further:
            //
            // 2.6.3 :007 > -(3 ** 10)
            // => -59049
            // 2.6.3 :008 > (-3) ** 10
            // => 59049
            //
            // I believe the semantics we have right now makes most sense, but feel free to challenge this if you think
            // this is a fallacy on my behalf.

            string source = @"
                -3 ** 10
            ";

            object result = Eval(source);

            Assert.Equal(new BigInteger(59049), result);
        }

        [SkippableTheory]
        [ClassData(typeof(TestCultures))]
        public async Task exponential_operator_works_on_different_cultures(CultureInfo cultureInfo)
        {
            CultureInfo.CurrentCulture = cultureInfo;

            string source = @"
                10 ** 3.5
            ";

            object result = Eval(source);

            Assert.Equal(3162.2776601683795, result);
        }

        [SkippableTheory]
        [ClassData(typeof(TestCultures))]
        public async Task exponential_integer_and_float_literals_infers_to_expected_type(CultureInfo cultureInfo)
        {
            CultureInfo.CurrentCulture = cultureInfo;

            string source = @"
                var v = 10 ** 3.5;
                print v.get_type();
            ";

            string result = EvalReturningOutputString(source);

            Assert.Equal("System.Double", result);
        }

        [SkippableTheory]
        [ClassData(typeof(TestCultures))]
        public async Task exponential_integer_and_negative_float_literals(CultureInfo cultureInfo)
        {
            CultureInfo.CurrentCulture = cultureInfo;

            string source = @"
                10 ** -3.5
            ";

            object result = Eval(source);

            Assert.Equal(0.00031622776601683794, result);
        }

        [SkippableFact]
        public void exponential_integer_literals_and_multiplication()
        {
            string source = @"
                2 ** 10 * 2
            ";

            object result = Eval(source);

            Assert.Equal(new BigInteger(2048), result);
        }

        [SkippableFact]
        public void exponential_bigint_and_int()
        {
            string source = @"
                1267650600228229401496703205376 ** 3
            ";

            object result = Eval(source);

            Assert.Equal(BigInteger.Parse("2037035976334486086268445688409378161051468393665936250636140449354381299763336706183397376"), result);
        }

        [SkippableFact]
        public void exponential_multiple_times()
        {
            string source = @"
                (2 ** 100) ** 3
            ";

            object result = Eval(source);

            Assert.Equal(BigInteger.Parse("2037035976334486086268445688409378161051468393665936250636140449354381299763336706183397376"), result);
        }

        [Fact]
        public void exponential_integer_literal_and_function_return_value()
        {
            string source = @"
                fun foo(): int { return 16; }

                print 2 ** foo();
            ";

            string result = EvalReturningOutputString(source);

            Assert.Equal("65536", result);
        }

        [Theory]
        [InlineData("2", "12", "4096", "sv-SE")]
        [InlineData("2", "12", "4096", "en-US")]
        [InlineData("10", "3.5", "3162.27766016838", "sv-SE")]
        [InlineData("10", "3.5", "3162.27766016838", "en-US")]
        public async Task exponential_integer_literals_as_variable_initializer(string left, string right, string expectedResult, string cultureName)
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultureName);

            string source = $@"
                var x = {left} ** {right};
                print x;
            ";

            string result = EvalReturningOutputString(source);

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void exponential_function_return_value_and_int_literal()
        {
            string source = @"
                fun foo(): int { return 4; }

                print foo() ** 8;
            ";

            string result = EvalReturningOutputString(source);

            Assert.Equal("65536", result);
        }

        [Fact]
        public void exponential_int_variable_and_int_literal()
        {
            string source = @"
                var left = 2;

                print left ** 8;
            ";

            string result = EvalReturningOutputString(source);

            Assert.Equal("256", result);
        }

        [Fact]
        public void exponential_function_return_values()
        {
            string source = @"
                fun base(): int { return 2; }
                fun exponent(): int { return 8; }

                print base() ** exponent();
            ";

            string result = EvalReturningOutputString(source);

            Assert.Equal("256", result);
        }

        [Fact]
        public void exponential_bigint_and_negative_int_throws_expected_runtime_error()
        {
            string source = @"
                1267650600228229401496703205376 ** -3
            ";

            var result = EvalWithRuntimeErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Equal("The number must be greater than or equal to zero. (Parameter 'exponent')", exception.Message);
        }

        [Fact]
        public void exponential_string_and_integer_throws_expected_error()
        {
            string source = @"
                ""string"" ** 10
            ";

            var result = EvalWithValidationErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Equal("Unsupported ** operand types: 'AsciiString' and 'int'", exception.Message);
        }

        [Fact]
        public void exponential_integer_and_string_throws_expected_error()
        {
            string source = @"
                10 ** ""string""
            ";

            var result = EvalWithValidationErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Equal("Unsupported ** operand types: 'int' and 'AsciiString'", exception.Message);
        }

        [Fact]
        public void exponential_function_reference_and_literal_throws_expected_error()
        {
            string source = @"
                fun base(): int { return 2; }

                print base ** 8;
            ";

            // TODO: Should not be runtime error but rather a compile-time error from the TypeResolver class. See the
            // TODO: comment in TypeResolver.VisitBinaryExpr() for more details about why this is not currently doable.
            var result = EvalWithRuntimeErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Matches("Operands must be numbers, not function and int", exception.Message);
        }
    }
}
