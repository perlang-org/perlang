#pragma warning disable S3963
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
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
        private static readonly CompilerFlags DefaultCompilerFlags = CompilerFlags.None;

        static EvalHelper()
        {
            if (PerlangMode.RunWithValgrind)
            {
                DefaultCompilerFlags |= CompilerFlags.RunWithValgrind;
            }
        }

        /// <summary>
        /// Evaluates the provided expression or list of statements, returning the evaluated value.
        ///
        /// This method will propagate all types of errors to the caller. If multiple errors are encountered, only the
        /// first will be thrown.
        ///
        /// Output printed to the standard output stream will be silently discarded by this method. For tests which need
        /// to make assertions based on the output printed, see e.g. the
        /// <see cref="EvalReturningOutput(string, string[], string)"/> method.
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
        /// <param name="arguments">An optional array of arguments to be passed to the program.</param>
        /// <param name="callerMethod">The name of the calling method.</param>
        /// <returns>An <see cref="EvalResult{T}"/> with the <see cref="EvalResult{T}.Value"/> property set to the
        /// result of the provided expression. If not provided a valid expression, <see cref="EvalResult{T}.Value"/>
        /// will be set to `null`.</returns>
        internal static EvalResult<RuntimeError> EvalWithRuntimeErrorCatch(string source, string[] arguments = null, [CallerMemberName]string callerMethod = null)
        {
            if (PerlangMode.ExperimentalCompilation)
            {
                var result = new EvalResult<RuntimeError>();

                using var compiler = new PerlangCompiler(
                    result.ErrorHandler, result.OutputHandler, null
                );

                try
                {
                    result.Value = compiler.CompileAndRun(
                        source,
                        CreateTemporaryPath(callerMethod, source),
                        targetPath: null,
                        DefaultCompilerFlags,
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
        /// <param name="arguments">An optional array of arguments to be passed to the program.</param>
        /// <param name="callerMethod">The name of the calling method.</param>
        /// <returns>An <see cref="EvalResult{T}"/> with the <see cref="EvalResult{T}.Value"/> property set to the
        /// result of the provided expression. If not provided a valid expression, <see cref="EvalResult{T}.Value"/>
        /// will be set to `null`.</returns>
        internal static EvalResult<PerlangCompilerException> EvalWithCppCompilationErrorCatch(string source, string[] arguments = null, [CallerMemberName]string callerMethod = null)
        {
            if (PerlangMode.ExperimentalCompilation) {
                var result = new EvalResult<PerlangCompilerException>();

                using var compiler = new PerlangCompiler(
                    AssertFailRuntimeErrorHandler, result.OutputHandler, null
                );

                try {
                    result.Value = compiler.CompileAndRun(
                        source,
                        CreateTemporaryPath(callerMethod, source),
                        targetPath: null,
                        DefaultCompilerFlags,
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
        /// <param name="callerMethod">The name of the calling method.</param>
        /// <returns>An <see cref="EvalResult{T}"/> with the <see cref="EvalResult{T}.Value"/> property set to the
        /// result of the provided expression. If not provided a valid expression, <see cref="EvalResult{T}.Value"/>
        /// will be set to `null`.</returns>
        internal static EvalResult<ScanError> EvalWithScanErrorCatch(string source, [CallerMemberName]string callerMethod = null)
        {
            if (PerlangMode.ExperimentalCompilation)
            {
                var result = new EvalResult<ScanError>();

                using var compiler = new PerlangCompiler(AssertFailRuntimeErrorHandler, result.OutputHandler);

                try
                {
                    result.Value = compiler.CompileAndRun(
                        source,
                        CreateTemporaryPath(callerMethod, source),
                        targetPath: null,
                        DefaultCompilerFlags,
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
        /// <param name="callerMethod">The name of the calling method.</param>
        /// <returns>An <see cref="EvalResult{T}"/> with the <see cref="EvalResult{T}.Value"/> property set to the
        /// result of the provided expression. If not provided a valid expression, <see cref="EvalResult{T}.Value"/>
        /// will be set to `null`.</returns>
        internal static EvalResult<ParseError> EvalWithParseErrorCatch(string source, [CallerMemberName]string callerMethod = null)
        {
            if (PerlangMode.ExperimentalCompilation) {
                var result = new EvalResult<ParseError>();

                using var compiler = new PerlangCompiler(AssertFailRuntimeErrorHandler, result.OutputHandler);

                try {
                    result.Value = compiler.CompileAndRun(
                        source,
                        CreateTemporaryPath(callerMethod, source),
                        targetPath: null,
                        DefaultCompilerFlags,
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
        /// <param name="callerMethod">The name of the calling method.</param>
        /// <returns>An <see cref="EvalResult{T}"/> with the <see cref="EvalResult{T}.Value"/> property set to the
        /// result of the provided expression. If not provided a valid expression, <see cref="EvalResult{T}.Value"/>
        /// will be set to `null`.</returns>
        internal static EvalResult<NameResolutionError> EvalWithNameResolutionErrorCatch(string source, [CallerMemberName]string callerMethod = null)
        {
            if (PerlangMode.ExperimentalCompilation) {
                var result = new EvalResult<NameResolutionError>();

                using var compiler = new PerlangCompiler(AssertFailRuntimeErrorHandler, result.OutputHandler);

                try {
                    result.Value = compiler.CompileAndRun(
                        source,
                        CreateTemporaryPath(callerMethod, source),
                        targetPath: null,
                        DefaultCompilerFlags,
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
        /// <param name="callerMethod">The name of the calling method.</param>
        /// <returns>An <see cref="EvalResult{T}"/> with the <see cref="EvalResult{T}.Value"/> property set to the
        /// result of the provided expression. If not provided a valid expression, <see cref="EvalResult{T}.Value"/>
        /// will be set to `null`.</returns>
        internal static EvalResult<ValidationError> EvalWithValidationErrorCatch(string source, [CallerMemberName]string callerMethod = null)
        {
            if (PerlangMode.ExperimentalCompilation) {
                var result = new EvalResult<ValidationError>();

                using var compiler = new PerlangCompiler(AssertFailRuntimeErrorHandler, result.OutputHandler);

                try {
                    result.Value = compiler.CompileAndRun(
                        source,
                        CreateTemporaryPath(callerMethod, source),
                        targetPath: null,
                        DefaultCompilerFlags,
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
        /// <param name="arguments">An optional array of arguments to be passed to the program.</param>
        /// <param name="callerMethod">The name of the calling method.</param>
        /// <returns>An <see cref="EvalResult{T}"/> with the <see cref="EvalResult{T}.Value"/> property set to the
        /// result of the provided expression. If not provided a valid expression, <see cref="EvalResult{T}.Value"/>
        /// will be set to `null`.</returns>
        internal static EvalResult<Exception> EvalWithResult(string source, string[] arguments = null, [CallerMemberName]string callerMethod = null)
        {
            return EvalWithResult(source, DefaultCompilerFlags, arguments, callerMethod);
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
        /// <param name="arguments">An optional array of arguments to be passed to the program.</param>
        /// <param name="callerMethod">The name of the calling method.</param>
        /// <returns>An <see cref="EvalResult{T}"/> with the <see cref="EvalResult{T}.Value"/> property set to the
        /// result of the provided expression. If not provided a valid expression, <see cref="EvalResult{T}.Value"/>
        /// will be set to `null`.</returns>
        internal static EvalResult<Exception> EvalWithResult(string source, CompilerFlags compilerFlags, string[] arguments = null, [CallerMemberName]string callerMethod = null)
        {
            if (PerlangMode.ExperimentalCompilation) {
                var result = new EvalResult<Exception>();
                using var compiler = new PerlangCompiler(AssertFailRuntimeErrorHandler, result.OutputHandler, null);

                try {
                    result.ExecutablePath = compiler.CompileAndRun(
                        source,
                        CreateTemporaryPath(callerMethod, source),
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
                catch (NotImplementedInCompiledModeException e) {
                    // This exception is thrown to make it possible for integration tests to skip tests for code which
                    // is known to not yet work.
                    throw new SkipException(e.Message, e);
                }
                catch (EvalException e) {
                    // We must make sure to include any output already written by the CompileAndRun() method here, since
                    // it'll otherwise be indefinitely lost.
                    throw new EvalException($"An exception occurred during evaluation. Process output:\n\n{result.OutputAsString}", e);
                }

                // Return something else than `null` to make it reasonable for callers to distinguish that compiled mode
                // (with no native "result") is being used, if needed.
                result.Value = VoidObject.Void;

                return result;
            }
            else {
                // Interpreted mode no longer exists. We may re-introduce it based on LLVM, but it's uncertain if it will
                // become a special mode or not. If it does, the code for handling it should be added here.
                throw new NotImplementedException("EvalWithResult is not yet supported in interpreted mode");
            }
        }

        private static string CreateTemporaryPath(string testMethodName, string source)
        {
            // Note: this is obviously very Linux-specific. We need to figure out a good way to do this on macOS and
            // Windows as well.
            string xdgRuntimeDir = Environment.GetEnvironmentVariable("XDG_RUNTIME_DIR");
            string perlangTempDir = Path.Join(xdgRuntimeDir, "perlang", "tmp", "unit_tests");

            Directory.CreateDirectory(perlangTempDir);

            // The idea is to include a hashed version of the source as the file name, which means that a given Perlang
            // program will reliably always produce the same hash => can use the caching mechanism already in place to
            // avoid meaningless recompilation. (Might want to add a `make cache_clear` target or similar.)
            //
            // The calling method is also included in the file name, to make it easier to find the matching .cc file for
            // a particular test case.
            byte[] sourceAsBytes = Encoding.UTF8.GetBytes(source);
            byte[] sourceHash = SHA256.HashData(sourceAsBytes);
            return Path.Join(perlangTempDir, $"{testMethodName}-{Convert.ToHexString(sourceHash)}.per");
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
        /// <param name="callerMethod">The name of the calling method.</param>
        /// <returns>The output from the provided expression/statements.</returns>
        internal static IEnumerable<string> EvalReturningOutput(string source, string[] arguments = null, [CallerMemberName]string callerMethod = null)
        {
            return EvalReturningOutput(source, DefaultCompilerFlags, arguments, callerMethod);
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
        /// <param name="callerMethod">The name of the calling method.</param>
        /// <returns>The output from the provided expression/statements.</returns>
        internal static IEnumerable<string> EvalReturningOutput(string source, CompilerFlags compilerFlags, string[] arguments = null, [CallerMemberName]string callerMethod = null)
        {
            var result = EvalWithResult(source, compilerFlags, arguments, callerMethod);

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
        /// <param name="callerMethod">The name of the calling method.</param>
        /// <returns>The output from the provided expression/statements.</returns>
        internal static string EvalReturningOutputString(string source, string[] arguments = null, [CallerMemberName]string callerMethod = null)
        {
            return String.Join("\n", EvalReturningOutput(source, arguments, callerMethod));
        }

        /// <summary>
        /// Evaluates the provided expression or list of statements, with the provided arguments.
        ///
        /// This method will propagate all errors to the caller. Note that compiler warnings will also be propagated as
        /// exceptions to the caller; in other words, this resembles the `-Werror` flag being enabled.
        /// </summary>
        /// <param name="source">A valid Perlang program.</param>
        /// <param name="arguments">Zero or more arguments to be passed to the program.</param>
        /// <param name="callerMethod">The name of the calling method.</param>
        /// <returns>The result of evaluating the provided expression, or `null` if provided a list of statements or
        /// an invalid program.</returns>
        internal static object EvalWithArguments(string source, string[] arguments, [CallerMemberName]string callerMethod = null)
        {
            return EvalWithResult(source, arguments, callerMethod).Value;
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
            if (nameResolutionError.StackTrace != null) {
                throw new EvalException($"Name resolution error: {nameResolutionError.Message}. See inner exception for details.", nameResolutionError);
            }
            else {
                throw new EvalException($"Name resolution error: {nameResolutionError.Message}");
            }
        }

        private static void AssertFailRuntimeErrorHandler(RuntimeError runtimeError)
        {
            if (runtimeError.StackTrace != null) {
                throw new EvalException($"Runtime error: {runtimeError.Message}. See inner exception for details.", runtimeError);
            }
            else {
                throw new EvalException($"Runtime error: {runtimeError.Message}");
            }
        }

        private static void AssertFailValidationErrorHandler(ValidationError validationError)
        {
            if (validationError.StackTrace != null) {
                throw new EvalException($"Validation error: {validationError.Message}. See inner exception for details.");
            }
            else {
                throw new EvalException($"Validation error: {validationError.Message}");
            }
        }
    }
}
