using System;
using System.Linq;
using FluentAssertions;
using Perlang.Compiler;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Var
{
    // Tests based on Lox test suite: https://github.com/munificent/craftinginterpreters/tree/master/test/variable
    public class VarTests
    {
        [Fact]
        public void declare_typed_global_does_not_throw_any_error()
        {
            // It is an error to declare an implicitly typed variable without an initializer (what type would it be?),
            // but declaring an uninitialized variable with an explicit type is perfectly fine.
            string source = @"
                var a: int;
            ";

            var result = EvalWithRuntimeErrorCatch(source);

            Assert.Empty(result.Errors);
        }

        [Fact]
        public void collide_with_parameter_throws_expected_error()
        {
            string source = @"
                fun foo(a) {
                    var a;
                }
            ";

            var result = EvalWithNameResolutionErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Matches("Error at 'a': Variable with this name already declared in this scope.", exception.ToString());
        }

        [Fact]
        public void duplicate_local_throws_expected_error()
        {
            string source = @"
                {
                    var a = ""value"";
                    var a = ""other"";
                }
            ";

            var result = EvalWithNameResolutionErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Matches("Error at 'a': Variable with this name already declared in this scope.", exception.ToString());
        }

        [Fact]
        public void duplicate_parameter_throws_expected_error()
        {
            string source = @"
                fun foo(arg,
                        arg) {
                    ""body"";
                }
            ";

            var result = EvalWithNameResolutionErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Matches("Error at 'arg': Variable with this name already declared in this scope.", exception.ToString());
        }

        [SkippableFact]
        public void early_bound()
        {
            // This is a bit of an edge case. Unsure if we need to care about it in compiled mode, at least not
            // initially.
            Skip.If(PerlangMode.ExperimentalCompilation, "Early-bound variable test is not supported in compiled mode");

            string source = @"
                var a = ""outer"";
                {
                    fun foo(): void {
                        print a;
                    }

                    foo();
                    var a = ""inner"";
                    foo();
                }
            ";

            var output = EvalReturningOutput(source);

            Assert.Equal(new[]
            {
                "outer",
                "outer"
            }, output);
        }

        [SkippableFact]
        public void in_middle_of_block()
        {
            string source = @"
                {
                    var a = ""a"";
                    print a;

                    var b = a + "" b"";
                    print b;

                    var c = a + "" c"";
                    print c;

                    var d = b + "" d"";
                    print d;
                }
            ";

            var output = EvalReturningOutput(source);

            Assert.Equal(new[]
            {
                "a",
                "a b",
                "a c",
                "a b d"
            }, output);
        }

        [Fact]
        public void in_nested_block()
        {
            string source = @"
                {
                    var a = ""outer"";
                    {
                        print a;
                    }
                }
            ";

            var output = EvalReturningOutputString(source);

            Assert.Equal("outer", output);
        }

        [Fact(Skip = "Blocked pending https://github.com/perlang-org/perlang/issues/66")]
        public void local_from_method()
        {
            string source = @"
                var foo = ""variable"";

                class Foo {
                  method() {
                    print foo;
                  }
                }

                Foo().method();
            ";

            var output = EvalReturningOutputString(source);

            Assert.Equal("variable", output);
        }

        [Fact]
        public void redeclare_global_throws_expected_error()
        {
            string source = @"
                var a = 1;
                var a: int;
                print a;
            ";

            if (PerlangMode.ExperimentalCompilation) {
                Action action = () => EvalReturningOutput(source);

                // This is horribly hardwired for (a particular version of) CLang, but it will have to do for now.
                action.Should().Throw<PerlangCompilerException>()
                    .WithMessage("*redefinition of 'a'*");
            }
            else {
                var result = EvalWithRuntimeErrorCatch(source);
                var exception = result.Errors.FirstOrDefault();

                Assert.Single(result.Errors);
                Assert.Matches("Variable with this name already declared in this scope.", exception.Message);
            }
        }

        [Fact]
        public void redefine_global_variable_throws_expected_error()
        {
            string source = @"
                var a = 1;
                var a = 2;
                print a;
            ";

            if (PerlangMode.ExperimentalCompilation) {
                Action action = () => EvalReturningOutput(source);

                // This is horribly hardwired for (a particular version of) CLang, but it will have to do for now.
                action.Should().Throw<PerlangCompilerException>()
                    .WithMessage("*redefinition of 'a'*");
            }
            else {
                var result = EvalWithRuntimeErrorCatch(source);
                var exception = result.Errors.FirstOrDefault();

                Assert.Single(result.Errors);
                Assert.Matches("Variable with this name already declared in this scope.", exception.Message);
            }
        }

        [Fact]
        public void scope_reuse_in_different_blocks()
        {
            string source = @"
                {
                    var a = ""first"";
                    print a;
                }

                {
                    var a = ""second"";
                    print a;
                }
            ";

            var output = EvalReturningOutput(source);

            Assert.Equal(new[]
            {
                "first",
                "second"
            }, output);
        }

        [Fact]
        public void shadow_and_local()
        {
            string source = @"
                {
                    var a = ""outer"";

                    {
                        print a;

                        var a = ""inner"";
                        print a;
                    }
                }
            ";

            var output = EvalReturningOutput(source);

            Assert.Equal(new[]
            {
                "outer",
                "inner"
            }, output);
        }

        [Fact]
        public void shadow_global()
        {
            string source = @"
                var a = ""global"";

                {
                    var a = ""shadow"";
                    print a;
                }

                print a;
            ";

            var output = EvalReturningOutput(source);

            Assert.Equal(new[]
            {
                "shadow",
                "global"
            }, output);
        }

        [Fact]
        public void shadow_local()
        {
            string source = @"
                {
                    var a = ""local"";

                    {
                        var a = ""shadow"";
                        print a;
                    }

                    print a;
                }
            ";

            var output = EvalReturningOutput(source);

            Assert.Equal(new[]
            {
                "shadow",
                "local"
            }, output);
        }

        [Fact]
        public void undefined_global()
        {
            string source = @"
                print not_defined;
            ";

            var result = EvalWithValidationErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Matches("Undefined identifier 'not_defined'", exception.Message);
        }

        [Fact]
        public void undefined_local()
        {
            string source = @"
                {
                    print not_defined;
                }
            ";

            var result = EvalWithValidationErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Matches("Undefined identifier 'not_defined'", exception.Message);
        }

        [Fact]
        public void uninitialized_global_without_type_throws_expected_error()
        {
            string source = @"
                var a;
            ";

            var result = EvalWithValidationErrorCatch(source);
            var exception = result.Errors.FirstOrDefault();

            Assert.Single(result.Errors);
            Assert.Matches("Type inference for variable 'a' cannot be performed when initializer is not specified", exception.Message);
        }

        [Fact]
        public void unreached_undefined()
        {
            // Early versions of Perlang inherited the semantics from Lox and other dynamic languages, like Ruby;
            // variable references were not analyzed unless the interpreter would actually reach the code in question.
            // Now that we have static type analysis in place, we also go for a much stricter model like most all
            // other compiled languages, detecting invalid variable references already at "compile-time".
            string source = @"
                if (false) {
                    print not_defined;
                }

                print ""ok"";
            ";

            var result = EvalWithValidationErrorCatch(source);
            var exception = result.Errors.FirstOrDefault();

            Assert.Single(result.Errors);
            Assert.Matches("Undefined identifier 'not_defined'", exception.Message);
        }

        [Fact]
        public void use_false_as_var()
        {
            string source = @"
                var false = ""value"";
            ";

            var result = EvalWithParseErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);

            Assert.Matches("Error at 'false': Expecting variable name", exception.ToString());
        }

        [Fact]
        public void use_global_in_initializer_for_global_variable_works()
        {
            string source = @"
                var a = ""value"";
                var b = a;
                print b;
            ";

            var output = EvalReturningOutputString(source);

            Assert.Equal("value", output);
        }

        [Fact]
        public void use_global_in_initializer_for_local_variable_fails()
        {
            string source = @"
                var a = ""outer"";
                
                {
                    var a = a;
                }
            ";

            var result = EvalWithNameResolutionErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Matches("Error at 'a': Cannot read local variable in its own initializer", exception.ToString());
        }

        [Fact]
        public void use_null_as_var()
        {
            string source = @"
                var null = ""value"";
            ";

            var result = EvalWithParseErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);

            Assert.Matches("Error at 'null': Expecting variable name.", exception.ToString());
        }

        [Fact]
        public void use_this_as_var()
        {
            string source = @"
                var this = ""value"";
            ";

            var result = EvalWithParseErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);

            Assert.Matches("Error at 'this': Expecting variable name", exception.ToString());
        }
    }
}
