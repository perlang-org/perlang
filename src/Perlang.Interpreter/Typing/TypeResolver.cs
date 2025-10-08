#nullable enable
#pragma warning disable S1871
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Perlang.Compiler;
using Perlang.Interpreter.Internals;
using Perlang.Interpreter.NameResolution;
using Perlang.Parser;

namespace Perlang.Interpreter.Typing;

/// <summary>
/// Class responsible for resolving implicit and explicit type references.
///
/// The class implements the Visitor pattern, using the mechanisms provided by the base class to reduce the
/// amount of boilerplate code. The tree traversal must be done depth-first, since the resolving for tree nodes
/// closer to the top are sometimes dependent on child nodes' having their type references already resolved.
/// </summary>
internal class TypeResolver : VisitorBase
{
    private readonly IBindingRetriever bindingHandler;
    private readonly ITypeHandler typeHandler;
    private readonly Action<TypeValidationError> typeValidationErrorCallback;

    /// <summary>
    /// Initializes a new instance of the <see cref="TypeResolver"/> class.
    /// </summary>
    /// <param name="bindingHandler">A handler used for retrieving a binding for a given expression.</param>
    /// <param name="typeHandler">A handler used for adding and retrieving global, top-level types.</param>
    /// <param name="typeValidationErrorCallback">A callback which will receive type-validation errors, if they
    ///     occur.</param>
    public TypeResolver(IBindingRetriever bindingHandler, ITypeHandler typeHandler, Action<TypeValidationError> typeValidationErrorCallback)
    {
        this.bindingHandler = bindingHandler;
        this.typeHandler = typeHandler;
        this.typeValidationErrorCallback = typeValidationErrorCallback;
    }

    public void Resolve(IList<Stmt> statements)
    {
        Visit(statements);
    }

    //
    // Expr visitors
    //

    public override VoidObject VisitAssignExpr(Expr.Assign expr)
    {
        base.VisitAssignExpr(expr);

        // Letting the type be inferred in an assignment expression is important to make constructs like
        // "var i = 100; var j = i+= 2;" work correctly. This is indeed an odd way of writing code, but as long
        // as += is an expression and not a statement, we need to have predictable semantics for cases like
        // this.
        if (!expr.TypeReference.IsResolved) {
            expr.TypeReference.SetCppType(expr.Value.TypeReference.CppType);
        }

        expr.TypeReference.SetPerlangType(expr.Value.TypeReference.PerlangType);

        return VoidObject.Void;
    }

