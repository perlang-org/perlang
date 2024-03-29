#nullable enable
#pragma warning disable SA1513
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using Microsoft.CSharp.RuntimeBinder;
using Perlang.Attributes;
using Perlang.Exceptions;
using Perlang.Internal.Extensions;
using Perlang.Interpreter.CodeAnalysis;
using Perlang.Interpreter.Immutability;
using Perlang.Interpreter.Internals;
using Perlang.Interpreter.NameResolution;
using Perlang.Interpreter.Typing;
using Perlang.Lang;
using Perlang.Parser;
using Perlang.Stdlib;
using static Perlang.Internal.Utils;
using static Perlang.TokenType;
using String = System.String;

namespace Perlang.Interpreter
{
    /// <summary>
    /// Interpreter for Perlang code.
    ///
    /// Instances of this class are not thread safe; calling <see cref="Eval"/> on multiple threads simultaneously can
    /// lead to race conditions and is not supported.
    /// </summary>
    public class PerlangInterpreter : IInterpreter, Expr.IVisitor<object?>, Stmt.IVisitor<VoidObject>
    {
        private readonly Action<RuntimeError> runtimeErrorHandler;
        private readonly PerlangEnvironment globals = new();
        private readonly IImmutableDictionary<string, Type> superGlobals;

        internal IBindingHandler BindingHandler { get; }

        /// <summary>
        /// A collection of all currently defined global classes (both native/.NET and classes defined in Perlang code.)
        /// </summary>
        private readonly IDictionary<string, object> globalClasses = new Dictionary<string, object>();

        private readonly ImmutableDictionary<string, Type> nativeClasses;
        private readonly Action<Lang.String> standardOutputHandler;
        private readonly bool replMode;

        private ImmutableList<Stmt> previousStatements = ImmutableList.Create<Stmt>();
        private IEnvironment currentEnvironment;

        /// <summary>
        /// Initializes a new instance of the <see cref="PerlangInterpreter"/> class.
        /// </summary>
        /// <param name="runtimeErrorHandler">A callback that will be called on runtime errors. Note that after calling
        ///     this handler, the interpreter will abort the script.</param>
        /// <param name="standardOutputHandler">An callback that will receive output printed to standard output.</param>
        /// <param name="bindingHandler">A binding handler, or `null` to let the interpreter create a new instance.</param>
        /// <param name="arguments">An optional list of runtime arguments.</param>
        /// <param name="replMode">A flag indicating whether REPL mode will be active or not. In REPL mode, statements
        ///     without semicolons are accepted.</param>
        public PerlangInterpreter(
            Action<RuntimeError> runtimeErrorHandler,
            Action<Lang.String> standardOutputHandler,
            IBindingHandler? bindingHandler = null,
            IEnumerable<string>? arguments = null,
            bool replMode = false)
        {
            this.runtimeErrorHandler = runtimeErrorHandler;
            this.standardOutputHandler = standardOutputHandler;
            this.replMode = replMode;
            this.BindingHandler = bindingHandler ?? new BindingHandler();

            var argumentsList = (arguments ?? Array.Empty<string>()).ToImmutableList();

            currentEnvironment = globals;

            superGlobals = CreateSuperGlobals(argumentsList);

            LoadStdlib();
            nativeClasses = RegisterGlobalFunctionsAndClasses();
        }

        private IImmutableDictionary<string, Type> CreateSuperGlobals(ImmutableList<string> argumentsList)
        {
            // Set up the super-global ARGV variable.
            var result = new Dictionary<string, Type>
            {
                { "ARGV", typeof(Argv) }
            }.ToImmutableDictionary();

            // TODO: Returning a value AND modifying the globals like this feels like a code smell. Try to figure out
            // TODO: a more sensible way.
            globals.Define(new Token(VAR, "ARGV", null, -1), new Argv(argumentsList));

            return result;
        }

        private static void LoadStdlib()
        {
            // Because of implicit dependencies, this is not loaded automatically; we must manually load this
            // assembly to ensure all Callables within it are registered in the global namespace.
            Assembly.Load("Perlang.StdLib");
        }

        private ImmutableDictionary<string, Type> RegisterGlobalFunctionsAndClasses()
        {
            RegisterGlobalClasses();

            // We need to make a copy of this at this early stage, when it _only_ contains native classes, so that
            // we can feed it to the Resolver class.
            return globalClasses.ToImmutableDictionary(kvp => kvp.Key, kvp => (Type)kvp.Value);
        }

        /// <summary>
        /// Registers global classes defined in native .NET code.
        /// </summary>
        /// <exception cref="PerlangInterpreterException">Multiple classes with the same name was encountered.</exception>
        private void RegisterGlobalClasses()
        {
            var globalClassesQueryable = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Select(t => new
                {
                    Type = t,
                    ClassAttribute = t.GetCustomAttribute<GlobalClassAttribute>()
                })
                .Where(t => t.ClassAttribute != null && (
                    !t.ClassAttribute.Platforms.Any() || t.ClassAttribute.Platforms.Contains(Environment.OSVersion.Platform)
                ));

            foreach (var globalClass in globalClassesQueryable)
            {
                string name = globalClass.ClassAttribute!.Name ?? globalClass.Type.Name;

                if (globals.Get(name) != null)
                {
                    throw new PerlangInterpreterException(
                        $"Attempted to define global class '{name}', but another identifier with the same name already exists"
                    );
                }

                globalClasses[name] = globalClass.Type;
            }
        }

        /// <summary>
        /// Runs the provided source code, in an `eval()`/REPL fashion.
        ///
        /// If provided an expression, returns the result; otherwise, null.
        /// </summary>
        /// <remarks>
        /// Note that variables, methods and classes defined in an invocation to this method will persist to subsequent
        /// invocations. This might seem inconvenient at times, but it makes it possible to implement the Perlang
        /// REPL in a reasonable way.
        /// </remarks>
        /// <param name="source">The source code to a Perlang program (typically a single line of Perlang code).</param>
        /// <param name="scanErrorHandler">A handler for scanner errors.</param>
        /// <param name="parseErrorHandler">A handler for parse errors.</param>
        /// <param name="nameResolutionErrorHandler">A handler for resolve errors.</param>
        /// <param name="typeValidationErrorHandler">A handler for type validation errors.</param>
        /// <param name="immutabilityValidationErrorHandler">A handler for immutability validation errors.</param>
        /// <param name="compilerWarningHandler">A handler for compiler warnings.</param>
        /// <returns>If the provided source is an expression, the value of the expression (which can be `null`) is
        ///     returned. If a runtime error occurs, <see cref="VoidObject.Void"/> is returned. In all other cases, `null`
        ///     is returned.</returns>
        public object? Eval(
            string source,
            ScanErrorHandler scanErrorHandler,
            ParseErrorHandler parseErrorHandler,
            NameResolutionErrorHandler nameResolutionErrorHandler,
            ValidationErrorHandler typeValidationErrorHandler,
            ValidationErrorHandler immutabilityValidationErrorHandler,
            CompilerWarningHandler compilerWarningHandler)
        {
            if (String.IsNullOrWhiteSpace(source))
            {
                return null;
            }

            ScanAndParseResult result = PerlangParser.ScanAndParse(
                source,
                scanErrorHandler,
                parseErrorHandler,
                replMode
            );

            if (result == ScanAndParseResult.ScanErrorOccurred ||
                result == ScanAndParseResult.ParseErrorEncountered)
            {
                // These errors have already been propagated to the caller; we can simply return a this point.
                return null;
            }

            if (result.HasStatements)
            {
                var previousAndNewStatements = previousStatements.Concat(result.Statements!).ToImmutableList();

                //
                // Resolving names phase
                //

                bool hasNameResolutionErrors = false;

                var nameResolver = new NameResolver(
                    nativeClasses,
                    superGlobals,
                    BindingHandler,
                    AddGlobalClass,
                    nameResolutionError =>
                    {
                        hasNameResolutionErrors = true;
                        nameResolutionErrorHandler(nameResolutionError);
                    }
                );

                nameResolver.Resolve(previousAndNewStatements);

                if (hasNameResolutionErrors)
                {
                    // Resolution errors has been reported back to the provided error handler. Nothing more remains
                    // to be done than aborting the evaluation.
                    return null;
                }

                //
                // Type validation
                //

                bool typeValidationFailed = false;

                TypeValidator.Validate(
                    previousAndNewStatements,
                    typeValidationError =>
                    {
                        typeValidationFailed = true;
                        typeValidationErrorHandler(typeValidationError);
                    },
                    BindingHandler.GetVariableOrFunctionBinding,
                    compilerWarning =>
                    {
                        bool result = compilerWarningHandler(compilerWarning);

                        if (result)
                        {
                            typeValidationFailed = true;
                        }
                    }
                );

                if (typeValidationFailed)
                {
                    return null;
                }

                //
                // Immutability validation
                //

                bool immutabilityValidationFailed = false;

                ImmutabilityValidator.Validate(
                    previousAndNewStatements,
                    immutabilityValidationError =>
                    {
                        immutabilityValidationFailed = true;
                        immutabilityValidationErrorHandler(immutabilityValidationError);
                    },
                    BindingHandler.GetVariableOrFunctionBinding
                );

                if (immutabilityValidationFailed)
                {
                    return null;
                }

                //
                // "Code analysis" validation
                //

                bool codeAnalysisValidationFailed = false;

                CodeAnalysisValidator.Validate(
                    previousAndNewStatements,
                    compilerWarning =>
                    {
                        bool result = compilerWarningHandler(compilerWarning);

                        if (result)
                        {
                            codeAnalysisValidationFailed = true;
                        }
                    }
                );

                if (codeAnalysisValidationFailed)
                {
                    return null;
                }

                // All validation was successful => add these statements to the list of "previous statements". Recording
                // them like this is necessary to be able to declare a variable in one REPL line and refer to it in
                // another.
                previousStatements = previousAndNewStatements.ToImmutableList();

                try
                {
                    Interpret(result.Statements!);
                }
                catch (RuntimeError e)
                {
                    runtimeErrorHandler(e);
                    return VoidObject.Void;
                }

                return VoidObject.Void;
            }
            else if (result.HasExpr)
            {
                // Even though this is an expression, we need to make it a statement here so we can run the various
                // validation steps on the complete program now (all the statements executed up to now + the expression
                // we just received).
                var previousAndNewStatements = previousStatements
                    .Concat(ImmutableList.Create(new Stmt.ExpressionStmt(result.Expr!)))
                    .ToImmutableList();

                //
                // Name resolution phase
                //

                bool hasNameResolutionErrors = false;
                var nameResolver = new NameResolver(
                    nativeClasses,
                    superGlobals,
                    BindingHandler,
                    AddGlobalClass,
                    nameResolutionError =>
                    {
                        hasNameResolutionErrors = true;
                        nameResolutionErrorHandler(nameResolutionError);
                    }
                );

                nameResolver.Resolve(previousAndNewStatements);

                if (hasNameResolutionErrors)
                {
                    // Resolution errors has been reported back to the provided error handler. Nothing more remains
                    // to be done than aborting the evaluation.
                    return null;
                }

                //
                // Type validation
                //

                bool typeValidationFailed = false;

                TypeValidator.Validate(
                    previousAndNewStatements,
                    typeValidationError =>
                    {
                        typeValidationFailed = true;
                        typeValidationErrorHandler(typeValidationError);
                    },
                    BindingHandler.GetVariableOrFunctionBinding,
                    compilerWarning => compilerWarningHandler(compilerWarning)
                );

                if (typeValidationFailed)
                {
                    return null;
                }

                //
                // Immutability validation
                //

                bool immutabilityValidationFailed = false;

                ImmutabilityValidator.Validate(
                    previousAndNewStatements,
                    immutabilityValidationError =>
                    {
                        immutabilityValidationFailed = true;
                        immutabilityValidationErrorHandler(immutabilityValidationError);
                    },
                    BindingHandler.GetVariableOrFunctionBinding
                );

                //
                // "Code analysis" validation
                //

                bool codeAnalysisValidationFailed = false;

                CodeAnalysisValidator.Validate(
                    previousAndNewStatements,
                    compilerWarning =>
                    {
                        bool result = compilerWarningHandler(compilerWarning);

                        if (result)
                        {
                            codeAnalysisValidationFailed = true;
                        }
                    }
                );

                if (codeAnalysisValidationFailed)
                {
                    return null;
                }

                // All validation was successful, but unlike for statements, there is no need to mutate the
                // previousStatements field in this case. Think about it for a moment. We know that the line being
                // interpreted is an expression, so it _cannot_ have declared any new variable or anything like that
                // (those are only allowed in statements). Hence, we presume that this expression is, if you will,
                // "side-effect-free" in that sense.

                if (immutabilityValidationFailed)
                {
                    return null;
                }

                try
                {
                    return Evaluate(result.Expr!);
                }
                catch (RuntimeError e)
                {
                    runtimeErrorHandler(e);
                    return VoidObject.Void;
                }
            }
            else
            {
                throw new IllegalStateException("syntax was neither Expr nor list of Stmt");
            }
        }

