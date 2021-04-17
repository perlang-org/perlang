using System;
using System.Collections.Generic;
using Perlang.Interpreter;
using Perlang.Interpreter.Resolution;
using Perlang.Parser;

namespace Perlang.Tests.Integration
{
    internal static class EvalHelper
    {
        /// <summary>
        /// Evaluates the provided expression or list of statements, returning the evaluated value.
        ///
        /// This method will propagate both scanner, parser, resolver and runtime errors to the caller. If multiple
        /// errors are encountered, only the first will be thrown.
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
            var interpreter = new PerlangInterpreter(AssertFailRuntimeErrorHandler, _ => { });

            return interpreter.Eval(
                source,
                AssertFailScanErrorHandler,
                AssertFailParseErrorHandler,
                AssertFailResolveErrorHandler,
                AssertFailValidationErrorHandler,
                AssertFailValidationErrorHandler,
                AssertFailCompilerWarningHandler
            );
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
                result.ErrorHandler, result.OutputHandler, arguments
            );

            result.Value = interpreter.Eval(
                source,
                AssertFailScanErrorHandler,
                AssertFailParseErrorHandler,
                AssertFailResolveErrorHandler,
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
                AssertFailResolveErrorHandler,
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
        /// This method will propagate all errors apart from  <see cref="ResolveError"/> to the caller. Resolve errors
        /// will be available in the returned <see cref="EvalResult{T}.Errors"/> property.
        ///
        /// If any warnings are emitted, they will be available in the returned <see
        /// cref="EvalResult{T}.CompilerWarnings"/> property. "Warnings as errors" will be disabled for all warnings.
        /// </summary>
        /// <param name="source">A valid Perlang program.</param>
        /// <returns>An <see cref="EvalResult{T}"/> with the <see cref="EvalResult{T}.Value"/> property set to the
        /// result of the provided expression. If not provided a valid expression, <see cref="EvalResult{T}.Value"/>
        /// will be set to `null`.</returns>
        internal static EvalResult<ResolveError> EvalWithResolveErrorCatch(string source)
        {
            var result = new EvalResult<ResolveError>();
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
                AssertFailResolveErrorHandler,
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
        /// cref="EvalResult{T}.CompilerWarnings"/> property. "Warnings as errors" will be disabled for all warnings.
        /// </summary>
        /// <param name="source">A valid Perlang program.</param>
        /// <param name="arguments">Zero or more arguments to be passed to the program.</param>
        /// <returns>An <see cref="EvalResult{T}"/> with the <see cref="EvalResult{T}.Value"/> property set to the
        /// result of the provided expression. If not provided a valid expression, <see cref="EvalResult{T}.Value"/>
        /// will be set to `null`.</returns>
        internal static EvalResult<Exception> EvalWithResult(string source, params string[] arguments)
        {
            var result = new EvalResult<Exception>();
            var interpreter = new PerlangInterpreter(AssertFailRuntimeErrorHandler, result.OutputHandler, arguments);

            result.Value = interpreter.Eval(
                source,
                AssertFailScanErrorHandler,
                AssertFailParseErrorHandler,
                AssertFailResolveErrorHandler,
                AssertFailValidationErrorHandler,
                AssertFailValidationErrorHandler,
                result.WarningHandler
            );

            return result;
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
            throw scanError;
        }

        private static void AssertFailParseErrorHandler(ParseError parseError)
        {
            throw parseError;
        }

        private static void AssertFailResolveErrorHandler(ResolveError resolveError)
        {
            throw resolveError;
        }

        private static void AssertFailRuntimeErrorHandler(RuntimeError runtimeError)
        {
            throw runtimeError;
        }

        private static void AssertFailValidationErrorHandler(ValidationError validationError)
        {
            throw validationError;
        }

        private static bool AssertFailCompilerWarningHandler(CompilerWarning compilerWarning)
        {
            throw compilerWarning;
        }
    }
}
