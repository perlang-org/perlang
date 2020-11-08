#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Perlang.Attributes;
using Perlang.Exceptions;
using Perlang.Interpreter.Resolution;
using Perlang.Interpreter.Typing;
using Perlang.Parser;
using static Perlang.TokenType;
using static Perlang.Utils;

namespace Perlang.Interpreter
{
    /// <summary>
    /// Interpreter for Perlang code.
    ///
    /// This class is not thread safe; calling <see cref="Eval"/> on multiple threads simultaneously can lead to race
    /// conditions and is not supported.
    /// </summary>
    public class PerlangInterpreter : IInterpreter, Expr.IVisitor<object?>, Stmt.IVisitor<VoidObject>
    {
        private readonly Action<RuntimeError> runtimeErrorHandler;
        private readonly PerlangEnvironment globals = new PerlangEnvironment();

        private readonly IDictionary<Expr, Binding> globalBindings = new Dictionary<Expr, Binding>();
        private readonly IDictionary<Expr, Binding> localBindings = new Dictionary<Expr, Binding>();

        /// <summary>
        /// A collection of all currently defined global classes (both native/.NET and classes defined in Perlang code.)
        /// </summary>
        private readonly IDictionary<string, object> globalClasses = new Dictionary<string, object>();

        private readonly ImmutableDictionary<string, Type> nativeClasses;
        private readonly Action<string> standardOutputHandler;
        private readonly bool replMode;

        private ImmutableList<Stmt> previousStatements = ImmutableList.Create<Stmt>();
        private IEnvironment currentEnvironment;

        /// <summary>
        /// Initializes a new instance of the <see cref="PerlangInterpreter"/> class.
        /// </summary>
        /// <param name="runtimeErrorHandler">A callback that will be called on runtime errors.</param>
        /// <param name="standardOutputHandler">An optional parameter that will receive output printed to
        ///     standard output. If not provided or null, output will be printed to the standard output of the
        ///     running process.</param>
        /// <param name="arguments">An optional list of runtime arguments.</param>
        /// <param name="replMode">A flag indicating whether REPL mode will be active or not. In REPL mode, statements
        /// without semicolons are accepted.</param>
        public PerlangInterpreter(Action<RuntimeError> runtimeErrorHandler, Action<string>? standardOutputHandler = null, IEnumerable<string>? arguments = null, bool replMode = false)
        {
            this.runtimeErrorHandler = runtimeErrorHandler;
            this.standardOutputHandler = standardOutputHandler ?? Console.WriteLine;
            this.replMode = replMode;

            var argumentsList = (arguments ?? new string[0]).ToImmutableList();

            currentEnvironment = globals;

            LoadStdlib();
            nativeClasses = RegisterGlobalFunctionsAndClasses();
            var attributeSetters = DiscoverAttributeSetters();

            PassAttributesToAttributeSetters(attributeSetters, argumentsList);
        }

        private static void LoadStdlib()
        {
            // Because of implicit dependencies, this is not loaded automatically; we must manually load this
            // assembly to ensure all Callables within it are registered in the global namespace.
            Assembly.Load("Perlang.StdLib");
        }

        private ImmutableDictionary<string, Type> RegisterGlobalFunctionsAndClasses()
        {
            RegisterGlobalClasses();

            // We need to make a copy of this at this early stage, when it _only_ contains native classes, so that
            // we can feed it to the Resolver class.
            return globalClasses.ToImmutableDictionary(kvp => kvp.Key, kvp => (Type) kvp.Value);
        }

        private static ImmutableList<Action<ImmutableList<string>>> DiscoverAttributeSetters()
        {
            var argumentSettersQueryable = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .SelectMany(t => t.GetMethods())
                .Select(m => new
                {
                    MethodInfo = m,
                    ArgumentsSetterAttribute = m.GetCustomAttribute<ArgumentsSetterAttribute>()
                })
                .Where(t => t.ArgumentsSetterAttribute != null);

            var result = ImmutableList.CreateBuilder<Action<ImmutableList<string>>>();

            foreach (var argumentSetter in argumentSettersQueryable)
            {
                result.Add((Action<ImmutableList<string>>) Delegate.CreateDelegate(typeof(Action<ImmutableList<string>>), argumentSetter.MethodInfo));
            }

            return result.ToImmutable();
        }

