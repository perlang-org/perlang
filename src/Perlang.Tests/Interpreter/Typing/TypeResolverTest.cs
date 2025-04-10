#pragma warning disable SA1118

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using Perlang.Interpreter;
using Perlang.Interpreter.Compiler;
using Perlang.Interpreter.NameResolution;
using Perlang.Interpreter.Typing;
using Perlang.Parser;
using Perlang.Tests.Extensions;
using Xunit;

namespace Perlang.Tests.Interpreter.Typing
{
    public class TypeResolverTest
    {
        [Fact]
        public void Resolve_var_with_long_type_defines_variable_with_expected_ClrType()
        {
            // Act
            (Stmt singleStatement, NameResolver resolver) = ScanParseResolveAndTypeResolveSingleStatement(
                nameof(Resolve_var_with_long_type_defines_variable_with_expected_ClrType) + ".per",
                @"
                    var l: long = 123456;
                ");

            // Assert
            Assert.IsType<Stmt.Var>(singleStatement);
            Assert.True(resolver.Globals.ContainsKey("l"));
            Assert.Equal(typeof(Int64), ((Stmt.Var)singleStatement).TypeReference.ClrType);
        }

        [Fact]
        public void Resolve_implicitly_typed_var_initialized_from_binary_literal_has_expected_ClrType()
        {
            (Stmt singleStatement, NameResolver resolver) = ScanParseResolveAndTypeResolveSingleStatement(
                nameof(Resolve_implicitly_typed_var_initialized_from_binary_literal_has_expected_ClrType) + ".per",
                @"
                    var v = 0b00101010;
                ");

            // Assert
            Assert.IsType<Stmt.Var>(singleStatement);
            Assert.True(resolver.Globals.ContainsKey("v"));
            Assert.Equal(typeof(Int32), ((Stmt.Var)singleStatement).TypeReference.ClrType);
        }

        [Fact]
        public void Resolve_implicitly_typed_var_initialized_from_octal_literal_has_expected_ClrType()
        {
            (Stmt singleStatement, NameResolver resolver) = ScanParseResolveAndTypeResolveSingleStatement(
                nameof(Resolve_implicitly_typed_var_initialized_from_octal_literal_has_expected_ClrType) + ".per",
                @"
                    var v = 0o755;
                ");

            // Assert
            Assert.IsType<Stmt.Var>(singleStatement);
            Assert.True(resolver.Globals.ContainsKey("v"));
            Assert.Equal(typeof(Int32), ((Stmt.Var)singleStatement).TypeReference.ClrType);
        }

        [Fact]
        public void Resolve_implicitly_typed_var_initialized_from_hexadecimal_literal_has_expected_ClrType()
        {
            (Stmt singleStatement, NameResolver resolver) = ScanParseResolveAndTypeResolveSingleStatement(
                nameof(Resolve_implicitly_typed_var_initialized_from_hexadecimal_literal_has_expected_ClrType) + ".per",
                @"
                    var v = 0xC0CAC01A;
                ");

            // Assert
            Assert.IsType<Stmt.Var>(singleStatement);
            Assert.True(resolver.Globals.ContainsKey("v"));
            Assert.Equal(typeof(UInt32), ((Stmt.Var)singleStatement).TypeReference.ClrType);
        }

        [Fact]
        public void Resolve_implicitly_typed_var_initialized_from_long_var_has_expected_ClrType()
        {
            // Act
            (List<Stmt> statements, NameResolver resolver) = ScanParseResolveAndTypeResolveStatements(
                nameof(Resolve_implicitly_typed_var_initialized_from_long_var_has_expected_ClrType) + ".per",
                @"
                    var l: long = 123456;
                    var m = l;
                ");

            // Assert
            Stmt firstStmt = statements.First();
            Stmt secondStmt = statements.Second();

            Assert.IsType<Stmt.Var>(firstStmt);
            Assert.IsType<Stmt.Var>(secondStmt);

            Assert.True(resolver.Globals.ContainsKey("l"));
            Assert.True(resolver.Globals.ContainsKey("m"));

            // Both of these are expected to have been resolved this way; the first because of the explicit type
            // specifier and the second because of type inference.
            Assert.Equal(typeof(Int64), ((Stmt.Var)firstStmt).TypeReference.ClrType);
            Assert.Equal(typeof(Int64), ((Stmt.Var)secondStmt).TypeReference.ClrType);
        }

        [Fact]
        public void Resolve_exponential_expression_with_constant_value_is_resolved_as_BigInteger()
        {
            // Act
            (Stmt stmt, _) = ScanParseResolveAndTypeResolveSingleStatement(
                nameof(Resolve_exponential_expression_with_constant_value_is_resolved_as_BigInteger) + ".per",
                @"
                    2 ** 31;
                ");

            // Assert
            Assert.IsType<Stmt.ExpressionStmt>(stmt);

            Expr expr = ((Stmt.ExpressionStmt)stmt).Expression;

            Assert.Equal(typeof(BigInteger), expr.TypeReference.ClrType);
        }

        private static (Stmt Stmt, NameResolver Resolver) ScanParseResolveAndTypeResolveSingleStatement(string fileName, string program)
        {
            (IList<Stmt> stmts, NameResolver nameResolver) = ScanParseResolveAndTypeResolveStatements(fileName, program);
            Assert.Single(stmts);

            Stmt singleStatement = stmts.Single();

            return (singleStatement, nameResolver);
        }

        private static (List<Stmt> Stmt, NameResolver Resolver) ScanParseResolveAndTypeResolveStatements(string fileName, string program)
        {
            using var compiler = new PerlangCompiler(AssertFailRuntimeErrorHandler, s => throw new ApplicationException(s.ToString()));

            var scanAndParseResult = PerlangParser.ScanAndParse(
                [
                    new SourceFile(fileName, program)
                ],
                AssertFailScanErrorHandler,
                AssertFailParseErrorHandler
            );

            Assert.True(scanAndParseResult.HasStatements);

            var nameResolver = new NameResolver(
                ImmutableDictionary<string, Type>.Empty,
                compiler.BindingHandler,
                AssertFailAddGlobalClassHandler,
                AssertFailNameResolutionErrorHandler
            );

            nameResolver.Resolve(scanAndParseResult.Statements!);

            // This is a partial extract of code from TypeValidator. Time will tell whether it's a good or bad idea
            // to copy-paste the code to the test like this or not.
            var typeResolver = new TypeResolver(
                compiler.BindingHandler,
                AssertFailValidationErrorHandler
            );

            typeResolver.Resolve(scanAndParseResult.Statements ?? new List<Stmt>());

            return (scanAndParseResult.Statements!, nameResolver);
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

        private static void AssertFailValidationErrorHandler(ValidationError validationError)
        {
            throw validationError;
        }

        private static void AssertFailAddGlobalClassHandler(string name, IPerlangClass perlangClass)
        {
            throw new Exception($"Unexpected global class {name} attempted to be added. Global class: {perlangClass}");
        }
    }
}
