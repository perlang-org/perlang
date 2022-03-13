#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Numerics;
using Perlang.Interpreter.Extensions;
using Perlang.Parser;

namespace Perlang.Interpreter.Typing
{
    /// <summary>
    /// Handles type coercions.
    /// </summary>
    public static class TypeCoercer
    {
        internal static ImmutableDictionary<Type, int?> SignedIntegerLengthByType => new Dictionary<Type, int?>
        {
            { typeof(Int32), 31 }, // 32:nd bit is signed/unsigned.
            { typeof(Int64), 64 },

            // In practice, even larger numbers should be possible. For the time being, I think it's quite fine if
            // bigints in Perlang are limited to 2 billion digits. :)
            { typeof(BigInteger), Int32.MaxValue }
        }.ToImmutableDictionary();

        // Not supported by explicit type definitions yet, but can be used implicitly because of type inference.
        internal static ImmutableDictionary<Type, int?> UnsignedIntegerLengthByType => new Dictionary<Type, int?>
        {
            { typeof(UInt32), 32 },
            { typeof(UInt64), 64 },

            // In practice, even larger numbers should be possible. For the time being, I think it's quite fine if
            // bigints in Perlang are limited to 2 billion digits. :)
            { typeof(BigInteger), Int32.MaxValue }
        }.ToImmutableDictionary();

        internal static ImmutableDictionary<Type, int?> FloatIntegerLengthByType => new Dictionary<Type, int?>
        {
            // Double-precision values are 64-bit but can only save 53-bit integers with exact precision. Since there
            // are no "53-bit" types, we just make this the "next smaller available" bit length.
            { typeof(Double), 32 }
        }.ToImmutableDictionary();

        /// <summary>
        /// Determines if a value of <paramref name="sourceTypeReference"/> can be coerced into
        /// <paramref name="targetTypeReference"/>.
        ///
        /// The `source` and `target` concepts are important here. Sometimes values can be coerced in one direction
        /// but not the other. For example, an `int` can be coerced to a `long`, but not the other way around
        /// (without an explicit type cast). The same goes for unsigned integer types; they can not be coerced to
        /// their signed counterpart (`uint` -> `int`), but they can be coerced to a larger signed type if
        /// available.
        /// </summary>
        /// <param name="targetTypeReference">A reference to the target type.</param>
        /// <param name="sourceTypeReference">A reference to the source type.</param>
        /// <param name="numericLiteral">If the source is a numeric literal, this parameter holds data about it. If not,
        ///                              this will be `null`.</param>
        /// <returns>`true` if a source value can be coerced into the target type, `false` otherwise.</returns>
        public static bool CanBeCoercedInto(ITypeReference targetTypeReference, ITypeReference sourceTypeReference, INumericLiteral? numericLiteral)
        {
            return CanBeCoercedInto(targetTypeReference.ClrType, sourceTypeReference.ClrType, numericLiteral);
        }

        /// <summary>
        /// Determines if a value of <paramref name="sourceType"/> can be coerced into
        /// <paramref name="targetType"/>.
        ///
        /// The `source` and `target` concepts are important here. Sometimes values can be coerced in one direction
        /// but not the other. For example, an `int` can be coerced to a `long`, but not the other way around
        /// (without an explicit type cast). The same goes for unsigned integer types; they can not be coerced to
        /// their signed counterpart (e.g. `uint` -> `int`), but they can be coerced to a larger signed type if
        /// available (e.g. `uint` to `long`).
        /// </summary>
        /// <param name="targetType">The target type.</param>
        /// <param name="sourceType">The source type.</param>
        /// <param name="numericLiteral">If the source is a numeric literal, this parameter holds data about it. If not,
        ///                              this will be `null`.</param>
        /// <returns>`true` if a source value can be coerced into the target type, `false` otherwise.</returns>
        public static bool CanBeCoercedInto(Type? targetType, Type? sourceType, INumericLiteral? numericLiteral)
        {
            if (targetType == sourceType)
            {
                return true;
            }

            // TODO: Ensure we have checks that validate that `var i: int = null` etc fails for all supported numeric types.

            if (sourceType == typeof(NullObject) &&
                targetType != typeof(int) &&
                targetType != typeof(float))
            {
                // Reassignment to `null` is valid as long as the target is not a value type. In other words, reference
                // types are nullable by default. We do emit a compiler warning though, and depending on the
                // configuration of the front end, this warning can cause compilation to fail (if the front end is
                // configured to disallow compiler warnings).
                return true;
            }

            long? sourceSize = numericLiteral?.BitsUsed ?? SignedIntegerLengthByType.TryGetObjectValue(sourceType!);
            int? targetSize = (numericLiteral is { IsPositive: true } ? UnsignedIntegerLengthByType.TryGetObjectValue(targetType!) : null) ??
                              SignedIntegerLengthByType.TryGetObjectValue(targetType!) ??
                              FloatIntegerLengthByType.TryGetObjectValue(targetType!);

            if (sourceSize == null || targetSize == null)
            {
                // One or both of the values involved are non-numeric. The coercion is unsupported in this case.
                return false;
            }

            // Expansions are fine; in other words, as long as the target type is wider (number of bits) or than or
            // equal to the source type, the conversion will always work.
            if (targetSize >= sourceSize)
            {
                return true;
            }

            // None of the defined type coercions was successful.
            return false;
        }
    }
}
