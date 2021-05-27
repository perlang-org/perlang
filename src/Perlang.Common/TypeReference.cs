#nullable enable
using System;

namespace Perlang
{
    /// <summary>
    /// A type reference is a (potentially unresolved) reference to a Perlang type.
    ///
    /// The object is mutable.
    /// </summary>
    public class TypeReference
    {
        public static TypeReference Bool { get; } = new TypeReference(typeof(bool));

        private Type? clrType;
        public Token? TypeSpecifier { get; }

        public Type? ClrType
        {
            get => clrType;
            set
            {
                if (clrType != null && clrType != value)
                {
                    // Poor-man's fake immutability. The problem is that the CLR type isn't really known at
                    // construction time of the TypeReference; it is therefore always null. It gets populated
                    // at a later stage in the compilation. But, how do we mutate it? We must either allow this
                    // property to have a setter, or discard the TypeReference altogether and create a new one.
                    // If we go the latter route, we just postpone the problem though; e.g. Stmt.Var.TypeReference
                    // is not mutable either. So we would have to replace the Stmt node altogether, and so forth,
                    // essentially discarding the whole AST for each node being handled by the type resolver.
                    //
                    // This is of course completely absurd and unreasonable. It has been said that an AST is a good
                    // example of where immutability doesn't fit that well into the picture, and the above explanation
                    // is one of the reasons why.
                    //
                    // So: the best we can do is to ensure that once the property is mutated, it is at least only
                    // ever mutated _once_. I wonder if that will work all the way or if there any other nice
                    // gotchas to be discovered further along the road... :-)
                    throw new ArgumentException(
                        $"ClrType already set to {clrType}; property is read-only. Attempted to " +
                        $"set it to {value}");
                }

                clrType = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the type reference contains an explicit type specifier or not. If this is
        /// false, the user is perhaps intending for the type to be inferred from the program context.
        /// </summary>
        public bool ExplicitTypeSpecified => TypeSpecifier != null;

        /// <summary>
        /// Gets a value indicating whether the type reference has been successfully resolved to a (loaded) CLR type or
        /// not.
        /// </summary>
        public bool IsResolved => ClrType != null;

        /// <summary>
        /// Gets a value indicating whether this type reference refers to a `null` value.
        /// </summary>
        public bool IsNullObject => ClrType == typeof(NullObject);

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeReference"/> class, for a given type specifier. The type
        /// specifier can be null, in which case type inference will be attempted.
        /// </summary>
        /// <param name="typeSpecifier">The token providing the type specifier (e.g. 'int' or 'string').</param>
        public TypeReference(Token? typeSpecifier)
        {
            TypeSpecifier = typeSpecifier;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeReference"/> class, for a given CLR type.
        /// </summary>
        /// <param name="clrType">The CLR type..</param>
        public TypeReference(Type clrType)
        {
            // TODO: Remove once we are done with https://github.com/perlang-org/perlang/issues/39
            this.clrType = clrType ?? throw new ArgumentException("clrType cannot be null");
        }

        public override string ToString()
        {
            if (ExplicitTypeSpecified)
            {
                return IsResolved ? $"Explicit: {ClrType}" : $"Explicit: {TypeSpecifier}";
            }
            else
            {
                return IsResolved ? $"Inferred: {ClrType}" : "Inferred, not yet resolved";
            }
        }
    }
}
