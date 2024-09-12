# The Perlang Programming Language

## About

Perlang is a general-purpose programming language which aims to incorporate the following concepts (or _paradigms_ if you so prefer):

* _static typing_
* strong _type inference_
* _immutability_ by default, mutability where required
* _safe_ by default, unsafe where required
* _object-orientation_
* _functional programming_

Perlang deliberately aims to be a low-barrier language, much like scripting languages like Ruby, JavaScript and Python. As soon as you have the `perlang` binary available, you can start writing your program. Much like these other languages, there is a minimum of "ceremony" required: you don't have to declare a top-level class (or even function!). You can focus on the essentials of "getting things done" and start hacking right away.

At the same time, these concepts (functions, classes<sup>1</sup>, type-parametric polymorphism<sup>2</sup> etc) are there, readily available in the "toolbox" whenever your program grows to the point where you need it. No need to rewrite your program in some other, "better" (or more performant) language as its needs evolve. Perlang aims to have both of these use cases covered.

Similarly, we refuse to prematurely force the user to decide whether to use one of the following:

- a "scripting-based" ("rapid prototyping") language, with a great "edit-run" cycle, often at the cost of inferior performance and a weaker (dynamically typed) type system
- a "general-purpose object-oriented language" like Java, C# or Go (which tries to strike a balance in most tradeoffs between e.g. performance and programmer productivity)
- a "system programming language" like Rust, C or C++, with great performance and features necessary for proper "bare-metal" support

Often you know what kind of program you are writing when you start out, but really, why should you be forced to learn at least _three different languages_ (one scripting-based, one general-purpose and one system programming language) for this? Wouldn't it be much better if we could make a language that can excel in _all_ of these domains?

Perlang aims to be just that. Welcome to [join our journey](contribute/index.md).

(If you like to, you can read some more about how the project started on the [Perlang History](about/history/index.md) page.)

## License

[MIT](https://gitlab.perlang.org/perlang/perlang/-/blob/master/LICENSE)

## Thanks

A special thanks to our following corporate sponsors. Your contributions are highly appreciated. ❤️

- [JetBrains](https://www.jetbrains.com/) for donating Rider licenses to the project.
- [Fastly](https://www.fastly.com/) for providing bandwidth and a great CDN to host our content. The website you're currently reading is powered by Fastly's global network.

## Footnotes

<sup>1</sup>: Classes are not [fully supported yet](https://gitlab.perlang.org/perlang/perlang/-/issues/66).<br/>
<sup>2</sup> Type-parametric polymorphism (generics) has not even been started. :-)