        private static void PassAttributesToAttributeSetters(ImmutableList<Action<ImmutableList<string>>> attributeSetters, ImmutableList<string> arguments)
        {
            foreach (var setter in attributeSetters)
            {
                setter.Invoke(arguments);
            }
        }

        /// <summary>
        /// Registers global classes defined in native .NET code.
        /// </summary>
        /// <exception cref="PerlangInterpreterException">Multiple classes with the same name was encountered.</exception>
        private void RegisterGlobalClasses()
        {
            var globalClassesQueryable = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Select(t => new
                {
                    Type = t,
                    ClassAttribute = t.GetCustomAttribute<GlobalClassAttribute>()
                })
                .Where(t => t.ClassAttribute != null);

            foreach (var globalClass in globalClassesQueryable)
            {
                string name = globalClass.ClassAttribute!.Name ?? globalClass.Type.Name;

                if (globals.Get(name) != null)
                {
                    throw new PerlangInterpreterException(
                        $"Attempted to define global class '{name}', but another identifier with the same name already exists"
                    );
                }

                globalClasses[name] = globalClass.Type;
            }
        }

        /// <summary>
        /// Runs the provided source code, in an eval()/REPL fashion.
        ///
        /// If provided an expression, returns the result; otherwise, null.
        /// </summary>
        /// <remarks>
        /// Note that variables, methods and classes defined in an invocation to this method will persist to subsequent
        /// invocations. This might seem inconvenient at times, but it makes it possible to implement the Perlang
        /// REPL in a reasonable way.
        /// </remarks>
        /// <param name="source">The source code to a Perlang program (typically a single line of Perlang code).</param>
        /// <param name="scanErrorHandler">A handler for scanner errors.</param>
        /// <param name="parseErrorHandler">A handler for parse errors.</param>
        /// <param name="resolveErrorHandler">A handler for resolve errors.</param>
        /// <param name="typeValidationErrorHandler">A handler for type validation errors.</param>
        /// <returns>If the provided source is an expression, the value of the expression is returned. Otherwise,
        /// `null`.</returns>
        public object? Eval(
            string source,
            ScanErrorHandler scanErrorHandler,
            ParseErrorHandler parseErrorHandler,
            ResolveErrorHandler resolveErrorHandler,
            TypeValidationErrorHandler typeValidationErrorHandler)
        {
            if (String.IsNullOrWhiteSpace(source))
            {
                return null;
            }

            bool hasScanErrors = false;
            var scanner = new Scanner(source, scanError =>
            {
                hasScanErrors = true;
                scanErrorHandler(scanError);
            });

            var tokens = scanner.ScanTokens();

            if (hasScanErrors)
            {
                // Something went wrong as early as the "scan" stage. Abort the rest of the processing.
                return null;
            }

            bool hasParseErrors = false;
            var parser = new PerlangParser(
                tokens,
                parseError =>
                {
                    hasParseErrors = true;
                    parseErrorHandler(parseError);
                },
                allowSemicolonElision: replMode
            );

            object syntax = parser.ParseExpressionOrStatements();

            if (hasParseErrors)
            {
                // One or more parse errors were encountered. They have been reported upstream, so we just abort
                // the evaluation at this stage.
                return null;
            }

            if (syntax is List<Stmt> statements)
            {
                var previousAndNewStatements = previousStatements.Concat(statements).ToImmutableList();

                // The provided code parsed cleanly as a set of statements. Move on to the next phase in the
                // evaluation - resolving variable and function names.

                bool hasResolveErrors = false;

                var resolver = new Resolver(
                    nativeClasses,
                    AddLocal,
                    AddGlobal,
                    AddGlobalClass,
                    resolveError =>
                    {
                        hasResolveErrors = true;
                        resolveErrorHandler(resolveError);
                    }
                );

                resolver.Resolve(previousAndNewStatements);

                if (hasResolveErrors)
                {
                    // Resolution errors has been reported back to the provided error handler. Nothing more remains
                    // to be done than aborting the evaluation.
                    return null;
                }

                bool typeValidationFailed = false;

                TypeValidator.Validate(
                    previousAndNewStatements,
                    typeValidationError =>
                    {
                        typeValidationFailed = true;
                        typeValidationErrorHandler(typeValidationError);
                    },
                    GetVariableOrFunctionBinding
                );

                if (typeValidationFailed)
                {
                    return null;
                }

                // All validation was successful => add these statements to the list of "previous statements". Recording
                // them like this is necessary to be able to declare a variable in one REPL line and refer to it in
                // another.
                previousStatements = previousAndNewStatements.ToImmutableList();

                Interpret(statements);

                return null;
            }
            else if (syntax is Expr expr)
            {
                // The provided code is a single expression. Move on to the next phase in the evaluation - resolving
                // variable and function names. This is important since even a single expression might refer to a method
                // call or reading a variable.

                bool hasResolveErrors = false;
                var resolver = new Resolver(
                    nativeClasses,
                    AddLocal,
                    AddGlobal,
                    AddGlobalClass,
                    resolveError =>
                    {
                        hasResolveErrors = true;
                        resolveErrorHandler(resolveError);
                    }
                );

                resolver.Resolve(expr);

                if (hasResolveErrors)
                {
                    // Resolution errors has been reported back to the provided error handler. Nothing more remains
                    // to be done than aborting the evaluation.
                    return null;
                }

                bool typeValidationFailed = false;

                TypeValidator.Validate(
                    expr,
                    typeValidationError =>
                    {
                        typeValidationFailed = true;
                        typeValidationErrorHandler(typeValidationError);
                    },
                    GetVariableOrFunctionBinding
                );

                if (typeValidationFailed)
                {
                    return null;
                }

                return Evaluate(expr);
            }
            else
            {
                throw new IllegalStateException("syntax was neither Expr nor list of Stmt");
            }
        }

