namespace Perlang;

/// <summary>
/// Dummy type used to fulfill the requirements of the <see cref="Expr.IVisitor{TR}"/> and <see cref="Stmt.IVisitor{TR}"/>
/// type parameters
///
/// For some use cases of the IVisitor interfaces mentioned above, we need to be able to provide a type parameter
/// (typically "object"). Instead of hard-wiring these interfaces to return "object", we have made them be
/// a couple of type-parameterized interfaces. However, for other use cases the return type does not make sense.
///
/// Since "void" is not a type in .NET, we must use something else than void to describe "no return value". The
/// VoidObject is an answer to this question. Given that it is abstract, it can never be instantiated. A visitor
/// using this type as type parameter is expected to return "VoidObject.Void" from all its methods.
/// </summary>
/// <remarks>This link goes into detail in explaining how "void" works in .NET:
/// https://softwareengineering.stackexchange.com/a/131099/87701.</remarks>
public abstract class VoidObject
{
    /// <summary>
    /// Gets a value indicating that no meaningful return value was provided.
    /// </summary>
    public static VoidObject Void => null;
}