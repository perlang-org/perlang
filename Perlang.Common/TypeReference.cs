using System;

namespace Perlang
{
    /// <summary>
    /// A type reference is a (potentially unresolved) reference to a Perlang type.
    /// </summary>
    public class TypeReference
    {
        /// <summary>
        /// A "None" type reference, indicating that no type information is available. Note that this is different
        /// from an unresolved type reference, which is the state of a TypeReference before type inference has taken
        /// place.
        /// </summary>
        public static TypeReference None { get; } = new TypeReference(null);

        private Type clrType;
        public Token TypeSpecifier { get; }

        public Type ClrType
        {
            get => clrType;
            set
            {
                if (clrType != null)
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
                    throw new ArgumentException("ClrType already set; property is read-only");
                }

                clrType = value;
            }
        }

        /// <summary>
        /// The type reference contains an explicit type specifier. If this is false, the user is perhaps intending for
        /// the type to be inferred from the program context.
        /// </summary>
        public bool IsTypeSpecified => TypeSpecifier != null;

        /// <summary>
        /// The type reference has been successfully resolved to a (loaded) CLR type.
        /// </summary>
        public bool IsResolved => ClrType != null;

        public TypeReference(Token typeSpecifier)
        {
            TypeSpecifier = typeSpecifier;
        }

        public override string ToString()
        {
            if (IsTypeSpecified)
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
