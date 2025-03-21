#nullable enable
#pragma warning disable SA1117

using System.Collections.Generic;
using System.Collections.Immutable;

namespace Perlang
{
    public abstract class Stmt
    {
        public interface IVisitor<out TR>
        {
            TR VisitBlockStmt(Block stmt);
            TR VisitClassStmt(Class stmt);
            TR VisitEnumStmt(Enum stmt);
            TR VisitExpressionStmt(ExpressionStmt stmt);
            TR VisitFunctionStmt(Function stmt);
            TR VisitIfStmt(If stmt);
            TR VisitPrintStmt(Print stmt);
            TR VisitReturnStmt(Return stmt);
            TR VisitVarStmt(Var stmt);
            TR VisitWhileStmt(While stmt);
        }

        public class Block : Stmt
        {
            public List<Stmt> Statements { get; }

            public Block(List<Stmt> statements)
            {
                Statements = statements;
            }

            public override TR Accept<TR>(IVisitor<TR> visitor)
            {
                return visitor.VisitBlockStmt(this);
            }
        }

        public class Class : Stmt
        {
            public Token Name { get; }
            public List<Function> Methods { get; }
            public Visibility Visibility { get; }
            public TypeReference TypeReference { get; }

            public Class(Token name, Visibility visibility, List<Function> methods, TypeReference typeReference)
            {
                Name = name;
                Visibility = visibility;
                Methods = methods;
                TypeReference = typeReference;
            }

            public override TR Accept<TR>(IVisitor<TR> visitor)
            {
                return visitor.VisitClassStmt(this);
            }
        }

        /// <summary>
        /// An expression statement is a statement for wrapping a single expression in statement form.
        /// </summary>
        public class ExpressionStmt : Stmt
        {
            public Expr Expression { get; }

            public ExpressionStmt(Expr expression)
            {
                Expression = expression;
            }

            public override TR Accept<TR>(IVisitor<TR> visitor)
            {
                return visitor.VisitExpressionStmt(this);
            }
        }

        public class Function : Stmt
        {
            public Token Name { get; }
            public ImmutableList<Parameter> Parameters { get; }
            public ImmutableList<Stmt> Body { get; }
            public ITypeReference ReturnTypeReference { get; }
            public bool IsConstructor { get; set; }
            public bool IsDestructor { get; set; }

            public Function(
                Token name, IEnumerable<Parameter> parameters, IEnumerable<Stmt> body, TypeReference returnTypeReference,
                bool isConstructor, bool isDestructor
            ) {
                Name = name;
                Parameters = parameters.ToImmutableList();
                Body = body.ToImmutableList();
                ReturnTypeReference = returnTypeReference;
                IsConstructor = isConstructor;
                IsDestructor = isDestructor;
            }

            public override TR Accept<TR>(IVisitor<TR> visitor)
            {
                return visitor.VisitFunctionStmt(this);
            }

            public override string ToString()
            {
                // TODO: Include parameters here as well.
                return $"fun {Name.Lexeme}(): {ReturnTypeReference.ClrType}";
            }
        }

        public class If : Stmt
        {
            public Expr Condition { get; }
            public Stmt ThenBranch { get; }
            public Stmt? ElseBranch { get; }

            public If(Expr condition, Stmt thenBranch, Stmt? elseBranch)
            {
                Condition = condition;
                ThenBranch = thenBranch;
                ElseBranch = elseBranch;
            }

            public override TR Accept<TR>(IVisitor<TR> visitor)
            {
                return visitor.VisitIfStmt(this);
            }
        }

        public class Print : Stmt
        {
            public Expr Expression { get; }

            public Print(Expr expression)
            {
                Expression = expression;
            }

            public override TR Accept<TR>(IVisitor<TR> visitor)
            {
                return visitor.VisitPrintStmt(this);
            }
        }

        public class Return : Stmt
        {
            public Token Keyword { get; }
            public Expr? Value { get; }

            public Return(Token keyword, Expr? value)
            {
                Keyword = keyword;
                Value = value;
            }

            public override TR Accept<TR>(IVisitor<TR> visitor)
            {
                return visitor.VisitReturnStmt(this);
            }
        }

        public class Var : Stmt
        {
            public Token Name { get; }
            public Expr? Initializer { get; }
            public ITypeReference TypeReference { get; }

            public Var(Token name, Expr? initializer, TypeReference typeReference)
            {
                Name = name;
                Initializer = initializer;
                TypeReference = typeReference;
            }

            public override TR Accept<TR>(IVisitor<TR> visitor)
            {
                return visitor.VisitVarStmt(this);
            }

            public override string ToString()
            {
                if (Initializer != null)
                {
                    if (TypeReference.TypeSpecifier?.Lexeme != null)
                    {
                        return $"var {Name.Lexeme}: {TypeReference.TypeSpecifier.Lexeme} = {Initializer};";
                    }
                    else
                    {
                        return $"var {Name.Lexeme} = {Initializer};";
                    }
                }
                else
                {
                    if (TypeReference.TypeSpecifier?.Lexeme != null)
                    {
                        return $"var {Name.Lexeme}: {TypeReference.TypeSpecifier.Lexeme};";
                    }
                    else
                    {
                        return $"var {Name.Lexeme};";
                    }
                }
            }
        }

        public class While : Stmt
        {
            public Expr Condition { get; }
            public Stmt Body { get; }

            public While(Expr condition, Stmt body)
            {
                Condition = condition;
                Body = body;
            }

            public override TR Accept<TR>(IVisitor<TR> visitor)
            {
                return visitor.VisitWhileStmt(this);
            }
        }

        public class Enum : Stmt
        {
            public Token Name { get; }
            public Dictionary<string, Expr?> Members { get; }

            public Enum(Token name, Dictionary<string, Expr?> members)
            {
                Members = members;
                Name = name;
            }

            public override TR Accept<TR>(IVisitor<TR> visitor)
            {
                return visitor.VisitEnumStmt(this);
            }
        }

        public abstract TR Accept<TR>(IVisitor<TR> visitor);
    }
}
