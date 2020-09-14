using System;
using System.Text;

namespace Perlang.Stdlib
{
    [GlobalClass]
    public static class Base64
    {
        public static string Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);

            return Convert.ToBase64String(plainTextBytes, Base64FormattingOptions.InsertLineBreaks);
        }

        public static string Decode(string base64Data)
        {
            var data = Convert.FromBase64String(base64Data);

            // For now, blindly assume that the base64-encoded data is a valid UTF-8/ASCII string and parse it as such.
            // Future improvements here could be to make it possible to return a byte array instead, but for now,
            // returning a string is a much more practically useful approach.
            return Encoding.UTF8.GetString(data);
        }
    }
}
