using System;
using Perlang.Internal.Extensions;
using Perlang.Interpreter.NameResolution;

namespace Perlang.Interpreter.Typing
{
    /// <summary>
    /// Visitor which ensures that all expressions which expect boolean operands have been passed values which can be
    /// coerced to `bool`.
    /// </summary>
    internal class BooleanOperandsValidator : Validator
    {
        public BooleanOperandsValidator(
            Func<Expr, Binding> getVariableOrFunctionCallback,
            Action<TypeValidationError> typeValidationErrorCallback)
            : base(getVariableOrFunctionCallback, typeValidationErrorCallback)
        {
        }

        public override VoidObject VisitLogicalExpr(Expr.Logical expr)
        {
            base.VisitLogicalExpr(expr);

            if (expr.Left.TypeReference.ClrType != typeof(bool))
            {
                TypeValidationErrorCallback(new TypeValidationError(
                    expr.Token,
                    $"'{expr.Left.TypeReference.ClrType.ToTypeKeyword()}' is not a valid {expr.Operator.Lexeme} operand."
                ));
            }

            if (expr.Right.TypeReference.ClrType != typeof(bool))
            {
                TypeValidationErrorCallback(new TypeValidationError(
                    expr.Token,
                    $"'{expr.Right.TypeReference.ClrType.ToTypeKeyword()}' is not a valid {expr.Operator.Lexeme} operand."
                ));
            }

            return VoidObject.Void;
        }

        public override VoidObject VisitIfStmt(Stmt.If stmt)
        {
            base.VisitIfStmt(stmt);

            if (stmt.Condition.TypeReference.ClrType != typeof(bool))
            {
                TypeValidationErrorCallback(new TypeValidationError(
                    (stmt.Condition as ITokenAware)?.Token,
                    $"'{stmt.Condition.TypeReference.ClrType.ToTypeKeyword()}' is not a valid 'if' condition."
                ));
            }

            return VoidObject.Void;
        }

        public override VoidObject VisitWhileStmt(Stmt.While stmt)
        {
            base.VisitWhileStmt(stmt);

            if (stmt.Condition.TypeReference.ClrType != typeof(bool))
            {
                TypeValidationErrorCallback(new TypeValidationError(
                    (stmt.Condition as ITokenAware)?.Token,
                    $"'{stmt.Condition.TypeReference.ClrType.ToTypeKeyword()}' is not a valid 'while' condition."
                ));
            }

            return VoidObject.Void;
        }
    }
}
