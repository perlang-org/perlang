using System;

namespace Perlang.Parser
{
    public class ScanError : Exception
    {
        public int Line { get; }

        public ScanError(string message, int line)
            : base(message)
        {
            Line = line;
        }
    }
}
