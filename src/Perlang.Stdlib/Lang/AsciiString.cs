#nullable enable
#pragma warning disable S112
#pragma warning disable SA1300
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Perlang.Internal;
using Perlang.Stdlib;

namespace Perlang.Lang;

/// <summary>
/// Representation of a string with ASCII-only content.
///
/// <see cref="AsciiString"/> have the following characteristics:
///
/// - They consist of valid ASCII characters (0-127) only.
///
/// - They are _immutable_. Once constructed, an <see cref="AsciiString"/> cannot be modified. Methods which seem to
///   perform "modifications" of the string like <see cref="to_upper"/> etc. will always allocate a new instance and
///   operate on that.
///
/// - They are _backed by native memory_. This means that they can be used with zero/very low cost together with
///   standard POSIX/libc functions like `puts`, `memcmp`, etc.
/// </summary>
public class AsciiString : Lang.String
{
    private readonly unsafe byte* bytes;
    private readonly nuint length;

    public override nuint Length => length;
    internal override unsafe IntPtr Bytes => (IntPtr)bytes;

    /// <summary>
    /// Memoized uppercase form of this string. Will be initialized on the first call to <see cref="to_upper"/>.
    /// </summary>
    private AsciiString? upperString;

    /// <summary>
    /// Initializes a new instance of the <see cref="AsciiString"/> class from an existing <see cref="char"/> array. A
    /// new <see cref="byte"/> buffer will be allocated to contain the string content.
    /// </summary>
    /// <param name="chars">A char array, validated by the caller to be ASCII safe.</param>
    /// <returns>A newly allocated <see cref="AsciiString"/>.</returns>
    public static AsciiString from(IReadOnlyList<char> chars)
    {
        // TODO: Consider validating the data here after all, or try to enforce that the caller has validated it
        // TODO: somehow. Enforcing correctness might be a more noble goal than speed in this use case.
        return new AsciiString(chars);
    }

    /// <summary>
    /// Creates a new <see cref="AsciiString"/> from the given .NET string. Note that this method will allocate a new
    /// backing buffer for holding the string contents; it will not reuse the existing bytes.
    /// </summary>
    /// <param name="s">The input string.</param>
    /// <returns>A newly allocated <see cref="AsciiString"/>.</returns>
    /// <exception cref="ArgumentException">The given string contains non-ASCII characters.</exception>
    public static new AsciiString from(string s)
    {
        char[] chars = s.ToCharArray();

        foreach (char c in chars)
        {
            if (c > 127)
            {
                throw new ArgumentException($"Attempted to construct an AsciiString from a string which contains non-ASCII characters: {s}");
            }
        }

        return new AsciiString(chars);
    }

    /// <summary>
    /// Concatenates the given strings. This method will allocate a new <see cref="AsciiString"/> and the existing
    /// string data will be copied to it.
    /// </summary>
    /// <param name="a">The first string to concatenate.</param>
    /// <param name="b">The second string to concatenate.</param>
    /// <returns>A concatenated string (`a` + `b`).</returns>
    public static unsafe AsciiString operator +(AsciiString a, AsciiString b)
    {
        nuint resultLength = a.Length + b.Length;
        byte* c = (byte*)MemoryAllocator.Allocate(resultLength + 1);

        // TODO: Consider converting to call memcpy(), to be less .NET dependent.
        Buffer.MemoryCopy(a.bytes, c, resultLength + 1, a.Length);
        Buffer.MemoryCopy(b.bytes, c + a.Length, resultLength + 1 - a.Length, b.Length);

        // Add a trailing NUL character
        c[resultLength] = 0;

        return from(c, resultLength);
    }

    /// <summary>
    /// Determines whether the given strings are identical.
    /// </summary>
    /// <param name="a">The first string to compare, or `null`.</param>
    /// <param name="b">The second string to compare, or `null`.</param>
    /// <returns>`true` if `a` is identical to `b`; otherwise, `false`.</returns>
    public static bool operator ==(AsciiString? a, AsciiString? b)
    {
        return a?.GetHashCode() == b?.GetHashCode();
    }

