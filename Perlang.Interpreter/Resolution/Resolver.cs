using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Perlang.Interpreter.Extensions;

namespace Perlang.Interpreter.Resolution
{
    /// <summary>
    /// The Resolver is responsible for resolving names of local and global variable/function names.
    /// </summary>
    internal class Resolver : Expr.IVisitor<VoidObject>, Stmt.IVisitor<VoidObject>
    {
        private readonly ImmutableDictionary<string, TypeReferenceNativeFunction> globalCallables;

        private readonly List<IDictionary<string, IBindingFactory>> scopes =
            new List<IDictionary<string, IBindingFactory>>();

        private readonly IDictionary<string, IBindingFactory> globals =
            new Dictionary<string, IBindingFactory>();

        private FunctionType currentFunction = FunctionType.NONE;

        private readonly Action<Binding> addLocalExprCallback;
        private readonly Action<Binding> addGlobalExprCallback;
        private readonly ResolveErrorHandler resolveErrorHandler;

        /// <summary>
        /// Creates a new Resolver instance.
        /// </summary>
        /// <param name="globalCallables">a dictionary of global callables</param>
        /// <param name="addLocalExprCallback">A callback used to add an expression to a local scope at a
        /// given depth away from the call site. One level of nesting = one extra level of depth.</param>
        /// <param name="addGlobalExprCallback">A callback used to add an expression to the global scope.</param>
        /// <param name="resolveErrorHandler">A callback which will be called in case of resolution errors. Note that
        /// multiple resolution errors will cause the provided callback to be called multiple times.</param>
        internal Resolver(ImmutableDictionary<string, TypeReferenceNativeFunction> globalCallables,
            Action<Binding> addLocalExprCallback,
            Action<Binding> addGlobalExprCallback, ResolveErrorHandler resolveErrorHandler)
        {
            this.globalCallables = globalCallables;
            this.addLocalExprCallback = addLocalExprCallback;
            this.addGlobalExprCallback = addGlobalExprCallback;
            this.resolveErrorHandler = resolveErrorHandler;
        }

        internal void Resolve(IEnumerable<Stmt> statements)
        {
            foreach (Stmt statement in statements)
            {
                Resolve(statement);
            }
        }

        internal void Resolve(Expr expr)
        {
            expr.Accept(this);
        }

        private void BeginScope()
        {
            scopes.Add(new Dictionary<string, IBindingFactory>());
        }

        private void EndScope()
        {
            scopes.RemoveAt(scopes.Count - 1);
        }

        /// <summary>
        /// Declares a variable or function as existing (but not yet initialized) in the innermost scope. This allows
        /// the variable to shadow variables in outer scopes with the same name.
        /// </summary>
        /// <param name="name">The name of the variable or function.</param>
        private void Declare(Token name)
        {
            if (IsEmpty(scopes)) return;

            // This adds the variable to the innermost scope so that it shadows any outer one and so that we know the
            // variable exists.
            var scope = scopes.Last();

            if (scope.ContainsKey(name.Lexeme))
            {
                resolveErrorHandler(new ResolveError("Variable with this name already declared in this scope.", name));
            }

            // We mark it as “not ready yet” by binding a known None-value in the scope map. Each value in the scope
            // map means “is finished being initialized”, at this stage of traversing the tree. Being able to
            // distinguish between uninitialized and initialized values is critical to be able to detect erroneous code
            // like "var a = a".
            scope[name.Lexeme] = VariableBindingFactory.None;
        }

        private static bool IsEmpty(ICollection stack)
        {
            return stack.Count == 0;
        }

