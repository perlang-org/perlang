// This class contains a number of these violations. While slightly ugly, it does make the code a bit more dense and
// arguably, readable. I'm not fully convinced that adding the braces in this _particular case_ makes the code better.
// Some form of switch expression-based solution would probably be good enough to convince me; feel free to give it a
// try and send a PR.

#pragma warning disable SA1010
#pragma warning disable SA1117
#pragma warning disable SA1503
#pragma warning disable S1117

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Perlang.Exceptions;
using Perlang.Internal.Extensions;
using static Perlang.Internal.Utils;
using static Perlang.TokenType;

namespace Perlang.Parser
{
    // Convoluted class name to avoid conflict with namespace

    /// <summary>
    /// Parses Perlang code to either a list of statements or a single expression.
    ///
    /// This class is not thread safe; a single instance of the class can not be safely used from multiple threads
    /// simultaneously.
    /// </summary>
    public class PerlangParser
    {
        private readonly bool allowSemicolonElision;

        private bool allowExpression;
        private bool foundExpression = false;

        [SuppressMessage("SonarAnalyzer.CSharp", "S3871", Justification = "Exception is not propagated outside class")]
        private class InternalParseError : Exception
        {
            public ParseErrorType? ParseErrorType { get; }

            public InternalParseError(ParseErrorType? parseErrorType)
            {
                ParseErrorType = parseErrorType;
            }
        }

        private readonly ParseErrorHandler parseErrorHandler;
        private readonly List<Token> tokens;

        private readonly List<Token> cppPrototypes = new List<Token>();
        private readonly List<Token> cppMethods = new List<Token>();

        private int current;

        /// <summary>
        /// Scans and parses the given program, to prepare for execution or inspection.
        ///
        /// This method is useful for inspecting the AST or perform other validation of the internal state after
        /// parsing a given program. It is also used for interpreting the expressions/statements, or compiling it to
        /// executable form.
        /// </summary>
        /// <param name="sourceFiles">The source code to a Perlang program, consisting of one or more source files.</param>
        /// <param name="scanErrorHandler">A handler for scanner errors.</param>
        /// <param name="parseErrorHandler">A handler for parse errors.</param>
        /// <param name="replMode">`true` if the program is being executed in REPL mode; otherwise, `false`. REPL mode
        /// implies a more relaxed more where e.g. semicolons are automatically added after each line.</param>
        /// <returns>A <see cref="ScanAndParseResult"/> instance.</returns>
        public static ScanAndParseResult ScanAndParse(
            ImmutableList<SourceFile> sourceFiles,
            ScanErrorHandler scanErrorHandler,
            ParseErrorHandler parseErrorHandler,
            bool replMode = false)
        {
            //
            // Scanning phase
            //

            bool hasScanErrors = false;
            var tokens = new List<Token>();

            foreach (SourceFile sourceFile in sourceFiles) {
                var scanner = new Scanner(sourceFile.FileName, sourceFile.Source, scanError =>
                {
                    hasScanErrors = true;
                    scanErrorHandler(scanError);
                });

                tokens.AddRange(scanner.ScanTokens());

                if (hasScanErrors)
                {
                    // Something went wrong as early as the "scan" stage. Abort the rest of the processing.
                    return ScanAndParseResult.ScanErrorOccurred;
                }
            }

            // This was previously included in the ScanTokens() result, but this didn't work once we started support
            // multiple sourceFiles (because that will cause the Parser to stop when reaching the EOF after the first
            // file, instead of continue processing the other ones)
            tokens.Add(new Token(PERLANG_EOF, String.Empty, literal: null, fileName: String.Empty, line: 0));

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

            (object syntax, List<Token> cppPrototypes, List<Token> cppMethods) = parser.ParseExpressionOrStatements();

            if (hasParseErrors)
            {
                // One or more parse errors were encountered. They have been reported upstream, so we just abort
                // the evaluation at this stage.
                return ScanAndParseResult.ParseErrorEncountered;
            }

            // TODO: Should we return here (and change PrepareForEvalResult to something like ScanAndParseResult) or
            // should we continue with moving more of the code in eval into this method? Given that we want to inspect
            // the result from Resolver, maybe doing more work here would be completely fine...
            if (syntax is Expr expr)
            {
                if (cppPrototypes.Count > 0 || cppMethods.Count > 0)
                {
                    throw new IllegalStateException("C++ code is not supported when evaluating an expression");
                }

                return ScanAndParseResult.OfExpr(expr);
            }
            else if (syntax is List<Stmt> stmts)
            {
                return ScanAndParseResult.OfStmts(stmts, cppPrototypes, cppMethods);
            }
            else
            {
                throw new IllegalStateException($"syntax expected to be Expr or List<Stmt>, not {syntax}");
            }
        }

