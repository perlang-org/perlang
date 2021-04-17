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
        public string Name { get; }

        /// <summary>
        /// `nil` is used. This warning covers multiple usages of nil, like variable assignment (`s = nil`), variable
        /// initializers (`var s: string = nil`) and function calls (`some_function(nil)`).
        /// </summary>
        public static readonly WarningType NIL_USAGE = new("nil-usage");

        private static readonly IReadOnlyDictionary<string, WarningType> AllWarnings = new[]
        {
            NIL_USAGE
        }.ToImmutableDictionary(w => w.Name, w => w);

        private WarningType(string name)
        {
            Name = name;
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
