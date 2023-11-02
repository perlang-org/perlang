#nullable enable
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

    public bool HasExpr => Expr != null;
    public bool HasStatements => Statements != null;

    private ScanAndParseResult()
    {
    }

    private ScanAndParseResult(Expr expr)
    {
        Expr = expr;
    }

    private ScanAndParseResult(List<Stmt> statements)
    {
        Statements = statements;
    }

    public static ScanAndParseResult OfExpr(Expr expr)
    {
        return new ScanAndParseResult(expr);
    }

    public static ScanAndParseResult OfStmts(List<Stmt> stmts)
    {
        return new ScanAndParseResult(stmts);
    }
}
