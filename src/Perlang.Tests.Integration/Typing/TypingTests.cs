#pragma warning disable SA1025
using System;
using System.Linq;
using Perlang.Compiler;
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

            // TODO: Fix this error. Should be something like "Cannot initialize int variable with AsciiString";
            // TODO: initialization isn't assignment.
            Assert.Single(result.Errors);
            Assert.Equal("Cannot assign AsciiString to int variable", exception.Message);
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

        // These tests deliberately test the edge cases (minimum/maximum values), to prevent off-by-one errors.

        [SkippableTheory]
        [InlineData("2147483647", "System.Int32")]              // Int32.MaxValue
        [InlineData("-2147483648", "System.Int32")]             // Int32.MinValue
        [InlineData("4294967295", "System.UInt32")]             // UInt32.MaxValue
        [InlineData("4294967296", "System.Int64")]              // UInt32.MaxValue + 1 => must be parsed as a `long` to avoid data loss.
        [InlineData("-4294967296", "System.Int64")]             // -(UInt32.MaxValue + 1) => must be parsed as a `long` to avoid data loss.
        [InlineData("9223372036854775807", "System.Int64")]     // Int64.MaxValue
        [InlineData("-9223372036854775808", "System.Int64")]    // Int64.MinValue
        [InlineData("18446744073709551616", "System.Numerics.BigInteger")] // UInt64.MaxValue + 1 => should be parsed as a `bigint` to avoid data loss.
        [InlineData("-9223372036854775809", "System.Numerics.BigInteger")] // Int64.MinValue - 1 => should be parsed as a `bigint` to avoid data loss.
        public void var_declaration_infers_type_from_initializer(string value, string expectedType)
        {
            string source = $@"
                var i = {value};

                print i.get_type();
            ";

            string result = EvalReturningOutputString(source);
            Assert.Equal(expectedType, result);
        }

        [SkippableFact]
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

        [SkippableFact]
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

        [SkippableFact]
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

            var result = EvalWithResult(source, CompilerFlags.CacheDisabled);

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
            Assert.Matches("Cannot assign 'AsciiString' to 'int' variable", exception!.Message);
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

            EvalResult<Exception> result = EvalWithResult(source, CompilerFlags.CacheDisabled);

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
            Assert.Matches("Cannot assign null to 'int' variable", exception!.Message);
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
            Assert.Matches("Cannot pass int argument as parameter 's: string'", exception!.Message);
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

            var result = EvalWithResult(source, CompilerFlags.CacheDisabled);

            Assert.Empty(result.Errors);
            Assert.Single(result.CompilerWarnings);

            var warning = result.CompilerWarnings.Single();
            Assert.Matches("Null parameter detected", warning.Message);
        }

        [Fact]
        public void function_parameter_detects_null_argument_for_value_type_parameter()
        {
            string source = @"
                fun foo(i: int): void {
                    print(i);
                }

                foo(null);
            ";

            var result = EvalWithValidationErrorCatch(source);
            var exception = result.Errors.FirstOrDefault();

            Assert.Single(result.Errors);
            Assert.Matches("Cannot pass null argument as parameter 'i: int'", exception!.Message);
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
            Assert.Matches("Type not found: SomeUnknownType", exception!.Message);
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
            Assert.Equal("Cannot assign string to int variable", exception!.Message);
        }

        [Fact]
        public void var_of_non_existent_type_with_initializer_emits_expected_error()
        {
            // TODO: I have a feeling that this somehow incorrectly infers the type from the 1 integer to some parts of
            // the parsed syntax tree (since NonexistentType does not exist). Investigate this someday, since it could
            // lead to subtle, very hard-to-track bugs.
            string source = @"
                var i: NonexistentType = 1; i++; print i.get_type();
            ";

            var result = EvalWithValidationErrorCatch(source);
            var exception = result.Errors.FirstOrDefault();

            Assert.Single(result.Errors);
            Assert.Equal("Type not found: NonexistentType", exception!.Message);
        }

        [Fact]
        public void var_of_non_existent_type_without_initializer_emits_expected_error()
        {
            string source = @"
                var f: Foo; f.do_stuff();
            ";

            var result = EvalWithValidationErrorCatch(source);
            var exception = result.Errors.FirstOrDefault();

            Assert.Single(result.Errors);
            Assert.Equal("Type not found: Foo", exception!.Message);
        }
    }
}