        /// <summary>
        /// Parses the provided source code and returns a string representation of the parsed AST.
        /// </summary>
        /// <param name="source">The source code to a Perlang program (typically a single line of Perlang code).</param>
        /// <param name="scanErrorHandler">A handler for scanner errors.</param>
        /// <param name="parseErrorHandler">A handler for parse errors.</param>
        /// <returns>A string representation of the parsed syntax tree for the given Perlang program, or `null` in case
        /// one or more errors occurred.</returns>
        public string? Parse(string source, Action<ScanError> scanErrorHandler, Action<ParseError> parseErrorHandler)
        {
            //
            // Scanning phase
            //

            bool hasScanErrors = false;
            var scanner = new Scanner(source, scanError =>
            {
                hasScanErrors = true;
                scanErrorHandler(scanError);
            });

            var tokens = scanner.ScanTokens();

            if (hasScanErrors)
            {
                // Something went wrong as early as the "scan" stage. Abort the rest of the processing.
                return null;
            }

            //
            // Parsing phase
            //

            bool hasParseErrors = false;
            var parser = new PerlangParser(
                tokens,
                parseError =>
                {
                    hasParseErrors = true;
                    parseErrorHandler(parseError);
                },
                allowSemicolonElision: replMode
            );

            object syntax = parser.ParseExpressionOrStatements();

            if (hasParseErrors)
            {
                // One or more parse errors were encountered. They have been reported upstream, so we just abort
                // the evaluation at this stage.
                return null;
            }

            if (syntax is List<Stmt> statements)
            {
                StringBuilder result = new();

                foreach (Stmt statement in statements)
                {
                    result.Append(AstPrinter.Print(statement));
                }

                return result.ToString();
            }
            else if (syntax is Expr expr)
            {
                return AstPrinter.Print(expr);
            }
            else
            {
                throw new IllegalStateException("syntax was neither Expr nor list of Stmt");
            }
        }

        /// <summary>
        /// Entry-point for interpreting one or more statements.
        /// </summary>
        /// <param name="statements">An enumerator for a collection of statements.</param>
        private void Interpret(IEnumerable<Stmt> statements)
        {
            foreach (Stmt statement in statements)
            {
                try
                {
                    Execute(statement);
                }
                catch (TargetInvocationException ex)
                {
                    // ex.InnerException should always be non-null at this point, but since it is a nullable property,
                    // I guess it's best to take the unexpected into account and presume it can be null... :-)
                    string message = ex.InnerException?.Message ?? ex.Message;

                    // Setting the token to 'null' here is clearly not optimal, but the problem is that we really don't
                    // know what particular source location triggered the error in question.
                    throw new RuntimeError(null, message);
                }
                catch (SystemException ex)
                {
                    throw new RuntimeError(null, ex.Message, ex);
                }
            }
        }

        public object? VisitLiteralExpr(Expr.Literal expr)
        {
            if (expr.Value is INumericLiteral parsedNumber)
            {
                return parsedNumber.Value;
            }
            else
            {
                return expr.Value;
            }
        }

        public object VisitLogicalExpr(Expr.Logical expr)
        {
            // Note: ! operators in this method are considered "safe" since the type validation is expected to have
            // validated that these operators are only called with `bool` operands.

            object left = Evaluate(expr.Left)!;

            if (expr.Operator.Type == PIPE_PIPE)
            {
                // Preserve the short-circuit semantics of || operations; the first `true` expression encountered in an
                // expression like `false || true || false` will terminate the evaluation.
                if ((bool)left)
                {
                    return true;
                }

                bool right = (bool)Evaluate(expr.Right)!;

                return (bool)left || right;
            }
            else if (expr.Operator.Type == AMPERSAND_AMPERSAND)
            {
                // Preserve the short-circuit semantics of &&| operations; the first `false` expression encountered in an
                // expression like `true && false && true` will terminate the evaluation.
                if (!(bool)left)
                {
                    return false;
                }

                bool right = (bool)Evaluate(expr.Right)!;

                return (bool)left && right;
            }
            else
            {
                throw new RuntimeError(expr.Operator, $"Unsupported logical operator: {expr.Operator.Type}");
            }
        }

        public object? VisitUnaryPrefixExpr(Expr.UnaryPrefix expr)
        {
            object? right = Evaluate(expr.Right);

            switch (expr.Operator.Type)
            {
                case BANG:
                    // Note: bang operator considered "safe" since type validation is expected to have validated the
                    // expression operands.
                    return !(bool)right!;

                case MINUS:
                    CheckNumberOperand(expr.Operator, right);

                    // This was previously done using the 'dynamic' keyword, but a major drawback of doing it like this
                    // is that it makes it harder for the compiler to do a static analysis of the code. This particular
                    // section was fine, but other parts broke as part of the .NET 6 upgrade. More details:
                    // https://github.com/perlang-org/perlang/pull/223/files#r747380990
                    //
                    // The general conclusion was that it's safer and more predictable to move away from 'dynamic'
                    // altogether.

                    switch (right)
                    {
                        case SByte value:
                            return -value;

                        case Int16 value:
                            return -value;

                        case Int32 value:
                            return -value;

                        case Int64 value:
                            return -value;

                        case Byte value:
                            return -value;

                        case UInt16 value:
                            return -value;

                        case UInt32 value:
                            return -value;

                        case UInt64 value:
                            // .NET will give an "ambiguous invocation" here, suggesting that we cast `value` to either
                            // a `decimal`, `double` or `float`. However, making an integer-type of variable become a
                            // floating point value all of a sudden seems like very odd and unexpected semantics here.
                            // Even though it is less performant, we go with the BigInteger approach for now: negating a
                            // UInt64 will inherently expand it to a BigInteger.
                            return -new BigInteger(value);

                        case Single value: // i.e. float
                            return -value;

                        case Double value:
                            return -value;

                        case BigInteger value:
                            return -value;

                        default:
                            // TODO: Use TypeExtensions.ToTypeKeywordNew() instead. Just need to make it handle null objects
                            throw new RuntimeError(expr.Operator, $"Internal runtime error: Unexpected type of object ${right?.GetType().FullName} encountered");
                    }
            }

            // Unreachable.
            return null;
        }

        public object VisitUnaryPostfixExpr(Expr.UnaryPostfix expr)
        {
            object? left = Evaluate(expr.Left);

            // We do have a check at the parser side also, but this one covers "null" cases.
            if (!IsValidNumberType(left))
            {
                switch (expr.Operator.Type)
                {
                    case PLUS_PLUS:
                        throw new RuntimeError(expr.Operator, $"++ can only be used to increment numbers, not {StringifyType(left)}");

                    case MINUS_MINUS:
                        throw new RuntimeError(expr.Operator, $"-- can only be used to decrement numbers, not {StringifyType(left)}");

                    default:
                        throw new RuntimeError(expr.Operator, $"Unsupported operator encountered: {expr.Operator.Type}");
                }
            }

            // The nullability check has been taken care of by IsValidNumberType() for us.
            var variable = (Expr.Identifier)expr.Left;

            // Note: this must NOT be converted to a switch expression, since it mangles the type of `int` and `long`
            // values to `double`. This is because this is a type that all used types (`int`, `long`, `double`) can be
            // implicitly converted to. More details: https://stackoverflow.com/a/70130076/227779
            object value;
            switch (left)
            {
                case int previousIntValue:
                    value = expr.Operator.Type switch
                    {
                        PLUS_PLUS => previousIntValue + 1,
                        MINUS_MINUS => previousIntValue - 1,
                        _ => throw new RuntimeError(expr.Operator, $"Unsupported operator encountered: {expr.Operator.Type}")
                    };
                    break;
                case long previousLongValue:
                    value = expr.Operator.Type switch
                    {
                        PLUS_PLUS => previousLongValue + 1,
                        MINUS_MINUS => previousLongValue - 1,
                        _ => throw new RuntimeError(expr.Operator, $"Unsupported operator encountered: {expr.Operator.Type}")
                    };
                    break;
                case BigInteger previousBigIntegerValue:
                    value = expr.Operator.Type switch
                    {
                        PLUS_PLUS => previousBigIntegerValue + 1,
                        MINUS_MINUS => previousBigIntegerValue - 1,
                        _ => throw new RuntimeError(expr.Operator, $"Unsupported operator encountered: {expr.Operator.Type}")
                    };
                    break;
                case double previousDoubleValue:
                    value = expr.Operator.Type switch
                    {
                        PLUS_PLUS => previousDoubleValue + 1,
                        MINUS_MINUS => previousDoubleValue - 1,
                        _ => throw new RuntimeError(expr.Operator, $"Unsupported operator encountered: {expr.Operator.Type}")
                    };
                    break;
                default:
                    throw new RuntimeError(expr.Operator, $"Internal runtime: Unsupported type {StringifyType(left)} encountered with {expr.Operator.Type} operator");
            }

            if (BindingHandler.GetLocalBinding(expr, out Binding? binding))
            {
                if (binding is IDistanceAwareBinding distanceAwareBinding)
                {
                    currentEnvironment.AssignAt(distanceAwareBinding.Distance, expr.Name, value);
                }
                else
                {
                    throw new RuntimeError(expr.Operator, $"Unsupported operator '{expr.Operator.Type}' encountered for non-distance-aware binding '{binding}'");
                }
            }
            else
            {
                globals.Assign(variable.Name, value);
            }

            return left!;
        }

        public object VisitIdentifierExpr(Expr.Identifier expr)
        {
            return LookUpVariable(expr.Name, expr);
        }

        public object VisitGetExpr(Expr.Get expr)
        {
            object? obj = Evaluate(expr.Object);

            if (obj == null)
            {
                throw new RuntimeError(expr.Name, "Object reference not set to an instance of an object");
            }

            if (expr.Methods.SingleOrDefault() != null)
            {
                return new TargetAndMethodContainer(obj, expr.Methods.Single());
            }
            else
            {
                throw new RuntimeError(expr.Name, "Internal runtime error: Expected expr.Method to be non-null");
            }
        }

