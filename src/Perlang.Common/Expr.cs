#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;

namespace Perlang
{
    public abstract class Expr
    {
        public ITypeReference TypeReference { get; }

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

        /// <summary>
        /// An assignment expression.
        /// </summary>
        public class Assign : Expr, ITokenAware
        {
            /// <summary>
            /// Gets the identifier which is the target of the assignment. For example, in the `a = 42` expression, `a` is the
            /// identifier.
            /// </summary>
            public new Identifier Identifier { get; }

            /// <summary>
            /// Gets the value being assigned. Can be either a compile-time constant or an expression with a dynamic
            /// value, computed at runtime.
            /// </summary>
            public Expr Value { get; }

            /// <summary>
            /// Gets the name of the identifier being assigned to.
            /// </summary>
            public Token Name => Identifier.Name;

            public Assign(Identifier identifier, Expr value)
            {
                Identifier = identifier;
                Value = value;
            }

            public override TR Accept<TR>(IVisitor<TR> visitor)
            {
                return visitor.VisitAssignExpr(this);
            }

            public override string ToString()
            {
                return $"{Name.Lexeme} = {Value}";
            }

            public Token Token => Name;
        }

        public class Binary : Expr, ITokenAware
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

            public Token Token => Operator;
        }

        public class Call : Expr, ITokenAware
        {
            public Expr Callee { get; }
            public Token Paren { get; }
            public List<Expr> Arguments { get; }

            public string? CalleeToString
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

            public override string? ToString()
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

            public Token Token => Paren;
        }

        public class Grouping : Expr, ITokenAware
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

            public Token? Token => (Expression as ITokenAware)?.Token;
        }

        public class Literal : Expr
        {
            public object? Value { get; }

            public Literal(object? value)
            {
                Value = value;
            }

            public override TR Accept<TR>(IVisitor<TR> visitor)
            {
                return visitor.VisitLiteralExpr(this);
            }

            public override string ToString()
            {
                if (Value is string s)
                {
                    // Surround with quotes to make it more distinguishable when debugging.
                    return '"' + s + '"';
                }
                else
                {
                    return Value?.ToString() ?? "null";
                }
            }
        }

        public class Logical : Expr, ITokenAware
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

            public Token Token => Operator;
        }

        /// <summary>
        /// Represents unary prefix expressions. Examples of such expressions are `!flag` and `-10`. Note that prefix
        /// increment and decrement are currently not supported.
        /// </summary>
        public class UnaryPrefix : Expr, ITokenAware
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

            public Token Token => Operator;
        }

        /// <summary>
        /// Represents unary postfix expressions. Examples of such expressions are `i++` and `j--`.
        /// </summary>
        public class UnaryPostfix : Expr, ITokenAware
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

            public Token Token => Operator;
        }

        /// <summary>
        /// Represents an identifier, such as a variable name, a function name or a class.
        /// </summary>
        public class Identifier : Expr, ITokenAware
        {
            public Token Name { get; }

            public Identifier(Token name)
            {
                Name = name ?? throw new ArgumentException("name cannot be null");
            }

            public override TR Accept<TR>(IVisitor<TR> visitor)
            {
                return visitor.VisitIdentifierExpr(this);
            }

            public override string ToString() =>
                $"#<Identifier {Name.Lexeme}>";

            public Token Token => Name;
        }

        public class Get : Expr, ITokenAware
        {
            public Expr Object { get; }
            public Token Name { get; }

            // TODO: Would be much nicer to have this be without setter, but there is no easy way to accomplish this,
            // TODO: since the MethodInfo data isn't available at construction time. Also, we replace this with the
            // TODO: single matching MethodInfo after method overload resolution has completed.
            public ImmutableArray<MethodInfo> Methods { get; set; } = ImmutableArray<MethodInfo>.Empty;

            public Get(Expr @object, Token name)
            {
                Object = @object ?? throw new ArgumentException("object cannot be null");
                Name = name;
            }

            public override TR Accept<TR>(IVisitor<TR> visitor)
            {
                return visitor.VisitGetExpr(this);
            }

            public Token Token => Name;
        }

        public abstract TR Accept<TR>(IVisitor<TR> visitor);
    }
}
