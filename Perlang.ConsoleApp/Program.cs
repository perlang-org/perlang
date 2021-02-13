using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Perlang.Interpreter;
using Perlang.Interpreter.Resolution;
using Perlang.Parser;
using static System.CommandLine.Parsing.TokenType;
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
            return MainWithCustomConsole(args, console: null);
        }

        /// <summary>
        /// Entry point for `perlang`, with the possibility to override the console streams (stdin, stdout, stderr).
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <param name="console">A custom `IConsole` implementation to use. May be null, in which case the standard
        /// streams of the calling process will be used.</param>
        /// <returns>Zero if the program executed successfully; non-zero otherwise.</returns>
        public static int MainWithCustomConsole(string[] args, IConsole console)
        {
            var versionOption = new Option(new[] { "--version", "-v" }, "Show version information");
            var detailedVersionOption = new Option("-V", "Show detailed version information");
            var evalOption = new Option("-e", "Executes a single-line script");

            var rootCommand = new RootCommand
            {
                Description = "The Perlang Interpreter",

                Handler = CommandHandler.Create((ParseResult parseResult, IConsole console) =>
                {
                    if (parseResult.HasOption(versionOption))
                    {
                        console.Out.WriteLine(CommonConstants.InformationalVersion);
                        return Task.FromResult(0);
                    }

                    if (parseResult.HasOption(detailedVersionOption))
                    {
                        console.Out.WriteLine($"Perlang {CommonConstants.InformationalVersion} running on .NET {Environment.Version}");
                        console.Out.WriteLine();
                        console.Out.WriteLine($"  Number of detected (v)CPUs: {Environment.ProcessorCount}");
                        console.Out.WriteLine($"  Running in 64-bit mode: {Environment.Is64BitProcess}");
                        console.Out.WriteLine($"  Operating system info: {Environment.OSVersion.VersionString}");
                        console.Out.WriteLine();

                        return Task.FromResult(0);
                    }

                    if (parseResult.HasOption(evalOption))
                    {
                        IEnumerable<string> arguments = parseResult.Tokens
                            .Where(t => t.Type == System.CommandLine.Parsing.TokenType.Argument)
                            .Select(t => t.Value);

                        new Program(standardOutputHandler: console.Out.WriteLine).Run(String.Join(" ", arguments));
                    }
                    else if (parseResult.Tokens.Count == 0)
                    {
                        new Program(standardOutputHandler: console.Out.WriteLine).RunPrompt();
                    }
                    else
                    {
                        string scriptName = parseResult.Tokens[0].Value;

                        if (parseResult.Tokens.Count == 1)
                        {
                            new Program(standardOutputHandler: console.Out.WriteLine).RunFile(scriptName);
                        }
                        else
                        {
                            // More than 1 argument. The remaining arguments are passed to the program, which can use
                            // argv_pop() to retrieve them.
                            var remainingArguments = parseResult.Tokens.Skip(1)
                                .Take(parseResult.Tokens.Count - 1)
                                .Select(r => r.Value);

                            new Program(remainingArguments, console.Out.WriteLine).RunFile(scriptName);
                        }
                    }

                    return Task.FromResult(0);
                })
            };

            rootCommand.AddArgument(new Argument
            {
                Arity = ArgumentArity.ZeroOrMore
            });

            rootCommand.AddOption(versionOption);
            rootCommand.AddOption(detailedVersionOption);
            rootCommand.AddOption(evalOption);

            return new CommandLineBuilder(rootCommand)
                .UseDefaults()
                .Build()
                .Invoke(args, console);
        }

        internal Program(
            IEnumerable<string> arguments = null,
            Action<string> standardOutputHandler = null,
            Action<RuntimeError> runtimeErrorHandler = null)
        {
            this.standardOutputHandler = standardOutputHandler ?? Console.WriteLine;

            // TODO: Make it possible to override this at some point, so the caller can separate between these types of output.
            this.standardErrorHandler = standardOutputHandler ?? Console.Error.WriteLine;

            interpreter = new PerlangInterpreter(
                runtimeErrorHandler ?? RuntimeError,
                this.standardOutputHandler,
                arguments ?? new List<string>(),
                replMode: true
            );
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
            object result = interpreter.Eval(source, ScanError, ParseError, ResolveError, ValidationError, ValidationError);

            if (result != null)
            {
                standardOutputHandler(result.ToString());
            }
        }

        private void PrintBanner()
        {
            standardOutputHandler($"Perlang Interactive REPL Console ({CommonConstants.GetFullVersion()}, built from {CommonConstants.GitRevision})");
        }

        private void ScanError(ScanError scanError)
        {
            Report(scanError.Line, String.Empty, scanError.Message);
        }

        private void RuntimeError(RuntimeError error)
        {
            string line = error.Token?.Line.ToString() ?? "unknown";

            standardOutputHandler($"[line {line}] {error.Message}");
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

        private void ValidationError(ValidationError validationError)
        {
            Error(validationError.Token, validationError.Message);
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
