#nullable enable
using System;
using System.Text;

namespace Perlang.Stdlib
{
    // TODO: Remove this eventually, preferably when we have a C++ replacement for it. Keeping it for now since it
    // documents the expected existing public API and implementation.

    /// <summary>
    /// Provides support for encoding and decoding base64-encoded content.
    ///
    /// See https://en.wikipedia.org/wiki/Base64 for more details on the base64 encoding.
    /// </summary>
    public static class Base64
    {
        /// <summary>
        /// Encodes the given plain-text string in base64.
        ///
        /// Note that this method inserts line breaks after every 76 characters in the string representation.
        /// </summary>
        /// <param name="plainText">A plain-text string.</param>
        /// <returns>The string representation in base64 of `plainText`.</returns>
        public static string Encode(Lang.String plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText.ToString());

            return Convert.ToBase64String(plainTextBytes, Base64FormattingOptions.InsertLineBreaks);
        }

        /// <summary>
        /// Decodes the given base 64 data.
        ///
        /// Note that this method returns a `string`; it is unsuitable for decoding binary content.
        /// </summary>
        /// <param name="base64Data">A base 64-encoded string.</param>
        /// <returns>The decoded representation of `base64Data`.</returns>
        public static string Decode(Lang.String base64Data)
        {
            // Convert.FromBase64String requires data to be padded. We relax this a bit, like many other programming
            // languages (Ruby, PHP, Javascript) and do not require the padding to be present. It would be even more
            // efficient to be able to tell Convert.FromBase64String() to ignore it, but this will do for now.
            nuint numPaddingCharacters = base64Data.Length % 4;

            // Using a .NET string here is simple, but we should use Perlang strings consistently here in the long run
            // (when we rewrite Base64 to not rely on the .NET BCL)
            string base64String = base64Data.ToString();

            if (numPaddingCharacters != 0)
            {
                base64String += new string('=', (int)(4 - numPaddingCharacters));
            }

            var data = Convert.FromBase64String(base64String);

            // For now, blindly assume that the base64-encoded data is a valid UTF-8/ASCII string and parse it as such.
            // Future improvements here could be to make it possible to return a byte array instead, but for now,
            // returning a string is a much more practically useful approach.
            return Encoding.UTF8.GetString(data);
        }
    }
}
