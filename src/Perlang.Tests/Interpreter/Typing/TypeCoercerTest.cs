using FluentAssertions;
using Perlang.Interpreter.Typing;
using Perlang.Parser;
using Xunit;

namespace Perlang.Tests.Interpreter.Typing;

public static class TypeCoercerTest
{
    public class CanBeCoercedInto
    {
        [Fact]
        void returns_true_for_int_to_long()
        {
            bool result = TypeCoercer.CanBeCoercedInto(typeof(long), typeof(int), null);

            result.Should()
                .BeTrue();
        }

        [Fact]
        void returns_false_for_long_to_int()
        {
            bool result = TypeCoercer.CanBeCoercedInto(typeof(int), typeof(long), null);

            result.Should()
                .BeFalse();
        }

        [Fact]
        void returns_true_for_int_to_uint_with_31_bit_positive_literal_integer()
        {
            bool result = TypeCoercer.CanBeCoercedInto(typeof(int), typeof(uint), new IntegerLiteral<int>(2147483647));

            result.Should()
                .BeTrue();
        }

        // These conversions are expected to fail, since the 32nd bit is the sign bit; a 32-bit `uint` cannot be stored
        // in a 32-bit `int` without data loss.
        [Fact]
        void returns_false_for_int_to_uint_with_32_bit_positive_literal_integer()
        {
            bool result = TypeCoercer.CanBeCoercedInto(typeof(int), typeof(uint), new IntegerLiteral<uint>(4294967295));

            result.Should()
                .BeFalse();
        }
    }
}
