#nullable enable
#pragma warning disable SA1010
#pragma warning disable SA1117

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Perlang;

public abstract class Stmt
{
    public interface IVisitor<out TR>
    {
        TR VisitBlockStmt(Block stmt);
        TR VisitClassStmt(Class stmt);
        TR VisitEnumStmt(Enum stmt);
        TR VisitExpressionStmt(ExpressionStmt stmt);
        TR VisitFunctionStmt(Function stmt);
        TR VisitFieldStmt(Field stmt);
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

    public class Class : Stmt, IPerlangClass
    {
        public static readonly Class None = new Class();

        public string Name { get; }
        public IToken NameToken { get; }
        public Visibility Visibility { get; }
        public ImmutableList<IPerlangFunction> Methods { get; }
        public ImmutableList<Function> StmtMethods { get; }

        // TODO: Should use some form of dictionary type here for faster lookups, but preferably with something like
        // TODO: Guava's ImmutableMap in Java (which preserves insertion order).
        public ImmutableList<IPerlangField> Fields { get; }
        public ImmutableList<Field> StmtFields { get; }

        public TypeReference TypeReference { get; }

        private Class()
        {
            Name = string.Empty;
            NameToken = null!;
            StmtMethods = [];
            Methods = [];
            StmtFields = [];
            Fields = [];
            TypeReference = null!;
        }

        public Class(IToken name, Visibility visibility, IList<Function> methods, IList<Field> fields, TypeReference typeReference)
        {
            Name = name.Lexeme;
            NameToken = name;
            Visibility = visibility;
            StmtMethods = methods.ToImmutableList();
            Methods = StmtMethods.Cast<IPerlangFunction>().ToImmutableList();
            StmtFields = fields.ToImmutableList();
            Fields = StmtFields.Cast<IPerlangField>().ToImmutableList();
            TypeReference = typeReference;
        }

        public override TR Accept<TR>(IVisitor<TR> visitor)
        {
            return visitor.VisitClassStmt(this);
        }

        public override string ToString() =>
            Name;
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

        public override string? ToString() =>
            Expression.ToString();
    }

    public class Function : Stmt, IPerlangFunction
    {
        public new Class Class { get; private set; }
        public IToken NameToken { get; }
        public string Name => NameToken.Lexeme;
        public Visibility Visibility { get; }
        public ImmutableList<Parameter> Parameters { get; }
        public ImmutableList<Stmt> Body { get; }
        public ITypeReference ReturnTypeReference { get; }
        public bool IsConstructor { get; set; }
        public bool IsDestructor { get; set; }
        public bool IsExtern { get; }
        public bool IsStatic { get; }

        public Function(IToken name, Visibility visibility, IEnumerable<Parameter> parameters, IEnumerable<Stmt> body, TypeReference returnTypeReference,
            bool isConstructor, bool isDestructor, bool isExtern, bool isStatic) {
            NameToken = name ?? throw new System.ArgumentNullException(nameof(name));
            Visibility = visibility;
            Parameters = parameters.ToImmutableList();
            Body = body.ToImmutableList();
            ReturnTypeReference = returnTypeReference;
            IsConstructor = isConstructor;
            IsDestructor = isDestructor;
            IsExtern = isExtern;
            IsStatic = isStatic;
            Class = Stmt.Class.None;
        }

        // Cannot be set in constructor, since the Function instances are created before the class.
        public void SetClass(Class @class)
        {
            Class = @class;
        }

        public override TR Accept<TR>(IVisitor<TR> visitor)
        {
            return visitor.VisitFunctionStmt(this);
        }

        public override string ToString()
        {
            // TODO: Include parameters here as well.
            return $"fun {NameToken.Lexeme}(): {ReturnTypeReference.TypeKeywordOrPerlangType}";
        }
    }

    public class Field : Stmt, IPerlangField
    {
        public IToken NameToken { get; }
        public string Name => NameToken.Lexeme;
        public Visibility Visibility { get; }
        public bool IsMutable { get; }
        public Expr? Initializer { get; }
        public ITypeReference TypeReference { get; }

        public Field(IToken name, Visibility visibility, bool isMutable, Expr? initializer, ITypeReference typeReference)
        {
            NameToken = name;
            Visibility = visibility;
            IsMutable = isMutable;
            Initializer = initializer;
            TypeReference = typeReference;
        }

        public override TR Accept<TR>(IVisitor<TR> visitor)
        {
            return visitor.VisitFieldStmt(this);
        }

        public override string ToString()
        {
            if (Initializer != null)
            {
                if (TypeReference.TypeSpecifier?.Lexeme != null)
                {
                    return $"{Visibility.ToString().ToLower()} {Name}: {TypeReference.TypeSpecifier.Lexeme} = {Initializer};";
                }
                else
                {
                    return $"{Visibility.ToString().ToLower()} {Name} = {Initializer};";
                }
            }
            else
            {
                if (TypeReference.TypeSpecifier?.Lexeme != null)
                {
                    return $"{Visibility.ToString().ToLower()} {Name}: {TypeReference.TypeSpecifier.Lexeme};";
                }
                else
                {
                    return $"{Visibility.ToString().ToLower()} {Name};";
                }
            }
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
        public IToken Keyword { get; }
        public Expr? Value { get; }

        public Return(IToken keyword, Expr? value)
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
        public IToken Name { get; }
        public Expr? Initializer { get; }
        public ITypeReference TypeReference { get; }

        public Var(IToken name, Expr? initializer, TypeReference typeReference)
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
                    return $"var {Name.Lexeme}: {TypeReference.TypeKeywordOrPerlangType} = {Initializer};";
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
                    return $"var {Name.Lexeme}: {TypeReference.TypeKeywordOrPerlangType};";
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
        public IToken Name { get; }
        public Dictionary<string, Expr?> Members { get; }

        public Enum(IToken name, Dictionary<string, Expr?> members)
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
