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

            if (binding == null)
            {
                throw new PerlangInterpreterException($"Failed to locate binding for {expr.Identifier}");
            }

            if (binding.IsImmutable)
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