    public override VoidObject VisitBinaryExpr(Expr.Binary expr)
    {
        // Must traverse the tree first to ensure types get resolved, since the code below relies on it.
        base.VisitBinaryExpr(expr);

        // Determine the type of this expression, to ensure that it can be used in e.g. variable initializers.
        var leftTypeReference = expr.Left.TypeReference;
        var rightTypeReference = expr.Right.TypeReference;

        if (!leftTypeReference.IsResolved || !rightTypeReference.IsResolved) {
            // Something has caused implicit and explicit type resolving to fail. We ignore the expression for
            // now since previous or subsequent steps will catch these errors.
            return VoidObject.Void;
        }

        if (leftTypeReference.CppType!.IsNullObject) {
            throw new TypeValidationError(
                expr.Operator,
                $"{leftTypeReference} cannot be used with the ${expr.Operator} operator"
            );
        }
        else if (rightTypeReference.CppType!.IsNullObject) {
            throw new TypeValidationError(
                expr.Operator,
                $"{leftTypeReference} is cannot be used with the ${expr.Operator} operator"
            );
        }

        // Only a certain set of type combinations are supported with these operators. We validate the operands here
        // to be able to provide good error messages, rather than leaving it to clang to catch these errors.

        switch (expr.Operator.Type) {
            case TokenType.PLUS:
            case TokenType.MINUS:
            case TokenType.SLASH:
            case TokenType.STAR:
            case TokenType.PERCENT:
                // String concatenation is only supported for the `+` operator. All supported `string` + type forms
                // must be listed here.
                if (expr.Operator.Type == TokenType.PLUS &&
                    (leftTypeReference.CppType == PerlangTypes.String &&
                     rightTypeReference.CppType == PerlangTypes.String)) {
                    // "string" + "string"
                    expr.TypeReference.SetCppType(PerlangTypes.String);
                }
                else if (expr.Operator.Type == TokenType.PLUS &&
                         (leftTypeReference.CppType == PerlangTypes.String &&
                          rightTypeReference.CppType == PerlangTypes.AsciiString)) {
                    // string_variable + "string" literal
                    expr.TypeReference.SetCppType(PerlangTypes.String);
                }
                else if (expr.Operator.Type == TokenType.PLUS &&
                         (leftTypeReference.CppType == PerlangTypes.String &&
                          rightTypeReference.CppType == PerlangTypes.UTF8String)) {
                    // string_variable + "åäö string" literal
                    expr.TypeReference.SetCppType(PerlangTypes.String);
                }
                else if (expr.Operator.Type == TokenType.PLUS &&
                         (leftTypeReference.CppType == PerlangTypes.AsciiString &&
                          rightTypeReference.CppType == PerlangTypes.String)) {
                    // "string" literal + string_variable
                    expr.TypeReference.SetCppType(PerlangTypes.String);
                }
                else if (expr.Operator.Type == TokenType.PLUS &&
                         (leftTypeReference.CppType == PerlangTypes.UTF8String &&
                          rightTypeReference.CppType == PerlangTypes.String)) {
                    // "åäö string" literal + string_variable
                    expr.TypeReference.SetCppType(PerlangTypes.String);
                }
                else if (expr.Operator.Type == TokenType.PLUS &&
                         (leftTypeReference.CppType == PerlangTypes.String &&
                          new[] { PerlangValueTypes.Int32, PerlangValueTypes.Int64, PerlangValueTypes.UInt32, PerlangValueTypes.UInt64, PerlangValueTypes.BigInt, PerlangValueTypes.Float, PerlangValueTypes.Double }.Contains(rightTypeReference.CppType))) {
                    // "string" + 42
                    expr.TypeReference.SetCppType(PerlangTypes.String);
                }
                else if (expr.Operator.Type == TokenType.PLUS &&
                         (leftTypeReference.CppType == PerlangTypes.AsciiString &&
                          new[] { PerlangValueTypes.Int32, PerlangValueTypes.Int64, PerlangValueTypes.UInt32, PerlangValueTypes.UInt64, PerlangValueTypes.BigInt, PerlangValueTypes.Float, PerlangValueTypes.Double }.Contains(rightTypeReference.CppType))) {
                    // "string" + 42
                    expr.TypeReference.SetCppType(PerlangTypes.AsciiString);
                }
                else if (expr.Operator.Type == TokenType.PLUS &&
                         (leftTypeReference.CppType == PerlangTypes.UTF8String &&
                          new[] { PerlangValueTypes.Int32, PerlangValueTypes.Int64, PerlangValueTypes.UInt32, PerlangValueTypes.UInt64, PerlangValueTypes.BigInt, PerlangValueTypes.Float, PerlangValueTypes.Double }.Contains(rightTypeReference.CppType))) {
                    // "åäö string" + 42
                    expr.TypeReference.SetCppType(PerlangTypes.UTF8String);
                }
                else if (expr.Operator.Type == TokenType.PLUS &&
                         (new[] { PerlangValueTypes.Int32, PerlangValueTypes.Int64, PerlangValueTypes.UInt32, PerlangValueTypes.UInt64, PerlangValueTypes.BigInt, PerlangValueTypes.Float, PerlangValueTypes.Double }.Contains(leftTypeReference.CppType) &&
                          rightTypeReference.CppType == PerlangTypes.String)) {
                    // 42 + "string"
                    expr.TypeReference.SetCppType(PerlangTypes.String);
                }
                else if (expr.Operator.Type == TokenType.PLUS &&
                         (new[] { PerlangValueTypes.Int32, PerlangValueTypes.Int64, PerlangValueTypes.UInt32, PerlangValueTypes.UInt64, PerlangValueTypes.BigInt, PerlangValueTypes.Float, PerlangValueTypes.Double }.Contains(leftTypeReference.CppType) &&
                          rightTypeReference.CppType == PerlangTypes.AsciiString)) {
                    // 42 + "string" + 42
                    expr.TypeReference.SetCppType(PerlangTypes.AsciiString);
                }
                else if (expr.Operator.Type == TokenType.PLUS &&
                         (new[] { PerlangValueTypes.Int32, PerlangValueTypes.Int64, PerlangValueTypes.UInt32, PerlangValueTypes.UInt64, PerlangValueTypes.BigInt, PerlangValueTypes.Float, PerlangValueTypes.Double }.Contains(leftTypeReference.CppType) &&
                          rightTypeReference.CppType == PerlangTypes.UTF8String)) {
                    // 42 + "åäö string"
                    expr.TypeReference.SetCppType(PerlangTypes.UTF8String);
                }
                else if (expr.Operator.Type == TokenType.PLUS &&
                         (leftTypeReference.CppType == PerlangTypes.String &&
                          rightTypeReference.CppType == PerlangValueTypes.Double)) {
                    // "string" + 123.45
                    expr.TypeReference.SetCppType(PerlangTypes.String);
                }
                else if (expr.Operator.Type == TokenType.PLUS &&
                         (leftTypeReference.CppType == PerlangTypes.AsciiString &&
                          rightTypeReference.CppType == PerlangTypes.AsciiString)) {
                    // "string" + "string"
                    expr.TypeReference.SetCppType(PerlangTypes.AsciiString);
                }
                else if (expr.Operator.Type == TokenType.PLUS &&
                         (leftTypeReference.CppType == PerlangTypes.UTF8String &&
                          rightTypeReference.CppType == PerlangTypes.UTF8String)) {
                    // "åäö string" + "åäö string"
                    expr.TypeReference.SetCppType(PerlangTypes.UTF8String);
                }
                else if (expr.Operator.Type == TokenType.PLUS &&
                         (leftTypeReference.CppType == PerlangTypes.AsciiString &&
                          rightTypeReference.CppType == PerlangTypes.UTF8String)) {
                    // "string" + "åäö string". These must use Utf8String type, since the result will be a UTF-8
                    // string.
                    expr.TypeReference.SetCppType(PerlangTypes.UTF8String);
                }
                else if (expr.Operator.Type == TokenType.PLUS &&
                         (leftTypeReference.CppType == PerlangTypes.UTF8String &&
                          rightTypeReference.CppType == PerlangTypes.AsciiString)) {
                    // "åäö string" + "string"; likewise
                    expr.TypeReference.SetCppType(PerlangTypes.UTF8String);
                }
                else if (leftTypeReference.CppType == PerlangValueTypes.Int32 && rightTypeReference.CppType == PerlangValueTypes.Int32) {
                    expr.TypeReference.SetCppType(PerlangValueTypes.Int32);
                }
                else if (leftTypeReference.CppType == PerlangValueTypes.UInt32 && rightTypeReference.CppType == PerlangValueTypes.UInt32) {
                    expr.TypeReference.SetCppType(PerlangValueTypes.UInt32);
                }
                else if (leftTypeReference.CppType == PerlangValueTypes.Int64 && rightTypeReference.CppType == PerlangValueTypes.Int64) {
                    expr.TypeReference.SetCppType(PerlangValueTypes.Int64);
                }
                else if (new[] { PerlangValueTypes.UInt32, PerlangValueTypes.UInt64 }.Contains(leftTypeReference.CppType) &&
                         new[] { PerlangValueTypes.UInt32, PerlangValueTypes.UInt64 }.Contains(rightTypeReference.CppType)) {
                    expr.TypeReference.SetCppType(PerlangValueTypes.UInt64);
                }
                else if (new[] { PerlangValueTypes.Int32, PerlangValueTypes.Int64, PerlangValueTypes.UInt32 }.Contains(leftTypeReference.CppType) &&
                         new[] { PerlangValueTypes.Int32, PerlangValueTypes.Int64, PerlangValueTypes.UInt32 }.Contains(rightTypeReference.CppType)) {
                    expr.TypeReference.SetCppType(PerlangValueTypes.Int64);
                }
                else if (new[] { PerlangValueTypes.Int32, PerlangValueTypes.Int64, PerlangValueTypes.UInt32, PerlangValueTypes.UInt64, PerlangValueTypes.Float, PerlangValueTypes.Double }.Contains(leftTypeReference.CppType) &&
                         new[] { PerlangValueTypes.Int32, PerlangValueTypes.Int64, PerlangValueTypes.UInt32, PerlangValueTypes.UInt64, PerlangValueTypes.Float, PerlangValueTypes.Double }.Contains(rightTypeReference.CppType) &&
                         (leftTypeReference.CppType == PerlangValueTypes.Double || rightTypeReference.CppType == PerlangValueTypes.Double)) {
                    // Order is important. This branch must come _before_ the float branch, since `float +
                    // double` and `double + float` is expected to produce a `double`.
                    expr.TypeReference.SetCppType(PerlangValueTypes.Double);
                }
                else if (new[] { PerlangValueTypes.Int32, PerlangValueTypes.Int64, PerlangValueTypes.UInt32, PerlangValueTypes.UInt64, PerlangValueTypes.Float, PerlangValueTypes.Double }.Contains(leftTypeReference.CppType) &&
                         new[] { PerlangValueTypes.Int32, PerlangValueTypes.Int64, PerlangValueTypes.UInt32, PerlangValueTypes.UInt64, PerlangValueTypes.Float, PerlangValueTypes.Double }.Contains(rightTypeReference.CppType) &&
                         (leftTypeReference.CppType == PerlangValueTypes.Float || rightTypeReference.CppType == PerlangValueTypes.Float)) {
                    expr.TypeReference.SetCppType(PerlangValueTypes.Float);
                }
                else if (new[] { PerlangValueTypes.Int32, PerlangValueTypes.Int64, PerlangValueTypes.UInt32, PerlangValueTypes.UInt64, PerlangValueTypes.BigInt }.Contains(leftTypeReference.CppType) &&
                         new[] { PerlangValueTypes.Int32, PerlangValueTypes.Int64, PerlangValueTypes.UInt32, PerlangValueTypes.UInt64, PerlangValueTypes.BigInt }.Contains(rightTypeReference.CppType) &&
                         (leftTypeReference.CppType == PerlangValueTypes.BigInt || rightTypeReference.CppType == PerlangValueTypes.BigInt)) {
                    expr.TypeReference.SetCppType(PerlangValueTypes.BigInt);
                }
                else {
                    string message = Messages.UnsupportedOperandTypes(expr.Operator.Type, leftTypeReference, rightTypeReference);
                    throw new TypeValidationError(expr.Operator, message);
                }

                return VoidObject.Void;

            case TokenType.PLUS_EQUAL:
            case TokenType.MINUS_EQUAL:
                if (leftTypeReference.CppType == PerlangValueTypes.Int32 && rightTypeReference.CppType == PerlangValueTypes.Int32) {
                    expr.TypeReference.SetCppType(PerlangValueTypes.Int32);
                }
                else if (leftTypeReference.CppType == PerlangValueTypes.UInt32 && rightTypeReference.CppType == PerlangValueTypes.UInt32) {
                    expr.TypeReference.SetCppType(PerlangValueTypes.UInt32);
                }
                else if (new[] { PerlangValueTypes.UInt64 }.Contains(leftTypeReference.CppType) &&
                         new[] { PerlangValueTypes.UInt32, PerlangValueTypes.UInt64 }.Contains(rightTypeReference.CppType)) {
                    expr.TypeReference.SetCppType(PerlangValueTypes.UInt64);
                }
                else if (new[] { PerlangValueTypes.Int32, PerlangValueTypes.Int64, PerlangValueTypes.UInt32 }.Contains(leftTypeReference.CppType) &&
                         new[] { PerlangValueTypes.Int32, PerlangValueTypes.Int64, PerlangValueTypes.UInt32 }.Contains(rightTypeReference.CppType)) {
                    expr.TypeReference.SetCppType(PerlangValueTypes.Int64);
                }
                else if (new[] { PerlangValueTypes.Float }.Contains(leftTypeReference.CppType) &&
                         new[] { PerlangValueTypes.Int32, PerlangValueTypes.Int64, PerlangValueTypes.UInt32, PerlangValueTypes.UInt64, PerlangValueTypes.Float }.Contains(rightTypeReference.CppType)) {
                    // Here it gets interesting: `float` += `double` is legal in Java but *NOT* supported in C#
                    // (Cannot implicitly convert type 'double' to 'float'). We go with the C# semantics for now
                    // since it seems like the safer approach. If/when we need to support this, some form of
                    // explicit casting mechanism would be more suitable.
                    expr.TypeReference.SetCppType(PerlangValueTypes.Float);
                }
                else if (new[] { PerlangValueTypes.Double }.Contains(leftTypeReference.CppType) &&
                         new[] { PerlangValueTypes.Int32, PerlangValueTypes.Int64, PerlangValueTypes.UInt32, PerlangValueTypes.UInt64, PerlangValueTypes.Float, PerlangValueTypes.Double }.Contains(rightTypeReference.CppType)) {
                    expr.TypeReference.SetCppType(PerlangValueTypes.Double);
                }
                else if (new[] { PerlangValueTypes.Int32, PerlangValueTypes.Int64, PerlangValueTypes.UInt32, PerlangValueTypes.UInt64, PerlangValueTypes.BigInt }.Contains(leftTypeReference.CppType) &&
                         new[] { PerlangValueTypes.Int32, PerlangValueTypes.Int64, PerlangValueTypes.UInt32, PerlangValueTypes.UInt64, PerlangValueTypes.BigInt }.Contains(rightTypeReference.CppType) &&
                         (leftTypeReference.CppType == PerlangValueTypes.BigInt || rightTypeReference.CppType == PerlangValueTypes.BigInt)) {
                    expr.TypeReference.SetCppType(PerlangValueTypes.BigInt);
                }
                else {
                    string message = $"Cannot assign {rightTypeReference.ToQuotedTypeKeyword()} to '{leftTypeReference.TypeKeywordOrPerlangType}' variable";

                    throw new TypeValidationError(expr.Operator, message);
                }

                return VoidObject.Void;

            case TokenType.LESS_LESS:
            case TokenType.GREATER_GREATER:

                if (leftTypeReference.CppType == PerlangValueTypes.Int32 && rightTypeReference.CppType == PerlangValueTypes.Int32) {
                    expr.TypeReference.SetCppType(PerlangValueTypes.Int32);
                }
                else if (leftTypeReference.CppType == PerlangValueTypes.Int64 && rightTypeReference.CppType == PerlangValueTypes.Int32) {
                    expr.TypeReference.SetCppType(PerlangValueTypes.Int64);
                }
                else if (leftTypeReference.CppType == PerlangValueTypes.UInt32 && rightTypeReference.CppType == PerlangValueTypes.Int32) {
                    expr.TypeReference.SetCppType(PerlangValueTypes.UInt32);
                }
                else if (leftTypeReference.CppType == PerlangValueTypes.UInt64 && rightTypeReference.CppType == PerlangValueTypes.Int32) {
                    expr.TypeReference.SetCppType(PerlangValueTypes.UInt64);
                }
                else if (leftTypeReference.CppType == PerlangValueTypes.BigInt && rightTypeReference.CppType == PerlangValueTypes.Int32) {
                    expr.TypeReference.SetCppType(PerlangValueTypes.BigInt);
                }
                else {
                    string message = Messages.UnsupportedOperandTypes(expr.Operator.Type, leftTypeReference, rightTypeReference);
                    throw new TypeValidationError(expr.Operator, message);
                }

                return VoidObject.Void;

            case TokenType.STAR_STAR:
                // TODO: If expr.Left is an Expr.Identifier, it might refer to a variable (= supported left-hand
                // TODO: operand), a function call (also supported) _or_ a function/method reference. This is _not_
                // TODO: supported at this stage. I think we should somehow work on making it possible to
                // TODO: distinguish between `foo` and `foo()` - perhaps the inferred type of the former should be
                // TODO: something like `PerlangFunction` instead of the return type of the function? This is
                // TODO: probably a significant change so be prepared that it will take some time.

                if (new[] { PerlangValueTypes.Int32, PerlangValueTypes.Int64, PerlangValueTypes.UInt32, PerlangValueTypes.UInt64, PerlangValueTypes.BigInt }.Contains(leftTypeReference.CppType) &&
                    rightTypeReference.CppType == PerlangValueTypes.Int32) {
                    // TODO: Once we have a possibility to do compile-time evaluation of constant expressions, we
                    // TODO: should use it to throw an exception in case `expr.Right` has a negative value (since such
                    // TODO: values will otherwise throw a runtime error - see
                    // TODO: `PerlangInterpreter.VisitBinaryExpr()` for details.
                    expr.TypeReference.SetCppType(PerlangValueTypes.BigInt);
                }
                else if (new[] { PerlangValueTypes.Int32, PerlangValueTypes.Int64, PerlangValueTypes.UInt32, PerlangValueTypes.UInt64, PerlangValueTypes.Float, PerlangValueTypes.Double }.Contains(leftTypeReference.CppType) &&
                         new[] { PerlangValueTypes.Float, PerlangValueTypes.Double }.Contains(rightTypeReference.CppType)) {
                    expr.TypeReference.SetCppType(PerlangValueTypes.Double);
                }
                else if (new[] { PerlangValueTypes.Float, PerlangValueTypes.Double }.Contains(leftTypeReference.CppType) &&
                         new[] { PerlangValueTypes.Int32, PerlangValueTypes.Int64, PerlangValueTypes.UInt32, PerlangValueTypes.UInt64, PerlangValueTypes.Float, PerlangValueTypes.Double }.Contains(rightTypeReference.CppType)) {
                    expr.TypeReference.SetCppType(PerlangValueTypes.Double);
                }
                else {
                    string message = Messages.UnsupportedOperandTypes(expr.Operator.Type, leftTypeReference, rightTypeReference);
                    throw new TypeValidationError(expr.Operator, message);
                }

                return VoidObject.Void;

            case TokenType.GREATER:
            case TokenType.GREATER_EQUAL:
            case TokenType.LESS:
            case TokenType.LESS_EQUAL:
                if (new[] { PerlangValueTypes.Int32, PerlangValueTypes.Int64, PerlangValueTypes.UInt32 }.Contains(leftTypeReference.CppType) &&
                    new[] { PerlangValueTypes.Int32, PerlangValueTypes.Int64, PerlangValueTypes.UInt32 }.Contains(rightTypeReference.CppType)) {
                    expr.TypeReference.SetCppType(PerlangValueTypes.Bool);
                }
                else if (new[] { PerlangValueTypes.UInt32, PerlangValueTypes.UInt64 }.Contains(leftTypeReference.CppType) &&
                         new[] { PerlangValueTypes.UInt32, PerlangValueTypes.UInt64 }.Contains(rightTypeReference.CppType)) {
                    expr.TypeReference.SetCppType(PerlangValueTypes.Bool);
                }
                else if (new[] { PerlangValueTypes.Int32, PerlangValueTypes.Int64, PerlangValueTypes.UInt32, PerlangValueTypes.UInt64, PerlangValueTypes.Float, PerlangValueTypes.Double }.Contains(leftTypeReference.CppType) &&
                         new[] { PerlangValueTypes.Int32, PerlangValueTypes.Int64, PerlangValueTypes.UInt32, PerlangValueTypes.UInt64, PerlangValueTypes.Float, PerlangValueTypes.Double }.Contains(rightTypeReference.CppType) &&
                         (new[] { PerlangValueTypes.Float, PerlangValueTypes.Double }.Contains(leftTypeReference.CppType) || new[] { PerlangValueTypes.Float, PerlangValueTypes.Double }.Contains(rightTypeReference.CppType))) {
                    // The above check to ensure that either of the operands is `float` or `double` is important
                    // here, since e.g. `ulong` and `int` cannot be compared to each other (because of
                    // not-so-hard-to-understand limitations in the C# language; I'm not even sure this would be
                    // possible to implement with the existing x64 math instructions)
                    expr.TypeReference.SetCppType(PerlangValueTypes.Bool);
                }
                else if (new[] { PerlangValueTypes.Int32, PerlangValueTypes.Int64, PerlangValueTypes.UInt32, PerlangValueTypes.UInt64, PerlangValueTypes.BigInt }.Contains(leftTypeReference.CppType) &&
                         new[] { PerlangValueTypes.Int32, PerlangValueTypes.Int64, PerlangValueTypes.UInt32, PerlangValueTypes.UInt64, PerlangValueTypes.BigInt }.Contains(rightTypeReference.CppType) &&
                         (leftTypeReference.CppType == PerlangValueTypes.BigInt || rightTypeReference.CppType == PerlangValueTypes.BigInt)) {
                    expr.TypeReference.SetCppType(PerlangValueTypes.Bool);
                }
                else {
                    string message = Messages.UnsupportedOperandTypes(expr.Operator.Type, leftTypeReference, rightTypeReference);
                    throw new TypeValidationError(expr.Operator, message);
                }

                expr.TypeReference.SetCppType(PerlangValueTypes.Bool);
                return VoidObject.Void;

            // Equality and non-equality can always be checked, regardless of the combination of types; they will
            // just return `false` and `true` in case the types are incoercible.
            case TokenType.BANG_EQUAL:
            case TokenType.EQUAL_EQUAL:
                expr.TypeReference.SetCppType(PerlangValueTypes.Bool);
                return VoidObject.Void;

            default:
                throw new TypeValidationError(
                    expr.Operator,
                    $"Internal compiler error: {expr.Operator.Type} not valid for binary expressions"
                );
        }
    }