        public PerlangParser(List<Token> tokens, ParseErrorHandler parseErrorHandler, bool allowSemicolonElision)
        {
            this.parseErrorHandler = parseErrorHandler;
            this.tokens = tokens;
            this.allowSemicolonElision = allowSemicolonElision;
        }

        public IList<Stmt> ParseStatements()
        {
            var statements = new List<Stmt>();

            while (!IsAtEnd)
            {
                statements.Add(Declaration(isExternDeclaration: false));
            }

            return statements;
        }

        public Expr ParseExpression()
        {
            try
            {
                return Expression();
            }
            catch (InternalParseError)
            {
                // Error has already been reported at this point
                return null;
            }
        }

        /// <summary>
        /// Parses the current set of tokens as either a single expression (if the tokens contain exactly one
        /// expression + EOF) or a list of statements.
        ///
        /// If you know from beforehand that the provided code is either an expression or a list of statements, the
        /// <see cref="ParseExpression"/> and <see cref="ParseStatements"/> methods are typically more convenient
        /// to use.
        /// </summary>
        /// <returns>A tuple with an <see cref="Expr"/> or a list of <see cref="Stmt"/> objects, a list of C++ prototypes
        /// and a list of C++ methods.</returns>
        public (object Syntax, List<Token> CppPrototypes, List<Token> CppMethods) ParseExpressionOrStatements()
        {
            allowExpression = true;

            var statements = new List<Stmt>();

            while (!IsAtEnd)
            {
                statements.Add(Declaration(isExternDeclaration: false));

                if (foundExpression)
                {
                    Stmt last = statements.Last();
                    return (((Stmt.ExpressionStmt)last).Expression, cppPrototypes, cppMethods);
                }

                allowExpression = false;
            }

            return (statements, cppPrototypes, cppMethods);
        }

        /// <summary>
        /// Returns the expression at the current position in the stream.
        /// </summary>
        /// <returns>An expression.</returns>
        private Expr Expression()
        {
            return Assignment();
        }

        private Stmt Declaration(bool isExternDeclaration)
        {
            try
            {
                // This is a silly limitation, but for now we enforce a particular token order for various "prefixes"
                // for fields, methods etc. In the future, we could aim for relaxing this a bit.
                if (Match(EXTERN)) {
                    throw Error(Previous(), "'extern' keyword must come after visibility");
                }

                if (Check(PUBLIC) || Check(PRIVATE))
                {
                    Token visibilityToken = Advance();

                    var isExtern = isExternDeclaration;
                    var isMutable = false;

                    var visibility = visibilityToken switch
                    {
                        { Type: PUBLIC } => Visibility.Public,
                        { Type: PRIVATE } => Visibility.Private,
                        _ => throw new IllegalStateException($"Unexpected token type {visibilityToken.Type}")
                    };

                    if (Match(EXTERN)) {
                        isExtern = true;
                    }

                    if (Match(MUTABLE)) {
                        isMutable = true;
                    }

                    if (Match(CLASS)) return Class(visibility, isExtern);
                    if (Match(CONSTRUCTOR)) return Function("constructor", visibility, isExtern);
                    if (Match(DESTRUCTOR)) return Function("destructor", visibility, isExtern);

                    // If it's not a class, it might as well be a method definition. In the future, we'll likely need to
                    // support instance and static fields here too, which will make things considerably more challenging
                    // (since we are leaning towards dropping the slightly obnoxious 'fun' keyword for functions.
                    // Obnoxious it is, but it does simplify the parsing of the code significantly).
                    return FunctionOrField("method", visibility, isMutable, isExtern);
                }

                if (Match(FUN)) return Function("function", Visibility.Unspecified, isExtern: false);
                if (Match(VAR)) return VarDeclaration();
                if (Match(ENUM)) return Enum();

                if (Match(CLASS)) {
                    throw Error(Previous(), "Class declaration without visibility encountered. You must explicitly mark the class as 'public'.");
                }

                return Statement();
            }
            catch (InternalParseError)
            {
                Synchronize();
                return null;
            }
        }

