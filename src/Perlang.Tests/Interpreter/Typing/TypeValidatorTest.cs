using System.Collections.Generic;
using System.Linq;
using Perlang.Interpreter.Resolution;
using Perlang.Interpreter.Typing;
using Perlang.Parser;
using Xunit;

namespace Perlang.Tests.Interpreter.Typing
{
    /// <summary>
    /// Test for <see cref="TypeValidator"/>.
    ///
    /// Note that the test methods in this test work on a very low level; as can be seen, tokens and expressions are
    /// constructed manually in a very tedious way. This can be useful sometimes, but most of the time, it's much easier
    /// to create integration tests where you just feed an entire Perlang program to the interpreter and let it
    /// construct the data structures for you.
    ///
    /// For an example of the latter, see <see cref="Perlang.Tests.Integration.Assignment.AssignmentTests"/> and the
    /// other tests in the `Perlang.Tests.Integration` project.
    /// </summary>
    public class TypeValidatorTest
    {
        [Fact]
        public void Validate_Call_expr_does_something_useful()
        {
            // Arrange
            var name = new Token(TokenType.IDENTIFIER, "to_string", null, 1);
            var identifier = new Expr.Identifier(name);
            var get = new Expr.Get(identifier, name);
            var paren = new Token(TokenType.RIGHT_PAREN, ")", null, 1);
            var callExpr = new Expr.Call(get, paren, new List<Expr>());
            var bindings = new Dictionary<Expr, Binding>
            {
                { identifier, new NativeClassBinding(identifier, typeof(string)) }
            };

            var typeValidationErrors = new List<TypeValidationError>();
            var warnings = new List<CompilerWarning>();

            // Act
            TypeValidator.Validate(
                new List<Stmt> { new Stmt.ExpressionStmt(callExpr) },
                error => typeValidationErrors.Add(error),
                expr => bindings[expr],
                warning => warnings.Add(warning)
            );

            // Assert
            Assert.Empty(typeValidationErrors);
            Assert.Empty(warnings);
        }

        [Fact]
        public void Validate_Get_expr_yields_expected_error_for_undefined_variable()
        {
            // Arrange
            var name = new Token(TokenType.IDENTIFIER, "foo", null, -1);
            var identifier = new Expr.Identifier(name);
            var getExpr = new Expr.Get(identifier, name);

            var bindings = new Dictionary<Expr, Binding>
            {
                // A null TypeReference here is the condition that is expected to lead to an "undefined variable" error.
                { identifier, new VariableBinding(typeReference: null, 0, identifier) }
            };

            var typeValidationErrors = new List<TypeValidationError>();
            var warnings = new List<CompilerWarning>();

            // Act
            TypeValidator.Validate(
                new List<Stmt> { new Stmt.ExpressionStmt(getExpr) },
                error => typeValidationErrors.Add(error),
                expr => bindings[expr],
                warning => warnings.Add(warning)
            );

            // Assert
            Assert.Single(typeValidationErrors);
            Assert.Matches(typeValidationErrors.Single().Message, "Undefined identifier 'foo'");

            Assert.Empty(warnings);
        }

        [Fact]
        public void Validate_Get_expr_yields_no_error_for_defined_variable()
        {
            // Arrange
            //
            // This is the method being referred to. The expression below roughly matches "Foo.to_string", where
            // Foo is a defined Perlang class.
            var name = new Token(TokenType.IDENTIFIER, "to_string", null, -1);
            var identifier = new Expr.Identifier(name);
            var getExpr = new Expr.Get(identifier, name);

            var bindings = new Dictionary<Expr, Binding>
            {
                { identifier, new ClassBinding(identifier, new PerlangClass("Foo", new List<Stmt.Function>())) }
            };

            var typeValidationErrors = new List<TypeValidationError>();
            var warnings = new List<CompilerWarning>();

            // Act
            TypeValidator.Validate(
                new List<Stmt> { new Stmt.ExpressionStmt(getExpr) },
                error => typeValidationErrors.Add(error),
                expr => bindings[expr],
                warning => warnings.Add(warning)
            );

            // Assert
            Assert.Empty(typeValidationErrors);
            Assert.Empty(warnings);
        }
    }
}
