#nullable enable
#pragma warning disable S907
#pragma warning disable S3218
#pragma warning disable SA1118

using System;
using System.Collections.Generic;
using System.Linq;
using Perlang.Compiler;
using Perlang.Interpreter.Internals;
using Perlang.Interpreter.NameResolution;
using Perlang.Parser;

namespace Perlang.Interpreter.Typing
{
    /// <summary>
    /// Visitor which validates that the type references are resolved in the given list of statements.
    ///
    /// This visitor works under the assumption that explicit types have been resolved, and implicit types have
    /// been inferred; in other words, <see cref="TypeResolver"/> have been visiting the expression tree(s) in
    /// question.
    /// </summary>
    internal class TypesResolvedValidator : Validator
    {
        private readonly Action<CompilerWarning> compilerWarningCallback;

        internal TypesResolvedValidator(
            IBindingRetriever variableOrFunctionRetriever,
            Action<TypeValidationError> typeValidationErrorCallback,
            Action<CompilerWarning> compilerWarningCallback)
            : base(variableOrFunctionRetriever, typeValidationErrorCallback)
        {
            this.compilerWarningCallback = compilerWarningCallback;
        }

        //
        // Expr visitors
        //

        public override VoidObject VisitBinaryExpr(Expr.Binary expr)
        {
            base.VisitBinaryExpr(expr);

            if (!expr.TypeReference.IsResolved)
            {
                // As elsewhere, this might be a valid scenario if other errors are present. Try to log this with
                // 'debug' severity when possible.
                //TypeValidationErrorCallback(new TypeValidationError(
                //    expr.Token,
                //    $"Internal compiler error: '{expr}' inference has not been attempted"
                //));
            }

            return VoidObject.Void;
        }

        public override VoidObject VisitCallExpr(Expr.Call expr)
        {
            if (expr.Callee is Expr.Get get)
            {
                VisitCallExprForGetCallee(expr, get);
            }
            else
            {
                VisitCallExprForOtherCallee(expr);
            }

            return VoidObject.Void;
        }

        private void VisitCallExprForGetCallee(Expr.Call call, Expr.Get get)
        {
            string methodName = get.Name.Lexeme;

            if (get.PerlangMethods.Length == 0)
            {
                if (!get.Object.TypeReference.IsResolved)
                {
                    // This is a bit of an oddball, but... We get here when an attempt is made to call a method on some
                    // undefined type. (`Foo.do_stuff`)
                    //
                    // Now, this is a compile-time error, but the problem is that it's handled by this class itself;
                    // encountering this error will not abort the tree traversal, so we must avoid breaking it.
                }
                else if (get.Object.TypeReference.CppType?.WrapInSharedPtr == false)
                {
                    // TODO: This is the wrong exception type; should rather be something like
                    // TODO: InvalidOperationException. For now, we use this to make sure that it causes tests
                    // TODO: triggering this to be marked as Skipped.
                    throw new NotImplementedInCompiledModeException($"Calling methods on {get.Object} which is of type {get.Object.TypeReference.CppType.CppTypeName} is not supported");
                }
                else
                {
                    string inTypeString = get.Object.TypeReference.CppType == null ?
                        "" : $" in type '{get.Object.TypeReference.CppType.TypeKeyword}'";

                    // This is even more odd, but we must ensure that we have well-defined semantics in the weird case
                    // where this would happen.
                    TypeValidationErrorCallback(new TypeValidationError(
                        call.Paren,
                        $"Internal compiler error: no methods with name '{methodName}' could be found{inTypeString}. This is a critical " +
                        $"error that should have aborted the compilation before the {nameof(TypesResolvedValidator)} " +
                        "validation is started. "
                    ));
                }
            }
            else if (get.PerlangMethods.Length == 1)
            {
                IPerlangFunction method = get.PerlangMethods.Single();
                var parameters = method.Parameters;

                // There is exactly one potential method to call in this case. We use this fact to provide better
                // error messages to the caller than when calling an overloaded method.
                if (parameters.Count != call.Arguments.Count)
                {
                    TypeValidationErrorCallback(new TypeValidationError(
                        call.Paren,
                        $"Method '{methodName}' has {parameters.Count} parameter(s) but was called with {call.Arguments.Count} argument(s)"
                    ));

                    return;
                }

                for (int i = 0; i < call.Arguments.Count; i++)
                {
                    Parameter parameter = parameters[i];
                    Expr argument = call.Arguments[i];

                    if (!argument.TypeReference.IsResolved) {
                        throw new PerlangInterpreterException(
                            $"Internal compiler error: Argument '{argument}' to function {methodName} not resolved");
                    }

                    // FIXME: `null` here has disadvantages as described elsewhere.
                    // FIXME: Parameter could be a Perlang class instance, in which case ClrType will be null (but CppType
                    // will be set)
                    if (!TypeCoercer.CanBeCoercedInto(parameter.TypeReference.CppType, argument.TypeReference.CppType, null)) {
                        // Very likely refers to a native method, where parameter names are not available at this point.
                        TypeValidationErrorCallback(new TypeValidationError(
                            argument.TypeReference.TypeSpecifier!,
                            $"Cannot pass {argument.TypeReference.TypeKeywordOrPerlangType} argument as {parameter.TypeReference.TypeKeywordOrPerlangType} parameter to {methodName}()"));
                    }
                }
            }
            else if (get.PerlangMethods.Length > 1) {
                TypeValidationErrorCallback(new NameResolutionTypeValidationError(
                    call.Paren,
                    $"Method '{call.CalleeToString}' has multiple overloads. This is not yet supported."
                ));
            }
        }

