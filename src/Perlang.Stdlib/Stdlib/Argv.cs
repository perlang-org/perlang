using System.Collections.Concurrent;
using System.Collections.Immutable;
using JetBrains.Annotations;

namespace Perlang.Stdlib;

/// <summary>
/// "Argument vector". This class serves as a container for arguments passed to the program at run time.
///
/// To read the next available argument, use the <see cref="Pop"/> method. Example: `ARGV.pop()`.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public class Argv
{
    private readonly ConcurrentQueue<string> remainingArguments;

    public Argv(ImmutableList<string> arguments)
    {
        remainingArguments = new ConcurrentQueue<string>(arguments);
    }

    /// <summary>
    /// Pops the next argument from the list of program arguments. The arguments are popped in FIFO order; the first
    /// argument is popped first, the second next, and so forth.
    ///
    /// Example usage:
    ///
    /// ```
    /// var arg1 = ARGV.pop;
    /// print arg1;
    /// ```
    ///
    /// If you are familiar with the concept of <a
    /// href="https://en.wikipedia.org/wiki/Queue_(abstract_data_type)">queues</a>, this method matches the
    /// "dequeue" operation.
    /// </summary>
    /// <returns>The next argument.</returns>
    /// <exception cref="IllegalStateException">There are no arguments left to be popped.</exception>
    public string Pop()
    {
        if (remainingArguments.TryDequeue(out string result))
        {
            return result;
        }
        else
        {
            throw new Perlang.Exceptions.IllegalStateException("No arguments left");
        }
    }
}
