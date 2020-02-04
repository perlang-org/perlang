namespace Perlang
{
    /// <summary>
    /// Various utility methods
    /// </summary>
    public class Utils
    {
        public static string Stringify(object _object)
        {
            if (_object == null)
            {
                return "nil";
            }

            return _object.ToString();
        }

        public static string StringifyType(object _object)
        {
            if (_object == null)
            {
                return "nil";
            }

            return _object.GetType().Name;
        }

    }
}
