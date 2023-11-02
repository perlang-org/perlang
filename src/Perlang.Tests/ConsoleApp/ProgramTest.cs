#pragma warning disable S3626

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Perlang.ConsoleApp;
using Perlang.Parser;
using Xunit;

namespace Perlang.Tests.ConsoleApp
{
    /// <summary>
    /// Test for the Program class. This essentially lets us test the REPL, or various aspects of the language which
    /// behaves differently in REPL mode vs regular interpreted/compiled modes.
    /// </summary>
    public static class ProgramTest
    {
        public class Run
        {
            private readonly Program subject;
            private readonly List<string> output = new();

            public Run()
            {
                subject = new Program(
                    replMode: true,
                    standardOutputHandler: s => output.Add(s.ToString()),
                    runtimeErrorHandler: e => throw e,
                    disabledWarningsAsErrors: Enumerable.Empty<WarningType>()
                );
            }

            [Fact]
            public void supports_multiple_statements_separated_by_semicolon()
            {
                subject.Run("var a = 42; print a;", FatalWarningsHandler);

                output.Should().Equal("42");
            }

            [Fact]
            public void supports_final_semicolon_elision_single_statement()
            {
                subject.Run("print 10", FatalWarningsHandler);

                output.Should().Equal("10");
            }

            [Fact]
            public void supports_final_semicolon_elision_multiple_statements()
            {
                subject.Run("var a = 43; print a", FatalWarningsHandler);

                output.Should().Equal("43");
            }

            [Fact]
            public void state_persists_between_invocations()
            {
                subject.Run("var a = 44;", FatalWarningsHandler);
                subject.Run("print a;", FatalWarningsHandler);

                output.Should().Equal("44");
            }

            [Fact]
            public void can_call_function_from_statement()
            {
                subject.Run("fun hello(): void { print 1; }", FatalWarningsHandler);
                subject.Run("hello();", FatalWarningsHandler);

                output.Should().Equal("1");
            }

            // This illustrates the bug described in #124; the example there used to throw an exception like this:
            // [line 1] Error at 'hello': Attempting to call undefined function 'hello'
            [Fact]
            public void can_call_function_from_expression()
            {
                subject.Run("fun hello(): void { print 1; }", FatalWarningsHandler);
                subject.Run("hello()", FatalWarningsHandler);

                output.Should().Equal("1");
            }

            [Fact]
            public void state_does_not_persist_if_one_statement_is_invalid()
            {
                // When a program has an error (like the second Run() invocation below), all variables defined in it are
                // discarded. That's why the third line is expected to generate a similar error; b is not defined at that
                // stage.
                //
                // This might seem a bit counterintuitive and we should consider changing this going forward. One way to
                // improve in this area would be to break up the resolve-and-type-validate-and-interpret block a bit, to
                // instead of doing it for all statements parsed instead do it for one statement at a time. That way, only
                // subsequent declarations _after_ a statement with error would be discarded.
                //
                // Or even so: we could even just ignore invalid statements, meaning that the 'var c = 44' statement below
                // would be successfully executed. This would perhaps be the most intuitive REPL experience.
                subject.Run("var a = 42;", FatalWarningsHandler);
                subject.Run("var b = 43; x; var c = 44;", FatalWarningsHandler);
                subject.Run("print b;", FatalWarningsHandler);
                subject.Run("print c;", FatalWarningsHandler);

                output.Count.Should().Be(3);
                output[0].Should().Match("* Undefined identifier 'x'");
                output[1].Should().Match("* Undefined identifier 'b'");
                output[2].Should().Match("* Undefined identifier 'c'");
            }

            [Fact]
            public void variable_redefined_throws_expected_error()
            {
                // Act
                subject.Run("var a = 42;", FatalWarningsHandler);

                // Assert
                subject.Invoking(y => y.Run("var a = 44;", FatalWarningsHandler))
                    .Should().Throw<RuntimeError>()
                    .WithMessage("Variable with this name already declared in this scope.");
            }

            // Test added to assert the bug fix for #117. Interestingly enough, the NRE described there did not occur when

            // the test was placed in the ArgvTests class.

            [Fact]
            public void Time_now_tickz_fails_with_expected_exception()
            {
                subject.Run("Time.now().tickz()", FatalWarningsHandler);

                output.Should().Equal(
                    "[line 1] Error at 'tickz': Failed to locate method 'tickz' in class 'DateTime'"
                );
            }

            [Fact]
            public void local_variable_inference_from_null_throws_the_expected_exception()
            {
                subject.Run("var s = null;", FatalWarningsHandler);

                output.Should().Equal(
                    "[line 1] Error at 's': Cannot assign null to an implicitly typed local variable"
                );
            }

            private static bool FatalWarningsHandler(CompilerWarning compilerWarning)
            {
                throw compilerWarning;
            }
        }

        public class MainWithCustomConsole
        {
            private readonly IPerlangConsole testConsole = new TestConsole();

