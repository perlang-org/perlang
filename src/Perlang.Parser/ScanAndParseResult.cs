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
    public List<Token> CppMethods { get; set; }
    public List<Token> CppPrototypes { get; set; }

    public bool HasExpr => Expr != null;
    public bool HasStatements => Statements != null;

    private ScanAndParseResult()
    {
        CppMethods = new List<Token>();
        CppPrototypes = new List<Token>();
    }

    private ScanAndParseResult(Expr expr) : this()
    {
        Expr = expr;
    }

    private ScanAndParseResult(List<Stmt> statements, List<Token> cppPrototypes, List<Token> cppMethods)
    {
        Statements = statements;
        CppPrototypes = cppPrototypes;
        CppMethods = cppMethods;
    }

    public static ScanAndParseResult OfExpr(Expr expr)
    {
        return new ScanAndParseResult(expr);
    }

    public static ScanAndParseResult OfStmts(List<Stmt> stmts, List<Token> cppPrototypes, List<Token> cppMethods)
    {
        return new ScanAndParseResult(stmts, cppPrototypes, cppMethods);
    }
}
