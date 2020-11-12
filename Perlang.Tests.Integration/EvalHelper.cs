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
        /// </summary>
        /// <param name="source">A valid Perlang program.</param>
        /// <returns>The result of evaluating the provided expression, or `null` if provided a list of statements.</returns>
        internal static object Eval(string source)
        {
            var interpreter = new PerlangInterpreter(AssertFailRuntimeErrorHandler);

            return interpreter.Eval(
                source,
                AssertFailScanErrorHandler,
                AssertFailParseErrorHandler,
                AssertFailResolveErrorHandler,
                AssertFailValidationErrorHandler,
                AssertFailValidationErrorHandler
            );
        }

        /// <summary>
        /// Evaluates the provided expression or list of statements, returning an <see cref="EvalResult{T}"/> with <see
        /// cref="EvalResult{T}.Value"/> set to the evaluated value.
        ///
        /// This method will propagate all errors apart from  <see cref="RuntimeError"/> to the caller. Runtime errors
        /// will be available in the returned <see cref="EvalResult{T}"/>.
        /// </summary>
        /// <param name="source">A valid Perlang program.</param>
        /// <param name="arguments">Zero or more arguments to be passed to the program.</param>
        /// <returns>An <see cref="EvalResult{T}"/> with the <see cref="EvalResult{T}.Value"/> property set to the
        /// result of the provided expression. If not provided a valid expression, <see cref="EvalResult{T}.Value"/>
        /// will be set to `null`.</returns>
        internal static EvalResult<RuntimeError> EvalWithRuntimeCatch(string source, params string[] arguments)
        {
            var result = new EvalResult<RuntimeError>();
            var interpreter =
                new PerlangInterpreter(runtimeError => result.Errors.Add(runtimeError), null, arguments);

            result.Value = interpreter.Eval(
                source,
                AssertFailScanErrorHandler,
                AssertFailParseErrorHandler,
                AssertFailResolveErrorHandler,
                AssertFailValidationErrorHandler,
                AssertFailValidationErrorHandler
            );

            return result;
        }

        /// <summary>
        /// Evaluates the provided expression or list of statements, returning an <see cref="EvalResult{T}"/> with <see
        /// cref="EvalResult{T}.Value"/> set to the evaluated value.
        ///
        /// This method will propagate all errors apart from  <see cref="ParseError"/> to the caller. Parse errors
        /// will be available in the returned <see cref="EvalResult{T}.Errors"/> property.
        /// </summary>
        /// <param name="source">A valid Perlang program.</param>
        /// <returns>An <see cref="EvalResult{T}"/> with the <see cref="EvalResult{T}.Value"/> property set to the
        /// result of the provided expression. If not provided a valid expression, <see cref="EvalResult{T}.Value"/>
        /// will be set to `null`.</returns>
        internal static EvalResult<ParseError> EvalWithParseErrorCatch(string source)
        {
            var result = new EvalResult<ParseError>();
            var interpreter = new PerlangInterpreter(AssertFailRuntimeErrorHandler);

            result.Value = interpreter.Eval(
                source,
                AssertFailScanErrorHandler,
                parseError => result.Errors.Add(parseError),
                AssertFailResolveErrorHandler,
                AssertFailValidationErrorHandler,
                AssertFailValidationErrorHandler
            );

            return result;
        }

        /// <summary>
        /// Evaluates the provided expression or list of statements, returning an <see cref="EvalResult{T}"/> with <see
        /// cref="EvalResult{T}.Value"/> set to the evaluated value.
        ///
        /// This method will propagate all errors apart from  <see cref="ResolveError"/> to the caller. Resolve errors
        /// will be available in the returned <see cref="EvalResult{T}.Errors"/> property.
        /// </summary>
        /// <param name="source">A valid Perlang program.</param>
        /// <returns>An <see cref="EvalResult{T}"/> with the <see cref="EvalResult{T}.Value"/> property set to the
        /// result of the provided expression. If not provided a valid expression, <see cref="EvalResult{T}.Value"/>
        /// will be set to `null`.</returns>
        internal static EvalResult<ResolveError> EvalWithResolveErrorCatch(string source)
        {
            var result = new EvalResult<ResolveError>();
            var interpreter = new PerlangInterpreter(AssertFailRuntimeErrorHandler);

            result.Value = interpreter.Eval(
                source,
                AssertFailScanErrorHandler,
                AssertFailParseErrorHandler,
                resolveError => result.Errors.Add(resolveError),
                AssertFailValidationErrorHandler,
                AssertFailValidationErrorHandler
            );

            return result;
        }

        /// <summary>
        /// Evaluates the provided expression or list of statements, returning an <see cref="EvalResult{T}"/> with <see
        /// cref="EvalResult{T}.Value"/> set to the evaluated value.
        ///
        /// This method will propagate all errors apart from  <see cref="ValidationError"/> to the caller. Validation
        /// errors will be available in the returned <see cref="EvalResult{T}.Errors"/> property.
        /// </summary>
        /// <param name="source">A valid Perlang program.</param>
        /// <returns>An <see cref="EvalResult{T}"/> with the <see cref="EvalResult{T}.Value"/> property set to the
        /// result of the provided expression. If not provided a valid expression, <see cref="EvalResult{T}.Value"/>
        /// will be set to `null`.</returns>
        internal static EvalResult<ValidationError> EvalWithValidationErrorCatch(string source)
        {
            var result = new EvalResult<ValidationError>();
            var interpreter = new PerlangInterpreter(AssertFailRuntimeErrorHandler);

            result.Value = interpreter.Eval(
                source,
                AssertFailScanErrorHandler,
                AssertFailParseErrorHandler,
                AssertFailResolveErrorHandler,
                validationError => result.Errors.Add(validationError),
                validationError => result.Errors.Add(validationError)
            );

            return result;
        }

        /// <summary>
        /// Evaluates the provided expression or list of statements. If the expression or statements prints to the
        /// standard output, the content will be returned as a collection of strings (one string per line printed).
        /// </summary>
        /// <param name="source">A valid Perlang program.</param>
        /// <param name="arguments">Zero or more arguments to be passed to the program.</param>
        /// <returns>The output from the provided expression/statements.</returns>
        internal static IEnumerable<string> EvalReturningOutput(string source, params string[] arguments)
        {
            var output = new List<string>();
            var interpreter = new PerlangInterpreter(AssertFailRuntimeErrorHandler, s => output.Add(s), arguments);

            interpreter.Eval(
                source,
                AssertFailScanErrorHandler,
                AssertFailParseErrorHandler,
                AssertFailResolveErrorHandler,
                AssertFailValidationErrorHandler,
                AssertFailValidationErrorHandler
            );

            return output;
        }

        /// <summary>
        /// Evaluates the provided expression or list of statements. If the expression or statements prints to the
        /// standard output, the content will be returned as a string with LF as line separator between each line.
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
        /// </summary>
        /// <param name="source">A valid Perlang program.</param>
        /// <param name="arguments">Zero or more arguments to be passed to the program.</param>
        /// <returns>The result of evaluating the provided expression, or `null` if provided a list of statements or
        /// an invalid program.</returns>
        internal static object EvalWithArguments(string source, params string[] arguments)
        {
            return EvalWithArguments(source, standardOutputHandler: null, arguments);
        }

        /// <summary>
        /// Evaluates the provided expression or list of statements, with the provided arguments.
        /// </summary>
        /// <param name="source">A valid Perlang program.</param>
        /// <param name="standardOutputHandler">An optional parameter that will receive output printed to standard
        /// output. If not provided or null, output will be printed to the standard output of the running
        /// process.</param>
        /// <param name="arguments">Zero or more arguments to be passed to the program.</param>
        /// <returns>The result of evaluating the provided expression, or `null` if provided a list of statements or
        /// an invalid program.</returns>
        internal static object EvalWithArguments(string source, Action<string> standardOutputHandler, params string[] arguments)
        {
            var interpreter = new PerlangInterpreter(AssertFailRuntimeErrorHandler, standardOutputHandler, arguments);

            return interpreter.Eval(
                source,
                AssertFailScanErrorHandler,
                AssertFailParseErrorHandler,
                AssertFailResolveErrorHandler,
                AssertFailValidationErrorHandler,
                AssertFailValidationErrorHandler
            );
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
    }
}
