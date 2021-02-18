// This class contains a number of these violations. While slightly ugly, it does make the code a bit more dense and
// arguably, readable. I'm not fully convinced that adding the braces in this _particular case_ makes the code better.
// Some form of switch expression-based solution would probably be good enough to convince me; feel free to give it a
// try and send a PR.

#pragma warning disable SA1503

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using static Perlang.TokenType;
using static Perlang.Utils;

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

        private int current;

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
                statements.Add(Declaration());
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
        /// <returns>An <see cref="Expr"/> or a list of <see cref="Stmt"/> objects.</returns>
        public object ParseExpressionOrStatements()
        {
            allowExpression = true;

            var statements = new List<Stmt>();

            while (!IsAtEnd)
            {
                statements.Add(Declaration());

                if (foundExpression)
                {
                    Stmt last = statements.Last();
                    return ((Stmt.ExpressionStmt) last).Expression;
                }

                allowExpression = false;
            }

            return statements;
        }

        /// <summary>
        /// Returns the expression at the current position in the stream.
        /// </summary>
        /// <returns>An expression.</returns>
        private Expr Expression()
        {
            return Assignment();
        }

        private Stmt Declaration()
        {
            try
            {
                if (Match(CLASS)) return ClassDeclaration();
                if (Match(FUN)) return Function("function");
                if (Match(VAR)) return VarDeclaration();

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
            Token name = Consume(IDENTIFIER, "Expecting variable name.");

            // Support optional typing on this form:
            // var s: String;
            Token typeSpecifier = null;
            if (Match(COLON))
            {
                typeSpecifier = Consume(IDENTIFIER, "Expecting type name.");
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

            return new Stmt.Var(name, initializer, new TypeReference(typeSpecifier));
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

        private Stmt.Class ClassDeclaration()
        {
            Token name = Consume(IDENTIFIER, "Expect class name.");
            Consume(LEFT_BRACE, "Expect '{' before class body.");

            var methods = new List<Stmt.Function>();

            while (!Check(RIGHT_BRACE) && !IsAtEnd)
            {
                methods.Add(Function("method"));
            }

            Consume(RIGHT_BRACE, "Expect '}' after class body.");

            return new Stmt.Class(name, methods);
        }

        private Stmt.Function Function(string kind)
        {
            Token name = Consume(IDENTIFIER, "Expect " + kind + " name.");
            Consume(LEFT_PAREN, "Expect '(' after " + kind + " name.");
            var parameters = new List<Parameter>();

            if (!Check(RIGHT_PAREN))
            {
                do
                {
                    if (parameters.Count >= 255)
                    {
                        Error(Peek(), "Cannot have more than 255 parameters.");
                    }

                    var parameterName = Consume(IDENTIFIER, "Expect parameter name.");
                    Token parameterTypeSpecifier = null;

                    // Parameters can optionally use a specific type. If the type is not provided, the compiler will
                    // try to infer the type based on the usage.
                    if (Match(COLON))
                    {
                        parameterTypeSpecifier = Consume(IDENTIFIER, "Expecting type name.");
                    }

                    parameters.Add(new Parameter(parameterName, new TypeReference(parameterTypeSpecifier)));
                }
                while (Match(COMMA));
            }

            Consume(RIGHT_PAREN, "Expect ')' after parameters.");

            Token returnTypeSpecifier = null;

            if (Match(COLON))
            {
                returnTypeSpecifier = Consume(IDENTIFIER, "Expecting type name.");
            }

            Consume(LEFT_BRACE, "Expect '{' before " + kind + " body.");
            List<Stmt> body = Block();
            return new Stmt.Function(name, parameters, body, new TypeReference(returnTypeSpecifier));
        }

        private List<Stmt> Block()
        {
            var statements = new List<Stmt>();

            while (!Check(RIGHT_BRACE) && !IsAtEnd)
            {
                statements.Add(Declaration());
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

                Error(equals, "Invalid assignment target.");
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

            while (Match(OR))
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

            while (Match(AND))
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

            while (Match(SLASH, STAR, STAR_STAR, PERCENT))
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
                return new Expr.UnaryPrefix(@operator, right);
            }

            return Call();
        }

        private Expr Call()
        {
            Expr expr = Primary();

            while (true)
            {
                if (Match(LEFT_PAREN))
                {
                    expr = FinishCall(expr);
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

        private Expr Primary()
        {
            if (Match(FALSE)) return new Expr.Literal(false);
            if (Match(TRUE)) return new Expr.Literal(true);
            if (Match(NIL)) return new Expr.Literal(null);

            if (Match(NUMBER, STRING))
            {
                return new Expr.Literal(Previous().Literal);
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

        private Token Advance()
        {
            if (!IsAtEnd)
            {
                current++;
            }

            return Previous();
        }

        private bool IsAtEnd =>
            Peek().Type == EOF;

        /// <summary>
        /// Returns the token at the current position.
        /// </summary>
        /// <returns>A token.</returns>
        private Token Peek()
        {
            return tokens[current];
        }

        /// <summary>
        /// Returns the token right before the current position.
        /// </summary>
        /// <returns>A token.</returns>
        private Token Previous()
        {
            return tokens[current - 1];
        }

        private InternalParseError Error(Token token, string message, ParseErrorType? parseErrorType = null)
        {
            parseErrorHandler(new ParseError(message, token, parseErrorType));

            return new InternalParseError(parseErrorType);
        }

        private void Synchronize()
        {
            Advance();

            while (!IsAtEnd)
            {
                if (Previous().Type == SEMICOLON)
                {
                    return;
                }

                switch (Peek().Type)
                {
                    case CLASS:
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
    }
}
