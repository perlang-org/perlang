using System;

namespace Perlang.Interpreter.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="Type"/>.
    /// </summary>
    internal static class TypeExtensions
    {
        public static string ToTypeKeyword(this Type type)
        {
            return type switch
            {
                { } when type == typeof(Double) => "double",
                { } when type == typeof(Int32) => "int",
                { } when type == typeof(Int64) => "long",
                { } when type == typeof(NullObject) => "null",
                { } when type == typeof(String) => "string",
                null => "null",
                _ => type.ToString()
            };
        }
    }
}
