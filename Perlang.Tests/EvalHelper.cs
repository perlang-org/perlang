using Perlang.Interpreter;
using Perlang.Parser;

namespace Perlang.Tests
{
    internal static class EvalHelper
    {
        /// <summary>
        /// Evaluates the provided expression or list of statements. If provided an expression, returns the result;
        /// otherwise, returns null.
        /// </summary>
        /// <param name="source">a valid Perlang programs</param>
        /// <returns>the result of the provided expression, or null if not provided an expression.</returns>
        internal static object Eval(string source)
        {
            var interpreter = new PerlangInterpreter(AssertFailRuntimeErrorHandler);
            return interpreter.Eval(source, AssertFailScanErrorHandler, AssertFailParseErrorHandler,
                AssertFailResolveErrorHandler);
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
}