#nullable  enable
namespace Perlang.Parser;

public interface IFloatingPointLiteral
{
    /// <summary>
    /// Gets a string representation of this floating point literal. This is to ensure we avoid precision loss while
    /// carrying the value over to the compiler, since `float`/`double` `ToString()` and back are not necessarily
    /// round-trip safe.
    /// </summary>
    public string NumberCharacters { get; }
}