        /// <summary>
        /// Gets the value of a variable, in the current scope or any surrounding scopes.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="identifier">The expression identifying the variable. For example, in `var foo = bar`, `bar` is
        /// the identifier expression.</param>
        /// <returns>The value of the variable.</returns>
        /// <exception cref="RuntimeError">When the binding found is not a <see cref="IDistanceAwareBinding"/>
        /// instance.</exception>
        private object LookUpVariable(Token name, Expr.Identifier identifier)
        {
            if (BindingHandler.GetLocalBinding(identifier, out Binding? localBinding))
            {
                if (localBinding is IDistanceAwareBinding distanceAwareBinding)
                {
                    return currentEnvironment.GetAt(distanceAwareBinding.Distance, name.Lexeme);
                }
                else
                {
                    throw new RuntimeError(name, $"Attempting to lookup variable for non-distance-aware binding '{localBinding}'");
                }
            }
            else if (globalClasses.TryGetValue(name.Lexeme, out object? globalClass))
            {
                // TODO: This probably means we could drop Perlang classes from being registered as globals as well.
                return globalClass;
            }
            else
            {
                return globals.Get(name);
            }
        }

        private static void CheckNumberOperand(Token @operator, object? operand)
        {
            if (IsValidNumberType(operand))
            {
                return;
            }

            throw new RuntimeError(@operator, "Operand must be a number.");
        }

        /// <summary>
        /// Ensures that the given operands to a binary expression are numeric.
        /// </summary>
        /// <param name="operator">The operator for the given operand. Only used for error handling.</param>
        /// <param name="left">The left-hand operand (evaluated value).</param>
        /// <param name="right">The right-hand operand (evaluated value).</param>
        /// <exception cref="RuntimeError">When one or both of the given operands are non-numeric.</exception>
        private static void CheckNumberOperands(Token @operator, object? left, object? right)
        {
            if (IsValidNumberType(left) && IsValidNumberType(right))
            {
                return;
            }

            // TODO: We currently get this for `string + null`, which doesn't feel right. See
            // TODO: https://github.com/perlang-org/perlang/issues/330 for more details.
            throw new RuntimeError(@operator, $"Operands must be numbers, not {StringifyType(left)} and {StringifyType(right)}");
        }

        private static bool IsValidNumberType(object? value)
        {
            if (value == null)
            {
                return false;
            }

            switch (value)
            {
                case SByte _:
                case Int16 _:
                case Int32 _:
                case Int64 _:
                case Byte _:
                case UInt16 _:
                case UInt32 _:
                case UInt64 _:
                case Single _: // i.e. float
                case Double _:
                case BigInteger _:
                    return true;
            }

            return false;
        }

        private static bool IsEqual(object? a, object? b)
        {
            // null is only equal to null.
            if (a == null && b == null)
            {
                return true;
            }

            if (a == null)
            {
                // null != non-null
                return false;
            }

            // Note: must use dynamic here since `object == object` behaves differently than e.g. `int == double`,
            // tragically enough. More details:
            // https://learn.microsoft.com/en-us/dotnet/api/System.Object.Equals?view=net-7.0#notes-for-callers
            try
            {
                return (dynamic)a == (dynamic?)b;
            }
            catch (RuntimeBinderException)
            {
                // This will happen when trying to compare e.g `double == BigInteger`. We fall back to Equals() in this
                // case, hoping that it will make sense...
                return a.Equals(b);
            }
        }

        public object? VisitGroupingExpr(Expr.Grouping expr)
        {
            return Evaluate(expr.Expression);
        }

        /// <summary>
        /// Entry-point for evaluating a single expression. This method is also recursively called from the methods
        /// implementing <see cref="Expr.IVisitor{TR}"/>.
        /// </summary>
        /// <param name="expr">An expression.</param>
        /// <returns>The evaluated value of the expression. For example, if the expression is "1 + 1", the return value
        /// is the integer "2" and so forth.</returns>
        /// <exception cref="RuntimeError">When a runtime error is encountered while evaluating.</exception>
        private object? Evaluate(Expr expr)
        {
            try
            {
                return expr.Accept(this);
            }
            catch (TargetInvocationException ex)
            {
                Token? token = (expr as ITokenAware)?.Token;

                // ex.InnerException should always be non-null at this point, but since it is a nullable property,
                // I guess it's best to take the unexpected into account and presume it can be null... :-)
                string message = ex.InnerException?.Message ?? ex.Message;

                throw new RuntimeError(token, message, ex);
            }
            catch (SystemException ex)
            {
                Token? token = (expr as ITokenAware)?.Token;

                throw new RuntimeError(token, ex.Message, ex);
            }
        }

        private void Execute(Stmt stmt)
        {
            stmt.Accept(this);
        }

        private void AddGlobalClass(string name, PerlangClass perlangClass)
        {
            globalClasses[name] = perlangClass;
        }

        public void ExecuteBlock(IEnumerable<Stmt> statements, IEnvironment blockEnvironment)
        {
            IEnvironment previousEnvironment = currentEnvironment;

            try
            {
                currentEnvironment = blockEnvironment;

                foreach (Stmt statement in statements)
                {
                    Execute(statement);
                }
            }
            finally
            {
                currentEnvironment = previousEnvironment;
            }
        }

        public VoidObject VisitBlockStmt(Stmt.Block stmt)
        {
            ExecuteBlock(stmt.Statements, new PerlangEnvironment(currentEnvironment));
            return VoidObject.Void;
        }

        public VoidObject VisitClassStmt(Stmt.Class stmt)
        {
            currentEnvironment.Define(stmt.Name, globalClasses[stmt.Name.Lexeme]);
            return VoidObject.Void;
        }

        public VoidObject VisitExpressionStmt(Stmt.ExpressionStmt stmt)
        {
            Evaluate(stmt.Expression);
            return VoidObject.Void;
        }

        public VoidObject VisitFunctionStmt(Stmt.Function stmt)
        {
            var function = new PerlangFunction(stmt, currentEnvironment);
            currentEnvironment.Define(stmt.Name, function);
            return VoidObject.Void;
        }

        public VoidObject VisitIfStmt(Stmt.If stmt)
        {
            // Bang operator should be safe, since types have been validated by TypeValidator.
            if ((bool)Evaluate(stmt.Condition)!)
            {
                Execute(stmt.ThenBranch);
            }
            else if (stmt.ElseBranch != null)
            {
                Execute(stmt.ElseBranch);
            }

            return VoidObject.Void;
        }

        public VoidObject VisitPrintStmt(Stmt.Print stmt)
        {
            object? value = Evaluate(stmt.Expression);
            standardOutputHandler(Stringify(value));
            return VoidObject.Void;
        }

        public VoidObject VisitReturnStmt(Stmt.Return stmt)
        {
            object? value = null;

            if (stmt.Value != null)
            {
                value = Evaluate(stmt.Value);
            }

            throw new Return(value);
        }

        public VoidObject VisitVarStmt(Stmt.Var stmt)
        {
            object? value = null;

            if (stmt.Initializer != null)
            {
                value = Evaluate(stmt.Initializer);

                value = ExpandIntegerIfRequired(value, stmt.TypeReference);
            }

            currentEnvironment.Define(stmt.Name, value);
            return VoidObject.Void;
        }

        public VoidObject VisitWhileStmt(Stmt.While stmt)
        {
            // Bang operator should be safe, since types have been validated by TypeValidator.
            while ((bool)(Evaluate(stmt.Condition)!))
            {
                Execute(stmt.Body);
            }

            return VoidObject.Void;
        }

        public object? VisitEmptyExpr(Expr.Empty expr)
        {
            return null;
        }

        public object? VisitAssignExpr(Expr.Assign expr)
        {
            object? value = Evaluate(expr.Value);

            if (BindingHandler.GetLocalBinding(expr, out Binding? binding))
            {
                if (binding is IDistanceAwareBinding distanceAwareBinding)
                {
                    currentEnvironment.AssignAt(distanceAwareBinding.Distance, expr.Name, value);
                }
                else
                {
                    throw new RuntimeError(expr.Name, $"Unsupported variable assignment encountered for non-distance-aware binding '{binding}'");
                }
            }
            else
            {
                globals.Assign(expr.Name, value);
            }

            return value;
        }

        public object? VisitBinaryExpr(Expr.Binary expr)
        {
            // Null checks have been taken care of by TypeResolver.VisitBinaryExpr().
            object left = Evaluate(expr.Left)!;
            object right = Evaluate(expr.Right)!;

            var leftConvertible = left as IConvertible;
            var rightConvertible = right as IConvertible;

