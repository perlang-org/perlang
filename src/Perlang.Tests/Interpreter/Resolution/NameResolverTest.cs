using System;
using System.Collections.Immutable;
using System.Linq;
using Perlang.Interpreter;
using Perlang.Interpreter.NameResolution;
using Perlang.Parser;
using Xunit;

namespace Perlang.Tests.Interpreter.Resolution
{
    public class NameResolverTest
    {
        [Fact]
        public void VisitVarStmt_defines_variable_with_correct_TypeReference()
        {
            // Act
            (Stmt singleStatement, NameResolver resolver) = ScanParseAndResolveSingleStatement(@"
                var i: int = 123456;
            ");

            // Assert. We want to ensure that the proper TypeReference is being used in the VariableBindingFactory
            // which has been created at this point, since using the TypeReference from the initializer instead of the
            // variable will work incorrectly with e.g. integer expansion (`var l: long = 123` - it's critical that
            // TypeReference becomes `long` here and not `int` or `short`)
            Assert.IsType<Stmt.Var>(singleStatement);
            Assert.True(resolver.Globals.ContainsKey("i"));
            Assert.Equal(((Stmt.Var)singleStatement).TypeReference, ((VariableBindingFactory)resolver.Globals["i"]).TypeReference);
        }

        private static (Stmt Stmt, NameResolver Resolver) ScanParseAndResolveSingleStatement(string program)
        {
            var interpreter = new PerlangInterpreter(AssertFailRuntimeErrorHandler, s => throw new ApplicationException(s));

            var scanAndParseResult = interpreter.ScanAndParse(
                program,
                AssertFailScanErrorHandler,
                AssertFailParseErrorHandler
            );

            Assert.True(scanAndParseResult.HasStatements);
            Assert.Single(scanAndParseResult.Statements!);

            Stmt singleStatement = scanAndParseResult.Statements!.Single();

            var resolver = new NameResolver(
                ImmutableDictionary<string, Type>.Empty,
                ImmutableDictionary<string, Type>.Empty,
                interpreter.BindingHandler,
                AssertFailAddGlobalClassHandler,
                AssertFailNameResolutionErrorHandler
            );

            resolver.Resolve(scanAndParseResult.Statements!);

            return (singleStatement, resolver);
        }

        private static void AssertFailScanErrorHandler(ScanError scanError)
        {
            throw scanError;
        }

        private static void AssertFailParseErrorHandler(ParseError parseError)
        {
            throw parseError;
        }

        private static void AssertFailNameResolutionErrorHandler(NameResolutionError nameResolutionError)
        {
            throw nameResolutionError;
        }

        private static void AssertFailRuntimeErrorHandler(RuntimeError runtimeError)
        {
            throw runtimeError;
        }

        private static void AssertFailAddGlobalClassHandler(string name, PerlangClass perlangClass)
        {
            throw new Exception($"Unexpected global class {name} attempted to be added. Global class: {perlangClass}");
        }
    }
}
