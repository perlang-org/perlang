using System;
using System.Globalization;
using Perlang.Internal.Extensions;
using Perlang.Lang;
using String = Perlang.Lang.String;

namespace Perlang.Internal;

/// <summary>
/// Various utility methods.
/// </summary>
public static class Utils
{
    private static readonly String NullString = AsciiString.from("null");

    public static String Stringify(object @object)
    {
        if (@object == null)
        {
            return NullString;
        }
        else if (@object is String nativeString)
        {
            return nativeString;
        }
        else if (@object is string clrString)
        {
            return String.from(clrString);
        }
        else if (@object is float f)
        {
            // Us an explicit format, so we can produce something equal in the C# and C++/fmt-based implementations
            return String.from(f.ToString("G7", CultureInfo.InvariantCulture));
        }
        else if (@object is double d)
        {
            // Us an explicit format, so we can produce something equal in the C# and C++/fmt-based implementations
            return String.from(d.ToString("G15", CultureInfo.InvariantCulture));
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