        private Stmt Statement()
        {
            if (Match(FOR)) return ForStatement();
            if (Match(IF)) return IfStatement();
            if (Match(PRINT)) return PrintStatement();
            if (Match(RETURN)) return ReturnStatement();
            if (Match(WHILE)) return WhileStatement();
            if (Match(LEFT_BRACE)) return new Stmt.Block(Block());

            if (Check(PREPROCESSOR_DIRECTIVE_CPP_PROTOTYPES)) {
                cppPrototypes.Add(Advance());
                return new Stmt.ExpressionStmt(new Expr.Empty());
            }
            else if (Check(PREPROCESSOR_DIRECTIVE_CPP_METHODS))
            {
                cppMethods.Add(Advance());
                return new Stmt.ExpressionStmt(new Expr.Empty());
            }

            return ExpressionStatement();
        }

        private Stmt ForStatement()
        {
            // The "for" implementation is a bit special in that it doesn't use any special AST nodes of its own.
            // Instead, it just desugars the for loop into already existing elements in our toolbox.
            // More details: http://craftinginterpreters.com/control-flow.html#desugaring
            Consume(LEFT_PAREN, "Expect '(' after 'for'.");

            Stmt initializer;
            if (Match(SEMICOLON))
            {
                initializer = null;
            }
            else if (Match(VAR))
            {
                initializer = VarDeclaration();
            }
            else
            {
                initializer = ExpressionStatement();
            }

            Expr condition = null;
            if (!Check(SEMICOLON))
            {
                condition = Expression();
            }

            Consume(SEMICOLON, "Expect ';' after loop condition.");

            Expr increment = null;
            if (!Check(RIGHT_PAREN))
            {
                increment = Expression();
            }

            Consume(RIGHT_PAREN, "Expect ')' after for clauses.");

            Stmt body = Statement();

            if (increment != null)
            {
                body = new Stmt.Block(new List<Stmt>
                {
                    body,
                    new Stmt.ExpressionStmt(increment)
                });
            }

            condition ??= new Expr.Literal(true);

            body = new Stmt.While(condition, body);

            if (initializer != null)
            {
                body = new Stmt.Block(new List<Stmt> { initializer, body });
            }

            return body;
        }

        private Stmt IfStatement()
        {
            Consume(LEFT_PAREN, "Expect '(' after 'if'.");
            Expr condition = Expression();
            Consume(RIGHT_PAREN, "Expect ')' after if condition.");

            Stmt thenBranch = Statement();
            Stmt elseBranch = null;

            if (Match(ELSE))
            {
                elseBranch = Statement();
            }

            return new Stmt.If(condition, thenBranch, elseBranch);
        }

        private Stmt PrintStatement()
        {
            Expr value = Expression();

            if (!IsAtEnd || !allowSemicolonElision)
            {
                Consume(SEMICOLON, "Expect ';' after value.");
            }

            return new Stmt.Print(value);
        }

        private Stmt ReturnStatement()
        {
            Token keyword = Previous();
            Expr value = null;

            if (!Check(SEMICOLON))
            {
                value = Expression();
            }

            if (!IsAtEnd || !allowSemicolonElision)
            {
                Consume(SEMICOLON, "Expecting ';' after return value.");
            }

            return new Stmt.Return(keyword, value);
        }

        private Stmt VarDeclaration()
        {
            if (Check(RESERVED_WORD))
            {
                // Special-case to provide a more helpful error message when e.g. 'byte' is being used as a variable name.
                throw Error(Advance(), "Reserved keyword encountered");
            }

            Token name = Consume(IDENTIFIER, "Expecting variable name.");

            BlockReservedIdentifiers(name);

            // Support optional typing on this form:
            // var s: String;
            Token typeSpecifier = null;
            bool isArray = false;

            if (Match(COLON))
            {
                typeSpecifier = Consume(IDENTIFIER, "Expecting type name.");

                if (IsAtArray())
                    isArray = true;
            }

            Expr initializer = null;

            if (Match(EQUAL))
            {
                initializer = Expression();
            }

            if (!IsAtEnd || !allowSemicolonElision)
            {
                Consume(SEMICOLON, "Expect ';' after variable declaration.");
            }

            return new Stmt.Var(name, initializer, new TypeReference(typeSpecifier, isArray));
        }

        private Stmt WhileStatement()
        {
            Consume(LEFT_PAREN, "Expect '(' after 'while'.");
            Expr condition = Expression();
            Consume(RIGHT_PAREN, "Expect ')' after condition.");
            Stmt body = Statement();

            return new Stmt.While(condition, body);
        }

        private Stmt ExpressionStatement()
        {
            Expr expr = Expression();

            if (allowExpression && IsAtEnd)
            {
                foundExpression = true;
            }
            else if (!allowSemicolonElision || !IsAtEnd)
            {
                Consume(SEMICOLON, "Expect ';' after expression.", ParseErrorType.MISSING_TRAILING_SEMICOLON);
            }

            return new Stmt.ExpressionStmt(expr);
        }

