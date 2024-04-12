#nullable enable
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Perlang.Compiler;
using Perlang.Internal;
using Perlang.Interpreter;
using Perlang.Interpreter.Compiler;
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
        private readonly PerlangCompiler compiler;

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

        /// <summary>
        /// A flag which determines if (highly experimental) compilation to machine code is enabled or not.
        /// </summary>
        private readonly bool experimentalCompilation;

        private bool hadError;
        private bool hadRuntimeError;

        /// <summary>
        /// Entry point for the `perlang` binary.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <returns>Zero if the program executed successfully; non-zero otherwise.</returns>
        public static int Main(string[] args)
        {
            var versionOption = new Option<bool>(new[] { "--version", "-v" }, "Show version information");
            var detailedVersionOption = new Option<bool>("-V", "Show detailed version information");
            var compileAndAssembleOnlyOption = new Option<bool>("-c", "Compile and assemble to a .o file, but do not produce and execute an executable");
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
                compileAndAssembleOnlyOption,
                printOption,
                noWarnAsErrorOption
            };

            var scriptNameArgument = new Argument<string>
            {
                Name = "script-name",
                Arity = ArgumentArity.ZeroOrOne,
            };

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
                        Console.Out.WriteLine(CommonConstants.InformationalVersion);
                        return Task.FromResult(0);
                    }

                    if (parseResult.HasOption(detailedVersionOption))
                    {
                        Console.Out.WriteLine($"Perlang {CommonConstants.InformationalVersion} (built from git commit {CommonConstants.GitCommit}) on .NET {Environment.Version}");
                        Console.Out.WriteLine();
                        Console.Out.WriteLine($"  Number of detected (v)CPUs: {Environment.ProcessorCount}");
                        Console.Out.WriteLine($"  Running in 64-bit mode: {Environment.Is64BitProcess}");
                        Console.Out.WriteLine($"  Operating system info: {Environment.OSVersion.VersionString}");
                        Console.Out.WriteLine();

                        return Task.FromResult(0);
                    }

                    if (parseResult.HasOption(printOption))
                    {
                        string source = parseResult.GetValueForOption(printOption)!;

                        new Program(
                            replMode: true,
                            standardOutputHandler: Console.WriteLine,
                            disabledWarningsAsErrors: disabledWarningsAsErrorsList
                        ).ParseAndPrint(source);

                        return Task.FromResult(0);
                    }
                    else if (parseResult.Tokens.Count == 0)
                    {
                        // TODO: Tried to fix this using some logic in the rootCommand.AddValidator() lambda, but I
                        // TODO: couldn't get it working. Since this is going to be rewritten in Perlang at some point
                        // TODO: anyway, let's not spend too much time thinking about it.
                        Console.Error.WriteLine("ERROR: One of the -p <script> or <script-name> arguments must be provided");
                        return Task.FromResult(1);
                    }
                    else if (parseResult.HasOption(compileAndAssembleOnlyOption))
                    {
                        string scriptName = parseResult.GetValueForArgument(scriptNameArgument);

                        var program = new Program(
                            replMode: false,
                            standardOutputHandler: Console.WriteLine,
                            disabledWarningsAsErrors: disabledWarningsAsErrorsList,
                            experimentalCompilation: PerlangMode.ExperimentalCompilation
                        );

                        int result = program.CompileAndAssembleFile(scriptName);
                        return Task.FromResult(result);
                    }
                    else
                    {
                        string scriptName = parseResult.GetValueForArgument(scriptNameArgument);
                        int result;

                        if (parseResult.Tokens.Count == 1)
                        {
                            var program = new Program(
                                replMode: false,
                                standardOutputHandler: Console.WriteLine,
                                disabledWarningsAsErrors: disabledWarningsAsErrorsList,
                                experimentalCompilation: PerlangMode.ExperimentalCompilation
                            );

                            result = program.RunFile(scriptName);
                        }
                        else
                        {
                            var remainingArguments = parseResult.GetValueForArgument(scriptArguments);

                            var program = new Program(
                                replMode: false,
                                arguments: remainingArguments,
                                standardOutputHandler: Console.WriteLine,
                                disabledWarningsAsErrors: disabledWarningsAsErrorsList,
                                experimentalCompilation: PerlangMode.ExperimentalCompilation
                            );

                            result = program.RunFile(scriptName);
                        }

                        return Task.FromResult(result);
                    }
                })
            };

            rootCommand.AddValidator(result =>
            {
                if (result.HasOption(compileAndAssembleOnlyOption) && result.HasOption(printOption))
                {
                    result.ErrorMessage = "Error: the -c and -p options are mutually exclusive";
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
                .Invoke(args);
        }

        internal Program(
            bool replMode,
            Action<Lang.String> standardOutputHandler,
            IEnumerable<string>? arguments = null,
            IEnumerable<WarningType>? disabledWarningsAsErrors = null,
            Action<RuntimeError>? runtimeErrorHandler = null,
            bool experimentalCompilation = false)
        {
            // TODO: Make these be separate handlers at some point, so the caller can separate between these types of
            // TODO: output.
            this.standardOutputHandler = standardOutputHandler;
            this.standardErrorHandler = standardOutputHandler;
            this.disabledWarningsAsErrors = (disabledWarningsAsErrors ?? Enumerable.Empty<WarningType>()).ToHashSet();
            this.experimentalCompilation = experimentalCompilation;

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

            compiler = new PerlangCompiler(
                runtimeErrorHandler ?? RuntimeError,
                this.standardOutputHandler
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
            string source = Encoding.UTF8.GetString(bytes);

            if (experimentalCompilation)
            {
                CompileAndRun(source, path, CompilerWarning);
            }
            else
            {
                Run(source, CompilerWarning);
            }

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

        private int CompileAndAssembleFile(string path)
        {
            if (!File.Exists(path))
            {
                standardErrorHandler(Lang.String.from($"Error: File {path} not found"));
                return (int)ExitCodes.FILE_NOT_FOUND;
            }

            var bytes = File.ReadAllBytes(path);
            string source = Encoding.UTF8.GetString(bytes);

            CompileAndAssemble(source, path, CompilerWarning);

            // Indicate an error in the exit code.
            if (hadError)
            {
                return (int)ExitCodes.ERROR;
            }

            // Should never happen, but _if_ it does, it's surely better that we return with non-zero rather than falsely
            // imply success.
            if (hadRuntimeError)
            {
                return (int)ExitCodes.RUNTIME_ERROR;
            }

            return (int)ExitCodes.SUCCESS;
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

        private void CompileAndRun(string source, string path, CompilerWarningHandler compilerWarningHandler)
        {
            compiler.CompileAndRun(source, path, CompilerFlags.None, ScanError, ParseError, NameResolutionError, ValidationError, ValidationError, compilerWarningHandler);
        }

        private void CompileAndAssemble(string source, string path, CompilerWarningHandler compilerWarningHandler)
        {
            compiler.CompileAndAssemble(source, path, CompilerFlags.None, ScanError, ParseError, NameResolutionError, ValidationError, ValidationError, compilerWarningHandler);
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
