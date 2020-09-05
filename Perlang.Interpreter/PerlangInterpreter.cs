using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Perlang.Exceptions;
using Perlang.Interpreter.Resolution;
using Perlang.Interpreter.Typing;
using Perlang.Parser;
using static Perlang.TokenType;
using static Perlang.Utils;

namespace Perlang.Interpreter
{
    public class PerlangInterpreter : IInterpreter, Expr.IVisitor<object>, Stmt.IVisitor<VoidObject>
    {
        private readonly Action<RuntimeError> runtimeErrorHandler;
        private readonly PerlangEnvironment globals = new PerlangEnvironment();

        private readonly IDictionary<string, TypeReferenceNativeFunction> globalCallables =
            new Dictionary<string, TypeReferenceNativeFunction>();

        private readonly IDictionary<Expr, Binding> globalBindings = new Dictionary<Expr, Binding>();
        private readonly IDictionary<Expr, Binding> localBindings = new Dictionary<Expr, Binding>();

        private IEnvironment currentEnvironment;
        private readonly Action<string> standardOutputHandler;

        public List<string> Arguments { get; }

        /// <summary>
        /// Creates a new Perlang interpreter instance.
        /// </summary>
        /// <param name="runtimeErrorHandler">a callback that will be called on runtime errors</param>
        /// <param name="standardOutputHandler">an optional parameter that will receive output printed to
        ///     standard output. If not provided or null, output will be printed to the standard output of the
        ///     running process</param>
        /// <param name="arguments">an optional list of runtime arguments</param>
        public PerlangInterpreter(Action<RuntimeError> runtimeErrorHandler,
            Action<string> standardOutputHandler = null, IEnumerable<string> arguments = null)
        {
            this.runtimeErrorHandler = runtimeErrorHandler;
            this.standardOutputHandler = standardOutputHandler ?? Console.WriteLine;

            Arguments = new List<string>(arguments ?? new string[0]);

            currentEnvironment = globals;

            RegisterGlobalCallables();
        }

        private void RegisterGlobalCallables()
        {
            // Because of implicit dependencies, this is not loaded automatically; we must manually load this
            // assembly to ensure all Callables within it are registered in the global namespace.
            Assembly.Load("Perlang.StdLib");

            var globalCallablesQueryable = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Select(t => new
                {
                    Type = t,
                    CallableAttribute = t.GetCustomAttributes(typeof(GlobalCallableAttribute), inherit: false)
                        .Cast<GlobalCallableAttribute>()
                        .FirstOrDefault()
                })
                .Where(t => t.CallableAttribute != null);

            foreach (var globalCallable in globalCallablesQueryable)
            {
                MethodInfo method = globalCallable.Type.GetMethod("Call");

                if (method == null)
                {
                    throw new PerlangInterpreterException("Invalid callable encountered: Call method missing");
                }

                var parameters = method.GetParameters();

                if (parameters.Length == 0)
                {
                    throw new PerlangInterpreterException(
                        "Invalid callable encountered: Call method must take an IInterpreter parameter as its first parameter");
                }

                if (parameters[0].ParameterType != typeof(IInterpreter))
                {
                    throw new PerlangInterpreterException(
                        $"Invalid callable encountered: First parameter of Call method must be IInterpreter, not {parameters[0].ParameterType.Name}");
                }

                object callableInstance = Activator.CreateInstance(globalCallable.Type);
                var callableReturnTypeReference = new TypeReference(method.ReturnType);

                // The first parameter is the IInterpreter instance; it's not interesting in this context. We tuck away the
                // information about the parameters here so that it can be used by the static type analysis later on.
                var callableParameters = parameters[1..]
                    .Select(pi => pi.ParameterType)
                    .ToArray();

                globalCallables[globalCallable.CallableAttribute.Name] = new TypeReferenceNativeFunction(
                    callableReturnTypeReference, callableInstance, method, callableParameters
                );
            }
        }

        /// <summary>
        /// Runs the provided source code, in an eval()/REPL fashion.
        ///
        /// If provided an expression, returns the result; otherwise, null.
        /// </summary>
        /// <param name="source">the Perlang source code</param>
        /// <param name="scanErrorHandler">a handler for scanner errors</param>
        /// <param name="parseErrorHandler">a handler for parse errors</param>
        /// <param name="resolveErrorHandler">a handler for resolve errors</param>
        /// <param name="typeValidationErrorHandler">a handler for type validation errors</param>
        /// <returns>if the provided source is an expression, the value of the expression. Otherwise, null.</returns>
        public object Eval(string source, ScanErrorHandler scanErrorHandler, ParseErrorHandler parseErrorHandler,
            ResolveErrorHandler resolveErrorHandler, TypeValidationErrorHandler typeValidationErrorHandler)
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
            var parser = new PerlangParser(tokens, parseError =>
            {
                hasParseErrors = true;
                parseErrorHandler(parseError);
            });

            object syntax = parser.ParseExpressionOrStatements();

