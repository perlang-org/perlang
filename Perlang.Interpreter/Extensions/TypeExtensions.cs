using System;

namespace Perlang.Interpreter.Extensions
{
    internal static class TypeExtensions
    {
        public static bool IsAssignableTo(this Type derivedType, Type baseType)
        {
            return baseType.IsAssignableFrom(derivedType);
        }
    }
}