    public override VoidObject VisitCallExpr(Expr.Call expr)
    {
        try
        {
            Visit(expr.Callee);
        }
        catch (NameResolutionTypeValidationError)
        {
            if (expr.Callee is Expr.Identifier identifier)
            {
                throw new NameResolutionTypeValidationError(identifier.Name, $"Attempting to call undefined function '{identifier.Name.Lexeme}'");
            }
            else
            {
                throw;
            }
        }

        foreach (Expr argument in expr.Arguments)
        {
            Visit(argument);
        }

        if (expr.Callee is Expr.Get get)
        {
            if (get.ClrMethods.Any() && get.TypeReference.IsResolved)
            {
                // All is fine, we have a type.
                expr.TypeReference.SetCppType(get.TypeReference.CppType);
                expr.TypeReference.SetPerlangType(get.TypeReference.PerlangType);
                return VoidObject.Void;
            }
            else if (get.PerlangMethods.Any() && get.TypeReference.IsResolved)
            {
                // All is fine, we have a type.
                expr.TypeReference.SetCppType(get.TypeReference.CppType);
                expr.TypeReference.SetPerlangType(get.TypeReference.PerlangType);
                return VoidObject.Void;
            }
            else if (!get.TypeReference.IsResolved)
            {
                // This can happen when referencing an invalid method name, like Time.now().tickz()
                return VoidObject.Void;
            }
            else
            {
                throw new TypeValidationError(
                    expr.Paren,
                    $"Internal compiler error: No methods found for {expr.CalleeToString}"
                );
            }
        }
        else if (expr.Callee is Expr.Identifier)
        {
            ITypeReference? typeReference = bindingHandler.GetVariableOrFunctionBinding(expr)?.TypeReference;

            if (typeReference == null)
            {
                throw new TypeValidationError(
                    expr.Paren,
                    $"Internal compiler error: Failed to locate type reference for {expr.CalleeToString}"
                );
            }
            else
            {
                expr.TypeReference.SetCppType(typeReference.CppType);
                expr.TypeReference.SetPerlangType(typeReference.PerlangType);
            }
        }
        else
        {
            throw new TypeValidationError(
                expr.Paren,
                $"Internal compiler error: Unsupported callee expression type encountered: {expr.CalleeToString}"
            );
        }

        return VoidObject.Void;
    }

