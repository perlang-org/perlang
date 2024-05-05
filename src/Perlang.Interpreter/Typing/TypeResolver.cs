#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using Humanizer;
using Perlang.Internal.Extensions;
using Perlang.Interpreter.NameResolution;
using Perlang.Lang;
using Perlang.Parser;

namespace Perlang.Interpreter.Typing
{
    /// <summary>
    /// Class responsible for resolving implicit and explicit type references.
    ///
    /// The class implements the Visitor pattern, using the mechanisms provided by the base class to reduce the
    /// amount of boilerplate code. The tree traversal must be done depth-first, since the resolving for tree nodes
    /// closer to the top are sometimes dependent on child nodes' having their type references already resolved.
    /// </summary>
    internal class TypeResolver : VisitorBase
    {
        private readonly Func<Expr, Binding?> getVariableOrFunctionCallback;
        private readonly Action<TypeValidationError> typeValidationErrorCallback;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeResolver"/> class.
        /// </summary>
        /// <param name="getVariableOrFunctionCallback">A callback used to retrieve a binding for a given
        /// expression.</param>
        /// <param name="typeValidationErrorCallback">A callback which will receive type-validation errors, if they
        /// occur.</param>
        public TypeResolver(Func<Expr, Binding?> getVariableOrFunctionCallback, Action<TypeValidationError> typeValidationErrorCallback)
        {
            this.getVariableOrFunctionCallback = getVariableOrFunctionCallback;
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
            if (!expr.TypeReference.IsResolved && expr.Value.TypeReference.IsResolved)
            {
                expr.TypeReference.ClrType = expr.Value.TypeReference.ClrType;
            }

            return VoidObject.Void;
        }

