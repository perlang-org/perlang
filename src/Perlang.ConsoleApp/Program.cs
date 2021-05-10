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
using ParseError = Perlang.Parser.ParseError;

namespace Perlang.ConsoleApp
{
    public class Program
    {
        private static readonly ISet<string> ReplCommands = new HashSet<string>
        {
            "quit"
        };

        internal enum ExitCodes
        {
            /// <summary>
            /// The program executed successfully.
            /// </summary>
            SUCCESS = 0,

            // The convention to start error codes at 64 originates from /usr/include/sysexits.h in the Berkeley (BSD)
            // system, anno 1993. :-)

            /// <summary>
            /// An attempt was made to execute a script that does not exist.
            /// </summary>
            FILE_NOT_FOUND = 64,

            /// <summary>
            /// A scanner, parser or type validation error.
            /// </summary>
            ERROR = 65,

            /// <summary>
            /// Any kind of runtime exception was thrown when running the user-provided program.
            /// </summary>
            RUNTIME_ERROR = 66,

            /// <summary>
            /// One or more command line argument provided had an invalid value.
            /// </summary>
            INVALID_ARGUMENT = 67
        }

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
            var evalOption = new Option<string>("-e", "Executes a single-line script") { AllowMultipleArgumentsPerToken = false, ArgumentHelpName = "script" };
            var printOption = new Option<string>("-p", "Parse a single-line script and output a human-readable version of the AST") { ArgumentHelpName = "script" };

            // Note: options must be present in this list to be valid for the RootCommand.
            var options = new[]
            {
                versionOption,
                detailedVersionOption,
                evalOption,
                printOption
            };

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
                        // TODO: Workaround until we have a command-line-api package with https://github.com/dotnet/command-line-api/pull/1271 included.
                        OptionResult optionResult = parseResult.FindResultFor(evalOption);
                        string source = optionResult.Children
                            .Where(c => c.Symbol.Name == evalOption.ArgumentHelpName)
                            .Cast<ArgumentResult>()
                            .First()
                            .GetValueOrDefault<string>();

                        var program = new Program(
                            replMode: true,
                            standardOutputHandler: console.Out.WriteLine
                        );

                        int result = program.Run(source);

                        return Task.FromResult(result);
                    }
                    else if (parseResult.HasOption(printOption))
                    {
                        // TODO: Workaround until we have a command-line-api package with https://github.com/dotnet/command-line-api/pull/1271 included.
                        OptionResult optionResult = parseResult.FindResultFor(printOption);
                        string source = optionResult.Children
                            .Where(c => c.Symbol.Name == evalOption.ArgumentHelpName)
                            .Cast<ArgumentResult>()
                            .First()
                            .GetValueOrDefault<string>();

                        new Program(
                            replMode: true,
                            standardOutputHandler: console.Out.WriteLine
                        ).ParseAndPrint(source);

                        return Task.FromResult(0);
                    }
                    else if (parseResult.Tokens.Count == 0)
                    {
                        new Program(
                            replMode: true,
                            standardOutputHandler: console.Out.WriteLine
                        ).RunPrompt();

                        return Task.FromResult(0);
                    }
                    else
                    {
                        string scriptName = parseResult.Tokens[0].Value;
                        int result;

                        if (parseResult.Tokens.Count == 1)
                        {
                            var program = new Program(
                                replMode: false,
                                standardOutputHandler: console.Out.WriteLine
                            );

                            result = program.RunFile(scriptName);
                        }
                        else
                        {
                            // More than 1 argument. The remaining arguments are passed to the program, which can use
                            // ARGV.pop() to retrieve them.
                            var remainingArguments = parseResult.Tokens.Skip(1)
                                .Take(parseResult.Tokens.Count - 1)
                                .Select(r => r.Value);

                            var program = new Program(
                                replMode: false,
                                arguments: remainingArguments,
                                standardOutputHandler: console.Out.WriteLine
                            );

                            result = program.RunFile(scriptName);
                        }

                        return Task.FromResult(result);
                    }
                })
            };

            var scriptNameArgument = new Argument<string>
            {
                Name = "script-name",
                Arity = ArgumentArity.ZeroOrOne,
            };

            scriptNameArgument.AddValidator(result =>
            {
                var tokens = result.Parent!.Tokens;

                if (tokens.Any(t => t.Type == System.CommandLine.Parsing.TokenType.Option && t.Value == evalOption.Name))
                {
                    return "<script-name> positional argument cannot be used together with the -e option";
                }

                return null;
            });

            rootCommand.AddArgument(scriptNameArgument);

            rootCommand.AddValidator(result =>
            {
                if (result.HasOption(evalOption) && result.HasOption(printOption))
                {
                    return "Error: the -e and -p option are mutually exclusive";
                }

                if (result.HasOption(evalOption) && result.HasArgument(scriptNameArgument))
                {
                    return "Error: the -e option cannot be combined with the <script-name> argument";
                }

                return null;
            });

            var scriptArguments = new Argument<string>
            {
                Name = "args",
                Arity = ArgumentArity.ZeroOrMore
            };

            rootCommand.AddArgument(scriptArguments);

            foreach (Option option in options)
            {
                rootCommand.AddOption(option);
            }

            return new CommandLineBuilder(rootCommand)
                .UseDefaults()
                .Build()
                .Invoke(args, console);
        }

        internal Program(
            bool replMode,
            IEnumerable<string> arguments = null,
            Action<string> standardOutputHandler = null,
            Action<RuntimeError> runtimeErrorHandler = null)
        {
            // TODO: Make these be separate handlers at some point, so the caller can separate between these types of
            // output.
            this.standardOutputHandler = standardOutputHandler ?? Console.WriteLine;
            this.standardErrorHandler = standardOutputHandler ?? Console.Error.WriteLine;

            interpreter = new PerlangInterpreter(
                runtimeErrorHandler ?? RuntimeError,
                this.standardOutputHandler,
                arguments ?? new List<string>(),
                replMode: replMode
            );
        }

        private int RunFile(string path)
        {
            if (!File.Exists(path))
            {
                Console.Error.WriteLine($"Error: File {path} not found");
                return (int)ExitCodes.FILE_NOT_FOUND;
            }

            var bytes = File.ReadAllBytes(path);

            Run(Encoding.UTF8.GetString(bytes));

            // Indicate an error in the exit code.
            if (hadError)
            {
                return (int)ExitCodes.ERROR;
            }

            if (hadRuntimeError)
            {
                return (int)ExitCodes.RUNTIME_ERROR;
            }

            return (int)ExitCodes.SUCCESS;
        }

        private void RunPrompt()
        {
            PrintBanner();
            ReadLine.HistoryEnabled = true;
            ReadLine.AutoCompletionHandler = new AutoCompletionHandler();

            while (true)
            {
                string command = ReadLine.Read("> ");

                if (command == "quit" || command == "exit")
                {
                    break;
                }

                Run(command);
            }
        }

        internal int Run(string source)
        {
            object result = interpreter.Eval(source, ScanError, ParseError, ResolveError, ValidationError, ValidationError);

            if (result != null && result != VoidObject.Void)
            {
                standardOutputHandler(result.ToString());
            }

            return (int)ExitCodes.SUCCESS;
        }

        private void ParseAndPrint(string source)
        {
            string result = interpreter.Parse(source, ScanError, ParseError);

            standardOutputHandler(result);
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
