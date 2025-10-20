#pragma warning disable SA1601
#pragma warning disable S2372
#pragma warning disable S3903
#nullable enable

using Perlang;
using Perlang.Compiler;

public partial class Token : IToken
{
    public string Lexeme => perlang_cli.GetTokenLexeme(this);

    public object? Literal
    {
        get {
            if (perlang_cli.IsStringToken(this)) {
                return perlang_cli.GetTokenStringLiteral(this);
            }
            else if (perlang_cli.IsCharToken(this)) {
                return perlang_cli.GetTokenCharLiteral(this);
            }
            else {
                throw new PerlangCompilerException("Internal error: Unexpected token type encountered");
            }
        }
    }

    public string FileName => perlang_cli.GetTokenFileName(this);
}