            /// <summary>
            /// Gets the content printed to the standard output stream during test execution.
            ///
            /// This property returns the output as one, long string.
            /// </summary>
            private string StdoutContent => testConsole.Out.ToString() ?? String.Empty;

            /// <summary>
            /// Gets the content printed to the standard output stream during test execution.
            ///
            /// This property returns the output split by newline separators.
            /// </summary>
            private IEnumerable<string> StdoutLines => StdoutContent.Split(Environment.NewLine);

            /// <summary>
            /// Gets the content printed to the standard output stream during test execution.
            /// </summary>
            private string StderrContent => testConsole.Error.ToString() ?? String.Empty;

            public class WithPrintParameter
            {
                private readonly IPerlangConsole testConsole = new TestConsole();

                /// <summary>
                /// Gets the content printed to the standard output stream during test execution.
                ///
                /// This property returns the output as one, long string.
                /// </summary>
                private string StdoutContent => testConsole.Out.ToString() ?? String.Empty;

                /// <summary>
                /// Gets the content printed to the standard output stream during test execution.
                ///
                /// This property returns the output split by newline separators.
                /// </summary>
                private IEnumerable<string> StdoutLines => StdoutContent.Split(Environment.NewLine);

                [Fact]
                public void assignment_and_increment()
                {
                    CallWithPrintParameter("i = i + 1");

                    StdoutLines.Should().Equal(
                        "(i (+ i 1))", String.Empty
                    );
                }

                [Fact]
                public void addition_assignment()
                {
                    CallWithPrintParameter("i += 1");

                    StdoutLines.Should().Equal(
                        "(i (+= i 1))", String.Empty
                    );
                }

                [Fact]
                public void print_variable()
                {
                    CallWithPrintParameter("print hej");

                    StdoutLines.Should().Equal(
                        "(print hej)", String.Empty
                    );
                }

                // This was previously broken, before #161. The incomplete expression was not properly detected by the
                // interpreter.
                [Fact]
                public void invalid_expression()
                {
                    CallWithPrintParameter("hej hej");

                    StdoutLines.Should().Equal(
                        "[line 1] Error at 'hej': Expect ';' after expression.", String.Empty
                    );
                }

                private void CallWithPrintParameter(string script)
                {
                    Program.MainWithCustomConsole(new[] { "-p", script }, testConsole);
                }
            }

            [Fact]
            public void with_version_parameter_outputs_expected_value()
            {
                // Arrange & Act
                Program.MainWithCustomConsole(new[] { "--version" }, testConsole);

                // Assert
                StdoutLines.Should().Equal(
                    CommonConstants.InformationalVersion, String.Empty
                );
            }

            [Fact]
            public void with_eval_parameter_outputs_expected_value()
            {
                // Arrange & Act
                Program.MainWithCustomConsole(new[] { "-e", "print 10" }, testConsole);

                // Assert
                StdoutLines.Should().Equal(
                    "10", String.Empty
                );
            }

            [Fact]
            public void with_script_outputs_expected_value()
            {
                // Arrange & Act
                Program.MainWithCustomConsole(new[] { "test/fixtures/hello_world.per" }, testConsole);

                // Assert
                StdoutLines.Should().Equal(
                    "Hello, World", String.Empty
                );
            }

            [SkippableFact]
            public void with_script_and_script_argument_outputs_expected_value()
            {
                Skip.If(PerlangMode.ExperimentalCompilation, "Not supported in compiled mode");

                // Arrange & Act
                Program.MainWithCustomConsole(new[] { "test/fixtures/argv_pop.per", "foo" }, testConsole);

                // Assert
                StdoutLines.Should().Equal(
                    "foo", String.Empty
                );
            }

            [SkippableFact]
            public void with_script_and_no_argument_prints_expected_error()
            {
                Skip.If(PerlangMode.ExperimentalCompilation, "Not supported in compiled mode");

                // Arrange & Act
                Program.MainWithCustomConsole(new[] { "test/fixtures/argv_pop.per" }, testConsole);

                // Assert
                StdoutLines.Should().Equal(
                    "[line 1] No arguments left", String.Empty
                );
            }

            [Fact]
            public void with_invalid_script_throws_expected_exception()
            {
                // Arrange & Act
                Program.MainWithCustomConsole(new[] { "test/fixtures/invalid.per" }, testConsole);

                // Assert
                StdoutLines.Should().Equal(
                    "[line 3] Error at end: Expect ';' after value.", String.Empty
                );
            }

            [Fact]
            public void with_invalid_script_and_script_argument_returns_expected_exit_code()
            {
                // Arrange & Act
                int result = Program.MainWithCustomConsole(new[] { "test/fixtures/invalid.per", "foo" }, testConsole);

                // Assert
                result.Should().Be(
                    (int)Program.ExitCodes.ERROR
                );
            }

            [Fact]
            public void with_invalid_script_and_script_argument_prints_expected_error_message()
            {
                // Arrange & Act
                Program.MainWithCustomConsole(new[] { "test/fixtures/invalid.per", "foo" }, testConsole);

                // Assert
                StdoutContent.Should().Contain(
                    "Error at end: Expect ';' after value"
                );
            }