        /// <summary>
        /// Defines a previously declared variable as initialized, available for use.
        /// </summary>
        /// <param name="name">The variable or function name.</param>
        /// <param name="typeReference">A TypeReference describing the variable or function.</param>
        /// <exception cref="ArgumentException">If typeReference is null.</exception>
        private void Define(Token name, TypeReference typeReference)
        {
            if (typeReference == null)
            {
                throw new ArgumentException("typeReference cannot be null");
            }

            if (IsEmpty(scopes))
            {
                globals[name.Lexeme] = new VariableBindingFactory(typeReference);
                return;
            }

            // We set the variable’s value in the scope map to mark it as fully initialized and available for
            // use. It’s alive! As an extra bonus, we store the type reference of the initializer (if present), or the
            // function return type and function statement (in case of a function being defined). These details are
            // useful later on, in the static type analysis.
            scopes.Last()[name.Lexeme] = new VariableBindingFactory(typeReference);
        }

        /// <summary>
        /// Defines a previously declared function as defined, available for use.
        /// </summary>
        /// <param name="name">The variable or function name.</param>
        /// <param name="typeReference">A TypeReference describing the variable or function.</param>
        /// <param name="function">The function statement should be provided here.</param>
        private void DefineFunction(Token name, TypeReference typeReference, Stmt.Function function)
        {
            if (typeReference == null)
            {
                throw new ArgumentException("typeReference cannot be null");
            }

            if (IsEmpty(scopes))
            {
                globals[name.Lexeme] = new FunctionBindingFactory(typeReference, function);
                return;
            }

            // We set the variable’s value in the scope map to mark it as fully initialized and available for
            // use. It’s alive! As an extra bonus, we store the type reference of the initializer (if present), or the
            // function return type and function statement (in case of a function being defined). These details are
            // useful later on, in the static type analysis.
            scopes.Last()[name.Lexeme] = new FunctionBindingFactory(typeReference, function);
        }

        private void ResolveLocal(Expr referringExpr, Token name)
        {
            // Loop over all the scopes, from the innermost and outwards, trying to find a binding for this name.
            for (int i = scopes.Count - 1; i >= 0; i--)
            {
                if (scopes[i].ContainsKey(name.Lexeme))
                {
                    IBindingFactory bindingFactory = scopes[i][name.Lexeme];

                    if (bindingFactory == VariableBindingFactory.None)
                    {
                        resolveErrorHandler(
                            new ResolveError("Cannot read local variable in its own initializer.", name));
                        return;
                    }

                    addLocalExprCallback(bindingFactory.CreateBinding(scopes.Count - 1 - i, referringExpr));

                    return;
                }
            }

            // Not found in the local scopes. Could we perhaps be dealing with a native (.NET) callable?
            if (globalCallables.ContainsKey(name.Lexeme))
            {
                var globalTypeReferenceAndCallable = globalCallables[name.Lexeme];

                addGlobalExprCallback(new NativeBinding(globalTypeReferenceAndCallable.Method, name.Lexeme,
                    globalTypeReferenceAndCallable.ParameterTypes, globalTypeReferenceAndCallable.ReturnTypeReference,
                    referringExpr));
            }

            // Not found in any of the local scopes. Assume it is global, or non-existent.
            if (!globals.ContainsKey(name.Lexeme))
            {
                return;
            }

            // Note: the extra block here is actually not just "for fun". We get a conflict with the bindingFactory
            // in the for-loop above if we skip it.
            {
                IBindingFactory bindingFactory = globals[name.Lexeme];
                addGlobalExprCallback(bindingFactory.CreateBinding(-1, referringExpr));
            }
        }

        public VoidObject VisitEmptyExpr(Expr.Empty expr)
        {
            return VoidObject.Void;
        }

        public VoidObject VisitAssignExpr(Expr.Assign expr)
        {
            Resolve(expr.Value);
            ResolveLocal(expr, expr.Name);

            return VoidObject.Void;
        }

        public VoidObject VisitBinaryExpr(Expr.Binary expr)
        {
            Resolve(expr.Left);
            Resolve(expr.Right);

            return VoidObject.Void;
        }