        private void Interpret(IEnumerable<Stmt> statements)
        {
            try
            {
                foreach (Stmt statement in statements)
                {
                    Execute(statement);
                }
            }
            catch (RuntimeError error)
            {
                runtimeErrorHandler(error);
            }
            catch (TargetInvocationException e)
            {
                if (e.InnerException is RuntimeError error)
                {
                    runtimeErrorHandler(error);
                }
                else
                {
                    // No "well-defined" code path exists for this. We let it at least bubble up so that it doesn't go
                    // unnoticed.
                    throw;
                }
            }
        }

        /// <summary>
        /// Evaluates the given expression and returns its value, converted to a string representation.
        /// </summary>
        /// <param name="expression">The expression to evaluate.</param>
        /// <returns>The evaluated value.</returns>
        private string? Interpret(Expr expression)
        {
            object? value = Evaluate(expression);
            return Stringify(value);
        }

        public object VisitLiteralExpr(Expr.Literal expr)
        {
            return expr.Value;
        }

        public object? VisitLogicalExpr(Expr.Logical expr)
        {
            object? left = Evaluate(expr.Left);

            if (expr.Operator.Type == OR)
            {
                if (IsTruthy(left))
                {
                    return left;
                }
            }
            else if (expr.Operator.Type == AND)
            {
                if (!IsTruthy(left))
                {
                    return left;
                }
            }
            else
            {
                throw new RuntimeError(expr.Operator, $"Unsupported logical operator: {expr.Operator.Type}");
            }

            return Evaluate(expr.Right);
        }

        public object? VisitUnaryPrefixExpr(Expr.UnaryPrefix expr)
        {
            object? right = Evaluate(expr.Right);

            switch (expr.Operator.Type)
            {
                case BANG:
                    return !IsTruthy(right);

                case MINUS:
                    // Using 'dynamic' here is arguably a bit weird, but like in VisitBinaryExpr(), it simplifies things
                    // significantly. The other option would be to handle all kind of numeric types here individually,
                    // which is clearly doable but a bit more work. For now, the CheckNumberOperand() method is the
                    // guarantee that the dynamic operation will succeed.
                    CheckNumberOperand(expr.Operator, right);
                    return -(dynamic?) right;
            }

            // Unreachable.
            return null;
        }

