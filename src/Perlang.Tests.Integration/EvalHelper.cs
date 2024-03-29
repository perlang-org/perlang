using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Perlang.Compiler;
using Perlang.Interpreter;
using Perlang.Interpreter.Compiler;
using Perlang.Interpreter.NameResolution;
using Perlang.Parser;
using Xunit;

namespace Perlang.Tests.Integration
{
    internal static class EvalHelper
    {
        /// <summary>
        /// Evaluates the provided expression or list of statements, returning the evaluated value.
        ///
        /// This method will propagate all types of errors to the caller. If multiple errors are encountered, only the
        /// first will be thrown.
        ///
        /// Output printed to the standard output stream will be silently discarded by this method. For tests which need
        /// to make assertions based on the output printed, see e.g. the <see cref="EvalReturningOutput"/> method.
        ///
        /// Note that compiler warnings will be propagated as exceptions to the caller; in other words, this resembles
        /// the `-Werror` flag being enabled.
        /// </summary>
        /// <param name="source">A valid Perlang program.</param>
        /// <returns>The result of evaluating the provided expression, or `null` if provided a list of
        /// statements.</returns>
        internal static object Eval(string source)
        {
            if (PerlangMode.ExperimentalCompilation)
            {
                throw new SkipException("Evaluating and returning a result is not supported in experimental compilation mode");
            }
            else
            {
                var interpreter = new PerlangInterpreter(AssertFailRuntimeErrorHandler, _ => { });

                return interpreter.Eval(
                    source,
                    AssertFailScanErrorHandler,
                    AssertFailParseErrorHandler,
                    AssertFailNameResolutionErrorHandler,
                    AssertFailValidationErrorHandler,
                    AssertFailValidationErrorHandler,
                    AssertFailCompilerWarningHandler
                );
            }
        }

        /// <summary>
        /// Evaluates the provided expression or list of statements, returning an <see cref="EvalResult{T}"/> with <see
        /// cref="EvalResult{T}.Value"/> set to the evaluated value.
        ///
        /// Output printed to the standard output stream will be available in <see cref="EvalResult{T}.Output"/>.
        ///
        /// This method will propagate all errors apart from  <see cref="RuntimeError"/> to the caller. Runtime errors
        /// will be available in the returned <see cref="EvalResult{T}"/>.
        ///
        /// If any warnings are emitted, they will be available in the returned <see
        /// cref="EvalResult{T}.CompilerWarnings"/> property. "Warnings as errors" will be disabled for all warnings.
        /// </summary>
        /// <param name="source">A valid Perlang program.</param>
        /// <param name="arguments">Zero or more arguments to be passed to the program.</param>
        /// <returns>An <see cref="EvalResult{T}"/> with the <see cref="EvalResult{T}.Value"/> property set to the
        /// result of the provided expression. If not provided a valid expression, <see cref="EvalResult{T}.Value"/>
        /// will be set to `null`.</returns>
        internal static EvalResult<RuntimeError> EvalWithRuntimeErrorCatch(string source, params string[] arguments)
        {
            var result = new EvalResult<RuntimeError>();

            var interpreter = new PerlangInterpreter(
                result.ErrorHandler, result.OutputHandler, null, arguments
            );

            result.Value = interpreter.Eval(
                source,
                AssertFailScanErrorHandler,
                AssertFailParseErrorHandler,
                AssertFailNameResolutionErrorHandler,
                AssertFailValidationErrorHandler,
                AssertFailValidationErrorHandler,
                result.WarningHandler
            );

            return result;
        }

        /// <summary>
        /// Evaluates the provided expression or list of statements, returning an <see cref="EvalResult{T}"/> with <see
        /// cref="EvalResult{T}.Value"/> set to the evaluated value.
        ///
        /// Output printed to the standard output stream will be available in <see cref="EvalResult{T}.Output"/>.
        ///
        /// This method will propagate all errors apart from  <see cref="ScanError"/> to the caller. Scan errors
        /// will be available in the returned <see cref="EvalResult{T}.Errors"/> property.
        ///
        /// If any warnings are emitted, they will be available in the returned <see
        /// cref="EvalResult{T}.CompilerWarnings"/> property. "Warnings as errors" will be disabled for all warnings.
        /// </summary>
        /// <param name="source">A valid Perlang program.</param>
        /// <returns>An <see cref="EvalResult{T}"/> with the <see cref="EvalResult{T}.Value"/> property set to the
        /// result of the provided expression. If not provided a valid expression, <see cref="EvalResult{T}.Value"/>
        /// will be set to `null`.</returns>
        internal static EvalResult<ScanError> EvalWithScanErrorCatch(string source)
        {
            var result = new EvalResult<ScanError>();
            var interpreter = new PerlangInterpreter(AssertFailRuntimeErrorHandler, result.OutputHandler);

            result.Value = interpreter.Eval(
                source,
                result.ErrorHandler,
                AssertFailParseErrorHandler,
                AssertFailNameResolutionErrorHandler,
                AssertFailValidationErrorHandler,
                AssertFailValidationErrorHandler,
                result.WarningHandler
            );

            return result;
        }

