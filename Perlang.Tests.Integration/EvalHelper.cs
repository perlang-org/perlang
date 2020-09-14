using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;
using Perlang.Interpreter;
using Perlang.Interpreter.Resolution;
using Perlang.Interpreter.Typing;
using Perlang.Parser;

namespace Perlang.Tests.Integration
{
    internal static class EvalHelper
    {
        /// <summary>
        /// Evaluates the provided expression or list of statements. If provided an expression, returns the result;
        /// otherwise, returns null.
        ///
        /// This method will propagate both scanner, parser, resolver and runtime errors to the caller. If multiple
        /// errors are encountered, only the first will be thrown.
        /// </summary>
        /// <param name="source">a valid Perlang programs</param>
        /// <returns>the result of the provided expression, or null if not provided an expression.</returns>
        internal static object Eval(string source)
        {
            var interpreter = new PerlangInterpreter(AssertFailRuntimeErrorHandler);
            return interpreter.Eval(source, AssertFailScanErrorHandler, AssertFailParseErrorHandler,
                AssertFailResolveErrorHandler, AssertFailTypeValidationErrorHandler);
        }

        /// <summary>
        /// Evaluates the provided expression or list of statements. If provided an expression, EvalResult.Value
        /// contains the value of the evaluated expression; otherwise, this field will be false.
        ///
        /// This method will propagate all errors apart from runtime errors to the caller. Runtime errors will be
        /// available in the returned <see cref="EvalResult"/>.
        /// </summary>
        /// <param name="source">a valid Perlang programs</param>
        /// <param name="arguments">zero or more arguments to be passed to the program</param>
        /// <returns>an EvalResult with the result of the provided expression, or null if not provided an expression.</returns>
        internal static EvalResult EvalWithRuntimeCatch(string source, params string[] arguments)
        {
            var result = new EvalResult();
            var interpreter = new PerlangInterpreter(runtimeError => result.RuntimeErrors.Add(runtimeError), null, arguments);

            result.Value = interpreter.Eval(source, AssertFailScanErrorHandler, AssertFailParseErrorHandler,
                AssertFailResolveErrorHandler, AssertFailTypeValidationErrorHandler);

            return result;
        }

        /// <summary>
        /// Evaluates the provided expression or list of statements. If provided an expression, EvalResult.Value
        /// contains the value of the evaluated expression; otherwise, this field will be false.
        ///
        /// This method will propagate all errors apart from  <see cref="ParseError"/> to the caller. Runtime errors
        /// will be available in the returned <see cref="EvalResult"/>.
        /// </summary>
        /// <param name="source">a valid Perlang programs</param>
        /// <returns>an EvalResult with the result of the provided expression, or null if not provided an expression.</returns>
        internal static EvalResult EvalWithParseErrorCatch(string source)
        {
            var result = new EvalResult();
            var interpreter = new PerlangInterpreter(AssertFailRuntimeErrorHandler);

            result.Value = interpreter.Eval(source, AssertFailScanErrorHandler, parseError => result.ParseErrors.Add(parseError),
                AssertFailResolveErrorHandler, AssertFailTypeValidationErrorHandler);

            return result;
        }

        /// <summary>
        /// Evaluates the provided expression or list of statements. If provided an expression, EvalResult.Value
        /// contains the value of the evaluated expression; otherwise, this field will be false.
        ///
        /// This method will propagate all errors apart from  <see cref="ResolveError"/> to the caller. Runtime errors
        /// will be available in the returned <see cref="EvalResult"/>.
        /// </summary>
        /// <param name="source">a valid Perlang programs</param>
        /// <returns>an EvalResult with the result of the provided expression, or null if not provided an expression.</returns>
        internal static EvalResult EvalWithResolveErrorCatch(string source)
        {
            var result = new EvalResult();
            var interpreter = new PerlangInterpreter(AssertFailRuntimeErrorHandler);

            result.Value = interpreter.Eval(source, AssertFailScanErrorHandler, AssertFailParseErrorHandler,
                resolveError => result.ResolveErrors.Add(resolveError), AssertFailTypeValidationErrorHandler);

            return result;
        }

        internal static EvalResult EvalWithTypeValidationErrorCatch(string source)
        {
            var result = new EvalResult();
            var interpreter = new PerlangInterpreter(AssertFailRuntimeErrorHandler);

            result.Value = interpreter.Eval(source, AssertFailScanErrorHandler, AssertFailParseErrorHandler,
                AssertFailResolveErrorHandler, typeValidationError => result.TypeValidationErrors.Add(typeValidationError));

            return result;
        }

        /// <summary>
        /// Evaluates the provided expression or list of statements. If the expression or statements prints to the
        /// standard output, the content will be returned.
        /// </summary>
        /// <param name="source">a valid Perlang programs</param>
        /// <param name="arguments">zero or more arguments to be passed to the program</param>
        /// <returns>the output from the provided expression/statements.</returns>
        internal static IEnumerable<string> EvalReturningOutput(string source, params string[] arguments)
        {
            var output = new List<string>();
            var interpreter = new PerlangInterpreter(AssertFailRuntimeErrorHandler, s => output.Add(s), arguments);

            interpreter.Eval(source, AssertFailScanErrorHandler, AssertFailParseErrorHandler,
                AssertFailResolveErrorHandler, AssertFailTypeValidationErrorHandler);

            return output;
        }

        /// <summary>
        /// Evaluates the provided expression or list of statements, with the provided arguments
        /// </summary>
        /// <param name="source">a valid Perlang programs</param>
        /// <param name="arguments">zero or more arguments to be passed to the program</param>
        /// <returns>the result of the provided expression, or null if not provided an expression.</returns>
        internal static object EvalWithArguments(string source, params string[] arguments)
        {
            return EvalWithArguments(source, standardOutputHandler: null, arguments);
        }

        /// <summary>
        /// Evaluates the provided expression or list of statements, with the provided arguments
        /// </summary>
        /// <param name="source">a valid Perlang programs</param>
        /// <param name="standardOutputHandler">an optional parameter that will receive output printed to
        ///     standard output. If not provided or null, output will be printed to the standard output of the
        ///     running process</param>
        /// <param name="arguments">zero or more arguments to be passed to the program</param>
        /// <returns>the result of the provided expression, or null if not provided an expression.</returns>
        internal static object EvalWithArguments(string source, Action<string> standardOutputHandler, params string[] arguments)
        {
            var interpreter = new PerlangInterpreter(AssertFailRuntimeErrorHandler, standardOutputHandler, arguments);

            return interpreter.Eval(source, AssertFailScanErrorHandler, AssertFailParseErrorHandler,
                AssertFailResolveErrorHandler, AssertFailTypeValidationErrorHandler);
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

        private static void AssertFailTypeValidationErrorHandler(TypeValidationError typeValidationError)
        {
            throw typeValidationError;
        }
    }

    internal class EvalResult
    {
        public object Value { get; set; }
        public List<ParseError> ParseErrors { get; } = new List<ParseError>();
        public List<RuntimeError> RuntimeErrors { get ; } = new List<RuntimeError>();
        public List<ResolveError> ResolveErrors { get; } = new List<ResolveError>();
        public TypeValidationErrors TypeValidationErrors { get; } = new TypeValidationErrors();
    }
}
