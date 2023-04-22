#nullable enable
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.IO;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Terminal;
using Perlang.Internal;
using Perlang.Interpreter;
using Perlang.Interpreter.NameResolution;
using Perlang.Parser;
using ParseError = Perlang.Parser.ParseError;

#if _WINDOWS
using System.Diagnostics;
#endif

namespace Perlang.ConsoleApp
{
    public class Program
    {
        private static readonly ISet<string> ReplCommands = new HashSet<string>(new List<string>
        {
            "exit",
            "help",
            "quit"
        }.Select(s => "/" + s));

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

        /// <summary>
        /// Writes a Perlang native string to the standard output stream.
        /// </summary>
        private readonly Action<Lang.String> standardOutputHandler;

        /// <summary>
        /// Writes a Perlang native string to the standard error stream.
        /// </summary>
        private readonly Action<Lang.String> standardErrorHandler;

        /// <summary>
        /// Writes a CLR string to the standard output stream.
        /// </summary>
        private readonly Action<string> standardOutputHandlerFromClrString;

        /// <summary>
        /// Writes a CLR string to the standard error stream.
        /// </summary>
        private readonly Action<string> standardErrorHandlerFromClrString;

        private readonly HashSet<WarningType> disabledWarningsAsErrors;

        private bool hadError;
        private bool hadRuntimeError;

        /// <summary>
        /// Entry point for the `perlang` binary.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <returns>Zero if the program executed successfully; non-zero otherwise.</returns>
        public static int Main(string[] args)
        {
            return MainWithCustomConsole(args, console: new PerlangConsole());
        }

        /// <summary>
        /// Entry point for `perlang`, with the possibility to override the console streams (stdin, stdout, stderr).
        ///
        /// This constructor is typically used from tests, wishing to intercept the output to stdout and/or stderr.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <param name="console">A custom `IConsole` implementation to use. May be null, in which case the standard
        /// streams of the calling process will be used.</param>
        /// <returns>Zero if the program executed successfully; non-zero otherwise.</returns>
        // TODO: Replace IConsole here with interface which is "Perlang string-aware", so we can delegate to libc write() instead of using Console.WriteLine.
        public static int MainWithCustomConsole(string[] args, IPerlangConsole console)
        {
            var versionOption = new Option<bool>(new[] { "--version", "-v" }, "Show version information");
            var detailedVersionOption = new Option<bool>("-V", "Show detailed version information");
            var evalOption = new Option<string>("-e", "Executes a single-line script") { AllowMultipleArgumentsPerToken = false, ArgumentHelpName = "script" };
            var printOption = new Option<string>("-p", "Parse a single-line script and output a human-readable version of the AST") { ArgumentHelpName = "script" };
            var noWarnAsErrorOption = new Option<string>("-Wno-error", "Treats specified warning as a warning instead of an error.") { ArgumentHelpName = "error" };

            var disabledWarningsAsErrorsList = new List<WarningType>();

            noWarnAsErrorOption.AddValidator(result =>
            {
                string warningName = result.GetValueOrDefault<string>()!;

                if (!WarningType.KnownWarning(warningName))
                {
                    result.ErrorMessage = $"Unknown warning: {warningName}";
                }

                disabledWarningsAsErrorsList.Add(WarningType.Get(warningName));
            });

            // Note: options must be present in this list to be valid for the RootCommand.
            var options = new Option[]
            {
                versionOption,
                detailedVersionOption,
                evalOption,
                printOption,
                noWarnAsErrorOption
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
                    result.ErrorMessage = "<script-name> positional argument cannot be used together with the -e option";
                }
            });

            var scriptArguments = new Argument<IEnumerable<string>>
            {
                Name = "args",
                Arity = ArgumentArity.ZeroOrMore
            };

