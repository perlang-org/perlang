using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.For
{
    // Tests based on https://github.com/munificent/craftinginterpreters/blob/master/test/for/syntax.lox
    public class Syntax
    {
        [Fact]
        public void single_expression_body()
        {
            string source = @"
                for (var c = 0; c < 3;)
                    print c = c + 1;
            ";

            var output = EvalReturningOutput(source);

            Assert.Equal(new[]
            {
                "1",
                "2",
                "3"
            }, output);
        }

        [Fact]
        public void block_body()
        {
            string source = @"
                for (var a = 0; a < 3; a = a + 1) {
                  print a;
                }
            ";

            var output = EvalReturningOutput(source);

            Assert.Equal(new[]
            {
                "0",
                "1",
                "2"
            }, output);
        }

        [Fact]
        public void no_clauses()
        {
            string source = @"
                fun foo(): string {
                  for (;;) return ""done"";
                }
                print foo();
            ";

            var output = EvalReturningOutput(source);

            Assert.Equal(new[] {"done"}, output);
        }

        [Fact]
        public void no_variable()
        {
            string source = @"
                var i = 0;
                for (; i < 2; i = i + 1) print i;
            ";

            var output = EvalReturningOutput(source);

            Assert.Equal(new[]
            {
                "0",
                "1",
            }, output);
        }

        [Fact]
        public void no_condition()
        {
            string source = @"
                fun bar(): void {
                  for (var i = 0;; i = i + 1) {
                    print i;
                    if (i >= 2) return;
                  }
                }
                bar();
            ";

            var output = EvalReturningOutput(source);

            Assert.Equal(new[]
            {
                "0",
                "1",
                "2"
            }, output);
        }

        [Fact]
        public void no_increment()
        {
            string source = @"
                for (var i = 0; i < 2;) {
                  print i;
                  i = i + 1;
                }
            ";

            var output = EvalReturningOutput(source);

            Assert.Equal(new[]
            {
                "0",
                "1",
            }, output);
        }

        [Fact]
        public void statement_bodies()
        {
            string source = @"
                for (; false;) if (true) 1; else 2;
                for (; false;) while (true) 1;
                for (; false;) for (;;) 1;
            ";

            var output = EvalReturningOutput(source);

            Assert.Equal(new string[] { }, output);
        }
    }
}