        private void VisitCallExprForOtherCallee(Expr.Call expr)
        {
            Binding? binding = this.VariableOrFunctionRetriever.GetVariableOrFunctionBinding(expr);

            if (binding == null)
            {
                TypeValidationErrorCallback(
                    new NameResolutionTypeValidationError(expr.Paren, $"Attempting to call undefined function '{expr.CalleeToString}'")
                );

                return;
            }

            IList<Parameter> parameters;
            string functionName;

            switch (binding)
            {
                case FunctionBinding functionBinding:
                    Stmt.Function function = functionBinding.Function;

                    if (function == null)
                    {
                        throw new NameResolutionTypeValidationError(expr.Paren, $"Internal compiler error: function for {expr} not expected to be null");
                    }

                    parameters = function.Parameters;
                    functionName = function.NameToken.Lexeme;
                    break;

                default:
                    throw new NameResolutionTypeValidationError(expr.Paren, $"Attempting to call invalid function {binding} using {expr}");
            }

            if (parameters.Count != expr.Arguments.Count)
            {
                TypeValidationErrorCallback(new TypeValidationError(
                    expr.Paren,
                    $"Function '{functionName}' has {parameters.Count} parameter(s) but was called with {expr.Arguments.Count} argument(s)")
                );

                return;
            }

            for (int i = 0; i < expr.Arguments.Count; i++)
            {
                Parameter parameter = parameters[i];
                Expr argument = expr.Arguments[i];

                if (!argument.TypeReference.IsResolved)
                {
                    throw new PerlangInterpreterException($"Internal compiler error: Argument '{argument}' to function {functionName} not resolved");
                }

                if (argument.TypeReference.IsNullObject)
                {
                    compilerWarningCallback(new CompilerWarning($"Null parameter detected for '{parameter.Name.Lexeme}'", expr.TokenAwareCallee.Token, WarningType.NULL_USAGE));
                }

                // FIXME: expr.Token is an approximation here as well (see other similar comments in this file)
                // FIXME: `null` here means that small-constants of e.g. `long` will not be able to be passed as `int` parameters.
                if (!TypeCoercer.CanBeCoercedInto(parameter.TypeReference, argument.TypeReference, null))
                {
                    if (parameter.Name != null)
                    {
                        TypeValidationErrorCallback(new TypeValidationError(
                            argument.TypeReference.TypeSpecifier!,
                            $"Cannot pass {argument.TypeReference.TypeKeywordOrPerlangType} argument as parameter '{parameter.Name.Lexeme}: {parameter.TypeReference.TypeKeywordOrPerlangType}' to {functionName}()"));
                    }
                    else
                    {
                        // Very likely refers to a native method, where parameter names are not available at this point.
                        TypeValidationErrorCallback(new TypeValidationError(
                            argument.TypeReference.TypeSpecifier!,
                            $"Cannot pass {argument.TypeReference.TypeKeywordOrPerlangType} argument as {parameter.TypeReference.TypeKeywordOrPerlangType} parameter to {functionName}()"));
                    }
                }
            }
        }

