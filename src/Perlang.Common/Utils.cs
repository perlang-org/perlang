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
                return "nil";
            }

            return @object.ToString();
        }

        public static string StringifyType(object @object)
        {
            if (@object == null)
            {
                return "nil";
            }

            return @object.GetType().Name;
        }
    }
}
