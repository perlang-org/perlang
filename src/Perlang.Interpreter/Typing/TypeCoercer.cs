using System;
using Perlang.Parser;

namespace Perlang.Interpreter.Typing
{
    /// <summary>
    /// Handles type coercions.
    /// </summary>
    public class TypeCoercer
    {
        private readonly Action<CompilerWarning> compilerWarningCallback;

        public TypeCoercer(Action<CompilerWarning> compilerWarningCallback)
        {
            this.compilerWarningCallback = compilerWarningCallback;
        }

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
        /// <param name="token">The approximate location of the source token (the token from which the <paramref
        ///     name="sourceTypeReference"/> is derived).</param>
        /// <param name="targetTypeReference">A reference to the target type.</param>
        /// <param name="sourceTypeReference">A reference to the source type.</param>
        /// <returns>`true` if a source value can be coerced into the target type, `false` otherwise.</returns>
        public bool CanBeCoercedInto(Token token, TypeReference targetTypeReference, TypeReference sourceTypeReference)
        {
            return CanBeCoercedInto(token, targetTypeReference.ClrType, sourceTypeReference.ClrType);
        }

        /// <summary>
        /// Determines if a value of <paramref name="sourceType"/> can be coerced into
        /// <paramref name="targetType"/>.
        ///
        /// The `source` and `target` concepts are important here. Sometimes values can be coerced in one direction
        /// but not the other. For example, an `int` can be coerced to a `long`, but not the other way around
        /// (without an explicit type cast). The same goes for unsigned integer types; they can not be coerced to
        /// their signed counterpart (`uint` -> `int`), but they can be coerced to a larger signed type if
        /// available.
        /// </summary>
        /// <param name="token">The approximate location of the source token (the token from which the <paramref
        ///     name="sourceType"/> is derived).</param>
        /// <param name="targetType">The target type.</param>
        /// <param name="sourceType">The source type.</param>
        /// <returns>`true` if a source value can be coerced into the target type, `false` otherwise.</returns>
        public bool CanBeCoercedInto(Token token, Type targetType, Type sourceType)
        {
            // TODO: Implement some of these coercions being advertised in the XML docs. ;)
            if (targetType == sourceType)
            {
                return true;
            }

            if (sourceType == typeof(NullObject) &&
                targetType != typeof(int) &&
                targetType != typeof(float))
            {
                // Reassignment to `nil` is valid as long as the target is not a value type. In other words, reference
                // types are nullable by default. We do emit a compiler warning though, and depending on the
                // configuration of the front end, this warning can cause compilation to fail (if the front end is
                // configured to disallow compiler warnings).
                return true;
            }

            // None of the defined type coercions was successful.
            return false;
        }
    }
}
