using System;
using System.Numerics;

namespace Perlang.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="Type"/>.
    /// </summary>
    public static class TypeExtensions
    {
        public static string ToTypeKeyword(this Type type)
        {
            return type switch
            {
                { } when type == typeof(Double) => "double",
                { } when type == typeof(Int32) => "int",
                { } when type == typeof(Int64) => "long",
                { } when type == typeof(BigInteger) => "bigint",
                { } when type == typeof(NullObject) => "null",
                { } when type == typeof(String) => "string",
                { } when type.IsAssignableTo(typeof(IPerlangFunction)) => "function",
                null => "null",
                _ => type.ToString()
            };
        }
    }
}
