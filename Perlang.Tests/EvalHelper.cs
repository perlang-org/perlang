using System.Collections.Generic;
using Perlang.Interpreter;
using Perlang.Parser;

namespace Perlang.Tests
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
                AssertFailResolveErrorHandler);
        }

        /// <summary>
        /// Evaluates the provided expression or list of statements. If provided an expression, EvalResult.Value
        /// contains the value of the evaluated expression; otherwise, this field will be false.
        ///
        /// This method will propagate all errors apart from runtime errors to the caller. Runtime errors will be
        /// available in the returned <see cref="EvalResult"/>.
        /// </summary>
        /// <param name="source">a valid Perlang programs</param>
        /// <returns>an EvalResult with the result of the provided expression, or null if not provided an expression.</returns>
        internal static EvalResult EvalWithRuntimeCatch(string source)
        {
            var result = new EvalResult();

            var interpreter = new PerlangInterpreter(runtimeError => result.RuntimeErrors.Add(runtimeError));
            result.Value = interpreter.Eval(source, AssertFailScanErrorHandler, AssertFailParseErrorHandler,
                AssertFailResolveErrorHandler);

            return result;
        }

        private static void AssertFailScanErrorHandler(ScanError scanError)
        {
            throw new ScanErrorXunitException(scanError.ToString());
        }

        private static void AssertFailParseErrorHandler(ParseError parseError)
        {
            throw new ParseErrorXunitException(parseError.ToString());
        }

        private static void AssertFailResolveErrorHandler(ResolveError resolveError)
        {
            throw new ResolveErrorXunitException(resolveError.ToString());
        }

        private static void AssertFailRuntimeErrorHandler(RuntimeError runtimeError)
        {
            throw runtimeError;
        }
    }

    internal class EvalResult
    {
        public object Value { get; set; }
        public List<RuntimeError> RuntimeErrors { get ; } = new List<RuntimeError>();
    }
}