        /// <summary>
        /// Evaluates the provided expression or list of statements, returning an <see cref="EvalResult{T}"/> with <see
        /// cref="EvalResult{T}.Value"/> set to the evaluated value.
        ///
        /// Output printed to the standard output stream will be available in <see cref="EvalResult{T}.Output"/>.
        ///
        /// This method will propagate all errors apart from  <see cref="ParseError"/> to the caller. Parse errors
        /// will be available in the returned <see cref="EvalResult{T}.Errors"/> property.
        ///
        /// If any warnings are emitted, they will be available in the returned <see
        /// cref="EvalResult{T}.CompilerWarnings"/> property. "Warnings as errors" will be disabled for all warnings.
        /// </summary>
        /// <param name="source">A valid Perlang program.</param>
        /// <returns>An <see cref="EvalResult{T}"/> with the <see cref="EvalResult{T}.Value"/> property set to the
        /// result of the provided expression. If not provided a valid expression, <see cref="EvalResult{T}.Value"/>
        /// will be set to `null`.</returns>
        internal static EvalResult<ParseError> EvalWithParseErrorCatch(string source)
        {
            var result = new EvalResult<ParseError>();
            var interpreter = new PerlangInterpreter(AssertFailRuntimeErrorHandler, result.OutputHandler);

            result.Value = interpreter.Eval(
                source,
                AssertFailScanErrorHandler,
                result.ErrorHandler,
                AssertFailNameResolutionErrorHandler,
                AssertFailValidationErrorHandler,
                AssertFailValidationErrorHandler,
                result.WarningHandler
            );

            return result;
        }

        /// <summary>
        /// Evaluates the provided expression or list of statements, returning an <see cref="EvalResult{T}"/> with <see
        /// cref="EvalResult{T}.Value"/> set to the evaluated value.
        ///
        /// Output printed to the standard output stream will be available in <see cref="EvalResult{T}.Output"/>.
        ///
        /// This method will propagate all errors apart from  <see cref="NameResolutionError"/> to the caller. Resolve errors
        /// will be available in the returned <see cref="EvalResult{T}.Errors"/> property.
        ///
        /// If any warnings are emitted, they will be available in the returned <see
        /// cref="EvalResult{T}.CompilerWarnings"/> property. "Warnings as errors" will be disabled for all warnings.
        /// </summary>
        /// <param name="source">A valid Perlang program.</param>
        /// <returns>An <see cref="EvalResult{T}"/> with the <see cref="EvalResult{T}.Value"/> property set to the
        /// result of the provided expression. If not provided a valid expression, <see cref="EvalResult{T}.Value"/>
        /// will be set to `null`.</returns>
        internal static EvalResult<NameResolutionError> EvalWithNameResolutionErrorCatch(string source)
        {
            var result = new EvalResult<NameResolutionError>();
            var interpreter = new PerlangInterpreter(AssertFailRuntimeErrorHandler, result.OutputHandler);

            result.Value = interpreter.Eval(
                source,
                AssertFailScanErrorHandler,
                AssertFailParseErrorHandler,
                result.ErrorHandler,
                AssertFailValidationErrorHandler,
                AssertFailValidationErrorHandler,
                result.WarningHandler
            );

            return result;
        }

        /// <summary>
        /// Evaluates the provided expression or list of statements, returning an <see cref="EvalResult{T}"/> with <see
        /// cref="EvalResult{T}.Value"/> set to the evaluated value.
        ///
        /// Output printed to the standard output stream will be available in <see cref="EvalResult{T}.Output"/>.
        ///
        /// This method will propagate all errors apart from  <see cref="ValidationError"/> to the caller. Validation
        /// errors will be available in the returned <see cref="EvalResult{T}.Errors"/> property.
        ///
        /// If any warnings are emitted, they will be available in the returned <see
        /// cref="EvalResult{T}.CompilerWarnings"/> property. "Warnings as errors" will be disabled for all warnings.
        /// </summary>
        /// <param name="source">A valid Perlang program.</param>
        /// <returns>An <see cref="EvalResult{T}"/> with the <see cref="EvalResult{T}.Value"/> property set to the
        /// result of the provided expression. If not provided a valid expression, <see cref="EvalResult{T}.Value"/>
        /// will be set to `null`.</returns>
        internal static EvalResult<ValidationError> EvalWithValidationErrorCatch(string source)
        {
            var result = new EvalResult<ValidationError>();
            var interpreter = new PerlangInterpreter(AssertFailRuntimeErrorHandler, result.OutputHandler);

            result.Value = interpreter.Eval(
                source,
                AssertFailScanErrorHandler,
                AssertFailParseErrorHandler,
                AssertFailNameResolutionErrorHandler,
                result.ErrorHandler,
                result.ErrorHandler,
                result.WarningHandler
            );

            return result;
        }

