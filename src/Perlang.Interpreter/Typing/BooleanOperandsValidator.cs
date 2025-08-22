using System;
using Perlang.Interpreter.Internals;

namespace Perlang.Interpreter.Typing;

/// <summary>
/// Visitor which ensures that all expressions which expect boolean operands have been passed values which can be
/// coerced to `bool`.
/// </summary>
internal class BooleanOperandsValidator : Validator
{
    public BooleanOperandsValidator(
        IBindingRetriever variableOrFunctionRetriever,
        Action<TypeValidationError> typeValidationErrorCallback)
        : base(variableOrFunctionRetriever, typeValidationErrorCallback)
    {
    }

    public override VoidObject VisitLogicalExpr(Expr.Logical expr)
    {
        base.VisitLogicalExpr(expr);

        if (expr.Left.TypeReference.CppType != PerlangValueTypes.Bool)
        {
            TypeValidationErrorCallback(new TypeValidationError(
                expr.Token,
                $"'{expr.Left.TypeReference.TypeKeywordOrPerlangType}' is not a valid {expr.Operator.Lexeme} operand."
            ));
        }

        if (expr.Right.TypeReference.CppType != PerlangValueTypes.Bool)
        {
            TypeValidationErrorCallback(new TypeValidationError(
                expr.Token,
                $"'{expr.Right.TypeReference.TypeKeywordOrPerlangType}' is not a valid {expr.Operator.Lexeme} operand."
            ));
        }

        return VoidObject.Void;
    }

    public override VoidObject VisitIfStmt(Stmt.If stmt)
    {
        base.VisitIfStmt(stmt);

        if (stmt.Condition.TypeReference.CppType != PerlangValueTypes.Bool)
        {
            TypeValidationErrorCallback(new TypeValidationError(
                (stmt.Condition as ITokenAware)?.Token,
                $"'{stmt.Condition.TypeReference.TypeKeywordOrPerlangType}' is not a valid 'if' condition."
            ));
        }

        return VoidObject.Void;
    }

    public override VoidObject VisitWhileStmt(Stmt.While stmt)
    {
        base.VisitWhileStmt(stmt);

        if (stmt.Condition.TypeReference.CppType != PerlangValueTypes.Bool)
        {
            TypeValidationErrorCallback(new TypeValidationError(
                (stmt.Condition as ITokenAware)?.Token,
                $"'{stmt.Condition.TypeReference.TypeKeywordOrPerlangType}' is not a valid 'while' condition."
            ));
        }

        return VoidObject.Void;
    }
}