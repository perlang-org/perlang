#nullable enable
#pragma warning disable SA1629

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace Perlang.Interpreter.Internals
{
    /// <summary>
    /// Creates an unambiguous, if ugly, string representation of AST nodes.
    ///
    /// Based on an idea &amp; implementation by Bob Nystrom:
    ///
    /// - https://craftinginterpreters.com/representing-code.html
    /// - https://github.com/munificent/craftinginterpreters/blob/7542d2782a620ccbae9aa817b66a8852e4c094e4/java/com/craftinginterpreters/lox/AstPrinter.java
    /// </summary>
    internal class AstPrinter : Expr.IVisitor<string>, Stmt.IVisitor<string>
    {
        // TODO: Return Perlang string instead
        [Pure]
        internal static string Print(Expr expr)
        {
            return expr.Accept(new AstPrinter());
        }

        // TODO: Return Perlang string instead
        [Pure]
        internal static string Print(Stmt stmt)
        {
            return stmt.Accept(new AstPrinter());
        }

        //
        // Expr.IVisitor<string> methods
        //

        public string VisitEmptyExpr(Expr.Empty expr)
        {
            return String.Empty;
        }

        public string VisitAssignExpr(Expr.Assign expr)
        {
            return Parenthesize(expr.Name.Lexeme, expr.Value);
        }

        public string VisitBinaryExpr(Expr.Binary expr)
        {
            return Parenthesize(expr.Operator.Lexeme, expr.Left, expr.Right);
        }

        public string VisitCallExpr(Expr.Call expr)
        {
            return Parenthesize2("call", expr.Callee, expr.Arguments);
        }

        public string VisitIndexExpr(Expr.Index expr)
        {
            return Parenthesize2("index", expr.Indexee, expr.Argument);
        }

        public string VisitGroupingExpr(Expr.Grouping expr)
        {
            return Parenthesize("group", expr.Expression);
        }

        public string VisitLiteralExpr(Expr.Literal expr)
        {
            if (expr.Value == null)
            {
                return "null";
            }

            return expr.Value.ToString()!;
        }

        public string VisitLogicalExpr(Expr.Logical expr)
        {
            return Parenthesize(expr.Operator.Lexeme, expr.Left, expr.Right);
        }

        public string VisitUnaryPrefixExpr(Expr.UnaryPrefix expr)
        {
            return Parenthesize(expr.Operator.Lexeme, expr.Right);
        }

        public string VisitUnaryPostfixExpr(Expr.UnaryPostfix expr)
        {
            return Parenthesize(expr.Operator.Lexeme, expr.Left);
        }

        public string VisitIdentifierExpr(Expr.Identifier expr)
        {
            return expr.Name.Lexeme;
        }

        public string VisitGetExpr(Expr.Get expr)
        {
            return Parenthesize2(".", expr.Object, expr.Name.Lexeme);
        }

        //
        // Stmt.IVisitor<string> methods
        //

        public string VisitBlockStmt(Stmt.Block stmt)
        {
            StringBuilder builder = new();
            builder.Append("(block ");

            foreach (Stmt statement in stmt.Statements)
            {
                builder.Append(statement.Accept(this));
            }

            builder.Append(')');

            return builder.ToString();
        }

        public string VisitClassStmt(Stmt.Class stmt)
        {
            StringBuilder builder = new();
            builder.Append("(class " + stmt.Name.Lexeme);

            foreach (Stmt.Function method in stmt.Methods)
            {
                builder.Append(" " + Print(method));
            }

            builder.Append(')');

            return builder.ToString();
        }

        public string VisitExpressionStmt(Stmt.ExpressionStmt stmt)
        {
            return Parenthesize(";", stmt.Expression);
        }

        public string VisitFunctionStmt(Stmt.Function stmt)
        {
            StringBuilder builder = new();

            builder.Append("(fun " + stmt.Name.Lexeme + "(");

            foreach (Parameter param in stmt.Parameters)
            {
                if (param != stmt.Parameters[0])
                {
                    builder.Append(' ');
                }

                builder.Append(param.Name.Lexeme);
            }

            builder.Append(") ");

            foreach (Stmt body in stmt.Body)
            {
                builder.Append(body.Accept(this));
            }

            builder.Append(')');

            return builder.ToString();
        }

        public string VisitIfStmt(Stmt.If stmt)
        {
            if (stmt.ElseBranch == null)
            {
                return Parenthesize2("if", stmt.Condition, stmt.ThenBranch);
            }

            return Parenthesize2("if-else", stmt.Condition, stmt.ThenBranch, stmt.ElseBranch);
        }

        public string VisitPrintStmt(Stmt.Print stmt)
        {
            return Parenthesize("print", stmt.Expression);
        }

        public string VisitReturnStmt(Stmt.Return stmt)
        {
            if (stmt.Value == null)
            {
                return "(return)";
            }

            return Parenthesize("return", stmt.Value);
        }

        public string VisitVarStmt(Stmt.Var stmt)
        {
            if (stmt.Initializer == null)
            {
                return Parenthesize2("var", stmt.Name);
            }
            else
            {
                return Parenthesize2("var", stmt.Name, "=", stmt.Initializer);
            }
        }

        public string VisitWhileStmt(Stmt.While stmt)
        {
            return Parenthesize2("while", stmt.Condition, stmt.Body);
        }

        private string Parenthesize(string name, params Expr[] expressions)
        {
            var builder = new StringBuilder();

            builder.Append('(').Append(name);

            foreach (Expr expr in expressions)
            {
                builder.Append(' ');
                builder.Append(expr.Accept(this));
            }

            builder.Append(')');

            return builder.ToString();
        }

        private string Parenthesize2(string name, params object[] parts)
        {
            StringBuilder builder = new();

            builder.Append('(').Append(name);
            Transform(builder, parts);
            builder.Append(')');

            return builder.ToString();
        }

        private void Transform(StringBuilder builder, params object[] parts)
        {
            foreach (object part in parts)
            {
                builder.Append(' ');

                if (part is Expr expr)
                {
                    builder.Append(expr.Accept(this));
                }
                else if (part is Stmt stmt)
                {
                    builder.Append(stmt.Accept(this));
                }
                else if (part is Token token)
                {
                    builder.Append(token.Lexeme);
                }
                else if (part is IList<object> list)
                {
                    Transform(builder, list.ToArray());
                }
                else
                {
                    builder.Append(part);
                }
            }
        }
    }
}
