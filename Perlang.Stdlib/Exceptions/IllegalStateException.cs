namespace Perlang.Exceptions
{
    /// <summary>
    /// Thrown when a method call is invalid for the current state of an object.
    /// </summary>
    public class IllegalStateException : StdlibException
    {
        public IllegalStateException(string message) :
            base(message)
        {
        }
    }
}