        public object VisitUnaryPostfixExpr(Expr.UnaryPostfix expr)
        {
            object? left = Evaluate(expr.Left);

            // We do have a check at the parser side also, but this one covers "null" cases.
            if (!IsValidNumberType(left))
            {
                switch (expr.Operator.Type)
                {
                    case PLUS_PLUS:
                        throw new RuntimeError(expr.Operator, $"++ can only be used to increment numbers, not {StringifyType(left)}");

                    case MINUS_MINUS:
                        throw new RuntimeError(expr.Operator, $"-- can only be used to decrement numbers, not {StringifyType(left)}");

                    default:
                        throw new RuntimeError(expr.Operator, $"Unsupported operator encountered: {expr.Operator.Type}");
                }
            }

            dynamic? previousValue = left;
            var variable = (Expr.Identifier) expr.Left;
            object value;

            switch (expr.Operator.Type)
            {
                case PLUS_PLUS:
                    value = previousValue + 1;
                    break;

                case MINUS_MINUS:
                    value = previousValue - 1;
                    break;

                default:
                    throw new RuntimeError(expr.Operator, $"Unsupported operator encountered: {expr.Operator.Type}");
            }

            if (localBindings.TryGetValue(expr, out Binding? binding))
            {
                if (binding is IDistanceAwareBinding distanceAwareBinding)
                {
                    currentEnvironment.AssignAt(distanceAwareBinding.Distance, expr.Name, value);
                }
                else
                {
                    throw new RuntimeError(expr.Operator, $"Unsupported operator '{expr.Operator.Type}' encountered for non-distance-aware binding '{binding}'");
                }
            }
            else
            {
                globals.Assign(variable.Name, value);
            }

            return value;
        }

        public object VisitIdentifierExpr(Expr.Identifier expr)
        {
            return LookUpVariable(expr.Name, expr);
        }

        public object VisitGetExpr(Expr.Get expr)
        {
            object? obj = Evaluate(expr.Object);

            if (obj == null)
            {
                throw new RuntimeError(expr.Name, "Object reference not set to an instance of an object");
            }

            if (expr.Methods.SingleOrDefault() != null)
            {
                return new TargetAndMethodContainer(obj, expr.Methods.Single());
            }
            else
            {
                throw new RuntimeError(expr.Name, "Internal runtime error: Expected expr.Method to be non-null");
            }
        }

        private Binding? GetVariableOrFunctionBinding(Expr expr)
        {
            if (localBindings.ContainsKey(expr))
            {
                return localBindings[expr];
            }

            if (globalBindings.ContainsKey(expr))
            {
                return globalBindings[expr];
            }

            // The variable does not exist, neither in the list of local nor global bindings.
            return null;
        }

        private object LookUpVariable(Token name, Expr expr)
        {
            if (localBindings.TryGetValue(expr, out Binding? localBinding))
            {
                if (localBinding is IDistanceAwareBinding distanceAwareBinding)
                {
                    return currentEnvironment.GetAt(distanceAwareBinding.Distance, name.Lexeme);
                }
                else
                {
                    throw new RuntimeError(name, $"Attempting to lookup variable for non-distance-aware binding '{localBinding}'");
                }
            }
            else if (globalClasses.TryGetValue(name.Lexeme, out object? globalClass))
            {
                // TODO: This probably means we could drop Perlang classes from being registered as globals as well.
                return globalClass;
            }
            else
            {
                return globals.Get(name);
            }
        }

        private static void CheckNumberOperand(Token @operator, object? operand)
        {
            if (IsValidNumberType(operand))
            {
                return;
            }

            throw new RuntimeError(@operator, "Operand must be a number.");
        }

        private static void CheckNumberOperands(Token @operator, object? left, object? right)
        {
            if (IsValidNumberType(left) && IsValidNumberType(right))
            {
                return;
            }

            throw new RuntimeError(@operator, "Operands must be numbers.");
        }

        private static bool IsValidNumberType(object? value)
        {
            if (value == null)
            {
                return false;
            }

            switch (value)
            {
                case SByte _:
                case Int16 _:
                case Int32 _:
                case Int64 _:
                case Byte _:
                case UInt16 _:
                case UInt32 _:
                case UInt64 _:
                case Single _: // i.e. float
                case Double _:
                    return true;
            }

            return false;
        }

