using System.Collections.Concurrent;
using System.Collections.Immutable;
using Perlang.Attributes;
using Perlang.Exceptions;

namespace Perlang.Stdlib
{
    [GlobalClass]
    public static class Argv
    {
        private static ConcurrentQueue<string> remainingArguments;

        [ArgumentsSetter]
        public static void SetArguments(ImmutableList<string> arguments)
        {
            remainingArguments = new ConcurrentQueue<string>(arguments);
        }

        public static string Pop()
        {
            if (remainingArguments == null)
            {
                throw new IllegalStateException("Internal runtime error: SetArguments has never been invoked");
            }

            if (remainingArguments.TryDequeue(out string result))
            {
                return result;
            }
            else
            {
                throw new IllegalStateException("No arguments left");
            }
        }
    }
}