        public override VoidObject VisitBinaryExpr(Expr.Binary expr)
        {
            // Must traverse the tree first to ensure types get resolved, since the code below relies on it.
            base.VisitBinaryExpr(expr);

            // Determine the type of this expression, to ensure that it can be used in e.g. variable initializers.
            var leftTypeReference = expr.Left.TypeReference;
            var rightTypeReference = expr.Right.TypeReference;

            if (!leftTypeReference.IsResolved || !rightTypeReference.IsResolved)
            {
                // Something has caused implicit and explicit type resolving to fail. We ignore the expression for
                // now since previous or subsequent steps will catch these errors.
                return VoidObject.Void;
            }

            if (leftTypeReference.ClrType!.IsAssignableTo(typeof(NullObject)))
            {
                throw new TypeValidationError(
                    expr.Operator,
                    $"{leftTypeReference} cannot be used with the ${expr.Operator} operator"
                );
            }
            else if (rightTypeReference.ClrType!.IsAssignableTo(typeof(NullObject)))
            {
                throw new TypeValidationError(
                    expr.Operator,
                    $"{leftTypeReference} is cannot be used with the ${expr.Operator} operator"
                );
            }

            // Only a certain set of type combinations are supported with these operators. We validate the operands here
            // to be able to provide "compile-time" error checking rather than runtime checking.

            switch (expr.Operator.Type)
            {
                case TokenType.PLUS:
                case TokenType.MINUS:
                case TokenType.SLASH:
                case TokenType.STAR:
                case TokenType.PERCENT:
                    // String concatenation is only supported for the `+` operator. All supported `string` + type forms
                    // must be listed here.
                    if (expr.Operator.Type == TokenType.PLUS &&
                        (leftTypeReference.ClrType == typeof(Lang.String) &&
                         rightTypeReference.ClrType == typeof(Lang.String)))
                    {
                        // "string" + "string"
                        expr.TypeReference.ClrType = typeof(Lang.String);
                    }
                    else if (expr.Operator.Type == TokenType.PLUS &&
                             (leftTypeReference.ClrType == typeof(Lang.String) &&
                              rightTypeReference.ClrType == typeof(AsciiString))) {
                        // string_variable + "string" literal
                        expr.TypeReference.ClrType = typeof(Lang.String);
                    }
                    else if (expr.Operator.Type == TokenType.PLUS &&
                             (leftTypeReference.ClrType == typeof(Lang.String) &&
                              rightTypeReference.ClrType == typeof(Utf8String))) {
                        // string_variable + "åäö string" literal
                        expr.TypeReference.ClrType = typeof(Lang.String);
                    }
                    else if (expr.Operator.Type == TokenType.PLUS &&
                             (leftTypeReference.ClrType == typeof(Lang.String) &&
                              rightTypeReference.ClrType == typeof(int))) {
                        // "string" + 42
                        expr.TypeReference.ClrType = typeof(Lang.String);
                    }
                    else if (expr.Operator.Type == TokenType.PLUS &&
                             (leftTypeReference.ClrType == typeof(AsciiString) &&
                              rightTypeReference.ClrType == typeof(int))) {
                        // "string" + 42
                        expr.TypeReference.ClrType = typeof(AsciiString);
                    }
                    else if (expr.Operator.Type == TokenType.PLUS &&
                             (leftTypeReference.ClrType == typeof(Utf8String) &&
                              rightTypeReference.ClrType == typeof(int))) {
                        // "åäö string" + 42
                        expr.TypeReference.ClrType = typeof(Utf8String);
                    }
                    else if (expr.Operator.Type == TokenType.PLUS &&
                             (leftTypeReference.ClrType == typeof(int) &&
                              rightTypeReference.ClrType == typeof(Lang.String))) {
                        // 42 + "string"
                        expr.TypeReference.ClrType = typeof(Lang.String);
                    }
                    else if (expr.Operator.Type == TokenType.PLUS &&
                             (leftTypeReference.ClrType == typeof(int) &&
                              rightTypeReference.ClrType == typeof(AsciiString))) {
                        // 42 + "string" + 42
                        expr.TypeReference.ClrType = typeof(AsciiString);
                    }
                    else if (expr.Operator.Type == TokenType.PLUS &&
                             (leftTypeReference.ClrType == typeof(int) &&
                              rightTypeReference.ClrType == typeof(Utf8String))) {
                        // 42 + "åäö string"
                        expr.TypeReference.ClrType = typeof(Utf8String);
                    }
                    else if (expr.Operator.Type == TokenType.PLUS &&
                             (leftTypeReference.ClrType == typeof(Lang.String) &&
                              rightTypeReference.ClrType == typeof(double))) {
                        // "string" + 123.45
                        expr.TypeReference.ClrType = typeof(Lang.String);
                    }
                    else if (expr.Operator.Type == TokenType.PLUS &&
                             (leftTypeReference.ClrType == typeof(AsciiString) &&
                              rightTypeReference.ClrType == typeof(AsciiString))) {
                        // "string" + "string"
                        expr.TypeReference.ClrType = typeof(Utf8String);
                    }
                    else if (expr.Operator.Type == TokenType.PLUS &&
                             (leftTypeReference.ClrType == typeof(Utf8String) &&
                              rightTypeReference.ClrType == typeof(Utf8String))) {
                        // "åäö string" + "åäö string"
                        expr.TypeReference.ClrType = typeof(Utf8String);
                    }
                    else if (expr.Operator.Type == TokenType.PLUS &&
                             (leftTypeReference.ClrType == typeof(AsciiString) &&
                              rightTypeReference.ClrType == typeof(Utf8String))) {
                        // "string" + "åäö string". These must use Utf8String type, since the result will be a UTF-8
                        // string.
                        expr.TypeReference.ClrType = typeof(Utf8String);
                    }
                    else if (expr.Operator.Type == TokenType.PLUS &&
                             (leftTypeReference.ClrType == typeof(Utf8String) &&
                              rightTypeReference.ClrType == typeof(AsciiString))) {
                        // "åäö string" + "string"; likewise
                        expr.TypeReference.ClrType = typeof(Utf8String);
                    }
                    else if (leftTypeReference.ClrType == typeof(int) && rightTypeReference.ClrType == typeof(int))
                    {
                        expr.TypeReference.ClrType = typeof(int);
                    }
                    else if (leftTypeReference.ClrType == typeof(uint) && rightTypeReference.ClrType == typeof(uint))
                    {
                        expr.TypeReference.ClrType = typeof(uint);
                    }
                    else if (leftTypeReference.ClrType == typeof(ulong) && rightTypeReference.ClrType == typeof(ulong))
                    {
                        expr.TypeReference.ClrType = typeof(ulong);
                    }
                    else if (new[] { typeof(uint), typeof(ulong) }.Contains(leftTypeReference.ClrType) &&
                             new[] { typeof(uint), typeof(ulong) }.Contains(rightTypeReference.ClrType))
                    {
                        expr.TypeReference.ClrType = typeof(ulong);
                    }
                    else if (new[] { typeof(int), typeof(long), typeof(uint) }.Contains(leftTypeReference.ClrType) &&
                             new[] { typeof(int), typeof(long), typeof(uint) }.Contains(rightTypeReference.ClrType))
                    {
                        expr.TypeReference.ClrType = typeof(long);
                    }
                    else if (new[] { typeof(int), typeof(long), typeof(uint), typeof(ulong), typeof(float), typeof(double) }.Contains(leftTypeReference.ClrType) &&
                             new[] { typeof(int), typeof(long), typeof(uint), typeof(ulong), typeof(float), typeof(double) }.Contains(rightTypeReference.ClrType) &&
                             (leftTypeReference.ClrType == typeof(double) || rightTypeReference.ClrType == typeof(double)))
                    {
                        // Order is important. This branch must come _before_ the float branch, since `float +
                        // double` and `double + float` is expected to produce a `double`.
                        expr.TypeReference.ClrType = typeof(double);
                    }
                    else if (new[] { typeof(int), typeof(long), typeof(uint), typeof(ulong), typeof(float), typeof(double) }.Contains(leftTypeReference.ClrType) &&
                             new[] { typeof(int), typeof(long), typeof(uint), typeof(ulong), typeof(float), typeof(double) }.Contains(rightTypeReference.ClrType) &&
                             (leftTypeReference.ClrType == typeof(float) || rightTypeReference.ClrType == typeof(float)))
                    {
                        expr.TypeReference.ClrType = typeof(float);
                    }
                    else if (new[] { typeof(int), typeof(long), typeof(uint), typeof(ulong), typeof(BigInteger) }.Contains(leftTypeReference.ClrType) &&
                             new[] { typeof(int), typeof(long), typeof(uint), typeof(ulong), typeof(BigInteger) }.Contains(rightTypeReference.ClrType) &&
                             (leftTypeReference.ClrType == typeof(BigInteger) || rightTypeReference.ClrType == typeof(BigInteger)))
                    {
                        expr.TypeReference.ClrType = typeof(BigInteger);
                    }
                    else
                    {
                        string message = Messages.UnsupportedOperandTypes(expr.Operator.Type, leftTypeReference, rightTypeReference);
                        throw new TypeValidationError(expr.Operator, message);
                    }

                    return VoidObject.Void;

                case TokenType.PLUS_EQUAL:
                case TokenType.MINUS_EQUAL:
                    if (leftTypeReference.ClrType == typeof(int) && rightTypeReference.ClrType == typeof(int))
                    {
                        expr.TypeReference.ClrType = typeof(int);
                    }
                    else if (leftTypeReference.ClrType == typeof(uint) && rightTypeReference.ClrType == typeof(uint))
                    {
                        expr.TypeReference.ClrType = typeof(uint);
                    }
                    else if (new[] { typeof(ulong) }.Contains(leftTypeReference.ClrType) &&
                             new[] { typeof(uint), typeof(ulong) }.Contains(rightTypeReference.ClrType))
                    {
                        expr.TypeReference.ClrType = typeof(ulong);
                    }
                    else if (new[] { typeof(int), typeof(long), typeof(uint) }.Contains(leftTypeReference.ClrType) &&
                             new[] { typeof(int), typeof(long), typeof(uint) }.Contains(rightTypeReference.ClrType))
                    {
                        expr.TypeReference.ClrType = typeof(long);
                    }
                    else if (new[] { typeof(float) }.Contains(leftTypeReference.ClrType) &&
                             new[] { typeof(int), typeof(long), typeof(uint), typeof(ulong), typeof(float) }.Contains(rightTypeReference.ClrType))
                    {
                        // Here it gets interesting: `float` += `double` is legal in Java but *NOT* supported in C#
                        // (Cannot implicitly convert type 'double' to 'float'). We go with the C# semantics for now
                        // since it seems like the safer approach. If/when we need to support this, some form of
                        // explicit casting mechanism would be more suitable.
                        expr.TypeReference.ClrType = typeof(float);
                    }
                    else if (new[] { typeof(double) }.Contains(leftTypeReference.ClrType) &&
                             new[] { typeof(int), typeof(long), typeof(uint), typeof(ulong), typeof(float), typeof(double) }.Contains(rightTypeReference.ClrType))
                    {
                        expr.TypeReference.ClrType = typeof(double);
                    }
                    else if (new[] { typeof(int), typeof(long), typeof(uint), typeof(ulong), typeof(BigInteger) }.Contains(leftTypeReference.ClrType) &&
                             new[] { typeof(int), typeof(long), typeof(uint), typeof(ulong), typeof(BigInteger) }.Contains(rightTypeReference.ClrType) &&
                             (leftTypeReference.ClrType == typeof(BigInteger) || rightTypeReference.ClrType == typeof(BigInteger)))
                    {
                        expr.TypeReference.ClrType = typeof(BigInteger);
                    }
                    else
                    {
                        string message = $"Cannot assign {rightTypeReference.ClrType.ToQuotedTypeKeyword()} to '{leftTypeReference.ClrType.ToTypeKeyword()}' variable";

                        throw new TypeValidationError(expr.Operator, message);
                    }

                    return VoidObject.Void;

                case TokenType.LESS_LESS:
                case TokenType.GREATER_GREATER:

                    if (leftTypeReference.ClrType == typeof(int) && rightTypeReference.ClrType == typeof(int))
                    {
                        expr.TypeReference.ClrType = typeof(int);
                    }
                    else if (leftTypeReference.ClrType == typeof(long) && rightTypeReference.ClrType == typeof(int))
                    {
                        expr.TypeReference.ClrType = typeof(long);
                    }
                    else if (leftTypeReference.ClrType == typeof(uint) && rightTypeReference.ClrType == typeof(int))
                    {
                        expr.TypeReference.ClrType = typeof(uint);
                    }
                    else if (leftTypeReference.ClrType == typeof(ulong) && rightTypeReference.ClrType == typeof(int))
                    {
                        expr.TypeReference.ClrType = typeof(ulong);
                    }
                    else if (leftTypeReference.ClrType == typeof(BigInteger) && rightTypeReference.ClrType == typeof(int))
                    {
                        expr.TypeReference.ClrType = typeof(BigInteger);
                    }
                    else
                    {
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

                    if (new[] { typeof(int), typeof(long), typeof(uint), typeof(ulong), typeof(BigInteger) }.Contains(leftTypeReference.ClrType) &&
                        rightTypeReference.ClrType == typeof(int))
                    {
                        // TODO: Once we have a possibility to do compile-time evaluation of constant expressions, we
                        // TODO: should use it to throw an exception in case `expr.Right` has a negative value (since such
                        // TODO: values will otherwise throw a runtime error - see
                        // TODO: `PerlangInterpreter.VisitBinaryExpr()` for details.
                        expr.TypeReference.ClrType = typeof(BigInteger);
                    }
                    else if (new[] { typeof(int), typeof(long), typeof(uint), typeof(ulong), typeof(float), typeof(double) }.Contains(leftTypeReference.ClrType) &&
                             new[] { typeof(float), typeof(double) }.Contains(rightTypeReference.ClrType))
                    {
                        expr.TypeReference.ClrType = typeof(double);
                    }
                    else if (new[] { typeof(float), typeof(double) }.Contains(leftTypeReference.ClrType) &&
                             new[] { typeof(int), typeof(long), typeof(uint), typeof(ulong), typeof(float), typeof(double) }.Contains(rightTypeReference.ClrType))
                    {
                        expr.TypeReference.ClrType = typeof(double);
                    }
                    else
                    {
                        string message = Messages.UnsupportedOperandTypes(expr.Operator.Type, leftTypeReference, rightTypeReference);
                        throw new TypeValidationError(expr.Operator, message);
                    }

                    return VoidObject.Void;

                case TokenType.GREATER:
                case TokenType.GREATER_EQUAL:
                case TokenType.LESS:
                case TokenType.LESS_EQUAL:
                    if (new[] { typeof(int), typeof(long), typeof(uint) }.Contains(leftTypeReference.ClrType) &&
                        new[] { typeof(int), typeof(long), typeof(uint) }.Contains(rightTypeReference.ClrType))
                    {
                        expr.TypeReference.ClrType = typeof(bool);
                    }
                    else if (new[] { typeof(uint), typeof(ulong) }.Contains(leftTypeReference.ClrType) &&
                             new[] { typeof(uint), typeof(ulong) }.Contains(rightTypeReference.ClrType))
                    {
                        expr.TypeReference.ClrType = typeof(bool);
                    }
                    else if (new[] { typeof(int), typeof(long), typeof(uint), typeof(ulong), typeof(float), typeof(double) }.Contains(leftTypeReference.ClrType) &&
                             new[] { typeof(int), typeof(long), typeof(uint), typeof(ulong), typeof(float), typeof(double) }.Contains(rightTypeReference.ClrType) &&
                             (new[] { typeof(float), typeof(double) }.Contains(leftTypeReference.ClrType) || new[] { typeof(float), typeof(double) }.Contains(rightTypeReference.ClrType)))
                    {
                        // The above check to ensure that either of the operands is `float` or `double` is important
                        // here, since e.g. `ulong` and `int` cannot be compared to each other (because of
                        // not-so-hard-to-understand limitations in the C# language; I'm not even sure this would be
                        // possible to implement with the existing x64 math instructions)
                        expr.TypeReference.ClrType = typeof(bool);
                    }
                    else if (new[] { typeof(int), typeof(long), typeof(uint), typeof(ulong), typeof(BigInteger) }.Contains(leftTypeReference.ClrType) &&
                             new[] { typeof(int), typeof(long), typeof(uint), typeof(ulong), typeof(BigInteger) }.Contains(rightTypeReference.ClrType) &&
                             (leftTypeReference.ClrType == typeof(BigInteger) || rightTypeReference.ClrType == typeof(BigInteger)))
                    {
                        expr.TypeReference.ClrType = typeof(bool);
                    }
                    else
                    {
                        string message = Messages.UnsupportedOperandTypes(expr.Operator.Type, leftTypeReference, rightTypeReference);
                        throw new TypeValidationError(expr.Operator, message);
                    }

                    expr.TypeReference.ClrType = typeof(bool);
                    return VoidObject.Void;

                // Equality and non-equality can always be checked, regardless of the combination of types; they will
                // just return `false` and `true` in case the types are incoercible.
                case TokenType.BANG_EQUAL:
                case TokenType.EQUAL_EQUAL:
                    expr.TypeReference.ClrType = typeof(bool);
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
                if (get.Methods.Any() && get.TypeReference.ClrType != null)
                {
                    // All is fine, we have a type.
                    expr.TypeReference.ClrType = get.TypeReference.ClrType;
                    return VoidObject.Void;
                }
                else if (get.TypeReference.ClrType == null)
                {
                    // This can happen when referencing an invalid method name, like Time.now().tickz()
                    return VoidObject.Void;
                }
            }

            ITypeReference? typeReference = getVariableOrFunctionCallback(expr)?.TypeReference;

            if (typeReference == null)
            {
                throw new TypeValidationError(
                    expr.Paren,
                    $"Internal compiler error: Failed to locate type reference for {expr.CalleeToString}"
                );
            }
            else
            {
                expr.TypeReference.ClrType = typeReference.ClrType;
            }

            return VoidObject.Void;
        }

        public override VoidObject VisitIndexExpr(Expr.Index expr)
        {
            base.VisitIndexExpr(expr);

            Type? type = expr.Indexee.TypeReference.ClrType;
            Type? argumentType = expr.Argument.TypeReference.ClrType;

            if (argumentType == null)
            {
                // TODO: Be ITokenAware and use expr.Argument.Token in this error, for better accuracy
                typeValidationErrorCallback(new TypeValidationError(
                    expr.Token,
                    "Internal compiler error: Type of index argument expected to be resolved at this stage")
                );

                return VoidObject.Void;
            }

            switch (type)
            {
                // TODO: Remove once the migration to AsciiString and Utf8String is complete
                case { } when type == typeof(string):
                    if (!argumentType.IsAssignableTo(typeof(int)))
                    {
                        typeValidationErrorCallback(new TypeValidationError(
                            expr.ClosingBracket,
                            $"'string' cannot be indexed by '{argumentType.ToTypeKeyword()}'")
                        );
                    }

                    expr.TypeReference.ClrType = typeof(char);
                    break;

                case { } when type == typeof(AsciiString):
                    if (!argumentType.IsAssignableTo(typeof(int)))
                    {
                        typeValidationErrorCallback(new TypeValidationError(
                            expr.ClosingBracket,
                            $"'AsciiString' cannot be indexed by '{argumentType.ToTypeKeyword()}'")
                        );
                    }

                    expr.TypeReference.ClrType = typeof(char);
                    break;

                // TODO: arrays
                // TODO: support lists

                case { } when type.IsGenericType && type.IsAssignableTo(typeof(IDictionary)):
                    Type[] genericArguments = type.GetGenericArguments();
                    Type keyType = genericArguments[0];
                    Type valueType = genericArguments[1];

                    if (!argumentType.IsAssignableTo(keyType))
                    {
                        typeValidationErrorCallback(new TypeValidationError(
                            expr.ClosingBracket,
                            $"Dictionary with TKey of type '{keyType}' cannot be indexed by '{argumentType.ToTypeKeyword()}'")
                        );
                    }

                    expr.TypeReference.ClrType = valueType;
                    break;

                case { } when type == typeof(NullObject):
                    typeValidationErrorCallback(new TypeValidationError(
                        expr.ClosingBracket,
                        $"'null' reference cannot be indexed")
                    );
                    break;

                default:
                    typeValidationErrorCallback(new TypeValidationError(
                        expr.ClosingBracket,
                        $"Unable to index object of type '{type.ToTypeKeyword()}': operation not supported")
                    );
                    break;
            }

            return VoidObject.Void;
        }

        public override VoidObject VisitGroupingExpr(Expr.Grouping expr)
        {
            base.VisitGroupingExpr(expr);

            expr.TypeReference.ClrType = expr.Expression.TypeReference.ClrType;

            return VoidObject.Void;
        }

        public override VoidObject VisitLiteralExpr(Expr.Literal expr)
        {
            base.VisitLiteralExpr(expr);

            if (expr.Value == null)
            {
                expr.TypeReference.ClrType = typeof(NullObject);
            }
            else
            {
                if (expr.Value is INumericLiteral numericLiteral)
                {
                    expr.TypeReference.ClrType = numericLiteral.Value.GetType();
                }
                else
                {
                    expr.TypeReference.ClrType = expr.Value.GetType();
                }
            }

            return VoidObject.Void;
        }

        public override VoidObject VisitLogicalExpr(Expr.Logical expr)
        {
            base.VisitLogicalExpr(expr);

            // All logical expressions return a `bool` value. The validation of the operands are handled by the
            // BooleanOperandsValidator class.
            expr.TypeReference.ClrType = typeof(bool);

            return VoidObject.Void;
        }

        public override VoidObject VisitUnaryPrefixExpr(Expr.UnaryPrefix expr)
        {
            base.VisitUnaryPrefixExpr(expr);

            expr.TypeReference.ClrType = expr.Right.TypeReference.ClrType;

            return VoidObject.Void;
        }

        public override VoidObject VisitUnaryPostfixExpr(Expr.UnaryPostfix expr)
        {
            base.VisitUnaryPostfixExpr(expr);

            expr.TypeReference.ClrType = expr.Left.TypeReference.ClrType;

            return VoidObject.Void;
        }

        public override VoidObject VisitIdentifierExpr(Expr.Identifier expr)
        {
            Binding? binding = getVariableOrFunctionCallback(expr);

            if (binding is ClassBinding)
            {
                expr.TypeReference.ClrType = typeof(PerlangClass);
            }
            else
            {
                ITypeReference? typeReference = binding?.TypeReference;

                if (typeReference == null)
                {
                    throw new NameResolutionTypeValidationError(expr.Name, $"Undefined identifier '{expr.Name.Lexeme}'");
                }

                if (typeReference.ExplicitTypeSpecified && !typeReference.IsResolved)
                {
                    ResolveExplicitTypes(typeReference);
                }

                expr.TypeReference.ClrType = typeReference.ClrType;
            }

            return VoidObject.Void;
        }

        public override VoidObject VisitGetExpr(Expr.Get expr)
        {
            base.VisitGetExpr(expr);

            Binding? binding = getVariableOrFunctionCallback(expr.Object);

            // The "== null" part is kind of sneaky. We run into that scenario whenever method calls are chained.
            // It still feels somewhat better than allowing any kind of wild binding to pass through at this
            // point.
            if (binding is ClassBinding or
                VariableBinding or
                NativeClassBinding or
                NativeObjectBinding or
                null)
            {
                Type? type = expr.Object.TypeReference.ClrType;

                if (type == null)
                {
                    // This is a legitimate code path in cases where a method call is attempted on an unknown type, like
                    // `var f: Foo; f.some_method()`. In this case, the ClrType will be null for the given `expr.Object`
                    // type reference.
                    return VoidObject.Void;
                }

                // Perlang uses snake_case by convention, but we still want to be able to call regular PascalCased
                // .NET methods. Converting it like this is not optimal (since it makes debugging harder), but I see
                // no way around this if we want to retain snake_case (which we do).
                //
                // We also make sure to support unconverted method names at this stage, i.e. methods which are named
                // "Some_Method" or "some_Method" on the C# side. This is uncommon but supported, and used as the
                // under-the-hood representation for property getters (a property named "Foo" produces a "get_Foo"
                // method under the hood). Calling property getters like this is surely ugly, but it's much better
                // than not being able to call them at all. If/when we support Foo.bar syntax for calling property
                // getters, we might want to exclude property getters and setters in this code.
                //
                // Tracking issue: https://github.com/perlang-org/perlang/issues/114
                string pascalizedMethodName = expr.Name.Lexeme.Pascalize();

                // TODO: Move this logic to new ReflectionHelper class
                var methods = type.GetMethods()
                    .Where(mi => mi.Name == pascalizedMethodName ||
                                 mi.Name == expr.Name.Lexeme ||
                                 mi.Name == "get_" + pascalizedMethodName) // Quick-hack while waiting for #114
                    .ToImmutableArray();

                if (methods.IsEmpty)
                {
                    typeValidationErrorCallback(new TypeValidationError(
                        expr.Name,
                        $"Failed to locate method '{expr.Name.Lexeme}' in class '{type.Name}'")
                    );

                    return VoidObject.Void;
                }

                expr.Methods = methods;

                // This is actually safe, since return-type-based polymorphism isn't allowed in .NET.
                expr.TypeReference.ClrType = methods.First().ReturnType;
            }
            else
            {
                throw new NotSupportedException($"Unsupported binding type encountered: {binding}");
            }

            return VoidObject.Void;
        }

        //
        // Stmt visitors
        //

        public override VoidObject VisitFunctionStmt(Stmt.Function stmt)
        {
            if (!stmt.ReturnTypeReference.ExplicitTypeSpecified)
            {
                // TODO: Remove once https://github.com/perlang-org/perlang/issues/43 is fully resolved.
                typeValidationErrorCallback(new TypeValidationError(
                    stmt.Name,
                    $"Inferred typing is not yet supported for function '{stmt.Name.Lexeme}'")
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
                    // TODO: Remove once https://github.com/perlang-org/perlang/issues/43 is fully resolved.
                    typeValidationErrorCallback(new TypeValidationError(
                        stmt.Name,
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
                        stmt.Name,
                        $"Internal compiler error: Explicit type reference for '{parameter}' failed to be resolved.")
                    );
                }
            }

            return base.VisitFunctionStmt(stmt);
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
                stmt.HasInitializer)
            {
                // An explicit type has not been provided. Try inferring it from the type of value provided.
                stmt.TypeReference.ClrType = stmt.Initializer.TypeReference.ClrType;
            }

            return VoidObject.Void;
        }