        private Stmt.Class Class(Visibility visibility, bool isExtern)
        {
            Token name = Consume(IDENTIFIER, "Expecting class name.");
            Consume(LEFT_BRACE, "Expecting '{' before class body.");

            List<Stmt.Function> methods = [];
            List<Stmt.Field> fields = [];

            while (!Check(RIGHT_BRACE) && !IsAtEnd) {
                Stmt stmt = Declaration(isExtern);

                if (stmt is Stmt.Function method) {
                    methods.Add(method);
                }
                else if (stmt is Stmt.Field field) {
                    fields.Add(field);
                }
                else if (stmt == null) {
                    // This will happen when we run into an error and the Synchronize() method tries to advance the stream
                    // to the end of the current statement. We try to handle this as gracefully as we can here.
                }
                else {
                    Error(Peek(), $"Internal error: Unexpected statement encountered: {stmt}.");
                }
            }

            Consume(RIGHT_BRACE, "Expect '}' after class body.");

            var typeReference = new TypeReference(name, isArray: false);
            var @class = new Stmt.Class(name, visibility, methods, fields, typeReference);

            foreach (Stmt.Function function in methods) {
                function.SetClass(@class);
            }

            return @class;
        }

        private Stmt FunctionOrField(string kind, Visibility visibility, bool isMutable, bool isExtern)
        {
            return FunctionOrFieldHelper(kind, supportFields: true, visibility, isMutable, isExtern);
        }

        private Stmt Function(string kind, Visibility visibility, bool isExtern)
        {
            return FunctionOrFieldHelper(kind, supportFields: false, visibility, isMutable: null, isExtern);
        }

        private Stmt FunctionOrFieldHelper(string kind, bool supportFields, Visibility visibility, bool? isMutable, bool isExtern)
        {
            Token name;

            bool isConstructor = kind == "constructor";
            bool isDestructor = kind == "destructor";

            if (isConstructor || isDestructor) {
                name = Previous();
            }
            else {
                name = Consume(IDENTIFIER, "Expect " + kind + " name.");
            }

            if (supportFields) {
                if (Match(LEFT_PAREN)) {
                    // This is a method; continue parsing it as such.
                }
                else if (Match(COLON)) {
                    BlockReservedIdentifiers(name);

                    // This is a 'field: type' declaration. Parse it as such.
                    return FieldDeclaration(name, visibility, isMutable ?? throw new ArgumentNullException(nameof(isMutable)), isExtern);
                }
                else {
                    throw Error(Peek(), "Expect '(' or ':' to declare a method or a field");
                }
            }
            else {
                Consume(LEFT_PAREN, "Expect '(' after " + kind + " name.");
            }

            BlockReservedIdentifiers(name);

            var parameters = new List<Parameter>();

            if (!Check(RIGHT_PAREN)) {
                if (isDestructor) {
                    Error(Peek(), "Destructor cannot have any parameters");
                }

                do {
                    if (parameters.Count >= 255) {
                        Error(Peek(), "Cannot have more than 255 parameters.");
                    }

                    if (Check(RESERVED_WORD)) {
                        // Special-case to provide a more helpful error message when e.g. 'byte' is being used as
                        // parameter name.
                        throw Error(Advance(), "Reserved keyword encountered");
                    }

                    Token parameterName = Consume(IDENTIFIER, "Expect parameter name.");

                    BlockReservedIdentifiers(parameterName);

                    Token parameterTypeSpecifier = null;
                    bool isArray = false;

                    // Parameters can optionally use a specific type. If the type is not provided, the compiler will
                    // try to infer the type based on the usage.
                    if (Match(COLON)) {
                        parameterTypeSpecifier = Consume(IDENTIFIER, "Expecting type name.");

                        if (IsAtArray())
                            isArray = true;
                    }

                    parameters.Add(new Parameter(parameterName, new TypeReference(parameterTypeSpecifier, isArray)));
                }
                while (Match(COMMA));
            }

            Consume(RIGHT_PAREN, "Expect ')' after parameters.");

            Token returnTypeSpecifier = null;
            bool isReturnTypeArray = false;
            TypeReference returnTypeReference;

            // Type specifiers are not allowed for constructors and destructors, but we construct a bogus type reference for them to
            // simplify the code elsewhere.
            if (isConstructor || isDestructor) {
                returnTypeReference = new TypeReference(typeof(void));
            }
            else {
                if (Match(COLON)) {
                    if (Check(RESERVED_WORD)) {
                        returnTypeSpecifier = Advance();

                        // Special-case to try and fail as gracefully as possible if one of the type-related reserved words
                        // (byte, sbyte, short etc) are specified at this position. If this happens, we want to emit _one_
                        // single parse error only.
                        parseErrorHandler(new ParseError("Expecting type name", returnTypeSpecifier, ParseErrorType.RESERVED_WORD_ENCOUNTERED));
                    }
                    else {
                        returnTypeSpecifier = Consume(IDENTIFIER, "Expecting type name.");

                        if (IsAtArray())
                            isReturnTypeArray = true;
                    }
                }

                returnTypeReference = new TypeReference(returnTypeSpecifier, isReturnTypeArray);
            }

            if (isExtern) {
                if (Check(LEFT_BRACE)) {
                    throw Error(Previous(), "'extern' methods must not have a body.");
                }

                Consume(SEMICOLON, "Expect ';' after field definition.");

                return new Stmt.Function(
                    name, visibility, parameters, [], returnTypeReference, isConstructor, isDestructor, isExtern: true
                );
            }
            else {
                Consume(LEFT_BRACE, "Expect '{' before " + kind + " body.");
                List<Stmt> body = Block();

                return new Stmt.Function(
                    name, visibility, parameters, body, returnTypeReference, isConstructor, isDestructor, isExtern: false
                );
            }
        }

