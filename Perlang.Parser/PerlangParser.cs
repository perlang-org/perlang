using System;
using System.Collections.Generic;
using System.Linq;
using static Perlang.TokenType;
using static Perlang.Utils;

namespace Perlang.Parser
{
    // Convoluted name to avoid conflict with namespace
    public class PerlangParser
    {
        private bool allowExpression;
        private bool foundExpression = false;

        private class ParseError : Exception
        {
            public ParseErrorType? ParseErrorType { get; }

            public ParseError(ParseErrorType? parseErrorType)
            {
                ParseErrorType = parseErrorType;
            }
        }

        private readonly ParseErrorHandler parseErrorHandler;
        private readonly List<Token> tokens;

        private int current;

        public PerlangParser(List<Token> tokens, ParseErrorHandler parseErrorHandler)
        {
            this.parseErrorHandler = parseErrorHandler;
            this.tokens = tokens;
        }

        public IList<Stmt> ParseStatements()
        {
            var statements = new List<Stmt>();

            while (!IsAtEnd())
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
            catch (ParseError)
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
        /// <returns>an <see cref="Expr"/> or a list of <see cref="Stmt"/> objects.</returns>
        public object ParseExpressionOrStatements()
        {
            allowExpression = true;

            var statements = new List<Stmt>();

            while (!IsAtEnd())
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

        private Expr Expression()
        {
            return Assignment();
        }

        private Stmt Declaration()
        {
            try
            {
                if (Match(FUN)) return Function("function");
                if (Match(VAR)) return VarDeclaration();

                return Statement();
            }
            catch (ParseError)
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

            if (condition == null) condition = new Expr.Literal(true);
            body = new Stmt.While(condition, body);

            if (initializer != null)
            {
                body = new Stmt.Block(new List<Stmt> {initializer, body});
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
            Consume(SEMICOLON, "Expect ';' after value.");
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

            Consume(SEMICOLON, "Expect ';' after return value.");
            return new Stmt.Return(keyword, value);
        }

        private Stmt VarDeclaration()
        {
            Token name = Consume(IDENTIFIER, "Expect variable name.");

            Expr initializer = null;
            if (Match(EQUAL))
            {
                initializer = Expression();
            }

            Consume(SEMICOLON, "Expect ';' after variable declaration.");
            return new Stmt.Var(name, initializer);
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

            if (allowExpression && IsAtEnd())
            {
                foundExpression = true;
            }
            else
            {
                Consume(SEMICOLON, "Expect ';' after expression.", ParseErrorType.MISSING_TRAILING_SEMICOLON);
            }

            return new Stmt.ExpressionStmt(expr);
        }

        private Stmt.Function Function(string kind)
        {
            Token name = Consume(IDENTIFIER, "Expect " + kind + " name.");
            Consume(LEFT_PAREN, "Expect '(' after " + kind + " name.");
            var parameters = new List<Token>();

            if (!Check(RIGHT_PAREN))
            {
                do
                {
                    if (parameters.Count >= 255)
                    {
                        Error(Peek(), "Cannot have more than 255 parameters.");
                    }

                    parameters.Add(Consume(IDENTIFIER, "Expect parameter name."));
                } while (Match(COMMA));
            }

            Consume(RIGHT_PAREN, "Expect ')' after parameters.");

            Consume(LEFT_BRACE, "Expect '{' before " + kind + " body.");
            List<Stmt> body = Block();
            return new Stmt.Function(name, parameters, body);
        }

        private List<Stmt> Block()
        {
            var statements = new List<Stmt>();

            while (!Check(RIGHT_BRACE) && !IsAtEnd())
            {
                statements.Add(Declaration());
            }

            Consume(RIGHT_BRACE, "Expect '}' after block.");
            return statements;
        }

        private Expr Assignment()
        {
            Expr expr = UnaryPostfix();

            if (Match(EQUAL))
            {
                Token equals = Previous();
                Expr value = Assignment();

                if (expr is Expr.Variable variable)
                {
                    Token name = variable.Name;
                    return new Expr.Assign(name, value);
                }

                Error(equals, "Invalid assignment target.");
            }

            return expr;
        }

        private Expr UnaryPostfix()
        {
            Expr expr = Or();

            if (Match(PLUS_PLUS))
            {
                Token increment = Previous();

                if (expr is Expr.Variable variable)
                {
                    return new Expr.UnaryPostfix(variable, variable.Name, increment);
                }

                Error(increment, $"Can only increment variables, not {StringifyType(expr)}.");
            }
            else if (Match(MINUS_MINUS))
            {
                Token decrement = Previous();

                if (expr is Expr.Variable variable)
                {
                    return new Expr.UnaryPostfix(variable, variable.Name, decrement);
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
                Token _operator = Previous();
                Expr right = And();
                expr = new Expr.Logical(expr, _operator, right);
            }

            return expr;
        }

        private Expr And()
        {
            Expr expr = Equality();

            while (Match(AND))
            {
                Token _operator = Previous();
                Expr right = Equality();
                expr = new Expr.Logical(expr, _operator, right);
            }

            return expr;
        }

        private Expr Equality()
        {
            Expr expr = Comparison();

            while (Match(BANG_EQUAL, EQUAL_EQUAL))
            {
                Token _operator = Previous();
                Expr right = Comparison();
                expr = new Expr.Binary(expr, _operator, right);
            }

            return expr;
        }

        private Expr Comparison()
        {
            Expr expr = Addition();

            while (Match(GREATER, GREATER_EQUAL, LESS, LESS_EQUAL))
            {
                Token _operator = Previous();
                Expr right = Addition();
                expr = new Expr.Binary(expr, _operator, right);
            }

            return expr;
        }

        private Expr Addition()
        {
            Expr expr = Multiplication();

            while (Match(MINUS, PLUS))
            {
                Token _operator = Previous();
                Expr right = Multiplication();
                expr = new Expr.Binary(expr, _operator, right);
            }

            return expr;
        }

        private Expr Multiplication()
        {
            Expr expr = UnaryPrefix();

            while (Match(SLASH, STAR))
            {
                Token _operator = Previous();
                Expr right = UnaryPrefix();
                expr = new Expr.Binary(expr, _operator, right);
            }

            return expr;
        }

        private Expr UnaryPrefix()
        {
            if (Match(BANG, MINUS))
            {
                Token _operator = Previous();
                Expr right = UnaryPrefix();
                return new Expr.UnaryPrefix(_operator, right);
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
                } while (Match(COMMA));
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
                return new Expr.Variable(Previous());
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
        /// <param name="types">One or more token types to match</param>
        /// <returns>true if a matching token was found and consumed, false otherwise</returns>
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
        /// Matches the given token type at the current position. If the current token does not match, an exception is
        /// thrown.
        /// </summary>
        /// <param name="type">the type of token to match</param>
        /// <param name="message">the error message to use if the token does not match</param>
        /// <param name="parseErrorType">an optional parameter indicating the type of parse error</param>
        /// <returns>the matched token</returns>
        /// <exception cref="ParseError">if the token does not match</exception>
        private Token Consume(TokenType type, string message, ParseErrorType? parseErrorType = null)
        {
            if (Check(type))
            {
                return Advance();
            }

            throw Error(Peek(), message, parseErrorType);
        }

        /// <summary>
        /// Checks if the current token matches the provided TokenType. Similar to <see cref="Match"/> but does not
        /// consume the token on matches.
        /// </summary>
        /// <param name="type">the TokenType to match</param>
        /// <returns>true if it matches, false otherwise</returns>
        private bool Check(TokenType type)
        {
            if (IsAtEnd())
            {
                return false;
            }

            return Peek().Type == type;
        }

        private Token Advance()
        {
            if (!IsAtEnd())
            {
                current++;
            }

            return Previous();
        }

        private bool IsAtEnd()
        {
            return Peek().Type == EOF;
        }

        /// <summary>
        /// Returns the token at the current position.
        /// </summary>
        /// <returns>A Token</returns>
        private Token Peek()
        {
            return tokens[current];
        }

        /// <summary>
        /// Returns the token right before the current position.
        /// </summary>
        /// <returns>A Token</returns>
        private Token Previous()
        {
            return tokens[current - 1];
        }

        private ParseError Error(Token token, string message, ParseErrorType? parseErrorType = null)
        {
            parseErrorHandler(new Parser.ParseError
            {
                Token = token,
                Message = message,
                ParseErrorType = parseErrorType
            });
            return new ParseError(parseErrorType);
        }

        private void Synchronize()
        {
            Advance();

            while (!IsAtEnd())
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
