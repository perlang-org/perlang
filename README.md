![Tests](https://github.com/perlang-org/perlang/actions/workflows/test.yml/badge.svg)
![perlang-install](https://github.com/perlang-org/perlang/actions/workflows/perlang-install.yml/badge.svg)
![Website](https://github.com/perlang-org/perlang/actions/workflows/website.yml/badge.svg)

# perlang

Perlang is a general-purpose programming language aimed at being "concurrently
safe, fast and fun". Its current implementation is an interpreter implemented in
C#, with some compiler-like characteristics for performing static code analysis.

The aim is to make it a fully compiled language in the long run (either to MSIL
or machine code), while still remaining low-barrier and low-entry. You should
never have to _install_ a lot of packages/complex tooling to be able to start
writing Perlang code; it should always remain just a single command away.

## Installation

The Perlang tools are installed using
[perlang-install](scripts/perlang-install). It works on all supported platforms
(Linux, macOS and Windows), as long as you have a POSIX compatible shell
available, e.g. `bash`. Use it like this:

```shell
$ curl -sSL https://perlang.org/install | sh
```

The installer will refuse to overwrite an existing installation, if present. If
this happens, you can force the install like this:

```shell
$ curl -sSL https://perlang.org/install | sh -s -- --force
```

The installer will download the latest Perlang build and unpack it in a folder
under `~/.perlang`. It will also print some instructions about how to add the
Perlang toolchain to your `$PATH`.

## Hello World

```perlang
// hello_world.per
print "Hello World";
```

Running this will give you the following:

```
$ perlang hello_world.per
Hello World
```

## Printing the first 1000 digits of pi

The first digit printed is `3`, and the first 999 decimals is then printed immediately after.

```perlang
var digits = 1000;

var i = 1;
var x = 3 * (10 ** (digits + 20));
var pi = x;

while (x > 0) {
    x = x * i / ((i + 1) * 4);
    pi += (x / (i + 2));
    i += 2;
}

print(pi / (10 ** 20));
```

### Further reading

* https://perlang.org/learn/quickstart/index.html - this page contains more
  examples and explains the basics of the Perlang REPL, which is a great tool for
  playing around with the language.

## More details

### Building from source

_Note_: this is not required for writing Perlang programs. These steps are
required if you want to make changes to the Perlang code itself.

#### Installing prerequisites for building

```shell
$ sudo apt-get install \
      dotnet-sdk-7.0 \
      make \
      ruby
```

#### Building the Perlang tooling

```shell
$ git clone https://gitlab.perlang.org/perlang/perlang.git
$ cd perlang
$ make
```

You should now have a `perlang` executable. Run `make run` to run it. If you are
brave, `make install` (currently Linux only) will put binaries in a path under
`~/.perlang` which you can put in your `$PATH` so you can easily use it from
anywhere.

**Note**: this script uses the same folder (`~/.perlang/nightly/bin`) as the
nightly build installer. Any previous version will be overwritten without
prompting. This means that if you have previously installed a nightly build and
added the folder to your `$PATH`, the version installed with `make install` will
now be available in the `$PATH` instead of the previous nightly version.

### Documentation

- [docs/syntax-grammar.md](docs/syntax-grammar.md): Specification of the syntax
  grammar for the Perlang language.
- [docs](docs): The source code to the https://perlang.org web site. Built using
  [docfx](https://dotnet.github.io/docfx).

#### Building the docs

Install the [DocFX
prerequisites](https://dotnet.github.io/docfx/tutorial/docfx_getting_started.html#2-use-docfx-as-a-command-line-tool),
including the Mono runtime. You should then be able to run the following:

```shell
$ npm install -g live-server
$ make docs
$ make docs-serve
```

When you make changes to the documentation, run `make docs` to regenerate them.
The `live-server` process which is started by `make docs-serve` will
conveniently make your browser auto-reload the changes.

If you want to continuously rebuild documentation, run `make docs-autobuild` in
a separate terminal window. This does not currently work flawlessly, so the
`make docs` approach is preferable.

### Published builds

Each commit to the `master` branch triggers a build that gets published as a set
of `.tar.gz` files at https://builds.perlang.org (CDN sponsored by
[Fastly](https://www.fastly.com/)). Binaries are available for Linux, macOS and
Windows. These builds can be installed using the
[perlang-install](scripts/perlang-install) script mentioned earlier in this
guide.

## License

[MIT (Expat)](LICENSE)

[perlang-install](scripts/perlang-install) is originally based on
[rustup-init](https://github.com/rust-lang/rustup/blob/master/rustup-init.sh),
which is also licensed under the MIT license. Copyright (c) 2016 The Rust
Project Developers.

[src/Perlang.Stdlib/Stdlib/Posix.cs](src/Perlang.Stdlib/Stdlib/Posix.cs)
includes content from the NetBSD project, licensed under the 3-clause BSD
license. Copyright (c) 1980, 1991, 1993 The Regents of the University of
California.  All rights reserved.

[src/stdlib/src/bigint.hpp](src/stdlib/src/bigint.hpp) includes content from
Syed Faheel Ahmad's `BigInt` library, available at
https://github.com/faheel/BigInt, licensed under the terms of the MIT license.
Copyright (c) 2017 - 2018 Syed Faheel Ahmad.

[src/stdlib/src/libtommath](src/stdlib/src/libtommath/) includes content from
libtommath (https://github.com/libtom/libtommath), licensed under the Unlicense
(http://unlicense.org).

[src/stdlib/src/double-conversion](src/stdlib/src/double-conversion) includes
content from the Google `double-conversion` library, available at
https://github.com/google/double-conversion, licensed under the BSD 3-Clause
"New" or "Revised" License. Copyright 2006-2011, the V8 project authors.

[src/stdlib/src/fmt](src/stdlib/src/fmt) includes content from the `{fmt}`
library, available at https://github.com/fmtlib/fmt, licensed under the MIT
license. Copyright (c) 2012 - present, Victor Zverovich and `{fmt}`
contributors.

## Disclaimer

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
