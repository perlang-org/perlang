using System.Linq;
using Xunit;
using static Perlang.Tests.EvalHelper;

namespace Perlang.Tests.Classes
{
    /// <summary>
    /// Class-related tests
    ///
    /// Based on https://github.com/munificent/craftinginterpreters/tree/master/test/class
    /// </summary>
    public class ClassesTests
    {
        //[Fact]
        public void empty_class()
        {
            string source = @"
                class Foo {}

                print Foo;
            ";

            var output = Eval(source);

            Assert.Equal(new[]
            {
                "Foo",
            }, output);
        }

        [Fact]
        public void duplicate_class_name()
        {
            string source = @"
                class Foo {}
                class Foo {}
            ";

            var result = EvalWithTypeValidationErrorCatch(source);
            var exception = result.TypeValidationErrors.First();

            Assert.Single(result.TypeValidationErrors);
            Assert.Matches("TODO: do something useful here", exception.Message);
        }
    }
}
