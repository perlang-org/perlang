using System;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace Perlang.ConsoleApp
{
    public static class CommandResultExtensions
    {
        public static bool HasOption(
            this CommandResult commandResult,
            Option option)
        {
            if (commandResult is null)
            {
                throw new ArgumentNullException(nameof(commandResult));
            }

            return commandResult.FindResultFor(option) is { };
        }

        public static bool HasArgument(
            this CommandResult commandResult,
            Argument argument)
        {
            if (commandResult is null)
            {
                throw new ArgumentNullException(nameof(commandResult));
            }

            return commandResult.FindResultFor(argument) is { };
        }
    }
}
