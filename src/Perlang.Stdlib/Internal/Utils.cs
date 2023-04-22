using System;
using System.Globalization;
using Perlang.Internal.Extensions;
using Perlang.Lang;
using String = Perlang.Lang.String;

namespace Perlang.Internal
{
    /// <summary>
    /// Various utility methods.
    /// </summary>
    public static class Utils
    {
        private static readonly Lang.String NullString = AsciiString.from("null");

        public static Lang.String Stringify(object @object)
        {
            if (@object == null)
            {
                return NullString;
            }
            else if (@object is Lang.String nativeString)
            {
                return nativeString;
            }
            else if (@object is string clrString)
            {
                return String.from(clrString);
            }
            else if (@object is IConvertible convertible)
            {
                // The explicit IFormatProvider is required to ensure we use 123.45 format, regardless of
                // host OS language/region settings. See #263 for more details.
                return String.from(convertible.ToString(CultureInfo.InvariantCulture));
            }

            return String.from(@object.ToString());
        }

        public static string StringifyType(object @object)
        {
            if (@object == null)
            {
                return "null";
            }

            return @object.GetType().ToTypeKeyword();
        }
    }
}
