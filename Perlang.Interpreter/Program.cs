using System;
using System.IO;
using System.Text;
using Perlang.Parser;

namespace Perlang.Interpreter
{
    public class Program
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
            interpreter = new PerlangInterpreter(RuntimeError);
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
            PrintBanner();

            for (;;)
            {
                Console.Write("> ");
                Run(Console.ReadLine());
                hadError = false;
            }
        }

        private void Run(string source)
        {
            var result = interpreter.Eval(source, ScanError, ParseError, ResolveError);

            if (result != null)
            {
                Console.WriteLine(result);
            }
        }

        private static void PrintBanner()
        {
            Console.WriteLine($"Perlang Interactive REPL Console ({CommonConstants.Version})");
        }

        private static void ScanError(ScanError scanError)
        {
            Report(scanError.Line, "", scanError.Message);
        }

        private static void RuntimeError(RuntimeError error)
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

        private static void ResolveError(Token token, string message)
        {
            Error(token, message);
        }
    }
}