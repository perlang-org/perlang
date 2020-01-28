using Xunit.Sdk;

namespace Perlang.Tests
{
    internal class ScanErrorXunitException : XunitException
    {
        public ScanErrorXunitException(string message)
            : base(message)
        {
        }
    }

    internal class ParseErrorXunitException : XunitException
    {
        public ParseErrorXunitException(string message)
            : base(message)
        {
        }
    }

    internal class ResolveErrorXunitException : XunitException
    {
        public ResolveErrorXunitException(string message)
            : base(message)
        {
        }
    }
}