        private static bool IsTruthy(object? @object)
        {
            if (@object == null)
            {
                return false;
            }

            if (@object is bool b)
            {
                return b;
            }

            return true;
        }

        private static bool IsEqual(object? a, object? b)
        {
            // nil is only equal to nil.
            if (a == null && b == null)
            {
                return true;
            }

            if (a == null)
            {
                return false;
            }

            return a.Equals(b);
        }

        public object? VisitGroupingExpr(Expr.Grouping expr)
        {
            return Evaluate(expr.Expression);
        }

        private object? Evaluate(Expr expr)
        {
            try
            {
                return expr.Accept(this);
            }
            catch (RuntimeError e)
            {
                runtimeErrorHandler(e);
                return null;
            }
            catch (TargetInvocationException e)
            {
                if (e.InnerException is RuntimeError error)
                {
                    runtimeErrorHandler(error);
                    return null;
                }
                else
                {
                    // No "well-defined" code path exists for this. We let it at least bubble up so that it doesn't go
                    // unnoticed.
                    throw;
                }
            }
        }

        private void Execute(Stmt stmt)
        {
            stmt.Accept(this);
        }

        private void AddGlobal(Binding binding)
        {
            globalBindings[binding.ReferringExpr] = binding;
        }

        private void AddLocal(Binding binding)
        {
            localBindings[binding.ReferringExpr] = binding;
        }

        private void AddGlobalClass(string name, PerlangClass perlangClass)
        {
            globalClasses[name] = perlangClass;
        }

        public void ExecuteBlock(IEnumerable<Stmt> statements, IEnvironment blockEnvironment)
        {
            IEnvironment previousEnvironment = currentEnvironment;

            try
            {
                currentEnvironment = blockEnvironment;

                foreach (Stmt statement in statements)
                {
                    Execute(statement);
                }
            }
            finally
            {
                currentEnvironment = previousEnvironment;
            }
        }

        public VoidObject VisitBlockStmt(Stmt.Block stmt)
        {
            ExecuteBlock(stmt.Statements, new PerlangEnvironment(currentEnvironment));
            return VoidObject.Void;
        }

        public VoidObject VisitClassStmt(Stmt.Class stmt)
        {
            currentEnvironment.Define(stmt.Name, globalClasses[stmt.Name.Lexeme]);
            return VoidObject.Void;
        }

        public VoidObject VisitExpressionStmt(Stmt.ExpressionStmt stmt)
        {
            Evaluate(stmt.Expression);
            return VoidObject.Void;
        }

        public VoidObject VisitFunctionStmt(Stmt.Function stmt)
        {
            var function = new PerlangFunction(stmt, currentEnvironment);
            currentEnvironment.Define(stmt.Name, function);
            return VoidObject.Void;
        }

        public VoidObject VisitIfStmt(Stmt.If stmt)
        {
            if (IsTruthy(Evaluate(stmt.Condition)))
            {
                Execute(stmt.ThenBranch);
            }
            else if (stmt.ElseBranch != null)
            {
                Execute(stmt.ElseBranch);
            }

            return VoidObject.Void;
        }

        public VoidObject VisitPrintStmt(Stmt.Print stmt)
        {
            object? value = Evaluate(stmt.Expression);
            standardOutputHandler(Stringify(value));
            return VoidObject.Void;
        }

        public VoidObject VisitReturnStmt(Stmt.Return stmt)
        {
            object? value = null;

            if (stmt.Value != null)
            {
                value = Evaluate(stmt.Value);
            }

            throw new Return(value);
        }

        public VoidObject VisitVarStmt(Stmt.Var stmt)
        {
            object? value = null;

            if (stmt.Initializer != null)
            {
                value = Evaluate(stmt.Initializer);
            }

            currentEnvironment.Define(stmt.Name, value);
            return VoidObject.Void;
        }

        public VoidObject VisitWhileStmt(Stmt.While stmt)
        {
            while (IsTruthy(Evaluate(stmt.Condition)))
            {
                Execute(stmt.Body);
            }

            return VoidObject.Void;
        }

