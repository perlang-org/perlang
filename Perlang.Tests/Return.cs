using System.Linq;
using Xunit;
using static Perlang.Tests.EvalHelper;

namespace Perlang.Tests
{
    public class Return
    {
        // Tests based on Lox test suite: https://github.com/munificent/craftinginterpreters/tree/master/test/return

        [Fact]
        void after_else()
        {
            string source = @"
                fun f(): string {
                  if (false) ""no""; else return ""ok"";
                }

                print f();
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("ok", output);
        }

        [Fact]
        void after_if()
        {
            string source = @"
                fun f(): string {
                  if (true) return ""ok"";
                }

                print f();
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("ok", output);
        }

        [Fact]
        void after_while()
        {
            string source = @"
                fun f(): string {
                  while (true) return ""ok"";
                }

                print f();
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("ok", output);
        }

        [Fact]
        void at_top_level()
        {
            string source = @"
                return ""wat"";
            ";

            var result = EvalWithResolveErrorCatch(source);
            var exception = result.ResolveErrors.First();

            Assert.Single(result.ResolveErrors);
            Assert.Matches("Cannot return from top-level code.", exception.Message);
        }

        [Fact]
        void in_function()
        {
            string source = @"
                fun f(): string {
                  return ""ok"";
                  print ""bad"";
                }

                print f();
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("ok", output);
        }

        [Fact(Skip = "Blocked pending https://github.com/perlun/perlang/issues/66")]
        void in_method()
        {
            string source = @"
                class Foo {
                  method() {
                    return ""ok""
                    print ""bad"";
                  }
                }

                print Foo().method();
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("ok", output);
        }

        [Fact]
        void return_nil_if_no_value()
        {
            string source = @"
                fun f(): string {
                  return;
                  print ""bad"";
                }

                print f();
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("nil", output);
        }
    }
}