            var rootCommand = new RootCommand
            {
                Description = "The Perlang Interpreter",

                Handler = CommandHandler.Create((ParseResult parseResult, IConsole _) =>
                {
                    if (parseResult.HasOption(versionOption))
                    {
                        console.Out.WriteLine(CommonConstants.InformationalVersion);
                        return Task.FromResult(0);
                    }

                    if (parseResult.HasOption(detailedVersionOption))
                    {
                        console.Out.WriteLine($"Perlang {CommonConstants.InformationalVersion} (built from git commit {CommonConstants.GitCommit}) on .NET {Environment.Version}");
                        console.Out.WriteLine();
                        console.Out.WriteLine($"  Number of detected (v)CPUs: {Environment.ProcessorCount}");
                        console.Out.WriteLine($"  Running in 64-bit mode: {Environment.Is64BitProcess}");
                        console.Out.WriteLine($"  Operating system info: {Environment.OSVersion.VersionString}");
                        console.Out.WriteLine();

                        return Task.FromResult(0);
                    }

                    if (parseResult.HasOption(evalOption))
                    {
                        string source = parseResult.GetValueForOption(evalOption)!;

                        var program = new Program(
                            replMode: true,
                            standardOutputHandler: console.WriteStdoutLine,
                            disabledWarningsAsErrors: disabledWarningsAsErrorsList
                        );

                        int result = program.Run(source, program.CompilerWarning);

                        return Task.FromResult(result);
                    }
                    else if (parseResult.HasOption(printOption))
                    {
                        string source = parseResult.GetValueForOption(printOption)!;

                        new Program(
                            replMode: true,
                            standardOutputHandler: console.WriteStdoutLine,
                            disabledWarningsAsErrors: disabledWarningsAsErrorsList
                        ).ParseAndPrint(source);

                        return Task.FromResult(0);
                    }
                    else if (parseResult.Tokens.Count == 0)
                    {
                        new Program(
                            replMode: true,
                            standardOutputHandler: console.WriteStdoutLine,
                            disabledWarningsAsErrors: disabledWarningsAsErrorsList
                        ).RunPrompt();

                        return Task.FromResult(0);
                    }
                    else
                    {
                        string scriptName = parseResult.GetValueForArgument(scriptNameArgument);
                        int result;

                        if (parseResult.Tokens.Count == 1)
                        {
                            var program = new Program(
                                replMode: false,
                                standardOutputHandler: console.WriteStdoutLine,
                                disabledWarningsAsErrors: disabledWarningsAsErrorsList
                            );

                            result = program.RunFile(scriptName);
                        }
                        else
                        {
                            var remainingArguments = parseResult.GetValueForArgument(scriptArguments);

                            var program = new Program(
                                replMode: false,
                                arguments: remainingArguments,
                                standardOutputHandler: console.WriteStdoutLine,
                                disabledWarningsAsErrors: disabledWarningsAsErrorsList
                            );

                            result = program.RunFile(scriptName);
                        }

                        return Task.FromResult(result);
                    }
                })
            };

            rootCommand.AddValidator(result =>
            {
                if (result.HasOption(evalOption) && result.HasOption(printOption))
                {
                    result.ErrorMessage = "Error: the -e and -p options are mutually exclusive";
                }

                if (result.HasOption(evalOption) && result.HasArgument(scriptNameArgument))
                {
                    result.ErrorMessage = "Error: the -e option cannot be combined with the <script-name> argument";
                }
            });

            rootCommand.AddArgument(scriptNameArgument);
            rootCommand.AddArgument(scriptArguments);

            foreach (Option option in options)
            {
                rootCommand.AddOption(option);
            }

