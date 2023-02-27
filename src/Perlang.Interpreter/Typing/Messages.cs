#nullable enable
using Perlang.Internal.Extensions;
using Perlang.Interpreter.Extensions;

namespace Perlang.Interpreter.Typing;

internal static class Messages
{
    public static string UnsupportedOperandTypes(TokenType operatorType, ITypeReference leftTypeReference, ITypeReference rightTypeReference) =>
        $"Unsupported {operatorType.ToSourceString()} operand types: " +
        $"'{leftTypeReference.ClrType.ToTypeKeyword()}' and '{rightTypeReference.ClrType.ToTypeKeyword()}'";
}
