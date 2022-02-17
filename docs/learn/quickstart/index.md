# Quick Start

## Prerequisites

This section presumes that you have installed the Perlang tooling using the instructions on our [download page](../../download/index.md), and added it to your `$PATH`.

There are basically two ways in which you can use Perlang:

- _REPL mode_
- _Scripting mode_

In _REPL mode_, each line you type is interpreted as you press Enter. This mode is useful for doing mathematical operations (for example: `2 * 3`, `2 ** 10`, etc) as well as for doing "exploratory programming", learning how a particular class or API works. For this purpose, it works well. If you are working on a program where the program will be executed more than one time, it is typically not the right choice.

In _scripting mode_, you type your program in a file and execute `perlang <filename>` (without the angle brackets). Perlang scripts conventionally follows the  `filename.per` convention, although this is no way enforced by the interpreter.

> In the future, we hope to add a _compiled mode_ as well, where Perlang programs are precompiled to MSIL bytecode before they are executed. It is anticipated that this will provide magnitudes better performance than the current, interpreted mode.

Some of the examples below illustrate both ways of running a Perlang program, but the longer examples are only really practical to run in scripting mode. As we move along through this guide, we introduce various concepts in how the Perlang interpreter and language works as well, so this section aims to provide the reader with more than just the bare essentials.

## Hello World

#### REPL version

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

#### Scripting mode

[!code-perlang[hello_world](../../examples/quickstart/hello_world.per)]

The comment on the first line is strictly speaking not a required part in this script. It is only there to help you understand that this program should be saved to disk using the suggested file name given there. (Feel free to disobey this suggestion if you are a little rebellious, just like me.)

Run the program like this: `perlang hello_world.per`. If all goes well, you should get an output that looks like this:

```shell
$ perlang hello_world.per
Hello World
```

## Calculating an arbitrary number of pi decimals

Many programming language tutorials are kind of boring (_feel like calculating Fibonacci sequences, anyone?_). This is my feeble attempt to make it just a _little_ bit more exciting. Based on an algorithm by Andrew Jennings[1], this program will give you the first 1000 digits of π.

(Given the size of this, there is little reason to show the REPL version; some of this would also have to be typed on a single line in the REPL at the moment, making it even more inconvenient.)

[!code-perlang[pi](../../examples/quickstart/pi.per)]

[1]: http://ajennings.net/blog/a-million-digits-of-pi-in-9-lines-of-javascript.html

### How about performance?

Perlang is in no way performance-optimized (yet). Because of its scripted, interpreted nature, it will be much inferior to more grown-up languages like Java and C# (which compile to "virtual machine executables" - Java bytecode and MSIL respectively). It will likely even be less efficient than JavaScript. Let's try it and see for ourselves!

If we increase the number of Pi digits from 1000 to 10000, this is the runtime on my current machine (i5-8250U CPU @ 1.60GHz):

```shell
$ time perlang docs/examples/pi.per
314159265358979323846264338327950288419716939937510582097494459230781640628620899862803482534211706798214808651328230664709384460955058223172535940812848111745028410270193852110555964462294895493038196442881097566593344612847564823378678316527120190914564856692346034861045432664821339360726024914127372458700660631558817488152092096282925409171536436789259036001133053054882046652138414695194151160943305727036575959195309218611738193261179310511854807446237996274956735188575272489122793818301194912983367336244065664308602139494639522473719070217986094370277053921717629317675238467481846766940513200056812714526356082778577134275778960917363717872146844090122495343014654958537105079227968925892354201995611212902196086403441815981362977477130996051870721134999999837297804995105973173281609631859502445945534690830264252230825334468503526193118817101000313783875288658753320838142061717766914730359825349042875546873115956286388235378759375195778185778053217122680661300192787661119590921642019893809525720106548586327886593615338182796823030195203530185296899577362259941389[...]

real	0m0.614s
user	0m0.602s
sys	0m0.032s
```

Now, for the sake of it, here is the JavaScript equivalent (Andrew Jennings example with number of digits increased from 1000 to 10000):

```
$ node -v
v14.15.5
$ time node pi.js
314159265358979323846264338327950288419716939937510582097494459230781640628620899862803482534211706798214808651328230664709384460955058223172535940812848111745028410270193852110555964462294895493038196442881097566593344612847564823378678316527120190914564856692346034861045432664821339360726024914127372458700660631558817488152092096282925409171536436789259036001133053054882046652138414695194151160943305727036575959195309218611738193261179310511854807446237996274956735188575272489122793818301194912983367336244065664308602139494639522473719070217986094370277053921717629317675238467481846766940513200056812714526356082778577134275778960917363717872146844090122495343014654958537105079227968925892354201995611212902196086403441815981362977477130996051870721134999999837297804995105973173281609631859502445945534690830264252230825334468503526193118817101000313783875288658753320838142061717766914730359825349042875546873115956286388235378759375195778185778053217122680661300192787661119590921642019893809525720106548586327886593615338182796823030195203530185296899577362259941389[...]

real	0m0.380s
user	0m0.383s
sys	0m0.005s
```

So, we are running about 60% slower than the JavaScript counterpart. For many applications, this could be tolerable but it's quite obvious that there's a lot of work that needs to be done in this area before Perlang is anywhere near "production quality".

### Further reading

This guide only takes some initial steps in teaching the user how to use Perlang. For example, _functions_ aren't covered at all.

For a more in-depth guide which aims to cover all parts of Perlang which are currently implemented, see the page about [the Perlang language](../the-language/index.md).