    public override VoidObject VisitIndexExpr(Expr.Index expr)
    {
        base.VisitIndexExpr(expr);

        CppType? cppType = expr.Indexee.TypeReference.CppType;
        IPerlangType? perlangType = expr.Indexee.TypeReference.PerlangType;
        CppType? argumentType = expr.Argument.TypeReference.CppType;

        if (cppType == null)
        {
            // This could be an issue, but OTOH this can be legal if an unresolved type is used and the error has
            // already been reported. If we had a logging framework in place, this would make sense to log at
            // 'debug' or 'trace' log level. Silencing for now.
            //
            //typeValidationErrorCallback(new TypeValidationError(
            //    (expr.Indexee as ITokenAware)?.Token ?? expr.Token,
            //    "Internal compiler error: Type of indexed object expected to be resolved at this stage")
            //);

            return VoidObject.Void;
        }

        if (argumentType == null)
        {
            //typeValidationErrorCallback(new TypeValidationError(
            //    (expr.Argument as ITokenAware)?.Token ?? expr.Token,
            //    "Internal compiler error: Type of index argument expected to be resolved at this stage")
            //);

            return VoidObject.Void;
        }

        switch (cppType)
        {
            // TODO: This code path is still used when indexing e.g. Libc.environ() (which is a Dictionary<string, string>)
            case { } when cppType == PerlangTypes.StringArray:
                if (!argumentType.IsAssignableTo(PerlangValueTypes.Int32))
                {
                    typeValidationErrorCallback(new TypeValidationError(
                        expr.ClosingBracket,
                        $"'string' cannot be indexed by '{expr.Argument.TypeReference.TypeKeywordOrPerlangType}'")
                    );
                }

                expr.TypeReference.SetCppType(PerlangValueTypes.Char);
                break;

            case { } when cppType == PerlangTypes.AsciiString || cppType == PerlangTypes.UTF16String:
                if (!argumentType.IsAssignableTo(PerlangValueTypes.Int32))
                {
                    typeValidationErrorCallback(new TypeValidationError(
                        expr.ClosingBracket,
                        $"'{expr.Indexee.TypeReference.TypeKeywordOrPerlangType}' cannot be indexed by '{expr.Argument.TypeReference.TypeKeywordOrPerlangType}'")
                    );
                }

                expr.TypeReference.SetCppType(PerlangValueTypes.Char);
                break;

            case { } when cppType.IsArray:
                CppType elementType = cppType.GetElementType()!;

                if (!argumentType.IsAssignableTo(PerlangValueTypes.Int32))
                {
                    typeValidationErrorCallback(new TypeValidationError(
                        expr.ClosingBracket,
                        $"Array of type '{elementType}' cannot be indexed by '{expr.Argument.TypeReference.TypeKeywordOrPerlangType}'")
                    );
                }

                expr.TypeReference.SetCppType(elementType);
                expr.TypeReference.SetPerlangType(perlangType);
                break;

            case { } when cppType == PerlangTypes.NullObject:
                typeValidationErrorCallback(new TypeValidationError(
                    expr.ClosingBracket,
                    "'null' reference cannot be indexed")
                );
                break;

            default:
                typeValidationErrorCallback(new TypeValidationError(
                    expr.ClosingBracket,
                    $"Unable to index object of type '{expr.Indexee.TypeReference.TypeKeywordOrPerlangType}': operation not supported")
                );
                break;
        }

        return VoidObject.Void;
    }

