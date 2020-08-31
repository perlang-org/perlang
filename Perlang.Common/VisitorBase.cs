using System.Collections.Generic;

namespace Perlang
{
    /// <summary>
    /// Abstract base class for expression visitors.
    ///
    /// All the methods implemented in this class are no-op, but they do their best to try and make the whole tree
    /// be traversed. The idea is to give child classes an opportunity to only override a smaller subset of the full
    /// set of expression types, to avoid unnecessary boilerplate code.
    /// </summary>
    public abstract class VisitorBase : Expr.IVisitor<VoidObject>, Stmt.IVisitor<VoidObject>
    {
        public virtual VoidObject VisitEmptyExpr(Expr.Empty expr)
        {
            return VoidObject.Void;
        }

        public virtual VoidObject VisitAssignExpr(Expr.Assign expr)
        {
            Visit(expr.Value);

            return VoidObject.Void;
        }

        public virtual VoidObject VisitBinaryExpr(Expr.Binary expr)
        {
            Visit(expr.Left);
            Visit(expr.Right);

            return VoidObject.Void;
        }

        public virtual VoidObject VisitCallExpr(Expr.Call expr)
        {
            Visit(expr.Callee);

            foreach (Expr argument in expr.Arguments)
            {
                Visit(argument);
            }

            return VoidObject.Void;
        }

        public virtual VoidObject VisitGroupingExpr(Expr.Grouping expr)
        {
            Visit(expr.Expression);

            return VoidObject.Void;
        }

        /// <summary>
        /// Does not need to be called by child classes; method is no-op.
        /// </summary>
        public virtual VoidObject VisitLiteralExpr(Expr.Literal expr)
        {
            return VoidObject.Void;
        }

        public virtual VoidObject VisitLogicalExpr(Expr.Logical expr)
        {
            Visit(expr.Left);
            Visit(expr.Right);

            return VoidObject.Void;
        }

        public virtual VoidObject VisitUnaryPrefixExpr(Expr.UnaryPrefix expr)
        {
            Visit(expr.Right);

            return VoidObject.Void;
        }

        public virtual VoidObject VisitUnaryPostfixExpr(Expr.UnaryPostfix expr)
        {
            Visit(expr.Left);

            return VoidObject.Void;
        }

        /// <summary>
        /// Need not be called in child classes.
        /// </summary>
        public virtual VoidObject VisitVariableExpr(Expr.Variable expr)
        {
            return VoidObject.Void;
        }

        public virtual VoidObject VisitBlockStmt(Stmt.Block stmt)
        {
            Visit(stmt.Statements);

            return VoidObject.Void;
        }

        public VoidObject VisitClassStmt(Stmt.Class stmt)
        {
            // TODO: visit fields also, once we have implemented support for them.

            foreach (Stmt.Function method in stmt.Methods)
            {
                Visit(method);
            }

            return VoidObject.Void;
        }

        public virtual VoidObject VisitExpressionStmt(Stmt.ExpressionStmt stmt)
        {
            Visit(stmt.Expression);

            return VoidObject.Void;
        }

        public virtual VoidObject VisitFunctionStmt(Stmt.Function stmt)
        {
            Visit(stmt.Body);

            return VoidObject.Void;
        }

        public virtual VoidObject VisitIfStmt(Stmt.If stmt)
        {
            Visit(stmt.Condition);
            Visit(stmt.ThenBranch);

            if (stmt.ElseBranch != null)
            {
                Visit(stmt.ElseBranch);
            }

            return VoidObject.Void;
        }

        public virtual VoidObject VisitPrintStmt(Stmt.Print stmt)
        {
            Visit(stmt.Expression);

            return VoidObject.Void;
        }

        public virtual VoidObject VisitReturnStmt(Stmt.Return stmt)
        {
            if (stmt.Value != null)
            {
                Visit(stmt.Value);
            }

            return VoidObject.Void;
        }

        public virtual VoidObject VisitVarStmt(Stmt.Var stmt)
        {
            if (stmt.Initializer != null)
            {
                Visit(stmt.Initializer);
            }

            return VoidObject.Void;
        }

        public virtual VoidObject VisitWhileStmt(Stmt.While stmt)
        {
            Visit(stmt.Condition);
            Visit(stmt.Body);

            return VoidObject.Void;
        }

        private void Visit(IEnumerable<Stmt> statements)
        {
            foreach (Stmt statement in statements)
            {
                Visit(statement);
            }
        }

        private void Visit(Stmt stmt)
        {
            stmt.Accept(this);
        }

        private void Visit(Expr expr)
        {
            expr.Accept(this);
        }
    }
}
