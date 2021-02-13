using System.Linq;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Typing
{
    public class TypingTests
    {
        [Fact]
        public void var_declaration_can_provide_a_type()
        {
            string source = @"
                var s: String = ""Hello World"";
            ";

            var result = EvalReturningOutput(source);

            Assert.Empty(result);
        }

        [Fact]
        public void var_declaration_generates_expected_error_if_non_type_expression_specified()
        {
            string source = @"
                var s: 42 = ""Hello World"";
            ";

            var result = EvalWithParseErrorCatch(source);
            var exception = result.Errors.FirstOrDefault();

            Assert.Single(result.Errors);
            Assert.Matches("Expecting type name", exception.Message);
        }

        [Fact]
        public void var_declaration_generates_expected_error_if_assigned_to_incoercible_type()
        {
            string source = @"
                var s: int = ""Hello World"";
            ";

            var result = EvalWithValidationErrorCatch(source);
            var exception = result.Errors.FirstOrDefault();

            Assert.Single(result.Errors);
            Assert.Matches("Cannot assign String value to Int32", exception.Message);
        }

        [Fact]
        public void var_declaration_detects_type_not_found()
        {
            string source = @"
                var s: SomeUnknownType = ""Hello World"";
            ";

            var result = EvalWithValidationErrorCatch(source);
            var exception = result.Errors.FirstOrDefault();

            Assert.Single(result.Errors);
            Assert.Matches("Type not found: SomeUnknownType", exception.Message);
        }

        [Fact]
        public void var_declaration_in_block_detects_type_not_found()
        {
            string source = @"
                var foo: int = 123;

                {
                    var s: SomeUnknownType = ""Hello World"";
                }
            ";

            var result = EvalWithValidationErrorCatch(source);
            var exception = result.Errors.FirstOrDefault();

            Assert.Single(result.Errors);
            Assert.Matches("Type not found: SomeUnknownType", exception.Message);
        }

        [Fact]
        public void var_declaration_in_function_detects_type_not_found()
        {
            string source = @"
                var foo: int = 123;

                fun bar(): void {
                    var s: SomeUnknownType = ""Hello World"";
                }
            ";

            var result = EvalWithValidationErrorCatch(source);
            var exception = result.Errors.FirstOrDefault();

            Assert.Single(result.Errors);
            Assert.Matches("Type not found: SomeUnknownType", exception.Message);
        }

        [Fact]
        public void var_declaration_with_initializer_correctly_infers_type_from_assignment_source_variable()
        {
            string source = @"
                var foo = 123;
                var bar = foo;

                print bar.get_type();
            ";

            string result = EvalReturningOutputString(source);
            Assert.Equal("System.Int32", result);
        }

        [Fact]
        public void var_declaration_with_initializer_correctly_infers_type_from_assignment_source_expression()
        {
            string source = @"
                var foo = 123;
                var bar = foo + 2;

                print bar.get_type();
            ";

            string result = EvalReturningOutputString(source);
            Assert.Equal("System.Int32", result);
        }

        [Fact]
        public void function_parameter_can_provide_a_type()
        {
            string source = @"
                fun foo(s: String): void {
                    print(s);
                }

                foo(""Hello World"");
            ";

            var output = EvalReturningOutputString(source);

            Assert.Equal("Hello World", output);
        }

        [Fact]
        public void function_parameter_detects_when_method_is_called_with_wrong_type()
        {
            string source = @"
                fun foo(s: String): void {
                    print(s);
                }

                foo(42);
            ";

            var result = EvalWithValidationErrorCatch(source);
            var exception = result.Errors.FirstOrDefault();

            Assert.Single(result.Errors);
            Assert.Matches("Cannot pass System.Int32 argument as parameter 's: System.String'", exception.Message);
        }

        [Fact]
        public void function_return_value_can_provide_a_type()
        {
            string source = @"
                fun foo(): String {
                    return ""typed return value"";
                }

                print foo();
            ";

            var output = EvalReturningOutputString(source);

            Assert.Equal("typed return value", output);
        }

        [Fact]
        public void function_return_value_can_be_assigned_to_variable_of_same_type()
        {
            string source = @"
                fun foo(): string {
                    return ""typed return value"";
                }

                var s: String = foo();
                print s;
            ";

            var output = EvalReturningOutputString(source);

            Assert.Equal("typed return value", output);
        }

        [Fact]
        public void function_return_value_detects_type_not_found()
        {
            string source = @"
                fun foo(): SomeUnknownType {
                    return ""typed return value"";
                }
            ";

            var result = EvalWithValidationErrorCatch(source);
            var exception = result.Errors.FirstOrDefault();

            Assert.Single(result.Errors);
            Assert.Matches("Type not found: SomeUnknownType", exception.Message);
        }

        [Fact]
        public void function_return_value_detects_invalid_assignment_to_variable_of_another_type()
        {
            string source = @"
                fun foo(): string {
                    return ""typed return value"";
                }

                // The line below should fail, because types are mixed up.
                var s: int = foo();
                print s;
            ";

            var result = EvalWithValidationErrorCatch(source);
            var exception = result.Errors.FirstOrDefault();

            Assert.Single(result.Errors);
            Assert.Matches("Cannot assign String value to Int32", exception.Message);
        }
    }
}
