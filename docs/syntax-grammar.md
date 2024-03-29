(This whole document is shamelessly inspired by https://github.com/munificent/craftinginterpreters/blob/master/book/appendix-i.md)

## Syntax Grammar

The syntactic grammar is used to parse the linear sequence of tokens into the
nested syntax tree structure. It starts with the first rule that matches an
entire Perlang program (or a single REPL entry):

```perlang
program        → declaration* EOF ;
```

### Declarations

A program is a series of declarations, which are the statements that bind new
identifiers or any of the other statement types:

```perlang
declaration    → classDecl
               | funDecl
               | varDecl
               | statement ;

classDecl      → "class" IDENTIFIER ( "<" IDENTIFIER )?
                 "{" function* "}" ;
funDecl        → "fun" function ;
varDecl        → "var" IDENTIFIER ( "=" expression )? ";" ;
```

### Statements

The remaining statement rules produce side effects, but do not introduce
bindings:

```perlang
statement      → exprStmt
               | forStmt
               | ifStmt
               | printStmt
               | returnStmt
               | whileStmt
               | block ;

exprStmt       → expression ";" ;
forStmt        → "for" "(" ( varDecl | exprStmt | ";" )
                           expression? ";"
                           expression? ")" statement ;
ifStmt         → "if" "(" expression ")" statement ( "else" statement )? ;
printStmt      → "print" expression ";" ;
returnStmt     → "return" expression? ";" ;
whileStmt      → "while" "(" expression ")" statement ;
block          → "{" declaration* "}" ;
```

Note that `block` is a statement rule, but is also used as a nonterminal in a
couple of other rules for things like function bodies.

### Expressions

Expressions produce values. Perlang has a number of unary and binary operators with
different levels of precedence. Some grammars for languages do not directly
encode the precedence relationships and specify that elsewhere. Here, we use a
separate rule for each precedence level to make it explicit:

```perlang
expression     → assignment ;

assignment     → ( call "." )? IDENTIFIER "=" assignment
               | unary_postfix | logic_or;
unary_postfix  → IDENTIFIER ( "--" | "++" ) ;

logic_or       → logic_and ( "or" logic_and )* ;
logic_and      → equality ( "and" equality )* ;
equality       → comparison ( ( "!=" | "==" ) comparison )* ;
comparison     → addition ( ( ">" | ">=" | "<" | "<=" ) addition )* ;
addition       → multiplication ( ( "-" | "+" ) multiplication )* ;
multiplication → unary_prefix ( ( "/" | "*" ) unary_prefix )* ;

unary_prefix   → ( "!" | "-" ) unary_prefix | indexing | call ;
call           → primary ( "(" arguments? ")" | "." IDENTIFIER )* ;
indexing       → primary ( "[" expression "]" ) ;
primary        → "true" | "false" | "nil" | "this"
               | NUMBER | STRING | IDENTIFIER | "(" expression ")"
               | "super" "." IDENTIFIER ;
```

### Utility Rules

In order to keep the above rules a little cleaner, some of the grammar is
split out into a few reused helper rules:

```perlang
function       → IDENTIFIER "(" parameters? ")" block ;
parameters     → IDENTIFIER ( "," IDENTIFIER )* ;
arguments      → expression ( "," expression )* ;
```

## Lexical Grammar

The lexical grammar is used by the scanner to group characters into tokens.
Where the syntax is [context free][], the lexical grammar is [regular][] -- note
that there are no recursive rules.

[context free]: https://en.wikipedia.org/wiki/Context-free_grammar
[regular]: https://en.wikipedia.org/wiki/Regular_grammar

```lox
NUMBER         → DIGIT+ ( "." DIGIT+ )? ;
STRING         → '"' <any char except '"'>* '"' ;
IDENTIFIER     → ALPHA ( ALPHA | DIGIT )* ;
ALPHA          → 'a' ... 'z' | 'A' ... 'Z' | '_' ;
DIGIT          → '0' ... '9' ;
```
