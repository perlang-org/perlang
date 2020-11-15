using System.Linq;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Classes
{
    /// <summary>
    /// Class-related tests.
    ///
    /// Based on https://github.com/munificent/craftinginterpreters/tree/master/test/class.
    /// </summary>
    public class ClassesTests
    {
        [Fact]
        public void empty_class_can_be_accessed_by_name()
        {
            string source = @"
                class Foo {}

                print Foo;
            ";

            var output = EvalReturningOutputString(source);

            Assert.Equal("Foo", output);
        }

        [Fact]
        public void duplicate_class_name_throws_expected_error()
        {
            string source = @"
                class Foo {}
                class Foo {}
            ";

            var result = EvalWithResolveErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Matches("Class Foo already defined; cannot redefine", exception.Message);
        }

        [Fact]
        public void native_class_can_be_accessed_by_name()
        {
            // For now, all native classes are registered in the global namespace. We could consider changing this,
            // since a global namespace is a precious thing that should be treated as such, preventing unnecessary
            // pollution. As long as we use this mechanism for mostly providing system-utilities that are widely useful
            // and ubiquitously useful, we'll live with this limitation..

            string source = @"
                print Base64;
            ";

            var output = EvalReturningOutputString(source);

            Assert.Equal("Perlang.Stdlib.Base64", output);
        }

        [Fact]
        public void class_name_clash_with_native_class_throws_expected_error()
        {
            string source = @"
                class Base64 {}
            ";

            var result = EvalWithResolveErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Matches("Class Base64 already defined; cannot redefine", exception.Message);
        }

        [Fact]
        public void class_name_clash_with_native_object_throws_expected_error()
        {
            string source = @"
                class ARGV {}
            ";

            var result = EvalWithResolveErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Matches("Object ARGV already defined; cannot redefine", exception.Message);
        }

        [Fact]
        public void class_name_clash_with_function_throws_expected_error()
        {
            string source = @"
                fun Hello() {}

                class Hello {}
            ";

            var result = EvalWithResolveErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Matches("Function Hello already defined; cannot redefine", exception.Message);
        }

        [Fact]
        public void class_name_clash_with_variable_throws_expected_error()
        {
            string source = @"
                var Hello = 1;

                class Hello {}
            ";

            var result = EvalWithResolveErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Matches("Variable Hello already defined; cannot redefine", exception.Message);
        }

        [Fact]
        public void can_get_reference_to_static_method()
        {
            string source = @"
                class Foo {}

                print Foo.to_string;
            ";

            var output = EvalReturningOutputString(source);

            // This comes straight out of the MethodInfo, so uses .NET-casing for now.
            Assert.Equal("#<Foo System.String ToString()>", output);
        }

        [Fact]
        public void can_get_reference_to_static_method_native_class()
        {
            string source = @"
                print Base64.to_string;
            ";

            var output = EvalReturningOutputString(source);

            // This comes straight out of the MethodInfo, so uses .NET-casing for now.
            Assert.Equal("#<Perlang.Stdlib.Base64 System.String ToString()>", output);
        }

        [Fact]
        public void can_call_static_method()
        {
            string source = @"
                class Foo {}

                print Foo.to_string();
            ";

            var output = EvalReturningOutputString(source);

            Assert.Equal("Foo", output);
        }

        [Fact]
        public void can_call_static_method_native_class()
        {
            string source = @"
                print Base64.to_string();
            ";

            var output = EvalReturningOutputString(source);

            Assert.Equal("Perlang.Stdlib.Base64", output);
        }

        [Fact]
        public void can_chain_method_calls_for_static_method()
        {
            string source = @"
                class Foo {}

                print Foo.to_string().to_string();
            ";

            var output = EvalReturningOutputString(source);

            Assert.Equal("Foo", output);
        }
    }
}
