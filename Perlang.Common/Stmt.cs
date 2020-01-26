using System.Collections.Generic;

namespace Perlang
{
    public abstract class Stmt
    {
        public interface IVisitor<TR>
        {
            TR VisitBlockStmt(Block stmt);
            TR VisitExpressionStmt(ExpressionStmt stmt);
            TR VisitFunctionStmt(Function stmt);
            TR VisitIfStmt(If stmt);
            TR VisitPrintStmt(Print stmt);
            TR VisitReturnStmt(Return stmt);
            TR VisitVarStmt(Var stmt);
            TR VisitWhileStmt(While stmt);
        }

        public class Block : Stmt
        {
            public List<Stmt> Statements { get; }

            public Block(List<Stmt> statements) {
                Statements = statements;
            }

            public override TR Accept<TR>(IVisitor<TR> visitor)
            {
                return visitor.VisitBlockStmt(this);
            }
        }

        public class ExpressionStmt : Stmt
        {
            public Expr Expression { get; }

            public ExpressionStmt(Expr expression) {
                Expression = expression;
            }

            public override TR Accept<TR>(IVisitor<TR> visitor)
            {
                return visitor.VisitExpressionStmt(this);
            }
        }

        public class Function : Stmt
        {
            public Token Name { get; }
            public List<Token> Params { get; }
            public List<Stmt> Body { get; }

            public Function(Token name, List<Token> _params, List<Stmt> body) {
                Name = name;
                Params = _params;
                Body = body;
            }

            public override TR Accept<TR>(IVisitor<TR> visitor)
            {
                return visitor.VisitFunctionStmt(this);
            }
        }

        public class If : Stmt
        {
            public Expr Condition { get; }
            public Stmt ThenBranch { get; }
            public Stmt ElseBranch { get; }

            public If(Expr condition, Stmt thenBranch, Stmt elseBranch) {
                Condition = condition;
                ThenBranch = thenBranch;
                ElseBranch = elseBranch;
            }

            public override TR Accept<TR>(IVisitor<TR> visitor)
            {
                return visitor.VisitIfStmt(this);
            }
        }

        public class Print : Stmt
        {
            public Expr Expression { get; }

            public Print(Expr expression) {
                Expression = expression;
            }

            public override TR Accept<TR>(IVisitor<TR> visitor)
            {
                return visitor.VisitPrintStmt(this);
            }
        }

        public class Return : Stmt
        {
            public Token Keyword { get; }
            public Expr Value { get; }

            public Return(Token keyword, Expr value) {
                Keyword = keyword;
                Value = value;
            }

            public override TR Accept<TR>(IVisitor<TR> visitor)
            {
                return visitor.VisitReturnStmt(this);
            }
        }

        public class Var : Stmt
        {
            public Token Name { get; }
            public Expr Initializer { get; }

            public Var(Token name, Expr initializer) {
                Name = name;
                Initializer = initializer;
            }

            public override TR Accept<TR>(IVisitor<TR> visitor)
            {
                return visitor.VisitVarStmt(this);
            }
        }

        public class While : Stmt
        {
            public Expr Condition { get; }
            public Stmt Body { get; }

            public While(Expr condition, Stmt body) {
                Condition = condition;
                Body = body;
            }

            public override TR Accept<TR>(IVisitor<TR> visitor)
            {
                return visitor.VisitWhileStmt(this);
            }
        }

        public abstract TR Accept<TR>(IVisitor<TR> visitor);
    }
}
