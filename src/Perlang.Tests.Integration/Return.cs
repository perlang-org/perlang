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

            var result = EvalWithNameResolutionErrorCatch(source);
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

        // TODO: This is an oversight; as of https://github.com/perlang-org/perlang/pull/54, these semantics should no
        // longer be supported. Automatically returning `null` when no value is provided is wrong. This should only be
        // supported when the return type is explicitly declared as `void`.
        [SkippableFact]
        public void return_null_if_no_value()
        {
            Skip.If(PerlangMode.ExperimentalCompilation, "Not supported in compiled mode");

            string source = @"
                fun f(): void {
                  return;
                  print ""bad"";
                }

                print f();
            ";

            string output = EvalReturningOutput(source).SingleOrDefault();

            Assert.Equal("null", output);
        }
    }
}
