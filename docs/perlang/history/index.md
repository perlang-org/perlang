# History

## Background - Why Perlang?

The idea of Perlang - _Per's Language_ or _The Per Language_ - was formed sometime around 2017-2018. At this time, I was working mostly with Ruby and a little bit of CoffeeScript. Historically, I had also spent some years in .NET/C# land, and also before that working with other languages as well.

Some of these languages felt inherently more pleasant to work with than others. I also heard people complaining about languages like Java being overly verbose. As for me, my Java experiences were from around the time of the late 90's/early 00's, with Java 1.2 or "Java 2" as it was touted. In other words, a rather old and outdated version of Java (at this time).

Of the languages mentioned, Ruby was a language that _particularly_ made me frustrated and annoyed (which was perfectly natural since it was the language I worked most with - about 350 hours in 2016 and 400 hours in 2017, according to my [WakaTime](https://wakatime.com) stats). If you don't know Ruby from beforehand, it's a dynamically typed scripting language, often used for writing web application backends (with "Ruby on Rails", which is a highly popular web application framework written in Ruby). Ruby definitely has its strengths, and a few years later, I actually started to appreciate it much more, perhaps because I was now using it for use cases where it was an even better fit than for my previous projects.

## Things I was missing in Ruby

### Optional typing

Ruby is a dynamically typed language, so variables, fields and method parameters are inherently untyped. However, unlike modern languages like [TypeScript](https://www.typescriptlang.org/) that provide a way to add an "optional" type specifier for a variable or parameter, Ruby provides no such mechanism. Why is this a problem?

Well, it doesn't have to be, depending on whether you are dynamically or statically inclined as a person. But regardless of our personal preferences, types _exist_. They are real. It's not like they don't exist in a dynamically typed language, it's just that all the type checking has been deferred to a later stage - to runtime.

For me, when writing code, I tend to think about what type of objects a method will receive. A simple example to get you going:

```ruby
def do_something(s)
  s.swapcase
end
```

Perhaps I know when writing the code above that the _only_ valid type of parameter for this method is really a String. (Yes, I know that this is the wrong way to think about Ruby code because you should not care about the particular type, instead just that it "quacks like a duck" and responds to the `swapcase` message. But there are still cases where you *must* enforce the type, for example because the data is being converted to JSON (and you don't want the result to be arbitrarily typed), sent to a database (where columns are typically using a fixed data type), etc.)

When I write Ruby code, I'm essentially forced to **throw away** this information in my mind when writing the code. There is absolutely no way to write down in my program's source code what type a given parameter is expected to be of. For me, this is frustrating since it means that in the conversion from a mental model (in my head) to source code, I am essentially _forced_ to remove information. Not redundant information, but actually information that would help the computer run my program in a more efficient way.

This leads us naturally over to my next point, namely:

### Static type analysis

When a program contains type information, from one of the following sources:

- For _all variables/parameters_ because of explicit typing (i.e. as in traditional, statically typed languages like Java, C# and C++)
- For all/most variables/parameters because of _strong inferred typing_ (as in languages like F#)
- For _some_ variables/parameters because of optional typing

...it can use this information about the typing information to perform static analysis. For example, it can determine that the following Java program is invalid:

```java
var s = "Hej hej";

// Produces the following error in JShell 11.0.6:
//
// cannot find symbol
//   symbol:   method nonExistingMethod()
s.nonExistingMethod();
```

This is because even though `s` is using an _inferred_ type in this case, the compiler knows that this variable will **always** contain a String (or `null`). That way, it can statically determine that the `s.nonExistingMethod()` method call will always fail. There is simply _no way_ for it to succeed, under any circumstances whatsoever.

Why is this important? It is important because typically, it's better if a program fails _as early as possible_ rather than deferring potential failures to runtime checking. We call this the ["fail fast"](https://en.wikipedia.org/wiki/Fail-fast) philosophy.

As systems grow in size, this becomes more and more important. Simply put, it gives you an extra level of comfort where you can trust the compiler to find errors in your program. Not _all_ errors unfortunately, but at least a good deal of them.

What if we could extend these concepts further to let the compiler make an even more extensive analysis of the program? This is definitely an area we want to delve further into as we explore what Perlang will become.

### Static dispatch of methods

Ruby uses an extreme form of "dynamic dispatch" when it comes to calling methods. The following program is completely valid:

```ruby
def foo
  # Note how this method is calling another, non-existent method
  bar(123)
end

puts("Hello world")
```

If you save this to a file `foo.rb` and run `ruby foo.rb`, the program will run without any errors whatsoever, even though the `foo` method referenced a method does that does not exist. This is because of how method invocations work in Ruby; all method calls are by definition "legal", even when they refer to methods that do not exist. If you call an undefined method, there is even a nice little mechanism called `method_missing` which lets you define, in any class, a method called `method_missing`. All calls to non-existing methods will then be delegated to that method. An example:

```ruby
def foo
  bar(123)
end

def method_missing(method_name, *args)
  puts "You called #{method_name} with #{args}"
end

# (Note: parentheses for method calls are not mandatory in Ruby. The
# line below calls the "foo" method defined above.)
foo
```

Running the program above yields the following output:

```shell
$ ruby foo.rb
You called bar with [123]
```

Nice, huh? This functionality is what powers some of the existing [DSL:s](https://en.wikipedia.org/wiki/Domain-specific_language) written in Ruby, like som parts of the [Sequel](https://github.com/jeremyevans/sequel) library.

I want to make it clear here that I _do_ think it's good to have this kind of functionality in the language sometimes, but the bad part about it is that there's absolutely no way to "turn it off" for cases where you would prefer a `use strict`-mode or similar. I have had cases when there was a typo in a method call or field/variable reference where this was not caught by the unit tests, and I released an updated version of an (internal) Ruby gem - only to realize my folly a bit later.

It is my strong conviction that a really good programming language should help you avoid easy mistakes like this.

## Consequences of the above

### Excessive reliance on unit tests to detect all errors

Because of the lack of static analysis of the code (or at least a very weak analysis of it), people in the Ruby camp rely heavily on unit tests for finding software errors. This is not necessarily a bad thing in itself, since unit tests are great and I definitely think the world is better _with_ unit tests than without them, but the thing I _don't_ like is that with Ruby, you are essentially forced to go for at least 100% code coverage to even know that your code is at all _remotely_ working. There are so many small things that can be broken if you ignore this.

There is a joke in the industry saying that "if it compiles, ship it". Even though this is indeed a joke, there is still a valid point in it. If a computer system _compiles_, it's not at last fundamentally broken in that methods or classes are being referred to using the wrong name, or silly things like that. The compiler gives you a certain safety net, and I personally find that very valuable. With Perlang, we will strive to make this safety net as comfortable and convenient for you as a developer as possible.

### Inferior tooling/IDE support

The tooling and IDE support is also inherently more limited because of the above (which I consider a limitation in the Ruby language). Yes, editors like VS Code do their best to try and cover up for this, but it still feels limited.

RubyMine is a world of its own, and I must admit I feel sorry I never tried it while I was living in Ruby land. After having used JetBrains InteliJ (for Java) for a couple of years at work and Rider (for C#) also for some time, I know what kind of quality the JetBrains tools typically deliver.

But still: you can only get so far when the language doesn't implement the _feature_ that is static typing. Yes, I consider static typing a feature, very much so.

### Inferior performance

Typing information is not just valuable to humans when they read the source code of your program. It is also an invaluable source of knowledge about very intricate details about the inner workings of your program, which can be of great use to make it run as efficiently as possible. It also goes the other way around: when you _know_ the details about how the data in your program is stored in memory, you can use this knowledge to your benefit.

Take the following C# class as an example:

```c#
public class Foo
{
    public string bar;
    public string baz;
    public Zot zot;

    // ...other data fields.
}

public struct Zot
{
    public int x;
    public int y;
}
```

Because structs typically use a sequential layout in .NET, the above roughly translates to something like this in memory:

```
0000000000000000: 64-bit pointer to bar
0000000000000008: 64-bit pointer to baz
0000000000000010: 32-bit value of x
0000000000000014: 32-bit value of y
```

Do you get what I mean? The `x` and `y` fields get inlined here, which means all the data is nicely available here in a co-located manner.

Imagine if this was a Ruby program instead. The memory layout would perhaps be something like this:

```
0000000000000000: 64-bit pointer to bar
0000000000000008: 64-bit pointer to baz
0000000000000010: 64-bit pointer to zot

...[somewhere completely else on the heap]...

0000000000100000: 32-bit value of x
0000000000100004: 32-bit value of y
```

Now, I'll admit, this really doesn't have anything with _static typing_ to do per se; it's more an argument for languages with value types (like C#). Other languages like Java do not support it at the moment (although [JEP 169](https://openjdk.java.net/jeps/169) proposes it in that specific case)

But the point is this: static typing _enables_ more advanced features like this. Dynamically typed languages will likely always have slightly inferior performance than statically typed languages, although companies and highly motivated individuals alike are working as hard as they can to diminish the size of the gap all the time.
