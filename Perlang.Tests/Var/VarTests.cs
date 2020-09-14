using System.Linq;
using Xunit;
using static Perlang.Tests.EvalHelper;

namespace Perlang.Tests.Var
{
    // Tests based on Lox test suite: https://github.com/munificent/craftinginterpreters/tree/master/test/variable
    public class VarTests
    {
        [Fact]
        public void collide_with_parameter_throws_expected_error()
        {
            string source = @"
                fun foo(a) {
                    var a;
                }
            ";

            var result = EvalWithResolveErrorCatch(source);
            var exception = result.ResolveErrors.First();

            Assert.Single(result.ResolveErrors);
            Assert.Matches("Error at 'a': Variable with this name already declared in this scope.",
                exception.ToString());
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

            var result = EvalWithResolveErrorCatch(source);
            var exception = result.ResolveErrors.First();

            Assert.Single(result.ResolveErrors);
            Assert.Matches("Error at 'a': Variable with this name already declared in this scope.",
                exception.ToString());
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

            var result = EvalWithResolveErrorCatch(source);
            var exception = result.ResolveErrors.First();

            Assert.Single(result.ResolveErrors);
            Assert.Matches("Error at 'arg': Variable with this name already declared in this scope.",
                exception.ToString());
        }

        [Fact]
        public void early_bound()
        {
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

        [Fact]
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

            var output = EvalReturningOutput(source);

            Assert.Equal(new[]
            {
                "outer"
            }, output);
        }

        [Fact(Skip = "Blocked pending https://github.com/perlun/perlang/issues/66")]
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

            var output = EvalReturningOutput(source);

            Assert.Equal(new[]
            {
                "variable"
            }, output);
        }

        [Fact]
        public void redeclare_global()
        {
            string source = @"
                var a = 1;
                var a = 2;
                print a;
            ";

            var output = EvalReturningOutput(source);

            Assert.Equal(new[]
            {
                "2"
            }, output);
        }

        [Fact]
        public void redefine_global()
        {
            string source = @"
                var a = 1;
                var a = 2;
                print a;
            ";

            var output = EvalReturningOutput(source);

            Assert.Equal(new[]
            {
                "2"
            }, output);
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

            var result = EvalWithTypeValidationErrorCatch(source);
            var exception = result.TypeValidationErrors.First();

            Assert.Single(result.TypeValidationErrors);
            Assert.Matches("Undefined variable 'not_defined'", exception.Message);
        }

        [Fact]
        public void undefined_local()
        {
            string source = @"
                {
                    print not_defined;
                }
            ";

            var result = EvalWithTypeValidationErrorCatch(source);
            var exception = result.TypeValidationErrors.First();

            Assert.Single(result.TypeValidationErrors);
            Assert.Matches("Undefined variable 'not_defined'", exception.Message);
        }

        [Fact]
        public void uninitialized_global_without_type_throws_expected_error()
        {
            string source = @"
                var a;
            ";

            var result = EvalWithTypeValidationErrorCatch(source);
            var exception = result.TypeValidationErrors.FirstOrDefault();

            Assert.Single(result.TypeValidationErrors);
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

            var result = EvalWithTypeValidationErrorCatch(source);
            var exception = result.TypeValidationErrors.FirstOrDefault();

            Assert.Single(result.TypeValidationErrors);
            Assert.Matches("Undefined variable 'not_defined'", exception.Message);
        }

        [Fact]
        public void use_false_as_var()
        {
            string source = @"
                var false = ""value"";
            ";

            var result = EvalWithParseErrorCatch(source);
            var exception = result.ParseErrors.First();

            Assert.Single(result.ParseErrors);

            Assert.Matches("Error at 'false': Expecting variable name", exception.ToString());
        }

        [Fact]
        public void redefining_global_variable_works()
        {
            // TODO: I'm not at all convinced this is such a great thing to support. Investigate how Java/C#/Rust
            // TODO: handles this. I _think_ that it actually works in Rust, but not in the others.
            // TODO:
            // TODO: C#: fails
            // TODO: Java: ??
            // TODO: Rust: works?
            string source = @"
                var a = 1;
                var a = 2;
                print a;
            ";

            var output = EvalReturningOutput(source);

            Assert.Equal(new[]
            {
                "2"
            }, output);
        }

        [Fact]
        public void use_global_in_initializer_for_global_variable_works()
        {
            string source = @"
                var a = ""value"";
                var b = a;
                print b;
            ";

            var output = EvalReturningOutput(source);

            Assert.Equal(new[]
            {
                "value"
            }, output);
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

            var result = EvalWithResolveErrorCatch(source);
            var exception = result.ResolveErrors.First();

            Assert.Single(result.ResolveErrors);
            Assert.Matches("Error at 'a': Cannot read local variable in its own initializer", exception.ToString());
        }

        [Fact]
        public void use_nil_as_var()
        {
            string source = @"
                var nil = ""value"";
            ";

            var result = EvalWithParseErrorCatch(source);
            var exception = result.ParseErrors.First();

            Assert.Single(result.ParseErrors);

            Assert.Matches("Error at 'nil': Expecting variable name.", exception.ToString());
        }

        [Fact]
        public void use_this_as_var()
        {
            string source = @"
                var this = ""value"";
            ";

            var result = EvalWithParseErrorCatch(source);
            var exception = result.ParseErrors.First();

            Assert.Single(result.ParseErrors);

            Assert.Matches("Error at 'this': Expecting variable name", exception.ToString());
        }
    }
}
