# The Perlang Programming Language

## About

Perlang is a general-purpose programming language which aims to incorporate the following concepts/paradigms:

* _static typing_
* strong _type inference_
* _immutability_ by default, mutability where required
* _safe_ by default, unsafe where required
* _object-orientation_
* _functional programming_

Perlang deliberately aims to be a low-barrier language, much like scripting languages like Ruby, JavaScript and Python. As soon as you have the `perlang` binary available, you can start writing your program. Much like these other languages, there is a minimum of "ceremony" required: you don't have to declare a top-level class (or even function!). You can focus on the essentials of "getting things done" and start hacking right away.

At the same time, these concepts (functions, classes<sup>1</sup>, type-parametric polymorphism<sup>2</sup> etc) are there, readily available in the "toolbox" whenever your program grows to the point where you need it. No need to rewrite your program in some other, "better" (or more performant) language as its needs evolve. Perlang aims to have both of these use cases covered.

Languages can also be grouped in different "kinds". Some are more "scripting-oriented", some are "general-purpose" languages, and some are "system-oriented", giving you good access to the hardware so that you can perform whatever optimizations you deem necessary. Perlang tries to strike a balance in beeing good in _all_ of these domains. You can run `perlang myprogram.per` after just typing a few code lines in a text editor (or LLM tool) and saving your program. At the same time, more advanced concepts like _inline C++_ is available, which gives you total flexibility. The language lets your program grow and tries to provide the mechanisms for this growth.

We invite you to [learn the language](learn/the-language/index.md) and [join our journey](contribute/index.md)

(If you like to, you can read some more about how the project started on the [Perlang History](about/history/index.md) page.)

## License

* [LGPLv2.1](https://gitlab.perlang.org/perlang/perlang/-/blob/master/LICENSE) (compiler and tooling)
* [MIT (Expat)](https://gitlab.perlang.org/perlang/perlang/-/blob/master/LICENSE-MIT) (standard library)

## Thanks

A special thanks to our following corporate sponsors. Your contributions are highly appreciated. ❤️

- [JetBrains](https://www.jetbrains.com/) for donating Rider licenses to the project.
- [Fastly](https://www.fastly.com/) for providing bandwidth and a great CDN to host our content. The website you're currently reading is powered by Fastly's global network.

## Footnotes

<sup>1</sup>: Classes are supported, but more advanced features like inheritance is not yet in place. For the full story, see this page: [https://gitlab.perlang.org/perlang/perlang/-/issues/66](https://gitlab.perlang.org/perlang/perlang/-/issues/66).<br/>
<sup>2</sup> Type-parametric polymorphism (generics) has not even been started. :-)
