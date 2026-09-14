#pragma warning disable SA1601
#pragma warning disable S2372
#pragma warning disable S3903
#nullable enable

using Perlang.Compiler;

public partial class ITokenInternal
{
    public override Perlang.TokenType Type => perlang_cli.GetTokenType(this);

    public override string Lexeme => perlang_cli.GetTokenLexeme(this);

    public override object? Literal
    {
        get
        {
            if (perlang_cli.IsStringToken(this)) {
                return perlang_cli.GetTokenStringLiteral(this);
            }
            else if (perlang_cli.IsCharToken(this)) {
                return perlang_cli.GetTokenCharLiteral(this);
            }
            else if (perlang_cli.IsNullToken(this)) {
                return null;
            }
            else {
                throw new PerlangCompilerException("Internal error: Unexpected token type encountered");
            }
        }
    }

    public override string FileName =>
        perlang_cli.GetTokenFileName(this);

    public override int Line =>
        perlang_cli.GetTokenLine(this);

    // The CppSharp-generated implementation overwrites the IToken vptr of every wrapped token with a copy of the vtable
    // of the first IToken-deriving instance it sees. This breaks virtual dispatch on the native side for all other
    // IToken-deriving types. Overriding this method here prevents this breakage.
    internal override void SetupVTables(bool destructorOnly = false)
    {
        // No-op
    }
}