        //
        // Stmt visitors
        //

        public override VoidObject VisitFunctionStmt(Stmt.Function stmt)
        {
            if (!stmt.ReturnTypeReference.IsResolved && !stmt.IsConstructor && !stmt.IsDestructor) {
                // Note: this is a bit duplicated; we also have a check in TypeResolver for this. I considered
                // de-duplicating it, but it feels dangerous, since we need to remember to edit this when we
                // add support for function return type inference.
                if (stmt.ReturnTypeReference.ExplicitTypeSpecified) {
                    this.TypeValidationErrorCallback(
                        new TypeValidationError(
                            stmt.ReturnTypeReference.TypeSpecifier!,
                            $"Type not found: {stmt.ReturnTypeReference.TypeSpecifier!.Lexeme}"
                        ));
                }
                else {
                    // TODO: Remove once https://gitlab.perlang.org/perlang/perlang/-/issues/43 is fully resolved.
                    this.TypeValidationErrorCallback(
                        new TypeValidationError(
                            stmt.NameToken,
                            $"Inferred typing is not yet supported for function '{stmt.NameToken.Lexeme}'")
                    );
                }
            }

            foreach (Parameter parameter in stmt.Parameters)
            {
                if (parameter.TypeSpecifier == null)
                {
                    // TODO: Remove once https://gitlab.perlang.org/perlang/perlang/-/issues/43 is fully resolved.
                    TypeValidationErrorCallback(new TypeValidationError(
                        stmt.NameToken,
                        $"Inferred typing is not yet supported for parameter '{parameter.Name.Lexeme}' to function '{stmt.NameToken.Lexeme}'")
                    );
                }
            }

            return base.VisitFunctionStmt(stmt);
        }

        public override VoidObject VisitFieldStmt(Stmt.Field stmt)
        {
            if (!stmt.TypeReference.IsResolved)
            {
                TypeValidationErrorCallback(new TypeValidationError(
                    stmt.TypeReference.TypeSpecifier!,
                    $"Type not found: {stmt.TypeReference.TypeSpecifier!.Lexeme}"
                ));
            }

            if (stmt.Initializer != null && !stmt.Initializer.TypeReference.IsResolved)
            {
                TypeValidationErrorCallback(new TypeValidationError(
                    stmt.NameToken,
                    $"Internal compiler error: type for field '{stmt.NameToken.Lexeme}' initializer has not been resolved"
                ));
            }

            if (stmt.Initializer != null)
            {
                if (!TypeCoercer.CanBeCoercedInto(stmt.TypeReference, stmt.Initializer.TypeReference, (stmt.Initializer as Expr.Literal)?.Value as INumericLiteral))
                {
                    // TODO: Use stmt.Initializer.Token here instead of stmt.name, #189
                    TypeValidationErrorCallback(new TypeValidationError(
                        stmt.NameToken,
                        $"Cannot assign {stmt.Initializer.TypeReference.TypeKeywordOrPerlangType} to {stmt.TypeReference.TypeKeywordOrPerlangType} field"
                    ));
                }
                else if (stmt.Initializer.TypeReference.IsNullObject)
                {
                    // TODO: Use stmt.Initializer.Token here instead of stmt.name, #189
                    compilerWarningCallback(new CompilerWarning("Initializing field to null detected", stmt.NameToken, WarningType.NULL_USAGE));
                }
            }

            return base.VisitFieldStmt(stmt);
        }

