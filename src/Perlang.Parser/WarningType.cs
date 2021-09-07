// Poor-man's replacement for Java-style enums (which are much more like regular classes) in C#. Because we are doing it
// like this, we allow ourselves to override the rules for this particular file.
#pragma warning disable SA1310
#pragma warning disable S3453

using System.Collections.Generic;
using System.Collections.Immutable;

namespace Perlang.Parser
{
    public class WarningType
    {
        /// <summary>
        /// Gets the name of the warning, as used in e.g. `-Werror=&lt;some-warning&gt;`.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the text displayed in `--help warnings` output.
        /// </summary>
        public string HelpText { get; }

        /// <summary>
        /// `null` is used. This warning covers multiple usages of `null`, like variable assignment (`s = null`),
        /// variable initializers (`var s: string = null`) and function calls (`some_function(null)`).
        /// </summary>
        public static readonly WarningType NULL_USAGE = new("null-usage", "Warn whenever null is being used.");

        /// <summary>
        /// Gets a collection of all defined compiler warnings.
        /// </summary>
        public static IReadOnlyDictionary<string, WarningType> AllWarnings { get; } = new[]
        {
            NULL_USAGE
        }.ToImmutableDictionary(w => w.Name, w => w);

        private WarningType(string name, string helpText)
        {
            Name = name;
            HelpText = helpText;
        }

        public static bool KnownWarning(string warningName)
        {
            return AllWarnings.ContainsKey(warningName);
        }

        public static WarningType Get(string warningName)
        {
            return AllWarnings[warningName];
        }
    }
}
