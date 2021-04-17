#pragma warning disable S907
#pragma warning disable S3218
#pragma warning disable SA1118

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Perlang.Interpreter.Resolution;
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
    internal class TypesResolvedValidator : VisitorBase
    {
        private readonly Func<Expr, Binding> getVariableOrFunctionCallback;
        private readonly Action<TypeValidationError> typeValidationErrorCallback;
        private readonly Action<CompilerWarning> compilerWarningCallback;

        private readonly TypeCoercer typeCoercer;

        internal TypesResolvedValidator(
            Func<Expr, Binding> getVariableOrFunctionCallback,
            Action<TypeValidationError> typeValidationErrorCallback,
            Action<CompilerWarning> compilerWarningCallback)
        {
            this.getVariableOrFunctionCallback = getVariableOrFunctionCallback;
            this.typeValidationErrorCallback = typeValidationErrorCallback;
            this.compilerWarningCallback = compilerWarningCallback;

            typeCoercer = new TypeCoercer(compilerWarningCallback);
        }

        internal void ReportErrors(IEnumerable<Stmt> statements)
        {
            foreach (Stmt statement in statements)
            {
                ReportErrors(statement);
            }
        }

        internal void ReportErrors(Expr expr)
        {
            expr.Accept(this);
        }

        private void ReportErrors(Stmt stmt)
        {
            stmt.Accept(this);
        }

        //
        // Expr visitors
        //

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

            if (get.Methods.Length == 1)
            {
                MethodInfo method = get.Methods.Single();
                var parameters = method.GetParameters();

                // There is exactly one potential method to call in this case. We use this fact to provide better
                // error messages to the caller than when calling an overloaded method.
                if (parameters.Length != call.Arguments.Count)
                {
                    typeValidationErrorCallback(new TypeValidationError(
                        call.Paren,
                        $"Method '{methodName}' has {parameters.Length} parameter(s) but was called with {call.Arguments.Count} argument(s)"
                    ));

                    return;
                }

                for (int i = 0; i < call.Arguments.Count; i++)
                {
                    ParameterInfo parameter = parameters[i];
                    Expr argument = call.Arguments[i];

                    if (!argument.TypeReference.IsResolved)
                    {
                        throw new PerlangInterpreterException(
                            $"Internal compiler error: Argument '{argument}' to function {methodName} not resolved");
                    }

                    // FIXME: call.Token is a bit off here; it would be useful when constructing compiler warnings based
                    // on this if we could provide the token for the argument expression instead. However, the Expr type
                    // as used by 'argument' is a non-token-based expression so this is currently impossible.
                    if (!typeCoercer.CanBeCoercedInto(call.Token, parameter.ParameterType, argument.TypeReference.ClrType))
                    {
                        // Very likely refers to a native method, where parameter names are not available at this point.
                        typeValidationErrorCallback(new TypeValidationError(
                            argument.TypeReference.TypeSpecifier,
                            $"Cannot pass {argument.TypeReference.ClrType} argument as {parameter.ParameterType} parameter to {methodName}()"));
                    }
                }
            }
            else
            {
                // Method is overloaded. Try to resolve the best match we can find.
                foreach (MethodInfo method in get.Methods)
                {
                    var parameters = method.GetParameters();

                    if (parameters.Length != call.Arguments.Count)
                    {
                        // The number of parameters do not match, so this method will never be a suitable candidate
                        // for our expression.
                        continue;
                    }

                    bool coercionsFailed = false;

                    for (int i = 0; i < call.Arguments.Count; i++)
                    {
                        ParameterInfo parameter = parameters[i];
                        Expr argument = call.Arguments[i];

                        if (!argument.TypeReference.IsResolved)
                        {
                            throw new PerlangInterpreterException(
                                $"Internal compiler error: Argument '{argument}' to method {methodName} not resolved");
                        }

                        // FIXME: The same caveat as above with call.Token applies here as well.
                        if (!typeCoercer.CanBeCoercedInto(call.Token, parameter.ParameterType, argument.TypeReference.ClrType))
                        {
                            coercionsFailed = true;
                            break;
                        }
                    }

                    if (!coercionsFailed)
                    {
                        // We have found a suitable overload to use. Update the expression
                        get.Methods = ImmutableArray.Create(method);
                        return;
                    }
                }

                typeValidationErrorCallback(new NameResolutionError(
                    call.Paren,
                    $"Method '{call.CalleeToString}' found, but no overload matches the provided parameters."
                ));
            }
        }

        private void VisitCallExprForOtherCallee(Expr.Call expr)
        {
            Binding binding = getVariableOrFunctionCallback(expr);

            if (binding == null)
            {
                typeValidationErrorCallback(
                    new NameResolutionError(expr.Paren, $"Attempting to call undefined function '{expr.CalleeToString}'")
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
                        throw new NameResolutionError(expr.Paren, $"Internal compiler error: function for {expr} not expected to be null");
                    }

                    parameters = function.Parameters;
                    functionName = function.Name.Lexeme;
                    break;

                default:
                    throw new NameResolutionError(expr.Paren, $"Attempting to call invalid function {binding} using {expr}");
            }

            if (parameters.Count != expr.Arguments.Count)
            {
                typeValidationErrorCallback(new TypeValidationError(
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

                // FIXME: expr.Token is an approximation here as well (see other similar comments in this file)
                if (!typeCoercer.CanBeCoercedInto(expr.Token, parameter.TypeReference, argument.TypeReference))
                {
                    if (parameter.Name != null)
                    {
                        typeValidationErrorCallback(new TypeValidationError(
                            argument.TypeReference.TypeSpecifier,
                            $"Cannot pass {argument.TypeReference.ClrType} argument as parameter '{parameter.Name.Lexeme}: {parameter.TypeReference.ClrType}' to {functionName}()"));
                    }
                    else
                    {
                        // Very likely refers to a native method, where parameter names are not available at this point.
                        typeValidationErrorCallback(new TypeValidationError(
                            argument.TypeReference.TypeSpecifier,
                            $"Cannot pass {argument.TypeReference.ClrType} argument as {parameter.TypeReference.ClrType} parameter to {functionName}()"));
                    }
                }
            }
        }

        //
        // Stmt visitors
        //

        public override VoidObject VisitFunctionStmt(Stmt.Function stmt)
        {
            if (!stmt.ReturnTypeReference.IsResolved)
            {
                // Note: this is a bit duplicated; we also have a check in TypeResolver for this. I considered
                // de-duplicating it but it feels dangerous, since we need to remember to edit this when we
                // add support for function return type inference.
                if (!stmt.ReturnTypeReference.ExplicitTypeSpecified)
                {
                    // TODO: Remove once https://github.com/perlang-org/perlang/issues/43 is fully resolved.
                    typeValidationErrorCallback(new TypeValidationError(
                        stmt.Name,
                        $"Inferred typing is not yet supported for function '{stmt.Name.Lexeme}'")
                    );
                }
                else
                {
                    typeValidationErrorCallback(new TypeValidationError(
                        stmt.ReturnTypeReference.TypeSpecifier,
                        $"Type not found: {stmt.ReturnTypeReference.TypeSpecifier.Lexeme}"
                    ));
                }
            }

            foreach (Parameter parameter in stmt.Parameters)
            {
                if (parameter.TypeSpecifier == null)
                {
                    // TODO: Remove once https://github.com/perlang-org/perlang/issues/43 is fully resolved.
                    typeValidationErrorCallback(new TypeValidationError(
                        stmt.Name,
                        $"Inferred typing is not yet supported for parameter '{parameter.Name.Lexeme}' to function '{stmt.Name.Lexeme}'")
                    );
                }
            }

            return base.VisitFunctionStmt(stmt);
        }

        public override VoidObject VisitReturnStmt(Stmt.Return stmt)
        {
            bool sanityCheckFailed = false;

            if (stmt.Value != null)
            {
                if (!stmt.Value.TypeReference.IsResolved)
                {
                    typeValidationErrorCallback(new TypeValidationError(
                        stmt.Keyword,
                        $"Internal compiler error: return {stmt.Value} inference has not been attempted"
                    ));

                    sanityCheckFailed = true;
                }
            }
            else
            {
                // No return value - ensure that the return type of the current function is 'void'.
            }

            if (sanityCheckFailed)
            {
                return VoidObject.Void;
            }

            // TODO: Validate that return type in stmt.TypeReference matches that of the associated function.

            return VoidObject.Void;
        }

        public override VoidObject VisitVarStmt(Stmt.Var stmt)
        {
            bool sanityCheckFailed = false;

            // Sanity check the input to ensure that we don't get NullReferenceExceptions later on
            if (stmt.TypeReference == null)
            {
                typeValidationErrorCallback(new TypeValidationError(
                    stmt.Name,
                    $"Internal compiler error: {stmt.Name.Lexeme} is missing a TypeReference"
                ));

                sanityCheckFailed = true;
            }

            if (stmt.Initializer != null)
            {
                if (stmt.Initializer.TypeReference == null)
                {
                    typeValidationErrorCallback(new TypeValidationError(
                        stmt.Name,
                        $"Internal compiler error: var {stmt.Name.Lexeme} initializer {stmt.Initializer} missing a TypeReference"
                    ));

                    sanityCheckFailed = true;
                }
                else if (!stmt.Initializer.TypeReference.IsResolved)
                {
                    typeValidationErrorCallback(new TypeValidationError(
                        stmt.Name,
                        $"Internal compiler error: var {stmt.Name.Lexeme} initializer {stmt.Initializer} inference has not been attempted"
                    ));

                    sanityCheckFailed = true;
                }
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
                        typeValidationErrorCallback(new TypeValidationError(
                            stmt.Name,
                            "Cannot assign nil to an implicitly typed local variable"
                        ));
                    }
                    else if (!typeCoercer.CanBeCoercedInto(stmt.Name, stmt.TypeReference, stmt.Initializer.TypeReference))
                    {
                        // TODO: Use stmt.Initializer.Token here instead of stmt.name, #189
                        typeValidationErrorCallback(new TypeValidationError(
                            stmt.Name,
                            $"Cannot assign {stmt.Initializer.TypeReference.ClrType.Name} value to {stmt.TypeReference.ClrType.Name}"
                        ));
                    }
                }
            }
            else if (stmt.Initializer == null)
            {
                typeValidationErrorCallback(new TypeValidationError(
                    null,
                    $"Type inference for variable '{stmt.Name.Lexeme}' cannot be performed when initializer is not specified. " +
                    "Either provide an initializer, or specify the type explicitly."
                ));
            }
            else if (!stmt.TypeReference.ExplicitTypeSpecified)
            {
                // FIXME: Let's see if we'll ever go into this branch. If we don't have a test that reproduces
                // this once all tests are green, we should consider wiping it from the face off the earth.
                typeValidationErrorCallback(new TypeValidationError(
                    null,
                    $"Failed to infer type for variable '{stmt.Name.Lexeme}' from usage. Try specifying the type explicitly."
                ));
            }
            else
            {
                typeValidationErrorCallback(new TypeValidationError(
                    stmt.TypeReference.TypeSpecifier,
                    $"Type not found: {stmt.TypeReference.TypeSpecifier.Lexeme}"
                ));
            }

            return VoidObject.Void;
        }
    }
}
