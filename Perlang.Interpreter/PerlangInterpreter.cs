using System;
using System.Collections.Generic;
using Perlang.Parser;
using static Perlang.TokenType;

namespace Perlang.Interpreter
{
    internal class PerlangInterpreter : IInterpreter, Expr.IVisitor<object>, Stmt.IVisitor<VoidObject>
    {
        private readonly Action<RuntimeError> runtimeErrorHandler;
        private readonly PerlangEnvironment globals = new PerlangEnvironment();
        private readonly IDictionary<Expr, int> locals = new Dictionary<Expr, int>();

        private PerlangEnvironment perlangEnvironment;

        public PerlangInterpreter(Action<RuntimeError> runtimeErrorHandler)
        {
            this.runtimeErrorHandler = runtimeErrorHandler;
            
            perlangEnvironment = globals;

            globals.Define("clock", new ClockCallable());
        }

        internal void Interpret(IEnumerable<Stmt> statements)
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

        internal object Evaluate(Expr expr)
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
            Console.WriteLine(Stringify(value));
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