    public override VoidObject VisitGroupingExpr(Expr.Grouping expr)
    {
        base.VisitGroupingExpr(expr);

        expr.TypeReference.SetCppType(expr.Expression.TypeReference.CppType);

        return VoidObject.Void;
    }

    public override VoidObject VisitCollectionInitializerExpr(Expr.CollectionInitializer expr)
    {
        base.VisitCollectionInitializerExpr(expr);

        // In the future, the idea is to loosen this restriction and figure out the most specific base type instead,
        // and use that as the inferred type of the collection initializer.
        if (expr.Elements.Select(e => e.TypeReference.CppType).Distinct().Count() > 1)
        {
            // TODO: Fails because CppType objects which are equal do not compare as equal.
            typeValidationErrorCallback(new TypeValidationError(
                expr.Token,
                "All elements in a collection initializer must have the same type")
            );

            return VoidObject.Void;
        }

        // Infer the type of the collection initializer from the first element, since we have now validated that all
        // elements have the same type.
        ITypeReference firstElementTypeReference = expr.Elements.First().TypeReference;
        CppType elementCppType = firstElementTypeReference.CppType!;
        CppType collectionType = elementCppType.MakeArrayType();
        expr.TypeReference.SetCppType(collectionType);
        expr.TypeReference.SetPerlangType(firstElementTypeReference.PerlangType);

        return VoidObject.Void;
    }

