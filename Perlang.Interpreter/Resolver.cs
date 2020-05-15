using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Perlang.Interpreter.Extensions;
using Perlang.Parser;

namespace Perlang.Interpreter
{
    /// <summary>
    /// The Resolver is responsible for resolving names of local and global variable/function names.
    /// </summary>
    internal class Resolver : Expr.IVisitor<VoidObject>, Stmt.IVisitor<VoidObject>
    {
        private readonly List<IDictionary<string, TypeReference>> scopes = new List<IDictionary<string, TypeReference>>();
        private FunctionType currentFunction = FunctionType.NONE;

        private readonly Action<Expr, int> addLocalExprCallback;
        private readonly ResolveErrorHandler resolveErrorHandler;

        /// <summary>
        /// Creates a new Resolver instance.
        /// </summary>
        /// <param name="addLocalExprCallback">A callback used to add a local expression at a
        /// given depth away from the call site. One level of nesting = one extra level of depth.</param>
        /// <param name="resolveErrorHandler">A callback which will be called in case of resolution errors. Note that
        /// multiple resolution errors will cause the provided callback to be called multiple times.</param>
        internal Resolver(Action<Expr, int> addLocalExprCallback, ResolveErrorHandler resolveErrorHandler)
        {
            this.addLocalExprCallback = addLocalExprCallback;
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
            scopes.Add(new Dictionary<string, TypeReference>());
        }

        private void EndScope()
        {
            scopes.RemoveAt(scopes.Count - 1);
        }

        private void Declare(Token name)
        {
            if (IsEmpty(scopes)) return;

            // This adds the variable to the innermost scope so that it shadows any outer one and so that we know the
            // variable exists.
            var scope = scopes.Last();

            if (scope.ContainsKey(name.Lexeme))
            {
                resolveErrorHandler(new ResolveError
                {
                    Token = name,
                    Message = "Variable with this name already declared in this scope."
                });
            }

            // We mark it as “not ready yet” by binding its name to "null" in the scope map. Each value in the scope
            // map means “is finished being initialized”, at this stage of traversing the tree. Being able to
            // distinguish between uninitialized and initialized values is critical to be able to detect erroneous code
            // like "var a = a".
            scope[name.Lexeme] = null;
        }

        private static bool IsEmpty(ICollection stack)
        {
            return stack.Count == 0;
        }

        private void Define(Token name, TypeReference typeReference)
        {
            if (typeReference == null)
            {
                throw new ArgumentException("typeReference cannot be null");
            }

            if (IsEmpty(scopes)) return;

            // We set the variable’s value in the scope map to mark it as fully initialized and available for
            // use. It’s alive! As an extra bonus, we store the type reference of the initializer (if present).
            // This is useful later on, in the static type analysis, where we must be able to map a given variable
            // not to its value, but to the _type_ of the value produced by the initializer.
            scopes.Last()[name.Lexeme] = typeReference;
        }

        private void ResolveLocal(Expr expr, Token name)
        {
            // Loop over all the scopes, from the innermost and outwards, trying to find a binding for this name.
            for (int i = scopes.Count - 1; i >= 0; i--)
            {
                if (scopes[i].ContainsKey(name.Lexeme))
                {
                    addLocalExprCallback(expr, scopes.Count - 1 - i);
                    return;
                }
            }

            // Not found. Assume it is global.
        }

        public VoidObject VisitEmptyExpr(Expr.Empty expr)
        {
            return null;
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

        public VoidObject VisitUnaryPrefixExpr(Expr.UnaryPrefix expr)
        {
            Resolve(expr.Right);

            return null;
        }

        public VoidObject VisitUnaryPostfixExpr(Expr.UnaryPostfix expr)
        {
            Resolve(expr.Left);
            ResolveLocal(expr, expr.Name);

            return null;
        }

        public VoidObject VisitVariableExpr(Expr.Variable expr)
        {
            // Note: providing the defaultValue in the TryGetObjectValue() call here is critical, since we must
            // be able to distinguish between "set to null" and "not set at all".
            if (!IsEmpty(scopes) &&
                scopes.Last().TryGetObjectValue(expr.Name.Lexeme, TypeReference.None) == null)
            {
                resolveErrorHandler(new ResolveError
                {
                    Token = expr.Name,
                    Message = "Cannot read local variable in its own initializer."
                });
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
            Define(stmt.Name, stmt.ReturnTypeReference);

            ResolveFunction(stmt, FunctionType.FUNCTION);
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
                Define(param, TypeReference.None);
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
            if (currentFunction == FunctionType.NONE)
            {
                resolveErrorHandler(new ResolveError
                {
                    Token = stmt.Keyword,
                    Message = "Cannot return from top-level code."
                });
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

            // TODO: Get the type reference from the initializer instead.
            Define(stmt.Name, TypeReference.None);

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
            NONE,
            FUNCTION
        }
    }
}
