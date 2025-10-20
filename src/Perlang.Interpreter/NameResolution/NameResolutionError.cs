using System;

namespace Perlang.Interpreter.NameResolution;

/// <summary>
/// Emitted for name resolution errors which can be detected at an early stage, before type validation is even
/// attempted.
/// </summary>
public class NameResolutionError : Exception
{
    public IToken Token { get; }

    public NameResolutionError(string message, IToken token)
        : base(message)
    {
        Token = token;
    }

    public override string ToString()
    {
        string where;

        if (Token.Type == TokenType.PERLANG_EOF)
        {
            where = " at end";
        }
        else
        {
            where = " at '" + Token.Lexeme + "'";
        }

        return $"[line {Token.Line}] Error{where}: {Message}";
    }
}
