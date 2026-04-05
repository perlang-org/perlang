## Draft: The Perlang Language

> This is a more in-depth page describing the Perlang language. It tries to cover all features currently implemented. If you are impatient and just want to see some examples of what Perlang can look like in action, [the quickstart page](../quickstart/index.md) might be a better start for you.

_The text is currently a draft of the text which we provide as a "public preview". Once the document describes all aspects of Perlang in a satisfiable way, we will remove the "draft" status._

<!--
For some of the functionality where the documentation is missing, we have added links to the corresponding tests; these provide real-world examples on how the functionality is being used. Merge requests that add proper examples to this page are highly welcome and appreciated.
-->

## Language features

### The top-level scope

Similar to script-based languages like JavaScript, Ruby and Python, a program in Perlang does not necessarily have to consist of a _class_ or _function_ (which is the case in languages like Java and C). This is because of the existence of a _top-level scope_. You can write statements in this scope, and they will be executed when the program is executed:

[!code-perlang[printing-from-the-top-level-scope](../../examples/the-language/printing-from-the-top-level-scope.per)]

You can also declare variables in this scope and refer to them later in your program. It makes sense to think of the top-level scope as an "implicit `main` method" or even an "implicit class", if you come from a background in other languages where this way of thinking makes sense to you.

[!code-perlang[defining-a-variable](../../examples/the-language/defining-a-variable.per)]

### Variables

We already cheated a bit and defined a variable in the previous section, but let's look a bit more in-depth at this now. Variables can be defined in two ways: with _explicit_ or _implicit_ ("inferred") typing specified.

[!code-perlang[two-types-of-variables](../../examples/the-language/two-types-of-variables.per)]

The above variable declarations are the same in essence. However, be not deceived; Perlang is not a dynamic language even though it supports constructs like `var a = 1`. These examples illustrates this point further:

<!-- Inline example instead of file in examples/, because those are validated to return non-zero
     (=not generate any errors) in CI -->
```perlang
// invalid-reassignment-to-typed-variable.per
var a: int = 1;
a = "foo";
```

If you save the above and try to run it, you'll get an error like this:

`Error at 'a': Cannot assign 'ASCIIString' to 'int' variable`

This is because once a variable is declared, the type of this variable (explicitly or implicitly defined) is stored. The Perlang typechecker uses this information to ensure the type-wise correctness of your program, much like any other statically typed language.

### Integer types

Perlang currently supports the following integer types. Their usage is demonstrated below.

| Type     | Width    | Signed | Range                                                    |
|----------|----------|--------|----------------------------------------------------------|
| `int`    | 32 bits  | Yes    | -2,147,483,648 to 2,147,483,647                          |
| `uint`   | 32 bits  | No     | 0 to 4,294,967,295                                       |
| `long`   | 64 bits  | Yes    | -9,223,372,036,854,775,808 to 9,223,372,036,854,775,807  |
| `ulong`  | 64 bits  | No     | 0 to 18,446,744,073,709,551,615                          |
| `bigint` | Arbitrary| Yes    | No limit                                                 |

#### Automatic type inference

When using `var` without an explicit type annotation, the compiler automatically selects the smallest integer type in which the literal value fits, but never smaller than 32 bits, i.e. `int` or `uint`. The following program illustrates this:

[!code-perlang[integer-types](../../examples/the-language/integer-types.per)]

As can be seen above, the compiler will determine the appropriate size (from `int` and upwards) to use as the type as values grow larger. Here are some other examples:

```perlang
var a = 1;                    // int   (fits in 32 bits)
var b = -1;                   // int
var c = 2147483648;           // uint  (too large for int, fits in 32-bit unsigned)
var d = -2147483649;          // long  (too large for int)
var e = 4294967296;           // long  (too large for uint, fits in 64-bit signed)
var f = 9223372036854775808;  // ulong (too large for long)
```

#### Compile-time assignment checks

Assigning an `int` to a `long` is perfectly permissible, since such assignments can be performed without data loss. In other words, the following code compiles without errors:

```perlang
var i: int = 12345;
var v: long = i;
```

On the other hand, trying to assign a `long` to an `int` variable will fail, because the value cannot be guaranteed to fit into to the smaller variable size:

```perlang
var v: long = 8589934592;
var i: int = v; // Error: Cannot assign long to int variable
```

In many languages, the above behaviour can be overriden using explicit casts, like `int i = (int)v` in C, C# and Java. In Perlang, such explicit casting is currently not supported.

#### Alternative literal formats

Integer literals can be written in decimal, hexadecimal, octal, or binary notation. When applicable, underscores can be used as digit separators for improved readability:

[!code-perlang[integer-literal-formats](../../examples/the-language/integer-literal-formats.per)]

### Strings

