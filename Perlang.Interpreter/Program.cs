using System;
using System.IO;
using System.Text;
using Perlang.Parser;

namespace Perlang.Interpreter
{
    public class Program : IScannerErrorHandler, IResolveErrorHandler, IParseErrorHandler, IRuntimeErrorHandler
    {
        private readonly PerlangInterpreter interpreter;

        private static bool hadError;
        private static bool hadRuntimeError;

        public static void Main(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Usage: perlang [script]");
                Environment.Exit(64);
            }
            else if (args.Length == 1)
            {
                new Program().RunFile(args[0]);
            }
            else
            {
                new Program().RunPrompt();
            }
        }

        private Program()
        {
            interpreter = new PerlangInterpreter(this);
        }

        private void RunFile(string path)
        {
            var bytes = File.ReadAllBytes(path);
            Run(Encoding.UTF8.GetString(bytes));

            // Indicate an error in the exit code.
            if (hadError)
            {
                Environment.Exit(65);
            }

            if (hadRuntimeError)
            {
                Environment.Exit(70);
            }
        }
        
        private void RunPrompt()
        {
            for (;;)
            {
                Console.Write("> ");
                Run(Console.ReadLine());
                hadError = false;
            }
        }
        
        private void Run(string source)
        {
            if (String.IsNullOrWhiteSpace(source))
            {
                return;
            }

            var scanner = new Scanner(source, this);

            var tokens = scanner.ScanTokens();

            // For now, just print the tokens.
            var parser = new PerlangParser(tokens, this);
            var statements = parser.ParseStatements();

            // Stop if there was a syntax error.
            if (!hadError)
            {
                var resolver = new Resolver(interpreter, this);
                resolver.Resolve(statements);

                // Stop if there was a resolution error.
                if (hadError) return;

                interpreter.Interpret(statements);
            }
            else
            {
                // This was not a valid set of statements. But is it perhaps a valid expression? The parser is now
                // at EOF and since we don't currently have any form of "rewind" functionality, the easiest approach
                // is to just create a new parser at this point.
                parser = new PerlangParser(tokens, this);
                Expr expression = parser.ParseExpression();

                if (expression == null)
                {
                    // Likely not a valid expression. Errors are presumed to have been handled at this point, so we
                    // can just return.
                    return;
                }

                // TODO: we don't run the resolver in this case, which essentially means that we will be unable to
                // TODO: refer to local variables.
                object result = interpreter.Evaluate(expression);

                if (result != null)
                {
                    Console.WriteLine(result);
                }
            }
        }

        public void ScannerError(int line, string message)
        {
            Report(line, "", message);
        }

        public void RuntimeError(RuntimeError error)
        {
            Console.WriteLine($"{error.Message}\n" +
                              $"[line {error.Token.Line}]");
            hadRuntimeError = true;
        }

        private static void Report(int line, string where, string message)
        {
            Console.Error.WriteLine($"[line {line}] Error{where}: {message}");
            hadError = true;
        }

        private static void Error(Token token, string message)
        {
            if (token.Type == TokenType.EOF)
            {
                Report(token.Line, " at end", message);
            }
            else
            {
                Report(token.Line, " at '" + token.Lexeme + "'", message);
            }
        }

        public void ParseError(Token token, string message, ParseErrorType? parseErrorType)
        {
            if (parseErrorType == ParseErrorType.MISSING_TRAILING_SEMICOLON)
            {
                // These errors are ignored; we will get them all them when we try to parse expressions as
                // statements.
                hadError = true;
                return;
            }

            Error(token, message);
        }

        public void ResolveError(Token token, string message)
        {
            Error(token, message);
        }
    }
}