    public override VoidObject VisitLiteralExpr(Expr.Literal expr)
    {
        base.VisitLiteralExpr(expr);

        if (expr.Value == null)
        {
            expr.TypeReference.SetCppType(PerlangTypes.NullObject);
        }
        else
        {
            if (expr.Value is INumericLiteral numericLiteral)
            {
                expr.TypeReference.SetCppTypeFromClrType(numericLiteral.Value.GetType());
            }
            else
            {
                expr.TypeReference.SetCppTypeFromClrType(expr.Value.GetType());
            }
        }

        return VoidObject.Void;
    }

    public override VoidObject VisitLogicalExpr(Expr.Logical expr)
    {
        base.VisitLogicalExpr(expr);

        // All logical expressions return a `bool` value. The validation of the operands are handled by the
        // BooleanOperandsValidator class.
        expr.TypeReference.SetCppType(PerlangValueTypes.Bool);

        return VoidObject.Void;
    }

    public override VoidObject VisitUnaryPrefixExpr(Expr.UnaryPrefix expr)
    {
        base.VisitUnaryPrefixExpr(expr);

        expr.TypeReference.SetCppType(expr.Right.TypeReference.CppType);

        return VoidObject.Void;
    }

    public override VoidObject VisitUnaryPostfixExpr(Expr.UnaryPostfix expr)
    {
        base.VisitUnaryPostfixExpr(expr);

        expr.TypeReference.SetCppType(expr.Left.TypeReference.CppType);

        return VoidObject.Void;
    }

    public override VoidObject VisitIdentifierExpr(Expr.Identifier expr)
    {
        Binding? binding = bindingHandler.GetVariableOrFunctionBinding(expr);

        if (binding is ClassBinding)
        {
            //expr.TypeReference.SetClrType(typeof(IPerlangClass));

            // TODO: Figure out a good way to handle this in our new world
            throw new NotImplementedException();
        }
        else if (binding == null)
        {
            throw new NameResolutionTypeValidationError(expr.Name, $"Undefined identifier '{expr.Name.Lexeme}'");
        }
        else
        {
            ITypeReference typeReference = binding.TypeReference ?? throw new PerlangCompilerException($"Internal compiler error: Type reference unexpectedly null for binding for '{expr.Name.Lexeme}'");

            if (typeReference.ExplicitTypeSpecified && !typeReference.IsResolved)
            {
                ResolveExplicitTypes(typeReference);
            }

            expr.TypeReference.SetCppType(typeReference.CppType);
            expr.TypeReference.SetPerlangType(typeReference.PerlangType);
        }

        return VoidObject.Void;
    }

    public override VoidObject VisitGetExpr(Expr.Get expr)
    {
        base.VisitGetExpr(expr);

        Binding? binding = bindingHandler.GetVariableOrFunctionBinding(expr.Object);

        // The "== null" part is kind of sneaky. We run into that scenario whenever method calls are chained.
        // It still feels somewhat better than allowing any kind of wild binding to pass through at this
        // point.
        if (binding is ClassBinding or
            FieldBinding or
            VariableBinding or
            NativeClassBinding or
            null)
        {
            if (binding is NativeClassBinding) {
                throw new NotImplementedInCompiledModeException("Bindings to native .NET classes are no longer supported");
            }

            IPerlangType? perlangType = expr.Object.TypeReference.PerlangType ?? expr.Object.TypeReference.CppType;

            if (perlangType == null)
            {
                // This is a legitimate code path in cases where a method call is attempted on an unknown type, like
                // in the test var_of_non_existent_type_with_initializer_emits_expected_error. In this case, the
                // ClrType will be null for the given `expr.Object` type reference.
                return VoidObject.Void;
            }

            var field = perlangType.Fields
                .SingleOrDefault(f => f.Name == expr.Name.Lexeme);

            if (field != null) {
                // Duplicating this logic here since for forward references (methods referring to fields definer
                // later in the class). I wouldn't recommend writing the code like that, but it's still something
                // that we don't want to cause the compilation to fail for.
                if (!field.TypeReference.IsResolved)
                {
                    ResolveExplicitTypes(field.TypeReference);
                }

                expr.TypeReference.SetCppType(field.TypeReference.CppType ?? throw new PerlangCompilerException($"Internal compiler error: C++ type was null for field '{expr.Name.Lexeme}' in class '{perlangType.Name}'"));

                // This being null is a valid case, since we might not be returning a Perlang-defined type.
                expr.TypeReference.SetPerlangType(field.TypeReference.PerlangType);

                return VoidObject.Void;
            }

            var methods = perlangType.Methods
                .Where(m => m.Name == expr.Name.Lexeme)
                .ToImmutableArray();

            if (methods.IsEmpty) {
                typeValidationErrorCallback(new TypeValidationError(
                    expr.Name,
                    $"Failed to locate symbol '{expr.Name.Lexeme}' in class {perlangType.Name}")
                );

                return VoidObject.Void;
            }

            expr.PerlangMethods = methods;

            // We assume that return-type-based polymorphism isn't allowed in Perlang either. If this ever
            // changes, the code below will have to be revisited.
            var firstMatchingMethod = methods.First();

            // Duplicating this logic here since for forward references (methods defined later in the class),
            // types resolving will not have taken place at this point.
            if (!firstMatchingMethod.ReturnTypeReference.IsResolved)
            {
                ResolveExplicitTypes(firstMatchingMethod.ReturnTypeReference);
            }

            expr.TypeReference.SetCppType(firstMatchingMethod.ReturnTypeReference.CppType ?? throw new PerlangCompilerException($"Internal compiler error: C++ type was null for return type of method '{expr.Name.Lexeme}' in class '{perlangType.Name}'"));

            // This being null is a valid case, since we might not be returning a Perlang-defined type.
            expr.TypeReference.SetPerlangType(firstMatchingMethod.ReturnTypeReference.PerlangType);
        }
        else if (binding is EnumBinding enumBinding)
        {
            PerlangEnum perlangEnum = enumBinding.PerlangEnum;

            if (!perlangEnum.EnumMembers.ContainsKey(expr.Name.Lexeme))
            {
                typeValidationErrorCallback(new TypeValidationError(
                    expr.Name,
                    $"Enum member '{expr.Name.Lexeme}' not found in enum '{perlangEnum.Name}'")
                );

                return VoidObject.Void;
            }

            expr.TypeReference.SetCppType(binding.TypeReference!.CppType);
        }
        else
        {
            throw new NotSupportedException($"Unsupported binding type encountered: {binding}");
        }

        return VoidObject.Void;
    }

