using System.Linq;
using FluentAssertions;
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
        public void class_can_be_instantiated_and_instance_method_can_be_called()
        {
            string source = """
                public class Greeter
                {
                    public say_hello(): void
                    {
                        print("Hello World from class instance method");
                    }
                }

                var greeter = new Greeter();
                greeter.say_hello();
                """;

            var output = EvalReturningOutputString(source);

            output.Should()
                .Be("Hello World from class instance method");
        }

        [Fact]
        public void class_can_be_instantiated_and_instance_method_can_return_string()
        {
            string source = """
                public class Greeter
                {
                    public greet(): string
                    {
                        return "Hello World being returned from class instance method";
                    }
                }

                var greeter = new Greeter();
                print greeter.greet();
                """;

            var output = EvalReturningOutputString(source);

            output.Should()
                .Be("Hello World being returned from class instance method");
        }

        [Fact]
        public void inferred_variable_can_be_initialized_with_instance_method_result()
        {
            string source = """
                public class Greeter
                {
                    public greet(): string
                    {
                        return "Hello World being returned from class instance method";
                    }
                }

                var greeter = new Greeter();
                var result = greeter.greet();
                print result;
                """;

            var output = EvalReturningOutputString(source);

            output.Should()
                .Be("Hello World being returned from class instance method");
        }

        [Fact]
        public void class_can_be_instantiated_and_instance_method_can_be_called_with_parameter()
        {
            string source = """
                public class Greeter
                {
                    public say_hello(name: string): void
                    {
                        print("Hello World, " + name);
                    }
                }

                var greeter = new Greeter();
                greeter.say_hello("Bob");
                """;

            var output = EvalReturningOutputString(source);

            output.Should()
                .Be("Hello World, Bob");
        }

        [Fact]
        public void string_and_instance_method_result_can_be_concatenated()
        {
            string source = """
                public class Greeter
                {
                    public get_greeting(): string
                    {
                        return "Bob";
                    }
                }

                var greeter = new Greeter();
                print "Hello, " + greeter.get_greeting();
                """;

            var output = EvalReturningOutputString(source);

            output.Should()
                .Be("Hello, Bob");
        }

        [Fact]
        public void instance_method_result_can_be_assigned_to_inferred_variable()
        {
            string source = """
                public class Greeter
                {
                    public get_greeting(): string
                    {
                        return "Hello, Bob";
                    }
                }

                var greeter = new Greeter();
                var greeting = greeter.get_greeting();

                print greeting;
                """;

            var output = EvalReturningOutputString(source);

            output.Should()
                .Be("Hello, Bob");
        }

        [Fact]
        public void instance_method_result_can_be_assigned_to_explicitly_typed_variable()
        {
            string source = """
                public class Greeter
                {
                    public get_greeting(): string
                    {
                        return "Hello, Bob";
                    }
                }

                var greeter = new Greeter();
                var greeting: string = greeter.get_greeting();

                print greeting;
                """;

            var output = EvalReturningOutputString(source);

            output.Should()
                .Be("Hello, Bob");
        }

        [Fact]
        public void instance_method_can_call_other_instance_method_with_explicit_this_prefix()
        {
            string source = """
                public class Greeter
                {
                    public say_hello(): void
                    {
                        print("Hello");
                        this.say_world();
                    }

                    public say_world(): void
                    {
                        print("World");
                    }
                }

                var greeter = new Greeter();
                greeter.say_hello();
                """;

            var output = EvalReturningOutput(source);

            output.Should()
                .Equal(
                    "Hello",
                    "World"
                );
        }

        [Fact]
        public void instance_method_can_call_other_instance_method_without_this_prefix()
        {
            string source = """
                public class Greeter
                {
                    // Note the order of the methods here. The say_world() must be defined *before* the say_hello() method
                    // because the name resolving was previously limited to a single pass. Another test will ensure that
                    // the 2-pass approach works as intended (avoiding having to do C++-style forward references)
                    public say_world(): void
                    {
                        print("World");
                    }

                    public say_hello(): void
                    {
                        print("Hello");
                        say_world();
                    }
                }

                var greeter = new Greeter();
                greeter.say_hello();
                """;

            var output = EvalReturningOutput(source);

            output.Should()
                .Equal(
                    "Hello",
                    "World"
                );
        }

        [Fact]
        public void instance_method_calling_non_existent_instance_method_with_explicit_this_prefix_throws_expected_error()
        {
            string source = """
                public class TestClass
                {
                    public calls_non_existent_method(): void
                    {
                        this.does_not_exist();
                    }
                }

                var test_class = new TestClass();
                test_class.calls_non_existent_method();
                """;

            var result = EvalWithValidationErrorCatch(source);

            result.Errors.Should()
                .ContainSingle()
                .Which
                .Message.Should().Contain("Failed to locate method 'does_not_exist' in class 'TestClass'");
        }

        [Fact]
        public void instance_method_can_call_another_instance_method_defined_later_in_the_class()
        {
            string source = """
                public class Greeter
                {
                    // This test has the methods defined in "top-to-bottom" order. This relies on a 2-pass approach, since
                    // the say_world() method isn't yet defined when say_hello() is being traversed in the syntax tree.
                    public say_hello(): void
                    {
                        say_world();
                    }

                    public say_world(): void
                    {
                        print("World");
                    }
                }

                var greeter = new Greeter();
                greeter.say_hello();
                """;

            var output = EvalReturningOutput(source);

            output.Should()
                .Equal(
                    "World"
                );
        }

        [Fact]
        public void instance_method_can_initialize_explicitly_typed_variable_with_result_from_another_instance_method()
        {
            string source = """
                public class Greeter
                {
                    public say_hello(): void
                    {
                        print("The world");
                        var greeting: string = this.say_world();
                        print greeting;
                    }

                    public say_world(): string
                    {
                        return "is not enough";
                    }
                }

                var greeter = new Greeter();
                greeter.say_hello();
                """;

            var output = EvalReturningOutput(source);

            output.Should()
                .Equal(
                    "The world",
                    "is not enough"
                );
        }

        [Fact]
        public void instance_method_can_initialize_explicitly_typed_variable_with_result_from_another_instance_method_without_this_prefix()
        {
            string source = """
                public class Greeter
                {
                    public say_hello(): void
                    {
                        print("The world");
                        var greeting: string = say_world();
                        print greeting;
                    }

                    public say_world(): string
                    {
                        return "is not enough";
                    }
                }

                var greeter = new Greeter();
                greeter.say_hello();
                """;

            var output = EvalReturningOutput(source);

            output.Should()
                .Equal(
                    "The world",
                    "is not enough"
                );
        }

        [Fact]
        public void instance_method_can_initialize_inferred_variable_with_result_from_another_instance_method()
        {
            string source = """
                public class Greeter
                {
                    public greet(): void
                    {
                        print("The world");

                        var greeting = this.get_second_part();
                        print greeting;
                    }

                    public get_second_part(): string
                    {
                        return "is not enough";
                    }
                }

                var greeter = new Greeter();
                greeter.greet();
                """;

            var output = EvalReturningOutput(source);

            output.Should()
                .Equal(
                    "The world",
                    "is not enough"
                );
        }

        [Fact]
        public void class_can_define_custom_constructor()
        {
            string source = """
                public class Greeter
                {
                    public constructor()
                    {
                        print("Hello from constructor");
                    }
                }

                var greeter = new Greeter();
                """;

            var output = EvalReturningOutput(source);

            output.Should()
                .Equal(
                    "Hello from constructor"
                );
        }

        [Fact]
        public void class_can_define_constructor_with_parameter()
        {
            string source = """
                public class Greeter
                {
                    public constructor(name: string)
                    {
                        print("Hello from constructor, " + name);
                    }
                }

                var greeter = new Greeter("Alice");
                """;

            var output = EvalReturningOutput(source);

            output.Should()
                .Equal(
                    "Hello from constructor, Alice"
                );
        }

        [Fact]
        public void class_can_define_constructor_and_destructor()
        {
            string source = """
                public class Greeter
                {
                    public constructor()
                    {
                        print("Hello from constructor");
                    }

                    public destructor()
                    {
                        print("Hello from destructor");
                    }

                    public say_hello(): void
                    {
                        print("Hello from say_hello");
                    }
                }

                var greeter = new Greeter();
                greeter.say_hello();
                """;

            var output = EvalReturningOutput(source);

            output.Should()
                .Equal(
                    "Hello from constructor",
                    "Hello from say_hello",
                    "Hello from destructor"
                );
        }

        [Fact]
        public void destructor_cannot_have_parameters()
        {
            string source = """
                public class Greeter
                {
                    public destructor(name: string)
                    {
                        print("Hello from destructor");
                    }
                }
                """;

            var result = EvalWithParseErrorCatch(source);

            result.Errors.Should()
                .ContainSingle()
                .Which
                .Message.Should().Contain("Destructor cannot have any parameters");
        }

        [Fact(Skip = "Does not yet have any meaningful C++ representation")]
        public void empty_class_can_be_accessed_by_name()
        {
            string source = @"
                public class Foo {}

                print Foo;
            ";

            var output = EvalReturningOutputString(source);

            Assert.Equal("Foo", output);
        }

        [Fact]
        public void duplicate_class_name_throws_expected_error()
        {
            string source = @"
                public class Foo {}
                public class Foo {}
            ";

            var result = EvalWithNameResolutionErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Matches("Class Foo already defined; cannot redefine", exception.Message);
        }

        [Fact(Skip = "Does not yet have any meaningful C++ representation")]
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
                public class Base64 {}
            ";

            var result = EvalWithNameResolutionErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Matches("Class Base64 already defined; cannot redefine", exception.Message);
        }

        [Fact]
        public void class_name_clash_with_native_object_throws_expected_error()
        {
            string source = @"
                public class ARGV {}
            ";

            var result = EvalWithNameResolutionErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Matches("Object ARGV already defined; cannot redefine", exception.Message);
        }

        [Fact]
        public void class_name_clash_with_function_throws_expected_error()
        {
            string source = @"
                fun Hello(): void {}

                public class Hello {}
            ";

            var result = EvalWithNameResolutionErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Matches("Function Hello already defined; cannot redefine", exception.Message);
        }

        [Fact]
        public void class_name_clash_with_variable_throws_expected_error()
        {
            string source = @"
                var Hello = 1;

                public class Hello {}
            ";

            var result = EvalWithNameResolutionErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Matches("Variable Hello already defined; cannot redefine", exception.Message);
        }

        [Fact(Skip = "Does not yet have any meaningful C++ representation")]
        public void can_get_reference_to_static_method()
        {
            string source = @"
                public class Foo {}

                print Foo.to_string;
            ";

            var output = EvalReturningOutputString(source);

            // This comes straight out of the MethodInfo, so uses .NET-casing for now.
            Assert.Equal("#<Foo System.String ToString()>", output);
        }

        // We need to figure out a way to handle method references in compiled mode, and once we've done that, how
        // to `print()` it in a reasonable manner. :-)
        [Fact(Skip = "Does not yet have any meaningful C++ representation")]
        public void can_get_reference_to_static_method_native_class()
        {
            string source = @"
                print Base64.to_string;
            ";

            var output = EvalReturningOutputString(source);

            // This comes straight out of the MethodInfo, so uses .NET-casing for now.
            Assert.Equal("#<Perlang.Stdlib.Base64 System.String ToString()>", output);
        }

        [Fact(Skip = "Does not yet work for user-defined classes (error: no member named 'Foo' in namespace)")]
        public void can_call_static_method()
        {
            string source = @"
                public class Foo {}

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

        [Fact(Skip = "Internal compiler error: unhandled type of get expression: Perlang.Expr+Call")]
        public void can_chain_method_calls_for_static_method()
        {
            string source = @"
                public class Foo {}

                print Foo.to_string().to_string();
            ";

            var output = EvalReturningOutputString(source);

            Assert.Equal("Foo", output);
        }
    }
}
