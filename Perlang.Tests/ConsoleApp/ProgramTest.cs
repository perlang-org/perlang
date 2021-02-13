#pragma warning disable S3626

using System.Collections.Generic;
using System.CommandLine.IO;
using Perlang.ConsoleApp;
using Xunit;

namespace Perlang.Tests.ConsoleApp
{
    /// <summary>
    /// Test for the Program class. This essentially lets us test the REPL, or various aspects of the language which
    /// behaves differently in REPL mode vs regular interpreted/compiled modes.
    /// </summary>
    public class ProgramTest
    {
        private readonly Program subject;
        private readonly List<string> output = new();

        public ProgramTest()
        {
            subject = new Program(standardOutputHandler: s => output.Add(s), runtimeErrorHandler: e => throw e);
        }

        [Fact]
        public void Run_supports_multiple_statements_separated_by_semicolon()
        {
            subject.Run("var a = 42; print a;");

            Assert.Equal(new List<string> { "42" }, output);
        }

        [Fact]
        public void Run_supports_final_semicolon_elision_single_statement()
        {
            subject.Run("print 10");

            Assert.Equal(new List<string> { "10" }, output);
        }

        [Fact]
        public void Run_supports_final_semicolon_elision_multiple_statements()
        {
            subject.Run("var a = 43; print a");

            Assert.Equal(new List<string> { "43" }, output);
        }

        [Fact]
        public void Run_state_persists_between_invocations()
        {
            subject.Run("var a = 44;");
            subject.Run("print a;");

            Assert.Equal(new List<string> { "44" }, output);
        }

        [Fact]
        public void Run_can_call_function_from_statement()
        {
            subject.Run("fun hello(): void { print 1; }");
            subject.Run("hello();");

            Assert.Equal(new List<string> { "1" }, output);
        }

        // This illustrates the bug described in #124; the example there used to throw an exception like this:
        // [line 1] Error at 'hello': Attempting to call undefined function 'hello'
        [Fact]
        public void Run_can_call_function_from_expression()
        {
            subject.Run("fun hello(): void { print 1; }");
            subject.Run("hello()");

            Assert.Equal(new List<string> { "1" }, output);
        }

        [Fact]
        public void Run_state_does_not_persist_if_one_statement_is_invalid()
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
            subject.Run("var a = 42;");
            subject.Run("var b = 43; x; var c = 44;");
            subject.Run("print b;");
            subject.Run("print c;");

            Assert.Equal(3, output.Count);
            Assert.Matches("Undefined identifier 'x'", output[0]);
            Assert.Matches("Undefined identifier 'b'", output[1]);
            Assert.Matches("Undefined identifier 'c'", output[2]);
        }

        [Fact]
        public void Run_variable_redefined_throws_expected_error()
        {
            // Act
            subject.Run("var a = 42;");

            // Assert
            var exception = Assert.Throws<RuntimeError>(() => subject.Run("var a = 44;"));
            Assert.Matches("Variable with this name already declared in this scope", exception.Message);
        }

        [Fact]
        public void MainWithCustomConsole_with_version_parameter_outputs_expected_value()
        {
            // Arrange & Act
            var testConsole = new TestConsole();
            Program.MainWithCustomConsole(new[] { "--version" }, testConsole);

            // Assert
            Assert.Equal(CommonConstants.InformationalVersion + "\n", testConsole.Out.ToString());
        }

        [Fact]
        public void MainWithCustomConsole_with_eval_parameter_outputs_expected_value()
        {
            // Arrange & Act
            var testConsole = new TestConsole();
            Program.MainWithCustomConsole(new[] { "-e", "print", "10" }, testConsole);

            // Assert
            Assert.Equal("10" + "\n", testConsole.Out.ToString());
        }

        // Test added to assert the bug fix for #117. Interestingly enough, the NRE described there did not occur when
        // the test was placed in the ArgvTests class.
        [Fact]
        public void Run_Time_now_tickz_fails_with_expected_exception()
        {
            subject.Run("Time.now().tickz()");

            Assert.Equal(new List<string>
            {
                "[line 1] Error at 'tickz': Failed to locate method 'tickz' in class 'DateTime'"
            }, output);
        }

        // There used to be an exception in the default runtimeErrorHandler. This test would illustrate it.
        [Fact]
        public void Run_ARGV_pop_with_no_arguments_throws_the_expected_exception()
        {
            // Cannot use 'subject' here since we need it instantiated with different parameters to provoke this exact
            // error.
            var program = new Program(standardOutputHandler: s => output.Add(s));
            program.Run("ARGV.pop()");

            Assert.Equal(new List<string>
            {
                "[line 1] No arguments left"
            }, output);
        }
    }
}
