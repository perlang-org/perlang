#nullable enable
using System;
using System.Collections.Immutable;
using Perlang.Interpreter.Resolution;

namespace Perlang.Interpreter.Immutability
{
    /// <summary>
    /// Validator which ensures that objects which are intended to be immutable (either by nature or because they have
    /// been configured in a particular way) cannot be mutated.
    /// </summary>
    internal class ImmutabilityValidator : VisitorBase
    {
        private readonly Action<ValidationError> immutabilityValidationErrorCallback;
        private readonly Func<Expr, Binding?> getVariableOrFunctionBinding;

        public static void Validate(
            ImmutableList<Stmt> statements,
            Action<ValidationError> immutabilityValidationErrorCallback,
            Func<Expr, Binding?> getVariableOrFunctionBinding)
        {
            new ImmutabilityValidator(immutabilityValidationErrorCallback, getVariableOrFunctionBinding).Visit(statements);
        }

        public static void Validate(
            Expr expr,
            Action<ValidationError> immutabilityValidationErrorCallback,
            Func<Expr, Binding?> getVariableOrFunctionBinding)
        {
            new ImmutabilityValidator(immutabilityValidationErrorCallback, getVariableOrFunctionBinding).Visit(expr);
        }

        private ImmutabilityValidator(
            Action<ValidationError> immutabilityValidationErrorCallback,
            Func<Expr, Binding?> getVariableOrFunctionBinding)
        {
            this.immutabilityValidationErrorCallback = immutabilityValidationErrorCallback;
            this.getVariableOrFunctionBinding = getVariableOrFunctionBinding;
        }

        public override VoidObject VisitAssignExpr(Expr.Assign expr)
        {
            base.VisitAssignExpr(expr);

            Binding? binding = getVariableOrFunctionBinding(expr);

            // 'null' here can either be because of an internal error (failure to locate a binding that _should_ exist),
            // or a completely valid case when trying to reassign an undefined variable. Regretfully, we cannot
            // distinguish between these two scenarios at the moment.
            if (binding?.IsImmutable == true)
            {
                immutabilityValidationErrorCallback(new ImmutabilityValidationError(
                    expr.Name,
                    $"{binding.ObjectTypeTitleized} '{expr.Name.Lexeme}' is immutable and cannot be modified."
                ));
            }

            return VoidObject.Void;
        }
    }
}
