#pragma warning disable SA1601
#pragma warning disable S3903
#nullable enable

using Perlang;

public partial class PerlangScanner
{
    // These need to be wrapped since the C++ methods return std::variant types, which is not easily consumable from C#

    public static bool IsAlphaNumeric(char c)
    {
        return perlang_cli.IsAlphaNumericWrapper(c);
    }

    public static bool IsDigit(char c, NumericTokenBase numberBase)
    {
        return perlang_cli.IsDigitWrapper(c, numberBase);
    }
}
