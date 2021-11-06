using System;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace Perlang.ConsoleApp
{
    /// <summary>
    /// Extension methods for <see cref="CommandResult"/>.
    /// </summary>
    public static class CommandResultExtensions
    {
        /// <summary>
        /// Determines if the given option is present in the provided <see cref="CommandResult"/>.
        /// </summary>
        /// <param name="commandResult"></param>
        /// <param name="option"></param>
        /// <returns>`true` if the option is present, `false` otherwise</returns>
        /// <exception cref="ArgumentNullException">`commandResult` is null</exception>
        public static bool HasOption(
            this CommandResult commandResult,
            IOption option)
        {
            if (commandResult is null)
            {
                throw new ArgumentNullException(nameof(commandResult));
            }

            return commandResult.FindResultFor(option) is { };
        }

        public static bool HasArgument(
            this CommandResult commandResult,
            IArgument argument)
        {
            if (commandResult is null)
            {
                throw new ArgumentNullException(nameof(commandResult));
            }

            return commandResult.FindResultFor(argument) is { };
        }
    }
}