        private static ITypeReference? GreaterType(ITypeReference leftTypeReference, ITypeReference rightTypeReference)
        {
            // TODO: Return the number of bits here or something instead. Also, think about whether we need to special-case
            // TODO: signed and unsigned integers.
            var leftMaxValue = GetApproximateMaxValue(leftTypeReference.ClrType);
            var rightMaxValue = GetApproximateMaxValue(rightTypeReference.ClrType);

            if (leftMaxValue == null || rightMaxValue == null)
            {
                return null;
            }

            if (leftMaxValue > rightMaxValue)
            {
                return leftTypeReference;
            }
            else
            {
                return rightTypeReference;
            }
        }

        /// <summary>
        /// Returns the approximate max value for the given type.
        ///
        /// The "approximate" part is important to understand how to properly use this method. Do _not_ use it in
        /// case you need an exact value. It is only suited for "size comparisons", to determine which one of two
        /// values that uses the "larger" type. For example, Double is larger than Single; Double is also larger
        /// than Decimal (even though the latter has a higher level of precision). Because of this, an example of
        /// types for which this method will return an approximate, inexact return value is Decimal.
        /// </summary>
        /// <param name="type">A Type.</param>
        /// <returns>The approximate max value for the given type.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The given type is not supported by this method.</exception>
        private static double? GetApproximateMaxValue(Type? type)
        {
            var typeCode = Type.GetTypeCode(type);

            switch (typeCode)
            {
                case TypeCode.Empty:
                case TypeCode.DBNull:
                case TypeCode.Char:
                case TypeCode.Boolean:
                case TypeCode.DateTime:
                case TypeCode.String:
                    // The types above are unsuitable for arithmetic operations. We might at some point consider
                    // relaxing this and supporting Ruby-like things, like '*' * 10 to produce '**********'.
                    // However, after having used Ruby for a long time, our conclusion is that such constructs are
                    // rarely used and even though they add a bit of elegance/syntactic sugar to the expressiveness
                    // of the language, the cost in terms of added complexity might not be worth it.
                    return null;

                case TypeCode.Object:
                    if (type == typeof(BigInteger))
                    {
                        // Note: this is insanely wrong. It means that Double, Decimal and BigInteger are currently
                        // treated as "same size". The end result is that expressions like `5.0 ** 31` will return a
                        // `double`, not a `BigInteger`. `10.0 ** 309` will return positive infinity, where `10 ** 309`
                        // will return a large number. I'm not even sure what the "proper" semantics here would be
                        // actually.
                        return Convert.ToDouble(Decimal.MaxValue);
                    }

                    return null;

                case TypeCode.SByte:
                    return SByte.MaxValue;
                case TypeCode.Byte:
                    return Byte.MaxValue;
                case TypeCode.Int16:
                    return Int16.MaxValue;
                case TypeCode.UInt16:
                    return UInt16.MaxValue;
                case TypeCode.Int32:
                    return Int32.MaxValue;
                case TypeCode.UInt32:
                    return UInt32.MaxValue;
                case TypeCode.Int64:
                    return Int64.MaxValue;
                case TypeCode.UInt64:
                    return UInt64.MaxValue;
                case TypeCode.Single:
                    return Single.MaxValue;
                case TypeCode.Double:
                    return Double.MaxValue;
                case TypeCode.Decimal:
                    // Note: this will likely loose some precision.
                    return Convert.ToDouble(Decimal.MaxValue);
                default:
                    throw new ArgumentException($"{typeCode} is not supported");
            }
        }