    /// <summary>
    /// Determined whether the given strings are non-identical.
    /// </summary>
    /// <param name="a">The first string to compare, or `null`.</param>
    /// <param name="b">The second string to compare, or `null`.</param>
    /// <returns>`true` if `a` is not identical to `b`; otherwise, `false`.</returns>
    public static bool operator !=(AsciiString? a, AsciiString? b)
    {
        return a?.GetHashCode() != b?.GetHashCode();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AsciiString"/> class from an existing <see cref="byte"/> array. The
    /// array will be reused; no copying will take place.
    /// </summary>
    /// <param name="bytes">A pointer to a byte array, validated by the caller to be ASCII safe.</param>
    /// <param name="length">The length of the string, excluding NUL terminator.</param>
    /// <returns>A newly allocated <see cref="AsciiString"/>.</returns>
    private static unsafe AsciiString from(byte* bytes, nuint length)
    {
        return new AsciiString(bytes, length);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AsciiString"/> class.
    /// </summary>
    /// <param name="bytes">A pointer to a byte array, validated by the caller to be ASCII safe.</param>
    /// <param name="length">The length of the string, excluding NUL terminator.</param>
    private unsafe AsciiString(byte* bytes, nuint length)
    {
        this.bytes = bytes;
        this.length = length;

        // We presume the caller to provide us a valid string, but since the absence of this can cause so nefarious
        // bugs, we validate it when running in debug mode.
        Debug.Assert(bytes[length] == 0, $"String is not properly NUL-terminated");
    }

    // TODO: Try to get rid of this constructor, once we have rewritten the scanner to use Perlang strings instead
    // TODO: of .NET strings.

    private unsafe AsciiString(IReadOnlyList<char> chars)
    {
        bytes = (byte*)MemoryAllocator.Allocate((nuint)chars.Count + 1);
        length = (nuint)chars.Count;

        for (int i = 0; i < chars.Count; i++)
        {
            // This cast is safe, presuming the caller has asserted that the
            // string contains only ASCII safe content. We don't want to do it
            // at this point to avoid the cost of doing it multiple times.
            bytes[i] = (byte)chars[i];
        }

        // Add a trailing NUL character, to make it possible to use this byte array with methods expecting
        // NUL-terminated strings.
        bytes[length] = 0;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj is not AsciiString str)
        {
            return false;
        }

        if (str.Length != Length)
        {
            return false;
        }

        unsafe
        {
            // TODO: casting from nuint to uint here means we will not properly support strings larger than 4G on 64-bit
            // TODO: architectures.
            return Libc.Internal.memcmp(str.bytes, bytes, (uint)Length) == 0;
        }
    }

    /// <summary>Gets a numerical hash for this <see cref="AsciiString"/>.</summary>
    /// <returns>A hash code for the current <see cref="AsciiString"/>.</returns>
    public override unsafe int GetHashCode()
    {
        int hashCode = 0;

        for (nuint i = 0; i < length; i++)
        {
            // Inspired by https://stackoverflow.com/a/9009817/227779. This approach should be fine for us, since
            // `bytes` is guaranteed to be immutable.
            hashCode *= 17;
            hashCode += bytes[i];
        }

        return hashCode;
    }

    // TODO: This method is primarily used from tests. Since our tests are currently written in C#, it is convenient to
    // TODO: be able to make assertions based on .NET strings (since string constants in C# will be parsed as such).
    // TODO: Getting rid of this will take _time_, but we should definitely aim for doing it since this is a
    // TODO: performance-heavy, resource-allocation method for no good reason than the ones described above.

    /// <inheritdoc/>
    public override unsafe string ToString()
    {
        return Marshal.PtrToStringUTF8((nint)bytes)!;
    }

    public override unsafe Lang.String to_upper()
    {
        // TODO: This implementation will allocate memory multiple times when racing. However, the MemoryAllocator will
        // TODO: ensure that all "extra" buffers is freed at program exit. This is considered "good enough" for now, but
        // TODO: we should clearly make this side-effect free when racing at some future point. Perhaps even removing
        // TODO: the memoized upperString would be one way to "fix" this.
        if (upperString != null)
        {
            return upperString;
        }

        byte* upperBytes = (byte*)MemoryAllocator.Allocate(Length + 1);

        for (nuint i = 0; i < Length; i++)
        {
            upperBytes[i] = (byte)Libc.Internal.toupper(bytes[i]);
        }

        // Add a trailing NUL character
        upperBytes[Length] = 0;

        upperString = from(upperBytes, Length);
        return upperString;
    }

    // TODO: This could return `byte`, but the problem by doing so is that we will make it impossible to distinguish
    // TODO: between `byte` and `char` values. `print(byte)` would return 102 but `print(char)` would print the literal
    // TODO: `f` character. We should add a `Lang.Char` type to mitigate this. (Or AsciiChar + Utf16Char? A potential
    // TODO: Utf8Char would be 32-bit)
    public unsafe char this[nuint i]
    {
        get
        {
            // Manual implementation of "safe" byte arrays: perform the range checking ourselves. Doing it like this is
            // manual and more risky than letting the well-tested .NET CLR do it for us, *but* it has the significant
            // advantage of letting us keep the `bytes` array as an unmanaged `byte*` pointer. This means zero-cost
            // P/Invoke, which is for now considered more important than letting the .NET runtime handle safety for us.
            if (i < length)
            {
                return (char)bytes[i];
            }
            else
            {
                throw new IndexOutOfRangeException();
            }
        }
    }
}