        private Stmt.Field FieldDeclaration(Token name, Visibility visibility, bool isMutable, bool isExtern)
        {
            // This a bit strict, but we currently enforce it like this. Public fields definitely feel like an
            // anti-pattern, and I think we'll keep it like this for some time to see what it feels like. For 'struct'
            // types (similar to C#/C-style value-typed structs), we very likely want to enable public fields though, to
            // allow structs without any methods defined (i.e. C-style separation of code and data).
            if (visibility != Visibility.Private) {
                throw Error(name, "Fields must be declared as private.");
            }

            // TODO: This doesn't support array types yet.
            Token typeSpecifier = Consume(IDENTIFIER, "Expecting type name.");

            Expr initializer = null;

            if (Check(EQUAL) && isExtern)
            {
                throw Error(Previous(), "'extern' fields must not have an initializer.");
            }

            if (Match(EQUAL))
            {
                initializer = Expression();
            }

            if (!IsAtEnd || !allowSemicolonElision) {
                Consume(SEMICOLON, "Expect ';' after field definition.");
            }

            return new Stmt.Field(name, visibility, isMutable, initializer, new TypeReference(typeSpecifier, isArray: false));
        }

        private Stmt.Enum Enum()
        {
            Token name = Consume(IDENTIFIER, "Enum name expected.");
            Consume(LEFT_BRACE, "Expect '{' after enum keyword.");

            Dictionary<string, Expr> members = new Dictionary<string, Expr>();

            do {
                Token enumValue = Consume(IDENTIFIER, "Expect identifier for enum value.");

                // Enum values can optionally have a value assigned to them.
                Expr value = null;

                if (Match(EQUAL))
                {
                    value = Expression();
                }

                // TODO: Could we evaluate the value here somehow? We only want to support compile-time constants. I guess
                // forward-referencing will be simpler if we don't try to evaluate it this early...
                members[enumValue.Lexeme] = value;
            } while (Match(COMMA));

            Consume(RIGHT_BRACE, "Expect '}' at end of enum declaration.");

            return new Stmt.Enum(name, members);
        }

        private List<Stmt> Block()
        {
            var statements = new List<Stmt>();

            while (!Check(RIGHT_BRACE) && !IsAtEnd)
            {
                statements.Add(Declaration(isExternDeclaration: false));
            }

            Consume(RIGHT_BRACE, "Expect '}' after block.");
            return statements;
        }

        /// <summary>
        /// Handles assignment, as well as the += and -= shorthand assignment operators.
        /// </summary>
        /// <returns>An expression.</returns>
        private Expr Assignment()
        {
            Expr expr = UnaryPostfix();

            if (Match(EQUAL))
            {
                Token equals = Previous();
                Expr value = Assignment();

                if (expr is Expr.Identifier identifier)
                {
                    return new Expr.Assign(identifier, value);
                }
                else if (expr is Expr.Get get)
                {
                    return new Expr.Assign(get, value);
                }

                Error(equals, $"Invalid assignment target: {expr}");
            }
            else if (Match(PLUS_EQUAL, MINUS_EQUAL))
            {
                // Much like the "for" implementation, += and -= do not consist of any dedicated AST nodes. Instead,
                // the expression "i += 5" is essentially nothing but syntactic sugar for "i = i + 5". We want to
                // retain the original token however, to be able to special-case them in other parts of the code. This
                // is not currently necessary but it makes the code more predictable, again following the "explicit is
                // better than implicit" philosophy.
                Token token = Previous();

                // Calling Addition() here will give us the whole content of the right-hand part of the expression,
                // including an arbitrary number of other binary expressions. This makes things like "i += 1 + 5 - 3"
                // work.
                Expr addedOrSubtractedValue = Addition();

                if (expr is Expr.Identifier identifier)
                {
                    // The expression being constructed here can be visualized as "identifier = binary_expression",
                    // where "binary_expression = identifier + value1 + value2...".
                    Expr value = new Expr.Binary(identifier, token, addedOrSubtractedValue);

                    return new Expr.Assign(identifier, value);
                }

                Error(token, "Invalid assignment target.");
            }

            return expr;
        }

