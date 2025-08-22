#nullable enable
using System;
using System.Collections.Immutable;
using Perlang.Parser;
using static Perlang.TokenType;

namespace Perlang.Interpreter.CodeAnalysis;

/// <summary>
/// Validator which performs various forms of "code-analysis"-related validation.
///
/// One example of such validation is "valid combinations of expressions". Sometimes, expressions are combined in a
/// way which is "semantically permitted" by our <see cref="PerlangParser"/> class, but the result is still
/// "logically forbidden" or discouraged because of e.g. undesired ambiguity that it forces upon the reader of the
/// code.
///
/// Some of these checks could easily be done in the <see cref="PerlangParser"/> class, but doing so would have the
/// disadvantage of cluttering that class with things not strictly related to the parsing. Even though it does incur
/// a certain overhead, keeping these validations in a separate class feels worthwhile. It also has another
/// advantage: the validation here can presume that a full, already-parsed syntax tree is already available at this
/// point.
/// </summary>
internal class CodeAnalysisValidator : VisitorBase
{
    private readonly Action<CompilerWarning> compilerWarningCallback;

    public static void Validate(
        ImmutableList<Stmt> statements,
        Action<CompilerWarning> compilerWarningCallback)
    {
        // Moving forward, this validation could be split into multiple classes that get called at this point, like
        // the TypeValidator does its work. For now, it would just be overkill though.
        new CodeAnalysisValidator(compilerWarningCallback)
            .Visit(statements);
    }

    private CodeAnalysisValidator(Action<CompilerWarning> compilerWarningCallback)
    {
        this.compilerWarningCallback = compilerWarningCallback;
    }

    public override VoidObject VisitLogicalExpr(Expr.Logical expr)
    {
        base.VisitLogicalExpr(expr);

        if (expr.Operator.Type is AMPERSAND_AMPERSAND or PIPE_PIPE)
        {
            if (expr.Left is Expr.Logical logicalLeftExpr &&
                logicalLeftExpr.Operator.Type != expr.Operator.Type)
            {
                compilerWarningCallback(
                    new CompilerWarning(
                        $"Invalid combination of boolean operators: {logicalLeftExpr.Operator.Lexeme} and " +
                        $"{expr.Operator.Lexeme}. To avoid ambiguity for the reader, grouping parentheses () must " +
                        "be used.",
                        expr.Token,
                        WarningType.AMBIGUOUS_COMBINATION_OF_BOOLEAN_OPERATORS
                    )
                );
            }
            else if (expr.Right is Expr.Logical logicalRightExpr &&
                     expr.Operator.Type != logicalRightExpr.Operator.Type)
            {
                compilerWarningCallback(
                    new CompilerWarning(
                        $"Invalid combination of boolean operators: {expr.Operator.Lexeme} and " +
                        $"{logicalRightExpr.Operator.Lexeme}. To avoid ambiguity for the reader, grouping parentheses () " +
                        "must be used.",
                        expr.Token,
                        WarningType.AMBIGUOUS_COMBINATION_OF_BOOLEAN_OPERATORS
                    )
                );
            }
        }

        return VoidObject.Void;
    }
}