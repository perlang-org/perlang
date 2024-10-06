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
        /// to make assertions based on the output printed, see e.g. the
        /// <see cref="EvalReturningOutput(string, string[])"/> method.
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
                // Interpreted mode no longer exists. We may re-introduce it based on LLVM, but it's uncertain if it will
                // become a special mode or not. If it does, the code for handling it should be added here.
                throw new NotImplementedException("Evaluating and returning a result is not yet supported in interpreted mode");
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
            if (PerlangMode.ExperimentalCompilation)
            {
                var result = new EvalResult<RuntimeError>();

                var compiler = new PerlangCompiler(
                    result.ErrorHandler, result.OutputHandler, null, arguments
                );

                try
                {
                    result.Value = compiler.CompileAndRun(
                        source,
                        CreateTemporaryPath(source),
                        targetPath: null,
                        CompilerFlags.None,
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
                // Interpreted mode no longer exists. We may re-introduce it based on LLVM, but it's uncertain if it will
                // become a special mode or not. If it does, the code for handling it should be added here.
                throw new NotImplementedException("EvalWithRuntimeErrorCatch is not yet supported in interpreted mode");
            }
        }

        /// <summary>
        /// Evaluates the provided expression or list of statements, returning an <see cref="EvalResult{T}"/> with <see
        /// cref="EvalResult{T}.Value"/> set to the evaluated value.
        ///
        /// Output printed to the standard output stream will be available in <see cref="EvalResult{T}.Output"/>.
        ///
        /// This method will propagate all errors apart from  <see cref="PerlangCompilerException"/> to the caller.
        /// PerlangCompilerException errors will be available in the returned <see cref="EvalResult{T}"/>.
        ///
        /// If any warnings are emitted, they will be available in the returned <see
        /// cref="EvalResult{T}.CompilerWarnings"/> property. "Warnings as errors" will be disabled for all warnings.
        /// </summary>
        /// <param name="source">A valid Perlang program.</param>
        /// <param name="arguments">Zero or more arguments to be passed to the program.</param>
        /// <returns>An <see cref="EvalResult{T}"/> with the <see cref="EvalResult{T}.Value"/> property set to the
        /// result of the provided expression. If not provided a valid expression, <see cref="EvalResult{T}.Value"/>
        /// will be set to `null`.</returns>
        internal static EvalResult<PerlangCompilerException> EvalWithCppCompilationErrorCatch(string source, params string[] arguments)
        {
            if (PerlangMode.ExperimentalCompilation) {
                var result = new EvalResult<PerlangCompilerException>();

                var compiler = new PerlangCompiler(
                    AssertFailRuntimeErrorHandler, result.OutputHandler, null, arguments
                );

                try {
                    result.Value = compiler.CompileAndRun(
                        source,
                        CreateTemporaryPath(source),
                        targetPath: null,
                        CompilerFlags.None,
                        AssertFailScanErrorHandler,
                        AssertFailParseErrorHandler,
                        AssertFailNameResolutionErrorHandler,
                        AssertFailValidationErrorHandler,
                        AssertFailValidationErrorHandler,
                        result.WarningHandler
                    );
                }
                catch (PerlangCompilerException e) {
                    result.ErrorHandler(e);
                    return result;
                }

                // Return something else than `null` to make it reasonable for callers to distinguish that compiled mode
                // (with no native "result") is being used, if needed.
                result.Value = VoidObject.Void;

                return result;
            }
            else {
                // Interpreted mode no longer exists. We may re-introduce it based on LLVM, but it's uncertain if it will
                // become a special mode or not. If it does, the code for handling it should be added here.
                throw new NotImplementedException("EvalWithRuntimeErrorCatch is not yet supported in interpreted mode");
            }
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
            if (PerlangMode.ExperimentalCompilation)
            {
                var result = new EvalResult<ScanError>();

                var compiler = new PerlangCompiler(AssertFailRuntimeErrorHandler, result.OutputHandler);

                try
                {
                    result.Value = compiler.CompileAndRun(
                        source,
                        CreateTemporaryPath(source),
                        targetPath: null,
                        CompilerFlags.None,
                        result.ErrorHandler,
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
            else {
                // Interpreted mode no longer exists. We may re-introduce it based on LLVM, but it's uncertain if it will
                // become a special mode or not. If it does, the code for handling it should be added here.
                throw new NotImplementedException("EvalWithScanErrorCatch is not yet supported in interpreted mode");
            }
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
            if (PerlangMode.ExperimentalCompilation) {
                var result = new EvalResult<ParseError>();

                var compiler = new PerlangCompiler(AssertFailRuntimeErrorHandler, result.OutputHandler);

                try {
                    result.Value = compiler.CompileAndRun(
                        source,
                        CreateTemporaryPath(source),
                        targetPath: null,
                        CompilerFlags.None,
                        AssertFailScanErrorHandler,
                        result.ErrorHandler,
                        AssertFailNameResolutionErrorHandler,
                        AssertFailValidationErrorHandler,
                        AssertFailValidationErrorHandler,
                        result.WarningHandler
                    );
                }
                catch (NotImplementedInCompiledModeException e) {
                    // This exception is thrown to make it possible for integration tests to skip tests for code which
                    // is known to not yet work.
                    throw new SkipException(e.Message, e);
                }

                // Return something else than `null` to make it reasonable for callers to distinguish that compiled mode
                // (with no native "result") is being used, if needed.
                result.Value = VoidObject.Void;

                return result;
            }
            else {
                // Interpreted mode no longer exists. We may re-introduce it based on LLVM, but it's uncertain if it will
                // become a special mode or not. If it does, the code for handling it should be added here.
                throw new NotImplementedException("EvalWithParseErrorCatch is not yet supported in interpreted mode");
            }
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
            if (PerlangMode.ExperimentalCompilation) {
                var result = new EvalResult<NameResolutionError>();

                var compiler = new PerlangCompiler(AssertFailRuntimeErrorHandler, result.OutputHandler);

                try {
                    result.Value = compiler.CompileAndRun(
                        source,
                        CreateTemporaryPath(source),
                        targetPath: null,
                        CompilerFlags.None,
                        AssertFailScanErrorHandler,
                        AssertFailParseErrorHandler,
                        result.ErrorHandler,
                        AssertFailValidationErrorHandler,
                        AssertFailValidationErrorHandler,
                        result.WarningHandler
                    );
                }
                catch (NotImplementedInCompiledModeException e) {
                    // This exception is thrown to make it possible for integration tests to skip tests for code which
                    // is known to not yet work.
                    throw new SkipException(e.Message, e);
                }

                // Return something else than `null` to make it reasonable for callers to distinguish that compiled mode
                // (with no native "result") is being used, if needed.
                result.Value = VoidObject.Void;

                return result;
            }
            else {
                // Interpreted mode no longer exists. We may re-introduce it based on LLVM, but it's uncertain if it will
                // become a special mode or not. If it does, the code for handling it should be added here.
                throw new NotImplementedException("EvalWithNameResolutionErrorCatch is not yet supported in interpreted mode");
            }
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
            if (PerlangMode.ExperimentalCompilation) {
                var result = new EvalResult<ValidationError>();

                var compiler = new PerlangCompiler(AssertFailRuntimeErrorHandler, result.OutputHandler);

                try {
                    result.Value = compiler.CompileAndRun(
                        source,
                        CreateTemporaryPath(source),
                        targetPath: null,
                        CompilerFlags.None,
                        AssertFailScanErrorHandler,
                        AssertFailParseErrorHandler,
                        AssertFailNameResolutionErrorHandler,
                        result.ErrorHandler,
                        result.ErrorHandler,
                        result.WarningHandler
                    );
                }
                catch (NotImplementedInCompiledModeException e) {
                    // This exception is thrown to make it possible for integration tests to skip tests for code which
                    // is known to not yet work.
                    throw new SkipException(e.Message, e);
                }

                // Return something else than `null` to make it reasonable for callers to distinguish that compiled mode
                // (with no native "result") is being used, if needed.
                result.Value = VoidObject.Void;

                return result;
            }
            else {
                // Interpreted mode no longer exists. We may re-introduce it based on LLVM, but it's uncertain if it will
                // become a special mode or not. If it does, the code for handling it should be added here.
                throw new NotImplementedException("EvalWithValidationErrorCatch is not yet supported in interpreted mode");
            }
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
                        targetPath: null,
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
                // Interpreted mode no longer exists. We may re-introduce it based on LLVM, but it's uncertain if it will
                // become a special mode or not. If it does, the code for handling it should be added here.
                throw new NotImplementedException("EvalWithResult is not yet supported in interpreted mode");
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
            return EvalReturningOutput(source, CompilerFlags.None, arguments);
        }

        /// <summary>
        /// Evaluates the provided expression or list of statements. If the expression or statements prints to the
        /// standard output, the content will be returned as a collection of strings (one string per line printed).
        ///
        /// This method will propagate all errors to the caller. Note that compiler warnings will also be propagated as
        /// exceptions to the caller; in other words, this resembles the `-Werror` flag being enabled.
        /// </summary>
        /// <param name="source">A valid Perlang program.</param>
        /// <param name="compilerFlags">The <see cref="CompilerFlags"/> to use when performing the compilation.</param>
        /// <param name="arguments">Zero or more arguments to be passed to the program.</param>
        /// <returns>The output from the provided expression/statements.</returns>
        internal static IEnumerable<string> EvalReturningOutput(string source, CompilerFlags compilerFlags, params string[] arguments)
        {
            var result = EvalWithResult(source, compilerFlags, arguments);

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
            // Note: does not contain stack trace, but wrapping in Exception here means we can use the ToString() method
            // which provides valuable debugging information to the caller.
            throw new Exception(parseError.ToString());
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
    }
}