    public override VoidObject VisitNewExpression(Expr.NewExpression expr)
    {
        base.VisitNewExpression(expr);

        var binding = bindingHandler.GetVariableOrFunctionBinding(expr);
        var classBinding = binding as ClassBinding;

        if (expr.IsArray) {
            switch (expr.TypeName.Lexeme) {
                case "int":
                    expr.TypeReference.SetCppType(PerlangTypes.Int32Array);
                    break;
                default:
                    // TODO: Would be cleaner to check if the type in question is assignable to perlang::Object. For
                    // now, this check will be good enough since it will match all custom classes.
                    if (classBinding != null) {
                        var elementType = new CppType(classBinding.PerlangClass!.Name, classBinding.PerlangClass!.Name, wrapInSharedPtr: true);
                        expr.TypeReference.SetCppType(new CppType("perlang::ObjectArray", classBinding.PerlangClass!.Name, wrapInSharedPtr: true, isArray: true, elementType: elementType));
                    }
                    else if (binding != null) {
                        typeValidationErrorCallback(
                            new TypeValidationError(
                                expr.Token,
                                $"Internal compiler error: Dynamic arrays of type '{expr.TypeName.Lexeme}' are currently not supported")
                        );
                    }
                    else {
                        typeValidationErrorCallback(new TypeValidationError(
                            expr.Token,
                            $"Type '{expr.Token.Lexeme}' could not be found")
                        );
                    }

                    break;
            }

            return VoidObject.Void;
        }

        if (binding == null) {
            typeValidationErrorCallback(new TypeValidationError(
                expr.Token,
                $"Type '{expr.Token.Lexeme}' could not be found")
            );

            return VoidObject.Void;
        }

        if (classBinding == null) {
            typeValidationErrorCallback(new TypeValidationError(
                expr.Token,
                $"Internal compiler error: Unexpected type '{binding}' encountered; expected ClassBinding")
            );

            return VoidObject.Void;
        }

        expr.TypeReference.SetCppType(new CppType(classBinding.PerlangClass!.Name, classBinding.PerlangClass!.Name, wrapInSharedPtr: true));
        expr.TypeReference.SetPerlangType(classBinding.PerlangClass);

        return VoidObject.Void;
    }

    //
    // Stmt visitors
    //

    public override VoidObject VisitFunctionStmt(Stmt.Function stmt)
    {
        if (!stmt.IsConstructor && !stmt.IsDestructor && !stmt.ReturnTypeReference.ExplicitTypeSpecified)
        {
            // TODO: Remove once https://gitlab.perlang.org/perlang/perlang/-/issues/43 is fully resolved.
            typeValidationErrorCallback(new TypeValidationError(
                stmt.NameToken,
                $"Inferred typing of return type is not yet supported (attempted to be used in function '{stmt.NameToken.Lexeme}'")
            );
        }

        if (!stmt.ReturnTypeReference.IsResolved)
        {
            ResolveExplicitTypes(stmt.ReturnTypeReference);
        }

        foreach (Parameter parameter in stmt.Parameters)
        {
            if (!parameter.TypeReference.ExplicitTypeSpecified)
            {
                // TODO: Remove once https://gitlab.perlang.org/perlang/perlang/-/issues/43 is fully resolved.
                typeValidationErrorCallback(new TypeValidationError(
                    stmt.NameToken,
                    $"Inferred typing is not yet supported for function parameter '{parameter}'")
                );
            }

            if (!parameter.TypeReference.IsResolved)
            {
                ResolveExplicitTypes(parameter.TypeReference);
            }

            if (!parameter.TypeReference.IsResolved)
            {
                typeValidationErrorCallback(new TypeValidationError(
                    stmt.NameToken,
                    $"Internal compiler error: Explicit type reference for '{parameter}' failed to be resolved.")
                );
            }
        }

        return base.VisitFunctionStmt(stmt);
    }

    public override VoidObject VisitFieldStmt(Stmt.Field stmt)
    {
        if (!stmt.TypeReference.IsResolved)
        {
            ResolveExplicitTypes(stmt.TypeReference);
        }

        return base.VisitFieldStmt(stmt);
    }

