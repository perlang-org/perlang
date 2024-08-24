#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable S101
#pragma warning disable SA1300
#pragma warning disable SA1302
#pragma warning disable SA1623

namespace Perlang.Lang;

/// <summary>
/// Base class for Perlang strings
///
/// Concrete implementations of this string are currently the following:
///
/// - <see cref="AsciiString"/> - a string known to consist of valid ASCII characters only. The uppermost bit in each
///                               byte is zero for all characters.
/// - <see cref="Utf8String"/> - a string which can contain all Unicode characters, encoded with the UTF-8 encoding.
/// </summary>
public abstract class String
{
    public static String Empty { get; } = AsciiString.from("");
    public static String Newline { get; } = from(Environment.NewLine);

    public abstract nuint Length { get; }

    /// <summary>
    /// Framework-internal property, used for retrieving a pointer to the unmanaged backing byte buffer for this string.
    /// The byte buffer is to be considered immutable. In the future, we hope to be able to enforce this using language
    /// features.
    /// </summary>
    internal abstract IntPtr Bytes { get; }

    public abstract Lang.String to_upper();

    /// <summary>
    /// Creates a concrete String implementation based on the given CLR string. If the string contains only ASCII
    /// characters, an <see cref="AsciiString"/> will be returned. Otherwise, a <see cref="Utf8String"/> is returned.
    ///
    /// If the given parameter is `null`, a `null` reference will be returned.
    /// </summary>
    /// <param name="s">a CLR string.</param>
    /// <returns>A <see cref="String"/> implementation.</returns>
    [return: NotNullIfNotNull(nameof(s))]
    public static String? from(string? s)
    {
        if (s == null)
        {
            return null;
        }

        char[] chars = s.ToCharArray();

        foreach (char c in chars)
        {
            if (c > 127)
            {
                // Non-ASCII character encountered => use Utf8String instead
                // TODO: think about whether Utf16String would be a better default here. Could be Utf16 by
                // TODO: default and opt-in to Utf8 (or the other way around). See
                // TODO: https://gitlab.perlang.org/perlang/perlang/-/issues/370
                return Utf8String.from(s);
            }
        }

        // All characters in the string are ASCII safe => represent this
        // string as an AsciiString.
        return AsciiString.from(chars);
    }

    /// <summary>
    /// Constructs a .NET string based on the given Perlang String.
    /// </summary>
    /// <remarks>Note that this does not reuse the backing byte buffer for the string; it will cause a new byte buffer
    /// to be allocated and the Perlang String to be converted to .NET (UTF-16) format. This method should not be
    /// called in performance-critical code, and it should preferably be avoided altogether.</remarks>
    /// <returns>A new .NET string.</returns>
    public abstract override string ToString();
}