            return new CommandLineBuilder(rootCommand)
                .UseHelp()
                .Build()
                .Invoke(args, console);
        }

        internal Program(
            bool replMode,
            Action<Lang.String> standardOutputHandler,
            IEnumerable<string>? arguments = null,
            IEnumerable<WarningType>? disabledWarningsAsErrors = null,
            Action<RuntimeError>? runtimeErrorHandler = null)
        {
            // TODO: Make these be separate handlers at some point, so the caller can separate between these types of
            // TODO: output.
            this.standardOutputHandler = standardOutputHandler;
            this.standardErrorHandler = standardOutputHandler;
            this.disabledWarningsAsErrors = (disabledWarningsAsErrors ?? Enumerable.Empty<WarningType>()).ToHashSet();

            // Convenience fields while we are migrating away from CLR strings to Perlang strings.
            this.standardOutputHandlerFromClrString = s => this.standardOutputHandler(Lang.String.from(s));
            this.standardErrorHandlerFromClrString = s => this.standardErrorHandler(Lang.String.from(s));

            interpreter = new PerlangInterpreter(
                runtimeErrorHandler ?? RuntimeError,
                this.standardOutputHandler,
                null,
                arguments ?? new List<string>(),
                replMode: replMode
            );
        }

        private int RunFile(string path)
        {
            if (!File.Exists(path))
            {
                standardErrorHandler(Lang.String.from($"Error: File {path} not found"));
                return (int)ExitCodes.FILE_NOT_FOUND;
            }

            var bytes = File.ReadAllBytes(path);

            Run(Encoding.UTF8.GetString(bytes), CompilerWarning);

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

            // TODO: Should we use different history files for snapshots and release versions?
            var lineEditor = new LineEditor("perlang");

            lineEditor.AutoCompleteEvent += (a, pos) =>
            {
                var matchingKeywords = Scanner.ReservedKeywords
                    .Where(keyword => keyword.Key.StartsWith(a))
                    .Where(keyword => keyword.Value != TokenType.RESERVED_WORD)
                    .Select(keyword => keyword.Key)
                    .ToList();

                matchingKeywords.AddRange(
                    ReplCommands.Where(cmd => cmd.StartsWith(a))
                );

                string prefix = String.Empty;

                if (matchingKeywords.Count == 1)
                {
                    return new LineEditor.Completion(prefix, new[] { matchingKeywords[0].Substring(pos) });
                }
                else
                {
                    return new LineEditor.Completion(prefix, matchingKeywords.ToArray());
                }
            };

            string command;

            while ((command = lineEditor.Edit("> ", String.Empty)) != null)
            {
                if (command.ToLowerInvariant() == "/quit" || command.ToLowerInvariant() == "/exit")
                {
                    break;
                }
                else if (command.ToLowerInvariant() == "/help")
                {
                    ShowHelp();
                    continue;
                }

                // REPL mode is more relaxed: -Wno-error is enabled for all warnings by default. It simply makes sense
                // to be less strict in this mode, since it's often used in an ad-hoc fashion for exploratory
                // programming.
                Run(command, CompilerWarningAsWarning);
            }
        }

        private void ShowHelp()
        {
            standardOutputHandlerFromClrString(
                "\n" +
                "This is the Perlang interactive console (commonly called REPL, short for\n" +
                "Read-Evaluate-Print-Loop). Any valid Perlang expression or statement can be\n" +
                "entered here, and will be evaluated dynamically. For example, 10 + 5, 2 ** 32,\n" +
                "or `print \"Hello, World\"`"
            );

            standardOutputHandler(Lang.String.Empty);

            standardOutputHandlerFromClrString("The following special commands are also available:");
            standardOutputHandlerFromClrString("\x1B[36;1m/quit\x1B[0m or \x1B[36;1m/exit\x1B[0m to quit the program.");
            standardOutputHandlerFromClrString("\x1B[36;1m/help\x1B[0m to display this help.");
            standardOutputHandler(Lang.String.Empty);

            standardOutputHandlerFromClrString(
                "For more information on the Perlang language, please consult this web page:\n" +
                "https://perlang.org/learn. Thank you for your interest in the project! 🙏"
            );

            standardOutputHandler(Lang.String.Empty);
        }

        internal int Run(string source, CompilerWarningHandler compilerWarningHandler)
        {
            object? result = interpreter.Eval(source, ScanError, ParseError, NameResolutionError, ValidationError, ValidationError, compilerWarningHandler);

            if (result != null && result != VoidObject.Void)
            {
                standardOutputHandler(Utils.Stringify(result));
            }

            return (int)ExitCodes.SUCCESS;
        }

        private void ParseAndPrint(string source)
        {
            string? result = interpreter.Parse(source, ScanError, ParseError);

            if (result == null)
            {
                // Parse() returns `null` when one or more errors occurred. These errors have already been reported to
                // the user at this point, so we can safely just return here.
                return;
            }

            standardOutputHandler(Lang.String.from(result));
        }

        private void PrintBanner()
        {
            standardOutputHandlerFromClrString($"Perlang Interactive REPL Console (\x1B[1m{CommonConstants.GetFullVersion()}\x1B[0m, built from git commit {CommonConstants.GitCommit})");
            standardOutputHandlerFromClrString("Type \x1B[36;1m/help\x1B[0m for more information or \x1B[36;1m/quit\x1B[0m to quit the program.");
            standardOutputHandler(Lang.String.Empty);
        }

        private void ScanError(ScanError scanError)
        {
            ReportError(scanError.Line, String.Empty, scanError.Message);
        }

        private void RuntimeError(RuntimeError error)
        {
            string line = error.Token?.Line.ToString() ?? "unknown";

            standardOutputHandlerFromClrString($"[line {line}] {error.Message}");
            hadRuntimeError = true;
        }

        private void Error(Token token, string message)
        {
            if (token.Type == TokenType.EOF)
            {
                ReportError(token.Line, " at end", message);
            }
            else
            {
                ReportError(token.Line, " at '" + token.Lexeme + "'", message);
            }
        }

        private void ReportError(int line, string where, string message)
        {
            standardErrorHandlerFromClrString($"[line {line}] Error{where}: {message}");
            hadError = true;
        }

        private void Warn(Token token, string message)
        {
            if (token.Type == TokenType.EOF)
            {
                ReportWarning(token.Line, " at end", message);
            }
            else
            {
                ReportWarning(token.Line, " at '" + token.Lexeme + "'", message);
            }
        }

        private void ReportWarning(int line, string where, string message)
        {
            standardErrorHandlerFromClrString($"[line {line}] Warning{where}: {message}");
        }

        private void ParseError(ParseError parseError)
        {
            Error(parseError.Token, parseError.Message);
        }

        private void NameResolutionError(NameResolutionError nameResolutionError)
        {
            Error(nameResolutionError.Token, nameResolutionError.Message);
        }

        private void ValidationError(ValidationError validationError)
        {
            Error(validationError.Token, validationError.Message);
        }

        /// <returns>`true` if the warning is considered an error; `false` otherwise.</returns>
        private bool CompilerWarning(CompilerWarning compilerWarning)
        {
            if (!disabledWarningsAsErrors.Contains(compilerWarning.WarningType))
            {
                return CompilerWarningAsError(compilerWarning);
            }
            else
            {
                return CompilerWarningAsWarning(compilerWarning);
            }
        }

        private bool CompilerWarningAsError(CompilerWarning compilerWarning)
        {
            Error(compilerWarning.Token, compilerWarning.Message);

            return true;
        }

        private bool CompilerWarningAsWarning(CompilerWarning compilerWarning)
        {
            Warn(compilerWarning.Token, compilerWarning.Message);

            return false;
        }
    }
}
