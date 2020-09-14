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
        [Fact]
        public void empty_class()
        {
            string source = @"
                class Foo {}

                print Foo;
            ";

            var output = EvalReturningOutput(source);

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

            var result = EvalWithResolveErrorCatch(source);
            var exception = result.ResolveErrors.First();

            Assert.Single(result.ResolveErrors);
            Assert.Matches("Class Foo already defined; cannot redefine", exception.Message);
        }

        [Fact]
        public void can_get_reference_to_static_method()
        {
            string source = @"
                class Foo {}

                print Foo.to_string;
            ";

            var output = EvalReturningOutput(source);

            Assert.Equal(new[]
            {
                // This comes straight out of the MethodInfo, so uses .NET-casing for now.
                "#<Foo System.String ToString()>",
            }, output);
        }

        [Fact]
        public void can_call_static_method()
        {
            string source = @"
                class Foo {}

                print Foo.to_string();
            ";

            var output = EvalReturningOutput(source);

            Assert.Equal(new[]
            {
                "Foo",
            }, output);
        }

        [Fact] //(Skip = "Currently produces Perlang.Parser.ParseError: Expect ';' after value.")]
        public void can_chain_method_calls_for_static_method()
        {
            string source = @"
                class Foo {}

                print Foo.to_string().to_string();
            ";

            var output = EvalReturningOutput(source);

            Assert.Equal(new[]
            {
                "Foo",
            }, output);
        }
    }
}
