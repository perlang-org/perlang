using System.Collections.Generic;
using Perlang.ConsoleApp;
using Xunit;

namespace Perlang.Tests.ConsoleApp
{
    public class ProgramTest
    {
        private readonly Program subject;
        private readonly List<string> output = new List<string>();

        public ProgramTest()
        {
            subject = new Program(standardOutputHandler: s => output.Add(s), runtimeErrorHandler: e => throw e);
        }

        [Fact]
        public void Run_can_run_multiple_statements_separated_by_semicolon()
        {
            subject.Run("var a = 42; print a;");

            Assert.Equal(new List<string> { "42" }, output);
        }

        [Fact]
        public void Run_state_persists_between_invocations()
        {
            subject.Run("var a = 42;");
            subject.Run("print a;");

            Assert.Equal(new List<string> { "42" }, output);
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
        public void Run_last_variable_defined_with_a_given_name_takes_precedence()
        {
            // This is definitely a bug. The second statement should cause an error, since a global variable with the
            // same name already exists. For now, this test will remember us that these are the currently expected
            // semantics.
            subject.Run("var a = 42;");
            var exception = Assert.Throws<RuntimeError>(() => subject.Run("var a = 44;"));
            Assert.Matches("Variable with this name already declared in this scope", exception.Message);
        }
    }
}
