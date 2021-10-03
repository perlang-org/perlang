using System;
using System.Linq;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Typing
{
    /// <summary>
    /// Typing-related tests.
    ///
    /// These tests test both explicitly and implicitly typed expressions.
    /// </summary>
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
        public void var_declaration_with_initializer_correctly_infers_type_from_assignment_source_int_variable()
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
        public void var_declaration_with_initializer_correctly_infers_type_from_assignment_source_long_variable()
        {
            string source = @"
                var foo = 8589934592;
                var bar = foo;

                print bar.get_type();
            ";

            string result = EvalReturningOutputString(source);
            Assert.Equal("System.Int64", result);
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
        public void var_declaration_with_initializer_detects_inference_attempt_from_null()
        {
            // The following program should fail, since there is no way the compiler can infer any type information from
            // a 'null' initializer.
            string source = @"
                var s = null;

                print s;
            ";

            var result = EvalWithValidationErrorCatch(source);
            var exception = result.Errors.FirstOrDefault();

            Assert.Single(result.Errors);
            Assert.Matches("Cannot assign null to an implicitly typed local variable", exception.Message);
        }

        [Fact]
        public void var_declaration_with_initializer_emits_warning_on_null_usage()
        {
            string source = @"
                var s: string = null;

                print s;
            ";

            var result = EvalWithResult(source);

            Assert.Empty(result.Errors);
            Assert.Single(result.CompilerWarnings);

            var warning = result.CompilerWarnings.Single();
            Assert.Matches("Initializing variable to null detected", warning.Message);
        }

        [Fact]
        public void var_declaration_supports_reassignment_after_variable_is_defined()
        {
            string source = @"
                var i = 123;
                i = 456;

                print i;
            ";

            string result = EvalReturningOutputString(source);
            Assert.Equal("456", result);
        }

        [Fact]
        public void var_declaration_detects_reassignment_of_incoercible_type()
        {
            string source = @"
                var i = 123;
                i = ""foo"";

                print i;
            ";

            var result = EvalWithValidationErrorCatch(source);
            var exception = result.Errors.FirstOrDefault();

            Assert.Single(result.Errors);
            Assert.Matches("Cannot assign System.String to variable defined as 'System.Int32'", exception.Message);
        }

        [Fact]
        public void var_declaration_supports_reassignment_to_null_for_reference_type()
        {
            string source = @"
                var s = ""foo"";
                s = null;

                print s;
            ";

            var result = EvalWithResult(source);

            Assert.Equal("null", result.OutputAsString);
        }

        [Fact]
        public void var_declaration_emits_warning_on_reassignment_to_null_for_reference_type()
        {
            string source = @"
                var s = ""foo"";
                s = null;

                print s;
            ";

            EvalResult<Exception> result = EvalWithResult(source);

            Assert.Empty(result.Errors);
            Assert.Single(result.CompilerWarnings);

            var warning = result.CompilerWarnings.Single();
            Assert.Matches("Null assignment detected", warning.Message);
        }

        [Fact]
        public void var_declaration_detects_reassignment_to_null_for_value_type()
        {
            string source = @"
                var i = 1;
                i = null;

                print i;
            ";

            var result = EvalWithValidationErrorCatch(source);
            var exception = result.Errors.FirstOrDefault();

            Assert.Single(result.Errors);
            Assert.Matches("Cannot assign Perlang.NullObject to variable defined as 'System.Int32'", exception.Message);
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
        public void function_parameter_detects_when_method_is_called_with_wrong_argument_type()
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
        public void function_parameter_allows_null_argument_for_reference_type_parameter()
        {
            string source = @"
                fun foo(s: String): void {
                    print(s);
                }

                foo(null);
            ";

            var result = EvalWithResult(source);

            Assert.Equal("null", result.OutputAsString);
        }

        [Fact]
        public void function_parameter_emits_warning_when_null_passed_for_reference_type_parameter()
        {
            string source = @"
                fun foo(s: String): void {
                    print(s);
                }

                foo(null);
            ";

            var result = EvalWithResult(source);

            Assert.Empty(result.Errors);
            Assert.Single(result.CompilerWarnings);

            var warning = result.CompilerWarnings.Single();
            Assert.Matches("Null parameter detected", warning.Message);
        }

        [Fact]
        public void function_parameter_detects_null_argument_for_value_type_parameter()
        {
            string source = @"
                fun foo(s: int): void {
                    print(s);
                }

                foo(null);
            ";

            var result = EvalWithValidationErrorCatch(source);
            var exception = result.Errors.FirstOrDefault();

            Assert.Single(result.Errors);
            Assert.Matches("Cannot pass Perlang.NullObject argument as parameter 's: System.Int32'", exception.Message);
        }

        [Fact]
        public void function_return_type_can_specify_explicit_type()
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
        public void function_return_type_detects_type_not_found()
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
