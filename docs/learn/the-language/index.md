## Draft: The Perlang Language

> This is a more in-depth page describing the Perlang language. It tries to cover all features currently implemented. If you are impatient and just want to see some examples of what Perlang can look like in action, [the quickstart page](../quickstart/index.md) might be a better start for you.

_The text is currently a draft of the text which we provide as a "public preview". Once the document describes all aspects of Perlang in a satisfiable way, we will remove the "draft" status._

<!--
For some of the functionality where the documentation is missing, we have added links to the corresponding tests; these provide real-world examples on how the functionality is being used. Merge requests that add proper examples to this page are highly welcome and appreciated.
-->

## Language features

### The top-level scope

Much like other scripting-based languages like JavaScript, Ruby and Python, a program in Perlang does not necessarily have to consist of a _class_ or _function_ (which is the case in languages like Java and C). This is because of the existence of a top-level scope. You can write statements in this scope, and they will be executed when the program is executed:

[!code-perlang[printing-from-the-top-level-scope](../../examples/the-language/printing-from-the-top-level-scope.per)]

You can also declare variables in this scope and refer to them later in your program. It makes sense to think of the top-level scope as an "implicit function" or even an "implicit class", if you come from a background in other languages where this way of thinking makes sense to you.

[!code-perlang[defining-a-variable](../../examples/the-language/defining-a-variable.per)]

### Variables

We already cheated at bit and defined a variable in the previous section, but let's look a bit more in-depth at this now. Variables can be defined in two ways: with _explicit_ or _implicit_ ("inferred") typing specified.

[!code-perlang[two-types-of-variables](../../examples/the-language/two-types-of-variables.per)]

The above variable declarations are the same in essence. However, be not deceived; Perlang is not a dynamic language even though it supports constructs like `var a = 1`. These examples illustrates this point further:

<!-- Inline example instead of file in examples/, because those are validated to return non-zero
     (=not generate any errors) in CI -->
```perlang
// invalid-reassignment-to-typed-variable.per
var a: int = 1;
a = "foo";
```

If you try to run the above in a REPL session, you'll get an error like this:

`Error at 'a': Cannot assign System.String to variable defined as 'System.Int32'`

This is because once a variable is declared, the type of this variable (explicitly or implicitly defined) is stored. The Perlang typechecker uses this information to ensure the type-wise correctness of your program, much like any other statically typed language.

### Functions

Top-level functions are currently defined used the `fun` keyword. Here's a simple example of how a function can be defined and called:

[!code-perlang[defining-and-calling-a-function](../../examples/the-language/defining-and-calling-a-function.per)]

Many functions take one or more parameters. Here's an example of how such a function can be defined and called:

[!code-perlang[defining-and-calling-a-function-with-parameters](../../examples/the-language/defining-and-calling-a-function-with-parameters.per)]

The last example is interesting in a different way as well. It illustrates a language feature available in Perlang which we share with other languages like Java, C# and JavasScript - being able to concatenate `string` and `int` values without any conversions. Other languages like Ruby and Python are more strict in this regard, requiring an implicit conversion to `String`/`str`.

I imagine the reason for this to be the dynamic nature of these languages. In a dynamic language, it is not certain that a particular variable or parameter has a given type, so forcing the user to call `i.to_s` makes quite a bit of sense. By doing so, you ensure that the operation will do what the user expected. What would happen if you try to concatenate an integer and a random DTO/model instance? Such an operation does not make so much sense, so forcing the user to call `model.to_s` if they _really_ want to do that does make the code more explicit and clear to the reader.

However, in statically typed languages we can make a compile-time _guarantee_ that the implicit coercion to `string` will succeed (or produce a compilation error if this is not the case). While Perlang is not currently a compiled language, it does already have a primitive typechecker so it makes sense to mimic the behavior of our statically typed friends - Java and C#.

> Interestingly enough, JavaScript wants to be different - it is indeed a _dynamic_ programming language, but it still supports concatenation of arbitrary (non-numeric) objects. For example, doing `new Object() + new Object()` gives you the string `[object Object][object Object]`. To have a custom representation of the object being used in this case, you implement a custom `toString()` method for the object in question.

### The existence of `null`

