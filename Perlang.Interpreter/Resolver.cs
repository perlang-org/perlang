using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Perlang.Parser;

namespace Perlang.Interpreter
{
    class Resolver : Expr.IVisitor<VoidObject>, Stmt.IVisitor<VoidObject>
    {
        private readonly Stack<IDictionary<string, bool>> scopes = new Stack<IDictionary<string, bool>>();
        private FunctionType currentFunction = FunctionType.None;

        private readonly IInterpreter interpreter;
        private readonly ResolveErrorHandler resolveErrorHandler;

        internal Resolver(IInterpreter interpreter, ResolveErrorHandler resolveErrorHandler)
        {
            this.interpreter = interpreter;
            this.resolveErrorHandler = resolveErrorHandler;
        }

        internal void Resolve(IEnumerable<Stmt> statements)
        {
            foreach (Stmt statement in statements)
            {
                Resolve(statement);
            }
        }

        private void BeginScope()
        {
            scopes.Push(new Dictionary<string, bool>());
        }

        private void EndScope()
        {
            scopes.Pop();
        }

        private void Declare(Token name)
        {
            if (IsEmpty(scopes)) return;

            // This adds the variable to the innermost scope so that it shadows any outer one and so that we know the
            // variable exists. We mark it as “not ready yet” by binding its name to false in the scope map. Each value
            // in the scope map means “is finished being initialized”.
            var scope = scopes.Peek();

            if (scope.ContainsKey(name.Lexeme))
            {
                resolveErrorHandler(name, "Variable with this name already declared in this scope.");
            }

            scope[name.Lexeme] = false;
        }

        private static bool IsEmpty(ICollection stack)
        {
            return stack.Count == 0;
        }

        private void Define(Token name)
        {
            if (IsEmpty(scopes)) return;

            // We set the variable’s value in the scope map to true to mark it as fully initialized and available for
            // use. It’s alive!
            scopes.Peek()[name.Lexeme] = true;
        }

        private void ResolveLocal(Expr expr, Token name)
        {
            for (int i = scopes.Count - 1; i >= 0; i--)
            {
                // TODO: rewrite this for performance, since scopes.ElementAt() is much more inefficient on .NET
                // TODO: than the Java counterpart.
                if (scopes.ElementAt(i).ContainsKey(name.Lexeme))
                {
                    interpreter.Resolve(expr, scopes.Count - 1 - i);
                    return;
                }
            }

            // Not found. Assume it is global.                   
        }

        public VoidObject VisitAssignExpr(Expr.Assign expr)
        {
            Resolve(expr.Value);
            ResolveLocal(expr, expr.Name);
            return null;
        }

        public VoidObject VisitBinaryExpr(Expr.Binary expr)
        {
            Resolve(expr.Left);
            Resolve(expr.Right);
            return null;
        }

        public VoidObject VisitCallExpr(Expr.Call expr)
        {
            Resolve(expr.Callee);

            foreach (Expr argument in expr.Arguments)
            {
                Resolve(argument);
            }

            return null;
        }

        public VoidObject VisitGroupingExpr(Expr.Grouping expr)
        {
            Resolve(expr.Expression);
            return null;
        }

        public VoidObject VisitLiteralExpr(Expr.Literal expr)
        {
            return null;
        }

        public VoidObject VisitLogicalExpr(Expr.Logical expr)
        {
            Resolve(expr.Left);
            Resolve(expr.Right);
            
            return null;
        }

        public VoidObject VisitUnaryExpr(Expr.Unary expr)
        {
            Resolve(expr.Right);
            
            return null;
        }

        public VoidObject VisitVariableExpr(Expr.Variable expr)
        {
            if (!IsEmpty(scopes) &&
                scopes.Peek()[expr.Name.Lexeme] == false)
            {
                resolveErrorHandler(expr.Name, "Cannot read local variable in its own initializer.");
            }

            ResolveLocal(expr, expr.Name);
            return null;
        }

        public VoidObject VisitBlockStmt(Stmt.Block stmt)
        {
            BeginScope();
            Resolve(stmt.Statements);
            EndScope();
            return null;
        }

        private void Resolve(Stmt stmt)
        {
            stmt.Accept(this);
        }

        private void Resolve(Expr expr)
        {
            expr.Accept(this);
        }

        public VoidObject VisitExpressionStmt(Stmt.ExpressionStmt stmt)
        {
            Resolve(stmt.Expression);
            return null;
        }

        public VoidObject VisitFunctionStmt(Stmt.Function stmt)
        {
            Declare(stmt.Name);
            Define(stmt.Name);

            ResolveFunction(stmt, FunctionType.Function);
            return null;
        }

        private void ResolveFunction(Stmt.Function function, FunctionType type)
        {
            FunctionType enclosingFunction = currentFunction;
            currentFunction = type;

            BeginScope();

            foreach (Token param in function.Params)
            {
                Declare(param);
                Define(param);
            }

            Resolve(function.Body);
            EndScope();

            currentFunction = enclosingFunction;
        }

        public VoidObject VisitIfStmt(Stmt.If stmt)
        {
            Resolve(stmt.Condition);
            Resolve(stmt.ThenBranch);
            
            if (stmt.ElseBranch != null)
            {
                Resolve(stmt.ElseBranch);
            }

            return null;
        }

        public VoidObject VisitPrintStmt(Stmt.Print stmt)
        {
            Resolve(stmt.Expression);
            
            return null;
        }

        public VoidObject VisitReturnStmt(Stmt.Return stmt)
        {
            if (currentFunction == FunctionType.None)
            {
                resolveErrorHandler(stmt.Keyword, "Cannot return from top-level code.");
            }

            if (stmt.Value != null)
            {
                Resolve(stmt.Value);
            }

            return null;
        }

        public VoidObject VisitVarStmt(Stmt.Var stmt)
        {
            Declare(stmt.Name);
            
            if (stmt.Initializer != null)
            {
                Resolve(stmt.Initializer);
            }

            Define(stmt.Name);
            
            return null;
        }

        public VoidObject VisitWhileStmt(Stmt.While stmt)
        {
            Resolve(stmt.Condition);
            Resolve(stmt.Body);
            
            return null;
        }

        private enum FunctionType
        {
            None,
            Function
        }
    }
}