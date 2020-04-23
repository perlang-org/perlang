using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Perlang.Exceptions;
using Perlang.Parser;
using static Perlang.TokenType;
using static Perlang.Utils;

namespace Perlang.Interpreter
{
    public class PerlangInterpreter : IInterpreter, Expr.IVisitor<object>, Stmt.IVisitor<VoidObject>
    {
        private readonly Action<RuntimeError> runtimeErrorHandler;
        private readonly PerlangEnvironment globals = new PerlangEnvironment();
        private readonly IDictionary<Expr, int> locals = new Dictionary<Expr, int>();

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

            RegisterCallables();
        }

        private void RegisterCallables()
        {
            // Because of implicit dependencies, this is not loaded automatically; we must manually load this
            // assembly to ensure all Callables within it are registered in the global namespace.
            Assembly.Load("Perlang.StdLib");

            var globalCallables = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Select(t => new
                {
                    Type = t,
                    CallableAttribute = t.GetCustomAttributes(typeof(GlobalCallableAttribute), inherit: false)
                        .Cast<GlobalCallableAttribute>()
                        .FirstOrDefault()
                })
                .Where(t => t.CallableAttribute != null);

            foreach (var globalCallable in globalCallables)
            {
                object callableInstance = Activator.CreateInstance(globalCallable.Type);
                globals.Define(globalCallable.CallableAttribute.Name, callableInstance);
            }
        }

