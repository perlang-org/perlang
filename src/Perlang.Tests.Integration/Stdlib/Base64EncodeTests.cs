using System.Linq;
using System.Text;
using FluentAssertions;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Stdlib;

public class Base64EncodeTests
{
    [SkippableFact]
    public void Base64_encode_with_no_arguments_throws_the_expected_exception()
    {
        var result = EvalWithValidationErrorCatch("Base64.encode()");
        var exception = result.Errors.First();

        Assert.Single(result.Errors);
        Assert.Contains("Method 'encode' has 1 parameter(s) but was called with 0 argument(s)", exception.Message);
    }

    [SkippableFact]
    public void Base64_encode_with_a_string_argument_returns_the_expected_result()
    {
        Skip.If(PerlangMode.ExperimentalCompilation, "Not supported in compiled mode");

        string source = @"
               print Base64.encode(""hej hej"");
            ";

        string output = EvalReturningOutputString(source);

        output.Should()
            .Be("aGVqIGhlag==");
    }

    [SkippableFact]
    public void Base64_encode_with_a_long_string_argument_returns_the_expected_result()
    {
        Skip.If(PerlangMode.ExperimentalCompilation, "Not supported in compiled mode");

        var sb = new StringBuilder();

        for (int i = 0; i < 4; i++)
        {
            sb.Append("hej hej, hemskt mycket hej");
        }

        string source = $@"
               print Base64.encode(""{sb}"");
            ";

        // At the moment, all lines are wrapped at every 76 characters. We could consider to make this configurable,
        // but it's awkward until we support method overloading.
        string output = EvalReturningOutputString(source);

        output.Should()
            .Be("aGVqIGhlaiwgaGVtc2t0IG15Y2tldCBoZWpoZWogaGVqLCBoZW1za3QgbXlja2V0IGhlamhlaiBo\r\n" +
                "ZWosIGhlbXNrdCBteWNrZXQgaGVqaGVqIGhlaiwgaGVtc2t0IG15Y2tldCBoZWo="
            );
    }

    [SkippableFact]
    public void Base64_encode_with_a_numeric_argument_throws_the_expected_exception()
    {
        var result = EvalWithValidationErrorCatch("Base64.encode(123)");
        var runtimeError = result.Errors.First();

        Assert.Single(result.Errors);

        Assert.Equal("Cannot pass int argument as string parameter to encode()", runtimeError.Message);
    }
}