            switch (expr.Operator.Type)
            {
                //
                // Comparison operators
                //
                // IComparable would be useful to reduce code duplication here, but it has one major problem: it only
                // supports same-type comparisons (int+int, long+long etc). We do not want to limit our code like that.
                //

                // Note: do **NOT** add new operators here without adding corresponding tests. (in the future, try to
                // enforce this via e.g. ArchUnit.NET or a unit test)

                case GREATER:
                    CheckNumberOperands(expr.Operator, left, right);

                    // Bang operator (!) is safe because of the CheckNumberOperands() call above.
                    if (left is float or double && right is float or double)
                    {
                        double leftNumber = leftConvertible!.ToDouble(CultureInfo.InvariantCulture);
                        double rightNumber = rightConvertible!.ToDouble(CultureInfo.InvariantCulture);

                        return leftNumber > rightNumber;
                    }
                    else if (left is float or double && right is int or long)
                    {
                        double leftNumber = leftConvertible!.ToDouble(CultureInfo.InvariantCulture);
                        long rightNumber = rightConvertible!.ToInt64(CultureInfo.InvariantCulture);

                        return leftNumber > rightNumber;
                    }
                    else if (left is float or double && right is uint or ulong)
                    {
                        double leftNumber = leftConvertible!.ToDouble(CultureInfo.InvariantCulture);
                        ulong rightNumber = rightConvertible!.ToUInt64(CultureInfo.InvariantCulture);

                        return leftNumber >= rightNumber;
                    }
                    else if (left is int or long && right is float or double)
                    {
                        long leftNumber = leftConvertible!.ToInt64(CultureInfo.InvariantCulture);
                        double rightNumber = rightConvertible!.ToDouble(CultureInfo.InvariantCulture);

                        return leftNumber > rightNumber;
                    }
                    else if (left is uint or ulong && right is float or double)
                    {
                        ulong leftNumber = leftConvertible!.ToUInt64(CultureInfo.InvariantCulture);
                        double rightNumber = rightConvertible!.ToDouble(CultureInfo.InvariantCulture);

                        return leftNumber > rightNumber;
                    }
                    else if (left is int or long or uint && right is int or long or uint)
                    {
                        long leftNumber = leftConvertible!.ToInt64(CultureInfo.InvariantCulture);
                        long rightNumber = rightConvertible!.ToInt64(CultureInfo.InvariantCulture);

                        return leftNumber > rightNumber;
                    }
                    else if (left is uint or ulong && right is uint or ulong)
                    {
                        ulong leftNumber = leftConvertible!.ToUInt64(CultureInfo.InvariantCulture);
                        ulong rightNumber = rightConvertible!.ToUInt64(CultureInfo.InvariantCulture);

                        return leftNumber > rightNumber;
                    }
                    else if (left is BigInteger && right is BigInteger)
                    {
                        var leftBigInt = (BigInteger)left;
                        var rightBigInt = (BigInteger)right;

                        return leftBigInt > rightBigInt;
                    }
                    else if (left is int or long && right is BigInteger)
                    {
                        long leftNumber = leftConvertible!.ToInt64(CultureInfo.InvariantCulture);
                        var rightBigInt = (BigInteger)right;

                        return leftNumber > rightBigInt;
                    }
                    else if (left is uint or ulong && right is BigInteger)
                    {
                        ulong leftNumber = leftConvertible!.ToUInt64(CultureInfo.InvariantCulture);
                        var rightBigInt = (BigInteger)right;

                        return leftNumber > rightBigInt;
                    }
                    else if (left is BigInteger && right is int or long)
                    {
                        var leftBigInt = (BigInteger)left;
                        long rightNumber = rightConvertible!.ToInt64(CultureInfo.InvariantCulture);

                        return leftBigInt > rightNumber;
                    }
                    else if (left is BigInteger && right is uint or ulong)
                    {
                        var leftBigInt = (BigInteger)left;
                        ulong rightNumber = rightConvertible!.ToUInt64(CultureInfo.InvariantCulture);

                        return leftBigInt > rightNumber;
                    }
                    else
                    {
                        // Should in practice never get here, since TypeResolver should catch these at compile-time. If
                        // it ever happens, it indicates an error somewhere.
                        string message = InterpreterMessages.UnsupportedOperandTypes(expr.Operator.Type, left, right);
                        throw new RuntimeError(expr.Operator, message);
                    }

                case GREATER_EQUAL:
                    CheckNumberOperands(expr.Operator, left, right);

                    // Bang operator (!) is safe because of the CheckNumberOperands() call above.
                    if (left is float or double && right is float or double)
                    {
                        double leftNumber = leftConvertible!.ToDouble(CultureInfo.InvariantCulture);
                        double rightNumber = rightConvertible!.ToDouble(CultureInfo.InvariantCulture);

                        return leftNumber >= rightNumber;
                    }
                    else if (left is float or double && right is int or long)
                    {
                        double leftNumber = leftConvertible!.ToDouble(CultureInfo.InvariantCulture);
                        long rightNumber = rightConvertible!.ToInt64(CultureInfo.InvariantCulture);

                        return leftNumber >= rightNumber;
                    }
                    else if (left is float or double && right is uint or ulong)
                    {
                        double leftNumber = leftConvertible!.ToDouble(CultureInfo.InvariantCulture);
                        ulong rightNumber = rightConvertible!.ToUInt64(CultureInfo.InvariantCulture);

                        return leftNumber >= rightNumber;
                    }
                    else if (left is int or long && right is float or double)
                    {
                        long leftNumber = leftConvertible!.ToInt64(CultureInfo.InvariantCulture);
                        double rightNumber = rightConvertible!.ToDouble(CultureInfo.InvariantCulture);

                        return leftNumber >= rightNumber;
                    }
                    else if (left is uint or ulong && right is float or double)
                    {
                        ulong leftNumber = leftConvertible!.ToUInt64(CultureInfo.InvariantCulture);
                        double rightNumber = rightConvertible!.ToDouble(CultureInfo.InvariantCulture);

                        return leftNumber >= rightNumber;
                    }
                    else if (left is int or long or uint && right is int or long or uint)
                    {
                        long leftNumber = leftConvertible!.ToInt64(CultureInfo.InvariantCulture);
                        long rightNumber = rightConvertible!.ToInt64(CultureInfo.InvariantCulture);

                        return leftNumber >= rightNumber;
                    }
                    else if (left is uint or ulong && right is uint or ulong)
                    {
                        ulong leftNumber = leftConvertible!.ToUInt64(CultureInfo.InvariantCulture);
                        ulong rightNumber = rightConvertible!.ToUInt64(CultureInfo.InvariantCulture);

                        return leftNumber >= rightNumber;
                    }
                    else if (left is BigInteger && right is BigInteger)
                    {
                        var leftBigInt = (BigInteger)left;
                        var rightBigInt = (BigInteger)right;

                        return leftBigInt >= rightBigInt;
                    }
                    else if (left is int or long && right is BigInteger)
                    {
                        long leftNumber = leftConvertible!.ToInt64(CultureInfo.InvariantCulture);
                        var rightBigInt = (BigInteger)right;

                        return leftNumber >= rightBigInt;
                    }
                    else if (left is uint or ulong && right is BigInteger)
                    {
                        ulong leftNumber = leftConvertible!.ToUInt64(CultureInfo.InvariantCulture);
                        var rightBigInt = (BigInteger)right;

                        return leftNumber >= rightBigInt;
                    }
                    else if (left is BigInteger && right is int or long)
                    {
                        var leftBigInt = (BigInteger)left;
                        long rightNumber = rightConvertible!.ToInt64(CultureInfo.InvariantCulture);

                        return leftBigInt >= rightNumber;
                    }
                    else if (left is BigInteger && right is uint or ulong)
                    {
                        var leftBigInt = (BigInteger)left;
                        ulong rightNumber = rightConvertible!.ToUInt64(CultureInfo.InvariantCulture);

                        return leftBigInt >= rightNumber;
                    }
                    else
                    {
                        // Should in practice never get here, since TypeResolver should catch these at compile-time. If
                        // it ever happens, it indicates an error somewhere.
                        string message = InterpreterMessages.UnsupportedOperandTypes(expr.Operator.Type, left, right);
                        throw new RuntimeError(expr.Operator, message);
                    }

                case LESS:
                    CheckNumberOperands(expr.Operator, left, right);

                    // Bang operator (!) is safe because of the CheckNumberOperands() call above.
                    if (left is float or double && right is float or double)
                    {
                        double leftNumber = leftConvertible!.ToDouble(CultureInfo.InvariantCulture);
                        double rightNumber = rightConvertible!.ToDouble(CultureInfo.InvariantCulture);

                        return leftNumber < rightNumber;
                    }
                    else if (left is float or double && right is int or long)
                    {
                        double leftNumber = leftConvertible!.ToDouble(CultureInfo.InvariantCulture);
                        long rightNumber = rightConvertible!.ToInt64(CultureInfo.InvariantCulture);

                        return leftNumber < rightNumber;
                    }
                    else if (left is float or double && right is uint or ulong)
                    {
                        double leftNumber = leftConvertible!.ToDouble(CultureInfo.InvariantCulture);
                        ulong rightNumber = rightConvertible!.ToUInt64(CultureInfo.InvariantCulture);

                        return leftNumber < rightNumber;
                    }
                    else if (left is int or long && right is float or double)
                    {
                        long leftNumber = leftConvertible!.ToInt64(CultureInfo.InvariantCulture);
                        double rightNumber = rightConvertible!.ToDouble(CultureInfo.InvariantCulture);

                        return leftNumber < rightNumber;
                    }
                    else if (left is uint or ulong && right is float or double)
                    {
                        ulong leftNumber = leftConvertible!.ToUInt64(CultureInfo.InvariantCulture);
                        double rightNumber = rightConvertible!.ToDouble(CultureInfo.InvariantCulture);

                        return leftNumber < rightNumber;
                    }
                    else if (left is int or long or uint && right is int or long or uint)
                    {
                        long leftNumber = leftConvertible!.ToInt64(CultureInfo.InvariantCulture);
                        long rightNumber = rightConvertible!.ToInt64(CultureInfo.InvariantCulture);

                        return leftNumber < rightNumber;
                    }
                    else if (left is uint or ulong && right is uint or ulong)
                    {
                        ulong leftNumber = leftConvertible!.ToUInt64(CultureInfo.InvariantCulture);
                        ulong rightNumber = rightConvertible!.ToUInt64(CultureInfo.InvariantCulture);

                        return leftNumber < rightNumber;
                    }
                    else if (left is BigInteger && right is BigInteger)
                    {
                        var leftBigInt = (BigInteger)left;
                        var rightBigInt = (BigInteger)right;

                        return leftBigInt < rightBigInt;
                    }
                    else if (left is int or long && right is BigInteger)
                    {
                        long leftNumber = leftConvertible!.ToInt64(CultureInfo.InvariantCulture);
                        var rightBigInt = (BigInteger)right;

                        return leftNumber < rightBigInt;
                    }
                    else if (left is uint or ulong && right is BigInteger)
                    {
                        ulong leftNumber = leftConvertible!.ToUInt64(CultureInfo.InvariantCulture);
                        var rightBigInt = (BigInteger)right;

                        return leftNumber < rightBigInt;
                    }
                    else if (left is BigInteger && right is int or long)
                    {
                        var leftBigInt = (BigInteger)left;
                        long rightNumber = rightConvertible!.ToInt64(CultureInfo.InvariantCulture);

                        return leftBigInt < rightNumber;
                    }
                    else if (left is BigInteger && right is uint or ulong)
                    {
                        var leftBigInt = (BigInteger)left;
                        ulong rightNumber = rightConvertible!.ToUInt64(CultureInfo.InvariantCulture);

                        return leftBigInt < rightNumber;
                    }
                    else
                    {
                        // Should in practice never get here, since TypeResolver should catch these at compile-time. If
                        // it ever happens, it indicates an error somewhere.
                        string message = InterpreterMessages.UnsupportedOperandTypes(expr.Operator.Type, left, right);
                        throw new RuntimeError(expr.Operator, message);
                    }

                case LESS_EQUAL:
                    CheckNumberOperands(expr.Operator, left, right);

                    // Bang operator (!) is safe because of the CheckNumberOperands() call above.
                    if (left is float or double && right is float or double)
                    {
                        double leftNumber = leftConvertible!.ToDouble(CultureInfo.InvariantCulture);
                        double rightNumber = rightConvertible!.ToDouble(CultureInfo.InvariantCulture);

                        return leftNumber <= rightNumber;
                    }
                    else if (left is float or double && right is int or long)
                    {
                        double leftNumber = leftConvertible!.ToDouble(CultureInfo.InvariantCulture);
                        long rightNumber = rightConvertible!.ToInt64(CultureInfo.InvariantCulture);

                        return leftNumber <= rightNumber;
                    }
                    else if (left is float or double && right is uint or ulong)
                    {
                        double leftNumber = leftConvertible!.ToDouble(CultureInfo.InvariantCulture);
                        ulong rightNumber = rightConvertible!.ToUInt64(CultureInfo.InvariantCulture);

                        return leftNumber <= rightNumber;
                    }
                    else if (left is int or long && right is float or double)
                    {
                        long leftNumber = leftConvertible!.ToInt64(CultureInfo.InvariantCulture);
                        double rightNumber = rightConvertible!.ToDouble(CultureInfo.InvariantCulture);

                        return leftNumber <= rightNumber;
                    }
                    else if (left is uint or ulong && right is float or double)
                    {
                        ulong leftNumber = leftConvertible!.ToUInt64(CultureInfo.InvariantCulture);
                        double rightNumber = rightConvertible!.ToDouble(CultureInfo.InvariantCulture);

                        return leftNumber <= rightNumber;
                    }
                    else if (left is int or long or uint && right is int or long or uint)
                    {
                        long leftNumber = leftConvertible!.ToInt64(CultureInfo.InvariantCulture);
                        long rightNumber = rightConvertible!.ToInt64(CultureInfo.InvariantCulture);

                        return leftNumber <= rightNumber;
                    }
                    else if (left is uint or ulong && right is uint or ulong)
                    {
                        ulong leftNumber = leftConvertible!.ToUInt64(CultureInfo.InvariantCulture);
                        ulong rightNumber = rightConvertible!.ToUInt64(CultureInfo.InvariantCulture);

                        return leftNumber <= rightNumber;
                    }
                    else if (left is BigInteger && right is BigInteger)
                    {
                        var leftBigInt = (BigInteger)left;
                        var rightBigInt = (BigInteger)right;

                        return leftBigInt <= rightBigInt;
                    }
                    else if (left is int or long && right is BigInteger)
                    {
                        long leftNumber = leftConvertible!.ToInt64(CultureInfo.InvariantCulture);
                        var rightBigInt = (BigInteger)right;

                        return leftNumber <= rightBigInt;
                    }
                    else if (left is uint or ulong && right is BigInteger)
                    {
                        ulong leftNumber = leftConvertible!.ToUInt64(CultureInfo.InvariantCulture);
                        var rightBigInt = (BigInteger)right;

                        return leftNumber <= rightBigInt;
                    }
                    else if (left is BigInteger && right is int or long)
                    {
                        var leftBigInt = (BigInteger)left;
                        long rightNumber = rightConvertible!.ToInt64(CultureInfo.InvariantCulture);

                        return leftBigInt <= rightNumber;
                    }
                    else if (left is BigInteger && right is uint or ulong)
                    {
                        var leftBigInt = (BigInteger)left;
                        ulong rightNumber = rightConvertible!.ToUInt64(CultureInfo.InvariantCulture);

                        return leftBigInt <= rightNumber;
                    }
                    else
                    {
                        // Should in practice never get here, since TypeResolver should catch these at compile-time. If
                        // it ever happens, it indicates an error somewhere.
                        string message = InterpreterMessages.UnsupportedOperandTypes(expr.Operator.Type, left, right);
                        throw new RuntimeError(expr.Operator, message);
                    }

                case BANG_EQUAL:
                    return !IsEqual(left, right);

                case EQUAL_EQUAL:
                    return IsEqual(left, right);

                //
                // Arithmetic operators
                //

                case MINUS:
                case MINUS_EQUAL: // Actual reassignment is handled elsewhere.
                    CheckNumberOperands(expr.Operator, left, right);

                    // Bang operator (!) is safe because of the CheckNumberOperands() call above.
                    if (left is float && right is float)
                    {
                        float leftNumber = leftConvertible!.ToSingle(CultureInfo.InvariantCulture);
                        float rightNumber = rightConvertible!.ToSingle(CultureInfo.InvariantCulture);

                        return leftNumber - rightNumber;
                    }
                    else if (left is float or double && right is float or double)
                    {
                        double leftNumber = leftConvertible!.ToDouble(CultureInfo.InvariantCulture);
                        double rightNumber = rightConvertible!.ToDouble(CultureInfo.InvariantCulture);

                        return leftNumber - rightNumber;
                    }
                    else if (left is float && right is int or long)
                    {
                        float leftNumber = leftConvertible!.ToSingle(CultureInfo.InvariantCulture);
                        long rightNumber = rightConvertible!.ToInt64(CultureInfo.InvariantCulture);

                        return leftNumber - rightNumber;
                    }
                    else if (left is double && right is int or long)
                    {
                        double leftNumber = leftConvertible!.ToDouble(CultureInfo.InvariantCulture);
                        long rightNumber = rightConvertible!.ToInt64(CultureInfo.InvariantCulture);

                        return leftNumber - rightNumber;
                    }
                    else if (left is float && right is uint or ulong)
                    {
                        float leftNumber = leftConvertible!.ToSingle(CultureInfo.InvariantCulture);
                        ulong rightNumber = rightConvertible!.ToUInt64(CultureInfo.InvariantCulture);

                        return leftNumber - rightNumber;
                    }
                    else if (left is double && right is uint or ulong)
                    {
                        double leftNumber = leftConvertible!.ToDouble(CultureInfo.InvariantCulture);
                        ulong rightNumber = rightConvertible!.ToUInt64(CultureInfo.InvariantCulture);

                        return leftNumber - rightNumber;
                    }
                    else if (left is int or long && right is float)
                    {
                        long leftNumber = leftConvertible!.ToInt64(CultureInfo.InvariantCulture);
                        float rightNumber = rightConvertible!.ToSingle(CultureInfo.InvariantCulture);

                        return leftNumber - rightNumber;
                    }
                    else if (left is int or long && right is double)
                    {
                        long leftNumber = leftConvertible!.ToInt64(CultureInfo.InvariantCulture);
                        double rightNumber = rightConvertible!.ToDouble(CultureInfo.InvariantCulture);

                        return leftNumber - rightNumber;
                    }
                    else if (left is uint or ulong && right is float)
                    {
                        ulong leftNumber = leftConvertible!.ToUInt64(CultureInfo.InvariantCulture);
                        float rightNumber = rightConvertible!.ToSingle(CultureInfo.InvariantCulture);

                        return leftNumber - rightNumber;
                    }
                    else if (left is uint or ulong && right is double)
                    {
                        ulong leftNumber = leftConvertible!.ToUInt64(CultureInfo.InvariantCulture);
                        double rightNumber = rightConvertible!.ToDouble(CultureInfo.InvariantCulture);

                        return leftNumber - rightNumber;
                    }
                    else if (left is int && right is int)
                    {
                        int leftNumber = leftConvertible!.ToInt32(CultureInfo.InvariantCulture);
                        int rightNumber = rightConvertible!.ToInt32(CultureInfo.InvariantCulture);

                        return leftNumber - rightNumber;
                    }
                    else if (left is uint && right is uint)
                    {
                        uint leftNumber = leftConvertible!.ToUInt32(CultureInfo.InvariantCulture);
                        uint rightNumber = rightConvertible!.ToUInt32(CultureInfo.InvariantCulture);

                        return leftNumber - rightNumber;
                    }
                    else if (left is int or long or uint && right is int or long or uint)
                    {
                        long leftNumber = leftConvertible!.ToInt64(CultureInfo.InvariantCulture);
                        long rightNumber = rightConvertible!.ToInt64(CultureInfo.InvariantCulture);

                        return leftNumber - rightNumber;
                    }
                    else if (left is uint or ulong && right is uint or ulong)
                    {
                        ulong leftNumber = leftConvertible!.ToUInt64(CultureInfo.InvariantCulture);
                        ulong rightNumber = rightConvertible!.ToUInt64(CultureInfo.InvariantCulture);

                        return leftNumber - rightNumber;
                    }
                    else if (left is BigInteger && right is BigInteger)
                    {
                        var leftBigInt = (BigInteger)left;
                        var rightBigInt = (BigInteger)right;

                        return leftBigInt - rightBigInt;
                    }
                    else if (left is int or long && right is BigInteger)
                    {
                        long leftNumber = leftConvertible!.ToInt64(CultureInfo.InvariantCulture);
                        var rightBigInt = (BigInteger)right;

                        return leftNumber - rightBigInt;
                    }
                    else if (left is uint or ulong && right is BigInteger)
                    {
                        ulong leftNumber = leftConvertible!.ToUInt64(CultureInfo.InvariantCulture);
                        var rightBigInt = (BigInteger)right;

                        return leftNumber - rightBigInt;
                    }
                    else if (left is BigInteger && right is int or long)
                    {
                        var leftBigInt = (BigInteger)left;
                        long rightNumber = rightConvertible!.ToInt64(CultureInfo.InvariantCulture);

                        return leftBigInt - rightNumber;
                    }
                    else if (left is BigInteger && right is uint or ulong)
                    {
                        var leftBigInt = (BigInteger)left;
                        ulong rightNumber = rightConvertible!.ToUInt64(CultureInfo.InvariantCulture);

                        return leftBigInt - rightNumber;
                    }
                    else
                    {
                        // Should in practice never get here, since TypeResolver should catch these at compile-time. If
                        // it ever happens, it indicates an error somewhere.
                        string message = InterpreterMessages.UnsupportedOperandTypes(expr.Operator.Type, left, right);
                        throw new RuntimeError(expr.Operator, message);
                    }

                case PLUS:
                    Lang.String? leftString = left as Lang.String;
                    Lang.String? rightString = right as Lang.String;

                    if (leftString != null)
                    {
                        if (rightString != null)
                        {
                            // TODO: How to handle this complexity? Lang.String does _not_ implement the + operator, and
                            // I'm not sure we even want it to. What would Utf16String + AsciiString mean? (one way to
                            // deal with that would be to not let Utf16String implement the String interface, but it
                            // would be kind of odd. It would almost even seem better to not even support UTF-16 strings
                            // at all in that case)
                            return (AsciiString)leftString + (AsciiString)rightString;
                        }
                        else if (IsValidNumberType(right))
                        {
                            if (rightConvertible != null)
                            {
                                // The explicit IFormatProvider is required to ensure we use 123.45 format, regardless
                                // of host OS language/region settings. See #263 for more details.
                                return leftString + rightConvertible.ToString(CultureInfo.InvariantCulture);
                            }
                            else if (right is BigInteger rightBigint)
                            {
                                return leftString + rightBigint.ToString(CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                // TODO: Make this a compile-time error instead.
                                string message = InterpreterMessages.UnsupportedOperandTypes(expr.Operator.Type, left, right);
                                throw new RuntimeError(expr.Operator, message);
                            }
                        }
                    }

                    if (IsValidNumberType(left) && rightString != null)
                    {
                        if (leftConvertible != null)
                        {
                            // The explicit IFormatProvider is required to ensure we use 123.45 format, regardless of
                            // host OS language/region settings. See #263 for more details.
                            return leftConvertible.ToString(CultureInfo.InvariantCulture) + rightString;
                        }
                        else if (left is BigInteger leftBigint)
                        {
                            return leftBigint.ToString(CultureInfo.InvariantCulture) + right;
                        }
                        else
                        {
                            // TODO: Make this a compile-time error instead.
                            string message = InterpreterMessages.UnsupportedOperandTypes(expr.Operator.Type, left, right);
                            throw new RuntimeError(expr.Operator, message);
                        }
                    }

                    CheckNumberOperands(expr.Operator, left, right);

                    // Regular numeric operand implementation comes after string concatenation.
                    // Bang operator (!) is safe because of the CheckNumberOperands() call above.
                    if (left is float && right is float)
                    {
                        float leftNumber = leftConvertible!.ToSingle(CultureInfo.InvariantCulture);
                        float rightNumber = rightConvertible!.ToSingle(CultureInfo.InvariantCulture);

                        return leftNumber + rightNumber;
                    }
                    else if (left is float or double && right is float or double)
                    {
                        double leftNumber = leftConvertible!.ToDouble(CultureInfo.InvariantCulture);
                        double rightNumber = rightConvertible!.ToDouble(CultureInfo.InvariantCulture);

                        return leftNumber + rightNumber;
                    }
                    else if (left is int or long && right is float)
                    {
                        long leftNumber = leftConvertible!.ToInt64(CultureInfo.InvariantCulture);
                        float rightNumber = rightConvertible!.ToSingle(CultureInfo.InvariantCulture);

                        return leftNumber + rightNumber;
                    }
                    else if (left is uint or ulong && right is float)
                    {
                        ulong leftNumber = leftConvertible!.ToUInt64(CultureInfo.InvariantCulture);
                        float rightNumber = rightConvertible!.ToSingle(CultureInfo.InvariantCulture);

                        return leftNumber + rightNumber;
                    }
                    else if (left is int or long && right is double)
                    {
                        long leftNumber = leftConvertible!.ToInt64(CultureInfo.InvariantCulture);
                        double rightNumber = rightConvertible!.ToDouble(CultureInfo.InvariantCulture);

                        return leftNumber + rightNumber;
                    }
                    else if (left is uint or ulong && right is double)
                    {
                        ulong leftNumber = leftConvertible!.ToUInt64(CultureInfo.InvariantCulture);
                        double rightNumber = rightConvertible!.ToDouble(CultureInfo.InvariantCulture);

                        return leftNumber + rightNumber;
                    }
                    else if (left is float && right is int or long)
                    {
                        float leftNumber = leftConvertible!.ToSingle(CultureInfo.InvariantCulture);
                        long rightNumber = rightConvertible!.ToInt64(CultureInfo.InvariantCulture);

                        return leftNumber + rightNumber;
                    }
                    else if (left is double && right is int or long)
                    {
                        double leftNumber = leftConvertible!.ToDouble(CultureInfo.InvariantCulture);
                        long rightNumber = rightConvertible!.ToInt64(CultureInfo.InvariantCulture);

                        return leftNumber + rightNumber;
                    }
                    else if (left is float && right is uint or ulong)
                    {
                        float leftNumber = leftConvertible!.ToSingle(CultureInfo.InvariantCulture);
                        ulong rightNumber = rightConvertible!.ToUInt64(CultureInfo.InvariantCulture);

                        return leftNumber + rightNumber;
                    }
                    else if (left is double && right is uint or ulong)
                    {
                        double leftNumber = leftConvertible!.ToDouble(CultureInfo.InvariantCulture);
                        ulong rightNumber = rightConvertible!.ToUInt64(CultureInfo.InvariantCulture);

                        return leftNumber + rightNumber;
                    }
                    else if (left is int && right is int)
                    {
                        int leftInt = (int)left;
                        int rightInt = (int)right;

                        return leftInt + rightInt;
                    }
                    else if (left is uint && right is uint)
                    {
                        uint leftUint = (uint)left;
                        uint rightUint = (uint)right;

                        return leftUint + rightUint;
                    }
                    else if (left is uint or ulong && right is uint or ulong)
                    {
                        ulong leftUlong = leftConvertible!.ToUInt64(CultureInfo.InvariantCulture);
                        ulong rightUlong = rightConvertible!.ToUInt64(CultureInfo.InvariantCulture);

                        return leftUlong + rightUlong;
                    }
                    else if (left is int or long or uint && right is int or long or uint)
                    {
                        long leftLong = leftConvertible!.ToInt64(CultureInfo.InvariantCulture);
                        long rightLong = rightConvertible!.ToInt64(CultureInfo.InvariantCulture);

                        return leftLong + rightLong;
                    }
                    else if (left is BigInteger && right is BigInteger)
                    {
                        var leftBigInt = (BigInteger)left;
                        var rightBigInt = (BigInteger)right;

                        return leftBigInt + rightBigInt;
                    }
                    else if (left is int or long && right is BigInteger)
                    {
                        long leftNumber = leftConvertible!.ToInt64(CultureInfo.InvariantCulture);
                        var rightBigInt = (BigInteger)right;

                        return leftNumber + rightBigInt;
                    }
                    else if (left is uint or ulong && right is BigInteger)
                    {
                        ulong leftNumber = leftConvertible!.ToUInt64(CultureInfo.InvariantCulture);
                        var rightBigInt = (BigInteger)right;

                        return leftNumber + rightBigInt;
                    }
                    else if (left is BigInteger && right is int or long)
                    {
                        var leftBigInt = (BigInteger)left;
                        long rightNumber = rightConvertible!.ToInt64(CultureInfo.InvariantCulture);

                        return leftBigInt + rightNumber;
                    }
                    else if (left is BigInteger && right is uint or ulong)
                    {
                        var leftBigInt = (BigInteger)left;
                        ulong rightNumber = rightConvertible!.ToUInt64(CultureInfo.InvariantCulture);

                        return leftBigInt + rightNumber;
                    }
                    else
                    {
                        // Should in practice never get here, since TypeResolver should catch these at compile-time. If
                        // it ever happens, it indicates an error somewhere.
                        string message = InterpreterMessages.UnsupportedOperandTypes(expr.Operator.Type, left, right);
                        throw new RuntimeError(expr.Operator, message);
                    }

                case PLUS_EQUAL:
                    CheckNumberOperands(expr.Operator, left, right);

                    // The method returns the value to be assigned. The assignment takes place elsewhere.
                    // Bang operator (!) is safe because of the CheckNumberOperands() call above.
                    if (left is float && right is float or double)
                    {
                        // Note: `rightNumber` might loose precision here because of conversion from `double` to
                        // `single`. This is all fine and matches the semantics of other languages perfectly. :-)
                        float leftNumber = leftConvertible!.ToSingle(CultureInfo.InvariantCulture);
                        float rightNumber = rightConvertible!.ToSingle(CultureInfo.InvariantCulture);

                        return leftNumber + rightNumber;
                    }
                    else if (left is double && right is float or double)
                    {
                        double leftNumber = leftConvertible!.ToDouble(CultureInfo.InvariantCulture);
                        double rightNumber = rightConvertible!.ToDouble(CultureInfo.InvariantCulture);

                        return leftNumber + rightNumber;
                    }
                    /* left is int or long && right is float or double -- deliberately unsupported, since it would lose fractional part */
                    /* left is uint or ulong && right is float or double -- likewise */
                    else if (left is float && right is int or long)
                    {
                        float leftNumber = leftConvertible!.ToSingle(CultureInfo.InvariantCulture);
                        long rightNumber = rightConvertible!.ToInt64(CultureInfo.InvariantCulture);

                        return leftNumber + rightNumber;
                    }
                    else if (left is double && right is int or long)
                    {
                        double leftNumber = leftConvertible!.ToDouble(CultureInfo.InvariantCulture);
                        long rightNumber = rightConvertible!.ToInt64(CultureInfo.InvariantCulture);

                        return leftNumber + rightNumber;
                    }
                    else if (left is float && right is uint or ulong)
                    {
                        float leftNumber = leftConvertible!.ToSingle(CultureInfo.InvariantCulture);
                        ulong rightNumber = rightConvertible!.ToUInt64(CultureInfo.InvariantCulture);

                        return leftNumber + rightNumber;
                    }
                    else if (left is double && right is uint or ulong)
                    {
                        double leftNumber = leftConvertible!.ToDouble(CultureInfo.InvariantCulture);
                        ulong rightNumber = rightConvertible!.ToUInt64(CultureInfo.InvariantCulture);

                        return leftNumber + rightNumber;
                    }
                    else if (left is int && right is int)
                    {
                        int leftNumber = leftConvertible!.ToInt32(CultureInfo.InvariantCulture);
                        int rightNumber = rightConvertible!.ToInt32(CultureInfo.InvariantCulture);

                        return leftNumber + rightNumber;
                    }
                    else if (left is uint && right is uint)
                    {
                        uint leftNumber = leftConvertible!.ToUInt32(CultureInfo.InvariantCulture);
                        uint rightNumber = rightConvertible!.ToUInt32(CultureInfo.InvariantCulture);

                        return leftNumber + rightNumber;
                    }
                    else if (left is uint or ulong && right is uint or ulong)
                    {
                        ulong leftNumber = leftConvertible!.ToUInt64(CultureInfo.InvariantCulture);
                        ulong rightNumber = rightConvertible!.ToUInt64(CultureInfo.InvariantCulture);

                        return leftNumber + rightNumber;
                    }
                    else if (left is int or long or uint && right is int or long or uint)
                    {
                        long leftNumber = leftConvertible!.ToInt64(CultureInfo.InvariantCulture);
                        long rightNumber = rightConvertible!.ToInt64(CultureInfo.InvariantCulture);

                        return leftNumber + rightNumber;
                    }
                    else if (left is BigInteger && right is BigInteger)
                    {
                        var leftBigInt = (BigInteger)left;
                        var rightBigInt = (BigInteger)right;

                        return leftBigInt + rightBigInt;
                    }
                    else if (left is int or long && right is BigInteger)
                    {
                        long leftNumber = leftConvertible!.ToInt64(CultureInfo.InvariantCulture);
                        var rightBigInt = (BigInteger)right;

                        return leftNumber + rightBigInt;
                    }
                    else if (left is uint or ulong && right is BigInteger)
                    {
                        ulong leftNumber = leftConvertible!.ToUInt64(CultureInfo.InvariantCulture);
                        var rightBigInt = (BigInteger)right;

                        return leftNumber + rightBigInt;
                    }
                    else if (left is BigInteger && right is int or long)
                    {
                        var leftBigInt = (BigInteger)left;
                        long rightNumber = rightConvertible!.ToInt64(CultureInfo.InvariantCulture);

                        return leftBigInt + rightNumber;
                    }
                    else if (left is BigInteger && right is uint or ulong)
                    {
                        var leftBigInt = (BigInteger)left;
                        ulong rightNumber = rightConvertible!.ToUInt64(CultureInfo.InvariantCulture);

                        return leftBigInt + rightNumber;
                    }
                    else
                    {
                        // Should in practice never get here, since TypeResolver should catch these at compile-time. If
                        // it ever happens, it indicates an error somewhere.
                        string message = InterpreterMessages.UnsupportedOperandTypes(expr.Operator.Type, left, right);
                        throw new RuntimeError(expr.Operator, message);
                    }

                case SLASH:
                    CheckNumberOperands(expr.Operator, left, right);

                    // Bang operator (!) is safe because of the CheckNumberOperands() call above.
                    if (left is float && right is float)
                    {
                        float leftNumber = leftConvertible!.ToSingle(CultureInfo.InvariantCulture);
                        float rightNumber = rightConvertible!.ToSingle(CultureInfo.InvariantCulture);

                        return leftNumber / rightNumber;
                    }
                    else if (left is float or double && right is float or double)
                    {
                        double leftNumber = leftConvertible!.ToDouble(CultureInfo.InvariantCulture);
                        double rightNumber = rightConvertible!.ToDouble(CultureInfo.InvariantCulture);

                        return leftNumber / rightNumber;
                    }
                    else if (left is float && right is int or long)
                    {
                        float leftNumber = leftConvertible!.ToSingle(CultureInfo.InvariantCulture);
                        long rightNumber = rightConvertible!.ToInt64(CultureInfo.InvariantCulture);

                        return leftNumber / rightNumber;
                    }
                    else if (left is double && right is int or long)
                    {
                        double leftNumber = leftConvertible!.ToDouble(CultureInfo.InvariantCulture);
                        long rightNumber = rightConvertible!.ToInt64(CultureInfo.InvariantCulture);

                        return leftNumber / rightNumber;
                    }
                    else if (left is float && right is uint or ulong)
                    {
                        float leftNumber = leftConvertible!.ToSingle(CultureInfo.InvariantCulture);
                        ulong rightNumber = rightConvertible!.ToUInt64(CultureInfo.InvariantCulture);

                        return leftNumber / rightNumber;
                    }
                    else if (left is double && right is uint or ulong)
                    {
                        double leftNumber = leftConvertible!.ToDouble(CultureInfo.InvariantCulture);
                        ulong rightNumber = rightConvertible!.ToUInt64(CultureInfo.InvariantCulture);

                        return leftNumber / rightNumber;
                    }
                    else if (left is int or long && right is float)
                    {
                        long leftNumber = leftConvertible!.ToInt64(CultureInfo.InvariantCulture);
                        float rightNumber = rightConvertible!.ToSingle(CultureInfo.InvariantCulture);

                        return leftNumber / rightNumber;
                    }
                    else if (left is int or long && right is double)
                    {
                        long leftNumber = leftConvertible!.ToInt64(CultureInfo.InvariantCulture);
                        double rightNumber = rightConvertible!.ToDouble(CultureInfo.InvariantCulture);

                        return leftNumber / rightNumber;
                    }
                    else if (left is uint or ulong && right is float)
                    {
                        ulong leftNumber = leftConvertible!.ToUInt64(CultureInfo.InvariantCulture);
                        float rightNumber = rightConvertible!.ToSingle(CultureInfo.InvariantCulture);

                        return leftNumber / rightNumber;
                    }
                    else if (left is uint or ulong && right is double)
                    {
                        ulong leftNumber = leftConvertible!.ToUInt64(CultureInfo.InvariantCulture);
                        double rightNumber = rightConvertible!.ToDouble(CultureInfo.InvariantCulture);

                        return leftNumber / rightNumber;
                    }
                    else if (left is int && right is int)
                    {
                        int leftNumber = leftConvertible!.ToInt32(CultureInfo.InvariantCulture);
                        int rightNumber = rightConvertible!.ToInt32(CultureInfo.InvariantCulture);

                        return leftNumber / rightNumber;
                    }
                    else if (left is uint && right is uint)
                    {
                        uint leftNumber = leftConvertible!.ToUInt32(CultureInfo.InvariantCulture);
                        uint rightNumber = rightConvertible!.ToUInt32(CultureInfo.InvariantCulture);

                        return leftNumber / rightNumber;
                    }
                    else if (left is uint or ulong && right is uint or ulong)
                    {
                        ulong leftNumber = leftConvertible!.ToUInt64(CultureInfo.InvariantCulture);
                        ulong rightNumber = rightConvertible!.ToUInt64(CultureInfo.InvariantCulture);

                        return leftNumber / rightNumber;
                    }
                    else if (left is int or long or uint && right is int or long or uint)
                    {
                        long leftNumber = leftConvertible!.ToInt64(CultureInfo.InvariantCulture);
                        long rightNumber = rightConvertible!.ToInt64(CultureInfo.InvariantCulture);

                        return leftNumber / rightNumber;
                    }
                    else if (left is BigInteger && right is BigInteger)
                    {
                        var leftBigInt = (BigInteger)left;
                        var rightBigInt = (BigInteger)right;

                        return leftBigInt / rightBigInt;
                    }
                    else if (left is int or long && right is BigInteger)
                    {
                        long leftNumber = leftConvertible!.ToInt64(CultureInfo.InvariantCulture);
                        var rightBigInt = (BigInteger)right;

                        return leftNumber / rightBigInt;
                    }
                    else if (left is uint or ulong && right is BigInteger)
                    {
                        ulong leftNumber = leftConvertible!.ToUInt64(CultureInfo.InvariantCulture);
                        var rightBigInt = (BigInteger)right;

                        return leftNumber / rightBigInt;
                    }
                    else if (left is BigInteger && right is int or long)
                    {
                        var leftBigInt = (BigInteger)left;
                        long rightNumber = rightConvertible!.ToInt64(CultureInfo.InvariantCulture);

                        return leftBigInt / rightNumber;
                    }
                    else if (left is BigInteger && right is uint or ulong)
                    {
                        var leftBigInt = (BigInteger)left;
                        ulong rightNumber = rightConvertible!.ToUInt64(CultureInfo.InvariantCulture);

                        return leftBigInt / rightNumber;
                    }
                    else
                    {
                        // Should in practice never get here, since TypeResolver should catch these at compile-time. If
                        // it ever happens, it indicates an error somewhere.
                        string message = InterpreterMessages.UnsupportedOperandTypes(expr.Operator.Type, left, right);
                        throw new RuntimeError(expr.Operator, message);
                    }

                case STAR:
                    CheckNumberOperands(expr.Operator, left, right);

                    // Bang operator (!) is safe because of the CheckNumberOperands() call above.
                    if (left is float && right is float)
                    {
                        float leftNumber = leftConvertible!.ToSingle(CultureInfo.InvariantCulture);
                        float rightNumber = rightConvertible!.ToSingle(CultureInfo.InvariantCulture);

                        return leftNumber * rightNumber;
                    }
                    else if (left is float or double && right is float or double)
                    {
                        double leftNumber = leftConvertible!.ToDouble(CultureInfo.InvariantCulture);
                        double rightNumber = rightConvertible!.ToDouble(CultureInfo.InvariantCulture);

                        return leftNumber * rightNumber;
                    }
                    else if (left is float && right is int or long)
                    {
                        float leftNumber = leftConvertible!.ToSingle(CultureInfo.InvariantCulture);
                        long rightNumber = rightConvertible!.ToInt64(CultureInfo.InvariantCulture);

                        return leftNumber * rightNumber;
                    }
                    else if (left is double && right is int or long)
                    {
                        double leftNumber = leftConvertible!.ToDouble(CultureInfo.InvariantCulture);
                        long rightNumber = rightConvertible!.ToInt64(CultureInfo.InvariantCulture);

                        return leftNumber * rightNumber;
                    }
                    else if (left is float && right is uint or ulong)
                    {
                        float leftNumber = leftConvertible!.ToSingle(CultureInfo.InvariantCulture);
                        ulong rightNumber = rightConvertible!.ToUInt64(CultureInfo.InvariantCulture);

                        return leftNumber * rightNumber;
                    }
                    else if (left is double && right is uint or ulong)
                    {
                        double leftNumber = leftConvertible!.ToDouble(CultureInfo.InvariantCulture);
                        ulong rightNumber = rightConvertible!.ToUInt64(CultureInfo.InvariantCulture);

                        return leftNumber * rightNumber;
                    }
                    else if (left is int or long && right is float)
                    {
                        long leftNumber = leftConvertible!.ToInt64(CultureInfo.InvariantCulture);
                        float rightNumber = rightConvertible!.ToSingle(CultureInfo.InvariantCulture);

                        return leftNumber * rightNumber;
                    }
                    else if (left is int or long && right is double)
                    {
                        long leftNumber = leftConvertible!.ToInt64(CultureInfo.InvariantCulture);
                        double rightNumber = rightConvertible!.ToDouble(CultureInfo.InvariantCulture);

                        return leftNumber * rightNumber;
                    }
                    else if (left is uint or ulong && right is float)
                    {
                        ulong leftNumber = leftConvertible!.ToUInt64(CultureInfo.InvariantCulture);
                        float rightNumber = rightConvertible!.ToSingle(CultureInfo.InvariantCulture);

                        return leftNumber * rightNumber;
                    }
                    else if (left is uint or ulong && right is double)
                    {
                        ulong leftNumber = leftConvertible!.ToUInt64(CultureInfo.InvariantCulture);
                        double rightNumber = rightConvertible!.ToDouble(CultureInfo.InvariantCulture);

                        return leftNumber * rightNumber;
                    }
                    else if (left is int && right is int)
                    {
                        int leftNumber = leftConvertible!.ToInt32(CultureInfo.InvariantCulture);
                        int rightNumber = rightConvertible!.ToInt32(CultureInfo.InvariantCulture);

                        return leftNumber * rightNumber;
                    }
                    else if (left is uint && right is uint)
                    {
                        uint leftNumber = leftConvertible!.ToUInt32(CultureInfo.InvariantCulture);
                        uint rightNumber = rightConvertible!.ToUInt32(CultureInfo.InvariantCulture);

                        return leftNumber * rightNumber;
                    }
                    else if (left is uint or ulong && right is uint or ulong)
                    {
                        ulong leftNumber = leftConvertible!.ToUInt64(CultureInfo.InvariantCulture);
                        ulong rightNumber = rightConvertible!.ToUInt64(CultureInfo.InvariantCulture);

                        return leftNumber * rightNumber;
                    }
                    else if (left is int or long or uint && right is int or long or uint)
                    {
                        long leftNumber = leftConvertible!.ToInt64(CultureInfo.InvariantCulture);
                        long rightNumber = rightConvertible!.ToInt64(CultureInfo.InvariantCulture);

                        return leftNumber * rightNumber;
                    }
                    else if (left is BigInteger && right is BigInteger)
                    {
                        var leftBigInt = (BigInteger)left;
                        var rightBigInt = (BigInteger)right;

                        return leftBigInt * rightBigInt;
                    }
                    else if (left is int or long && right is BigInteger)
                    {
                        long leftNumber = leftConvertible!.ToInt64(CultureInfo.InvariantCulture);
                        var rightBigInt = (BigInteger)right;

                        return leftNumber * rightBigInt;
                    }
                    else if (left is uint or ulong && right is BigInteger)
                    {
                        ulong leftNumber = leftConvertible!.ToUInt64(CultureInfo.InvariantCulture);
                        var rightBigInt = (BigInteger)right;

                        return leftNumber * rightBigInt;
                    }
                    else if (left is BigInteger && right is int or long)
                    {
                        var leftBigInt = (BigInteger)left;
                        long rightNumber = rightConvertible!.ToInt64(CultureInfo.InvariantCulture);

                        return leftBigInt * rightNumber;
                    }
                    else if (left is BigInteger && right is uint or ulong)
                    {
                        var leftBigInt = (BigInteger)left;
                        ulong rightNumber = rightConvertible!.ToUInt64(CultureInfo.InvariantCulture);

                        return leftBigInt * rightNumber;
                    }
                    else
                    {
                        // Should in practice never get here, since TypeResolver should catch these at compile-time. If
                        // it ever happens, it indicates an error somewhere.
                        string message = InterpreterMessages.UnsupportedOperandTypes(expr.Operator.Type, left, right);
                        throw new RuntimeError(expr.Operator, message);
                    }

                case STAR_STAR:
                    CheckNumberOperands(expr.Operator, left, right);

                    if ((left is float or double && right is float or double) ||
                        (left is float or double && right is int or long or uint or ulong) ||
                        (left is int or long or uint or ulong && right is float or double))
                    {
                        double value = leftConvertible!.ToDouble(CultureInfo.InvariantCulture);
                        double exponent = rightConvertible!.ToDouble(CultureInfo.InvariantCulture);

                        return Math.Pow(value, exponent);
                    }
                    else if (left is int or long && right is int)
                    {
                        long value = leftConvertible!.ToInt64(CultureInfo.InvariantCulture);

                        // The right-hand operand cannot be 64-bit in this case because of limitations in
                        // BigInteger.Pow()
                        //
                        // Also note that this will throw a run-time exception on negative exponents, because this is
                        // unsupported by `BigInteger.Pow()`. The alternative would be to use `Math.Pow()` with `double`
                        // operands (which supports negative exponents, returning fractional results). _But_, doing so
                        // for `int/long`+`int` operands means that we would have a nasty implicit conversion from
                        // `long` to `double` which potentially loses precision. This is one of the cases where we do
                        // not want to inherit the C# semantics but instead be more "strict" to avoid unpleasant
                        // surprises to the user.
                        int exponent = (int)right;

                        // TODO: If we start doing compile-time evaluation of constant expressions, we should use
                        // TODO: Expr.TypeReference here to check if the result should be narrowed down. For example,
                        // TODO: `2 ** 10` can be stored without precision loss in both an `int` or a `uint`. We
                        // TODO: shouldn't enforce the usage of BigInteger when we don't have to.
                        return BigInteger.Pow(value, exponent);
                    }
                    else if (left is uint or ulong && right is int)
                    {
                        ulong value = leftConvertible!.ToUInt64(CultureInfo.InvariantCulture);
                        int exponent = (int)right;

                        return BigInteger.Pow(value, exponent);
                    }
                    else if (left is BigInteger value && right is int)
                    {
                        // The right-hand operand cannot be 64-bit in this case because of limitations in
                        // BigInteger.Pow()
                        int exponent = (int)right;

                        return BigInteger.Pow(value, exponent);
                    }
                    else
                    {
                        // Should in practice never get here, since TypeResolver should catch these at compile-time. If
                        // it ever happens, it indicates an error somewhere.
                        string message = InterpreterMessages.UnsupportedOperandTypes(expr.Operator.Type, left, right);
                        throw new RuntimeError(expr.Operator, message);
                    }

                case PERCENT:
                    CheckNumberOperands(expr.Operator, left, right);

                    // Bang operator (!) is safe because of the CheckNumberOperands() call above.
                    if (left is float && right is float)
                    {
                        float leftNumber = leftConvertible!.ToSingle(CultureInfo.InvariantCulture);
                        float rightNumber = rightConvertible!.ToSingle(CultureInfo.InvariantCulture);

                        return leftNumber % rightNumber;
                    }
                    else if (left is float or double && right is float or double)
                    {
                        double leftNumber = leftConvertible!.ToDouble(CultureInfo.InvariantCulture);
                        double rightNumber = rightConvertible!.ToDouble(CultureInfo.InvariantCulture);

                        return leftNumber % rightNumber;
                    }
                    else if (left is int or long && right is float)
                    {
                        long leftNumber = leftConvertible!.ToInt64(CultureInfo.InvariantCulture);
                        float rightNumber = rightConvertible!.ToSingle(CultureInfo.InvariantCulture);

                        return leftNumber % rightNumber;
                    }
                    else if (left is int or long && right is double)
                    {
                        long leftNumber = leftConvertible!.ToInt64(CultureInfo.InvariantCulture);
                        double rightNumber = rightConvertible!.ToDouble(CultureInfo.InvariantCulture);

                        return leftNumber % rightNumber;
                    }
                    else if (left is uint or ulong && right is float)
                    {
                        ulong leftNumber = leftConvertible!.ToUInt64(CultureInfo.InvariantCulture);
                        float rightNumber = rightConvertible!.ToSingle(CultureInfo.InvariantCulture);

                        return leftNumber % rightNumber;
                    }
                    else if (left is uint or ulong && right is double)
                    {
                        ulong leftNumber = leftConvertible!.ToUInt64(CultureInfo.InvariantCulture);
                        double rightNumber = rightConvertible!.ToDouble(CultureInfo.InvariantCulture);

                        return leftNumber % rightNumber;
                    }
                    else if (left is float && right is int or long)
                    {
                        float leftNumber = leftConvertible!.ToSingle(CultureInfo.InvariantCulture);
                        long rightNumber = rightConvertible!.ToInt64(CultureInfo.InvariantCulture);

                        return leftNumber % rightNumber;
                    }
                    else if (left is double && right is int or long)
                    {
                        double leftNumber = leftConvertible!.ToDouble(CultureInfo.InvariantCulture);
                        long rightNumber = rightConvertible!.ToInt64(CultureInfo.InvariantCulture);

                        return leftNumber % rightNumber;
                    }
                    else if (left is float && right is uint or ulong)
                    {
                        float leftNumber = leftConvertible!.ToSingle(CultureInfo.InvariantCulture);
                        ulong rightNumber = rightConvertible!.ToUInt64(CultureInfo.InvariantCulture);

                        return leftNumber % rightNumber;
                    }
                    else if (left is double && right is uint or ulong)
                    {
                        double leftNumber = leftConvertible!.ToDouble(CultureInfo.InvariantCulture);
                        ulong rightNumber = rightConvertible!.ToUInt64(CultureInfo.InvariantCulture);

                        return leftNumber % rightNumber;
                    }
                    else if (left is int && right is int)
                    {
                        int leftNumber = leftConvertible!.ToInt32(CultureInfo.InvariantCulture);
                        int rightNumber = rightConvertible!.ToInt32(CultureInfo.InvariantCulture);

                        return leftNumber % rightNumber;
                    }
                    else if (left is uint && right is uint)
                    {
                        uint leftNumber = leftConvertible!.ToUInt32(CultureInfo.InvariantCulture);
                        uint rightNumber = rightConvertible!.ToUInt32(CultureInfo.InvariantCulture);

                        return leftNumber % rightNumber;
                    }
                    else if (left is uint or ulong && right is uint or ulong)
                    {
                        ulong leftNumber = leftConvertible!.ToUInt64(CultureInfo.InvariantCulture);
                        ulong rightNumber = rightConvertible!.ToUInt64(CultureInfo.InvariantCulture);

                        return leftNumber % rightNumber;
                    }
                    else if (left is int or long or uint && right is int or long or uint)
                    {
                        long leftNumber = leftConvertible!.ToInt64(CultureInfo.InvariantCulture);
                        long rightNumber = rightConvertible!.ToInt64(CultureInfo.InvariantCulture);

                        return leftNumber % rightNumber;
                    }
                    else if (left is BigInteger && right is BigInteger)
                    {
                        var leftBigInt = (BigInteger)left;
                        var rightBigInt = (BigInteger)right;

                        return leftBigInt % rightBigInt;
                    }
                    else if (left is int or long && right is BigInteger)
                    {
                        long leftNumber = leftConvertible!.ToInt64(CultureInfo.InvariantCulture);
                        var rightBigInt = (BigInteger)right;

                        return leftNumber % rightBigInt;
                    }
                    else if (left is uint or ulong && right is BigInteger)
                    {
                        ulong leftNumber = leftConvertible!.ToUInt64(CultureInfo.InvariantCulture);
                        var rightBigInt = (BigInteger)right;

                        return leftNumber % rightBigInt;
                    }
                    else if (left is BigInteger && right is int or long)
                    {
                        var leftBigInt = (BigInteger)left;
                        long rightNumber = rightConvertible!.ToInt64(CultureInfo.InvariantCulture);

                        return leftBigInt % rightNumber;
                    }
                    else if (left is BigInteger && right is uint or ulong)
                    {
                        var leftBigInt = (BigInteger)left;
                        ulong rightNumber = rightConvertible!.ToUInt64(CultureInfo.InvariantCulture);

                        return leftBigInt % rightNumber;
                    }
                    else
                    {
                        // Should in practice never get here, since TypeResolver should catch these at compile-time. If
                        // it ever happens, it indicates an error somewhere.
                        string message = InterpreterMessages.UnsupportedOperandTypes(expr.Operator.Type, left, right);
                        throw new RuntimeError(expr.Operator, message);
                    }

                case LESS_LESS:
                    CheckNumberOperands(expr.Operator, left, right);

                    if (left is int && right is int)
                    {
                        int leftNumber = (int)left;
                        int rightNumber = (int)right;

                        return leftNumber << rightNumber;
                    }
                    else if (left is uint && right is int)
                    {
                        uint leftUint = (uint)left;
                        int rightInt = (int)right;

                        return leftUint << rightInt;
                    }
                    else if (left is long && right is int)
                    {
                        long leftLong = (long)left;
                        int rightInt = (int)right;

                        return leftLong << rightInt;
                    }
                    else if (left is ulong && right is int)
                    {
                        ulong leftLong = (ulong)left;
                        int rightInt = (int)right;

                        return leftLong << rightInt;
                    }
                    else if (left is BigInteger && right is int)
                    {
                        var leftBigInt = (BigInteger)left;
                        var rightInt = (int)right;

                        return leftBigInt << rightInt;
                    }
                    else
                    {
                        // Should in practice never get here, since TypeResolver should catch these at compile-time. If
                        // it ever happens, it indicates an error somewhere.
                        string message = InterpreterMessages.UnsupportedOperandTypes(expr.Operator.Type, left, right);
                        throw new RuntimeError(expr.Operator, message);
                    }

                case GREATER_GREATER:
                    CheckNumberOperands(expr.Operator, left, right);

                    if (left is int && right is int)
                    {
                        int leftNumber = (int)left;
                        int rightNumber = (int)right;

                        return leftNumber >> rightNumber;
                    }
                    else if (left is uint && right is int)
                    {
                        uint leftUint = (uint)left;
                        int rightInt = (int)right;

                        return leftUint >> rightInt;
                    }
                    else if (left is long && right is int)
                    {
                        long leftLong = (long)left;
                        int rightInt = (int)right;

                        return leftLong >> rightInt;
                    }
                    else if (left is ulong && right is int)
                    {
                        ulong leftLong = (ulong)left;
                        int rightInt = (int)right;

                        return leftLong >> rightInt;
                    }
                    else if (left is BigInteger && right is int)
                    {
                        var leftBigInt = (BigInteger)left;
                        var rightInt = (int)right;

                        return leftBigInt >> rightInt;
                    }
                    else
                    {
                        // Should in practice never get here, since TypeResolver should catch these at compile-time. If
                        // it ever happens, it indicates an error somewhere.
                        string message = InterpreterMessages.UnsupportedOperandTypes(expr.Operator.Type, left, right);
                        throw new RuntimeError(expr.Operator, message);
                    }

                default:
                {
                    string message = InterpreterMessages.UnsupportedOperatorTypeInBinaryExpression(expr.Operator.Type);
                    throw new RuntimeError(expr.Operator, message);
                }
            }
        }

