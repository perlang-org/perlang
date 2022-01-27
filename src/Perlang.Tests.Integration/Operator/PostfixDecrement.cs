using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Operator
{
    public class PostfixDecrement
    {
        //
        // "Positive" tests, testing for supported behavior
        //

        [Theory]
        [ClassData(typeof(TestCultures))]
        public async Task decrementing_defined_variable(CultureInfo cultureInfo)
        {
            CultureInfo.CurrentCulture = cultureInfo;

            string source = @"
                var i = 0;
                i--;
                print i;
            ";

            var output = EvalReturningOutput(source);

            Assert.Equal(new[] { "-1" }, output);
        }

        [Theory]
        [InlineData("int", "1", "System.Int32", "en-US")]
        [InlineData("int", "1", "System.Int32", "sv-SE")]
        [InlineData("long", "4294967296", "System.Int64", "en-US")]
        [InlineData("long", "4294967296", "System.Int64", "sv-SE")]
        [InlineData("bigint", "1267650600228229401496703205376", "System.Numerics.BigInteger", "en-US")]
        [InlineData("bigint", "1267650600228229401496703205376", "System.Numerics.BigInteger", "sv-SE")]
        [InlineData("double", "4294967296.123", "System.Double", "en-US")]
        [InlineData("double", "4294967296.123", "System.Double", "sv-SE")]
        public async Task decrementing_variable_retains_expected_type(string type, string before, string expectedClrType, string cultureName)
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultureName);

            string source = $@"
                var i: {type} = {before};
                i--;
                print i.get_type();
            ";

            var output = EvalReturningOutputString(source);

            Assert.Equal(expectedClrType, output);
        }

        [Fact]
        public void decrement_can_be_used_in_for_loops()
        {
            string source = @"
                for (var c = 3; c > 0; c--)
                    print c;
            ";

            var output = EvalReturningOutput(source);

            Assert.Equal(new[]
            {
                "3",
                "2",
                "1"
            }, output);
        }

        [Fact]
        public void decrement_can_be_used_in_assignment()
        {
            string source = @"
                var i = 100;
                var j = i--;
                print j;
            ";

            var output = EvalReturningOutput(source).SingleOrDefault();

            // As in languages C# and Java (and unlike C and C++), the operation above has well-defined semantics.
            // The value of i++ is the value of the expression _before_ it gets evaluated, just like in those other
            // languages. If we had a prefix decrement operator, it would differ in this regard.
            Assert.Equal("100", output);
        }

        //
        // "Negative tests", ensuring that unsupported operations fail in the expected way.
        //

        [Fact]
        public void decrementing_undefined_variable_throws_expected_exception()
        {
            string source = @"
                x--;
            ";

            var result = EvalWithValidationErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Matches("Undefined identifier 'x'", exception.Message);
        }

        [Fact]
        public void decrementing_null_variable_throws_expected_exception()
        {
            string source = @"
                var s: string = null;
                s--;
            ";

            var result = EvalWithRuntimeErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Matches("can only be used to decrement numbers, not null", exception.Message);
        }

        [Fact]
        public void decrementing_string_throws_expected_exception()
        {
            string source = @"
                var s = ""foo"";
                s--;
            ";

            var result = EvalWithRuntimeErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Matches("can only be used to decrement numbers, not string", exception.Message);
        }
    }
}