        private static void ResolveExplicitTypes(ITypeReference typeReference)
        {
            if (typeReference.TypeSpecifier == null)
            {
                // No explicit type was specified. We let the inferred type handling deal with this type
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
                    typeReference.ClrType = typeof(int);
                    break;

                case "long" or "Int64":
                    typeReference.ClrType = typeof(long);
                    break;

                case "bigint" or "BigInteger":
                    typeReference.ClrType = typeof(BigInteger);
                    break;

                case "uint" or "UInt32":
                    typeReference.ClrType = typeof(uint);
                    break;

                case "ulong" or "UInt64":
                    typeReference.ClrType = typeof(ulong);
                    break;

                // "Float" is called "Single" in C#/.NET, but Java uses `float` and `Float`. In this case, I think it
                // makes little sense to make them inconsistent.
                case "float" or "Float":
                    typeReference.ClrType = typeof(float);
                    break;

                case "double" or "Double":
                    typeReference.ClrType = typeof(double);
                    break;

                case "string" or "String":
                    // Perlang String is NOT the same as the .NET String class.
                    typeReference.ClrType = typeof(Lang.String);
                    break;

                case "void":
                    typeReference.ClrType = typeof(void);
                    break;

                // Other types means ClrType will remain `null`; this is then handled elsewhere (in
                // TypesResolvedValidator)
            }
        }
    }
}
