#nullable enable
#pragma warning disable S907
#pragma warning disable S3218
#pragma warning disable SA1118

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Perlang.Internal.Extensions;
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

            if (get.ClrMethods.Length == 0 && get.PerlangMethods.Length == 0)
            {
                if (!get.Object.TypeReference.IsResolved)
                {
                    // This is a bit of an oddball, but... We get here when an attempt is made to call a method on some
                    // undefined type. (`Foo.do_stuff`)
                    //
                    // Now, this is a compile-time error, but the problem is that it's handled by this class itself;
                    // encountering this error will not abort the tree traversal, so we must avoid breaking it.
                }
                else
                {
                    string inTypeString = get.Object.TypeReference.CppType == null ?
                        "" : $" in type '{get.Object.TypeReference.CppType}'";

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
            else if (get.ClrMethods.Length == 1)
            {
                MethodInfo method = get.ClrMethods.Single();
                var parameters = method.GetParameters();

                // There is exactly one potential method to call in this case. We use this fact to provide better
                // error messages to the caller than when calling an overloaded method.
                if (parameters.Length != call.Arguments.Count)
                {
                    TypeValidationErrorCallback(new TypeValidationError(
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
                    // FIXME: `null` here has disadvantages as described elsewhere.
                    if (!TypeCoercer.CanBeCoercedInto(parameter.ParameterType, argument.TypeReference.ClrType, null))
                    {
                        // Very likely refers to a native method, where parameter names are not available at this point.
                        TypeValidationErrorCallback(new TypeValidationError(
                            argument.TypeReference.TypeSpecifier!,
                            $"Cannot pass {argument.TypeReference.ClrType.ToTypeKeyword()} argument as {parameter.ParameterType.ToTypeKeyword()} parameter to {methodName}()"));
                    }
                }
            }
            else if (get.ClrMethods.Length > 1)
            {
                // Method is overloaded. Try to resolve the best match we can find.
                foreach (MethodInfo method in get.ClrMethods)
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
                        if (!TypeCoercer.CanBeCoercedInto(parameter.ParameterType, argument.TypeReference.ClrType, null))
                        {
                            coercionsFailed = true;
                            break;
                        }
                    }

                    if (!coercionsFailed)
                    {
                        // We have found a suitable overload to use. Update the expression
                        get.ClrMethods = ImmutableArray.Create(method);
                        return;
                    }
                }

                TypeValidationErrorCallback(new NameResolutionTypeValidationError(
                    call.Paren,
                    $"Method '{call.CalleeToString}' found, but no overload matches the provided parameters."
                ));
            }
            else if (get.PerlangMethods.Length == 1)
            {
                Stmt.Function method = get.PerlangMethods.Single();
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

                    // FIXME: call.Token is a bit off here; it would be useful when constructing compiler warnings based
                    // on this if we could provide the token for the argument expression instead. However, the Expr type
                    // as used by 'argument' is a non-token-based expression so this is currently impossible.
                    // FIXME: `null` here has disadvantages as described elsewhere.
                    // FIXME: Parameter could be a Perlang class instance, in which case ClrType will be null (but CppType
                    // will be set)
                    if (!TypeCoercer.CanBeCoercedInto(parameter.TypeReference.ClrType, argument.TypeReference.ClrType, null)) {
                        // Very likely refers to a native method, where parameter names are not available at this point.
                        TypeValidationErrorCallback(new TypeValidationError(
                            argument.TypeReference.TypeSpecifier!,
                            $"Cannot pass {argument.TypeReference.ClrType.ToTypeKeyword()} argument as {parameter.TypeReference.ClrType.ToTypeKeyword()} parameter to {methodName}()"));
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
                    functionName = function.Name.Lexeme;
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
                            $"Cannot pass {argument.TypeReference.ClrType.ToTypeKeyword()} argument as parameter '{parameter.Name.Lexeme}: {parameter.TypeReference.ClrType.ToTypeKeyword()}' to {functionName}()"));
                    }
                    else
                    {
                        // Very likely refers to a native method, where parameter names are not available at this point.
                        TypeValidationErrorCallback(new TypeValidationError(
                            argument.TypeReference.TypeSpecifier!,
                            $"Cannot pass {argument.TypeReference.ClrType.ToTypeKeyword()} argument as {parameter.TypeReference.ClrType.ToTypeKeyword()} parameter to {functionName}()"));
                    }
                }
            }
        }

        //
        // Stmt visitors
        //

        public override VoidObject VisitFunctionStmt(Stmt.Function stmt)
        {
            if (!stmt.ReturnTypeReference.IsResolved && !stmt.IsConstructor && !stmt.IsDestructor)
            {
                // Note: this is a bit duplicated; we also have a check in TypeResolver for this. I considered
                // de-duplicating it, but it feels dangerous, since we need to remember to edit this when we
                // add support for function return type inference.
                if (!stmt.ReturnTypeReference.ExplicitTypeSpecified)
                {
                    // TODO: Remove once https://gitlab.perlang.org/perlang/perlang/-/issues/43 is fully resolved.
                    TypeValidationErrorCallback(new TypeValidationError(
                        stmt.Name,
                        $"Inferred typing is not yet supported for function '{stmt.Name.Lexeme}'")
                    );
                }
                else
                {
                    TypeValidationErrorCallback(new TypeValidationError(
                        stmt.ReturnTypeReference.TypeSpecifier!,
                        $"Type not found: {stmt.ReturnTypeReference.TypeSpecifier!.Lexeme}"
                    ));
                }
            }

            foreach (Parameter parameter in stmt.Parameters)
            {
                if (parameter.TypeSpecifier == null)
                {
                    // TODO: Remove once https://gitlab.perlang.org/perlang/perlang/-/issues/43 is fully resolved.
                    TypeValidationErrorCallback(new TypeValidationError(
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
                    TypeValidationErrorCallback(new TypeValidationError(
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
                TypeValidationErrorCallback(new TypeValidationError(
                    stmt.Name,
                    $"Internal compiler error: {stmt.Name.Lexeme} is missing a TypeReference"
                ));

                sanityCheckFailed = true;
            }

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

            if (stmt.TypeReference!.IsResolved)
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
                            $"Cannot assign {stmt.Initializer.TypeReference.ClrType.ToTypeKeyword()} to {stmt.TypeReference.ClrType.ToTypeKeyword()} variable"
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
                compilerWarningCallback(new CompilerWarning("Null assignment detected", expr.Name, WarningType.NULL_USAGE));
            }

            return VoidObject.Void;
        }
    }
}
