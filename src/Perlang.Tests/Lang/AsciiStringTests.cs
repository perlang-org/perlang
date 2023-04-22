using FluentAssertions;
using Perlang.Lang;
using Xunit;

namespace Perlang.Tests.Lang;

public class AsciiStringTests
{
    [Fact]
    void AsciiString_can_be_concatenated()
    {
        var foo = AsciiString.from("foo");
        var bar = AsciiString.from("bar");

        var foobar = foo + bar;

        foobar.Should()
            .Be(AsciiString.from("foobar"));
    }

    [Fact]
    void AsciiString_can_be_converted_to_NET_string()
    {
        var foo = AsciiString.from("foo");

        string result = foo.ToString();

        result.Should()
            .Be("foo");
    }

    [Fact]
    void AsciiString_ToUpper_converts_to_uppercase()
    {
        var s = AsciiString.from("Path");

        Perlang.Lang.String result = s.to_upper();

        result.Should()
            .Be(AsciiString.from("PATH"));
    }

    [Fact]
    void AsciiString_ToUpper_on_already_uppercased_string_is_noop()
    {
        var s = AsciiString.from("PATH");

        Perlang.Lang.String result = s.to_upper();

        result.Should()
            .Be(AsciiString.from("PATH"));
    }

    [Fact]
    void AsciiString_ToUpper_ToString_returns_expected_value()
    {
        var s = AsciiString.from("PATH");

        string result = s.to_upper().ToString();

        result.Should()
            .Be("PATH");
    }
}