        public object? VisitCallExpr(Expr.Call expr)
        {
            object? callee = Evaluate(expr.Callee);

            var arguments = new List<object>();

            foreach (Expr argument in expr.Arguments)
            {
                arguments.Add(Evaluate(argument)!);
            }

            switch (callee)
            {
                case ICallable callable:
                    if (arguments.Count != callable.Arity())
                    {
                        throw new RuntimeError(
                            expr.Paren,
                            "Expected " + callable.Arity() + " argument(s) but got " + arguments.Count + "."
                        );
                    }

                    try
                    {
                        return callable.Call(this, arguments);
                    }
                    catch (RuntimeError)
                    {
                        // This kind of exception already has the most-adjacent token available in it already, to
                        // provide the approximate source location in the error message.
                        throw;
                    }
                    catch (Exception e)
                    {
                        if (expr.Callee is Expr.Identifier identifier)
                        {
                            throw new RuntimeError(identifier.Name, $"{identifier.Name.Lexeme}: {e.Message}");
                        }
                        else
                        {
                            throw new RuntimeError(expr.Paren, e.Message);
                        }
                    }

                case TargetAndMethodContainer container:
                    if (expr.Callee is Expr.Get)
                    {
                        return container.Method.Invoke(container.Target, arguments.ToArray());
                    }
                    else
                    {
                        throw new RuntimeError(expr.Paren, $"Internal error: Expected Get expression, not {expr.Callee}.");
                    }

                default:
                    throw new RuntimeError(expr.Paren, $"Can only call functions, classes and native methods, not {callee}.");
            }
        }

