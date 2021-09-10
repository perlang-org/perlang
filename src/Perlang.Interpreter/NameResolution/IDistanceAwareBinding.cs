namespace Perlang.Interpreter.NameResolution
{
    internal interface IDistanceAwareBinding
    {
        /// <summary>
        /// Gets the distance (number of scopes) from the referring expression to the scope in which the name is declared.
        /// 0 = same scope, 1 = one scope above, etc.
        ///
        /// The magic value -1 is used to indicate that the binding refers to a global function.
        /// </summary>
        int Distance { get; }
    }
}
