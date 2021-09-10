using System.Collections.Generic;
using System.Linq;
using Perlang.Parser;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.ReservedKeywords
{
    public class ReservedKeywordsTests
    {
        // Special-cased test for this keyword, to ensure that it consumes the class name as expected
        [Fact]
        public void reserved_keyword_class_throws_expected_error()
        {
            string source = @"
                class Foo {}
            ";

            var result = EvalWithParseErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Matches($"Error at 'class': Expect expression", exception.ToString());
        }

        [Theory]
        [MemberData(nameof(GetReservedWords))]
        public void reserved_keyword_throws_expected_error(string reservedWord)
        {
            string source = @$"
                var {reservedWord} = 1;
            ";

            var result = EvalWithParseErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Matches($"Error at '{reservedWord}': Reserved word encountered", exception.ToString());
        }

        [Fact]
        public void reserved_keyword_public_throws_expected_error()
        {
            string source = @"
                var public = 1
            ";

            var result = EvalWithParseErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Matches("Error at 'public': Reserved word encountered", exception.ToString());
        }

        [Fact]
        public void reserved_keyword_private_throws_expected_error()
        {
            string source = @"
                var private = 1
            ";

            var result = EvalWithParseErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Matches("Error at 'private': Reserved word encountered", exception.ToString());
        }

        [Fact]
        public void reserved_keyword_protected_throws_expected_error()
        {
            string source = @"
                var protected = 1
            ";

            var result = EvalWithParseErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Matches("Error at 'protected': Reserved word encountered", exception.ToString());
        }

        [Fact]
        public void reserved_keyword_internal_throws_expected_error()
        {
            string source = @"
                var internal = 1
            ";

            var result = EvalWithParseErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Matches("Error at 'internal': Reserved word encountered", exception.ToString());
        }

        [Fact]
        public void reserved_keyword_static_throws_expected_error()
        {
            string source = @"
                var static = 1
            ";

            var result = EvalWithParseErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Matches("Error at 'static': Reserved word encountered", exception.ToString());
        }

        [Fact]
        public void reserved_keyword_volatile_throws_expected_error()
        {
            string source = @"
                var volatile = 1
            ";

            var result = EvalWithParseErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);
            Assert.Matches("Error at 'volatile': Reserved word encountered", exception.ToString());
        }

        //
        // Variable declarations: ensure that reserved keywords cannot be used as variable names
        //

        [Fact]
        public void reserved_keyword_int_cannot_be_used_as_variable_name()
        {
            string source = @"
                var int = 123;
            ";

            var result = EvalWithParseErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);

            Assert.Matches("Error at 'int': Reserved keyword encountered", exception.ToString());
        }

        [Fact]
        public void reserved_keyword_float_cannot_be_used_as_variable_name()
        {
            string source = @"
                var float = 123.45;
            ";

            var result = EvalWithParseErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);

            Assert.Matches("Error at 'float': Reserved word encountered", exception.ToString());
        }

        [Fact]
        public void reserved_keyword_string_cannot_be_used_as_variable_name()
        {
            string source = @"
                var string = ""foo"";
            ";

            var result = EvalWithParseErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);

            Assert.Matches("Error at 'string': Reserved keyword encountered", exception.ToString());
        }

        //
        // Functions: ensure that reserved keywords cannot be used as function names
        //

        [Fact]
        public void reserved_keyword_int_cannot_be_used_as_function_name()
        {
            string source = @"
                fun int(): void {
                }
            ";

            var result = EvalWithParseErrorCatch(source);
            var exception = result.Errors.FirstOrDefault();

            Assert.Single(result.Errors);
            Assert.Matches("Error at 'int': Reserved keyword encountered", exception.ToString());
        }

        [Fact]
        public void reserved_keyword_long_cannot_be_used_as_function_name()
        {
            string source = @"
                fun long(): void {
                }
            ";

            var result = EvalWithParseErrorCatch(source);
            var exception = result.Errors.FirstOrDefault();

            Assert.Single(result.Errors);
            Assert.Matches("Error at 'long': Reserved keyword encountered", exception.ToString());
        }

        [Fact]
        public void reserved_keyword_float_cannot_be_used_as_function_name()
        {
            string source = @"
                fun float(): void {
                }
            ";

            var result = EvalWithParseErrorCatch(source);
            var exception = result.Errors.FirstOrDefault();

            Assert.Single(result.Errors);
            Assert.Matches("Error at 'float': Expect function name", exception.ToString());
        }

        [Fact]
        public void reserved_keyword_string_cannot_be_used_as_function_name()
        {
            string source = @"
                fun string(): void {
                }
            ";

            var result = EvalWithParseErrorCatch(source);
            var exception = result.Errors.FirstOrDefault();

            Assert.Single(result.Errors);
            Assert.Matches("Error at 'string': Reserved keyword encountered", exception.ToString());
        }

        //
        // Functions: ensure that reserved words cannot be used as function parameter names
        //

        [Fact]
        public void reserved_keyword_int_cannot_be_used_as_function_parameter_name()
        {
            string source = @"
                fun foo(int: int): void {
                }
            ";

            var result = EvalWithParseErrorCatch(source);
            var exception = result.Errors.FirstOrDefault();

            Assert.Single(result.Errors);
            Assert.Matches("Error at 'int': Reserved keyword encountered", exception.ToString());
        }

        [Fact]
        public void reserved_keyword_long_cannot_be_used_as_function_parameter_name()
        {
            string source = @"
                fun foo(long: long): void {
                }
            ";

            var result = EvalWithParseErrorCatch(source);
            var exception = result.Errors.FirstOrDefault();

            Assert.Single(result.Errors);
            Assert.Matches("Error at 'long': Reserved keyword encountered", exception.ToString());
        }

        [Fact]
        public void reserved_keyword_float_cannot_be_used_as_function_parameter_name()
        {
            string source = @"
                fun foo(float: float): void {
                }
            ";

            var result = EvalWithParseErrorCatch(source);
            var exception = result.Errors.FirstOrDefault();

            Assert.Single(result.Errors);
            Assert.Matches("Error at 'float': Reserved word encountered", exception.ToString());
        }

        [Fact]
        public void reserved_keyword_string_cannot_be_used_as_function_parameter_name()
        {
            string source = @"
                fun foo(string: string): void {
                }
            ";

            var result = EvalWithParseErrorCatch(source);
            var exception = result.Errors.FirstOrDefault();

            Assert.Single(result.Errors);
            Assert.Matches("Error at 'string': Reserved keyword encountered", exception.ToString());
        }

        //
        // Return types: ensure that reserved keywords cannot be used as return types
        //
        // Note that as 'byte', 'sbyte', 'float' etc are not valid types name yet, this test expects a particular error
        // to be thrown. For more details on reserved words, see #178. (Technically, these test does not exercise the
        // "reserved words" code paths at all; since the type names aren't defined, they fail as they would with any
        // other non-defined type name.)

        [Fact]
        public void function_return_type_detects_reserved_keyword_byte()
        {
            string source = @"
                fun foo(): byte {
                    return 123.45;
                }
            ";

            var result = EvalWithParseErrorCatch(source);
            var exception = result.Errors.FirstOrDefault();

            Assert.Single(result.Errors);
            Assert.Matches("Error at 'byte': Expecting type name", exception.ToString());
        }

        [Fact]
        public void function_return_type_detects_reserved_keyword_sbyte()
        {
            string source = @"
                fun foo(): sbyte {
                    return 123.45;
                }
            ";

            var result = EvalWithParseErrorCatch(source);
            var exception = result.Errors.FirstOrDefault();

            Assert.Single(result.Errors);
            Assert.Matches("Error at 'sbyte': Expecting type name", exception.ToString());
        }

        [Fact]
        public void function_return_type_detects_reserved_keyword_short()
        {
            string source = @"
                fun foo(): short {
                    return 123.45;
                }
            ";

            var result = EvalWithParseErrorCatch(source);
            var exception = result.Errors.FirstOrDefault();

            Assert.Single(result.Errors);
            Assert.Matches("Error at 'short': Expecting type name", exception.ToString());
        }

        [Fact]
        public void function_return_type_detects_reserved_keyword_ushort()
        {
            string source = @"
                fun foo(): ushort {
                    return 123.45;
                }
            ";

            var result = EvalWithParseErrorCatch(source);
            var exception = result.Errors.FirstOrDefault();

            Assert.Single(result.Errors);
            Assert.Matches("Error at 'ushort': Expecting type name", exception.ToString());
        }

        [Fact]
        public void function_return_type_detects_reserved_keyword_uint()
        {
            string source = @"
                fun foo(): uint {
                    return 123.45;
                }
            ";

            var result = EvalWithParseErrorCatch(source);
            var exception = result.Errors.FirstOrDefault();

            Assert.Single(result.Errors);
            Assert.Matches("Error at 'uint': Expecting type name", exception.ToString());
        }

        [Fact]
        public void function_return_type_detects_reserved_keyword_ulong()
        {
            string source = @"
                fun foo(): ulong {
                    return 123.45;
                }
            ";

            var result = EvalWithParseErrorCatch(source);
            var exception = result.Errors.FirstOrDefault();

            Assert.Single(result.Errors);
            Assert.Matches("Error at 'ulong': Expecting type name", exception.ToString());
        }

        [Fact]
        public void function_return_type_detects_reserved_keyword_float()
        {
            string source = @"
                fun foo(): float {
                    return 123.45;
                }
            ";

            var result = EvalWithParseErrorCatch(source);
            var exception = result.Errors.FirstOrDefault();

            Assert.Single(result.Errors);
            Assert.Matches("Error at 'float': Expecting type name", exception.ToString());
        }

        [Fact]
        public void function_return_type_detects_reserved_keyword_double()
        {
            string source = @"
                fun foo(): double {
                    return 123.45;
                }
            ";

            var result = EvalWithParseErrorCatch(source);
            var exception = result.Errors.FirstOrDefault();

            Assert.Single(result.Errors);
            Assert.Matches("Error at 'double': Expecting type name", exception.ToString());
        }

        [Fact]
        public void function_return_type_detects_reserved_keyword_decimal()
        {
            string source = @"
                fun foo(): decimal {
                    return 123.45;
                }
            ";

            var result = EvalWithParseErrorCatch(source);
            var exception = result.Errors.FirstOrDefault();

            Assert.Single(result.Errors);
            Assert.Matches("Error at 'decimal': Expecting type name", exception.ToString());
        }

        [Fact]
        public void function_return_type_detects_reserved_keyword_char()
        {
            string source = @"
                fun foo(): char {
                    return 123.45;
                }
            ";

            var result = EvalWithParseErrorCatch(source);
            var exception = result.Errors.FirstOrDefault();

            Assert.Single(result.Errors);
            Assert.Matches("Error at 'char': Expecting type name", exception.ToString());
        }

        public static IEnumerable<object[]> GetReservedWords()
        {
            return Scanner.ReservedKeywordStrings
                .Select(s => new object[] { s });
        }
    }
}
