using Perlang.Extensions;

namespace Perlang
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