        private Expr UnaryPostfix()
        {
            Expr expr = Or();

            if (Match(PLUS_PLUS))
            {
                Token increment = Previous();

                if (expr is Expr.Identifier identifier)
                {
                    return new Expr.UnaryPostfix(identifier, identifier.Name, increment);
                }

                Error(increment, $"Can only increment variables, not {StringifyType(expr)}.");
            }
            else if (Match(MINUS_MINUS))
            {
                Token decrement = Previous();

                if (expr is Expr.Identifier identifier)
                {
                    return new Expr.UnaryPostfix(identifier, identifier.Name, decrement);
                }

                Error(decrement, $"Can only decrement variables, not {StringifyType(expr)}.");
            }

            return expr;
        }

        private Expr Or()
        {
            Expr expr = And();

            while (Match(PIPE_PIPE))
            {
                Token @operator = Previous();
                Expr right = And();
                expr = new Expr.Logical(expr, @operator, right);
            }

            return expr;
        }

        private Expr And()
        {
            Expr expr = Equality();

            while (Match(AMPERSAND_AMPERSAND))
            {
                Token @operator = Previous();
                Expr right = Equality();
                expr = new Expr.Logical(expr, @operator, right);
            }

            return expr;
        }

        private Expr Equality()
        {
            Expr expr = Comparison();

            while (Match(BANG_EQUAL, EQUAL_EQUAL))
            {
                Token @operator = Previous();
                Expr right = Comparison();
                expr = new Expr.Binary(expr, @operator, right);
            }

            return expr;
        }

        private Expr Comparison()
        {
            Expr expr = Addition();

            while (Match(GREATER, GREATER_EQUAL, LESS, LESS_EQUAL))
            {
                Token @operator = Previous();
                Expr right = Addition();
                expr = new Expr.Binary(expr, @operator, right);
            }

            return expr;
        }

        private Expr Addition()
        {
            Expr expr = Multiplication();

            while (Match(MINUS, PLUS))
            {
                Token @operator = Previous();
                Expr right = Multiplication();
                expr = new Expr.Binary(expr, @operator, right);
            }

            return expr;
        }

        private Expr Multiplication()
        {
            Expr expr = UnaryPrefix();

            while (Match(SLASH, STAR, STAR_STAR, PERCENT, LESS_LESS, GREATER_GREATER))
            {
                Token @operator = Previous();
                Expr right = UnaryPrefix();
                expr = new Expr.Binary(expr, @operator, right);
            }

            return expr;
        }

        private Expr UnaryPrefix()
        {
            if (Match(BANG, MINUS))
            {
                Token @operator = Previous();
                Expr right = UnaryPrefix();

                // We detect MINUS + NUMBER here and convert it to a single Expr.Literal() (with constant value) instead
                // of retaining it as a unary prefix expression. See #302 for some more details about why this was
                // changed.
                if (@operator.Type == MINUS && right is Expr.Literal rightLiteral)
                {
                    if (rightLiteral.Value is INumericLiteral numericLiteral)
                    {
                        return new Expr.Literal(NumberParser.MakeNegative(numericLiteral));
                    }
                    else if (rightLiteral.Value is null)
                    {
                        Error(Peek(), "Unary minus operator does not support null operand");
                        return new Expr.Literal(null);
                    }
                    else
                    {
                        // TODO: Call Error() here to produce a context-aware error instead of just throwing a raw exception
                        throw new ArgumentException($"Type {rightLiteral.Value.GetType().ToTypeKeyword()} not supported");
                    }
                }
                else
                {
                    return new Expr.UnaryPrefix(@operator, right);
                }
            }

            return IndexingOrCall();
        }

