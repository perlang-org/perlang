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
        /// `null` is used. This warning covers multiple usages of `null`, like variable assignment (`s = null`),
        /// variable initializers (`var s: string = null`) and function calls (`some_function(null)`).
        /// </summary>
        public static readonly WarningType NULL_USAGE = new("null-usage");

        /// <summary>
        /// An ambiguous combination of boolean operators was encountered. This can be things like `false &amp;&amp;
        /// false || true`. An expression like this is valid in most languages, and in many of them the `&amp;&amp;`
        /// operator has a higher precedence than `||`. However, remembering the operator precedence rules by heart is
        /// not always easy. We want to help people write code which is unambiguous to the reader, which is why we are
        /// strongly encouraging the use of grouping parentheses in expressions like the above.
        /// </summary>
        public static readonly WarningType AMBIGUOUS_COMBINATION_OF_BOOLEAN_OPERATORS = new("ambigous-combination-of-boolean-operators");

        private static readonly IReadOnlyDictionary<string, WarningType> AllWarnings = new[]
        {
            NULL_USAGE
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