    public override VoidObject VisitVarStmt(Stmt.Var stmt)
    {
        base.VisitVarStmt(stmt);

        if (!stmt.TypeReference.IsResolved)
        {
            ResolveExplicitTypes(stmt.TypeReference);
        }

        if (!stmt.TypeReference.IsResolved &&
            !stmt.TypeReference.ExplicitTypeSpecified &&
            stmt.Initializer != null)
        {
            // An explicit type has not been provided. Try inferring it from the type of value provided.
            stmt.TypeReference.SetCppType(stmt.Initializer.TypeReference.CppType);
            stmt.TypeReference.SetPerlangType(stmt.Initializer.TypeReference.PerlangType);
        }

        return VoidObject.Void;
    }

    private void ResolveExplicitTypes(ITypeReference typeReference)
    {
        if (typeReference.TypeSpecifier == null)
        {
            // No explicit type was specified. We let the inferred type handling deal with this type.
            return;
        }

        // Initial phase: Resolve a limited set of built-in types. Only short type names are supported; fully
        // qualified type names will have to come at a later stage.
        string lexeme = typeReference.TypeSpecifier.Lexeme;

        switch (lexeme)
        {
            // TODO: Replace these with a dictionary of type names in the currently imported namespaces or similar.
            // TODO: This is not really a scalable approach. :)
            //
            // Note that adding more supported types here also means the list of reserved identifiers in
            // `Scanner.ReservedKeywordStrings` should be updated. (Adding unit tests for the new
            // types in ReservedKeywordsTests is a good way to ensure this is not forgotten.)
            case "int" or "Int32":
                typeReference.SetCppType(typeReference.IsArray ? PerlangTypes.Int32Array : PerlangValueTypes.Int32);
                break;

            case "long" or "Int64":
                typeReference.SetCppType(typeReference.IsArray ? PerlangTypes.Int64Array : PerlangValueTypes.Int64);
                break;

            case "bigint" or "BigInteger":
                typeReference.SetCppType(typeReference.IsArray ? PerlangTypes.BigIntArray : PerlangValueTypes.BigInt);
                break;

            case "uint" or "UInt32":
                typeReference.SetCppType(typeReference.IsArray ? PerlangTypes.UInt32Array : PerlangValueTypes.UInt32);
                break;

            case "ulong" or "UInt64":
                typeReference.SetCppType(typeReference.IsArray ? PerlangTypes.UInt64Array : PerlangValueTypes.UInt64);
                break;

            // "Float" is called "Single" in C#/.NET, but Java uses `float` and `Float`. In this case, I think it
            // makes little sense to make them inconsistent.
            case "float" or "Float":
                typeReference.SetCppType(typeReference.IsArray ? PerlangTypes.FloatArray : PerlangValueTypes.Float);
                break;

            case "double" or "Double":
                typeReference.SetCppType(typeReference.IsArray ? PerlangTypes.DoubleArray : PerlangValueTypes.Double);
                break;

            case "bool" or "Bool":
                typeReference.SetCppType(typeReference.IsArray ? PerlangTypes.BoolArray : PerlangValueTypes.Bool);
                break;

            case "char" or "Char":
                typeReference.SetCppType(typeReference.IsArray ? PerlangTypes.CharArray : PerlangValueTypes.Char);
                break;

            case "string" or "String":
                typeReference.SetCppType(typeReference.IsArray ? PerlangTypes.StringArray : PerlangTypes.String);
                break;

            case "ASCIIString":
                typeReference.SetCppType(typeReference.IsArray ? PerlangTypes.ASCIIStringArray : PerlangTypes.AsciiString);
                break;

            case "UTF8String":
                typeReference.SetCppType(typeReference.IsArray ? PerlangTypes.UTF8StringArray : PerlangTypes.UTF8String);
                break;

            case "UTF16String":
                typeReference.SetCppType(typeReference.IsArray ? PerlangTypes.UTF16StringArray : PerlangTypes.UTF16String);
                break;

            case "object":
                typeReference.SetCppType(typeReference.IsArray ? PerlangTypes.ObjectArray : PerlangTypes.PerlangObject);
                break;

            case "void":
                if (typeReference.IsArray) {
                    typeValidationErrorCallback(new TypeValidationError(
                        typeReference.TypeSpecifier,
                        "void arrays are not supported")
                    );
                }

                typeReference.SetCppType(PerlangValueTypes.Void);
                break;

            case "perlang::Type" or "Type":
                typeReference.SetCppType(PerlangTypes.Type);
                break;

            default:
                // This can be a user-defined type. We check using the ITypeHandler instance we have. If that method
                // returns null, this means CppType will remain `null`, indicating that the type remains unresolved.
                // This is an error condition which is then handled elsewhere (in TypesResolvedValidator)
                IPerlangType? perlangType = typeHandler.GetType(lexeme);

                if (perlangType != null) {
                    if (typeReference.IsArray) {
                        // TODO: Creating a new CppType for the array here might be a bit weird, but we don't have any
                        // TODO: good way around it given that we want a CppType with the ElementType set to our
                        // TODO: concrete (derived, non-perlang::Object) type. We solve this by using name-based
                        // TODO: comparisons elsewhere, to do special handling of ObjectArray in Get expressions.
                        var elementType = new CppType(perlangType.Name, perlangType.Name, wrapInSharedPtr: true);
                        typeReference.SetCppType(new CppType("perlang::ObjectArray", perlangType.Name, wrapInSharedPtr: true, isArray: true, elementType: elementType));
                        typeReference.SetPerlangType(perlangType);
                    }
                    else if (perlangType.IsEnum) {
                        // Enums are not wrapped in std::shared_ptr, but the isEnum parameter is important to set here
                        // for the compiler to be able to generate the correct code for enum member accesses.

                        // The C++ type name is a bit over-complicated for these because we use a hack inspired by
                        // https://stackoverflow.com/a/46294875/227779 for the underlying C++ representation for enums
                        // at the moment.
                        string cppTypeName = $"{perlangType.Name}::{perlangType.Name}";

                        typeReference.SetCppType(new CppType(cppTypeName, perlangType.Name, isEnum: true));
                        typeReference.SetPerlangType(perlangType);
                    }
                    else {
                        // Note: this means that the CppType instances for a given type will not be shared. Can this
                        // become a problem? Perhaps we would need some form of mechanism for deducing a CppType for a
                        // given IPerlangType, which could then potentially cache the instantiated value as needed.
                        typeReference.SetCppType(new CppType(perlangType.Name, perlangType.Name, wrapInSharedPtr: true));
                        typeReference.SetPerlangType(perlangType);
                    }
                }

                break;
        }
    }
}
