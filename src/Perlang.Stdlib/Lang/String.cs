#nullable enable
#pragma warning disable S101
#pragma warning disable SA1302

namespace Perlang.Lang;

/// <summary>
/// Base class for Perlang strings
///
/// Concrete implementations of this string are currently the following:
///
/// - <see cref="AsciiString"/> - a string known to consist of valid ASCII characters only. The uppermost bit in each
///                               byte is zero for all characters.
/// </summary>
public abstract class String
{
    public abstract nuint Length { get; }

    public abstract Lang.String ToUpper();

    public abstract override string ToString();
}
