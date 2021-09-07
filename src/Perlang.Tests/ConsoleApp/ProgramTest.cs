#pragma warning disable S3626

using System;
using System.Collections.Generic;
using System.CommandLine.IO;
using System.Linq;
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
                    standardOutputHandler: s => output.Add(s),
                    runtimeErrorHandler: e => throw e,
                    disabledWarningsAsErrors: Enumerable.Empty<WarningType>()
                );
            }

            [Fact]
            public void supports_multiple_statements_separated_by_semicolon()
            {
                subject.Run("var a = 42; print a;", FatalWarningsHandler);

                Assert.Equal(new List<string> { "42" }, output);
            }

            [Fact]
            public void supports_final_semicolon_elision_single_statement()
            {
                subject.Run("print 10", FatalWarningsHandler);

                Assert.Equal(new List<string> { "10" }, output);
            }

            [Fact]
            public void supports_final_semicolon_elision_multiple_statements()
            {
                subject.Run("var a = 43; print a", FatalWarningsHandler);

                Assert.Equal(new List<string> { "43" }, output);
            }

            [Fact]
            public void state_persists_between_invocations()
            {
                subject.Run("var a = 44;", FatalWarningsHandler);
                subject.Run("print a;", FatalWarningsHandler);

                Assert.Equal(new List<string> { "44" }, output);
            }

            [Fact]
            public void can_call_function_from_statement()
            {
                subject.Run("fun hello(): void { print 1; }", FatalWarningsHandler);
                subject.Run("hello();", FatalWarningsHandler);

                Assert.Equal(new List<string> { "1" }, output);
            }

            // This illustrates the bug described in #124; the example there used to throw an exception like this:
            // [line 1] Error at 'hello': Attempting to call undefined function 'hello'
            [Fact]
            public void can_call_function_from_expression()
            {
                subject.Run("fun hello(): void { print 1; }", FatalWarningsHandler);
                subject.Run("hello()", FatalWarningsHandler);

                Assert.Equal(new List<string> { "1" }, output);
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

                Assert.Equal(3, output.Count);
                Assert.Matches("Undefined identifier 'x'", output[0]);
                Assert.Matches("Undefined identifier 'b'", output[1]);
                Assert.Matches("Undefined identifier 'c'", output[2]);
            }

            [Fact]
            public void variable_redefined_throws_expected_error()
            {
                // Act
                subject.Run("var a = 42;", FatalWarningsHandler);

                // Assert
                var exception = Assert.Throws<RuntimeError>(() => subject.Run("var a = 44;", FatalWarningsHandler));
                Assert.Matches("Variable with this name already declared in this scope", exception.Message);
            }

            // Test added to assert the bug fix for #117. Interestingly enough, the NRE described there did not occur when

            // the test was placed in the ArgvTests class.

            [Fact]
            public void Time_now_tickz_fails_with_expected_exception()
            {
                subject.Run("Time.now().tickz()", FatalWarningsHandler);

                Assert.Equal(new List<string>
                {
                    "[line 1] Error at 'tickz': Failed to locate method 'tickz' in class 'DateTime'"
                }, output);
            }

            [Fact]
            public void local_variable_inference_from_null_throws_the_expected_exception()
            {
                subject.Run("var s = null;", FatalWarningsHandler);

                Assert.Equal(new List<string>
                {
                    "[line 1] Error at 's': Cannot assign null to an implicitly typed local variable"
                }, output);
            }

            private static bool FatalWarningsHandler(CompilerWarning compilerWarning)
            {
                throw compilerWarning;
            }
        }

        public class MainWithCustomConsole
        {
            private readonly TestConsole testConsole = new();

            /// <summary>
            /// Gets the content printed to the standard output stream during test execution.
            /// </summary>
            private string StdoutContent => testConsole.Out.ToString() ?? String.Empty;

            /// <summary>
            /// Gets the content printed to the standard output stream during test execution.
            /// </summary>
            private string StderrContent => testConsole.Error.ToString() ?? String.Empty;

            public class WithPrintParameter
            {
                private readonly TestConsole testConsole = new();

                /// <summary>
                /// Gets the content printed to the standard output stream during test execution.
                /// </summary>
                private string StdoutContent => testConsole.Out.ToString() ?? String.Empty;

                [Fact]
                public void assignment_and_increment()
                {
                    CallWithPrintParameter("i = i + 1");

                    Assert.Equal("(i (+ i 1))\n", StdoutContent);
                }

                [Fact]
                public void addition_assignment()
                {
                    CallWithPrintParameter("i += 1");

                    Assert.Equal("(i (+= i 1))\n", StdoutContent);
                }

                [Fact]
                public void print_variable()
                {
                    CallWithPrintParameter("print hej");

                    Assert.Equal("(print hej)\n", StdoutContent);
                }

                // This was previously broken, before #161. The incomplete expression was not properly detected by the
                // interpreter.
                [Fact]
                public void invalid_expression()
                {
                    CallWithPrintParameter("hej hej");

                    Assert.Contains("Error at 'hej': Expect ';' after expression.", StdoutContent);
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
                Assert.Equal(CommonConstants.InformationalVersion + "\n", StdoutContent);
            }

            [Fact]
            public void with_eval_parameter_outputs_expected_value()
            {
                // Arrange & Act
                Program.MainWithCustomConsole(new[] { "-e", "print 10" }, testConsole);

                // Assert
                Assert.Equal("10" + "\n", StdoutContent);
            }

            [Fact]
            public void with_script_outputs_expected_value()
            {
                // Arrange & Act
                Program.MainWithCustomConsole(new[] { "test/fixtures/hello_world.per" }, testConsole);

                // Assert
                Assert.Equal("Hello, World\n", StdoutContent);
            }

            [Fact]
            public void with_script_and_script_argument_outputs_expected_value()
            {
                // Arrange & Act
                Program.MainWithCustomConsole(new[] { "test/fixtures/argv_pop.per", "foo" }, testConsole);

                // Assert
                Assert.Equal("foo\n", StdoutContent);
            }

            [Fact]
            public void with_script_and_no_argument_prints_expected_error()
            {
                // Arrange & Act
                Program.MainWithCustomConsole(new[] { "test/fixtures/argv_pop.per" }, testConsole);

                // Assert
                Assert.Equal("[line 1] No arguments left\n", StdoutContent);
            }

            [Fact]
            public void with_invalid_script_throws_expected_exception()
            {
                // Arrange & Act
                Program.MainWithCustomConsole(new[] { "test/fixtures/invalid.per" }, testConsole);

                // Assert
                Assert.Contains("Error at end: Expect ';' after value", StdoutContent);
            }

            [Fact]
            public void with_invalid_script_and_script_argument_returns_expected_exit_code()
            {
                // Arrange & Act
                int result = Program.MainWithCustomConsole(new[] { "test/fixtures/invalid.per", "foo" }, testConsole);

                // Assert
                Assert.Equal((int)Program.ExitCodes.ERROR, result);
            }

            [Fact]
            public void with_invalid_script_and_script_argument_prints_expected_error_message()
            {
                // Arrange & Act
                Program.MainWithCustomConsole(new[] { "test/fixtures/invalid.per", "foo" }, testConsole);

                // Assert
                Assert.Contains("Error at end: Expect ';' after value", StdoutContent);
            }

            // There used to be an exception in the default runtimeErrorHandler. This test would illustrate it.
            [Fact]
            public void ARGV_pop_expr_with_no_arguments_throws_the_expected_exception()
            {
                // Note that the trailing ; makes this a complete statement.
                Program.MainWithCustomConsole(new[] { "-e", "ARGV.pop()" }, testConsole);

                Assert.Equal("[line 1] No arguments left\n", StdoutContent);
            }

            [Fact]
            public void ARGV_pop_stmt_with_no_arguments_throws_the_expected_exception()
            {
                // Note that the trailing ; makes this a complete statement.
                Program.MainWithCustomConsole(new[] { "-e", "ARGV.pop();" }, testConsole);

                Assert.Equal("[line 1] No arguments left\n", StdoutContent);
            }

            [Fact(DisplayName = "with -Wno-error=list parameter: lists all warnings")]
            public void with_Wno_error_list_parameter_lists_all_warnings()
            {
                Program.MainWithCustomConsole(new[] { "-Wno-error=list" }, testConsole);

                Assert.Contains("null-usage: Warns on usage of null", StdoutContent);
                Assert.Equal(String.Empty, StderrContent);
            }

            [Fact(DisplayName = "with -Wno-error=null-usage parameter: emits no warning on null assignment")]
            public void with_Wno_error_null_usage_parameter_emits_warning_on_null_assignment()
            {
                Program.MainWithCustomConsole(new[] { "-Wno-error=null-usage", "-e", "var s: string; s = null;" }, testConsole);

                Assert.Contains("Warning at 's': Null assignment detected", StdoutContent);
                Assert.Equal(String.Empty, StderrContent);
            }

            [Fact(DisplayName = "with -Wno-error=null-usage parameter: emits no warning when initializing to null")]
            public void with_Wno_error_null_usage_parameter_emits_warning_when_initializing_to_null()
            {
                Program.MainWithCustomConsole(new[] { "-Wno-error=null-usage", "-e", "var s: string = null;" }, testConsole);

                Assert.Contains("Warning at 's': Initializing variable to null detected", StdoutContent);
                Assert.Equal(String.Empty, StderrContent);
            }

            [Fact]
            public void without_parameter_throws_error_on_null_assignment()
            {
                Program.MainWithCustomConsole(new[] { "-e", "var s: string; s = null; print 10;" }, testConsole);

                Assert.Contains("Error at 's': Null assignment detected", StdoutContent);

                // This test ensures that the 'print 10' never gets executed. When a compiler _error_ occurs, the
                // program should not be executed.
                Assert.DoesNotContain("10", StdoutContent);

                Assert.Equal(String.Empty, StderrContent);
            }

            [Fact]
            public void without_parameter_throws_error_when_initializing_to_null()
            {
                Program.MainWithCustomConsole(new[] { "-e", "var s: string = null; print 10;" }, testConsole);

                Assert.Contains("Error at 's': Initializing variable to null detected", StdoutContent);

                // This test ensures that the 'print 10' never gets executed. When a compiler _error_ occurs, program
                // execution should be aborted.
                Assert.DoesNotContain("10", StdoutContent);

                Assert.Equal(String.Empty, StderrContent);
            }
        }
    }
}