        public VoidObject VisitCallExpr(Expr.Call expr)
        {
            Resolve(expr.Callee);

            foreach (Expr argument in expr.Arguments)
            {
                Resolve(argument);
            }

            if (expr.Callee is Expr.Identifier identifierExpr)
            {
                ResolveLocal(expr, identifierExpr.Name);
            }

            return VoidObject.Void;
        }

        public VoidObject VisitGroupingExpr(Expr.Grouping expr)
        {
            Resolve(expr.Expression);
            return VoidObject.Void;
        }

        public VoidObject VisitLiteralExpr(Expr.Literal expr)
        {
            return VoidObject.Void;
        }

        public VoidObject VisitLogicalExpr(Expr.Logical expr)
        {
            Resolve(expr.Left);
            Resolve(expr.Right);

            return VoidObject.Void;
        }

        public VoidObject VisitUnaryPrefixExpr(Expr.UnaryPrefix expr)
        {
            Resolve(expr.Right);

            return VoidObject.Void;
        }

        public VoidObject VisitUnaryPostfixExpr(Expr.UnaryPostfix expr)
        {
            Resolve(expr.Left);
            ResolveLocal(expr, expr.Name);

            return VoidObject.Void;
        }

        public VoidObject VisitIdentifierExpr(Expr.Identifier expr)
        {
            // Note: providing the defaultValue in the TryGetObjectValue() call here is critical, since we must
            // be able to distinguish between "set to null" and "not set at all".
            if (!IsEmpty(scopes) &&
                scopes.Last().TryGetObjectValue(expr.Name.Lexeme, VariableBindingFactory.None) == null)
            {
                resolveErrorHandler(new ResolveError("Cannot read local variable in its own initializer.", expr.Name));
            }

            ResolveLocal(expr, expr.Name);

            return VoidObject.Void;
        }

        public VoidObject VisitBlockStmt(Stmt.Block stmt)
        {
            BeginScope();
            Resolve(stmt.Statements);
            EndScope();
            return VoidObject.Void;
        }

        private void Resolve(Stmt stmt)
        {
            stmt.Accept(this);
        }

        public VoidObject VisitExpressionStmt(Stmt.ExpressionStmt stmt)
        {
            Resolve(stmt.Expression);
            return VoidObject.Void;
        }

        public VoidObject VisitFunctionStmt(Stmt.Function stmt)
        {
            Declare(stmt.Name);
            DefineFunction(stmt.Name, stmt.ReturnTypeReference, stmt);

            ResolveFunction(stmt, FunctionType.FUNCTION);

            return VoidObject.Void;
        }

        private void ResolveFunction(Stmt.Function function, FunctionType type)
        {
            FunctionType enclosingFunction = currentFunction;
            currentFunction = type;

            BeginScope();

            foreach (Parameter param in function.Parameters)
            {
                Declare(param.Name);
                Define(param.Name, new TypeReference(param.TypeSpecifier));
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

            return VoidObject.Void;
        }

        public VoidObject VisitPrintStmt(Stmt.Print stmt)
        {
            Resolve(stmt.Expression);
            return VoidObject.Void;
        }

        public VoidObject VisitReturnStmt(Stmt.Return stmt)
        {
            if (currentFunction == FunctionType.NONE)
            {
                resolveErrorHandler(new ResolveError("Cannot return from top-level code.", stmt.Keyword));
            }

            if (stmt.Value != null)
            {
                Resolve(stmt.Value);
            }

            return VoidObject.Void;
        }

        public VoidObject VisitVarStmt(Stmt.Var stmt)
        {
            Declare(stmt.Name);

            if (stmt.Initializer != null)
            {
                Resolve(stmt.Initializer);
            }

            Define(stmt.Name, stmt.Initializer?.TypeReference ?? new TypeReference(stmt.Name));

            return VoidObject.Void;
        }

        public VoidObject VisitWhileStmt(Stmt.While stmt)
        {
            Resolve(stmt.Condition);
            Resolve(stmt.Body);

            return VoidObject.Void;
        }

        private enum FunctionType
        {
            NONE,
            FUNCTION
        }
    }
}
