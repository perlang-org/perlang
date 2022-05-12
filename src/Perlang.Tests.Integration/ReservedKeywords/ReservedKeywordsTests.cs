using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
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

            result.Errors.Should()
                .ContainSingle().Which
                .ToString().Should().Contain("Error at 'class': Expect expression");
        }

        //
        // Variable declarations: ensure that reserved keywords cannot be used as variable names
        //

        [Theory]
        [MemberData(nameof(GetReservedWords))]
        public void reserved_keyword_cannot_be_used_as_variable_name(string reservedWord)
        {
            string source = $@"
                var {reservedWord} = 123;
            ";

            var result = EvalWithParseErrorCatch(source);

            result.Errors.Should()
                .ContainSingle().Which
                .ToString().Should().Contain($"Error at '{reservedWord}': Reserved keyword encountered");
        }

        //
        // Functions: ensure that reserved keywords cannot be used as function names
        //

        [Theory]
        [MemberData(nameof(GetReservedWordsOnly))]
        public void reserved_keyword_cannot_be_used_as_function_name(string reservedWord)
        {
            string source = $@"
                fun {reservedWord}(): void {{
                }}
            ";

            var result = EvalWithParseErrorCatch(source);

            result.Errors.Should()
                .ContainSingle().Which
                .ToString().Should().Contain($"Error at '{reservedWord}': Expect function name");
        }

        [Theory]
        [MemberData(nameof(GetTypeReservedWords))]
        public void reserved_type_keyword_cannot_be_used_as_function_name(string reservedWord)
        {
            string source = $@"
                fun {reservedWord}(): void {{
                }}
            ";

            var result = EvalWithParseErrorCatch(source);

            result.Errors.Should()
                .ContainSingle().Which
                .ToString().Should().Contain($"Error at '{reservedWord}': Reserved keyword encountered");
        }

        //
        // Functions: ensure that reserved words cannot be used as function parameter names
        //

        [Theory]
        [MemberData(nameof(GetReservedWords))]
        public void reserved_keyword_cannot_be_used_as_function_parameter_name(string reservedWord)
        {
            string source = $@"
                fun foo({reservedWord}: int): void {{
                }}
            ";

            var result = EvalWithParseErrorCatch(source);

            result.Errors.Should()
                .ContainSingle().Which
                .ToString().Should().Contain($"Error at '{reservedWord}': Reserved keyword encountered");
        }

        //
        // Return types: ensure that reserved keywords cannot be used as return types
        //

        [Theory]
        [MemberData(nameof(GetReservedWordsOnly))]
        public void function_return_type_detects_reserved_keyword(string reservedWord)
        {
            string source = $@"
                fun foo(): {reservedWord} {{
                    return 123;
                }}
            ";

            var result = EvalWithParseErrorCatch(source);

            result.Errors.Should()
                .ContainSingle().Which
                .ToString().Should().Contain($"Error at '{reservedWord}': Expecting type name");
        }

        public static IEnumerable<object[]> GetReservedWords() =>
            Scanner.ReservedKeywordStrings
                .Select(s => new object[] { s });

        public static IEnumerable<object[]> GetTypeReservedWords() =>
            Scanner.ReservedTypeKeywordStrings
                .Select(s => new object[] { s });

        public static IEnumerable<object[]> GetReservedWordsOnly() =>
            Scanner.ReservedKeywordOnlyStrings
                .Select(s => new object[] { s });
    }
}