> This section reflects the currently implemented functionality; not the full intended functionality. For more details, see [#370 Implement `string` interface with smart handling of string literals](https://gitlab.perlang.org/perlang/perlang/-/issues/370).

String literals in Perlang are enclosed in double quotes (`"string-value"`). The easiest way to use strings are by using type inference (i.e. "implicit typing"). The example below also shows the actual type used under the hood:

[!code-perlang[string-types](../../examples/the-language/string-types.per)]

The above example can also be rephrased to use the generic `string` type explicitly, like this. As illustrated, the actual types used are still the same:

[!code-perlang[string-type-agnostic](../../examples/the-language/string-type-agnostic.per)]

#### String types

The examples above illustrate usage of three of the most common string types in Perlang: `string`, `ASCIIString` and `UTF8String`. Here is a brief explanation of how they work.

* `string` is the most generic type. It can be used for code saying "I can accept any kind of string". Note that retrieving the string length, or indexing the string based on character position is not possible with this type. This is because for some string types (most notably `UTF8String`), such operations are not easily supported. If indexing the string is required, you must convert it to `ASCIIString` or `UTF16String`, by calling `as_ascii()` or `as_utf16()` respectively.

* `ASCIIString` is the underlying type used for strings which contain only ASCII characters. It's an efficient type for such content, using one byte per character.

* `UTF8String` is used for string literals containing non-ASCII content. It uses the UTF-8 encoding, which is space-efficient for both ASCII and non-ASCII content, but has the significant disadvantage of using _varying length_ for each character (code point). Each individual code point can be between 1 and 4 bytes, per RFC 3629.

* `UTF16String` is never used implicitly for string literals, but can be used for programs that need to index a string based on position. It supports the full Unicode range, but code points outside the BMP (Basic Multilingual Plane) will use two characters using something called a _surrogate pair_. To create a `UTF16String`, call `as_utf16()` on an existing string.

All these types can be used explicitly in variable declarations:

[!code-perlang[string-explicit-types](../../examples/the-language/string-explicit-types.per)]

#### Concatenation

> Perlang currently does not support Ruby/C#-style string interpolation. For more details, see [#295 Support string interpolation](https://gitlab.perlang.org/perlang/perlang/-/issues/295)

Strings can be concatenated using the `+` operator. Perlang also supports concatenating strings directly with numeric types, without requiring an explicit conversion:

[!code-perlang[string-concatenation](../../examples/the-language/string-concatenation.per)]

#### Comparison

Strings are compared by value using the `==` operator:

[!code-perlang[string-comparison](../../examples/the-language/string-comparison.per)]

Note that comparisons are currently "dumb"; they perform a character-by-character comparison and do not take different locales into consideration. For example, the following strings are "semantically equivalent" (`café` vs `cafe` + combining acute accent (U+0301)), but will be compared as non-equal with our current operator:

[!code-perlang[string-comparison-unicode](../../examples/the-language/string-comparison-unicode.per)]

#### String length

The `.length` property is available on `ASCIIString` and `UTF16String`. For non-indexable strings like `UTF8String`, this information is not available.

[!code-perlang[string-length](../../examples/the-language/string-length.per)]

Like for `UTF8String`, the `.length` property is not available on `string`, so attempting to access it on such objects will result in compilation errors.

### Functions

Top-level functions are currently defined using the `fun` keyword. Here's a simple example of how a function can be defined and called:

[!code-perlang[defining-and-calling-a-function](../../examples/the-language/defining-and-calling-a-function.per)]

Many functions take one or more parameters. Here's an example of how such a function can be defined and called:

[!code-perlang[defining-and-calling-a-function-with-parameters](../../examples/the-language/defining-and-calling-a-function-with-parameters.per)]

The last example is interesting in a different way as well. It illustrates a language feature available in Perlang which we share with other languages like Java, C# and JavaScript - being able to concatenate `string` and `int` values without any conversions. Other languages like Ruby and Python are more strict in this regard, requiring an implicit conversion to `String`/`str`.

I imagine the reason for this to be the dynamic nature of these languages. In a dynamic language, it is not certain that a particular variable or parameter has a given type, so forcing the user to call `i.to_s` makes quite a bit of sense. By doing so, you ensure that the operation will do what the user expected. What would happen if you try to concatenate an integer and a random DTO/model instance? Such an operation does not make so much sense, so forcing the user to call `model.to_s` if they _really_ want to do that does make the code more explicit and clear to the reader.

As a compiled, statically typed language, Perlang can make compile-time guarantees that implicit coercions like this will succeed — or produce a compilation error if they cannot. This is consistent with the behavior of our statically typed friends — Java and C#.

> Interestingly enough, JavaScript wants to be different - it is indeed a _dynamic_ programming language, but it still supports concatenation of arbitrary (non-numeric) objects. For example, doing `new Object() + new Object()` gives you the string `[object Object][object Object]`. To have a custom representation of the object being used in this case, you implement a custom `toString()` method for the object in question.

### `switch` statements

Like many languages in the C family, Perlang supports `switch` statements for branching based on a value. The following types can be used as the switch expression: `int`, `char`, `string`, and `enum` types. A `default` branch can be added to handle values not matched by any `case`.

A notable feature is support for _range conditions_ using the `..` operator, which allows matching a contiguous range of values in a single case. The example below shows both range conditions and "regular", single conditions.

[!code-perlang[switch-statement](../../examples/the-language/switch-statement.per)]

Multiple cases can share the same branch by listing them consecutively without a body between them. Note that unlike C and C++, there is no implicit fallthrough between cases — each case is independent. Because of this, there is no need to use the `break` keyword in this context.

Switch statements can also branch based on `char` values:

[!code-perlang[switch-statement-char](../../examples/the-language/switch-statement-char.per)]

### Classes

Perlang supports defining user-defined classes with instance methods, static methods, constructors, and fields. Fields are immutable by default; use the `mutable` keyword to allow reassignment. Inheritance is not yet supported.

Here is a simple example of a user-defined class with a constructor, a private field, and an instance method:

[!code-perlang[user-defined-class](../../examples/the-language/user-defined-class.per)]

#### Static methods

Classes can also define static methods, which can be called directly on the class without instantiating it first:

[!code-perlang[user-defined-class-with-static-method](../../examples/the-language/user-defined-class-with-static-method.per)]

#### Destructors

Like in C++, a class can define a destructor. The destructor will always be called when the object goes out of scope. This is different from languages like Java and C#, where you have less control over when an object will actually be destroyed.

[!code-perlang[user-defined-class-with-destructor](../../examples/the-language/user-defined-class-with-destructor.per)]

<!-- TODO: Re-enable this example once calling Base64.encode() works again. Currently
     fails with "Internal error: C++ type for System.Object not defined".

Static methods can also be called on existing stdlib classes, as the following example demonstrates:

[!code-perlang[calling-base64-encode](../../examples/the-language/calling-base64-encode.per)]
-->

### The existence of `null`

The concept of `null` — a reference that points to no object — is well-known from languages like C, Java, and C#, but can be a common source of runtime errors<sup>2</sup>. Perlang includes `null` primarily for interoperability with other ecosystems, but deliberately restricts its use. Consider the following program:

<!-- Inline example instead of file in examples/, because those are validated to return non-zero
     (=not generate any errors) in CI -->
```perlang
// defining-and-calling-a-function-with-null-parameter.per
fun greet(name: string, age: int): void {
  print "Hello " + name + ". Your age is " + age;
}

// Expected error: [line 2] Error at 'name': Null parameter detected
greet(null, 42);
```

Running this program gives you an error like this:

`[line 7] Error at 'greet': Null parameter detected for 'name'`

The path we have chosen here is to let  `null` exist as a concept in Perlang, mainly for interoperability with C, C++ and other languages that uses `null` references extensively. Making it impossible to use `null` would significantly limit the ability to e.g. use existing C libraries. Hence, we have decided to include `null` in the language.

But: we deliberately restrict the use of `null` in an attempt to steer the user to better constructs, when possible. Whenever `null` is encountered, a compiler warning is emitted. By default, all compiler warnings are considered errors<sup>1</sup>, which is why you get the above error whenever you try to use `null`.

Now, including `null` in the language but making it impossible to use would be kind of pointless. What we have instead is a mechanism to demote this warning from an error to an actual warning:

```
$ perlang -Wno-error=null-usage defining-and-calling-a-function-with-null-parameter.per
[line 7] Warning at 'greet': Null parameter detected for 'name'
[line 3] Operands must be numbers, not string and null
```

As can be seen, the previous `Error at 'name'` has now turned into a slightly more friendly `Warning at 'greet'`. However, we then get a runtime error (the "line 3" output) because `"Hello " + name` is not a valid operation in cases where `name` is `null`. `string + null` will produce a runtime error as above.

> The reason for why these errors seem to come in the "wrong order" in terms of the line numbers is because the compilation and analysis phase of the program happens first, as a separate stage, before the actual execution of the program beings. In other words, all compilation warnings for a program would appear before any runtime errors would be emitted.

### The standard library

The standard library is in a very early stage of development. It is currently being rewritten from C# to C++, and more functionality is being added to it.

### The future

> _The future is not set_ (John & Sarah Connor, _Terminator 2: Judgment Day_)

There is currently no road map as for exactly "when" and "if" various features will be implemented into the language and its standard library. Your best bet for now is looking at [the milestones](https://gitlab.perlang.org/perlang/perlang/-/milestones) in the GitLab repo, where various ideas are roughly categorized into projected releases, depending on when we imagine that they may get introduced into the language.

## Footnotes

<sup>1</sup>: Making warnings be considered errors by default is a deliberate, conscious design decision in an attempt to ensure that a codebase is not littered with numerous minor errors - errors which are really _there_ but the developers have learned to look the other way, to ignore them. It is our experience that this can too-easily become the case when warnings are ignored by default.

<sup>2</sup>: Tony Hoare, who invented the `null` reference in the [ALGOL W](https://en.wikipedia.org/wiki/ALGOL_W) programming language, famously called it his "billion-dollar mistake": _"It was the invention of the null reference in 1965. [...] I couldn't resist the temptation to put in a null reference, simply because it was so easy to implement. This has led to innumerable errors, vulnerabilities, and system crashes, which have probably caused a billion dollars of pain and damage in the last forty years."_ ([QCon London, 2009](https://web.archive.org/web/20090628071208/http://qconlondon.com/london-2009/speaker/Tony+Hoare))