        /// <summary>
        /// Runs the provided source code, in an eval()/REPL fashion. For scenarios where more control over error
        /// handling, etc., consider manually scanning the source code and use the <see cref="Interpret"/> method
        /// instead.
        ///
        /// If provided an expression, returns the result; otherwise, null.
        /// </summary>
        /// <param name="source">the Perlang source code</param>
        /// <param name="scanErrorHandler">a handler for scanner errors</param>
        /// <param name="parseErrorHandler">a handler for parse errors</param>
        /// <param name="resolveErrorHandler">a handler for resolve errors</param>
        /// <returns>if the provided source is an expression, the value of the expression. Otherwise, null.</returns>
        public object Eval(string source, ScanErrorHandler scanErrorHandler, ParseErrorHandler parseErrorHandler,
            ResolveErrorHandler resolveErrorHandler)
        {
            if (String.IsNullOrWhiteSpace(source))
            {
                return null;
            }

            var scanErrors = new ScanErrors();
            var scanner = new Scanner(source, scanError => scanErrors.Add(scanError));

            var tokens = scanner.ScanTokens();

            if (!scanErrors.Empty())
            {
                // Something went wrong as early as the "scan" stage. Report it to the caller and return cleanly.
                foreach (ScanError scanError in scanErrors)
                {
                    scanErrorHandler(scanError);
                }

                return null;
            }

            var parseErrors = new ParseErrors();
            var parser = new PerlangParser(tokens, parseError => parseErrors.Add(parseError));
            object syntax = parser.ParseExpressionOrStatements();

            if (!parseErrors.Empty())
            {
                foreach (ParseError parseError in parseErrors)
                {
                    parseErrorHandler(parseError);
                }

                return null;
            }

            if (syntax is List<Stmt> statements)
            {
                // The provided code parsed cleanly as a set of statements. Move on to the next phase in the
                // evaluation - resolving variable and function names.

                var resolveErrors = new ResolveErrors();
                var resolver = new Resolver(AddLocal, resolveError => resolveErrors.Add(resolveError));
                resolver.Resolve(statements);

                if (!resolveErrors.Empty())
                {
                    // Report resolution errors back to the provided error handler. We defer this so that we can use the
                    // Empty() check to see if we had any errors; if we would just pass the resolveErrorHandler()
                    // to the Resolver constructor, we would have no idea if any errors has occurred at this stage.
                    foreach (ResolveError resolveError in resolveErrors)
                    {
                        resolveErrorHandler(resolveError);
                    }

                    return null;
                }

                Interpret(statements);

                return null;
            }
            else if (syntax is Expr expression)
            {
                try
                {
                    return Evaluate(expression);
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
                    CheckNumberOperand(expr.Operator, right);
                    return -(double) right;
            }

            // Unreachable.
            return null;
        }

        public object VisitUnaryPostfixExpr(Expr.UnaryPostfix expr)
        {
            object left = Evaluate(expr.Left);

            // We do have a check at the parser side also, but this one covers "null" cases.
            if (!(left is double previousValue))
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

            var variable = (Expr.Variable) expr.Left;
            double value;

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

            if (locals.TryGetValue(expr, out int distance))
            {
                currentEnvironment.AssignAt(distance, expr.Name, value);
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

        private object LookUpVariable(Token name, Expr expr)
        {
            if (locals.TryGetValue(expr, out int distance))
            {
                return currentEnvironment.GetAt(distance, name.Lexeme);
            }
            else
            {
                return globals.Get(name);
            }
        }

        private static void CheckNumberOperand(Token _operator, object operand)
        {
            if (operand is double)
            {
                return;
            }

            throw new RuntimeError(_operator, "Operand must be a number.");
        }

        private static void CheckNumberOperands(Token _operator, object left, object right)
        {
            if (left is double && right is double)
            {
                return;
            }

            throw new RuntimeError(_operator, "Operands must be numbers.");
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

        private void AddLocal(Expr expr, int depth)
        {
            locals[expr] = depth;
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
            return null;
        }

        public VoidObject VisitExpressionStmt(Stmt.ExpressionStmt stmt)
        {
            Evaluate(stmt.Expression);
            return null;
        }

        public VoidObject VisitFunctionStmt(Stmt.Function stmt)
        {
            var function = new PerlangFunction(stmt, currentEnvironment);
            currentEnvironment.Define(stmt.Name.Lexeme, function);
            return null;
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

            return null;
        }

        public VoidObject VisitPrintStmt(Stmt.Print stmt)
        {
            object value = Evaluate(stmt.Expression);
            standardOutputHandler(Stringify(value));
            return null;
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
            return null;
        }

        public VoidObject VisitWhileStmt(Stmt.While stmt)
        {
            while (IsTruthy(Evaluate(stmt.Condition)))
            {
                Execute(stmt.Body);
            }

            return null;
        }

        public object VisitEmptyExpr(Expr.Empty expr)
        {
            return null;
        }

        public object VisitAssignExpr(Expr.Assign expr)
        {
            object value = Evaluate(expr.Value);

            if (locals.TryGetValue(expr, out int distance))
            {
                currentEnvironment.AssignAt(distance, expr.Name, value);
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

            switch (expr.Operator.Type)
            {
                case GREATER:
                    CheckNumberOperands(expr.Operator, left, right);
                    return (double) left > (double) right;
                case GREATER_EQUAL:
                    CheckNumberOperands(expr.Operator, left, right);
                    return (double) left >= (double) right;
                case LESS:
                    CheckNumberOperands(expr.Operator, left, right);
                    return (double) left < (double) right;
                case LESS_EQUAL:
                    CheckNumberOperands(expr.Operator, left, right);
                    return (double) left <= (double) right;
                case MINUS:
                    CheckNumberOperands(expr.Operator, left, right);
                    return (double) left - (double) right;
                case PLUS:
                    if (left is double d1 && right is double d2)
                    {
                        return d1 + d2;
                    }

                    if (left is string s1 && right is string s2)
                    {
                        return s1 + s2;
                    }

                    throw new RuntimeError(expr.Operator, "Operands must be two numbers or two strings.");
                case SLASH:
                    CheckNumberOperands(expr.Operator, left, right);
                    return (double) left / (double) right;
                case STAR:
                    CheckNumberOperands(expr.Operator, left, right);
                    return (double) left * (double) right;
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

            if (!(callee is ICallable))
            {
                throw new RuntimeError(expr.Paren, "Can only call functions and classes.");
            }

            var function = (ICallable) callee;

            if (arguments.Count != function.Arity())
            {
                throw new RuntimeError(expr.Paren, "Expected " + function.Arity() + " argument(s) but got " +
                                                   arguments.Count + ".");
            }

            try
            {
                return function.Call(this, arguments);
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
        }
    }
}
