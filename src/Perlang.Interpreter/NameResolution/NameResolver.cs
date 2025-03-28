#nullable enable
#pragma warning disable S1199
#pragma warning disable S4136

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Perlang.Compiler;
using Perlang.Interpreter.Extensions;
using Perlang.Interpreter.Internals;

namespace Perlang.Interpreter.NameResolution;

/// <summary>
/// The `NameResolver` is responsible for resolving names of local and global variable/function names.
/// </summary>
internal class NameResolver : VisitorBase
{
    private readonly IBindingHandler bindingHandler;
    private readonly Action<string, IPerlangClass> addGlobalClassCallback;
    private readonly NameResolutionErrorHandler nameResolutionErrorHandler;

    /// <summary>
    /// An instance-local list of global symbols (variables, functions etc.)
    /// </summary>
    private readonly IDictionary<string, IBindingFactory> globals = new Dictionary<string, IBindingFactory>();

    internal IDictionary<string, IBindingFactory> Globals => globals;

    /// <summary>
    /// An instance-local list of scopes (for local symbols). The innermost scope is always the last entry in this list.
    /// As the code is being traversed, scopes are created and removed as blocks are opened/closed.
    /// </summary>
    private readonly List<IDictionary<string, IBindingFactory>> scopes = new List<IDictionary<string, IBindingFactory>>();

    private readonly Dictionary<Stmt, Dictionary<string, IBindingFactory>> stmtScopes = new Dictionary<Stmt, Dictionary<string, IBindingFactory>>();

    private FunctionType currentFunction = FunctionType.NONE;
    private bool firstPass = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="NameResolver"/> class.
    /// </summary>
    /// <param name="globalClasses">A dictionary of global classes, with the class name as key.</param>
    /// <param name="superGlobals">A dictionary of "super-globals"; a set of pre-defined global variables, coming
    /// from outside of the program itself.</param>
    /// <param name="bindingHandler">A handler used for adding local and global bindings.</param>
    /// <param name="addGlobalClassCallback">A callback used to add a global, top-level class.</param>
    /// <param name="nameResolutionErrorHandler">A callback which will be called in case of name resolution errors.
    /// Note that multiple resolution errors will cause the provided callback to be called multiple times.</param>
    internal NameResolver(
        IImmutableDictionary<string, Type> globalClasses,
        IImmutableDictionary<string, Type> superGlobals,
        IBindingHandler bindingHandler,
        Action<string, IPerlangClass> addGlobalClassCallback,
        NameResolutionErrorHandler nameResolutionErrorHandler)
    {
        this.bindingHandler = bindingHandler;
        this.addGlobalClassCallback = addGlobalClassCallback;
        this.nameResolutionErrorHandler = nameResolutionErrorHandler;

        foreach ((string key, Type value) in globalClasses)
        {
            globals[key] = new NativeClassBindingFactory(value);
        }

        foreach ((string key, Type value) in superGlobals)
        {
            globals[key] = new NativeObjectBindingFactory(value);
        }
    }

    internal void Resolve(IEnumerable<Stmt> statements)
    {
        foreach (Stmt statement in statements)
        {
            Resolve(statement);
        }
    }

    internal void StartSecondPass()
    {
        firstPass = false;
    }

    private void Resolve(Expr expr)
    {
        expr.Accept(this);
    }

