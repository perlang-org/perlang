using System.Linq;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration
{
    public class Return
    {
        // Tests based on Lox test suite: https://github.com/munificent/craftinginterpreters/tree/master/test/return

        [Fact]
        public void after_else()
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
        public void after_if()
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
        public void after_while()
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
        public void at_top_level()
        {
            string source = @"
                return ""wat"";
            ";

            var result = EvalWithResolveErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Matches("Cannot return from top-level code.", exception.Message);
        }

        [Fact]
        public void in_function()
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

        [Fact(Skip = "Blocked pending https://github.com/perlang-org/perlang/issues/66")]
        public void in_method()
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
        public void return_nil_if_no_value()
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
