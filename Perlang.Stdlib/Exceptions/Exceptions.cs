using System;

namespace Perlang.Exceptions
{
    /// <summary>
    /// Abstract base class for exceptions thrown by stdlib.
    /// </summary>
    public abstract class StdlibException : Exception
    {
        public StdlibException(string message) :
            base(message)
        {
        }
    }
}
