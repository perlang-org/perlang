![.NET build & test](https://github.com/perlang-org/perlang/workflows/.NET%20Core%20build%20&%20test/badge.svg)
![perlang-install](https://github.com/perlang-org/perlang/workflows/perlang-install/badge.svg)

# perlang

The Perlang Programming Language

## Installation

### Quick-start guide

Each commit to the `master` branch triggers a build that gets published as a set of `.tar.gz` files at https://builds.perlang.org (CDN sponsored by [Fastly](https://www.fastly.com/)). Binaries are available for Linux, macOS and Windows.

The easiest way to install the latest build is by using the [perlang-install](scripts/perlang-install) script. It works on all supported platforms (Linux, macOS and Windows - the latter requires a POSIX shell like Git Bash to be available). Use it like this:

```shell
$ curl -sSL https://perlang.org/install | sh
```

If you have a previous build installed and want to overwrite it, use the following:

```shell
$ curl -sSL https://perlang.org/install | sh -s -- --force
```

(By all means, feel free to read [the script](scripts/perlang-install) before executing it if you so prefer.)

The installer will download the latest Perlang build and unpack it in a folder under `~/.perlang`. It will also print some instructions about how to add the Perlang toolchain to your `$PATH`.

### Further reading

At the moment, there isn't much documentation about what you can do with Perlang available, but we are continuously working on making more and more material available at the following location:

* https://perlang.org
* [docs/syntax-grammar.md](docs/syntax-grammar.md): Specification of the syntax grammar for the Perlang language.

## Building from source

### Installing prerequisites for building

```shell
$ sudo apt-get install \
      dotnet-sdk-6.0 \
      make \
      ruby
```

### Building the Perlang tooling

```shell
$ git clone https://github.com/perlang-org/perlang.git
$ cd perlang
$ make
```

You should now have a `perlang` executable. Run `make run` to run it. If you are brave, `make install` (currently Linux only) will put binaries in a path under `~/.perlang` which you can put in your `$PATH` so you can easily use it from anywhere. **Note**: this script uses the same folder (`~/.perlang/nightly/bin`) as the nightly build installer. Any previous version will be overwritten without prompting. This means that if you have previously installed a nightly build and added the folder to your `$PATH`, the version installed with `make install` will now be available in the `$PATH` instead of the previous nightly version.

## Documentation

- [docs/syntax-grammar.md](docs/syntax-grammar.md): Specification of the syntax grammar for the Perlang language.
- [docs](docs): The source code to the https://perlang.org web site. Built using [docfx](https://dotnet.github.io/docfx)

## Building the docs

Install the [DocFX prerequisites](https://dotnet.github.io/docfx/tutorial/docfx_getting_started.html#2-use-docfx-as-a-command-line-tool), including the Mono runtime. You should then be able to run the following:

```shell
$ npm install -g live-server
$ make docs
$ make docs-serve
```

When you make changes to the documentation, run `make docs` to regenerate them (and make your browser auto-reload the changes).

If you want to continuously rebuild documentation, run `make docs-autobuild` in a separate terminal window. This does not currently work flawlessly, so the `make docs` approach is preferable.

## License

[MIT](LICENSE)

[perlang-install](scripts/perlang-install) is originally based on [rustup-init](https://github.com/rust-lang/rustup/blob/master/rustup-init.sh), which is also licensed under the MIT license. Copyright (c) 2016 The Rust Project Developers.

[src/Perlang.Stdlib/Stdlib/Posix.cs](src/Perlang.Stdlib/Stdlib/Posix.cs) includes content from the NetBSD project, licensed under the 3-clause BSD license. Copyright (c) 1980, 1991, 1993 The Regents of the University of California.  All rights reserved.

## Disclaimer

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
