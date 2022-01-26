#pragma warning disable CS1998

using System.Globalization;
using System.Threading.Tasks;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Operator
{
    public class Addition
    {
        [Fact]
        public void addition_of_strings_performs_concatenation()
        {
            string source = @"
                var s1 = ""foo"";
                var s2 = ""bar"";

                print s1 + s2;
            ";

            string result = EvalReturningOutputString(source);
            Assert.Equal("foobar", result);
        }

        [Fact]
        public void addition_of_integers_performs_numeric_addition()
        {
            string source = @"
                var i1 = 12;
                var i2 = 34;

                print i1 + i2;
            ";

            string result = EvalReturningOutputString(source);
            Assert.Equal("46", result);
        }

        [Fact]
        public void addition_of_integer_and_string_coerces_number_to_string()
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
            Assert.Equal("123abc", result);
        }

        [Fact]
        public void addition_of_string_and_integer_coerces_number_to_string()
        {
            string source = @"
                var s = ""abc"";
                var i = 123;

                print s + i;
            ";

            string result = EvalReturningOutputString(source);
            Assert.Equal("abc123", result);
        }

        [Theory]
        [ClassData(typeof(TestCultures))]
        public async Task addition_of_float_and_string_coerces_number_to_string(CultureInfo cultureInfo)
        {
            CultureInfo.CurrentCulture = cultureInfo;

            string source = @"
                var i = 123.45;
                var s = ""abc"";

                print i + s;
            ";

            string result = EvalReturningOutputString(source);
            Assert.Equal("123.45abc", result);
        }

        [Theory]
        [ClassData(typeof(TestCultures))]
        public async Task addition_of_string_and_float_coerces_number_to_string(CultureInfo cultureInfo)
        {
            CultureInfo.CurrentCulture = cultureInfo;

            string source = @"
                var s = ""abc"";
                var i = 123.45;

                print s + i;
            ";

            string result = EvalReturningOutputString(source);
            Assert.Equal("abc123.45", result);
        }
    }
}
