using Perlang.Interpreter.Extensions;
using static Perlang.Internal.Utils;

namespace Perlang.Interpreter;

public static class InterpreterMessages
{
    public static string UnsupportedOperandTypes(TokenType operatorType, object left, object right) =>
        $"Unexpected runtime error: Unsupported {operatorType.ToSourceString()} operands specified: {StringifyType(left)} and {StringifyType(right)}";

    public static string UnsupportedOperatorTypeInBinaryExpression(TokenType operatorType) =>
        $"Internal error: Unsupported operator {operatorType.ToSourceString()} in binary expression.";
}
