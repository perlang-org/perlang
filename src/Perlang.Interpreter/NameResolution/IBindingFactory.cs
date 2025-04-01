namespace Perlang.Interpreter.NameResolution;

internal interface IBindingFactory
{
    /// <summary>
    /// Creates a binding using the provided parameters.
    /// </summary>
    /// <param name="referringExpr">The referring expression.</param>
    /// <returns>A newly created `Binding` instance.</returns>
    Binding CreateBinding(Expr referringExpr);

    /// <summary>
    /// Gets the type of object this binding refers to. Can be used to e.g. construct helpful error messages to end
    /// users.
    /// </summary>
    string ObjectType { get; }

    /// <summary>
    /// Gets the type of object this binding refers to, with the initial letter converted to upper-case.
    /// </summary>
    object ObjectTypeTitleized => ObjectType[0].ToString().ToUpper() + ObjectType.Substring(1);
}