        private Expr IndexingOrCall()
        {
            Expr expr = Primary();

            // At this point, we have no idea whether the primary expression will be called or indexed.
            while (true)
            {
                if (Match(LEFT_PAREN))
                {
                    // Cannot return, since we must continue consuming Call expressions (to support
                    // foo().bar().baz().zot chaining of function calls)
                    expr = FinishCall(expr);
                }
                else if (Match(LEFT_SQUARE_BRACKET))
                {
                    // Likewise, cannot return here to be able to support things like foo[1][2][3]-style, i.e.
                    // multi-dimensional indexing.
                    expr = FinishIndex(expr);
                }
                else if (Match(DOT))
                {
                    Token name = Consume(IDENTIFIER, "Expect identifier after '.'.");
                    expr = new Expr.Get(expr, name);
                }
                else
                {
                    break;
                }
            }

            return expr;
        }

        private Expr FinishCall(Expr callee)
        {
            var arguments = new List<Expr>();

            if (!Check(RIGHT_PAREN))
            {
                do
                {
                    if (arguments.Count >= 255)
                    {
                        Error(Peek(), "Cannot have more than 255 arguments.");
                    }

                    arguments.Add(Expression());
                }
                while (Match(COMMA));
            }

            Token paren = Consume(RIGHT_PAREN, "Expect ')' after arguments.");

            return new Expr.Call(callee, paren, arguments);
        }

        private Expr FinishIndex(Expr indexee)
        {
            Expr argument = Expression();
            Token closingBracket = Consume(RIGHT_SQUARE_BRACKET, "Expect '] after index argument'");

            return new Expr.Index(indexee, closingBracket, argument);
        }

