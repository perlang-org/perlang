using System;
using System.Linq;
using FluentAssertions;
using Perlang.Compiler;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Assignment
{
    /// <summary>
    /// Assignment-related tests.
    ///
    /// Based on https://github.com/munificent/craftinginterpreters/tree/master/test/assignment.
    /// </summary>
    public class AssignmentTests
    {
        [Fact]
        public void assignment_is_right_associative()
        {
            string source = @"
                var a = ""a"";
                var b = ""b"";
                var c = ""c"";

                // Assignment is right-associative; the value of c is expected to be propagated to both 'b' and 'a'
                // below.
                a = b = c;
                print a;
                print b;
                print c;
            ";

            var output = EvalReturningOutput(source);

            Assert.Equal(new[]
            {
                "c",
                "c",
                "c"
            }, output);
        }

        [Fact]
        public void assignment_on_right_hand_side_of_variable_is_right_associative()
        {
            string source = @"
                var a = ""before"";
                var c = a = ""var"";
                print a;
                print c;
            ";

            var output = EvalReturningOutput(source);

            Assert.Equal(new[]
            {
                "var",
                "var"
            }, output);
        }

        [Fact]
        public void global_variable_can_be_reassigned()
        {
            string source = @"
                var a = ""before"";
                print a;

                a = ""after"";
                print a;

                print a = ""arg"";
                print a;
            ";

            var output = EvalReturningOutput(source);

            Assert.Equal(new[]
            {
                "before",
                "after",
                "arg",
                "arg"
            }, output);
        }

        [Fact]
        public void local_variable_can_be_reassigned()
        {
            string source = @"
                {
                    var a = ""before"";
                    print a;

                    a = ""after"";
                    print a;

                    print a = ""arg"";
                    print a;
                }
            ";

            var output = EvalReturningOutput(source);

            Assert.Equal(new[]
            {
                "before",
                "after",
                "arg",
                "arg"
            }, output);
        }

        [Fact]
        public void grouping_is_not_a_valid_assignment_target()
        {
            string source = @"
                var a = ""a"";
                (a) = ""value"";
            ";

            var result = EvalWithParseErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Matches("Invalid assignment target.", exception.Message);
        }

        [Fact]
        public void infix_operator_is_not_a_valid_assignment_target()
        {
            string source = @"
                var a = ""a"";
                var b = ""b"";
                a + b = ""value"";
            ";

            var result = EvalWithParseErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Matches("Invalid assignment target.", exception.Message);
        }

        [Fact]
        public void prefix_operator_is_not_a_valid_assignment_target()
        {
            string source = @"
                var a = ""a"";
                !a = ""value"";
            ";

            var result = EvalWithParseErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Matches("Invalid assignment target.", exception.Message);
        }

        [Fact(Skip = "Blocked pending https://github.com/perlang-org/perlang/issues/66")]
        public void this_is_not_a_valid_assignment_target()
        {
            string source = @"
                class Foo {
                    Foo() {
                        this = ""value:"";
                    }
                }

                Foo();
            ";

            var result = EvalWithParseErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Matches("Invalid assignment target.", exception.Message);
        }

        [Fact]
        public void undefined_variable_cannot_be_reassigned()
        {
            string source = @"
                unknown = ""what"";
            ";

            if (PerlangMode.ExperimentalCompilation) {
                Action action = () => EvalReturningOutput(source);

                action.Should().Throw<PerlangCompilerException>()
                    .WithMessage("*use of undeclared identifier 'unknown'*");
            }
            else {
                var result = EvalWithRuntimeErrorCatch(source);
                var exception = result.Errors.First();

                Assert.Single(result.Errors);
                Assert.Matches("Undefined variable 'unknown'.", exception.Message);
            }
        }
    }
}
