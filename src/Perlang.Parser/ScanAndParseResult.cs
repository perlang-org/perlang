#nullable enable
#pragma warning disable SA1128
using System.Collections.Generic;

namespace Perlang.Parser;

/// <summary>
/// Contains the result of the <see cref="PerlangParser.ScanAndParse"/> method.
/// </summary>
public class ScanAndParseResult
{
    public static ScanAndParseResult ScanErrorOccurred { get; } = new();
    public static ScanAndParseResult ParseErrorEncountered { get; } = new();

    public Expr? Expr { get; }
    public List<Stmt>? Statements { get; }
    public List<IToken> CppMethods { get; }
    public List<IToken> CppPrototypes { get; }
    public List<IToken> Tokens { get; }

    public bool HasExpr => Expr != null;
    public bool HasStatements => Statements != null;

    private ScanAndParseResult()
    {
        CppMethods = new List<IToken>();
        CppPrototypes = new List<IToken>();
        Tokens = new List<IToken>();
    }

    private ScanAndParseResult(Expr expr, List<IToken> tokens) : this()
    {
        Expr = expr;
        Tokens = tokens;
    }

    private ScanAndParseResult(List<Stmt> statements, List<IToken> cppPrototypes, List<IToken> cppMethods, List<IToken> tokens)
    {
        Statements = statements;
        CppPrototypes = cppPrototypes;
        CppMethods = cppMethods;
        Tokens = tokens;
    }

    public static ScanAndParseResult OfExpr(Expr expr, List<IToken> tokens)
    {
        return new ScanAndParseResult(expr, tokens);
    }

    public static ScanAndParseResult OfStmts(List<Stmt> stmts, List<IToken> cppPrototypes, List<IToken> cppMethods, List<IToken> tokens)
    {
        return new ScanAndParseResult(stmts, cppPrototypes, cppMethods, tokens);
    }
}
