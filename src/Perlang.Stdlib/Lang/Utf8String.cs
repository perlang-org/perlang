#nullable enable
#pragma warning disable SA1300
using System;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using Perlang.Internal;

namespace Perlang.Lang;

/// <summary>
/// Representation of a string encoded with the UTF-8 encoding.
///
/// UTF-8 strings can represent the full Unicode span of characters. Their main disadvantage is that the variable-length
/// encoding of non-ASCII characters makes looping over characters be a much more complex process than with <see
/// cref="AsciiString"/> or the future Utf16String.
/// </summary>
public class Utf8String : String
{
    private readonly unsafe byte* bytes;
    private readonly nuint length;

    public override nuint Length => length;

    internal override unsafe IntPtr Bytes => (IntPtr)bytes;

    /// <summary>
    /// Initializes a new instance of the <see cref="Utf8String"/> class from an existing <see cref="char"/> array. A
    /// new <see cref="byte"/> buffer will be allocated to contain the string content.
    /// </summary>
    /// <param name="s">A .NET string.</param>
    /// <returns>A newly allocated <see cref="Utf8String"/>.</returns>
    public static new Utf8String from(string s)
    {
        return new Utf8String(s);
    }

    // TODO: Try to get rid of this constructor, once we have rewritten the scanner to use Perlang strings instead
    // TODO: of .NET strings.

    private unsafe Utf8String(string s)
    {
        // Inspired by System.Runtime.InteropServices.Marshalling.Utf8StringMarshaller in the .NET 7 framework.
        length = (nuint)checked(Encoding.UTF8.GetByteCount(s) + 1);
        bytes = (byte*)MemoryAllocator.Allocate(length);

        // We use a Span<> here as a convenient way to make GetBytes() be able to write into our unmanaged memory
        // buffer.
        Span<byte> managedBuffer = new Span<byte>(bytes, (int)length);
        int bytesWritten = Encoding.UTF8.GetBytes(s, managedBuffer);

        // Add a trailing NUL character, to make it possible to use this byte array with methods expecting
        // NUL-terminated strings.
        bytes[bytesWritten] = 0;
    }

    public override String to_upper()
    {
        throw new NotImplementedException();
    }

    public override unsafe string ToString()
    {
        return Utf8StringMarshaller.ConvertToManaged(bytes)!;
    }
}
