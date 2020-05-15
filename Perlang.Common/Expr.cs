using System.Collections.Generic;

namespace Perlang
{
    public abstract class Expr
    {
        public interface IVisitor<TR>
        {
            TR VisitEmptyExpr(Empty expr);
            TR VisitAssignExpr(Assign expr);
            TR VisitBinaryExpr(Binary expr);
            TR VisitCallExpr(Call expr);
            TR VisitGroupingExpr(Grouping expr);
            TR VisitLiteralExpr(Literal expr);
            TR VisitLogicalExpr(Logical expr);
            TR VisitUnaryPrefixExpr(UnaryPrefix expr);
            TR VisitUnaryPostfixExpr(UnaryPostfix expr);
            TR VisitVariableExpr(Variable expr);
        }

        public class Empty : Expr
        {
            public override TR Accept<TR>(IVisitor<TR> visitor)
            {
                return visitor.VisitEmptyExpr(this);
            }
        }

        public class Assign : Expr
        {
            public Token Name { get; }
            public Expr Value { get; }

            public Assign(Token name, Expr value) {
                Name = name;
                Value = value;
            }

            public override TR Accept<TR>(IVisitor<TR> visitor)
            {
                return visitor.VisitAssignExpr(this);
            }
        }

        public class Binary : Expr
        {
            public Expr Left { get; }
            public Token Operator { get; }
            public Expr Right { get; }

            public Binary(Expr left, Token _operator, Expr right) {
                Left = left;
                Operator = _operator;
                Right = right;
            }

            public override TR Accept<TR>(IVisitor<TR> visitor)
            {
                return visitor.VisitBinaryExpr(this);
            }
        }

        public class Call : Expr
        {
            public Expr Callee { get; }
            public Token Paren { get; }
            public List<Expr> Arguments { get; }

            public Call(Expr callee, Token paren, List<Expr> arguments) {
                Callee = callee;
                Paren = paren;
                Arguments = arguments;
            }

            public override TR Accept<TR>(IVisitor<TR> visitor)
            {
                return visitor.VisitCallExpr(this);
            }
        }

        public class Grouping : Expr
        {
            public Expr Expression { get; }

            public Grouping(Expr expression) {
                Expression = expression;
            }

            public override TR Accept<TR>(IVisitor<TR> visitor)
            {
                return visitor.VisitGroupingExpr(this);
            }
        }

        public class Literal : Expr
        {
            public object Value { get; }

            public Literal(object value) {
                Value = value;
            }

            public override TR Accept<TR>(IVisitor<TR> visitor)
            {
                return visitor.VisitLiteralExpr(this);
            }
        }

        public class Logical : Expr
        {
            public Expr Left { get; }
            public Token Operator { get; }
            public Expr Right { get; }

            public Logical(Expr left, Token _operator, Expr right) {
                Left = left;
                Operator = _operator;
                Right = right;
            }

            public override TR Accept<TR>(IVisitor<TR> visitor)
            {
                return visitor.VisitLogicalExpr(this);
            }
        }

        public class UnaryPrefix : Expr
        {
            public Token Operator { get; }
            public Expr Right { get; }

            public UnaryPrefix(Token _operator, Expr right) {
                Operator = _operator;
                Right = right;
            }

            public override TR Accept<TR>(IVisitor<TR> visitor)
            {
                return visitor.VisitUnaryPrefixExpr(this);
            }
        }

        public class UnaryPostfix : Expr
        {
            public Expr Left { get; }
            public Token Name { get; }
            public Token Operator { get; }

            public UnaryPostfix(Expr left, Token name, Token _operator) {
                Left = left;
                Name = name;
                Operator = _operator;
            }

            public override TR Accept<TR>(IVisitor<TR> visitor)
            {
                return visitor.VisitUnaryPostfixExpr(this);
            }
        }

        public class Variable : Expr
        {
            public Token Name { get; }

            public Variable(Token name) {
                Name = name;
            }

            public override TR Accept<TR>(IVisitor<TR> visitor)
            {
                return visitor.VisitVariableExpr(this);
            }
        }

        public abstract TR Accept<TR>(IVisitor<TR> visitor);
    }
}
