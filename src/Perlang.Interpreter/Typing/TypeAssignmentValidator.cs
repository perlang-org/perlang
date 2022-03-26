using System;
using Perlang.Extensions;
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
            Func<Expr, Binding> getVariableOrFunctionCallback,
            Action<TypeValidationError> typeValidationErrorCallback)
            : base(getVariableOrFunctionCallback, typeValidationErrorCallback)
        {
        }

        public override VoidObject VisitAssignExpr(Expr.Assign expr)
        {
            base.VisitAssignExpr(expr);

            Binding variableBinding = GetVariableOrFunctionCallback(expr);

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
            long? sourceConstantValueSize = null;

            if (expr.Value is Expr.Literal { Value: INumericLiteral parsedNumber })
            {
                sourceConstantValueSize = parsedNumber.BitsUsed;
            }

            if (!TypeCoercer.CanBeCoercedInto(targetTypeReference, sourceTypeReference, sourceConstantValueSize))
            {
                TypeValidationErrorCallback(new TypeValidationError(
                    expr.Token,
                    $"Cannot assign {sourceTypeReference.ClrType.ToTypeKeyword()} to {targetTypeReference.ClrType.ToTypeKeyword()} variable"
                ));
            }

            return VoidObject.Void;
        }
    }
}
