using System;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace Perlang.ConsoleApp
{
    // TODO: Remove this class if/when https://github.com/dotnet/command-line-api/pull/1272 gets merged.
    public static class CommandResultExtensions
    {
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