        public override VoidObject VisitReturnStmt(Stmt.Return stmt)
        {
            if (stmt.Value != null)
            {
                if (!stmt.Value.TypeReference.IsResolved)
                {
                    // This is expected to be handled already. If we had 'debug' or 'trace' logging, logging this with
                    // either one of those severities could make sense.
                    //TypeValidationErrorCallback(new TypeValidationError(
                    //    stmt.Keyword,
                    //    $"Internal compiler error: return {stmt.Value} inference has not been attempted"
                    //));
                }
            }
            else
            {
                // No return value - ensure that the return type of the current function is 'void'.
            }

            // TODO: Validate that return type in stmt.TypeReference matches that of the associated function.

            // This is important, to ensure that the return value expression is also visited
            return base.VisitReturnStmt(stmt);
        }

        public override VoidObject VisitVarStmt(Stmt.Var stmt)
        {
            bool sanityCheckFailed = false;

            if (stmt.Initializer != null && !stmt.Initializer.TypeReference.IsResolved)
            {
                TypeValidationErrorCallback(new TypeValidationError(
                    stmt.Name,
                    $"Internal compiler error: 'var {stmt.Name.Lexeme}' initializer '{stmt.Initializer}' inference has not been attempted"
                ));

                sanityCheckFailed = true;
            }

            if (sanityCheckFailed)
            {
                return VoidObject.Void;
            }

            if (stmt.TypeReference.IsResolved)
            {
                if (stmt.Initializer != null)
                {
                    if (stmt.TypeReference.IsNullObject && stmt.Initializer.TypeReference.IsNullObject)
                    {
                        // TODO: Use stmt.Initializer.Token here instead of stmt.name, #189
                        TypeValidationErrorCallback(new TypeValidationError(
                            stmt.Name,
                            "Cannot assign null to an implicitly typed local variable"
                        ));
                    }
                    else if (!TypeCoercer.CanBeCoercedInto(stmt.TypeReference, stmt.Initializer.TypeReference, (stmt.Initializer as Expr.Literal)?.Value as INumericLiteral))
                    {
                        // TODO: Use stmt.Initializer.Token here instead of stmt.name, #189
                        TypeValidationErrorCallback(new TypeValidationError(
                            stmt.Name,
                            $"Cannot assign {stmt.Initializer.TypeReference.TypeKeywordOrPerlangType} to {stmt.TypeReference.TypeKeywordOrPerlangType} variable"
                        ));
                    }
                    else if (stmt.Initializer.TypeReference.IsNullObject)
                    {
                        // TODO: Use stmt.Initializer.Token here instead of stmt.name, #189
                        compilerWarningCallback(new CompilerWarning("Initializing variable to null detected", stmt.Name, WarningType.NULL_USAGE));
                    }
                }
            }
            else if (!stmt.TypeReference.ExplicitTypeSpecified && stmt.Initializer == null)
            {
                TypeValidationErrorCallback(new TypeValidationError(
                    stmt.Name,
                    $"Type inference for variable '{stmt.Name.Lexeme}' cannot be performed when initializer is not specified. " +
                    "Either provide an initializer, or specify the type explicitly."
                ));
            }
            else
            {
                // Note: you will get this exception when working on adding a new built-in type. This can happen when
                // the type is not present in TypeResolver.
                TypeValidationErrorCallback(new TypeValidationError(
                    stmt.TypeReference.TypeSpecifier!,
                    $"Type not found: {stmt.TypeReference.TypeSpecifier!.Lexeme}"
                ));
            }

            return VoidObject.Void;
        }

        public override VoidObject VisitAssignExpr(Expr.Assign expr)
        {
            base.VisitAssignExpr(expr);

            if (expr.Value.TypeReference.IsNullObject)
            {
                // TODO: Use expr.Value.Token here instead of expr.name, #189
                compilerWarningCallback(new CompilerWarning("Null assignment detected", expr.TargetName, WarningType.NULL_USAGE));
            }

            return VoidObject.Void;
        }
    }
}
