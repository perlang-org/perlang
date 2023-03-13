using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Operator
{
    public class PostfixIncrement
    {
        //
        // "Positive" tests, testing for supported behavior
        //

        [Theory]
        [InlineData("int", "0", "1", "en-US")]
        [InlineData("int", "0", "1", "sv-SE")]
        [InlineData("long", "4294967296", "4294967297", "en-US")]
        [InlineData("long", "4294967296", "4294967297", "sv-SE")]
        [InlineData("bigint", "1267650600228229401496703205376", "1267650600228229401496703205377", "en-US")]
        [InlineData("bigint", "1267650600228229401496703205376", "1267650600228229401496703205377", "sv-SE")]
        [InlineData("double", "4294967296.123", "4294967297.123", "en-US")]
        [InlineData("double", "4294967296.123", "4294967297.123", "sv-SE")]
        public async Task incrementing_variable_assigns_expected_value(string type, string before, string after, string cultureName)
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultureName);

            string source = $@"
                var i: {type} = {before};
                i++;
                print i;
            ";

            var output = EvalReturningOutputString(source);

            Assert.Equal(after, output);
        }

        [Theory]
        [InlineData("int", "0", "System.Int32", "en-US")]
        [InlineData("int", "0", "System.Int32", "sv-SE")]
        [InlineData("long", "4294967296", "System.Int64", "en-US")]
        [InlineData("long", "4294967296", "System.Int64", "sv-SE")]
        [InlineData("bigint", "1267650600228229401496703205376", "System.Numerics.BigInteger", "en-US")]
        [InlineData("bigint", "1267650600228229401496703205376", "System.Numerics.BigInteger", "sv-SE")]
        [InlineData("double", "4294967296.123", "System.Double", "en-US")]
        [InlineData("double", "4294967296.123", "System.Double", "sv-SE")]
        public async Task incrementing_variable_retains_expected_type(string type, string before, string expectedClrType, string cultureName)
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultureName);

            string source = $@"
                var i: {type} = {before};
                i++;
                print i.get_type();
            ";

            string output = EvalReturningOutputString(source);

            Assert.Equal(expectedClrType, output);
        }

        [Fact]
        public void increment_can_be_used_in_for_loops()
        {
            string source = @"
                for (var c = 0; c < 3; c++)
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
        public void increment_can_be_used_in_assignment()
        {
            string source = @"
                var i = 100;
                var j = i++;
                print j;
            ";

            var output = EvalReturningOutputString(source);

            // As in languages C# and Java (and unlike C and C++), the operation above has well-defined semantics.
            // The value of i++ is the value of the expression _before_ it gets evaluated, just like in those other
            // languages. If we had a prefix increment operator, it would differ in this regard.
            Assert.Equal("100", output);
        }

        //
        // "Negative tests", ensuring that unsupported operations fail in the expected way.
        //

        [Fact]
        public void incrementing_undefined_variable_throws_expected_exception()
        {
            string source = @"
                x++;
            ";

            var result = EvalWithValidationErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Matches("Undefined identifier 'x'", exception.Message);
        }

        [Fact]
        public void incrementing_null_variable_throws_expected_exception()
        {
            string source = @"
                var s: string = null;
                s++;
            ";

            var result = EvalWithRuntimeErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Matches("can only be used to increment numbers, not null", exception.Message);
        }

        [Fact]
        public void incrementing_string_throws_expected_exception()
        {
            string source = @"
                var i = ""foo"";
                i++;
            ";

            var result = EvalWithRuntimeErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Matches("can only be used to increment numbers, not AsciiString", exception.Message);
        }
    }
}
