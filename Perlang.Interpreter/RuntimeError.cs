using System;

namespace Perlang.Interpreter
{
    public class RuntimeError : Exception
    {
        internal Token Token { get; }

        internal RuntimeError(Token token, string message)
            : base(message)
        {
            Token = token;
        }
    }
}