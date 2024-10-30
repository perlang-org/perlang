#nullable enable
#pragma warning disable S112
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Perlang.Compiler;
using Perlang.Interpreter;
using Perlang.Interpreter.Compiler;
using Perlang.Interpreter.NameResolution;
using Perlang.Native;
using Perlang.Parser;
using ParseError = Perlang.Parser.ParseError;

#if _WINDOWS
using System.Diagnostics;
#endif

namespace Perlang.ConsoleApp
{
    public class Program : IDisposable
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

        private bool hadError;
        private bool hadRuntimeError;

        /// <summary>
        /// Entry point for the `perlang` binary.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <returns>Zero if the program executed successfully; non-zero otherwise.</returns>
        public static int Main(string[] args)
        {
            // Let the Perlang native method handle options first. When it returns, we handle the options which are still
            // handled by the C# code. The quirks here are because C# excludes the program name from the args, while
            // getopt_long on the native side of the fence expects it to be present.
            NativeProgram.native_main(args.Length + 1, new string[] { Environment.GetCommandLineArgs()[0] }.Concat(args).ToArray());

            var versionOption = new Option<bool>(new[] { "--version", "-v" }, "Show version information");
            var detailedVersionOption = new Option<bool>("-V", "Show detailed version information");
            var compileAndAssembleOnlyOption = new Option<bool>("-c", "Compile and assemble to a .o file, but do not produce and execute an executable");
            var idempotentOption = new Option<bool>("--idempotent", "Run in idempotent mode, where the output is deterministic and reproducible. This avoids timestamps and other variadic data in the output.");
            var outputOption = new Option<string>("-o", "Output file name. In normal mode, this controls the name of the executable. In compile-and-assemble mode, this controls the name of the .o file.") { ArgumentHelpName = "output" };
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
                idempotentOption,
                outputOption,
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
                        throw new ApplicationException("INTERNAL ERROR: This should already have been handled by the native code");
                    }

                    if (parseResult.HasOption(detailedVersionOption))
                    {
                        throw new ApplicationException("INTERNAL ERROR: This should already have been handled by the native code");
                    }

                    if (parseResult.HasOption(idempotentOption) && !parseResult.HasOption(compileAndAssembleOnlyOption))
                    {
                        Console.Error.WriteLine("ERROR: The --idempotent option can only be used together with the -c option");
                        return Task.FromResult(1);
                    }

                    if (parseResult.Tokens.Count == 0)
                    {
                        // TODO: Tried to fix this using some logic in the rootCommand.AddValidator() lambda, but I
                        // TODO: couldn't get it working. Since this is going to be rewritten in Perlang at some point
                        // TODO: anyway, let's not spend too much time thinking about it.
                        Console.Error.WriteLine("ERROR: The <script-name> argument must be provided");
                        return Task.FromResult(1);
                    }
                    else if (parseResult.HasOption(compileAndAssembleOnlyOption)) {
                        string[] scriptNames = new[] {
                            parseResult.GetValueForArgument(scriptNameArgument) }.Concat(
                            parseResult.GetValueForArgument(scriptArguments)
                        ).ToArray();

                        string? outputFileName = parseResult.GetValueForOption(outputOption);
                        bool idempotent = parseResult.GetValueForOption(idempotentOption);

                        using var program = new Program(
                            replMode: false,
                            standardOutputHandler: Console.WriteLine,
                            disabledWarningsAsErrors: disabledWarningsAsErrorsList,
                            experimentalCompilation: PerlangMode.ExperimentalCompilation
                        );

                        int result = program.CompileAndAssembleFile(scriptNames, outputFileName, idempotent);
                        return Task.FromResult(result);
                    }
                    else
                    {
                        string scriptName = parseResult.GetValueForArgument(scriptNameArgument);
                        string? outputFileName = parseResult.GetValueForOption(outputOption);
                        int result;

                        if (parseResult.Tokens.Count == 1)
                        {
                            using var program = new Program(
                                replMode: false,
                                standardOutputHandler: Console.WriteLine,
                                disabledWarningsAsErrors: disabledWarningsAsErrorsList,
                                experimentalCompilation: PerlangMode.ExperimentalCompilation
                            );

                            result = program.RunFile(scriptName, outputFileName);
                        }
                        else
                        {
                            var remainingArguments = parseResult.GetValueForArgument(scriptArguments);

                            using var program = new Program(
                                replMode: false,
                                arguments: remainingArguments,
                                standardOutputHandler: Console.WriteLine,
                                disabledWarningsAsErrors: disabledWarningsAsErrorsList,
                                experimentalCompilation: PerlangMode.ExperimentalCompilation
                            );

                            // FIXME: The remainingArguments should be passed here instead into the Program constructor.
                            result = program.RunFile(scriptName, outputFileName);
                        }

                        return Task.FromResult(result);
                    }
                })
            };

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

        private Program(
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

            // Convenience fields while we are migrating away from CLR strings to Perlang strings.
            this.standardOutputHandlerFromClrString = s => this.standardOutputHandler(Lang.String.from(s));
            this.standardErrorHandlerFromClrString = s => this.standardErrorHandler(Lang.String.from(s));

            compiler = new PerlangCompiler(
                runtimeErrorHandler ?? RuntimeError,
                this.standardOutputHandler
            );
        }

        public void Dispose()
        {
            compiler.Dispose();
        }

        private int RunFile(string path, string? outputPath)
        {
            if (!File.Exists(path))
            {
                standardErrorHandler(Lang.String.from($"Error: File {path} not found"));
                return (int)ExitCodes.FILE_NOT_FOUND;
            }

            string source = NativeFile.read_all_text(path);

            CompileAndRun(source, path, outputPath, CompilerWarning);

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

        private int CompileAndAssembleFile(string[] scriptFiles, string? targetPath, bool idempotent)
        {
            using var completeSource = NativeStringBuilder.Create();

            foreach (string scriptFile in scriptFiles)
            {
                if (!File.Exists(scriptFile))
                {
                    standardErrorHandler(Lang.String.from($"Error: File {scriptFile} not found"));
                    return (int)ExitCodes.FILE_NOT_FOUND;
                }

                string source = NativeFile.read_all_text(scriptFile);

                completeSource.Append(source);
                completeSource.AppendLine();
            }

            // Note: path here is a bit of a simplification. It means that if you specify multiple .per files as the
            // input, the output .cc/executable name will always be based on the first file name provided.
            string path = scriptFiles.First();

            CompileAndAssemble(completeSource.ToString(), path, targetPath, CompilerWarning, idempotent);

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

        private void CompileAndRun(string source, string path, string? targetPath, CompilerWarningHandler compilerWarningHandler)
        {
            compiler.CompileAndRun(source, path, targetPath, CompilerFlags.None, ScanError, ParseError, NameResolutionError, ValidationError, ValidationError, compilerWarningHandler);
        }

        private void CompileAndAssemble(string source, string path, string? targetPath, CompilerWarningHandler compilerWarningHandler, bool idempotent)
        {
            compiler.CompileAndAssemble(source, path, targetPath, CompilerFlags.None | (idempotent ? CompilerFlags.Idempotent : 0), ScanError, ParseError, NameResolutionError, ValidationError, ValidationError, compilerWarningHandler);
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
