using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Perlang.Interpreter;
using Perlang.Parser;

namespace Perlang.ConsoleApp
{
    public class Program
    {
        private readonly PerlangInterpreter interpreter;

        private static readonly HashSet<string> ReplCommands = new HashSet<string>
        {
            "quit"
        };

        private static bool hadError;
        private static bool hadRuntimeError;

        public static void Main(string[] args)
        {
//            if (args.Length > 1)
//            {
//                Console.WriteLine("Usage: perlang [script] [arg]...");
//                Console.WriteLine("If extra arguments are provided, they will be passed on to the script you are executing");
//
//                Environment.Exit(64);
//            }
            if (args.Length == 0)
            {
                new Program().RunPrompt();
            }
            else if (args.Length == 1)
            {
                new Program().RunFile(args[0]);
            }
            else // More than 1 argument
            {
                new Program(args[1..]).RunFile(args[0]);

            }
        }

        private Program(params string[] arguments)
        {
            interpreter = new PerlangInterpreter(RuntimeError, arguments: arguments);
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
            ReadLine.HistoryEnabled = true;
            ReadLine.AutoCompletionHandler = new AutoCompletionHandler();

            for (;;)
            {
                string command = ReadLine.Read("> ");

                if (command == "quit")
                {
                    break;
                }

                Run(command);
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
            Console.WriteLine($"[line {error.Token.Line}] {error.Message}");
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

        private void ParseError(ParseError parseError)
        {
            if (parseError.ParseErrorType == ParseErrorType.MISSING_TRAILING_SEMICOLON)
            {
                // These errors are ignored; we will get them all them when we try to parse expressions as
                // statements.
                hadError = true;
                return;
            }

            Error(parseError.Token, parseError.Message);
        }

        private static void ResolveError(ResolveError resolveError)
        {
            Error(resolveError.Token, resolveError.Message);
        }

        private class AutoCompletionHandler : IAutoCompleteHandler
        {
            public string[] GetSuggestions(string text, int index)
            {
                var matchingKeywords = Scanner.ReservedKeywords
                    .Where(keyword => keyword.Key.StartsWith(text))
                    .Select(keyword => keyword.Key)
                    .ToList();

                matchingKeywords.AddRange(
                    ReplCommands.Where(command => command.StartsWith(text))
                );

                return matchingKeywords.ToArray();
            }

            // characters to start completion from
            public char[] Separators { get; set; } = { ' ', '.', '/' };
        }
    }
}