If you have worked with other programming languages, you have likely encountered the concept of `null` (or `NULL` in C). Some languages call it something else (`nil` in Ruby, `None` in Python) but the concept is the same: it tries to describe the concept of an _object reference which does not point to an existing object_.

While this is incredibly useful sometimes, it can also cause significant headache since the existence of `null` in a programming language means that **all** code in your program now suddenly has to take into consideration that an object reference can have two forms - an actual object or this dreaded `null` thing. If you try to call a method on a `null` object, even something harmless like `obj.toString()`, you will get a runtime exception. (Ruby tries to do slightly better by making `nil` an actual object that you _can_ call `to_s` on, but it doesn't actually solve the big problem which would make your program easier to write and maintain).

Tony Hoare, who invented the `null` reference in the [ALGOL W](https://en.wikipedia.org/wiki/ALGOL_W) programming language actually went as far as to describe it as a huge mistake:

> _I call it my billion-dollar mistake. It was the invention of the null reference in 1965. At that time, I was designing the first comprehensive type system for references in an object oriented language (ALGOL W). My goal was to ensure that all use of references should be absolutely safe, with checking performed automatically by the compiler. But I couldn't resist the temptation to put in a null reference, simply because it was so easy to implement. This has led to innumerable errors, vulnerabilities, and system crashes, which have probably caused a billion dollars of pain and damage in the last forty years._ <sub>_Hoare, Tony (2009). ["Null References: The Billion Dollar Mistake"](http://qconlondon.com/london-2009/speaker/Tony+Hoare) (Presentation abstract). QCon London. [Archived](https://web.archive.org/web/20090628071208/http://qconlondon.com/london-2009/speaker/Tony+Hoare
 ) from the original on 28 June 2009._</sub>

The Perlang approach to `null` references is that we aim for doing things right from the start. Consider the following program:

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

The path we have chosen here is to let  `null` exist as a concept in Perlang, mainly for interoperability with C#, Java, C, C++ and other languages that uses `null` references extensively. Making it impossible to use `null` would significantly limit the ability to call existing code from e.g. the .NET Base Class Library. Hence, we have decided to include `null` in the language.

But: we deliberately restrict the use of `null` in an attempt to steer the user to better constructs, when possible. Whenever `null` is encountered, a compiler warning is emitted. By default, all compiler warnings are considered errors<sup>1</sup>, which is why you get the above error whenever you try to use `null`.

Now, including `null` in the language but making it impossible to use would be kind of pointless. What we have instead is a mechanism to demote this warning from an error to an actual warning:

```
$ perlang -Wno-error=null-usage defining-and-calling-a-function-with-null-parameter.per
[line 7] Warning at 'greet': Null parameter detected for 'name'
[line 3] Operands must be numbers, not string and null
```

As can be seen, the previous `Error at 'name'` has now turned into a slightly more friendly `Warning at 'greet'`. However, we then get a runtime error (the "line 3" output) because `"Hello " + name` is not a valid operation in cases where `name` is `null`. `string + null` will produce a runtime error as above.

> The reason for why these errors seem to come in the "wrong order" in terms of the line numbers is because the compilation and analysis phase of the program happens first, as a separate stage, before the actual execution of the program beings. In other words, all compilation warnings for a program would appear before any runtime errors would be emitted.

### Classes

In its current version, Perlang does not currently support defining user-defined classes. Calling static methods defined on already existing (stdlib) classes, written in C#, is however possible as the following example demonstrates:

[!code-perlang[calling-base64-encode](../../examples/the-language/calling-base64-encode.per)]

### The standard library

The standard library is in a very early stage of development. Is it currently being rewritten from C# to C++, and more functionality is being added to it.

### The future

> _The future is not set_ (John & Sarah Connor, _Terminator 2: Judgment Day_)

There is currently no road map as for exactly "when" and "if" various features will be implemented into the language and its standard library. Your best bet for now is looking at [the milestones](https://gitlab.perlang.org/perlang/perlang/-/milestones) in the GitLab repo, where various ideas are roughly categorized into projected releases, depending on when we imagine that they may get introduced into the language.

## Footnotes

<sup>1</sup>: Making warnings be considered errors by default is a deliberate, conscious design decision in an attempt to ensure that a codebase is not littered with numerous minor errors - errors which are really _there_ but the developers have learned to look the other way, to ignore them. It is our experience that this can too-easily become the case when warnings are ignored by default.
