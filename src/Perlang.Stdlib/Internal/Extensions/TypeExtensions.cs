using System;
using System.Numerics;
using Perlang.Lang;

namespace Perlang.Internal.Extensions
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
                { } when type == typeof(Int32) => "int",
                { } when type == typeof(Int64) => "long",
                { } when type == typeof(BigInteger) => "bigint",
                { } when type == typeof(UInt32) => "uint",
                { } when type == typeof(UInt64) => "ulong",
                { } when type == typeof(Single) => "float",
                { } when type == typeof(Double) => "double",
                { } when type == typeof(NullObject) => "null",
                { } when type == typeof(Lang.String) => "string",
                { } when type == typeof(AsciiString) => "AsciiString",

                // TODO: add bool here
                { } when type.IsAssignableTo(typeof(IPerlangFunction)) => "function",
                null => "null",
                _ => type.ToString()
            };
        }
    }
}