        /// <summary>
        /// Evaluates the provided expression or list of statements, returning an <see cref="EvalResult{T}"/> with <see
        /// cref="EvalResult{T}.Value"/> set to the evaluated value.
        ///
        /// Output printed to the standard output stream will be available in <see cref="EvalResult{T}.Output"/>.
        ///
        /// This method will propagate all kinds of errors to the caller, throwing an exception on the first error
        /// encountered. If any warnings are emitted, they will be available in the returned <see
        /// cref="EvalResult{T}.CompilerWarnings"/> property. This can be seen as "warnings as errors" is disabled
        /// for all warnings; the caller need to explicitly check for warnings and fail if appropriate.
        /// </summary>
        /// <param name="source">A valid Perlang program.</param>
        /// <param name="arguments">Zero or more arguments to be passed to the program.</param>
        /// <returns>An <see cref="EvalResult{T}"/> with the <see cref="EvalResult{T}.Value"/> property set to the
        /// result of the provided expression. If not provided a valid expression, <see cref="EvalResult{T}.Value"/>
        /// will be set to `null`.</returns>
        internal static EvalResult<Exception> EvalWithResult(string source, params string[] arguments)
        {
            return EvalWithResult(source, CompilerFlags.None, arguments);
        }

        /// <summary>
        /// Evaluates the provided expression or list of statements, returning an <see cref="EvalResult{T}"/> with <see
        /// cref="EvalResult{T}.Value"/> set to the evaluated value.
        ///
        /// Output printed to the standard output stream will be available in <see cref="EvalResult{T}.Output"/>.
        ///
        /// This method will propagate all kinds of errors to the caller, throwing an exception on the first error
        /// encountered. If any warnings are emitted, they will be available in the returned <see
        /// cref="EvalResult{T}.CompilerWarnings"/> property. This can be seen as "warnings as errors" is disabled
        /// for all warnings; the caller need to explicitly check for warnings and fail if appropriate.
        /// </summary>
        /// <param name="source">A valid Perlang program.</param>
        /// <param name="compilerFlags">One or more <see cref="CompilerFlags"/> to use if compilation is
        /// enabled.</param>
        /// <param name="arguments">Zero or more arguments to be passed to the program.</param>
        /// <returns>An <see cref="EvalResult{T}"/> with the <see cref="EvalResult{T}.Value"/> property set to the
        /// result of the provided expression. If not provided a valid expression, <see cref="EvalResult{T}.Value"/>
        /// will be set to `null`.</returns>
        internal static EvalResult<Exception> EvalWithResult(string source, CompilerFlags compilerFlags, params string[] arguments)
        {
            if (PerlangMode.ExperimentalCompilation)
            {
                var result = new EvalResult<Exception>();
                var compiler = new PerlangCompiler(AssertFailRuntimeErrorHandler, result.OutputHandler, null, arguments);

                try
                {
                    // TODO: include the name of the test method here, to make it easier to know what executable correspond to which test
                    result.ExecutablePath = compiler.CompileAndRun(
                        source,
                        CreateTemporaryPath(source),
                        compilerFlags,
                        AssertFailScanErrorHandler,
                        AssertFailParseErrorHandler,
                        AssertFailNameResolutionErrorHandler,
                        AssertFailValidationErrorHandler,
                        AssertFailValidationErrorHandler,
                        result.WarningHandler
                    );
                }
                catch (NotImplementedInCompiledModeException e)
                {
                    // This exception is thrown to make it possible for integration tests to skip tests for code which
                    // is known to not yet work.
                    throw new SkipException(e.Message, e);
                }

                // Return something else than `null` to make it reasonable for callers to distinguish that compiled mode
                // (with no native "result") is being used, if needed.
                result.Value = VoidObject.Void;

                return result;
            }
            else
            {
                var result = new EvalResult<Exception>();
                var interpreter = new PerlangInterpreter(AssertFailRuntimeErrorHandler, result.OutputHandler, null, arguments);

                result.Value = interpreter.Eval(
                    source,
                    AssertFailScanErrorHandler,
                    AssertFailParseErrorHandler,
                    AssertFailNameResolutionErrorHandler,
                    AssertFailValidationErrorHandler,
                    AssertFailValidationErrorHandler,
                    result.WarningHandler
                );

                return result;
            }
        }

