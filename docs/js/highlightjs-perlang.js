hljs.registerLanguage('perlang', function(hljs) {
    "use strict";

    var KEYWORDS = {
        keyword:
            // Currently implemented
            'case constructor default destructor else enum extern for fun if in mutable print return super this var while ' +

            // Reserved keywords
            'class byte sbyte short ushort float decimal ' +
            'public private protected internal static volatile printf switch ' +
            'break continue try catch finally async await lock synchronized new ' +
            'let const struct sizeof nameof typeof asm',
        literal:
            'true false null',
        built_in:
            'bigint bool char double float int long object string uint ulong void'
    }

    return {
        aliases: ['per'],
        case_insensitive: false,
        keywords: KEYWORDS,

        contains: [
            hljs.QUOTE_STRING_MODE,
            hljs.C_LINE_COMMENT_MODE,
            hljs.C_BLOCK_COMMENT_MODE,

            // Identifiers must be matched before numbers, so that e.g. `UTF8String` and `UTF16String` are consumed
            // as a whole token and the `8`/`16` parts are not incorrectly highlighted as numeric literals.
            // className 'type' is used as the fallback for non-keyword identifiers (user-defined class names etc.).
            {
                className: 'type',
                begin: /[A-Za-z_][A-Za-z0-9_]*/,
                keywords: KEYWORDS,
                relevance: 0
            },

            {
                className: 'number',
                variants: [
                    { begin: /0b[01][01_]*/ },                                       // binary: 0b11110000
                    { begin: /0o[0-7][0-7_]*/ },                                     // octal: 0o755
                    { begin: /0[xX][0-9a-fA-F][0-9a-fA-F_]*/ },                      // hex: 0xA0000
                    { begin: /[0-9][0-9_]*\.[0-9][0-9_]*([eE][+-]?[0-9]+)?[fd]?/ },  // float/double
                    { begin: /[0-9][0-9_]*/ },                                       // decimal integer
                ],
                relevance: 0
            },
        ]
    }
});
