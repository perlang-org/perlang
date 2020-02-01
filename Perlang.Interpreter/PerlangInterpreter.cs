using System;
using System.Collections.Generic;
using Perlang.Parser;
using static Perlang.TokenType;

namespace Perlang.Interpreter
{
    public class PerlangInterpreter : IInterpreter, Expr.IVisitor<object>, Stmt.IVisitor<VoidObject>
    {
        private readonly Action<RuntimeError> runtimeErrorHandler;
        private readonly PerlangEnvironment globals = new PerlangEnvironment();
        private readonly IDictionary<Expr, int> locals = new Dictionary<Expr, int>();

        private PerlangEnvironment perlangEnvironment;
        private readonly Action<string> standardOutputHandler;

        /// <summary>
        /// Creates a new Perlang interpreter instance.
        /// </summary>
        /// <param name="runtimeErrorHandler"></param>
        /// <param name="standardOutputHandler">an optional parameter that will receive output printed to
        /// standard output. If not provided or null, output will be printed to the standard output of the
        /// running process</param>
        public PerlangInterpreter(Action<RuntimeError> runtimeErrorHandler, Action<string> standardOutputHandler = null)
        {
            this.runtimeErrorHandler = runtimeErrorHandler;
            this.standardOutputHandler = standardOutputHandler ?? Console.WriteLine;

            perlangEnvironment = globals;

            globals.Define("clock", new ClockCallable());
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

            var statementParseErrors = new ParseErrors();
            var parser = new PerlangParser(tokens, parseError => statementParseErrors.Add(parseError));
            var statements = parser.ParseStatements();

            if (statementParseErrors.Empty())
            {
                // The provided code parsed cleanly as a set of statements. Move on to the next phase in the
                // evaluation.

                var resolveErrors = new ResolveErrors();
                var resolver = new Resolver(this, resolveError => resolveErrors.Add(resolveError));
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
            else
            {
                // This was not a valid set of statements. But is it perhaps a valid expression? The parser is now
                // at EOF and since we don't currently have any form of "rewind" functionality, the easiest approach
                // is to just create a new parser at this point.
                var expressionParseErrors = new ParseErrors();

                parser = new PerlangParser(tokens, parseError => expressionParseErrors.Add(parseError));
                Expr expression = parser.ParseExpression();

                // TODO: This approach (parsing the provided program as a set of statements first, then an
                // TODO: expression) has some clear drawbacks. We might return parse errors here which are
                // TODO: quite irrelevant, since we are parsing the program the "wrong" way... We should consider
                // TODO: at least reversing this so we try with expression first, then statements.
                if (!expressionParseErrors.Empty())
                {
                    foreach (ParseError parseError in expressionParseErrors)
                    {
                        parseErrorHandler(parseError);
                    }

                    return null;
                }

                if (expression == null)
                {
                    // TODO: throw some InternalStateException or something instead.
                    throw new Exception("expression was null even though no parse errors were encountered");
                }

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

        public object VisitLiteralExpr(Expr.Literal expr)
        {
            return expr.Value;
        }

        private class ClockCallable : ICallable
        {
            public int Arity()
            {
                return 0;
            }

            public object Call(IInterpreter interpreter, List<object> arguments)
            {
                return new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds() / 1000.0;
            }

            public override string ToString()
            {
                return "<native fn>";
            }
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

        public object VisitUnaryExpr(Expr.Unary expr)
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

        public object VisitVariableExpr(Expr.Variable expr)
        {
            return LookUpVariable(expr.Name, expr);
        }

        private object LookUpVariable(Token name, Expr expr)
        {
            if (locals.TryGetValue(expr, out int distance))
            {
                return perlangEnvironment.GetAt(distance, name.Lexeme);
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

        private static string Stringify(object _object)
        {
            if (_object == null)
            {
                return "nil";
            }

            return _object.ToString();
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

        public void Resolve(Expr expr, int depth)
        {
            locals[expr] = depth;
        }

        public void ExecuteBlock(IEnumerable<Stmt> statements, PerlangEnvironment blockEnvironment)
        {
            PerlangEnvironment previous = perlangEnvironment;

            try
            {
                perlangEnvironment = blockEnvironment;

                foreach (Stmt statement in statements)
                {
                    Execute(statement);
                }
            }
            finally
            {
                perlangEnvironment = previous;
            }
        }

        public VoidObject VisitBlockStmt(Stmt.Block stmt)
        {
            ExecuteBlock(stmt.Statements, new PerlangEnvironment(perlangEnvironment));
            return null;
        }

        public VoidObject VisitExpressionStmt(Stmt.ExpressionStmt stmt)
        {
            Evaluate(stmt.Expression);
            return null;
        }

        public VoidObject VisitFunctionStmt(Stmt.Function stmt)
        {
            var function = new PerlangFunction(stmt, perlangEnvironment);
            perlangEnvironment.Define(stmt.Name.Lexeme, function);
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

            perlangEnvironment.Define(stmt.Name.Lexeme, value);
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
                perlangEnvironment.AssignAt(distance, expr.Name, value);
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
                throw new RuntimeError(expr.Paren, "Expected " + function.Arity() + " arguments but got " +
                                                   arguments.Count + ".");
            }

            return function.Call(this, arguments);
        }
    }
}
