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
        public void assigning_instance_method_void_result_to_variable_emits_expected_error()
        {
            string source = """
                public class Greeter
                {
                    public get_greeting(): void
                    {
                    }
                }

                var greeter = new Greeter();

                // This line is expected to fail in the C++ compilation, since the return type will be inferred to 'void'.
                // In the future, we could catch this on the Perlang side and produce a pretty(er) error message, but the
                // important part right is not that we at least catch it _somewhere_.
                var greeting = greeter.get_greeting();
                """;

            var result = EvalWithCppCompilationErrorCatch(source);

            result.Errors.Should()
                .ContainSingle()
                .Which
                .Message.Should().Contain("error: variable has incomplete type 'void'");
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
                .Message.Should().Contain("Failed to locate symbol 'does_not_exist' in class 'TestClass'");
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
        public void defining_immutable_field_throws_expected_error()
        {
            string source = """
                public class Greeter
                {
                    private name_: string;
                }
                """;

            var result = EvalWithParseErrorCatch(source);

            result.Errors.Should()
                .ContainSingle()
                .Which
                .Message.Should().Contain("immutable fields are not yet supported");
        }

        [Fact]
        public void class_can_define_mutable_private_fields()
        {
            string source = """
                public class Greeter
                {
                    // Suffix is technically not necessary, but it makes it easier to look at the scopes in the debugger
                    // when there are less things with similar names. We have a separate test which validates that
                    // shadowing works as intended.
                    private mutable name_: string;
                    private mutable age_: int;

                    public constructor(name: string, age: int)
                    {
                        this.name_ = name;
                        this.age_ = age;
                    }

                    public say_hello(): void
                    {
                        print("Hello from say_hello, " + this.name_ + ", " + this.age_);
                    }
                }

                var greeter = new Greeter("Bob", 42);
                greeter.say_hello();
                """;

            var output = EvalReturningOutput(source);

            output.Should()
                .Equal(
                    "Hello from say_hello, Bob, 42"
                );
        }

        [Fact]
        public void class_can_define_private_fields_with_default_values()
        {
            string source = """
                public class Greeter
                {
                    // TODO: Remove the 'mutable' keyword here when we support immutable fields
                    private mutable name: string = "Bob";
                    private mutable age: int = 42;

                    public say_hello(): void
                    {
                        print("Hello from say_hello, " + this.name + ", " + this.age);
                    }
                }

                var greeter = new Greeter();
                greeter.say_hello();
                """;

            var output = EvalReturningOutput(source);

            output.Should()
                .Equal(
                    "Hello from say_hello, Bob, 42"
                );
        }

        [Fact]
        public void defining_fields_with_incoercible_value_throws_expected_error()
        {
            string source = """
                public class Greeter
                {
                    // TODO: Remove mutable specifier when supported

                    // The initializer (42) is not implicitly coercible to string
                    private mutable name: string = 42;
                }
                """;

            var result = EvalWithValidationErrorCatch(source);

            result.Errors.Should()
                .ContainSingle()
                .Which
                .Message.Should().Contain("Cannot assign int to string field");
        }

        [Fact]
        public void fields_can_be_accessed_without_this_prefix()
        {
            string source = """
                public class Greeter
                {
                    // TODO: Should be able to be immutable, when we support initializing immutable fields from
                    // constructor.
                    private mutable name: string;

                    public constructor(name: string)
                    {
                        // Print before assignment, to ensure the parameters take precedence over the fields.
                        print("Hello from constructor, " + name);

                        this.name = name;
                    }

                    public say_hello(): void
                    {
                        print("Hello from say_hello, " + name);
                    }
                }

                var greeter = new Greeter("Charlie");
                greeter.say_hello();
                """;

            var output = EvalReturningOutput(source);

            output.Should()
                .Equal(
                    "Hello from constructor, Charlie",
                    "Hello from say_hello, Charlie"
                );
        }

        [Fact]
        public void parameter_names_can_shadow_fields()
        {
            string source = """
                public class Greeter
                {
                    // TODO: Can be immutable when supported by compiler
                    private mutable name: string = "Default name";

                    public constructor(name: string)
                    {
                        print("this.name is, before assignment: " + this.name);

                        this.name = name;

                        print("name is: " + name);
                        print("this.name is, after assignment: " + this.name);
                    }
                }

                var greeter = new Greeter("Foxtrot");
                """;

            var output = EvalReturningOutput(source);

            output.Should()
                .Equal(
                    "this.name is, before assignment: Default name",
                    "name is: Foxtrot",
                    "this.name is, after assignment: Foxtrot"
                );
        }

        // TODO: This is a big limitation; code like this is something which should definitely be detected. We must not
        // TODO: have any such thing as "undefined behaviour" in Perlang.
        [Fact(Skip = "Validation of this is not yet implemented; does not emit warnings on either the Perlang or C++ side")]
        public void uninitialized_fields_emits_expected_error()
        {
            string source = """
                public class Greeter
                {
                    private name: string;

                    public constructor()
                    {
                    }

                    // Adding dummy method for now since this will trigger a Valgrind error, because of uninitialized
                    // memory (and hopefully crash the program/fail the test if run without Valgrind).
                    public say_hello(): void
                    {
                        print("Hello from say_hello, " + name);
                    }
                }

                var greeter = new Greeter();
                greeter.say_hello();
                """;

            var result = EvalWithCppCompilationErrorCatch(source);

            result.Errors.Should()
                .ContainSingle()
                .Which
                .Message.Should().Contain("TODO");
        }

        [Fact]
        public void public_field_emits_expected_error()
        {
            string source = """
                public class Greeter
                {
                    // TODO: Remove mutable keyword when we introduce support for imutable fields

                    // This is currently not supported; fields must be private (or protected, when we implement
                    // inheritance)
                    public mutable name: string;
                }
                """;

            var result = EvalWithParseErrorCatch(source);

            result.Errors.Should()
                .ContainSingle()
                .Which
                .Message.Should().Contain("Fields must be declared as private");
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