        public object? VisitIndexExpr(Expr.Index expr)
        {
            object? indexee = Evaluate(expr.Indexee);

            if (indexee == null)
            {
                throw new RuntimeError(expr.ClosingBracket, $"'null' reference cannot be indexed.");
            }

            var argument = Evaluate(expr.Argument);

            if (argument == null)
            {
                throw new RuntimeError(expr.ClosingBracket, "Cannot index by 'null' key");
            }

            // TODO: This approach is rather hardwired and ugly. We should probably try to detect the `this` method or
            // TODO: something like that instead, to support all kind of indexable types in the CLR. OTOH, we might not
            // TODO: want to couple ourselves too tightly to the .NET type system...
            if (indexee is IDictionary dictionary)
            {
                return dictionary[argument];
            }
            else if (indexee is string s && argument is int i)
            {
                return s[i];
            }
            else if (indexee is AsciiString asciiString && argument is int j)
            {
                // TODO: Why is j an `int` here? Needs to be investigated.
                return asciiString[(nuint)j];
            }
            else
            {
                // TODO: Try to make about 99.9% of the scenarios that would trigger this be compile-time errors instead
                // TODO: of runtime exceptions.
                throw new RuntimeError(expr.ClosingBracket, $"Indexing {indexee.GetType().ToTypeKeyword()} by {argument.GetType().ToTypeKeyword()} is not supported.");
            }
        }

