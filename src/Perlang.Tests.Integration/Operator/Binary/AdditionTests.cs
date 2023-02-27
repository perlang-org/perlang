// Needed to be able to set CultureInfo.CurrentCulture without leaking the change to other test methods.

#pragma warning disable CS1998

using System.Globalization;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Operator.Binary
{
    public class AdditionTests
    {
        [Theory]
        [MemberData(nameof(BinaryOperatorData.Addition_result), MemberType = typeof(BinaryOperatorData))]
        void performs_addition(string i, string j, string expectedResult)
        {
            string source = $@"
                var i1 = {i};
                var i2 = {j};

                print i1 + i2;
            ";

            string result = EvalReturningOutputString(source);

            result.Should()
                .Be(expectedResult);
        }

        [Theory]
        [MemberData(nameof(BinaryOperatorData.Addition_type), MemberType = typeof(BinaryOperatorData))]
        void with_supported_types_returns_expected_type(string i, string j, string expectedType)
        {
            string source = $@"
                print ({i} + {j}).get_type();
            ";

            string result = EvalReturningOutputString(source);

            result.Should()
                .Be(expectedType);
        }

        [Theory]
        [MemberData(nameof(BinaryOperatorData.Addition_type), MemberType = typeof(BinaryOperatorData))]
        void local_variable_inference_returns_expected_type(string i, string j, string expectedType)
        {
            string source = $@"
                var i = {i} + {j};

                print i.get_type();
            ";

            string result = EvalReturningOutputString(source);

            result.Should()
                .Be(expectedType);
        }

        [Theory]
        [MemberData(nameof(BinaryOperatorData.Addition_unsupported_types), MemberType = typeof(BinaryOperatorData))]
        public void with_unsupported_types_emits_expected_error(string i, string j, string expectedResult)
        {
            string source = $@"
                print {i} + {j};
            ";

            var result = EvalWithValidationErrorCatch(source);

            result.Errors.Should()
                .ContainSingle().Which
                .Message.Should().Match(expectedResult);
        }

        [Fact]
        void addition_of_strings_performs_concatenation()
        {
            string source = @"
                var s1 = ""foo"";
                var s2 = ""bar"";

                print s1 + s2;
            ";

            string result = EvalReturningOutputString(source);

            result.Should()
                .Be("foobar");
        }

        [Fact]
        void addition_of_integer_and_string_coerces_number_to_string()
        {
            // Some interesting notes on how other languages deal with this:
            //
            // Ruby 2.6: Not supported. TypeError (no implicit conversion of Integer into String)
            // Python 2.7: Not supported. TypeError: cannot concatenate 'str' and 'int' objects
            // Java 11: String + int works fine. Int + string also coerces the integer to a string.
            // C#: Likewise.
            // Javascript: Likewise.

            string source = @"
                var i = 123;
                var s = ""abc"";

                print i + s;
            ";

            string result = EvalReturningOutputString(source);

            result.Should()
                .Be("123abc");
        }

        [Fact]
        void addition_of_bigint_and_string_coerces_number_to_string()
        {
            string source = @"
                var i = 18446744073709551616;
                var s = ""xyz"";

                print i + s;
            ";

            string result = EvalReturningOutputString(source);

            result.Should()
                .Be("18446744073709551616xyz");
        }

        [Fact]
        void addition_of_string_and_integer_coerces_number_to_string()
        {
            string source = @"
                var s = ""abc"";
                var i = 123;

                print s + i;
            ";

            string result = EvalReturningOutputString(source);

            result.Should()
                .Be("abc123");
        }

        [Fact]
        void addition_of_string_and_bigint_coerces_number_to_string()
        {
            string source = @"
                var s = ""abc"";
                var i = 18446744073709551616;

                print s + i;
            ";

            string result = EvalReturningOutputString(source);

            result.Should()
                .Be("abc18446744073709551616");
        }

        [Theory]
        [ClassData(typeof(TestCultures))]
        async Task addition_of_float_and_string_coerces_number_to_string(CultureInfo cultureInfo)
        {
            CultureInfo.CurrentCulture = cultureInfo;

            string source = @"
                var i = 123.45;
                var s = ""abc"";

                print i + s;
            ";

            string result = EvalReturningOutputString(source);

            result.Should()
                .Be("123.45abc");
        }

        [Theory]
        [ClassData(typeof(TestCultures))]
        async Task addition_of_string_and_float_coerces_number_to_string(CultureInfo cultureInfo)
        {
            CultureInfo.CurrentCulture = cultureInfo;

            string source = @"
                var s = ""abc"";
                var i = 123.45;

                print s + i;
            ";

            string result = EvalReturningOutputString(source);

            result.Should()
                .Be("abc123.45");
        }
    }
}
