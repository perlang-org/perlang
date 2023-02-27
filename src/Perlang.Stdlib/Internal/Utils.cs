using System;
using System.Globalization;
using Perlang.Internal.Extensions;

namespace Perlang.Internal
{
    /// <summary>
    /// Various utility methods.
    /// </summary>
    public static class Utils
    {
        public static string Stringify(object @object)
        {
            if (@object == null)
            {
                return "null";
            }
            else if (@object is IConvertible convertible)
            {
                // The explicit IFormatProvider is required to ensure we use 123.45 format, regardless of
                // host OS language/region settings. See #263 for more details.
                return convertible.ToString(CultureInfo.InvariantCulture);
            }

            return @object.ToString();
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
