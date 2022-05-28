#nullable enable
namespace Perlang.Parser;

public interface INumericLiteral
{
    public object Value { get; }

    /// <summary>
    /// Gets a number that indicates the actual size of the value in <see cref="Value"/>. This is used to be able to do
    /// things like "implicit casting" from `int` to `uint`, which is perfectly safe for positive integers.
    /// </summary>
    public long BitsUsed { get; }

    public bool IsPositive { get; }
}
