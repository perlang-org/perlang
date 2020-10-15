using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Text;
using Perlang.Interpreter;
using Perlang.Interpreter.Resolution;
using Perlang.Interpreter.Typing;
using Perlang.Parser;
using ParseError = Perlang.Parser.ParseError;

namespace Perlang.ConsoleApp
{
    public class Program
    {
        private static readonly ISet<string> ReplCommands = new HashSet<string>
        {
            "quit"
        };

        private readonly PerlangInterpreter interpreter;
        private readonly Action<string> standardOutputHandler;
        private readonly Action<string> standardErrorHandler;

        private bool hadError;
        private bool hadRuntimeError;

        /// <summary>
        /// Entry point for the `perlang` binary.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <returns>Zero if the program executed successfully; non-zero otherwise.</returns>
        public static int Main(string[] args)
        {
            var rootCommand = new RootCommand
            {
                Description = "The Perlang Interpreter",

                Handler = CommandHandler.Create((ParseResult parseResult, IConsole console) =>
                {
                    if (parseResult.Tokens.Count == 0)
                    {
                        new Program().RunPrompt();
                    }
                    else
                    {
                        string scriptName = parseResult.Tokens[0].Value;

                        if (parseResult.Tokens.Count == 1)
                        {
                            new Program().RunFile(scriptName);
                        }
                        else
                        {
                            // More than 1 argument. The remaining arguments are passed to the program, which can use
                            // argv_pop() to retrieve them.
                            var remainingArguments = parseResult.Tokens.Skip(1)
                                .Take(parseResult.Tokens.Count - 1)
                                .Select(r => r.Value);

                            new Program(remainingArguments).RunFile(scriptName);
                        }
                    }
                })
            };

            rootCommand.AddArgument(new Argument
            {
                Arity = ArgumentArity.ZeroOrMore
            });

            // Parse the incoming args and invoke the handler
            return rootCommand.Invoke(args);
        }

        internal Program(
            IEnumerable<string> arguments = null,
            Action<string> standardOutputHandler = null,
            Action<RuntimeError> runtimeErrorHandler = null)
        {
            this.standardOutputHandler = standardOutputHandler ?? Console.WriteLine;

            // TODO: Make it possible to override this at some point, so the caller can separate between these types of output.
            this.standardErrorHandler = standardOutputHandler ?? Console.Error.WriteLine;

            interpreter = new PerlangInterpreter(runtimeErrorHandler: runtimeErrorHandler ?? RuntimeError, this.standardOutputHandler, arguments ?? new List<string>());
        }

        private void RunFile(string path)
        {
            if (!File.Exists(path))
            {
                Console.Error.WriteLine($"Error: File {path} not found");
                Environment.Exit(65);
            }

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

            while (true)
            {
                string command = ReadLine.Read("> ");

                if (command == "quit")
                {
                    break;
                }

                Run(command);
            }
        }

        internal void Run(string source)
        {
            object result = interpreter.Eval(source, ScanError, ParseError, ResolveError, TypeValidationError);

            if (result != null)
            {
                standardOutputHandler(result.ToString());
            }
        }

        private void PrintBanner()
        {
            standardOutputHandler($"Perlang Interactive REPL Console ({CommonConstants.GetFullVersion()})");
        }

        private void ScanError(ScanError scanError)
        {
            Report(scanError.Line, String.Empty, scanError.Message);
        }

        private void RuntimeError(RuntimeError error)
        {
            standardOutputHandler($"[line {error.Token.Line}] {error.Message}");
            hadRuntimeError = true;
        }

        private void Report(int line, string where, string message)
        {
            standardErrorHandler($"[line {line}] Error{where}: {message}");
            hadError = true;
        }

        private void Error(Token token, string message)
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

        private void ResolveError(ResolveError resolveError)
        {
            Error(resolveError.Token, resolveError.Message);
        }

        private void TypeValidationError(TypeValidationError typeValidationError)
        {
            Error(typeValidationError.Token, typeValidationError.Message);
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
