using System;

namespace Perlang.Interpreter
{
    public class RuntimeError : Exception
    {
        public Token Token { get; }

        public RuntimeError(Token token, string message)
            : base(message)
        {
            Token = token;
        }
    }
}