        /// <remarks>Note that statements are not handled here, but in <see cref="Declaration"/>.</remarks>
        private Expr Primary()
        {
            if (Match(FALSE)) return new Expr.Literal(false);
            if (Match(TRUE)) return new Expr.Literal(true);
            if (Match(PERLANG_NULL)) return new Expr.Literal(null);

            if (Match(NUMBER))
            {
                // Numbers are retained as strings in the scanning phase, to properly be able to parse negative numbers
                // in the parsing stage (where we can more easily join the MINUS and NUMBER token together). See #302
                // for details.
                INumericLiteral numericLiteral = NumberParser.Parse((NumericToken)Previous());
                return new Expr.Literal(numericLiteral);
            }

            if (Match(STRING))
            {
                string s = (string)Previous().Literal!;

                Lang.String nativeString = Lang.String.from(s);
                return new Expr.Literal(nativeString);
            }

            if (Match(CHAR))
            {
                char c = (char)Previous().Literal!;

                return new Expr.Literal(c);
            }

            if (Match(THIS))
            {
                // We don't use any separate expression type for 'this', but just puts 'this' as a regular local variable
                // in the current scope.
                return new Expr.Identifier(Previous());
            }

            if (Match(IDENTIFIER))
            {
                return new Expr.Identifier(Previous());
            }

            if (Match(LEFT_PAREN))
            {
                Expr expr = Expression();
                Consume(RIGHT_PAREN, "Expect ')' after expression.");
                return new Expr.Grouping(expr);
            }

            if (Match(LEFT_SQUARE_BRACKET))
            {
                var startToken = Previous();
                var elements = new List<Expr>();

                while (!Peek().Type.Equals(RIGHT_SQUARE_BRACKET) && !IsAtEnd)
                {
                    elements.Add(Expression());

                    if (Match(COMMA))
                    {
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }

                Consume(RIGHT_SQUARE_BRACKET, "Expect ']' at end of collection initializer.");

                return new Expr.CollectionInitializer(elements, startToken);
            }

            if (Match(NEW))
            {
                Token typeName = Consume(IDENTIFIER, "Expecting name of class to instantiate");

                Match(LEFT_PAREN);

                var parameters = new List<Expr>();

                while (!Peek().Type.Equals(RIGHT_PAREN) && !IsAtEnd)
                {
                    parameters.Add(Expression());

                    if (Match(COMMA))
                    {
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }

                Consume(RIGHT_PAREN, "Expect ')' at end of constructor parameters.");

                return new Expr.NewExpression(typeName, parameters);
            }

            if (Check(SEMICOLON))
            {
                // Bare semicolon, no expression inside. To avoid having to handle pesky null exceptions all over
                // the code base, we have a dedicated expression for this.
                return new Expr.Empty();
            }

            throw Error(Peek(), "Expect expression.");
        }

        /// <summary>
        /// Matches the given token type(s), at the current position. If a matching token is found, it gets consumed.
        ///
        /// For a non-consuming version of this, see <see cref="Check"/>.
        /// </summary>
        /// <param name="types">One or more token types to match.</param>
        /// <returns>`true` if a matching token was found and consumed, `false` otherwise.</returns>
        private bool Match(params TokenType[] types)
        {
            foreach (TokenType type in types)
            {
                if (Check(type))
                {
                    Advance();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Matches the given token type at the current position, expecting a match. If the current token does not
        /// match, an exception is thrown.
        /// </summary>
        /// <param name="type">The type of token to match.</param>
        /// <param name="message">The error message to use if the token does not match.</param>
        /// <param name="parseErrorType">An optional parameter indicating the type of parse error.</param>
        /// <returns>The matched token.</returns>
        /// <exception cref="InternalParseError">The token does not match.</exception>
        private Token Consume(TokenType type, string message, ParseErrorType? parseErrorType = null)
        {
            if (Check(type))
            {
                return Advance();
            }

            throw Error(Peek(), message, parseErrorType);
        }

        /// <summary>
        /// Non-consuming version of <see cref="Match"/>: checks if the current token matches the provided <see
        /// cref="TokenType"/>, but does not consume the token even if it matches.
        /// </summary>
        /// <param name="type">The `TokenType` to match.</param>
        /// <returns>`true` if it matches, `false` otherwise.</returns>
        private bool Check(TokenType type)
        {
            if (IsAtEnd)
            {
                return false;
            }

            return Peek().Type == type;
        }

        /// <summary>
        /// Advance the current position with one step. If the position is already at the end of the stream, this method
        /// does nothing.
        /// </summary>
        /// <returns>The token at the stream position before advancing it.</returns>
        private Token Advance()
        {
            if (!IsAtEnd)
            {
                current++;
            }

            return Previous();
        }

        private bool IsAtEnd =>
            Peek().Type == PERLANG_EOF;

        /// <summary>
        /// Are we currently at an `[]` array specifier? (appendix used in type specifiers like `string[]` and so forth).
        /// </summary>
        /// <returns>`true` if we are currently at an `[]` specifier, `false` otherwise.</returns>
        private bool IsAtArray()
        {
            if (Peek().Type == LEFT_SQUARE_BRACKET && PeekNext().Type == RIGHT_SQUARE_BRACKET) {
                // Skip over the brackets
                Advance();
                Advance();

                // This is a array definition, e.g. "a[]". We include this in the type reference.
                return true;
            }
            else {
                return false;
            }
        }

        /// <summary>
        /// Returns the token at the current position.
        /// </summary>
        /// <returns>A token.</returns>
        [DebuggerStepThrough]
        private Token Peek()
        {
            return tokens[current];
        }

        private Token PeekNext()
        {
            if (tokens.Count > current + 1) {
                return tokens[current + 1];
            }
            else {
                throw new InvalidOperationException("No more tokens available");
            }
        }

        /// <summary>
        /// Returns the token right before the current position.
        /// </summary>
        /// <returns>A token.</returns>
        [DebuggerStepThrough]
        private Token Previous()
        {
            return tokens[current - 1];
        }

        private InternalParseError Error(Token token, string message, ParseErrorType? parseErrorType = null)
        {
            parseErrorHandler(new ParseError(message, token, parseErrorType));

            return new InternalParseError(parseErrorType);
        }

        /// <summary>
        /// Synchronizes the parser after an <see cref="InternalParseError"/> has occurred.
        /// </summary>
        private void Synchronize()
        {
            // Do a best-effort attempt to recover from the current state. We try to forward to the end of the current
            // statement.

            Advance();

            while (!IsAtEnd)
            {
                if (Previous().Type == SEMICOLON)
                {
                    return;
                }

                switch (Peek().Type)
                {
                    case FUN:
                    case VAR:
                    case FOR:
                    case IF:
                    case WHILE:
                    case PRINT:
                    case RETURN:
                        return;
                }

                Advance();
            }
        }

        /// <summary>
        /// Throws an <see cref="Error"/> if the given token represents a reserved keyword.
        /// </summary>
        /// <param name="token">A token with the name of an identifier.</param>
        /// <exception cref="Error">The given token represents a reserved keyword.</exception>
        private void BlockReservedIdentifiers(Token token)
        {
            // "Reserved for future use". These are not currently supported in Perlang, but we reserve them for
            // future use and define them now already (making it impossible to use when e.g. defining a variable of
            // a function). That way, we reduce the risk of breaking old code if/when introducing these into the
            // language proper.
            //
            // More details might be found in #178.

            if (Scanner.ReservedKeywordStrings.Contains(token.Lexeme))
            {
                throw Error(token, "Reserved keyword encountered", ParseErrorType.RESERVED_WORD_ENCOUNTERED);
            }
        }
    }
}
