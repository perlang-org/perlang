using FluentAssertions;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Comments;

/// <summary>
/// Tests for comments
///
/// Based on https://github.com/munificent/craftinginterpreters/tree/master/test/comments.
/// </summary>
public class CommentsTests
{
    [Fact]
    public void line_comment_at_eof()
    {
        string source = @"
                print ""ok"";
                // comment";

        var output = EvalReturningOutputString(source);

        Assert.Equal("ok", output);
    }

    [Fact]
    public void only_line_comment()
    {
        string source = "// comment";

        var output = EvalReturningOutputString(source);

        output.Should()
            .BeEmpty();
    }

    [Fact]
    public void only_line_comment_and_line()
    {
        string source = "// comment\n";

        var output = EvalReturningOutputString(source);

        output.Should()
            .BeEmpty();
    }

    [Fact]
    public void unicode_allowed_in_comments()
    {
        string source = @"
                // Unicode characters are allowed in comments.
                //
                // Latin 1 Supplement: £§¶ÜÞ
                // Latin Extended-A: ĐĦŋœ
                // Latin Extended-B: ƂƢƩǁ
                // Other stuff: ឃᢆ᯽₪ℜ↩⊗┺░
                // Emoji: ☃☺♣

                print ""ok"";
            ";

        var output = EvalReturningOutputString(source);

        Assert.Equal("ok", output);
    }
}