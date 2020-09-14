using System.Collections.Generic;
using System.Reflection;

namespace Perlang
{
    public abstract class Expr
    {
        public TypeReference TypeReference { get; }

        private Expr()
        {
            TypeReference = new TypeReference(typeSpecifier: null);
        }

        public interface IVisitor<out TR>
        {
            TR VisitEmptyExpr(Empty expr);
            TR VisitAssignExpr(Assign expr);
            TR VisitBinaryExpr(Binary expr);
            TR VisitCallExpr(Call expr);
            TR VisitGroupingExpr(Grouping expr);
            TR VisitLiteralExpr(Literal expr);
            TR VisitLogicalExpr(Logical expr);
            TR VisitUnaryPrefixExpr(UnaryPrefix expr);
            TR VisitUnaryPostfixExpr(UnaryPostfix expr);
            TR VisitIdentifierExpr(Identifier expr);
            TR VisitGetExpr(Get expr);
        }

        //
        // Expression types follows.
        //

        public class Empty : Expr
        {
            public override TR Accept<TR>(IVisitor<TR> visitor)
            {
                return visitor.VisitEmptyExpr(this);
            }
        }

        public class Assign : Expr
        {
            public Token Name { get; }
            public Expr Value { get; }

            public Assign(Token name, Expr value)
            {
                Name = name;
                Value = value;
            }

            public override TR Accept<TR>(IVisitor<TR> visitor)
            {
                return visitor.VisitAssignExpr(this);
            }
        }

        public class Binary : Expr
        {
            public Expr Left { get; }
            public Token Operator { get; }
            public Expr Right { get; }

            public Binary(Expr left, Token @operator, Expr right)
            {
                Left = left;
                Operator = @operator;
                Right = right;
            }

            public override TR Accept<TR>(IVisitor<TR> visitor)
            {
                return visitor.VisitBinaryExpr(this);
            }

            public override string ToString()
            {
                return $"{Left} {Operator} {Right}";
            }
        }

        public class Call : Expr
        {
            public Expr Callee { get; }
            public Token Paren { get; }
            public List<Expr> Arguments { get; }

            public string CalleeToString
            {
                get
                {
                    if (Callee is Identifier variable)
                    {
                        return variable.Name.Lexeme;
                    }
                    else if (Callee is Get get)
                    {
                        return get.Name.Lexeme;
                    }
                    else
                    {
                        return ToString();
                    }
                }
            }

            public Call(Expr callee, Token paren, List<Expr> arguments)
            {
                Callee = callee;
                Paren = paren;
                Arguments = arguments;
            }

            public override TR Accept<TR>(IVisitor<TR> visitor)
            {
                return visitor.VisitCallExpr(this);
            }

            public override string ToString()
            {
                if (Callee is Identifier variable)
                {
                    return $"'call function {variable.Name.Lexeme}'";
                }
                else
                {
                    return base.ToString();
                }
            }
        }

        public class Grouping : Expr
        {
            public Expr Expression { get; }

            public Grouping(Expr expression)
            {
                Expression = expression;
            }

            public override TR Accept<TR>(IVisitor<TR> visitor)
            {
                return visitor.VisitGroupingExpr(this);
            }
        }

        public class Literal : Expr
        {
            public object Value { get; }

            public Literal(object value)
            {
                Value = value;
            }

            public override TR Accept<TR>(IVisitor<TR> visitor)
            {
                return visitor.VisitLiteralExpr(this);
            }

            public override string ToString()
            {
                return Value?.ToString() ?? "null";
            }
        }

        public class Logical : Expr
        {
            public Expr Left { get; }
            public Token Operator { get; }
            public Expr Right { get; }

            public Logical(Expr left, Token @operator, Expr right)
            {
                Left = left;
                Operator = @operator;
                Right = right;
            }

            public override TR Accept<TR>(IVisitor<TR> visitor)
            {
                return visitor.VisitLogicalExpr(this);
            }
        }

        public class UnaryPrefix : Expr
        {
            public Token Operator { get; }
            public Expr Right { get; }

            public UnaryPrefix(Token @operator, Expr right)
            {
                Operator = @operator;
                Right = right;
            }

            public override TR Accept<TR>(IVisitor<TR> visitor)
            {
                return visitor.VisitUnaryPrefixExpr(this);
            }
        }

        public class UnaryPostfix : Expr
        {
            public Expr Left { get; }
            public Token Name { get; }
            public Token Operator { get; }

            public UnaryPostfix(Expr left, Token name, Token @operator)
            {
                Left = left;
                Name = name;
                Operator = @operator;
            }

            public override TR Accept<TR>(IVisitor<TR> visitor)
            {
                return visitor.VisitUnaryPostfixExpr(this);
            }
        }

        /// <summary>
        /// Represents an identifier, such as a variable name, a function name or a class.
        /// </summary>
        public class Identifier : Expr
        {
            public Token Name { get; }

            public Identifier(Token name)
            {
                Name = name;
            }

            public override TR Accept<TR>(IVisitor<TR> visitor)
            {
                return visitor.VisitIdentifierExpr(this);
            }

            public override string ToString() =>
                Name.Lexeme;
        }

        public class Get : Expr
        {
            public Expr Object { get; }
            public Token Name { get; }

            // TODO: Would be much nicer to have this be immutable, but there is no easy way to accomplish this, since
            // TODO: the MethodInfo data isn't available at construction time.
            public MethodInfo Method { get; set; }

            public Get(Expr @object, Token name)
            {
                Object = @object;
                Name = name;
            }

            public override TR Accept<TR>(IVisitor<TR> visitor)
            {
                return visitor.VisitGetExpr(this);
            }
        }

        public abstract TR Accept<TR>(IVisitor<TR> visitor);
    }
}
