## The Perlang Language

> This is a more in-depth page about the Perlang language. It tries to cover all features currently implemented. If you are impatient and just want to see some examples of what Perlang can look like in action, [the quickstart page](../quickstart) might be a better start for you.

### The top-level scope

Much like other scripting-based languages like JavaScript, Ruby and Python, a program in Perlang does not necessarily have to consist of a _class_ or _function_ (which is the case in languages like Java and C). This is because of the existence of a top-level scope. You can write statements in this scope, and they will be executed when the program is executed:

[!code-perlang[printing-from-the-top-level-scope](../../../examples/the-language/printing-from-the-top-level-scope.per)]

You can also declare variables in this scope and refer to them later in your program. It makes sense to think of the top-level scope as an "implicit function" or even an "implicit class", if you come from a background in other languages where this way of thinking makes sense to you.

```
// defining-a-variable.per
var a = 1;
print a;
```

### Variables

We already cheated at bit and defined a variable in the previous section, but let's look a bit more in-depth at this now. Variables can be defined in two ways: with _explicit_ or _implicit_ ("inferred") typing specified.

```
// two-types-of-variables.per
var a = 1;
var b: int = 2;

print a;
print b;
```

The above variable declarations are the same in essence. However, be not deceived; Perlang is not a dynamic language even though it supports constructs like `var a = 1`. These examples illustrates this point further:

```
// invalid-reassignment-to-typed-variable.per
var a: int = 1;
a = "foo";
```

If you try to run the above in a REPL session, you'll get an error like this:

`Error at 'a': Cannot assign System.String to variable defined as 'System.Int32'`

This is because once a variable is declared, the type of this variable (explicitly or implicitly defined) is stored. The Perlang typechecker uses this information to ensure the type-wise correctness of your program, much like any other statically typed language.

### Functions

* TODO: Declaring a function
* TODO: Calling a function

### The existence of `nil`

TODO: rename nil to null

### Classes

### The future
