using System.Collections.Generic;
using System.Linq;
using Perlang.Interpreter.Resolution;
using Perlang.Interpreter.Typing;
using Xunit;

namespace Perlang.Tests.Interpreter
{
    /// <summary>
    /// Test for <see cref="TypeValidator"/>.
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
            var call = new Expr.Call(get, paren, new List<Expr>());
            var bindings = new Dictionary<Expr, Binding>
            {
                { identifier, new NativeClassBinding(identifier, typeof(string)) }
            };

            var typeValidationErrors = new List<TypeValidationError>();

            // Act
            TypeValidator.Validate(call, error => typeValidationErrors.Add(error), expr => bindings[expr]);

            // Assert
            Assert.Empty(typeValidationErrors);
        }

        [Fact]
        public void Validate_Get_expr_yields_expected_error_for_undefined_variable()
        {
            // Arrange
            var name = new Token(TokenType.IDENTIFIER, "foo", null, -1);
            var identifier = new Expr.Identifier(name);
            var get = new Expr.Get(identifier, name);

            var bindings = new Dictionary<Expr, Binding>
            {
                // A null TypeReference here is the condition that is expected to lead to an "undefined variable" error.
                { identifier, new VariableBinding(typeReference: null, 0, identifier) }
            };

            var typeValidationErrors = new List<TypeValidationError>();

            // Act
            TypeValidator.Validate(get, error => typeValidationErrors.Add(error), expr => bindings[expr]);

            // Assert
            Assert.Single(typeValidationErrors);
            Assert.Matches(typeValidationErrors.Single().Message, "Undefined identifier 'foo'");
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
            var get = new Expr.Get(identifier, name);

            var bindings = new Dictionary<Expr, Binding>
            {
                { identifier, new ClassBinding(identifier, new PerlangClass("Foo", new List<Stmt.Function>())) }
            };

            var typeValidationErrors = new List<TypeValidationError>();

            // Act
            TypeValidator.Validate(get, error => typeValidationErrors.Add(error), expr => bindings[expr]);

            // Assert
            Assert.Empty(typeValidationErrors);
        }
    }
}
