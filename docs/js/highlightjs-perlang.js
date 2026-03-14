hljs.registerLanguage('perlang', function(hljs) {
    "use strict";

    var KEYWORDS = {
        keyword:
            // Currently implemented
            'case constructor default destructor else enum extern for fun if in mutable print return super this var while ' +

            // Reserved keywords
            'class byte sbyte short ushort uint ulong float decimal ' +
            'char public private protected internal static volatile printf switch ' +
            'break continue try catch finally async await lock synchronized new ' +
            'let const struct sizeof nameof typeof asm',
        literal:
            'true false null',
        built_in:
            'bigint bool double int long string void'
    }

    return {
        aliases: ['per'],
        case_insensitive: false,
        keywords: KEYWORDS,

        contains: [
            hljs.QUOTE_STRING_MODE,
            hljs.C_LINE_COMMENT_MODE,
            hljs.C_BLOCK_COMMENT_MODE,

            {
                className: 'number',
                variants: [
                    // Technically, we don't support all variations of C numbers
                    // yet, but we'll likely add support for hex numbers etc
                    // eventually.
                    { begin: hljs.C_NUMBER_RE }
                ],
                relevance: 0
            },
        ]
    }
});
