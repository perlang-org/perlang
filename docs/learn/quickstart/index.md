# Quick Start

## Prerequisites

This section presumes that you have installed the Perlang tooling using the instructions on our [download page](../../download/index.md), and added it to your `$PATH`.

There are basically two ways in which you can use Perlang:

- _REPL mode_ _**(not currently available)**_
- _Scripting mode_ _**(current)**_

In _REPL mode_, each line you type is interpreted as you press Enter. This mode is useful for doing mathematical operations (for example: `2 * 3`, `2 ** 10`, etc) as well as for doing "exploratory programming", learning how a particular class or API works. The REPL is not currently available, but we hope to bring it back in the future, possibly implemented using on-the-fly compilation via LLVM. This is briefly mentioned in [#406](https://gitlab.perlang.org/perlang/perlang/-/issues/406), but we currently don't have an open issue about bringing back the REPL.

In _scripting mode_, you type your program in a file and execute `perlang <filename>` (without the angle brackets). Perlang scripts conventionally follow the `filename.per` convention, although this is in no way enforced. Previously, programs executed like this were executed in an interpreted manner. Nowadays, Perlang compiles your program to native code behind the scenes before running it — but the usage experience remains the same.

Some of the examples below include both REPL and scripting mode versions. The REPL examples are kept for reference and to illustrate how the language works, even though the REPL is not currently available. As we move along through this guide, we introduce various concepts in how the Perlang compiler and language works as well, so this section aims to provide the reader with more than just the bare essentials.

## Hello World

#### REPL mode _(not currently available)_

```shell
$ perlang
Perlang Interactive REPL Console (0.1.0-dev.117, built from 3263a42)
> print "Hello World"
Hello World
```

As can be seen, printing Hello World is pretty much like in many other programming languages. The `print` function is a language keyword, so there is no requirement to use parentheses around the string being printed. (The interpreter will not complain if you do choose to use parentheses, though.)

You can also do it like this:

```shell
> "Hello World"
Hello World
```

In the Perlang REPL, much like in the Python or Ruby REPL, the result of each (valid) expression is printed. You can try this with other expressions also, like `10 * 50`, `2 ** 32` and so forth.

Here's an example of a REPL session in action:

<asciinema-player cols="126" rows="30" speed="2" src="/casts/repl.cast"></asciinema-player>

Worth mentioning is that normally in Perlang, each complete _statement_ (like `print "Hello World"`) must be terminated by a semicolon, much like in the C family of languages (C, C++, C#, Java etc). In the REPL however, it's permissible to skip the semicolon. A newline can be considered to be interpreted as a "semicolon and newline", for convenience.

#### Compiled mode _(current)_

[!code-perlang[hello_world](../../examples/quickstart/hello_world.per)]

The comment on the first line is strictly speaking not a required part in this script. It is only there to help you understand that this program should be saved to disk using the suggested file name given there. (Feel free to disobey this suggestion if you are a little rebellious, just like me.)

Run the program like this: `perlang hello_world.per`. If all goes well, you should get an output that looks like this:

```shell
$ perlang hello_world.per
Hello World
```

## Calculating an arbitrary number of pi decimals

Many programming language tutorials are kind of boring (_feel like calculating Fibonacci sequences, anyone?_). This is my feeble attempt to make it just a _little_ bit more exciting. Based on an algorithm by Andrew Jennings[1], this program will give you the first 1000 digits of π.

(Given the size of this, there is little reason to show the REPL version even when it is available; some of this would also have to be typed on a single line in the REPL, making it even more inconvenient.)

[!code-perlang[pi](../../examples/quickstart/pi.per)]

[1]: http://ajennings.net/blog/a-million-digits-of-pi-in-9-lines-of-javascript.html

### Further reading

This guide only takes some initial steps in teaching the user how to use Perlang. For example, _functions_ and _classes_ aren't covered at all.

For a more in-depth guide which aims to cover all parts of Perlang which are currently implemented, see the page about [the Perlang language](../the-language/index.md).