        private static string CreateTemporaryPath(string source)
        {
            // Note: this is obviously very Linux-specific. We need to figure out a good way to do this on macOS and
            // Windows as well.
            string xdgRuntimeDir = Environment.GetEnvironmentVariable("XDG_RUNTIME_DIR");
            string perlangTempDir = Path.Join(xdgRuntimeDir, "perlang", "tmp", "unit_tests");

            Directory.CreateDirectory(perlangTempDir);

            // The idea is to use a hashed version of the source as the file name, which means that a given Perlang
            // program will reliably always produce the same hash => can use the caching mechanism already in place to
            // avoid meaningless recompilation. (Might want to add a `make cache_clear` target or similar.)
            byte[] sourceAsBytes = Encoding.UTF8.GetBytes(source);
            byte[] sourceHash = SHA256.HashData(sourceAsBytes);
            return Path.Join(perlangTempDir, Convert.ToHexString(sourceHash) + ".per");
        }

        /// <summary>
        /// Evaluates the provided expression or list of statements. If the expression or statements prints to the
        /// standard output, the content will be returned as a collection of strings (one string per line printed).
        ///
        /// This method will propagate all errors to the caller. Note that compiler warnings will also be propagated as
        /// exceptions to the caller; in other words, this resembles the `-Werror` flag being enabled.
        /// </summary>
        /// <param name="source">A valid Perlang program.</param>
        /// <param name="arguments">Zero or more arguments to be passed to the program.</param>
        /// <returns>The output from the provided expression/statements.</returns>
        internal static IEnumerable<string> EvalReturningOutput(string source, params string[] arguments)
        {
            var result = EvalWithResult(source, arguments);

            if (result.CompilerWarnings.Count > 0)
            {
                // Only the first warning will be thrown. This is still better than ignoring the warnings altogether.
                // For scenarios where more control over compiler warnings is desired, use `EvalWithResult` or one of
                // the `EvalWith*ErrorCatch` methods.
                throw result.CompilerWarnings[0];
            }

            return result.Output;
        }

        /// <summary>
        /// Evaluates the provided expression or list of statements. If the expression or statements prints to the
        /// standard output, the content will be returned as a string with LF as line separator between each line.
        ///
        /// This method will propagate all errors to the caller. Note that compiler warnings will also be propagated as
        /// exceptions to the caller; in other words, this resembles the `-Werror` flag being enabled.
        /// </summary>
        /// <param name="source">A valid Perlang program.</param>
        /// <param name="arguments">Zero or more arguments to be passed to the program.</param>
        /// <returns>The output from the provided expression/statements.</returns>
        internal static string EvalReturningOutputString(string source, params string[] arguments)
        {
            return String.Join("\n", EvalReturningOutput(source, arguments));
        }

        /// <summary>
        /// Evaluates the provided expression or list of statements, with the provided arguments.
        ///
        /// This method will propagate all errors to the caller. Note that compiler warnings will also be propagated as
        /// exceptions to the caller; in other words, this resembles the `-Werror` flag being enabled.
        /// </summary>
        /// <param name="source">A valid Perlang program.</param>
        /// <param name="arguments">Zero or more arguments to be passed to the program.</param>
        /// <returns>The result of evaluating the provided expression, or `null` if provided a list of statements or
        /// an invalid program.</returns>
        internal static object EvalWithArguments(string source, params string[] arguments)
        {
            return EvalWithResult(source, arguments).Value;
        }

        private static void AssertFailScanErrorHandler(ScanError scanError)
        {
            // Note: does not contain stack trace, so no point in wrapping in Exception() at this point.
            throw scanError;
        }

        private static void AssertFailParseErrorHandler(ParseError parseError)
        {
            // Note: does not contain stack trace, so no point in wrapping in Exception() at this point.
            throw parseError;
        }

        private static void AssertFailNameResolutionErrorHandler(NameResolutionError nameResolutionError)
        {
            throw new EvalException($"NameResolutionError occurred: {nameResolutionError.Message}", nameResolutionError);
        }

        private static void AssertFailRuntimeErrorHandler(RuntimeError runtimeError)
        {
            throw new EvalException($"RuntimeError occurred: {runtimeError.Message}", runtimeError);
        }

        private static void AssertFailValidationErrorHandler(ValidationError validationError)
        {
            throw new EvalException($"ValidationError occurred: {validationError.Message}", validationError);
        }

        private static bool AssertFailCompilerWarningHandler(CompilerWarning compilerWarning)
        {
            throw new EvalException($"CompilerWarning occurred: {compilerWarning.Message}", compilerWarning);
        }
    }
}
