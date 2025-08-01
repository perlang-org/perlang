using System;
using Perlang.Interpreter.Internals;
using Perlang.Interpreter.NameResolution;
using Perlang.Parser;

namespace Perlang.Interpreter.Typing
{
    /// <summary>
    /// Visitor which ensures that all assignments in the given list of statements use values which can be coerced to
    /// the variable type in question.
    /// </summary>
    internal class TypeAssignmentValidator : Validator
    {
        public TypeAssignmentValidator(
            IBindingRetriever variableOrFunctionRetriever,
            Action<TypeValidationError> typeValidationErrorCallback)
            : base(variableOrFunctionRetriever, typeValidationErrorCallback)
        {
        }

        public override VoidObject VisitAssignExpr(Expr.Assign expr)
        {
            base.VisitAssignExpr(expr);

            Binding variableBinding = this.VariableOrFunctionRetriever.GetVariableOrFunctionBinding(expr);

            if (variableBinding == null)
            {
                // An attempt is made to assign a value to an undefined variable. This is an error, but it's handled
                // elsewhere so we can silently ignore it at this point.
                return VoidObject.Void;
            }

            if (variableBinding is FunctionBinding)
            {
                // Functions are immutable, handled by a class in the Perlang.Interpreter.Immutability namespace. We can
                // ignore it at this point; the attempt to reassign it will be detected elsewhere.
                return VoidObject.Void;
            }

            var targetTypeReference = variableBinding.TypeReference;
            var sourceTypeReference = expr.Value.TypeReference;
            INumericLiteral numericLiteral = null;

            if (expr.Value is Expr.Literal { Value: INumericLiteral valueNumericLiteral })
            {
                numericLiteral = valueNumericLiteral;
            }

            if (!TypeCoercer.CanBeCoercedInto(targetTypeReference, sourceTypeReference, numericLiteral))
            {
                // TODO: When is this actually triggered? We really need to look into #341 at some point.
                TypeValidationErrorCallback(new TypeValidationError(
                    expr.Token,
                    $"Cannot assign {sourceTypeReference.ToQuotedTypeKeyword()} to '{targetTypeReference.TypeKeyword}' variable"
                ));
            }

            return VoidObject.Void;
        }
    }
}
