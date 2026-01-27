#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Perlang.Interpreter.Extensions;
using Perlang.Parser;

namespace Perlang.Interpreter.Typing;

/// <summary>
/// Handles type coercions.
/// </summary>
public static class TypeCoercer
{
    internal static ImmutableDictionary<CppType, int?> SignedIntegerLengthByType => new Dictionary<CppType, int?>
    {
        { PerlangValueTypes.Int32, 31 }, // 32:nd bit is signed/unsigned.
        { PerlangValueTypes.Int64, 64 },

        // In practice, even larger numbers should be possible. For the time being, I think it's quite fine if
        // bigints in Perlang are limited to 2 billion digits. :)
        { PerlangValueTypes.BigInt, Int32.MaxValue }
    }.ToImmutableDictionary();

    // Not supported by explicit type definitions yet, but can be used implicitly because of type inference.
    internal static ImmutableDictionary<CppType, int?> UnsignedIntegerLengthByType => new Dictionary<CppType, int?>
    {
        { PerlangValueTypes.UInt32, 32 },
        { PerlangValueTypes.UInt64, 64 },

        // In practice, even larger numbers should be possible. For the time being, I think it's quite fine if
        // bigints in Perlang are limited to 2 billion digits. :)
        { PerlangValueTypes.BigInt, Int32.MaxValue }
    }.ToImmutableDictionary();

    internal static ImmutableDictionary<CppType, int?> FloatIntegerLengthByType => new Dictionary<CppType, int?>
    {
        // Single-precision values are 32-bit but can store numbers between 1.4E-45 and ~3.40E38 (with data loss,
        // i.e. numbers larger or equal than +/- 2^24 cannot be exactly represented. We presume people working with
        // numbers this large to be (or make themselves aware of) this limitation.)
        { PerlangValueTypes.Float, 32 },

        // Double-precision values are 64-bit but can store numbers between 4.9E-324 and ~1.80E308 (with data loss,
        // i.e. numbers larger or equal than +/- 2^53 cannot be exactly represented. We presume people working with
        // numbers this large to be (or make themselves aware of) this limitation.)
        { PerlangValueTypes.Double, 64 }
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
        if (targetTypeReference.IsArray && sourceTypeReference.IsArray) {
            // This is a bit too relaxed, but we need it like this to support assignment of e.g. int[] collection
            // initializers (with integer constants) to long[] variables. The C++ compilation will hopefully catch
            // scenarios that won't really work, but we should improve on this at some point.
            return CanBeCoercedInto(targetTypeReference.CppType?.ElementType, sourceTypeReference.CppType?.ElementType);
        }
        else {
            return CanBeCoercedInto(targetTypeReference.CppType, sourceTypeReference.CppType, numericLiteral);
        }
    }

    /// <summary>
    /// Determines if all given literals of <paramref name="sourceType"/> can be coerced into <paramref
    /// name="targetType"/>.
    ///
    /// The `source` and `target` concepts are important here. Sometimes values can be coerced in one direction
    /// but not the other. For example, an `int` can be coerced to a `long`, but not the other way around
    /// (without an explicit type cast). The same goes for unsigned integer types; they can not be coerced to
    /// their signed counterpart (e.g. `uint` -> `int`), but they can be coerced to a larger signed type if
    /// available (e.g. `uint` to `long`).
    /// </summary>
    /// <param name="targetType">The target type.</param>
    /// <param name="sourceType">The source type.</param>
    /// <param name="literals">The literals in the collection initializer.</param>
    /// <returns>`true` if all literals can be coerced into the target type, `false` otherwise.</returns>
    public static bool CanBeCoercedInto(CppType targetType, CppType sourceType, IEnumerable<Expr.Literal> literals) =>
        literals.All(l => CanBeCoercedInto(targetType.ElementType, sourceType.ElementType, l.Value as INumericLiteral));

    // TODO: Consider merging this with CppType.IsAssignableTo(). Right now, these methods both perform part of the
    // logic for assignment coercion; it could make sense to merge it all into a single method. The CanBeCoercedInto()
    // method was created at a time when CppType didn't exist, so adding custom logic to the CLR Type class was not an
    // option at that time.

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
    public static bool CanBeCoercedInto(CppType? targetType, CppType? sourceType, INumericLiteral? numericLiteral = null)
    {
        if (targetType == null)
        {
            throw new ArgumentNullException("targetType cannot be null", innerException: null);
        }

        if (sourceType == null)
        {
            throw new ArgumentNullException("sourceType cannot be null", innerException: null);
        }

        if (targetType == sourceType)
        {
            return true;
        }

        if (sourceType?.IsAssignableTo(targetType) == true)
        {
            return true;
        }

        // TODO: Ensure we have checks that validate that `var i: int = null` etc fails for all supported numeric
        // TODO: types. The check below lacks many value types.

        if (sourceType == PerlangTypes.NullObject &&
            targetType != PerlangValueTypes.Int32 &&
            targetType != PerlangValueTypes.Float)
        {
            // Reassignment to `null` is valid as long as the target is not a value type. In other words, reference
            // types are nullable by default. We do emit a compiler warning though, and depending on the
            // configuration of the frontend, this warning can cause compilation to fail (if the frontend is
            // configured to disallow compiler warnings).
            return true;
        }

        // TODO: I think we have an off-by-one-bit error here, in that the BitsUsed implementation only works correctly
        // for positive numbers:
        //
        // - Math.Ceiling(Math.Log2(n)) will return a negative value for negative numbers, and
        // - The number of bits used should include one extra bit, for the sign bit
        //
        // This might be moot, since negative numbers cannot be assigned to an unsigned type anyway, but we should add
        // tests (particularly close to the boundary conditions!) to validate that our code does the right thing in
        // these cases.

        long? sourceSize = numericLiteral?.BitsUsed ?? SignedIntegerLengthByType.TryGetObjectValue(sourceType!);
        int? targetSize = (numericLiteral is { IsPositive: true } ? UnsignedIntegerLengthByType.TryGetObjectValue(targetType!) : null) ??
                          SignedIntegerLengthByType.TryGetObjectValue(targetType!) ??
                          FloatIntegerLengthByType.TryGetObjectValue(targetType!);

        if (sourceSize == null || targetSize == null)
        {
            // One or both of the values involved are non-numeric. The coercion is normally unsupported in this
            // case, but let's check for assignability first: The target type can be a supertype of sourceType
            // (inheritance or interfaces), in which case the coercion is fine.
            if (sourceType?.IsAssignableTo(targetType) ?? false)
            {
                return true;
            }
            else
            {
                return false;
            }
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