        private static object? ExpandIntegerIfRequired(object? value, ITypeReference targetTypeReference)
        {
            if (IsValidNumberType(value) && targetTypeReference.IsResolved)
            {
                if (value!.GetType() == targetTypeReference.ClrType)
                {
                    // Avoid conversions when the value is already of the correct type.
                    return value;
                }
                else if (targetTypeReference.ClrType == typeof(Int32))
                {
                    return Convert.ToInt32(value);
                }
                else if (targetTypeReference.ClrType == typeof(UInt32))
                {
                    return Convert.ToUInt32(value);
                }
                else if (targetTypeReference.ClrType == typeof(Int64))
                {
                    return Convert.ToInt64(value);
                }
                else if (targetTypeReference.ClrType == typeof(Single))
                {
                    return Convert.ToSingle(value);
                }
                else if (targetTypeReference.ClrType == typeof(Double))
                {
                    return Convert.ToDouble(value);
                }
                else if (targetTypeReference.ClrType == typeof(BigInteger))
                {
                    return value switch
                    {
                        int intValue => new BigInteger(intValue),
                        long longValue => new BigInteger(longValue),
                        float floatValue => new BigInteger(floatValue),
                        double doubleValue => new BigInteger(doubleValue),

                        // TODO: Might need to revisit this to support more types as we implement #70.
                        _ => throw new IllegalStateException($"Unsupported conversion from {value.GetType().ToTypeKeyword()} to {targetTypeReference.ClrType.ToTypeKeyword()}")
                    };
                }
                else
                {
                    throw new IllegalStateException($"Unsupported target type {targetTypeReference.ClrType.ToTypeKeyword()}");
                }
            }

            return value;
        }
    }
}