    private void BeginScope(Stmt stmt)
    {
        if (stmtScopes.TryGetValue(stmt, out var scope)) {
            scopes.Add(scope);
            return;
        }

        scope = new Dictionary<string, IBindingFactory>();
        scopes.Add(scope);

        stmtScopes[stmt] = scope;
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
        if (IsEmpty(scopes))
        {
            return;
        }

        // This adds the variable to the innermost scope so that it shadows any outer one and so that we know the
        // variable exists.
        var scope = scopes.Last();

        if (scope.ContainsKey(name.Lexeme))
        {
            if (!firstPass) {
                // This is expected on the second pass.
                return;
            }

            nameResolutionErrorHandler(new NameResolutionError("Variable with this name already declared in this scope.", name));
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
    /// <param name="name">The variable.</param>
    /// <param name="typeReference">An `ITypeReference` describing the variable or function.</param>
    /// <exception cref="ArgumentException">`typeReference` is null.</exception>
    private void DefineVariable(string name, ITypeReference typeReference)
    {
        if (typeReference == null)
        {
            throw new ArgumentException("typeReference cannot be null");
        }

        if (IsEmpty(scopes))
        {
            globals[name] = new VariableBindingFactory(typeReference);
            return;
        }

        // We set up a new VariableBindingFactory in the scope map to mark the variable as fully initialized and available
        // for use. This factory is useful later on, for e.g. static type analysis.
        scopes.Last()[name] = new VariableBindingFactory(typeReference);
    }

    /// <summary>
    /// Defines a previously declared function as defined, available for use.
    /// </summary>
    /// <param name="name">The variable or function name.</param>
    /// <param name="typeReference">An `ITypeReference` describing the variable or function.</param>
    /// <param name="function">The function statement should be provided here.</param>
    private void DefineFunction(Token name, ITypeReference typeReference, Stmt.Function function)
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

        // We set up a new FunctionBindingFactory in the scope map to mark the variable as fully initialized and available
        // for use. This factory is useful later on, for e.g. static type analysis.
        scopes.Last()[name.Lexeme] = new FunctionBindingFactory(typeReference, function);
    }

    private void DefineClass(Token name, IPerlangClass perlangClass)
    {
        if (globals.TryGetValue(name.Lexeme, out IBindingFactory? bindingFactory))
        {
            if (!firstPass) {
                // This is expected on the second pass.
                return;
            }

            nameResolutionErrorHandler(new NameResolutionError($"{bindingFactory.ObjectTypeTitleized} {name.Lexeme} already defined; cannot redefine", name));
            return;
        }

        globals[name.Lexeme] = new ClassBindingFactory(perlangClass);
        addGlobalClassCallback(name.Lexeme, perlangClass);
    }

    private void DefineThis(Stmt.Class classStmt, IPerlangClass perlangClass)
    {
        DefineVariable("this", classStmt.TypeReference);

        // These technically don't belong in the name resolving phase, but we need them for the type inference to work, and
        // we don't have the PerlangClass instance available in the TypeResolver class right now.
        classStmt.TypeReference.SetCppType(new CppType(perlangClass.Name, WrapInSharedPtr: true));
        classStmt.TypeReference.SetPerlangClass(perlangClass);
    }

    private void DefineField(Token name, ITypeReference typeReference, Stmt.Field field)
    {
        ArgumentNullException.ThrowIfNull(typeReference);

        if (IsEmpty(scopes))
        {
            globals[name.Lexeme] = new FieldBindingFactory(typeReference, field);
            return;
        }

        // The binding factory is used in the type resolving phase.
        scopes.Last()[name.Lexeme] = new FieldBindingFactory(typeReference, field);
    }

    // TODO: Should preferably receive a Dictionary<string, object> here with enum members pre-evaluated, since they are expected to be compile-time constants
    private void DefineEnum(Token name, Dictionary<string, Expr?> enumMembers)
    {
        if (globals.TryGetValue(name.Lexeme, out IBindingFactory? bindingFactory))
        {
            if (!firstPass) {
                // This is expected on the second pass.
                return;
            }

            nameResolutionErrorHandler(new NameResolutionError($"{bindingFactory.ObjectTypeTitleized} {name.Lexeme} already defined; cannot redefine", name));
            return;
        }

        var perlangEnum = new PerlangEnum(name, enumMembers);
        globals[name.Lexeme] = new EnumBindingFactory(perlangEnum);
    }

    private void ResolveLocalOrGlobal(Expr referringExpr, Token name)
    {
        // Loop over all the scopes, from the innermost and outwards, trying to find a registered "binding factory"
        // that matches this name.
        for (int i = scopes.Count - 1; i >= 0; i--)
        {
            if (scopes[i].ContainsKey(name.Lexeme))
            {
                IBindingFactory bindingFactory = scopes[i][name.Lexeme];

                if (bindingFactory == VariableBindingFactory.None)
                {
                    nameResolutionErrorHandler(
                        new NameResolutionError("Cannot read local variable in its own initializer.", name));
                    return;
                }

                bindingHandler.AddLocalExpr(bindingFactory.CreateBinding(scopes.Count - 1 - i, referringExpr));

                return;
            }
        }

        // The identifier was not found in any of the local scopes. If it cannot be found in the globals, we can
        // safely assume it is non-existent.
        if (!globals.ContainsKey(name.Lexeme))
        {
            return;
        }

        // Note: the extra block here is actually not just "for fun". We get a conflict with the bindingFactory
        // in the for-loop above if we skip it.
        {
            IBindingFactory bindingFactory = globals[name.Lexeme];
            bindingHandler.AddGlobalExpr(bindingFactory.CreateBinding(-1, referringExpr));
        }
    }

    public override VoidObject VisitEmptyExpr(Expr.Empty expr)
    {
        return VoidObject.Void;
    }

    public override VoidObject VisitAssignExpr(Expr.Assign expr)
    {
        Resolve(expr.Value);

        if (expr.Target is Expr.Identifier identifier) {
            ResolveLocalOrGlobal(expr, identifier.Name);
        }
        else if (expr.Target is Expr.Get get) {
            Resolve(get.Object);

            // TODO: This feels weird; get.Name is likely a local or global variable at all but rather a foo.bar.baz
            // TODO: expression. However, if we _don't_ do this, we break the immutability checker. There's something
            // TODO: really fishy going on here but I haven't gotten to the bottom of it yet...
            ResolveLocalOrGlobal(expr, get.Name);
        }
        else {
            throw new PerlangCompilerException($"Unsupported expression type encountered: {expr.Target}");
        }

        return VoidObject.Void;
    }

    public override VoidObject VisitBinaryExpr(Expr.Binary expr)
    {
        Resolve(expr.Left);
        Resolve(expr.Right);

        return VoidObject.Void;
    }

    public override VoidObject VisitCallExpr(Expr.Call expr)
    {
        Resolve(expr.Callee);

        foreach (Expr argument in expr.Arguments)
        {
            Resolve(argument);
        }

        if (expr.Callee is Expr.Identifier identifierExpr)
        {
            ResolveLocalOrGlobal(expr, identifierExpr.Name);
        }

        return VoidObject.Void;
    }

    public override VoidObject VisitIndexExpr(Expr.Index expr)
    {
        Resolve(expr.Indexee);
        Resolve(expr.Argument);

        if (expr.Indexee is Expr.Identifier identifierExpr)
        {
            ResolveLocalOrGlobal(expr, identifierExpr.Name);
        }

        return VoidObject.Void;
    }

    public override VoidObject VisitGroupingExpr(Expr.Grouping expr)
    {
        Resolve(expr.Expression);
        return VoidObject.Void;
    }

    public override VoidObject VisitLiteralExpr(Expr.Literal expr)
    {
        return VoidObject.Void;
    }

    public override VoidObject VisitLogicalExpr(Expr.Logical expr)
    {
        Resolve(expr.Left);
        Resolve(expr.Right);

        return VoidObject.Void;
    }

    public override VoidObject VisitUnaryPrefixExpr(Expr.UnaryPrefix expr)
    {
        Resolve(expr.Right);

        return VoidObject.Void;
    }

    public override VoidObject VisitUnaryPostfixExpr(Expr.UnaryPostfix expr)
    {
        Resolve(expr.Left);
        ResolveLocalOrGlobal(expr, expr.Name);

        return VoidObject.Void;
    }

    public override VoidObject VisitIdentifierExpr(Expr.Identifier expr)
    {
        // Note: providing the defaultValue in the TryGetObjectValue() call here is critical, since we must be able
        // to distinguish between "set to null" and "not set at all".
        if (!IsEmpty(scopes) &&
            scopes.Last().TryGetObjectValue(expr.Name.Lexeme, VariableBindingFactory.None) == null)
        {
            nameResolutionErrorHandler(new NameResolutionError("Cannot read local variable in its own initializer.", expr.Name));
        }

        ResolveLocalOrGlobal(expr, expr.Name);

        return VoidObject.Void;
    }

    public override VoidObject VisitGetExpr(Expr.Get expr)
    {
        Resolve(expr.Object);

        return VoidObject.Void;
    }

    public override VoidObject VisitNewExpression(Expr.NewExpression expr)
    {
        // This is a bit excessive; we could restrict to only resolve the type name in the list of types currently in
        // scope. The problem with this approach is that it makes silly things like `var i = 0; var j = new i();`
        // possible.
        ResolveLocalOrGlobal(expr, expr.TypeName);

        return VoidObject.Void;
    }

    public override VoidObject VisitBlockStmt(Stmt.Block stmt)
    {
        BeginScope(stmt);
        Resolve(stmt.Statements);
        EndScope();

        return VoidObject.Void;
    }

    public override VoidObject VisitClassStmt(Stmt.Class stmt)
    {
        Declare(stmt.NameToken);

        DefineClass(stmt.NameToken, stmt);

        // This part (creating a scope, visiting the functions like this) is needed to be able to call methods without
        // explicit `this.` prefix.
        BeginScope(stmt);

        // Make the current class available to its methods, using both explicit `this.foo()` and implicit `foo()`
        // notations.
        DefineThis(stmt, stmt);

        foreach (Stmt.Field field in stmt.Fields) {
            VisitFieldStmt(field);
        }

        foreach (Stmt.Function method in stmt.Methods) {
            VisitFunctionStmt(method);
        }

        EndScope();

        return VoidObject.Void;
    }

    private void Resolve(Stmt stmt)
    {
        stmt.Accept(this);
    }

    public override VoidObject VisitEnumStmt(Stmt.Enum stmt)
    {
        DefineEnum(stmt.Name, stmt.Members);
        return VoidObject.Void;
    }

    public override VoidObject VisitExpressionStmt(Stmt.ExpressionStmt stmt)
    {
        Resolve(stmt.Expression);
        return VoidObject.Void;
    }

    public override VoidObject VisitFunctionStmt(Stmt.Function stmt)
    {
        Declare(stmt.Name);
        DefineFunction(stmt.Name, stmt.ReturnTypeReference, stmt);

        ResolveFunction(stmt, FunctionType.FUNCTION);

        return VoidObject.Void;
    }

    public override VoidObject VisitFieldStmt(Stmt.Field stmt)
    {
        Declare(stmt.Name);
        DefineField(stmt.Name, stmt.TypeReference, stmt);

        return VoidObject.Void;
    }

    private void ResolveFunction(Stmt.Function function, FunctionType type)
    {
        FunctionType enclosingFunction = currentFunction;
        currentFunction = type;

        BeginScope(function);

        foreach (Parameter param in function.Parameters)
        {
            Declare(param.Name);
            DefineVariable(param.Name.Lexeme, new TypeReference(param.TypeSpecifier, param.IsArray));
        }

        Resolve(function.Body);
        EndScope();

        currentFunction = enclosingFunction;
    }

    public override VoidObject VisitIfStmt(Stmt.If stmt)
    {
        Resolve(stmt.Condition);
        Resolve(stmt.ThenBranch);

        if (stmt.ElseBranch != null)
        {
            Resolve(stmt.ElseBranch);
        }

        return VoidObject.Void;
    }

    public override VoidObject VisitPrintStmt(Stmt.Print stmt)
    {
        Resolve(stmt.Expression);
        return VoidObject.Void;
    }

    public override VoidObject VisitReturnStmt(Stmt.Return stmt)
    {
        if (currentFunction == FunctionType.NONE)
        {
            nameResolutionErrorHandler(new NameResolutionError("Cannot return from top-level code.", stmt.Keyword));
        }

        if (stmt.Value != null)
        {
            Resolve(stmt.Value);
        }

        return VoidObject.Void;
    }

    public override VoidObject VisitVarStmt(Stmt.Var stmt)
    {
        Declare(stmt.Name);

        if (stmt.Initializer != null)
        {
            Resolve(stmt.Initializer);
        }

        DefineVariable(stmt.Name.Lexeme, stmt.TypeReference);

        return VoidObject.Void;
    }

    public override VoidObject VisitWhileStmt(Stmt.While stmt)
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