            // There used to be an exception in the default runtimeErrorHandler. This test would illustrate it.
            [Fact]
            public void ARGV_pop_expr_with_no_arguments_throws_the_expected_exception()
            {
                // Note that the trailing ; makes this a complete statement.
                Program.MainWithCustomConsole(new[] { "-e", "ARGV.pop()" }, testConsole);

                StdoutLines.Should().Equal(
                    "[line 1] No arguments left", String.Empty
                );
            }

            [Fact]
            public void ARGV_pop_stmt_with_no_arguments_throws_the_expected_exception()
            {
                // Note that the trailing ; makes this a complete statement.
                Program.MainWithCustomConsole(new[] { "-e", "ARGV.pop();" }, testConsole);

                StdoutLines.Should().Equal(
                    "[line 1] No arguments left", String.Empty
                );
            }

            [Fact(DisplayName = "with -Wno-error=null-usage parameter: emits warning on null assignment")]
            public void with_Wno_error_null_usage_parameter_emits_warning_on_null_assignment()
            {
                Program.MainWithCustomConsole(new[] { "-Wno-error=null-usage", "-e", "var s: string; s = null;" }, testConsole);

                StdoutContent.Should().Contain(
                    "Warning at 's': Null assignment detected"
                );

                StderrContent.Should().BeEmpty();
            }

            [Fact(DisplayName = "with -Wno-error=null-usage parameter: emits warning when initializing to null")]
            public void with_Wno_error_null_usage_parameter_emits_warning_when_initializing_to_null()
            {
                Program.MainWithCustomConsole(new[] { "-Wno-error=null-usage", "-e", "var s: string = null;" }, testConsole);

                StdoutContent.Should().Contain(
                    "Warning at 's': Initializing variable to null detected"
                );

                StderrContent.Should().BeEmpty();
            }

            [SkippableFact(DisplayName = "with -Wno-error=null-usage parameter: emits warning for usage of null")]
            public void with_no_error_null_usage_parameter_emits_warning_for_usage_of_null()
            {
                Skip.If(PerlangMode.ExperimentalCompilation, "Not supported in compiled mode");

                int exitCode = Program.MainWithCustomConsole(new[] { "-Wno-error", "null-usage", "test/fixtures/null_usage.per" }, testConsole);

                StderrContent.Should().BeEmpty();

                StdoutContent.Should().Contain(
                    "Initializing variable to null detected"
                );

                exitCode.Should().Be(0);
            }

            [SkippableFact]
            public void with_no_error_null_usage_parameter_includes_correct_line_numbers_in_errors()
            {
                Skip.If(PerlangMode.ExperimentalCompilation, "Not supported in compiled mode");

                int exitCode = Program.MainWithCustomConsole(new[] { "-Wno-error", "null-usage", "test/fixtures/defining-and-calling-a-function-with-null-parameter.per" }, testConsole);

                StderrContent.Should().BeEmpty();

                // There used to be a bug with the reporting of both of these warnings and errors, where the line number
                // was messed up (#276)
                StdoutLines.Should().Contain(
                        "[line 6] Warning at 'greet': Null parameter detected for 'name'")
                    .And.Contain(
                        "[line 2] Operands must be numbers, not AsciiString and null"
                    );

                exitCode.Should().Be((int)Program.ExitCodes.RUNTIME_ERROR);
            }

            [SkippableFact(DisplayName = "with -Wno-error=null-usage parameter: can be combined with script argument")]
            public void with_no_error_null_usage_parameter_can_be_combined_with_script_argument()
            {
                Skip.If(PerlangMode.ExperimentalCompilation, "Not supported in compiled mode");

                int exitCode = Program.MainWithCustomConsole(new[] { "-Wno-error", "null-usage", "test/fixtures/argv_pop.per", "hello, world" }, testConsole);

                StderrContent.Should().BeEmpty();

                StdoutContent.Should().Contain(
                    "hello, world"
                );

                exitCode.Should().Be(0);
            }

            [Fact]
            public void without_parameter_throws_error_on_null_assignment()
            {
                Program.MainWithCustomConsole(new[] { "-e", "var s: string; s = null; print 10;" }, testConsole);

                StdoutContent.Should().Contain(
                    "Error at 's': Null assignment detected"
                );

                // This test ensures that the 'print 10' never gets executed. When a compiler _error_ occurs, the
                // program should not be executed.
                StdoutContent.Should().NotContain("10");

                StderrContent.Should().BeEmpty();
            }

            [Fact]
            public void without_parameter_throws_error_when_initializing_to_null()
            {
                Program.MainWithCustomConsole(new[] { "-e", "var s: string = null; print 10;" }, testConsole);

                StdoutContent.Should().Contain(
                    "Error at 's': Initializing variable to null detected"
                );

                // This test ensures that the 'print 10' never gets executed. When a compiler _error_ occurs, program
                // execution should be aborted.
                StdoutContent.Should().NotContain("10");

                StderrContent.Should().BeEmpty();
            }
        }
    }
}