            if (hasParseErrors)
            {
                // One or more parse errors were encountered. They have been reported upstream, so we just abort
                // the evaluation at this stage.
                return null;
            }

            if (syntax is List<Stmt> statements)
            {
                // The provided code parsed cleanly as a set of statements. Move on to the next phase in the
                // evaluation - resolving variable and function names.

                bool hasResolveErrors = false;
                var resolver = new Resolver(globalCallables.ToImmutableDictionary(), AddLocal, AddGlobal,
                    resolveError =>
                    {
                        hasResolveErrors = true;
                        resolveErrorHandler(resolveError);
                    });

                resolver.Resolve(statements);

                if (hasResolveErrors)
                {
                    // Resolution errors has been reported back to the provided error handler. Nothing more remains
                    // to be done than aborting the evaluation.
                    return null;
                }

                bool typeValidationFailed = false;

                TypeValidator.Validate(
                    statements,
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

                Interpret(statements);

                return null;
            }
            else if (syntax is Expr expr)
            {
                // The provided is a single expression. Move on to the next phase in the evaluation - resolving
                // variable and function names. This is important since even a single expression might refer to
                // a method call or reading a variable.

                bool hasResolveErrors = false;
                var resolver = new Resolver(globalCallables.ToImmutableDictionary(), AddLocal, AddGlobal,
                    resolveError =>
                    {
                        hasResolveErrors = true;
                        resolveErrorHandler(resolveError);
                    });

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

                try
                {
                    return Evaluate(expr);
                }
                catch (RuntimeError e)
                {
                    runtimeErrorHandler(e);
                    return null;
                }
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
        }

        private string Interpret(Expr expression)
        {
            try
            {
                Object value = Evaluate(expression);
                return Stringify(value);
            }
            catch (RuntimeError error)
            {
                runtimeErrorHandler(error);
                return null;
            }
        }

        public object VisitLiteralExpr(Expr.Literal expr)
        {
            return expr.Value;
        }

        public object VisitLogicalExpr(Expr.Logical expr)
        {
            object left = Evaluate(expr.Left);

            if (expr.Operator.Type == OR)
            {
                if (IsTruthy(left)) return left;
            }
            else if (expr.Operator.Type == AND)
            {
                if (!IsTruthy(left)) return left;
            }
            else
            {
                throw new RuntimeError(expr.Operator, $"Unsupported logical operator: {expr.Operator.Type}");
            }

            return Evaluate(expr.Right);
        }

        public object VisitUnaryPrefixExpr(Expr.UnaryPrefix expr)
        {
            object right = Evaluate(expr.Right);

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
                    return -(dynamic) right;
            }

            // Unreachable.
            return null;
        }

        public object VisitUnaryPostfixExpr(Expr.UnaryPostfix expr)
        {
            object left = Evaluate(expr.Left);

            // We do have a check at the parser side also, but this one covers "null" cases.
            if (!IsValidNumberType(left))
            {
                switch (expr.Operator.Type)
                {
                    case PLUS_PLUS:
                        throw new RuntimeError(expr.Operator,
                            $"++ can only be used to increment numbers, not {StringifyType(left)}");

                    case MINUS_MINUS:
                        throw new RuntimeError(expr.Operator,
                            $"-- can only be used to decrement numbers, not {StringifyType(left)}");

                    default:
                        throw new RuntimeError(expr.Operator,
                            $"Unsupported operator encountered: {expr.Operator.Type}");
                }
            }

            dynamic previousValue = left;
            var variable = (Expr.Variable) expr.Left;
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

            if (localBindings.TryGetValue(expr, out Binding binding))
            {
                if (binding is IDistanceAwareBinding distanceAwareBinding)
                {
                    currentEnvironment.AssignAt(distanceAwareBinding.Distance, expr.Name, value);
                }
                else
                {
                    throw new RuntimeError(expr.Operator,
                        $"Unsupported operator '{expr.Operator.Type}' encountered for non-distance-aware binding '{binding}'");
                }
            }
            else
            {
                globals.Assign(variable.Name, value);
            }

            return value;
        }

        public object VisitVariableExpr(Expr.Variable expr)
        {
            return LookUpVariable(expr.Name, expr);
        }

        private Binding GetVariableOrFunctionBinding(Expr expr)
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
            if (localBindings.TryGetValue(expr, out Binding localBinding))
            {
                if (localBinding is IDistanceAwareBinding distanceAwareBinding)
                {
                    return currentEnvironment.GetAt(distanceAwareBinding.Distance, name.Lexeme);
                }
                else
                {
                    throw new RuntimeError(name,
                        $"Attempting to lookup variable for non-distance-aware binding '{localBinding}'");
                }
            }
            else if (globalCallables.TryGetValue(name.Lexeme, out TypeReferenceNativeFunction globalCallable))
            {
                return globalCallable;
            }
            else
            {
                return globals.Get(name);
            }
        }

        private static void CheckNumberOperand(Token _operator, object operand)
        {
            if (IsValidNumberType(operand))
            {
                return;
            }

            throw new RuntimeError(_operator, "Operand must be a number.");
        }

