using Perlang.Interpreter.Extensions;

namespace Perlang.Interpreter.Compiler;

public static class CompilerMessages
{
    public static string UnsupportedOperatorTypeInBinaryExpression(TokenType operatorType) =>
        $"Internal error: Unsupported operator {operatorType.ToSourceString()} in binary expression.";

    public static string UnsupportedOperandsInBinaryExpression(TokenType operatorType, ITypeReference leftTypeReference, ITypeReference rightTypeReference) =>
        $"Internal error: Unsupported combination of operands to {operatorType.ToSourceString()} operator: {leftTypeReference.TypeKeywordOrPerlangType} and {rightTypeReference.TypeKeywordOrPerlangType}.";

    public static string UnsupportedOperatorTypeInLogicalExpression(TokenType operatorType) =>
        $"Internal error: Unsupported operator {operatorType.ToSourceString()} in logical expression.";

    public static string UnsupportedOperatorTypeInUnaryPrefixExpression(TokenType operatorType) =>
        $"Internal error: Unsupported operator {operatorType.ToSourceString()} in unary prefix expression.";

    public static string UnsupportedOperatorTypeInUnaryPostfixExpression(TokenType operatorType) =>
        $"Internal error: Unsupported operator {operatorType.ToSourceString()} in unary postfix expression.";
}