        public object? VisitEmptyExpr(Expr.Empty expr)
        {
            return null;
        }

        public object? VisitAssignExpr(Expr.Assign expr)
        {
            object? value = Evaluate(expr.Value);

            if (localBindings.TryGetValue(expr, out Binding? binding))
            {
                if (binding is IDistanceAwareBinding distanceAwareBinding)
                {
                    currentEnvironment.AssignAt(distanceAwareBinding.Distance, expr.Name, value);
                }
                else
                {
                    throw new RuntimeError(expr.Name, $"Unsupported variable assignment encountered for non-distance-aware binding '{binding}'");
                }
            }
            else
            {
                globals.Assign(expr.Name, value);
            }

            return value;
        }

        public object? VisitBinaryExpr(Expr.Binary expr)
        {
            object? left = Evaluate(expr.Left);
            object? right = Evaluate(expr.Right);

            // Using 'dynamic' here to avoid excessive complexity, having to support all permutations of
            // comparisons (int16 to int32, int32 to int64, etc etc). Since we validate the numerability of the
            // values first, these should be "safe" in that sense. Performance might not be great but let's live
            // with that until we rewrite the whole Perlang interpreter as an on-demand, statically typed but
            // dynamically compiled language.
            dynamic? leftNumber = left;
            dynamic? rightNumber = right;

            switch (expr.Operator.Type)
            {
                case GREATER:
                    CheckNumberOperands(expr.Operator, left, right);
                    return leftNumber > rightNumber;
                case GREATER_EQUAL:
                    CheckNumberOperands(expr.Operator, left, right);
                    return leftNumber >= rightNumber;
                case LESS:
                    CheckNumberOperands(expr.Operator, left, right);
                    return leftNumber < rightNumber;
                case LESS_EQUAL:
                    CheckNumberOperands(expr.Operator, left, right);
                    return leftNumber <= rightNumber;
                case MINUS:
                    CheckNumberOperands(expr.Operator, left, right);
                    return leftNumber - rightNumber;
                case PLUS:
                    if (left is string s1 && right is string s2)
                    {
                        return s1 + s2;
                    }

                    CheckNumberOperands(expr.Operator, left, right);
                    return leftNumber + rightNumber;
                case SLASH:
                    CheckNumberOperands(expr.Operator, left, right);
                    return leftNumber / rightNumber;
                case STAR:
                    CheckNumberOperands(expr.Operator, left, right);
                    return leftNumber * rightNumber;
                case BANG_EQUAL:
                    return !IsEqual(left, right);
                case EQUAL_EQUAL:
                    return IsEqual(left, right);
            }

            // Unreachable.
            return null;
        }

        public object? VisitCallExpr(Expr.Call expr)
        {
            object? callee = Evaluate(expr.Callee);

            var arguments = new List<object>();

            foreach (Expr argument in expr.Arguments)
            {
                arguments.Add(Evaluate(argument)!);
            }

            switch (callee)
            {
                case ICallable callable:
                    if (arguments.Count != callable.Arity())
                    {
                        throw new RuntimeError(
                            expr.Paren,
                            "Expected " + callable.Arity() + " argument(s) but got " + arguments.Count + "."
                        );
                    }

                    try
                    {
                        return callable.Call(this, arguments);
                    }
                    catch (Exception e)
                    {
                        if (expr.Callee is Expr.Identifier identifier)
                        {
                            throw new RuntimeError(identifier.Name, $"{identifier.Name.Lexeme}: {e.Message}");
                        }
                        else
                        {
                            throw new RuntimeError(expr.Paren, e.Message);
                        }
                    }

                case TargetAndMethodContainer container:
                    if (expr.Callee is Expr.Get)
                    {
                        return container.Method.Invoke(container.Target, arguments.ToArray());
                    }
                    else
                    {
                        throw new RuntimeError(expr.Paren, $"Internal error: Expected Get expression, not {expr.Callee}.");
                    }

                default:
                    throw new RuntimeError(expr.Paren, $"Can only call functions, classes and native methods, not {callee}.");
            }
        }
    }
}