        private static void CheckNumberOperands(Token _operator, object left, object right)
        {
            if (IsValidNumberType(left) && IsValidNumberType(right))
            {
                return;
            }

            throw new RuntimeError(_operator, "Operands must be numbers.");
        }

        private static bool IsValidNumberType(object value)
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

        private static bool IsTruthy(object _object)
        {
            if (_object == null)
            {
                return false;
            }

            if (_object is bool b)
            {
                return b;
            }

            return true;
        }

        private static bool IsEqual(object a, object b)
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

        public object VisitGroupingExpr(Expr.Grouping expr)
        {
            return Evaluate(expr.Expression);
        }

        private object Evaluate(Expr expr)
        {
            return expr.Accept(this);
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

        public VoidObject VisitExpressionStmt(Stmt.ExpressionStmt stmt)
        {
            Evaluate(stmt.Expression);
            return VoidObject.Void;
        }

        public VoidObject VisitFunctionStmt(Stmt.Function stmt)
        {
            var function = new PerlangFunction(stmt, currentEnvironment);
            currentEnvironment.Define(stmt.Name.Lexeme, function);
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
            object value = Evaluate(stmt.Expression);
            standardOutputHandler(Stringify(value));
            return VoidObject.Void;
        }

        public VoidObject VisitReturnStmt(Stmt.Return stmt)
        {
            object value = null;
            if (stmt.Value != null) value = Evaluate(stmt.Value);

            throw new Return(value);
        }

        public VoidObject VisitVarStmt(Stmt.Var stmt)
        {
            object value = null;

            if (stmt.Initializer != null)
            {
                value = Evaluate(stmt.Initializer);
            }

            currentEnvironment.Define(stmt.Name.Lexeme, value);
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

        public object VisitEmptyExpr(Expr.Empty expr)
        {
            return null;
        }

        public object VisitAssignExpr(Expr.Assign expr)
        {
            object value = Evaluate(expr.Value);

            if (localBindings.TryGetValue(expr, out Binding binding))
            {
                if (binding is IDistanceAwareBinding distanceAwareBinding)
                {
                    currentEnvironment.AssignAt(distanceAwareBinding.Distance, expr.Name, value);
                }
                else
                {
                    throw new RuntimeError(expr.Name,
                        $"Unsupported variable assignment encountered for non-distance-aware binding '{binding}'");
                }
            }
            else
            {
                globals.Assign(expr.Name, value);
            }

            return value;
        }

        public object VisitBinaryExpr(Expr.Binary expr)
        {
            object left = Evaluate(expr.Left);
            object right = Evaluate(expr.Right);

            // Using 'dynamic' here to avoid excessive complexity, having to support all permutations of
            // comparisons (int16 to int32, int32 to int64, etc etc). Since we validate the numerability of the
            // values first, these should be "safe" in that sense. Performance might not be great but let's live
            // with that until we rewrite the whole Perlang interpreter as an on-demand, statically typed but
            // dynamically compiled language.
            dynamic leftNumber = left;
            dynamic rightNumber = right;

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

        public object VisitCallExpr(Expr.Call expr)
        {
            object callee = Evaluate(expr.Callee);

            var arguments = new List<object>();

            foreach (Expr argument in expr.Arguments)
            {
                arguments.Add(Evaluate(argument));
            }

            switch (callee)
            {
                case TypeReferenceNativeFunction nativeFunction:
                    // A long as we are an interpreted language, calling native functions will provide the IInterpreter
                    // instance as the first parameter. Once we are compiled, we should aim for a more efficient
                    // implementation in general, and
                    var argumentsWithInterpreter = new object[arguments.Count + 1];

                    argumentsWithInterpreter[0] = this;

                    for (var i = 0; i < arguments.Count; i++)
                    {
                        object argument = arguments[i];
                        argumentsWithInterpreter[i + 1] = argument;
                    }

                    try
                    {
                        return nativeFunction.Method.Invoke(nativeFunction.Callable, argumentsWithInterpreter);
                    }
                    catch (TargetInvocationException e)
                    {
                        if (e.InnerException != null)
                        {
                            throw e.InnerException;
                        }
                        else
                        {
                            throw;
                        }
                    }

                case ICallable callable:
                    if (arguments.Count != callable.Arity())
                    {
                        throw new RuntimeError(expr.Paren, "Expected " + callable.Arity() + " argument(s) but got " +
                                                           arguments.Count + ".");
                    }

                    try
                    {
                        return callable.Call(this, arguments);
                    }
                    catch (Exception e)
                    {
                        if (expr.Callee is Expr.Variable v)
                        {
                            throw new RuntimeError(v.Name, $"{v.Name.Lexeme}: {e.Message}");
                        }
                        else
                        {
                            throw new RuntimeError(expr.Paren, e.Message);
                        }
                    }

                default:
                    throw new RuntimeError(expr.Paren, $"Can only call functions and classes, not {callee}.");
            }
        }
    }
}
